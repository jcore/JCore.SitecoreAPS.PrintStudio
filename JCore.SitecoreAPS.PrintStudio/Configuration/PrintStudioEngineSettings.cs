using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace JCore.SitecoreAPS.PrintStudio.Configuration
{
    public class PrintStudioEngineSettings
    {
        private static Database Database
        {
            get
            {
                var db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;
                if (db == null)
                {
                    db = Factory.GetDatabase("web");
                }
                return db;
            }
        }
        public static string EngineTemplates
        {
            get
            {
                Item obj = Database.GetItem("{9D7762D9-7AC1-4CFF-A26C-6FA1E403B39F}");
                if (obj == null)
                    return string.Empty;
                else
                    return obj.Paths.FullPath + "/";
            }
        }
    }
}
