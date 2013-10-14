using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Sitecore.PrintStudio.PublishingEngine;
using JCore.SitecoreAPS.PrintStudio.Rendering.Common;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.PitchBook
{
    public class XmlSectionTitleRenderer : XmlDynamicContentRenderer
    {
        /// <summary>
        /// Gets or sets the section title.
        /// </summary>
        /// <value>
        /// The section title.
        /// </value>
        public string SectionTitle { get; set; }
        
        /// <summary>
        /// Parses the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns>
        /// The content.
        /// </returns>
        protected override string ParseContent(PrintContext printContext)
        {
            if (!string.IsNullOrEmpty(this.SectionTitle))
            {
                return HttpUtility.UrlDecode(this.SectionTitle);
            }
            else
            {
                var dataItem = this.GetDataItem(printContext);
                if (dataItem != null)
                {
                    if (!string.IsNullOrEmpty(this.RenderingItem["Item Field"]))
                    {
                        return dataItem[this.RenderingItem["Item Field"]];
                    }
                    else
                    {
                        return dataItem.Name;
                    }
                }
            }
            return string.Empty;
        }
    }
}
