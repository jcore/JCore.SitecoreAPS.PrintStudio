using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCore.SitecoreAPS.PrintStudio
{
    public static class PatternBuilderExtension
    {
        /// <summary>
        /// Transforms the content of the rich.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="database">The database.</param>
        /// <param name="tempDoc">The temp doc.</param>
        /// <param name="defaultFormatting">The default formatting.</param>
        /// <param name="sourceFormat">The source format.</param>
        /// <param name="targetFormat">The target format.</param>
        /// <returns></returns>
        internal static string TransformRichContent(string value, Sitecore.Data.Database database, System.Xml.XmlDataDocument tempDoc, Sitecore.Data.Items.Item defaultFormatting, string sourceFormat, string targetFormat)
        {
            var result = Sitecore.PrintStudio.PublishingEngine.PatternBuilder.TransformRichContent(value, database, tempDoc, defaultFormatting, sourceFormat, targetFormat);
            return result.Replace("&amp;", "&");
        }

        /// <summary>
        /// Riches the text to text parser.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="rtfField">if set to <c>true</c> [RTF field].</param>
        /// <param name="replaceAllXmlChars">if set to <c>true</c> [replace all XML chars].</param>
        /// <returns></returns>
        internal static string RichTextToTextParser(string inputString, bool rtfField, bool replaceAllXmlChars)
        {
            return Sitecore.PrintStudio.PublishingEngine.PatternBuilder.RichTextToTextParser(inputString, rtfField, replaceAllXmlChars);
        }
    }
}
