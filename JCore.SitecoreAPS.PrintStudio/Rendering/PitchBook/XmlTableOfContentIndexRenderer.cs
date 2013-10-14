using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.PrintStudio.PublishingEngine;
using JCore.SitecoreAPS.PrintStudio.Helpers;
using JCore.SitecoreAPS.PrintStudio.Rendering.Common;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.PitchBook
{
    /// <summary>
    /// Defines the TableOfContentIndexRenderer class.
    /// </summary>
    public class XmlTableOfContentIndexRenderer : XmlContentRenderer
    {
        /// <summary>
        /// Gets or sets the name of the index.
        /// </summary>
        /// <value>
        /// The name of the index.
        /// </value>
        public string IndexName { get; set; }

        /// <summary>
        /// Gets or sets the replace var.
        /// </summary>
        /// <value>
        /// The replace var.
        /// </value>
        public string ReplaceVar { get; set; }

        /// <summary>
        /// Parses the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns>
        /// The content.
        /// </returns>
        protected override string ParseContent(PrintContext printContext)
        {
            string content = base.ParseContent(printContext);

            IDictionary<string, string> variables = new Dictionary<string, string>();

            int tocIndex = 0;
            if (printContext.Settings.Parameters.ContainsKey(this.IndexName))
            {
                tocIndex = (int)printContext.Settings.Parameters[this.IndexName];
            }

            variables.Add(this.ReplaceVar, tocIndex.ToString());
            RenderingHelper.ReplaceVariables(ref content, variables);

            printContext.Settings.Parameters[this.IndexName] = tocIndex + 1;

            return content;
        }
    }
}
