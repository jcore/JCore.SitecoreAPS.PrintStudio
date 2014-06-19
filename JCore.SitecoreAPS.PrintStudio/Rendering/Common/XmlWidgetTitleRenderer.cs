using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;
using Sitecore.PrintStudio.PublishingEngine.Scripting;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlWidgetTitleRenderer : XmlDroplinkRenderer
    {        
        public string FallbackFieldName { get; set; }

        /// <summary>
        /// Parses the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns>
        /// The content.
        /// </returns>
        protected override string ParseContent(PrintContext printContext)
        {
            Assert.IsNotNullOrEmpty(this.ContentFieldName, "Missing content field");

            Field contentField = this.ContentItem.Fields[this.ContentFieldName];
            if (contentField == null)
            {
                return string.Empty;
            }

            CustomField field = FieldTypeManager.GetField(contentField);
            if (field == null || !string.IsNullOrWhiteSpace(field.Value))
            {
                return AddWrapping(contentField.Value);
            }

            contentField = this.ContentItem.Fields[HttpUtility.UrlDecode(this.FallbackFieldName)];
            if (contentField == null)
            {
                return string.Empty;
            }

            field = FieldTypeManager.GetField(contentField);
            if (field == null)
            {
                return AddWrapping(contentField.Value);
            } 
            if(!string.IsNullOrWhiteSpace(this.ContentItem[HttpUtility.UrlDecode(this.FallbackFieldName)]))
            
            if (field is LookupField || field is ReferenceField)
            {
                Item innerItem = null;
                if (field is LookupField)
                {
                    innerItem = ((LookupField)field).TargetItem;                    
                }
                else if (field is ReferenceField)
                {
                    innerItem = ((ReferenceField)field).TargetItem;
                }

                if (innerItem != null)
                {
                    if (!string.IsNullOrEmpty(this.ReferencedItemField) && innerItem.Fields[HttpUtility.UrlDecode(this.ReferencedItemField)] != null)
                    {
                        this.ContentItem = innerItem;
                        this.ContentFieldName = HttpUtility.UrlDecode(this.ReferencedItemField);
                        return AddWrapping(base.ParseContent(printContext));
                    }
                    return AddWrapping(innerItem.DisplayName);
                }
            }
            else if (field is TextField)
            {
                return AddWrapping(field.Value);
            }
            
            return AddWrapping(base.ParseContent(printContext));
        }        
    }
}
