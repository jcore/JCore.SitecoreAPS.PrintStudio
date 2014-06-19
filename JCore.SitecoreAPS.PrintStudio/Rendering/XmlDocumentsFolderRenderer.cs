using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering
{
    public class XmlDocumentsFolderRenderer : InDesignItemRendererBase
    {
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            XElement parentContainer = printContext.DocumentContainer;
            if (parentContainer == null)
            {
                parentContainer = new XElement((XName)"Documents");
                output.Add((object)parentContainer);
                printContext.DocumentContainer = parentContainer;
            }
            this.RenderChildren(printContext, parentContainer);
        }
    }
}
