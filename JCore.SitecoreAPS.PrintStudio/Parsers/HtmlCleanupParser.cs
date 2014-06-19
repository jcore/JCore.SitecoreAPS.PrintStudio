using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine.Helpers;
using Sitecore.PrintStudio.PublishingEngine.Text;
using Weil.SC.Util.Extensions;

namespace JCore.SitecoreAPS.PrintStudio.Parsers
{
    public class HtmlCleanupParser 
    {
        public string Parse(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return string.Empty;
            try
            {
                return inputString.StripHtmlTags();
            }
            catch (Exception ex)
            {
                Log.Error("Parse error", ex, this);
            }
            return string.Empty;
        }
    }
}
