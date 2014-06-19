using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
    /// <summary>
    /// Defines repeater for project items.
    /// </summary>
    public class XmlRepeater : InDesignItemRendererBase
    {
        /// <summary>
        /// Gets or sets the data sources.
        /// </summary>
        /// <value>
        /// The data sources.
        /// </value>
        public string DataSources { get; set; }

        /// <summary>
        /// Gets or sets the repeat count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public string Count { get; set; }

        /// <summary>
        /// Gets or sets the name of the child data key.
        /// </summary>
        /// <value>
        /// The name of the child data key.
        /// </value>
        public string ChildDataKeyName { get; set; }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public string Column { get; set; }

        /// <summary>
        /// Preliminary render action invoked before RenderContent <see cref="RenderContent"/>.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            if (!string.IsNullOrEmpty(this.RenderingItem["Item Reference"]))
            {
                this.DataSource = this.RenderingItem["Item Reference"];
            }

            // Check if Data Key field contains a value. 
            // If it does and parameters collection contain Data Key value,
            // assign datasources from parameters collection.
            if (!string.IsNullOrEmpty(this.RenderingItem["Data Key"]) && printContext.Settings.Parameters.ContainsKey(this.RenderingItem["Data Key"]))
            {
                var data = printContext.Settings.Parameters[this.RenderingItem["Data Key"]].ToString();
                if (!string.IsNullOrEmpty(data))
                {
                    var items = StringUtil.Split(data, '|', true);
                    if (items.Count() > 1)
                    {
                        this.DataSources = data;
                        if (string.IsNullOrEmpty(this.Count))
                        {
                            this.Count = items != null ? items.Count().ToString() : "0";
                        }
                        return;
                    }

                    var contextItem = printContext.Database.GetItem(data);
                    if (contextItem != null)
                    {
                        this.DataSource = contextItem.ID.ToString();
                    }                    
                }
            }

            // If there is not Data Key value, try resolving datasource from Item Reference
            var dataItem = this.GetDataItem(printContext);
            if (dataItem != null)
            {
                // Check if Item Selector contains any value. If it does, apply the selector to the data item
                if (!string.IsNullOrEmpty(this.RenderingItem["Item Selector"]))
                {
                    var xpath = this.RenderingItem["Item Selector"];
                    if (!string.IsNullOrEmpty(xpath))
                    {
                        var items = dataItem.Axes.SelectItems(xpath);
                        if (items != null)
                        {
                            // Assign item collection to datasources 
                            this.DataSources = string.Join("|", items.Select(t => t.ID.ToString()).ToArray());
                            if (string.IsNullOrEmpty(this.Count))
                            {
                                this.Count = items != null ? items.Count().ToString() : "0";
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(this.RenderingItem["Item Field"]))
                {
                    // If Item Field is not empty, check if its a multilist field.                    
                    var datasources = dataItem[this.RenderingItem["Item Field"]];
                    var datasourceArray = datasources != null ? datasources.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) : null;

                    // Get the number of times we need to repeat the child elements
                    if (string.IsNullOrEmpty(this.Count))
                    {
                        this.Count = datasourceArray != null ? datasourceArray.Count().ToString() : "0";
                    }
                    if (!string.IsNullOrEmpty(this.Column))
                    {
                        this.DataSources = GetColumnItems(datasourceArray, this.Column);
                    }
                    else
                    {
                        this.DataSources = datasources;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the left items.
        /// </summary>
        /// <param name="datasources">The datasources.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        private string GetColumnItems(string[] datasources, string column)
        {
            var leftArray = new List<string>();
            var rightArray = new List<string>();
            for (var i = 0; i < datasources.Length; i++)
            {
                if (i % 2 == 0)
                {
                    leftArray.Add(datasources[i]);
                }
                else
                {
                    rightArray.Add(datasources[i]);
                }
            }
            if (column == "Left")
            {
                return string.Join("|", leftArray);
            }
            else if (column == "Right")
            {
                return string.Join("|", rightArray);
            }
            else
            {
                return string.Join("|", datasources);
            }
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, System.Xml.Linq.XElement output)
        {
            if (!string.IsNullOrEmpty(this.ChildDataKeyName))
            {
                printContext.Settings.Parameters[this.ChildDataKeyName] = this.DataSource;
            }
            int count;
            if (!string.IsNullOrEmpty(this.Count) && int.TryParse(this.Count, out count) && count > 0)
            {
                if (!string.IsNullOrEmpty(this.DataSources))
                {
                    var dataSources = this.DataSources.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (count > dataSources.Length)
                    {
                        count = dataSources.Length;
                    }
                    foreach (var dataSource in dataSources.Take(count))
                    {
                        this.DataSource = dataSource;
                        if (!string.IsNullOrEmpty(this.ChildDataKeyName))
                        {
                            printContext.Settings.Parameters[this.ChildDataKeyName] = dataSource;
                        }

                        RenderChildren(printContext, output);
                    }

                    return;
                }
                else
                {
                    count = 0;
                }

                var dataItem = this.GetDataItem(printContext);
                if (dataItem == null && !printContext.Settings.IsClient)
                {
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    // Render child elements
                    this.RenderChildren(printContext, output);
                }

                return;
            }
        }
    }
}
