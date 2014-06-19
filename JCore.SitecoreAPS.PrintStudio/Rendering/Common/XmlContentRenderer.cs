using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Helpers;
using Sitecore.PrintStudio.PublishingEngine.Rendering;
using Sitecore.PrintStudio.PublishingEngine.Text;
using Weil.Core.ErrorLogging;
using Weil.SC.Util.Extensions;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// Renders paragraph style element markup.
    /// </summary>
    public class XmlContentRenderer : InDesignItemRendererBase
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
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

        public string StripHtml { get; set; }
        public string CharacterLimit { get; set; }

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
            if (content == null)
            {
                return;
            }

            try
            {
                //to prevent exception related to "&" characters in values, encode it
                tempContentNode = XElement.Parse(string.Format("<TempContentRoot>{0}</TempContentRoot>", content.Contains("CDATA") ? content : content.Replace("&","&amp;")));
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
                    //now we need to decode "&" character to provide correct value for InDesign. APS char replacement is not working with this. doing replace :(
                    plainTextNode.ReplaceWith(new XCData(content.Contains("CDATA") ? plainTextNode.ToString() : plainTextNode.ToString().Replace("&amp;", "&").Replace("&amp;", "&")));
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
                return contentField.Value.Trim();
            }
            if (field is DateField)
            {
                if (!string.IsNullOrEmpty(field.Value))
                {
                    return AddWrapping(((DateField)field).DateTime.ToString("MMMM d, yyyy"));
                }
                else
                {
                    return string.Empty;
                }
            } 
            else if (field is HtmlField)
            {
                ParseContext context = new ParseContext(printContext.Database, printContext.Settings)
                {
                    DefaultParagraphStyle = this.Style,
                    ParseDefinitions = GetParseDefinitionCollection(this.RenderingItem)
                };
                var fieldValue = field.Value;
                int charLimit = 0;
                if (!string.IsNullOrWhiteSpace(this.CharacterLimit))
                {
                    if (int.TryParse(this.CharacterLimit, out charLimit))
                    {
                        charLimit = 0;
                    }
                }
                if (this.StripHtml == "True" || this.StripHtml == "1")
                {
                    return fieldValue.StripHtmlTags();
                }
                try
                {
                    return AddWrapping(RichTextParser.ConvertToXml(fieldValue.RemoveImages(), context, printContext.Language));
                }
                catch (Exception ex)
                {
                    LogManager<ILogProvider>.Error(ex.Message, ex, this);
                }
            }
            else if (field is LinkField)
            {
                var options = LinkManager.GetDefaultUrlOptions();
                options.AlwaysIncludeServerUrl = true;
                options.SiteResolving = true;
                return AddWrapping(((LinkField)field).LinkUrl(options));
            }
            return AddWrapping(field.Value.Trim());
        }

        /// <summary>
        /// Gets the parse definition collection.
        /// </summary>
        /// <param name="printItem">The print item.</param>
        /// <returns></returns>
        public  ParseDefinitionCollection GetParseDefinitionCollection(Item printItem)
        {
            Item setItem = (Item)null;
            Database database = printItem.Database;
            if (!string.IsNullOrEmpty(printItem["TransformationSet"]) && ID.IsID(printItem["TransformationSet"]))
                setItem = database.GetItem(printItem["TransformationSet"]);
            List<Item> list = Enumerable.ToList<Item>((IEnumerable<Item>)printItem.Axes.GetAncestors());
            
            Item obj1 = Enumerable.FirstOrDefault<Item>((IEnumerable<Item>)list, (Func<Item, bool>)(t => t.TemplateName == "P_Document"));
            if (obj1 != null)
            {
                ReferenceField referenceField = (ReferenceField)obj1.Fields["Reference"];
                if (referenceField != null && referenceField.TargetItem != null && referenceField.TargetItem.TemplateName == "P_MasterDocument")
                    list.Insert(list.IndexOf(obj1), referenceField.TargetItem);
            }
            list.Add(printItem);
            Item obj2 = Enumerable.LastOrDefault<Item>((IEnumerable<Item>)list, (Func<Item, bool>)(t =>
            {
                if (!string.IsNullOrEmpty(t["TransformationSet"]))
                    return ID.IsID(t["TransformationSet"]);
                else
                    return false;
            }));
            if (obj2 != null)
                setItem = database.GetItem(obj2["TransformationSet"]);
            if (setItem != null)
                return new ParseDefinitionCollection(setItem);
            else
                return (ParseDefinitionCollection)null;
        }

        /// <summary>
        /// Gets the style.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public string Style
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.RenderingItem["Style"]))
                {
                    if (this.RenderingItem["Style"].Contains("{"))
                    {
                        ReferenceField fld = this.RenderingItem.Fields["Style"];
                        if (fld != null && fld.TargetItem != null)
                        {
                            return fld.TargetItem.Name;
                        }
                    }
                    else
                    {
                        return this.RenderingItem["Style"];
                    }
                }
                return "NormalParagraphStyle";
            }
        }

        /// <summary>
        /// Adds the wrapping.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        protected string AddWrapping(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                return string.Concat(HttpUtility.UrlDecode(this.Prefix), val, HttpUtility.UrlDecode(this.Suffix));
            }
            return val;
        }
    }
}
