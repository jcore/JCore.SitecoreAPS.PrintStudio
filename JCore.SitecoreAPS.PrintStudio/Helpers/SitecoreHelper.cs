using System;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.PrintStudio.Configuration;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Helpers;
using Weil.Core.ErrorLogging;

namespace JCore.SitecoreAPS.PrintStudio.Helpers
{
    /// <summary>
    /// Overwrite for SitecoreHelper from Sitecore.PrintStudio.PublishingEngine
    /// </summary>
    public static class SitecoreHelper 
    {
        /// <summary>
        /// Fetches the field value.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        /// <param name="fieldname">The fieldname.</param>
        /// <param name="currentDatabase">The current database.</param>
        /// <returns></returns>
        public static string FetchFieldValue(Sitecore.Data.Items.Item currentItem, string fieldname, Database currentDatabase, string referenceFieldName = null)
        {
            string str1 = string.Empty;
            Field field = currentItem.Fields[fieldname];
            if (fieldname == "Item Reference" | fieldname == "Item Reference" | fieldname == "MasterSnippet Reference" | fieldname == "Preview Reference" | fieldname == "Content Reference" | fieldname == "MediaLibrary Reference" | fieldname == "Reference")
                str1 = currentItem.Fields[fieldname].Value;
            else if (field.Type.ToLower() == "date")
                str1 = ((DateField)field).DateTime.ToString();
            else if (field.Type.ToLower() == "datetime")
                str1 = ((DateField)field).DateTime.ToString();
            else if (field.Type.ToLower() == "checkbox")
                str1 = currentItem.Fields[fieldname].Value;
            else if (field.Type == "Droplink")
            {
                LookupField lookupField = (LookupField)field;
                string path = lookupField.InnerField.GetValue(true, true);
                Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                str1 = !(obj.Template.Name == "Pattern") ? lookupField.InnerField.GetValue(true, true) : obj.Name;
            }
            else if (field.Type == "Grouped Droplink")
            {
                LookupField lookupField = (LookupField)field;
                string path = lookupField.InnerField.GetValue(true, true);
                Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                str1 = !(obj.Template.Name == "Pattern") ? lookupField.InnerField.GetValue(true, true) : obj.Name;
            }
            else if (field.Type == "Droptree")
            {
                LookupField lookupField = (LookupField)field;
                string path = lookupField.InnerField.GetValue(true, true);
                Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                str1 = !(obj.Template.Name == "Pattern") ? lookupField.InnerField.GetValue(true, true) : obj.Name;
            }
            else if (field.Type == "Droplist")
                str1 = ((LookupField)field).InnerField.GetValue(true, true);
            else if (field.Type == "Grouped Droplist")
                str1 = ((LookupField)field).InnerField.GetValue(true, true);
            else if (field.Type == "Multilist")
                try
                {
                    str1 = String.Join(", ", ((MultilistField)field).Items.Select(i => currentDatabase.GetItem(i)[referenceFieldName]));
                }
                catch
                {

                }
            else if (field.Type == "Checklist")
            {
                string str2 = field.Value;
                char[] chArray = new char[1]
        {
          '|'
        };
                foreach (string path in str2.Split(chArray))
                {
                    Sitecore.Data.Items.Item obj = currentDatabase.GetItem(path);
                    str1 = str1 + obj.Name + "\n";
                }
            }
            else if (field.Type == "Rich Text")
            {
                string str2 = string.Empty;
                try
                {
                    str2 = RenderItemHelper.ReplaceSpecialCharacters(field.Value);
                }
                catch (Exception ex)
                {
                    LogManager<ILogProvider>.Error("FetchFieldValue ERROR while trying to replace special characters ", ex, typeof(SitecoreHelper));
                }
                XmlNode xmlNode = (XmlNode)((XmlDocument)new XmlDocument()).CreateElement("temp");
                xmlNode.InnerXml = str2;
                str1 = xmlNode.InnerText.Trim();
            }
            else if (field.Type == "Single-Line Text")
            {
                if (field.Value.Contains("<ParagraphStyle"))
                {
                    XmlElement element = ((XmlDocument)new XmlDocument()).CreateElement("temp");
                    element.InnerXml = Sitecore.PrintStudio.PublishingEngine.PatternBuilder.CdataParser(field.Value);
                    str1 = element.InnerText;
                }
                else
                    str1 = field.Value;
            }
            else if (field.Type == "Multi-Line Text")
            {
                if (field.Value.Contains("<ParagraphStyle"))
                {
                    XmlElement element = ((XmlDocument)new XmlDocument()).CreateElement("temp");
                    element.InnerXml = Sitecore.PrintStudio.PublishingEngine.PatternBuilder.CdataParser(field.Value);
                    str1 = element.InnerText;
                }
                else
                    str1 = field.Value;
            }
            else if (field.Type == "Image")
            {
                MediaItem mediaItem = (MediaItem)((ImageField)field).MediaItem;
                if (!Directory.Exists(WebConfigHandler.PrintStudioEngineSettings.ProjectsFolder + "Images\\"))
                    Directory.CreateDirectory(WebConfigHandler.PrintStudioEngineSettings.ProjectsFolder + "Images\\");
                string destination = WebConfigHandler.PrintStudioEngineSettings.ProjectsFolder + (object)"Images\\" + (string)(object)mediaItem.ID.Guid + "." + mediaItem.Extension;
                RenderItemHelper.CopyMediaToFile(mediaItem, destination, false);
                str1 = destination;
            }
            else if (field.Type == "Print Text")
            {
                if (field.Value.Contains("<ParagraphStyle"))
                {
                    str1 = field.Value;
                }
                else
                {
                    string str2 = '"'.ToString();
                    str1 = "<ParagraphStyle Style=" + str2 + "Text" + str2 + "><![CDATA[" + field.Value + "]]></ParagraphStyle>";
                }
            }
            else if (field.Type.ToLower() == "text")
                str1 = field.Value;
            else if (field.Type == "Integer" | field.Type == "Number")
                str1 = field.Value;
            else if (currentItem.Fields[fieldname].Value.Contains("{") | currentItem.Fields[fieldname].Value.Contains("/"))
            {
                try
                {
                    Sitecore.Data.Items.Item obj = currentDatabase.SelectSingleItem(currentItem.Fields[fieldname].Value);
                    if (obj.Template.Name == "Pattern")
                    {
                        if (obj.Children[0].Template.Name == "Image" | obj.Children[0].Template.Name == "Barcode")
                            str1 = WebConfigHandler.PrintStudioEngineSettings.ProjectsFolder + "Images\\NoImage.jpg";
                    }
                }
                catch (Exception ex)
                {
                    LogManager<ILogProvider>.Error("FetchFieldValue", ex, typeof(SitecoreHelper));
                }
            }
            return str1;
        }
    }
}
