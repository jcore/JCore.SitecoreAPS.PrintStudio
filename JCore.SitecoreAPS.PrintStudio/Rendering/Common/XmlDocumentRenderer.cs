using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;
using Sitecore.PrintStudio.PublishingEngine.Scripting;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlDocumentRenderer : InDesignItemRendererBase
    {
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            printContext.PageContainer = (XElement)null;
            if (this.RenderingItem["Skip"].Equals("1"))
                return;
            printContext.PageCount = 1;
            printContext.PageContainer = (XElement)null;
            XElement documentElement = JCore.SitecoreAPS.PrintStudio.Configuration.DocumentRendering.CreateDocumentElement(printContext, this.RenderingItem);
            if (documentElement == null)
                return;
            if (ScriptHelper.ExecuteScriptReference(printContext, this.RenderingItem, this.GetDataItem(printContext), documentElement))
            {
                output.Add((object)documentElement);
            }
            else
            {
                this.RenderChildren(printContext, documentElement);
                output.Add((object)documentElement);
            }
        }
    }
}
