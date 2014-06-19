using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;
using JCore.SitecoreAPS.PrintStudio.Rendering.Common;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Biography
{
    /// <summary>
    /// Defines repeater for project items.
    /// </summary>
    public class XmlWidgetsRepeater : XmlRepeater
    {
        private List<RenderingReference> Renderings { get; set; } 
        private const string DefaultDevice = "{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}";
        public string Placeholder { get; set; }
        public string ChildDataRenderingKeyName { get; set; }
         /// <summary>
        /// Preliminary render action invoked before RenderContent <see cref="RenderContent"/>.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            var dataItem = this.GetDataItem(printContext);
            if (dataItem != null)
            {
                Renderings = new List<RenderingReference>();

                var device = dataItem.Database.GetItem(DefaultDevice);
                var renderings = dataItem.Visualization.GetRenderings(device, false);
                foreach (var rendering in renderings)
                {
                    if (rendering != null && Placeholder == StringUtil.GetLastPart(rendering.Placeholder, '/', rendering.Placeholder))
                    {
                        Renderings.Add(rendering);
                    }
                }
                this.Count = Renderings != null ? Renderings.Count.ToString() : "0";
            }
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, System.Xml.Linq.XElement output)
        {
            if (!string.IsNullOrEmpty(this.ChildDataKeyName))
            {
                printContext.Settings.Parameters[this.ChildDataKeyName] = this.DataSource;
            }

            var dataItem = this.GetDataItem(printContext);
            if (dataItem == null && !printContext.Settings.IsClient)
            {
                return;
            }
            if (!string.IsNullOrEmpty(this.RenderingItem["Data Key"]))
            {
                printContext.Settings.Parameters[this.RenderingItem["Data Key"]] = dataItem.ID.ToString();
            }
            if (Renderings.Count > 0)
            {
                foreach (var rendering in this.Renderings)
                {
                    var dataSource = !string.IsNullOrEmpty(rendering.Settings.DataSource) ? rendering.Settings.DataSource : dataItem.ID.ToString();
                    this.DataSource = dataSource;
                    if (!string.IsNullOrEmpty(this.ChildDataKeyName))
                    {                        
                        printContext.Settings.Parameters[this.ChildDataKeyName] = dataSource;
                    }
                    if (!string.IsNullOrEmpty(this.ChildDataRenderingKeyName))
                    {
                        printContext.Settings.Parameters[this.ChildDataRenderingKeyName] = rendering;
                    }
                    RenderChildren(printContext, output);
                }
                return;
            }            

            int count;
            if (!string.IsNullOrEmpty(this.Count) && int.TryParse(this.Count, out count) && count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    // Render child elements
                    this.RenderChildren(printContext, output);
                }
                return;
            }
        }
     }
}
