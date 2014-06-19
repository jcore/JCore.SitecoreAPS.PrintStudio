using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlTextFrameInSequence : XmlTextFrameRenderer
    {
        public string CharacterLimit { get; set; }
        public string SectionNumber { get; set; }
        public string SequenceGroup { get; set; }
        public string CharactersPerLine { get; set; }

        private int CharacterCountPerLine {
            get
            {
                int c = 100;
                if (int.TryParse(CharactersPerLine, out c))
                {
                    return c;
                }
                return 100;
            }
        }

        /// <summary>
        /// Processes the paragraph.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void ProcessParagraph(XElement el, int charLimit, XElement section1, XElement section2, ref StringBuilder str)
        {
            if (!el.HasElements)
            {
                ProcessTextElement(el, charLimit, section1, section2, ref str);
            }
            else
            {
                ProcessChildren(el, charLimit, section1, section2, ref str);
            }
        }

        /// <summary>
        /// Processes the children.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void ProcessChildren(XElement el, int charLimit, XElement section1, XElement section2, ref StringBuilder str)
        {
            var elCopy1 = CopyElement(el);
            var elCopy2 = CopyElement(el);
            foreach (var node in el.Nodes())
            {                
                if (node.NodeType == System.Xml.XmlNodeType.CDATA)
                {
                    ProcessXCData((XCData)node, charLimit, elCopy1, elCopy2, ref str);
                }
                else if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    var elem = (XElement)node;
                    switch (elem.Name.ToString())
                    {
                        case "Table":
                            ProcessTable(elem, charLimit, elCopy1, elCopy2, ref str);
                            break;
                        case "Inline":
                            ProcessInline(elem, charLimit, elCopy1, elCopy2, ref str);
                            break;
                        default:
                            if (elem.HasElements)
                            {
                                ProcessChildren(elem, charLimit, elCopy1, elCopy2, ref str);
                            }
                            else
                            {
                                ProcessTextElement(elem, charLimit, elCopy1, elCopy2, ref str);
                            }
                            break;
                    }
                }
                else if (node.NodeType == System.Xml.XmlNodeType.Text)
                {
                    ProcessXText((XText)node, charLimit, elCopy1, elCopy2, ref str);
                }
            }
            if (!IsEmpty(elCopy1))
            {
                section1.Add(elCopy1);
            }
            if (!IsEmpty(elCopy2))
            {
                section2.Add(elCopy2);
            }
        }

        /// <summary>
        /// Processes the table.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void ProcessTable(XElement el, int charLimit, XElement section1, XElement section2, ref StringBuilder str)
        {
            var groupedElements = el.Elements().GroupBy(r => r.Attribute((XName)"CustomAttribute") != null ? r.Attribute((XName)"CustomAttribute").Value : string.Empty);
            var tableNode1 = CopyElement(el);
            var tableNode2 = CopyElement(el);
            var timeline = el.Attribute((XName)"TableStyle") != null && el.Attribute((XName)"TableStyle").Value == "Timeline" ? true : false;
            var containsOnlyHeader = true;
            foreach (var group in groupedElements)
            {
                if (group.Key == "Header")
                {
                    foreach (var row in group)
                    {
                        tableNode1.Add(row);
                        tableNode2.Add(row);
                        str.Append(row.Value);
                    }
                }
                else
                {
                    containsOnlyHeader = false;
                    var rowValue = string.Join(" ", group.Select(r => r.Value));
                    var numberOfLines = (int)Math.Ceiling((double)rowValue.Length / (double)CharacterCountPerLine);
                    var charValue = new string('*', numberOfLines * CharacterCountPerLine);
                    if (timeline)
                    {
                        charValue = new string('*', (int)(numberOfLines * CharacterCountPerLine * 1.3));
                        if (!tableNode2.HasElements)
                        {
                            charLimit = charLimit + charValue.Length;
                        }
                    }
                    else
                    {
                        charValue = new string('*', numberOfLines * CharacterCountPerLine);
                        if (!tableNode2.HasElements)
                        {
                            charLimit = charLimit + charValue.Length;
                        }
                  
                    }

                    if (IsFirstSection(charLimit, str.Length + rowValue.Length) && (IsOnlyHeader(tableNode2) || !tableNode2.HasElements))
                    {
                        foreach (var row in group)
                        {
                            tableNode1.Add(row);
                            str.Append(row.Value);                            
                        }
                    }
                    else
                    {
                        foreach (var row in group)
                        {
                            tableNode2.Add(row);
                            str.Append(row.Value);
                        }
                    }
                    str.Append(rowValue);
                }
            }
            if (!containsOnlyHeader)
            {
                if (tableNode1.HasElements && ((!IsOnlyHeader(tableNode1) && timeline) || !timeline))
                {
                    section1.Add(tableNode1);
                }
                if (tableNode2.HasElements && ((!IsOnlyHeader(tableNode2) && timeline) || !timeline))
                {
                    section2.Add(tableNode2);
                }
            }
        }

        /// <summary>
        /// Determines whether [is only header] [the specified table node1].
        /// </summary>
        /// <param name="tableNode1">The table node1.</param>
        /// <returns></returns>
        private bool IsOnlyHeader(XElement tableNode1)
        {
            var groupedTableNode = tableNode1.Elements().GroupBy(r => r.Attribute((XName)"CustomAttribute") != null ? r.Attribute((XName)"CustomAttribute").Value : string.Empty);
            var res = false;
            foreach (var group in groupedTableNode)
            {
                if (group.Key == "Header")
                {
                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            return res;
        }

        /// <summary>
        /// Determines whether the specified el is empty.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <returns></returns>
        private bool IsEmpty(XElement el)
        {
            if (!el.HasElements && string.IsNullOrEmpty(el.Value) && el.Name != "Image")
            {
                return true;
            }
            if (el.Name == "Inline" )
            {
                return false;
            }
            foreach(var elem in el.Elements())
            {
                return IsEmpty(elem);
            }
            return false;
        }

        /// <summary>
        /// Processes the inline.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void ProcessInline(XElement el, int charLimit, XElement section1, XElement section2, ref StringBuilder str)
        {
            var inlineNode1 = CopyElement(el);
            var inlineNode2 = CopyElement(el);
            var inlineStr = new StringBuilder();
            foreach (var element in el.Elements())
            {
                if (element.Name == "Image")
                {
                    if (!string.IsNullOrWhiteSpace(element.Attribute((XName)"Height").Value) && !string.IsNullOrWhiteSpace(el.Attribute((XName)"Width").Value))
                    {
                        int h = 0;
                        int w = 0;
                        if (!int.TryParse(element.Attribute((XName)"Height").Value, out h))
                        {
                            h = 10;
                        }
                        if (!int.TryParse(el.Attribute((XName)"Width").Value, out w))
                        {
                            w = 10;
                        }
                        inlineStr.Append(new string('*', (h * w) / 8));
                    }
                }
                else
                {
                    if (element.HasElements)
                    {
                        ProcessChildren(element, charLimit, inlineNode1, inlineNode2, ref inlineStr);
                    }
                    else
                    {
                        ProcessTextElement(element, charLimit, inlineNode1, inlineNode2, ref inlineStr);
                    }
                    //inlineStr.Append(element.Value);
                }
                //if (IsFirstSection(charLimit, str.Length + inlineStr.Length))
                //{
                //    inlineNode1.Add(element);
                //}
                //else
                //{
                //    inlineNode2.Add(element);
                //}
            }
            if (IsFirstSection(charLimit, inlineStr.Length))
            {
                section1.Add(inlineNode1);
            }
            else
            {
                section2.Add(inlineNode2);
            }
        }

        /// <summary>
        /// Processes the element.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void ProcessTextElement(XElement el, int charLimit, XElement section1, XElement section2, ref StringBuilder str)
        {
            var styleAttribute = el.Attribute((XName)"Style");
            var elemValue = el.Value;
            if (styleAttribute != null && styleAttribute.Value.ToLowerInvariant().Contains("bullet"))
            {
                var numberOfLines = (int)Math.Ceiling((double)el.Value.Length / (double)CharacterCountPerLine);
                elemValue = new string('*', numberOfLines * CharacterCountPerLine);
                if (!section2.HasElements)
                {
                    charLimit = charLimit + elemValue.Length;
                }
            }
            if (IsFirstSection(charLimit, str.Length + el.Value.Length) && !section2.HasElements)
            {
                section1.Add(el);
            }
            else
            {
                SplitIntoSections(el, charLimit, section1, section2, str);
            }
            str.Append(elemValue);
        }

        /// <summary>
        /// Splits the into sections.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void SplitIntoSections(XElement el, int charLimit, XElement section1, XElement section2, StringBuilder str)
        {
            var charCountToFitSection1 = GetCharCountForSection(el.Value, charLimit, 0, str);
            var charCountToFitSection2 = GetCharCountForSection(el.Value, charLimit, 1, str);
            if (charCountToFitSection1 == 0)
            {
                section2.Add(el);
            }
            else if (charCountToFitSection2 == 0)
            {
                section1.Add(el);
            }
            else
            {
                var firstSectionElement = CopyElement(el);
                var secondSectionElement = CopyElement(el);

                if (el.HasElements)
                {
                    foreach (var node in el.Nodes())
                    {
                        if (node.NodeType == System.Xml.XmlNodeType.Element)
                        {
                            var elem = (XElement)node;
                            SplitIntoSections(elem, charLimit, firstSectionElement, secondSectionElement, str);
                        }
                        else if (node.NodeType == System.Xml.XmlNodeType.CDATA)
                        {
                            SplitIntoSections(charLimit, str, firstSectionElement, secondSectionElement, node);
                        }
                    }
                }
                else
                {                    
                    firstSectionElement.Value = el.Value.Substring(0, charCountToFitSection1);
                    secondSectionElement.Value = el.Value.Substring(charCountToFitSection1, charCountToFitSection2).Trim();
                }
                if (!string.IsNullOrWhiteSpace(firstSectionElement.Value))
                {
                    section1.Add(firstSectionElement);
                }
                if (!string.IsNullOrWhiteSpace(secondSectionElement.Value))
                {
                    section2.Add(secondSectionElement);
                }
            }
        }

        /// <summary>
        /// Splits the into sections.
        /// </summary>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="str">The string.</param>
        /// <param name="firstSectionElement">The first section element.</param>
        /// <param name="secondSectionElement">The second section element.</param>
        /// <param name="node">The node.</param>
        private void SplitIntoSections(int charLimit, StringBuilder str, XElement firstSectionElement, XElement secondSectionElement, XNode node)
        {
            var val = node is XCData ? ((XCData)node).Value : node is XText ? ((XText)node).Value : string.Empty;
            var charCountToFitNode1 = GetCharCountForSection(val, charLimit, 0, str);
            var charCountToFitNode2 = GetCharCountForSection(val, charLimit, 1, str);

            if (!string.IsNullOrEmpty(val))
            {
                var cData1 = new XCData(val.Substring(0, charCountToFitNode1));
                firstSectionElement.Add(cData1);

                var cData2 = new XCData(val.Substring(charCountToFitNode1, charCountToFitNode2));
                secondSectionElement.Add(cData2);
            }
        }

        /// <summary>
        /// Gets the character count for section.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        private int GetCharCountForSection(string val, int charLimit, int sectionNumber, StringBuilder str)
        {
            if (charLimit == 0)
            {
                if (sectionNumber == 0)
                {
                    return val.Length;
                }
                else
                {
                    return 0;
                }
            }
            if (str.Length < charLimit)
            {
                var totalCount = str.Length + val.Length;
                var secondPart = totalCount - charLimit;
                var firstPart = val.Length - secondPart;
                if (firstPart < 0 || val.Length < firstPart)
                {
                    firstPart = 0;
                }
                var endIndex = val.Length - firstPart;
                if (endIndex <= 0)
                {
                    endIndex = 1;
                }
                
                var len = val.IndexOfAny(new char[] { '.' }, firstPart, endIndex);
                if (len > 0)
                {
                    var realFirstPart = val.Substring(0, len+1);
                    var realFirstPartLength = realFirstPart.Length;
                    var realSecondPartLength = totalCount - str.Length - realFirstPartLength;
                    if (sectionNumber == 0)
                    {
                        return realFirstPartLength;
                    }
                    else
                    {
                        return realSecondPartLength;
                    }
                }
                else
                {
                    if (sectionNumber == 0)
                    {
                        return val.Length;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else
            {
                if (sectionNumber == 0)
                {
                    return 0;
                }
                else
                {
                    return val.Length;
                }
            }            
        }

        /// <summary>
        /// Processes the xc data.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void ProcessXCData(XCData node, int charLimit, XElement section1, XElement section2, ref StringBuilder str)
        {
            if (IsFirstSection(charLimit,str.Length + node.Value.Length))
            {
                section1.Add(node);
            }
            else 
            {
                SplitIntoSections(charLimit, str, section1, section2, node);
            }
            str.Append(node.Value);
        }

        /// <summary>
        /// Processes the x text.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="section1">The section1.</param>
        /// <param name="section2">The section2.</param>
        /// <param name="str">The string.</param>
        private void ProcessXText(XText node, int charLimit, XElement section1, XElement section2, ref StringBuilder str)
        {
            if (IsFirstSection(charLimit, str.Length + node.Value.Length))
            {
                section1.Add(node);
            }
            else
            {
                SplitIntoSections(charLimit, str, section1, section2, node);
            }
            str.Append(node.Value);
        }

        /// <summary>
        /// Determines whether [is first section] [the specified character limit].
        /// </summary>
        /// <param name="charLimit">The character limit.</param>
        /// <param name="sectionNumber">The section number.</param>
        /// <param name="strLength">Length of the string.</param>
        /// <returns></returns>
        private bool IsFirstSection(int charLimit, int strLength)
        {
            if (charLimit > 0 && strLength <= charLimit)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            XElement baseXml = new XElement("base");
            base.RenderContent(printContext, baseXml);

            XElement textFrame = baseXml.Element("TextFrame");
            if (textFrame != null)
            {
                var charLimit = 0;
                if (!int.TryParse(CharacterLimit, out charLimit))
                {
                    charLimit = 0;
                }
                var sectionNumber = 0;
                if (!int.TryParse(SectionNumber, out sectionNumber))
                {
                    sectionNumber = 0;
                }
                var str = new StringBuilder();
                var textFrame1 = CopyElement(textFrame);
                var textFrame2 = CopyElement(textFrame);

                foreach (var el in textFrame.Elements())
                {
                    ProcessParagraph(el, charLimit, textFrame1, textFrame2, ref str);
                }
                printContext.Settings.Parameters["0" + this.SequenceGroup] = textFrame1;
                printContext.Settings.Parameters["1" + this.SequenceGroup] = textFrame2;

                if (sectionNumber == 0)
                {
                    output.Add(textFrame1);
                }
                else
                {
                    output.Add(textFrame2.Elements());
                }
            }
        }

        /// <summary>
        /// Gets the content of the text.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <returns></returns>
        private string GetTextContent(XElement el)
        {
            var result = new StringBuilder();
            foreach (var n in el.Nodes())
            {
                if (n.NodeType == System.Xml.XmlNodeType.Element)
                {
                    switch (((XElement)n).Name.ToString())
                    {
                        case "Image":
                            var element = (XElement)n;
                            var inlineElement = n.Parent.Name == "Inline" ? n.Parent : element;
                            if (!string.IsNullOrWhiteSpace(element.Attribute((XName)"Height").Value) && !string.IsNullOrWhiteSpace(inlineElement.Attribute((XName)"Width").Value))
                            {
                                var height = int.Parse(element.Attribute((XName)"Height").Value);
                                var width = int.Parse(inlineElement.Attribute((XName)"Width").Value);
                                result.Append(new string('*',(height*width)/8));
                            }
                            break;
                        default:
                            result.Append(GetTextContent((XElement)n));
                            break;
                    }
                }
                else
                {
                    result.Append(el.Value);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Copies the element.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <returns></returns>
        private XElement CopyElement(XElement el)
        {
            var newElem = new XElement((XName)el.Name);
            foreach (var attr in el.Attributes())
            {
                newElem.SetAttributeValue((XName)attr.Name, attr.Value);
            }
            return newElem;
        }
    }
}
