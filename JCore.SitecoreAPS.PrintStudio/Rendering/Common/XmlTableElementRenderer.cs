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
    public class XmlTableElementRenderer : XmlElementRenderer
    {
        public string CustomAttribute { get; set; }

        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            if (string.IsNullOrEmpty(this.Tag))
            {
                this.RenderChildren(printContext, output);
            }
            else
            {
                XElement xelement = RenderItemHelper.CreateXElement(this.Tag, this.RenderingItem, printContext.Settings.IsClient, this.GetDataItem(printContext), true);
                if (!string.IsNullOrWhiteSpace(this.CustomAttribute))
                {
                    if (this.CustomAttribute == "DataSource")
                    {
                        if (!string.IsNullOrWhiteSpace(this.DataSource))
                        {
                            xelement.SetAttributeValue((XName)"CustomAttribute", this.DataSource);
                        }
                    }
                    else
                    {
                        xelement.SetAttributeValue((XName)"CustomAttribute", this.CustomAttribute);
                    }
                }
                if (!string.IsNullOrEmpty(xelement.Value) || this.RenderingItem["Publish when empty"] == "1")
                {
                    output.Add((object)xelement);
                }
                this.RenderChildren(printContext, xelement);
            }
        }
    }
}
