using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.PrintStudio.PublishingEngine;
using JCore.SitecoreAPS.PrintStudio.Helpers;
using JCore.SitecoreAPS.PrintStudio.Rendering.Common;
using Sitecore.PrintStudio.PublishingEngine.Helpers;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.PitchBook
{
    /// <summary>
    /// Defines the index renderer class.
    /// </summary>
    public class XmlIndexRenderer : XmlParagraphStyleRenderer
    {
        /// <summary>
        /// Gets or sets the name of the list.
        /// </summary>
        /// <value>
        /// The name of the list.
        /// </value>
        public string ListName { get; set; }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            if (printContext.Settings.Parameters.ContainsKey(this.ListName))
            {
                var tempElement = new XElement("temp");
                base.RenderContent(printContext, tempElement);
                var textParagraph = tempElement.Element("ParagraphStyle");
                if (textParagraph != null)
                {
                    var indexList = (Dictionary<string, List<string>>)printContext.Settings.Parameters[this.ListName];

                    var destinations = from indexItem in indexList
                                       orderby indexItem.Key ascending
                                       select indexItem;

                    foreach (KeyValuePair<string, List<string>> destination in destinations)
                    {
                        IDictionary<string, string> variables = new Dictionary<string, string>();
                        variables.Add("$Destination", destination.Key);

                        var varList = new List<string>();
                        destination.Value.ForEach(s => this.FormatTocIndex(s, ref varList));
                        string formattedIndexes = string.Join("<![CDATA[, ]]>", varList.ToArray());

                        variables.Add("$Indexes", formattedIndexes);

                        var textValue = textParagraph.ToString(SaveOptions.DisableFormatting);
                        RenderingHelper.ReplaceVariables(ref textValue, variables);
                        output.AddFragment(textValue);
                    }
                }
            }
        }

        /// <summary>
        /// Formats the index of the toc.
        /// </summary>
        /// <param name="tocIndex">Index of the toc.</param>
        /// <param name="varList">The var list.</param>
        private void FormatTocIndex(string tocIndex, ref List<string> varList)
        {
            if (!string.IsNullOrEmpty(tocIndex))
            {
                string tocVar = @"<InsertVar Name=" + "\"TOC_" + tocIndex + "\" Value=" + "\"" + "\" />";
                varList.Add(tocVar);
            }
        }
    }
}
