using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// Renders paragraph style element markup.
    /// </summary>
    public class XmlVariablesFolderRenderer : InDesignItemRendererBase
    {
        private string spreadId = string.Empty;

        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            if (output.Element((XName)"Variables") != null)
                return;
            XElement parentContainer = new XElement((XName)"Variables");
            output.Add((object)parentContainer);
            this.RenderChildren(printContext, parentContainer);
        }

        protected override void BeginRender(PrintContext printContext)
        {
            if (printContext.StartItem.TemplateName.Equals("P_Variable", StringComparison.InvariantCultureIgnoreCase) && printContext.StartItem.Fields["Spread ID"] != null && !string.IsNullOrEmpty(printContext.StartItem.Fields["Spread ID"].Value))
                this.spreadId = printContext.StartItem.Fields["Spread ID"].Value;
            if (string.IsNullOrEmpty(this.spreadId))
                return;
            int count = printContext.CurrentItemAncestors.IndexOf(this.RenderingItem.ID);
            if (count <= 0)
                return;
            printContext.CurrentItemAncestors.RemoveRange(0, count);
        }

        protected override void RenderChildren(PrintContext printContext, XElement parentContainer)
        {
            IEnumerable<Item> children = (IEnumerable<Item>)this.RenderingItem.Children;
            if (!string.IsNullOrEmpty(this.spreadId))
                children = Enumerable.Where<Item>((IEnumerable<Item>)this.RenderingItem.Children, (Func<Item, bool>)(page => this.spreadId.Equals(page["Spread ID"], StringComparison.InvariantCultureIgnoreCase)));
            this.RenderChildren(printContext, parentContainer, children);
        }
    }
}
