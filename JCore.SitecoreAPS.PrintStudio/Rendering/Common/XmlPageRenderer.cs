using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Helpers;
using Sitecore.PrintStudio.PublishingEngine.Scripting;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlPageRenderer : Sitecore.PrintStudio.PublishingEngine.Rendering.XmlPageRenderer
    {
        /// <summary>
        /// Gets a value indicating whether [skip if empty].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [skip if empty]; otherwise, <c>false</c>.
        /// </value>
        public bool SkipIfEmpty
        {
            get
            {
                Item renderingItem = this.RenderingItem;
                if (renderingItem.Fields["Skip if empty"] != null && renderingItem.Fields["Skip if empty"].Value.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            if (this.RenderingItem["Skip"].Equals("1"))
                return;
            XElement parentNode = new XElement((XName)"ScriptResult");
            if (ScriptHelper.ExecuteScriptReference(printContext, this.RenderingItem, this.GetDataItem(printContext), parentNode))
            {
                int num = Enumerable.Count<XElement>(parentNode.Descendants((XName)"Page"));
                if (num > 0)
                    printContext.PageCount += num;
                output.Add((object)parentNode.Elements());
            }
            else
            {
                XElement page = this.CreatePage(printContext, false, false);
                if (page != null)
                {                    
                    ++printContext.PageCount;
                    this.RenderChildren(printContext, page);
                    if (SkipIfEmpty && string.IsNullOrWhiteSpace(page.Value))
                    {
                        --printContext.PageCount;
                        return;
                    }
                    output.Add((object)page);
                }
            }
        }
    }
}
