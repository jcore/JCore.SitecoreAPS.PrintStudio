using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlConditionalContentRenderer : XmlDynamicContentRenderer
    {
        public string DisplayContent { get; set; }

        protected override void RenderContent(Sitecore.PrintStudio.PublishingEngine.PrintContext printContext, System.Xml.Linq.XElement output)
        {
            string content = this.ParseContent(printContext);
            if (!string.IsNullOrWhiteSpace(content))
            {
                output.Add(HttpUtility.UrlDecode(DisplayContent));
            }
            return;
        }
    }
}
