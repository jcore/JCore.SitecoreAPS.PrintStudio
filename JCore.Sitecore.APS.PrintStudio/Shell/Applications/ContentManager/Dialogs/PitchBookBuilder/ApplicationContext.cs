using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Install.Files;
using Sitecore.IO;
using Sitecore.Shell.Applications.Install.Controls;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;

namespace JCore.SitecoreAPS.PrintStudio.Shell.Applications.ContentManager.Dialogs.PitchBookBuilder
{
    public class ApplicationContext
    {
        /// <summary>
        /// Attaches the document.
        /// </summary>
        /// <param name="document">The document.</param>
        public static void AttachDocument(object document)
        {
            DocumentHolder documentHolder = new DocumentHolder();
            documentHolder.ID = "DocumentHolder";
            documentHolder.Document = document;
            Context.ClientPage.Controls.Add((Control)documentHolder);
        }

        /// <summary>
        /// Gets the document holder.
        /// </summary>
        /// <value>
        /// The document holder.
        /// </value>
        public static DocumentHolder DocumentHolder
        {
            get
            {
                return Context.ClientPage.FindControl("DocumentHolder") as DocumentHolder;
            }
        }

        /// <summary>
        /// Stores the object.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        //public static string StoreObject(object instance)
        //{
        //    string key = ID.NewID.ToString();
        //    //string str = IOUtils.StoreObject(instance);
        //    WebUtil.SetSessionValue(key, (object)str);
        //    return key;
        //}

        public static string GetFullPublishingPath(string filename)
        {
            return ApplicationContext.GetFullPath(BookBuilderConstants.PublishingFolder, filename);
        }

        /// <summary>
        /// Gets the full package path.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static string GetFullPackagePath(string filename)
        {
            return ApplicationContext.GetFullPath(ApplicationContext.DownloadPath, filename);
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        private static string GetFullPath(string folder, string filename)
        {
            if (filename == null || filename.Length == 0)
                return string.Empty;
            filename = FileUtil.GetFileName(filename);
            return Path.Combine(folder, filename);
        }

        /// <summary>
        /// Gets the package path.
        /// </summary>
        /// <value>
        /// The package path.
        /// </value>
        public static string DownloadPath
        {
            get
            {
                return PathUtils.MapPath(Path.Combine(Settings.DataFolder,"eBooks"));
            }
        }

        public static string GetFilename(string filename)
        {
            Error.AssertString(filename, "filename", true);
            string str = filename;
            if (!str.Contains(".pdf"))
            {
                if (string.IsNullOrEmpty(FileUtil.GetExtension(str)))
                {
                    str = string.Concat(str, ".pdf");
                }
                else
                {
                    str = string.Concat(str.Replace(FileUtil.GetExtension(str),string.Empty), ".pdf");
                }
            }
            return str;
        }
    }
}
