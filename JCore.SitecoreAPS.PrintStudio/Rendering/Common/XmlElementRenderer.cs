using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlElementRenderer : InDesignItemRendererBase
    {
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            if (string.IsNullOrEmpty(this.Tag))
            {
                this.RenderChildren(printContext, output);
            }
            else
            {
                XElement xelement = RenderItemHelper.CreateXElement(this.Tag, this.RenderingItem, printContext.Settings.IsClient, this.GetDataItem(printContext), true);
                this.RenderChildren(printContext, xelement);
                if (!string.IsNullOrEmpty(xelement.Value) || this.RenderingItem["Publish when empty"] == "1")
                {
                    output.Add((object)xelement);
                }
            }
        }
    }
}
