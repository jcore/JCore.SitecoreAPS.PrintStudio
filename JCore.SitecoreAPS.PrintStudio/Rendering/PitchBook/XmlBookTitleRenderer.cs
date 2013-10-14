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
    public class XmlBookTitleRenderer : XmlDynamicContentRenderer
    {
        /// <summary>
        /// Gets or sets the section title.
        /// </summary>
        /// <value>
        /// The section title.
        /// </value>
        public string Title { get; set; }
        
        /// <summary>
        /// Parses the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns>
        /// The content.
        /// </returns>
        protected override string ParseContent(PrintContext printContext)
        {
            var title = HttpUtility.UrlDecode(this.Title);
            if (!string.IsNullOrEmpty(title) && !title.Contains("{0}"))
            {
                return title;
            }
            else
            {
                if (printContext.Settings.Parameters.ContainsKey("Weil_Username") && !string.IsNullOrEmpty(printContext.Settings.Parameters["Weil_Username"].ToString()))
                {
                    return String.Format(title, printContext.Settings.Parameters["Weil_Username"]);
                }
                else
                {
                    return String.Format(title, "You");
                }
            }            
        }
    }
}
