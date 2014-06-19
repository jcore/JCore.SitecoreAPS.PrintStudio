using System.Text.RegularExpressions;
using System.Xml.Linq;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// Renders paragraph style element markup.
    /// </summary>
    public class XmlParagraphStyleRenderer : XmlElementRenderer
    {
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            // Create a temp node to serve as a container
            var temp = new XElement("temp");

            // Call the base to generate the element xml
            base.RenderContent(printContext, temp);

            // Select the generated paragraph style element
            var paragraph = temp.Element(this.Tag);
            if (paragraph != null)
            {
                // Get the data item assigned to the snippet
                var dataItem = this.GetDataItem(printContext);
                if (dataItem != null && !string.IsNullOrEmpty(this.RenderingItem["Item Field"]))
                {
                    // Fetch the value for the field point in the ParagraphStyle element and add it as a CDATA
                    var data = dataItem[this.RenderingItem["Item Field"]];
                    if (!string.IsNullOrEmpty(data) || this.RenderingItem["Publish when empty"] == "1")
                    {
                        paragraph.AddFirst(new XCData(data));
                    }
                }
                if ((!string.IsNullOrEmpty(paragraph.Value) && ContainsAlphanumericCharacters(paragraph.Value)) || this.RenderingItem["Publish when empty"] == "1")
                {
                    paragraph = RemovedNestedParagraphStyles(paragraph);
                    output.Add(paragraph);
                }                            
            }
        }

        /// <summary>
        /// Removes the nested paragraph styles.
        /// </summary>
        /// <param name="paragraph">The paragraph.</param>
        /// <returns></returns>
        private XElement RemovedNestedParagraphStyles(XElement paragraph)
        {
            if (paragraph.HasElements)
            {
                foreach (var par in paragraph.Elements())
                {
                    if (par.Name == "ParagraphStyle" && par.Parent.Name == par.Name)
                    {
                        return RemovedNestedParagraphStyles(par);
                    }
                }
            }
            return paragraph;
        }

        /// <summary>
        /// Determines whether [contains orphan characters] [the specified value].
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        private bool ContainsAlphanumericCharacters(string val)
        {
            var reg = new Regex("[a-zA-Z0-9]");
            if (reg.IsMatch(val))
            {
                return true;
            }
            return false;
        }
    }
}
