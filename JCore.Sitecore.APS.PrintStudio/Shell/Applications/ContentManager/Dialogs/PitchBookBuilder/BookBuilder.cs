using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Weil.SC.Common.Utils;
using JCore.SitecoreAPS.PrintStudio.Managers;

namespace JCore.SitecoreAPS.PrintStudio.Shell.Applications.ContentManager.Dialogs.PitchBookBuilder
{
    public class BookBuilder
    {
        private static Database database = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
        /// <summary>
        /// Generates the report.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="name">The name.</param>
        /// <param name="bookFile">The book file.</param>
        /// <param name="items">The items.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static void GenerateReport(Item rootItem, string reportType, string bookFile, List<ID> items, string projectId, string username)
        {
            Error.AssertObject(rootItem, "rootItem");
            if (reportType == "eBook")
            {
                try
                {
                    List<string> biographies = new List<string>();
                    List<string> articles = new List<string>();
                    foreach (ID id in items)
                    {
                        var itm = database.GetItem(id);
                        if (itm.DoesItemImplementTemplate(BookBuilderConstants.BiographyBaseTemplateID))
                        {
                            biographies.Add(id.ToString());
                        }
                        else if (itm.DoesItemImplementTemplate(BookBuilderConstants.ArticleBaseTemplateID))
                        {
                            articles.Add(id.ToString());
                        }
                    }
                    var biographiesString = string.Join("|", biographies);
                    var articlesString = string.Join("|", articles);

                    bookFile = PrintManager.PrintPitchBook(projectId, biographiesString, articlesString, null, null, username, bookFile, BookBuilderConstants.PublishingFolder);

                    File.Copy(ApplicationContext.GetFullPublishingPath(bookFile), ApplicationContext.GetFullPackagePath(bookFile));
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
