using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlAlternateTitleRendering : XmlDroplinkRenderer
    {
        public string AlternateTitleDataKey { get; set; }
        public string AlternateTitleField { get; set; }

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
            if (!string.IsNullOrEmpty(AlternateTitleDataKey) && !string.IsNullOrEmpty(AlternateTitleField) && printContext.Settings.Parameters.ContainsKey(AlternateTitleDataKey))
            {
                var dataItemId = (string)printContext.Settings.Parameters[AlternateTitleDataKey];
                if (!string.IsNullOrEmpty(dataItemId) && this.ContentItem != null)
                {
                    var dataItem = this.ContentItem.Database.GetItem(dataItemId);
                    if (dataItem != null)
                    {
                        var fld = dataItem.Fields[HttpUtility.UrlDecode(AlternateTitleField)];
                        if (fld != null && !string.IsNullOrEmpty(fld.Value))
                        {
                            NameValueCollection titles = Sitecore.Web.WebUtil.ParseUrlParameters(fld.Value);
                            var personItemName = RemoveSpecialCharacter(this.ContentItem.Name);

                            foreach (string key in titles)
                            {
                                var title = titles[key];
                                if (RemoveSpecialCharacter(key) == personItemName)
                                {
                                    if (!string.IsNullOrEmpty(title))
                                    {
                                        return title;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return base.ParseContent(printContext);
        }

        private string RemoveSpecialCharacter(string name)
        {
            return Regex.Replace(name, @"[-_.\s]", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).ToLower();
        }
    }
}
