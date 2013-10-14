using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// Renders paragraph style element markup.
    /// </summary>
    public class XmlContentRenderer : InDesignItemRendererBase
    {
        /// <summary>
        /// Static Text Field Name
        /// </summary>
        private const string StaticTextFieldName = "Content";

        /// <summary>
        /// Content item
        /// </summary>
        private Item contentItem;

        /// <summary>
        /// Gets or sets the content field.
        /// </summary>
        /// <value>
        /// The content field.
        /// </value>
        private string contentFieldName;

        /// <summary>
        /// Gets or sets the name of the content field.
        /// </summary>
        /// <value>
        /// The name of the content field.
        /// </value>
        public string ContentFieldName
        {
            get
            {
                if (string.IsNullOrEmpty(this.contentFieldName))
                {
                    this.contentFieldName = this.GetFieldName();
                }

                return this.contentFieldName;
            }

            set
            {
                this.contentFieldName = value;
            }
        }

        /// <summary>
        /// Gets or sets the content item.
        /// </summary>
        /// <value>
        /// The internal data.
        /// </value>
        protected virtual Item ContentItem
        {
            get
            {
                return this.contentItem ?? (this.contentItem = this.RenderingItem);
            }

            set
            {
                this.contentItem = value;
            }
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <returns>The field name</returns>
        protected virtual string GetFieldName()
        {
            return StaticTextFieldName;
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, [NotNull] XElement output)
        {
            string content = this.ParseContent(printContext);
            XElement tempContentNode;

            try
            {
                //to prevent exception related to "&" characters in values, encode it
                tempContentNode = XElement.Parse(string.Format("<TempContentRoot>{0}</TempContentRoot>", content.Replace("&","&amp;")));
            }
            catch (Exception exc)
            {
                Log.Error("Render content parse content value", exc, this);
                return;
            }

            if (!tempContentNode.HasElements)
            {
                var plainTextNodes = tempContentNode.Nodes().Where(t => t.NodeType == XmlNodeType.Text);
                foreach (var plainTextNode in plainTextNodes)
                {
                    //no we need to decode "&" character to provide correct value for InDesign
                    plainTextNode.ReplaceWith(new XCData(plainTextNode.ToString().Replace("&amp;","&")));
                }

                output.Add(tempContentNode.Nodes());
                this.RenderChildren(printContext, output);
            }
            else
            {
                XElement currentNode = output;
                if (tempContentNode.Elements().Count() == 1)
                {
                    currentNode = tempContentNode.Elements().First();
                }

                // first render children, then add the nodes, prevents from losing the children xml
                this.RenderChildren(printContext, currentNode);
                output.Add(tempContentNode.Nodes());
            }
        }

        /// <summary>
        /// Parses the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns>
        /// The content.
        /// </returns>
        protected virtual string ParseContent(PrintContext printContext)
        {
            Assert.IsNotNull(this.ContentItem, "Content item is null");
            Assert.IsNotNullOrEmpty(this.ContentFieldName, "Missing content field");

            Field contentField = this.ContentItem.Fields[this.ContentFieldName];
            if (contentField == null)
            {
                return string.Empty;
            }

            CustomField field = FieldTypeManager.GetField(contentField);
            if (field == null)
            {
                return contentField.Value;
            }
            if (field is DateField)
            {
                return ((DateField)field).DateTime.ToString("MMMM dd, yyyy");
            } 
            else if (field is HtmlField)
            {
                Item defaultFormatting = printContext.Settings.GetDefaultFormattingItem(printContext.Database);
                if (defaultFormatting != null)
                {
                    try
                    {
                        XmlDataDocument tempDoc = new XmlDataDocument();
                        return PatternBuilderExtension.TransformRichContent(field.Value, printContext.Database, tempDoc, defaultFormatting, "XHTML", "XML");
                    }
                    catch (Exception exc)
                    {
                        Log.Error(exc.Message, this);
                    }
                }

                return PatternBuilderExtension.RichTextToTextParser(field.Value, true, false);
            }
            return field.Value;
        }
    }
}
