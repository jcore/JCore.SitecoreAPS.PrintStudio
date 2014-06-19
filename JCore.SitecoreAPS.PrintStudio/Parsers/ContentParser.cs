using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Text;
using Weil.Core.ErrorLogging;

namespace JCore.SitecoreAPS.PrintStudio.Parsers
{
    public static class ContentParser
    {
        /// <summary>
        /// Parses the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="printContext">The print context.</param>
        /// <param name="contentField">The content field.</param>
        /// <returns></returns>
        public static string ParseField(CustomField field, Field contentField = null)
        {
            if (field == null)
            {
                if (contentField != null)
                {
                    return contentField.Value.Trim();
                }
                else
                {
                    return string.Empty;
                }
            }

            if (field is DateField)
            {
                if (!string.IsNullOrEmpty(field.Value))
                {
                    return ((DateField)field).DateTime.ToString("MMMM dd, yyyy");
                }
                else
                {
                    return string.Empty;
                }
            }
            return field.Value.Trim();
        }
    }
}
