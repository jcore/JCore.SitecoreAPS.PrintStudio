using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlDynamicQRCodeImage : XmlImageRenderer
    {
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            XElement xelement = RenderItemHelper.CreateXElement("Image", this.RenderingItem, printContext.Settings.IsClient, (Item)null);
            string absoluteFilePath = string.Empty;

            Item dataItem = this.GetDataItem(printContext);
            if (dataItem != null)
            {
                absoluteFilePath = ImageRendering.CreateQrOnServer(printContext.Settings, this.RenderingItem, LinkManager.GetItemUrl(dataItem, new UrlOptions() { AlwaysIncludeServerUrl = true, LowercaseUrls = true }));
            }

            if (string.IsNullOrEmpty(absoluteFilePath))
            {
                xelement.SetAttributeValue((XName)"LowResSrc", (object)this.RenderingItem["LowResSrc"]);
                xelement.SetAttributeValue((XName)"HighResSrc", (object)this.RenderingItem["HighResSrc"]);
            }
            else
            {
                string str2 = printContext.Settings.FormatResourceLink(absoluteFilePath);
                xelement.SetAttributeValue((XName)"LowResSrc", (object)str2);
                xelement.SetAttributeValue((XName)"HighResSrc", (object)str2);
            }

            output.Add((object)xelement);
            this.RenderChildren(printContext, xelement);
        }
    }
}
