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
    public class XmlInlineRenderer : InDesignItemRendererBase
    {
        public string Position { get; set; }
        public string Alignment { get; set; }

        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            if (string.IsNullOrEmpty(this.Tag))
            {
                this.RenderChildren(printContext, output);
            }
            else
            {
                XElement xelement = RenderItemHelper.CreateXElement(this.Tag, this.RenderingItem, printContext.Settings.IsClient, this.GetDataItem(printContext), true);
                if (!string.IsNullOrEmpty(Position))
                {
                    xelement.SetAttributeValue("Position", this.Position);
                }
                if (!string.IsNullOrEmpty(this.RenderingItem["Center"]))
                {
                    xelement.SetAttributeValue("Center", this.RenderingItem["Center"]);
                }
                else
                {
                    xelement.SetAttributeValue("Center", "0");
                }
                if (!string.IsNullOrEmpty(Alignment))
                {
                    xelement.SetAttributeValue("Alignment", this.Alignment);
                }
                output.Add((object)xelement);
                this.RenderChildren(printContext, xelement);
            }
        }
    }
}
