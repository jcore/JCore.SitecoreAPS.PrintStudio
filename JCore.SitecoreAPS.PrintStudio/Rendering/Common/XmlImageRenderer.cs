using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// Renderer to generate xml for Image
    /// </summary>
    public class XmlImageRenderer : XmlDynamicContentRenderer
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
            string str1 = this.RenderingItem["MediaLibrary Reference"];
            bool flag = string.IsNullOrEmpty(str1);
            ID result;
            if (!flag && ID.TryParse(str1, out result) && result != ID.Null)
            {
                MediaItem mediaItem = (MediaItem)printContext.Database.GetItem(result);
                absoluteFilePath = ImageRendering.CreateImageOnServer(printContext.Settings, mediaItem);
            }
            else
            {
                string index = this.RenderingItem.Parent["Item Field"];
                flag = flag & string.IsNullOrEmpty(index);
                Item dataItem = this.GetDataItem(printContext);
                if (dataItem != null)
                {
                    Field field = null;
                    if (!string.IsNullOrEmpty(index))
                    {
                        field = dataItem.Fields[index];
                    }
                    else if (!string.IsNullOrEmpty(this.RenderingItem["Item Field"]))
                    {
                        field = dataItem.Fields[this.RenderingItem["Item Field"]];
                    }
                    if (field != null)
                    {
                        if (field.Type == "QR Code Image")
                            absoluteFilePath = ImageRendering.CreateQrOnServer(printContext.Settings, this.RenderingItem, field.Value);
                        else if (field.Type == "Image")
                        {
                            MediaItem mediaItem = (MediaItem)((ImageField)field).MediaItem;
                            absoluteFilePath = ImageRendering.CreateImageOnServer(printContext.Settings, mediaItem);

                            //what is the size of parent ImageFrame. Scale image to fit it. Default APS functionality doesn't do it properly
                            var frameWidth = this.RenderingItem.Parent["Width"];
                            var frameHeight = this.RenderingItem.Parent["Height"];
                            var height = GetNewHeight(mediaItem, frameWidth);

                            xelement.SetAttributeValue((XName)"Width", (object)frameWidth);
                            xelement.SetAttributeValue((XName)"Height", (object)height);

                            xelement.SetAttributeValue((XName)"Y", (object)((decimal.Parse(frameHeight) - height) / 2));
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(absoluteFilePath) && flag)
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

        /// <summary>
        /// Gets the new height.
        /// </summary>
        /// <param name="mediaItem">The media item.</param>
        /// <param name="parentWidth">Width of the parent.</param>
        /// <returns></returns>
        public virtual decimal GetNewHeight(MediaItem mediaItem, string parentWidth)
        {
            Assert.ArgumentNotNullOrEmpty("parentWidth", parentWidth);
            var width = decimal.Parse(parentWidth);
            if (mediaItem == null)
            {
                return width;
            }
            var strWidth = mediaItem.InnerItem["Width"];
            var strHeight = mediaItem.InnerItem["Height"];
            int mediaHeight = 0;
            int mediaWidth = 0;
                
            if (!string.IsNullOrWhiteSpace(strWidth))
            {
                if (!int.TryParse(strWidth, out mediaWidth))
                {
                    mediaWidth = 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(strHeight))
            {                
                if (!int.TryParse(strHeight, out mediaHeight))
                {
                    mediaHeight = 0;
                }
            }
            if (mediaWidth > 0)
            {
                return width * mediaHeight / mediaWidth;
            }
            return width;
        }
    }
}
