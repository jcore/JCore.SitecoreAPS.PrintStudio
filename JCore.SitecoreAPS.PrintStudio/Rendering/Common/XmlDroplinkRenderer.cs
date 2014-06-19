using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    public class XmlDroplinkRenderer : XmlDynamicContentRenderer
    {
        /// <summary>
        /// Gets or sets the referenced item field.
        /// </summary>
        /// <value>
        /// The referenced item field.
        /// </value>
        public string ReferencedItemField { get; set; }

        /// <summary>
        /// Preliminary render action invoked before RenderContent <see cref="InDesignItemRendererBase.RenderContent"/>.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            string dataSource = string.Empty;

            if (!string.IsNullOrEmpty(this.RenderingItem["Data Key"]) && printContext.Settings.Parameters.ContainsKey(this.RenderingItem["Data Key"]))
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.DataKey))
                    {
                        string dataItemId = printContext.Settings.Parameters[this.DataKey].ToString();
                        if (!string.IsNullOrEmpty(dataItemId))
                        {
                            dataSource = dataItemId;
                        }
                    }                    
                }
                catch (Exception exc)
                {
                    Log.Error(exc.Message, this);
                }
            }

            var dataItem = this.GetDataItem(printContext);
            if (dataItem != null)
            {
                dataSource = dataItem.ID.ToString();
            }

            if (!string.IsNullOrEmpty(this.RenderingItem["Item Reference"]) && dataSource == null)
            {
                dataSource = this.RenderingItem["Item Reference"];
            }

            if (!string.IsNullOrEmpty(dataSource))
            {
                this.ContentItem = printContext.Database.GetItem(dataSource);

                var xpath = this.RenderingItem["Item Selector"];
                if (!string.IsNullOrEmpty(xpath))
                {
                    Item selectorDataItem = this.ContentItem.Axes.SelectSingleItem(xpath);
                    if (selectorDataItem != null)
                    {
                        this.ContentItem = selectorDataItem;
                    }
                }
                this.DataSource = dataSource;
            }
        }
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
            if (field == null)
            {
                return contentField.Value;
            }
            
            if (field is LookupField || field is ReferenceField || field is MultilistField)
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
                else if (field is MultilistField && ((MultilistField)field).GetItems() != null)
                {
                    innerItem = ((MultilistField)field).GetItems().FirstOrDefault();
                }

                if (innerItem != null)
                {
                    if (!string.IsNullOrEmpty(this.ReferencedItemField) && innerItem.Fields[this.ReferencedItemField] != null)
                    {
                        this.ContentItem = innerItem;
                        this.ContentFieldName = this.ReferencedItemField;
                        return base.ParseContent(printContext);
                    }
                    return innerItem.DisplayName;
                }
                else
                {
                    return string.Empty;
                }
            }
            
            return base.ParseContent(printContext);
        }
    }
}
