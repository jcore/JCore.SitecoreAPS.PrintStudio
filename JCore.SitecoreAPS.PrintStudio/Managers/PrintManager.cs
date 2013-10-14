using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Sitecore.Collections;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.InDesign.Soap;
using Sitecore.PrintStudio.Configuration;
using Sitecore.PrintStudio.WebServices.Providers;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore.Analytics.Data;
using Sitecore.Analytics.Data.DataAccess.DataSets;

namespace JCore.SitecoreAPS.PrintStudio.Managers
{
    /// <summary>
    /// Adobe InDesign print manager.
    /// </summary>
    public static class PrintManager
    {
        #region Fields
        private const string resultFolder = "";
        const string defaultProjectId = "{F9ECC877-3F91-457A-B94C-4C4695688E32}";
        private static readonly Database DB = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
        private const string fileNameFormat = "{0}_{1}_{2}_{4}{3}";
        private static string PublisherFolder = @"\\JCSHP10DEV14\Public\APSPublishing\PublishFolder\"; 
        #endregion

        #region Print Goal
        /// <summary>
        /// Triggers the goal like Brochure Request. This method is to be used for PDF prochure printing. 
        /// Actions should be configured in Sitecore. Associated RuleActions should be created in Rules folder of this project.
        /// </summary>
        /// <param name="goalName">Name of the goal.</param>
        /// <returns></returns>
        public static string PrintGoal(string goalName)
        {
            PageEventData eventData = new PageEventData(goalName);
            VisitorDataSet.PageEventsRow pageEventsRow = Sitecore.Analytics.Tracker.CurrentPage.Register(eventData);
            Sitecore.Analytics.Tracker.Submit();
            return goalName;
        }
        
        #endregion

        #region Print Biography
        /// <summary>
        /// Prints a biography pdf for specified attorney and based on passed project id. If project ID is not passed in it takes default id from const above.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public static string PrintBiography(string id, string projectId = null)
        {
            var manager = new Sitecore.PrintStudio.PublishingEngine.PrintManager(DB, Sitecore.Context.Language);
            var printOptions = GetPrintOptions(id, projectId);
            return manager.Print(projectId, printOptions);
        }

        /// <summary>
        /// Async prints a biography pdf for specified attorney and based on passed project id. If project ID is not passed in it takes default id from const above.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static string PrintBiographyAsync(string id, string projectId = null)
        {
            var manager = new Sitecore.PrintStudio.PublishingEngine.PrintManager(DB, Sitecore.Context.Language);
            var printOptions = GetPrintOptions(id, projectId);
            manager.PrintAsync(projectId, printOptions);
            return printOptions.ResultFolder + printOptions.ResultFileName;
        } 
        #endregion

        #region Print Pitch Book
        /// <summary>
        /// Prints the pitch book.
        /// </summary>
        /// <param name="biographyIds">The biographyIds.</param>
        /// <param name="projectId">The project id.</param>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public static string PrintPitchBook(string projectId, string biographyIds = null, string articleIds = null, string serviceIds = null, string relatedContentIds = null, string username = null, string resultFileName = null, string publishingFolder = null)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                projectId = defaultProjectId;
            }

            var db = Sitecore.Context.ContentDatabase ??
                     Sitecore.Context.Database;

            var manager = new Sitecore.PrintStudio.PublishingEngine.PrintManager(db, Sitecore.Context.Language);

            var parameters = new SafeDictionary<string, object>();

            if (!string.IsNullOrEmpty(biographyIds))
            {
                parameters.Add("BiographyIds", biographyIds);
            }

            if (!string.IsNullOrEmpty(username))
            {
                parameters.Add("Weil_Username", username);
            }

            if (!string.IsNullOrEmpty(articleIds))
            {
                parameters.Add("ArticleIds", articleIds);
            }

            if (!string.IsNullOrEmpty(serviceIds))
            {
                parameters.Add("ServiceIds", serviceIds);
            }

            if (!string.IsNullOrEmpty(relatedContentIds))
            {
                parameters.Add("relatedContentIds", relatedContentIds);
            }

            var printOptions = new PrintOptions
            {
                PrintExportType = PrintExportType.Pdf,
                UseHighRes = true,
                Parameters = parameters,
                ResultFolder = publishingFolder ?? PublisherFolder
            };
            if (string.IsNullOrEmpty(resultFileName))
            {
                printOptions.ResultFileName = String.Format(fileNameFormat, "PitchBook", db.GetItem(projectId).Name.Replace(" ", "_"), DateTime.Now.Ticks, printOptions.ResultExtension, username);
            }
            else
            {
                printOptions.ResultFileName = resultFileName;
            }
            return manager.Print(projectId, printOptions);
        }
        
        #endregion

        #region Private
        /// <summary>
        /// Gets the print options.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        private static PrintOptions GetPrintOptions(string id, string projectId = null)
        {
            Assert.ArgumentNotNullOrEmpty(id, "id");
            if (string.IsNullOrEmpty(projectId))
            {
                projectId = defaultProjectId;
            }

            var item = DB.GetItem(id);
            var fileNameFormat = "{0}_{1}_{2}{3}";

            var parameters = new SafeDictionary<string, object> { { "ContentItem", id } };

            var printOptions = new PrintOptions
            {
                PrintExportType = PrintExportType.Pdf,
                UseHighRes = true,
                Parameters = parameters,
                ResultFolder = @"\\\\JCSHP10DEV14\\Public\\APSPublishing\\PublishFolder\\"
            };
            if (item != null)
            {
                printOptions.ResultFileName = String.Format(fileNameFormat, item.Name.Replace(" ", "_"), DB.GetItem(projectId).Name, DateTime.Now.Ticks, printOptions.ResultExtension);
            }
            return printOptions;
        } 
        #endregion
    }
}
