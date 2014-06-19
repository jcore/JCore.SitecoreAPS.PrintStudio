using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Security;
using Sitecore.SecurityModel;

namespace JCore.SitecoreAPS.PrintStudio.Helpers
{
    /// <summary>
    /// Static property and methods used for printing
    /// </summary>
    internal static class RenderingHelper
    {
        /// <summary>
        /// Default width for images and elements
        /// </summary>
        internal const string DefaultWidth = "5";

        /// <summary>
        /// Default height for images and elements
        /// </summary>
        internal const string DefaultHeight = "5";

        /// <summary>
        /// Gets the progress bar max width in mm
        /// </summary>
        /// <value>
        /// The width of the max scale.
        /// </value>
        internal const double MaxScaleWidth = 78.0;

        /// <summary>
        /// Gets the user points.
        /// </summary>
        /// <returns>
        /// The user points.
        /// </returns>
        internal static int GetUserPoints()
        {
            int userPoints;
            using (new SecurityDisabler())
            {
                UserProfile profile = Context.User.Profile;
                int.TryParse(profile["Points"], out userPoints);
            }

            return userPoints;
        }

        /// <summary>
        /// Gets the settings item.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>
        /// The settings item.
        /// </returns>
        internal static Item GetSettingsItem(Database database)
        {
            return database.GetItem("{E3612DF7-1CA0-4AD2-AEFC-5761FC49BC84}");
        }

        /// <summary>
        /// Replaces the variables.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="variables">The variables.</param>
        internal static void ReplaceVariables(ref string input, IDictionary<string, string> variables)
        {
            input = variables.Aggregate(input, (current, variable) => current.Replace(variable.Key, variable.Value));
        }

        /// <summary>
        /// Inners the XML.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string InnerText(this XElement elem)
        {
            return elem.Elements().Aggregate(string.Empty, (element, node) => element += node.Value.ToString());
        }
    }
}
