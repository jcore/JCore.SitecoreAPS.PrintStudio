using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace JCore.SitecoreAPS.PrintStudio.Helpers
{
    public static class ItemHelper
    {
        /// <summary>
        /// Determines if the item implements the specified template.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="templateName">Name of the template.</param>
        /// <returns>
        /// A boolean indicating weather the item implements the template or not
        /// </returns>
        public static bool DoesItemImplementTemplate(this Item item, ID templateId)
        {
            if (item == null || item.Template == null)
            {
                return false;
            }

            var items = new List<TemplateItem> { item.Template };
            int index = 0;

            // flatten the template tree during search
            while (index < items.Count)
            {
                // check for match
                TemplateItem template = items[index];
                if (template.ID == templateId)
                {
                    return true;
                }

                // add base templates to the list
                items.AddRange(template.BaseTemplates);

                // inspect next template
                index++;
            }

            // nothing found
            return false;
        }
        /// <summary>
        /// Gets the base templates.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static List<TemplateItem> GetBaseTemplates(this Item item)
        {
            if (item == null || item.Template == null)
            {
                return new List<TemplateItem>();
            }

            var items = new List<TemplateItem> { item.Template };
            int index = 0;

            // flatten the template tree during search
            while (index < items.Count)
            {
                TemplateItem template = items[index];
                // add base templates to the list
                items.AddRange(template.BaseTemplates);

                // inspect next template
                index++;
            }
            return items;
        }
    }
}
