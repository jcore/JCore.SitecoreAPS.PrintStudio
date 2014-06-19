using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.PrintStudio.InDesign.Soap;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Helpers;

namespace JCore.SitecoreAPS.PrintStudio.Configuration
{
    public class DocumentRendering
    {
        public static XElement CreateDocumentElement(PrintContext printContext, Sitecore.Data.Items.Item renderingItem)
        {
            int languageIndex = LanguageManager.GetLanguages(printContext.Database).IndexOf(printContext.Language);
            XmlNode documentElement = Sitecore.PrintStudio.PublishingEngine.Rendering.DocumentRendering.CreateDocumentElement(new XmlDocument(), renderingItem, printContext.Database, languageIndex, printContext.StartItem, printContext.Settings.FormatResourceLink(printContext.Settings.CacheFolder), printContext.Settings.CacheFolder, JCore.SitecoreAPS.PrintStudio.Configuration.PrintStudioEngineSettings.EngineTemplates, printContext.Settings.IsClient ? "1" : "0", printContext.Settings.OverrideCache);
            if (documentElement != null)
                return XElementExtensions.GetXElement(documentElement);
            else
                return (XElement)null;
        }
    }
}
