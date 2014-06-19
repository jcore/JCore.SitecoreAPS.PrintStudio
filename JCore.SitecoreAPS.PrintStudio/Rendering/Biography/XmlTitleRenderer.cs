using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCore.SitecoreAPS.PrintStudio.Rendering.Common;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using Weil.SC.Entities.Data.Items.Weil.Content.Pages;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Biography
{
    public class XmlTitleRenderer : XmlDroplinkRenderer
    {
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

            var textField = this.ContentItem[PersonItem.FieldNames.TitleFieldName];
            if (!string.IsNullOrWhiteSpace(textField))
            {
                this.ContentFieldName = PersonItem.FieldNames.TitleFieldName;
                return base.ParseContent(printContext);
            }
            else
            {
                return base.ParseContent(printContext);
            }
        }
    }
}
