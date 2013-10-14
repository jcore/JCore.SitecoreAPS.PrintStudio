using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Helpers;
using Sitecore.PrintStudio.PublishingEngine.Rendering;
using Sitecore.PrintStudio.PublishingEngine.Scripting;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlVariableRenderer : InDesignItemRendererBase
    {
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            XElement parentNode = new XElement((XName)"Variables");
            if (ScriptHelper.ExecuteScriptReference(printContext, this.RenderingItem, this.GetDataItem(printContext), parentNode))
            {
                int num = Enumerable.Count<XElement>(parentNode.Descendants((XName)"Variable"));
                if (num > 0)
                    printContext.PageCount += num;
                output.Add((object)parentNode.Elements());
            }
            else
            {
                XElement page = this.CreateVariable(printContext, false, false);
                ++printContext.PageCount;
                this.RenderChildren(printContext, page);
                output.Add((object)page);
            }
        }

        /// <summary>
        /// Creates the variable.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="skipNumber">if set to <c>true</c> [skip number].</param>
        /// <param name="emptyNumber">if set to <c>true</c> [empty number].</param>
        /// <returns></returns>
        protected virtual XElement CreateVariable(PrintContext printContext, bool skipNumber, bool emptyNumber)
        {
            Item renderingItem = this.RenderingItem;
            XElement xelement = RenderItemHelper.CreateXElement("Variable", renderingItem, printContext.Settings.IsClient, (Item)null, true);
                
            if (renderingItem.Fields["Find"] != null && renderingItem.Fields["Replace"] != null)
            {
                if (printContext.Settings.IsClient && renderingItem.Fields["PageXML"] != null)
                    XElementExtensions.AddInnerXml(xelement, renderingItem.Fields["PageXML"].Value, false);
                
                var findElement = new XElement("Find", (object)renderingItem["Find"]);
                xelement.Add(findElement);

                if (printContext.Settings.Parameters.ContainsKey("Variables") &&
                (SafeDictionary<string, object>)printContext.Settings.Parameters["Variables"] != null)
                {
                    var variables = (SafeDictionary<string, object>)printContext.Settings.Parameters["Variables"];
                    var variable = variables.FirstOrDefault(v => v.Key == (string)renderingItem["Find"]);
                    var replaceElement = new XElement("Replace", (object)variable.Value);
                    xelement.Add(replaceElement);
                }
                else
                {
                    var replaceElement = new XElement("Replace", (object)renderingItem["Replace"]);
                    xelement.Add(replaceElement);
                }
             
            }

            return xelement;
        }
    }
}
