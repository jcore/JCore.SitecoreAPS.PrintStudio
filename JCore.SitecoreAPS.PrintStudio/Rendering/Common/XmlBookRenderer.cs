using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.PrintStudio.Configuration;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlBookRenderer : InDesignItemRendererBase
    {
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            if (output.Element((XName)"Book") != null)
                return;
            printContext.DocumentContainer = (XElement)null;
            XElement xelement = RenderItemHelper.CreateXElement("Book", this.RenderingItem, printContext.Settings.IsClient);
            string name = this.RenderingItem.Name;
            string newValue1 = this.RenderingItem.ID.ToString().Substring(1, 8);
            foreach (Item obj in printContext.Database.GetItem(JCore.SitecoreAPS.PrintStudio.Configuration.PrintStudioEngineSettings.EngineTemplates + "/" + this.RenderingItem.TemplateName + "/attributes").Children)
            {
                Field field1 = this.RenderingItem.Fields[obj.Name];
                Field field2 = obj.Fields["Default value"];
                string str = field1.Value;
                bool flag = false;
                if (printContext.Settings.IsClient)
                {
                    if (str.Contains("$FolderName"))
                        str = str.Replace("$FolderName", RenderItemHelper.EnsureFolderPath(printContext.Settings.FormatResourceLink(printContext.Settings.CacheFolder)));
                    if (str.Contains("$ItemName"))
                        str = str.Replace("$ItemName", name);
                    if (str.Contains("$ID"))
                        str = str.Replace("$ID", newValue1);
                }
                else
                {
                    string absoluteFilePath = Path.Combine(printContext.Settings.ResultFolder, printContext.Settings.ResultFileName);
                    string newValue2 = absoluteFilePath.IndexOf("\\") > -1 ? string.Empty : RenderItemHelper.EnsureFolderPath(printContext.Settings.FormatResourceLink(printContext.Settings.CacheFolder));
                    string newValue3 = printContext.Settings.FormatResourceLink(absoluteFilePath);
                    if (obj.Name.Trim() == "PDFFilename")
                    {
                        if (printContext.Settings.PrintExportType == PrintExportType.Pdf)
                        {
                            str = str.Replace("$FolderName", newValue2).Replace("$ItemName_$ID.pdf", newValue3);
                        }
                        else
                        {
                            str = string.Empty;
                            flag = true;
                        }
                    }
                    else if (obj.Name.Trim() == "SWFFilename")
                    {
                        if (printContext.Settings.PrintExportType == PrintExportType.Flash)
                        {
                            str = str.Replace("$FolderName", newValue2).Replace("$ItemName_$ID.swf", newValue3);
                        }
                        else
                        {
                            str = string.Empty;
                            flag = true;
                        }
                    }
                    else
                        str = str.Replace("$FolderName", newValue2).Replace("$ItemName", name).Replace("$ID", newValue1);
                    if (obj.Name.Trim() == "ExportPDF")
                        str = (printContext.Settings.PrintExportType == PrintExportType.Pdf).ToString();
                    if (obj.Name.Trim() == "ExportSWF")
                        str = (printContext.Settings.PrintExportType == PrintExportType.Flash).ToString();
                    if (obj.Name.Trim() == "BookFilename")
                    {
                        str = string.Empty;
                        flag = true;
                    }
                    if (!(obj.Name.Trim() == "PrintBook") && !(obj.Name.Trim() == "CreateBook"))
                    {
                        int num = obj.Name.Trim() == "ExportBookPDF" ? 1 : 0;
                    }
                }
                if (str.Length > 0 || flag)
                    xelement.SetAttributeValue((XName)obj.Name, (object)str);
                else
                    xelement.SetAttributeValue((XName)obj.Name, (object)field2.Value);
            }
            xelement.SetAttributeValue((XName)"PDFExportSetting", (object)printContext.Settings.PdfExportSetting);
            xelement.SetAttributeValue((XName)"UseHighRes", (object)printContext.Settings.UseHighRes.ToString());
            if (printContext.Settings.PrintExportType == PrintExportType.Flash && this.RenderingItem.Fields["ExportSWF"].Value.ToLower().Equals("true") && !string.IsNullOrEmpty(this.RenderingItem.Fields["Flash export"].Value))
            {
                foreach (Field field in printContext.Database.GetItem(this.RenderingItem.Fields["Flash export"].Value).Fields)
                {
                    if (!field.Name.Contains("__"))
                    {
                        if (field.Type.Equals("Checkbox"))
                        {
                            if (!string.IsNullOrEmpty(field.Value) && field.Value.Equals("1"))
                                xelement.SetAttributeValue((XName)field.Name, (object)"True");
                            else
                                xelement.SetAttributeValue((XName)field.Name, (object)"False");
                        }
                        else
                            xelement.SetAttributeValue((XName)field.Name, (object)field.Value);
                    }
                }
            }
            this.RenderChildren(printContext, xelement);
            output.Add((object)xelement);
        }
    }
}
