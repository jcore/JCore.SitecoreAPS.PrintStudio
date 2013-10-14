using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.PrintStudio.PublishingEngine;
using JCore.SitecoreAPS.PrintStudio.Rendering.Common;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.PitchBook
{
    /// <summary>
    /// Defines the XmlBookIndexRenderer class.
    /// </summary>
    public class XmlBookIndexRenderer : XmlDynamicContentRenderer
    {
        /// <summary>
        /// Gets or sets the name of the index.
        /// </summary>
        /// <value>
        /// The name of the index.
        /// </value>
        public string IndexName { get; set; }

        /// <summary>
        /// Gets or sets the name of the list.
        /// </summary>
        /// <value>
        /// The name of the list.
        /// </value>
        public string ListName { get; set; }

        /// <summary>
        /// Gets or sets the name of the key field.
        /// </summary>
        /// <value>
        /// The name of the key field.
        /// </value>
        public string KeyFieldName { get; set; }

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

            if (content != null)
            {
                Item contentItem = printContext.Database.GetItem(content);
                if (contentItem != null)
                {
                    string key = contentItem[this.KeyFieldName] ?? contentItem.Name;

                    var indexList = new Dictionary<string, List<string>>();
                    if (printContext.Settings.Parameters.ContainsKey(this.ListName))
                    {
                        indexList = (Dictionary<string, List<string>>)printContext.Settings.Parameters[this.ListName];
                    }

                    var indexes = new List<string>();
                    if (indexList.ContainsKey(key))
                    {
                        indexes = indexList[key];
                    }

                    int tocIndex = 0;
                    if (printContext.Settings.Parameters.ContainsKey(this.IndexName))
                    {
                        tocIndex = (int)printContext.Settings.Parameters[this.IndexName] - 1;
                    }

                    indexes.Add(tocIndex.ToString());
                    indexList[key] = indexes;

                    printContext.Settings.Parameters[this.ListName] = indexList;
                }
            }

            return string.Empty;
        }
    }
}
