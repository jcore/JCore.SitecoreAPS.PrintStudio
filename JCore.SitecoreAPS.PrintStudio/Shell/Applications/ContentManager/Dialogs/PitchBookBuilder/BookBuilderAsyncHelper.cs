using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Jobs.AsyncUI;

namespace JCore.SitecoreAPS.PrintStudio.Shell.Applications.ContentManager.Dialogs.PitchBookBuilder
{
    public class BookBuilderAsyncHelper
    {
         // Fields
        private Item item;
        private string bookFile;
        private List<ID> items;
        private string username;
        private string projectId;

        // Methods
        public BookBuilderAsyncHelper(Item item, string reportFile, List<ID> items, string projectId, string username)
        {
            this.item = item;
            this.bookFile = reportFile;
            this.items = items;
            this.username = username;
            this.projectId = projectId;
        }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        public void Generate()
        {
            try
            {
                BookBuilder.GenerateReport(this.item, "eBook", this.bookFile, this.items, this.projectId, this.username);
            }
            catch (Exception exception)
            {
                Log.Error("Book generation failed: " + exception.ToString(), this);
                JobContext.SendMessage("reportbuild:failed(message=" + exception.Message + ")");
                JobContext.Flush();
                throw exception;
            }
        }
    }
}
