using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Proxies;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Install;
using Sitecore.Install.Framework;
using Sitecore.Install.Items;
using Sitecore.Jobs.AsyncUI;
using Sitecore.Shell.Applications.Install.Controls;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;

namespace JCore.SitecoreAPS.PrintStudio.Shell.Applications.ContentManager.Dialogs.PitchBookBuilder
{
    public class PitchBookBuilderDialog : WizardForm
    {
        /// <summary/>
        protected DataContext DataContext;
        /// </summary>
        protected TreeviewEx Treeview;
        /// <summary/>
        protected Listview ItemList;
        /// <summary/>
        protected Edit BookFile;        
        /// <summary/>
        protected JobMonitor Monitor;
        /// <summary/>
        protected Border SuccessMessage;
        /// <summary/>
        protected Border FailureMessage;
        /// </summary>
        protected Edit ClientName;
        /// </summary>
        protected Combobox ApsProjects;
        /// </summary>
        private string BookFileName;

        /// <summary>
        /// Gets or sets the result file.
        /// </summary>
        /// <value>
        /// The result file.
        /// </value>
        public string ResultFile
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["ResultFile"]);
            }
            set
            {
                Context.ClientPage.ServerProperties["ResultFile"] = (object)value;
            }
        }

        /// <summary>
        /// Gets or sets the project unique identifier.
        /// </summary>
        /// <value>
        /// The project unique identifier.
        /// </value>
        public string ProjectId
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["ProjectId"]);
            }
            set
            {
                Context.ClientPage.ServerProperties["ProjectId"] = (object)value;
            }
        }

        
        /// <summary>
        /// Raises the load event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>
        /// This method notifies the server control that it should perform actions common to each HTTP
        /// request for the page it is associated with, such as setting up a database query. At this
        /// stage in the page lifecycle, server controls in the hierarchy are created and initialized,
        /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
        /// property to determine whether the page is being loaded in response to a client postback,
        /// or if it is being loaded and accessed for the first time.
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                this.DataContext.GetFromQueryString();
                Item folder = this.DataContext.GetFolder();
                this.BuildProjectList();
        
                if (this.Monitor == null)
                {
                    this.Monitor = new JobMonitor();
                    this.Monitor.ID = "Monitor";
                    Context.ClientPage.Controls.Add((System.Web.UI.Control)this.Monitor);
                }
            }
            else if (this.Monitor == null)
            {
                this.Monitor = Context.ClientPage.FindControl("Monitor") as JobMonitor;
            }
            this.Monitor.JobFinished += new EventHandler(this.Monitor_Finished);
            this.Monitor.JobDisappeared += new EventHandler(this.Monitor_Finished);
        }

        /// <summary>
        /// Handles the Finished event of the Monitor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Monitor_Finished(object sender, EventArgs e)
        {
            base.Next();
        }

        /// <summary>
        /// Changes the project.
        /// </summary>
        public void ChangeProject()
        {
            this.ProjectId = this.ApsProjects.Value ?? this.ApsProjects.Items[0].Value;
        }
        /// <summary>
        /// Builds the project list.
        /// </summary>
        /// <param name="selectedName">Name of the selected.</param>
        private void BuildProjectList()
        {
            var db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;

            ListItem listItemSelect = new ListItem();
            this.ApsProjects.Controls.Add((System.Web.UI.Control)listItemSelect);
            listItemSelect.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("ListItem");
            listItemSelect.Header = "Select a Project";
            listItemSelect.Value = string.Empty;
            listItemSelect.Selected = true;

            var projects = db.GetItem(BookBuilderConstants.RootOfApsProjects).GetChildren();
            for (var i = 0; i < projects.Count; i++ )
            {
                var item = projects[i];
                ListItem listItem = new ListItem();
                this.ApsProjects.Controls.Add((System.Web.UI.Control)listItem);
                listItem.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("ListItem");
                listItem.Header = item.Name;
                listItem.Value = item.ID.ToString();
            }
        }

        /// <summary>
        /// Called when the active page has been changed.
        /// </summary>
        /// <param name="page">The page that has been entered.</param>
        /// <param name="oldPage">The page that was left.</param>
        /// <contract>
        ///   <requires name="page" condition="not null" />
        ///   <requires name="oldPage" condition="not null" />
        ///   </contract>
        protected override void ActivePageChanged(string page, string oldPage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(oldPage, "oldPage");
            base.ActivePageChanged(page, oldPage);
            if (page == "Building")
            {
                base.NextButton.Disabled = true;
                base.BackButton.Disabled = true;
                base.CancelButton.Disabled = true;
                SheerResponse.SetInnerHtml("StatusText", Translate.Text("Building book..."));
                Context.ClientPage.SendMessage(this, "bookbuilder:generate");
            }
            if (page == "LastPage")
            {
                base.BackButton.Disabled = true;
                base.NextButton.Disabled = true;
            }

        }
        /// <summary/>
        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            if (page == "SetName")
            {
                if (this.BookFile.Value.Trim().Length != 0)
                {
                    if (this.BookFile.Value.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                    {
                        string fullPackagePath;
                        try
                        {
                            fullPackagePath = ApplicationContext.GetFullPublishingPath(ApplicationContext.GetFilename(this.BookFile.Value.Trim()));
                            Path.GetDirectoryName(fullPackagePath);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Noncritical: " + ((object)ex).ToString(), (object)this);
                            Context.ClientPage.ClientResponse.Alert(Translate.Text("Entered name could not be resolved into an absolute file path.") + Environment.NewLine + Translate.Text("Enter a valid name for the book."));
                            Context.ClientPage.ClientResponse.Focus(this.BookFile.ID);
                            return false;
                        }
                        this.BookFileName = ApplicationContext.GetFilename(this.BookFile.Value.Trim());
                        if (File.Exists(fullPackagePath) && !MainUtil.GetBool(Context.ClientPage.ServerProperties["__NameConfirmed"], false))
                        {
                            Context.ClientPage.Start((object)this, "AskOverwrite");
                            return false;
                        }
                        else
                        {
                            Context.ClientPage.ServerProperties.Remove("__NameConfirmed");
                            return base.ActivePageChanging(page, ref newpage);
                        }
                    }
                }
                Context.ClientPage.ClientResponse.Alert(Translate.Text("Enter a valid name for the book."));
                Context.ClientPage.ClientResponse.Focus(this.BookFile.ID);
                return false;
            }
            return base.ActivePageChanging(page, ref newpage);
        }

        /// <summary>
        /// Adds the item.
        /// 
        /// </summary>
        public void AddItem()
        {
            this.AddEntry(this.DataContext.GetFolder(), "software/16x16/element.png");
        }
        
        /// <summary>
        /// Lists the context menu.
        /// 
        /// </summary>
        public void ListContextMenu()
        {
            if (!(Context.ClientPage.FindControl(Context.ClientPage.ClientRequest.Source) is ListviewItem))
                return;
            Context.ClientPage.ClientResponse.DisableOutput();
            Menu menu = new Menu();
            MenuItem menuItem = new MenuItem();
            Context.ClientPage.AddControl((System.Web.UI.Control)menu, (System.Web.UI.Control)menuItem);
            menuItem.Header = "Remove";
            menuItem.Icon = "applications/16x16/delete2.png";
            menuItem.Click = "Remove(\"" + Context.ClientPage.ClientRequest.Source + "\")";
            Context.ClientPage.ClientResponse.EnableOutput();
            Context.ClientPage.ClientResponse.ShowContextMenu(string.Empty, "right", (System.Web.UI.Control)menu);
        }

        /// <summary>
        /// Removes the control that has specified id.
        /// 
        /// </summary>
        /// <param name="id">The id.</param>
        public void Remove(string id)
        {
            ListviewItem[] listviewItemArray;
            if (id.Length == 0)
                listviewItemArray = this.ItemList.SelectedItems;
            else
                listviewItemArray = new ListviewItem[1]
        {
          Context.ClientPage.FindControl(id) as ListviewItem
        };
            foreach (ListviewItem listviewItem in listviewItemArray)
            {
                if (listviewItem != null)
                {
                    listviewItem.Parent.Controls.Remove((System.Web.UI.Control)listviewItem);
                    Context.ClientPage.ClientResponse.Remove(listviewItem.ID, true);
                }
            }
        }

        /// <summary>
        /// Adds the entry.
        /// </summary>
        /// <param name="Item">The item.</param>
        /// <param name="type">The type.</param>
        /// <param name="icon">The icon.</param>
        private void AddEntry(Item Item, string icon)
        {
            Context.ClientPage.ClientResponse.DisableOutput();
            try
            {
                ListviewItem listviewItem = new ListviewItem();
                listviewItem.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("ListItem");
                Context.ClientPage.AddControl((System.Web.UI.Control)this.ItemList, (System.Web.UI.Control)listviewItem);
                string str = ((object)Item.Uri).ToString();
                listviewItem.Icon = icon;
                listviewItem.Header = string.Format("{0}", (object)Item.Paths.Path);
                listviewItem.Value = string.Format("{0}", (object)str);
            }
            finally
            {
                Context.ClientPage.ClientResponse.EnableOutput();
            }
            Context.ClientPage.ClientResponse.Refresh((System.Web.UI.Control)this.ItemList);
        }

        /// <summary>
        /// Shows this dialog.
        /// 
        /// </summary>
        public static void Show()
        {
            Context.ClientPage.ClientResponse.ShowModalDialog(UIUtil.GetUri("control:PitchBookBuilder"), true);
        }


        /// <summary>
        /// Starts the report builder.
        /// </summary>
        /// <param name="message">The message.</param>
        [HandleMessage("bookbuilder:generate")]
        protected void StartBookBuilder(Message message)
        {
            Assert.ArgumentNotNullOrEmpty("ProjectId", this.ProjectId);
            Item item2 = Context.ContentDatabase.Items[this.ProjectId];
            Error.Assert(item2 != null, "Item not found");
            this.StartTask(item2);
        }
        /// <summary>
        /// Starts the task.
        /// </summary>
        /// <param name="solutionFile">The solution file.</param>
        /// <param name="packageFile">The package file.</param>
        private void StartTask(Item item)
        {
            Sitecore.IO.FileUtil.EnsureFileFolder(ApplicationContext.GetFullPublishingPath(this.BookFileName));
            Sitecore.IO.FileUtil.EnsureFileFolder(ApplicationContext.GetFullPackagePath(this.BookFileName));
            this.ResultFile = ApplicationContext.GetFullPackagePath(this.BookFileName);
            this.Monitor.Start("BuildBook", "BookBuilder", new ThreadStart(new BookBuilderAsyncHelper(item, this.BookFileName, GetSelectedItems(), this.ProjectId, this.ClientName.Value).Generate));
        }

        /// <summary>
        /// Gets the selected items.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private List<ID> GetSelectedItems()
        {
            var ids = new List<ID>();
            foreach (Sitecore.Web.UI.HtmlControls.Control control in this.ItemList.Items)
            {
                ItemUri uri = ItemUri.Parse(control.Value);
                if (!ID.IsNullOrEmpty(uri.ItemID))
                {
                    ids.Add(uri.ItemID);
                }
            }
            return ids;
        }

        /// <summary>
        /// Called when [build failed].
        /// </summary>
        /// <param name="message">The message.</param>
        [HandleMessage("bookbuilder:failed")]
        protected void OnBuildFailed(Message message)
        {
            string str = StringUtil.GetString(new string[] { message["message"] });
            Context.ClientPage.ClientResponse.SetStyle("SuccessMessage", "display", "none");
            Context.ClientPage.ClientResponse.SetStyle("FailureMessage", "display", "");
            Context.ClientPage.ClientResponse.SetInnerHtml("FailureMessage", Translate.Text("Report generation failed: {0}.", new object[] { str }));
        }
        /// <summary>
        /// Downloads the report.
        /// </summary>
        /// <param name="message">The message.</param>
        [HandleMessage("bookbuilder:download")]
        protected void DownloadReport(Message message)
        {
            string resultFile = this.ResultFile;
            if (resultFile == null)
            {
                Context.ClientPage.ClientResponse.Alert("Please open a Log file first.");
            }
            else if (!string.IsNullOrEmpty(resultFile))
            {
                string reportBuilderRoleName = @"sitecore\Downloading";
                Sitecore.Security.Accounts.Role role = Sitecore.Security.Accounts.Role.FromName(reportBuilderRoleName);
                if (Sitecore.Context.IsAdministrator || (Sitecore.Context.User != null && Sitecore.Context.User.Roles.Contains(role)))
                {
                    Sitecore.Shell.Framework.Files.Download(resultFile);
                }
                else
                {
                    Context.ClientPage.ClientResponse.Alert("Could not download book. Insufficient permissions.");
                }
            }
        }
        [HandleMessage("bookbuilder:close")]
        protected void CloseWindow(Message message)
        {
            base.EndWizard();
        }
        /// <summary>
        /// Asks usr whether the file should be overwritten.
        /// 
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void AskOverwrite(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                Context.ClientPage.ClientResponse.Confirm(Translate.Text("File exists. Do you wish to overwrite?"));
                args.WaitForPostBack();
            }
            else
            {
                if (!args.HasResult || !(args.Result == "yes"))
                    return;
                Context.ClientPage.ClientResponse.SetDialogValue(args.Result);
                Context.ClientPage.ServerProperties["__NameConfirmed"] = (object)true;
                this.Next();
            }
        }
    }
}
