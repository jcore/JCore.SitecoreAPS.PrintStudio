using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;

namespace JCore.SitecoreAPS.PrintStudio.Shell.Applications.ContentManager.Dialogs.PitchBookBuilder
{
    public static class BookBuilderConstants
    {
        public static ID BiographyBaseTemplateID = ID.Parse("{8C5E12EF-E4C5-4573-A25E-CF1D1E39D39F}");
        public static ID ArticleBaseTemplateID = ID.Parse("{C0F2C7A8-08B5-4F9C-9CDA-4620F64B29C4}");
        public static ID RootOfApsProjects = ID.Parse("{F1B4E272-43D4-4564-853B-699D77D75419}");
        public static string PublishingFolder = @"\\JCSHP10DEV14\Public\APSPublishing\PublishFolder\";
    }
}
