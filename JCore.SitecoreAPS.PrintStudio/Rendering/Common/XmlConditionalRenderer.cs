using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JCore.SitecoreAPS.PrintStudio.Parsers;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;
using Weil.SC.Entities.Factories;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    public class XmlConditionalRenderer : XmlDynamicContentRenderer
    {
        public string Condition { get; set; }
        public string AcceptableRenderings { get; set; }
        public string RenderingKey { get; set; }

        protected override void RenderContent(Sitecore.PrintStudio.PublishingEngine.PrintContext printContext, System.Xml.Linq.XElement output)
        {
            if (string.IsNullOrEmpty(Condition) || this.ContentItem == null)
            {
                return;
            }
            try
            {
                var conditionSegments = HttpUtility.UrlDecode(this.Condition).Split(new string[] { "," }, StringSplitOptions.None);
                if (conditionSegments.Length < 1)
                {
                    return;
                }
                var dataItemProperty = conditionSegments[0];
                var dataItemPropertyValue = conditionSegments.Length >= 2 ? conditionSegments[1] : string.Empty;

                var dataItem = this.ContentItem;
                if (dataItemProperty.Contains("."))
                {
                    var properties = dataItemProperty.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (properties.Any())
                    {
                        var firstProperty = this.ContentItem.GetType().GetProperty(properties.FirstOrDefault());
                        var propertyValue = firstProperty.GetValue(this.ContentItem);
                        if (propertyValue != null && properties.Count() >= 2)
                        {
                            var customItem = ItemFactory.GetCustomItem((Item)propertyValue);
                            if (customItem != null)
                            {
                                var secondProperty = customItem.GetType().GetProperty(properties[1]);
                                if (secondProperty != null)
                                {
                                    var secondPropertyValue = secondProperty.GetValue(customItem);
                                    if (secondPropertyValue != null)
                                    {
                                        var valueArray = dataItemPropertyValue.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                        if (valueArray.Any() && IsVisible(valueArray, secondPropertyValue, printContext))
                                        {
                                            this.RenderChildren(printContext, output);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    object propertyValue = null;
                    var propertyInfo = this.ContentItem.GetType().GetProperty(dataItemProperty);
                    if (propertyInfo == null && this.ContentItem.Fields[dataItemProperty] != null)
                    {
                        propertyValue = ContentParser.ParseField(FieldTypeManager.GetField(this.ContentItem.Fields[dataItemProperty]));
                    }
                    if (propertyInfo == null && propertyValue == null)
                    {
                        var customItem = ItemFactory.GetCustomItem(this.ContentItem);
                        if (customItem != null)
                        {
                            propertyInfo = customItem.GetType().GetProperty(dataItemProperty);
                            if (propertyInfo != null)
                            {
                                var val = propertyInfo.GetValue(customItem);
                                if (val is string)
                                {
                                    propertyValue = val;
                                }
                                else if (val is CustomField)
                                {
                                    propertyValue = ContentParser.ParseField((CustomField)val);
                                }
                                else if (val is IEnumerable<Item>)
                                {
                                    propertyValue = string.Join("|", ((IEnumerable<Item>)val).Select(i => i.ID.ToString()));
                                }
                                else if (val is IEnumerable<CustomItem>)
                                {
                                    propertyValue = string.Join("|", ((IEnumerable<CustomItem>)val).Select(i => i.ID.ToString()));
                                }
                            }
                        }
                    }
                    else if (propertyInfo != null)
                    {
                        propertyValue = propertyInfo.GetValue(this.ContentItem);
                    }
                    if (propertyValue != null)
                    {
                        var valueArray = dataItemPropertyValue.Split(new string[] { "|" }, StringSplitOptions.None);
                        if (valueArray.Any() && IsVisible(valueArray, propertyValue, printContext))
                        {
                            this.RenderChildren(printContext, output);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, this);
            }
            return;
        }

        /// <summary>
        /// Determines whether the specified value array is visible.
        /// </summary>
        /// <param name="valueArray">The value array.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns></returns>
        protected virtual bool IsVisible(string[] valueArray, object propertyValue, PrintContext printContext)
        {
            var result = IsValidCondition(valueArray, propertyValue, printContext);
            if (!string.IsNullOrEmpty(this.RenderingKey))
            {
                RenderingReference rendering = (RenderingReference)printContext.Settings.Parameters[this.RenderingKey];
                if (rendering != null && !string.IsNullOrEmpty(AcceptableRenderings))
                {
                    var renderrings = HttpUtility.UrlDecode(AcceptableRenderings).Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (renderrings != null && renderrings.Any()) {
                        if (renderrings.Contains(rendering.RenderingID.ToString()))
                        {
                            result = IsValidCondition(valueArray, propertyValue, printContext);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether [is valid condition] [the specified value array].
        /// </summary>
        /// <param name="valueArray">The value array.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns></returns>
        protected virtual bool IsValidCondition(string[] valueArray, object propertyValue, PrintContext printContext)
        {
            var result = false;
            foreach (var val in valueArray)
            {
                if (val.Contains("!"))
                {
                    var c = val.Replace("!", string.Empty);
                    if (c != propertyValue.ToString())
                    {
                        result = true;
                    }                 
                }
                else
                {
                    if (val == propertyValue.ToString())
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
