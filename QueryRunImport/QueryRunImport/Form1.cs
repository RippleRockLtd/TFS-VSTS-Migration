using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.TeamFoundation.WorkItemTracking;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Net;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.TestManagement.Client;
using System.IO;

namespace ASOS_MIG
{
    public partial class Form1 : Form
    {
        string _fallbackAssignee = "Peter Haynes";
        string _overrideAssignee = "";//normally set null or "" for normal operation
        int _offsetFromId = 0;//to handle migration from pilot set at 1000000
        string _logFile = @"C:\Migration\Logs\MigrationToolLog.txt";

        const string issue = "Issue";
        const string user_story = "User Story";
        const string product_backlog_item = "Product Backlog Item";
        const string bug = "Bug";
        const string task = "Task";
        const string feature = "Feature";
        const string epic = "Epic";
        const string impediment = "Impediment";

        List<WorkItem> _source_link_workitems = new List<WorkItem>();
        WorkItemStore _source_workItemStore = null;
        Project _source_workItemProject = null;
        WorkItemStore _target_workItemStore = null;
        Project _target_workItemProject = null;


        public Form1()
        {
            InitializeComponent();


            textBox_FallbackAsignee.Text = _fallbackAssignee;
            textBox_OverrideAll.Text = _overrideAssignee;
            textBox_OffsetFromId.Text = _offsetFromId.ToString();
            textBox_logFilePath.Text = _logFile;


        }
      

        WorkItemCollection GetWorkitemsForIDs(string projectID, string csvIDs)
        {

            string wiqlQuery = "Select ID from Workitems Where [System.TeamProject] ='" + projectID + "' AND [System.Id] IN (" + csvIDs + ")";

            // Execute the query.
            WorkItemCollection wCollection = _source_workItemStore.Query(wiqlQuery);



            return wCollection;
        }
        WorkItem CheckForExistingFromId(string projectID, int fromId)
        {

            string wiqlQuery = "Select ID from Workitems Where [System.TeamProject] ='" + projectID + "' AND [From Id] = " + (fromId + _offsetFromId);

            // Execute the query.
            WorkItemCollection witCollection = _target_workItemStore.Query(wiqlQuery);

            // Show the ID and Title of each WorkItem returned from the query.
            foreach (WorkItem workItem in witCollection)
            {
                return workItem;
            }

            return null;
        }
        Dictionary<string, int> listExistingConfigs(ITestManagementTeamProject2 pj)
        {
            logProgress(string.Format("\tTest Configs from {0}", pj.TeamProjectName));
            Dictionary<string, int> targetConfigs = new Dictionary<string, int>();
            foreach (ITestConfiguration config in pj.TestConfigurations.Query(
                "Select * from TestConfiguration"))
            {
                logProgress(string.Format("\tTest Config: {0}", config.Name));
                targetConfigs.Add(config.Name, config.Id);

            }

            return targetConfigs;
        }
        void listExistingPlans(ITestManagementTeamProject2 pj)
        {
            foreach (ITestPlan2 tp in pj.TestPlans.Query("Select * From TestPlan"))
            {
                logProgress(string.Format("Plan - {0} : {1}", tp.Id, tp.Name));
                foreach (ITestSuiteBase2 suite in tp.RootSuite.SubSuites)
                {
                    logProgress(string.Format("\tTest Suite: {0}", suite.Title));

                    IStaticTestSuite2 staticSuite = suite as IStaticTestSuite2;

                    // Let's Query the Test Points 
                    //A Test Point represents a Test Case to be run against a Test Configuration in a Test Suite.
                    //In MTLM, if you go to the Testing Center –> Plan tab –> Contents, you see the list of Test Case’s in each suite.  
                    //One of the columns in the grid is “Configurations” which shows a count of how many Test Configurations that 
                    //Test Case is tested against in that suite.  
                    //When you go to Testing Center –> Test tab –> Run Tests and click on the same suite you will see 
                    //an entry in the grid there for each Test Case / Test Configuration combination.  These are Test Points.

                    foreach (ITestPoint point in tp.QueryTestPoints(
                        string.Format("SELECT * FROM TestPoint WHERE SuiteId = {0}",
                        suite.Id)))
                    {
                        logProgress(string.Format("\t\tTest Point for Testcase \"{0}\" against config \"{1}\"",
                            point.TestCaseWorkItem.Title, point.ConfigurationName));
                    }
                }
            }
        }
        string replaceRootInPath(string rootSource, string rootTarget, string path)
        {
            if (path.Contains("\\"))
            {
                return (path.Replace(rootSource + "\\", rootTarget + "\\"));
            }
            else
            {
                return (path.Replace(rootSource, rootTarget));
            }
        }
        

        void duplicateOtherLinks(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {
            //does the source have links? If there are none no need to do anything.
            if ((source_workItem.Links == null || source_workItem.Links.Count == 0))
            {
                logProgress("No links on source workitem.");

                return;
            }

   

            Microsoft.TeamFoundation.WorkItemTracking.Client.LinkCollection source_wi_links = source_workItem.Links;
            foreach (Link s_link in source_wi_links)//loop through all links on source
            {
                if (s_link.BaseType != BaseLinkType.RelatedLink) { continue; }//only do related links. if there are no elated liks nothing will be created on the target




                RelatedLink source_link = (RelatedLink)s_link;
                WorkItem source_link_workItem = _source_workItemStore.GetWorkItem(source_link.RelatedWorkItemId);//get workitem from source
                if (source_link_workItem.Type.Name.Contains("Test")) { continue; }


                //keep a list of sources that we have links to
                bool doadd = true;
                foreach (WorkItem wi in _source_link_workitems)
                {
                    if (wi.Id == source_link_workItem.Id)
                    {
                        doadd = false;
                        break;
                    }
                }
                if (doadd)
                {
                    _source_link_workitems.Add(source_link_workItem);
                }
                

                //if the target wi does not yet exist create it on target if requied
                WorkItem target_link_workitem = createTargetWIfromSourceWI(source_project, source_link_workItem, target_project);

                if(target_link_workitem.Id == 0)
                {
                    return;
                }
                //is link already there?
                bool createLink = true;


                if (source_link.LinkTypeEnd.Name == "Parent" || source_link.LinkTypeEnd.Name == "Acceptance Test")
                {
                    //add if does not already exist
                    foreach (RelatedLink xt in target_link_workitem.Links)
                    {
                        if (xt.RelatedWorkItemId == target_workItem.Id)
                        {
                            logProgress("Related Link (" + source_link.LinkTypeEnd.Name + ") already exists from target Id : " + target_link_workitem.Id + " to " + " target Id : " + target_workItem.Id);

                            createLink = false;
                            break;
                        }
                    }
                    if (createLink == true)
                    {
                        logProgress("----> Creating Related Link (" + source_link.LinkTypeEnd.Name + ") from target Id : " + target_link_workitem.Id + " to " + " target Id : " + target_workItem.Id);

                        WorkItemLinkTypeEnd lte = null;
                        try { lte = _target_workItemStore.WorkItemLinkTypes.LinkTypeEnds[source_link.LinkTypeEnd.OppositeEnd.Name]; }
                        catch { }
                        if (lte != null)
                        {
                            target_link_workitem.Links.Add(new RelatedLink(lte, target_workItem.Id));
                            target_link_workitem.Save();
                        }


                    }
                }
                else
                {
                    //add if does not already exist
                    foreach (RelatedLink xt in target_workItem.Links)
                    {
                        if (xt.RelatedWorkItemId == target_link_workitem.Id)
                        {

                            logProgress("Related Link (" + source_link.LinkTypeEnd.Name + ") already exists from target Id : " + target_link_workitem.Id + " to " + " target Id : " + target_workItem.Id);

                            createLink = false;
                            break;
                        }
                    }

                    if (createLink == true)
                    {
                        logProgress("----> Creating Related Link (" + source_link.LinkTypeEnd.Name + ") from target Id : " + target_link_workitem.Id + " to " + " target Id : " + target_workItem.Id);

                        WorkItemLinkTypeEnd lte = null;
                        try { lte = _target_workItemStore.WorkItemLinkTypes.LinkTypeEnds[source_link.LinkTypeEnd.Name]; }
                        catch { }
                        if (lte != null)
                        {
                            target_workItem.Links.Add(new RelatedLink(lte, target_link_workitem.Id));
                            target_workItem.Save();
                        }
                    }
                }
            }
        }
        
        WorkItem createTargetWIfromSourceWI(Project source_project, WorkItem source_workItem, Project target_project)
        {

            WorkItem alreadyExistsAs = CheckForExistingFromId(target_project.Name, source_workItem.Id);
            if (alreadyExistsAs != null)
            {
                logProgress("Source Id : " + source_workItem.Id + " ****EXISTS on target ****" + " as Target Id : " + alreadyExistsAs.Id + " on the target TFS" + " (From Id stored as : " + alreadyExistsAs.Fields["From Id"].Value + ")");
                return alreadyExistsAs;
            }





            WorkItemType target_workItemType = null;

            if (source_workItem.Type.Name == user_story)
            {
                target_workItemType = _target_workItemProject.WorkItemTypes[product_backlog_item];
            }
            else if (source_workItem.Type.Name == issue)
            {
                target_workItemType = _target_workItemProject.WorkItemTypes[bug];
            }
            else
            {

                //use source type
                // logProgress(string.Format("No mapping set-up for {0}", source_workItem.Type.Name));
                target_workItemType = _target_workItemProject.WorkItemTypes[source_workItem.Type.Name];
            }

            //create target wi 
            WorkItem target_workItem = new WorkItem(target_workItemType)
            {

            };
            foreach (Field source_field in source_workItem.Fields)
            {
                if (target_workItem.Fields.Contains(source_field.ReferenceName)
                    && source_field.ReferenceName != "System.State"
                    && source_field.ReferenceName != "System.Reason"
                    && source_field.ReferenceName != "Microsoft.VSTS.Common.ClosedDate"

                    )
                {
                    try
                    {
                        if (source_field.ReferenceName == "System.AreaPath")
                        {
                            target_workItem.Fields[source_field.ReferenceName].Value = replaceRootInPath(source_project.Name, target_project.Name, source_field.Value.ToString());
                        }
                        else if (source_field.ReferenceName == "System.ActivatedBy")
                        {
                            //leave blank
                        }
                        else if (source_field.ReferenceName == "System.AssignedTo")
                        {
                            //leave blank
                        }
                        else if (source_field.ReferenceName == "System.IterationPath")
                        {
                            target_workItem.Fields[source_field.ReferenceName].Value = replaceRootInPath(source_project.Name, target_project.Name, source_field.Value.ToString());
                        }
                        else
                        {
                            target_workItem.Fields[source_field.ReferenceName].Value = source_field.Value;
                        }
                    }
                    catch
                    {
                      //  logProgress(string.Format("ERROR : Target cannot accept \"{0}\" in : {1}", source_field.ReferenceName, target_workItemType.Name));
                    }


                }
                else
                {
                //    logProgress(string.Format("Target has no field match (or skipping) for\"{0}\" in : {1}", source_field.ReferenceName, target_workItemType.Name));
                }
            }

          
            target_workItem.Fields["From Id"].Value = source_workItem.Id + _offsetFromId;//offset for migration from another source
            if (target_workItem.Fields.Contains("Activated By"))
            {
                target_workItem.Fields["Activated By"].Value = "";
            }
            if (_overrideAssignee != null && _overrideAssignee != "") {
                
                target_workItem.Fields["Title"].Value = source_workItem.Fields["Title"].Value.ToString()
                    .Replace("The","Le")
                    .Replace("the","le")
                    .Replace("b","y")
                    .Replace("a", "i").Replace("A", "").Replace("s", ". But given ")
                    .Replace("o", "u").Replace("B", "")
                    .Replace("d", "b").Replace("R", "")
                    .Replace("h", "g").Replace("L", "F").Replace("ee", " none ")
                    .Replace("i","e")
                    .Replace("l","w")
                    .Replace("n","k")
                    .Replace("g","l")
                    .Replace("1","3")
                    .Replace("0","8")
                    .Replace("2","7")
                    .Replace("3","5")
                    .Replace("j","q")
                    .Replace("-","");
                if (target_workItem.Fields.Contains("Closed By"))
                {
                    target_workItem.Fields["Closed By"].Value = _overrideAssignee;
                }
                if (target_workItem.Fields.Contains("Changed By"))
                {
                    target_workItem.Fields["Changed By"].Value = _overrideAssignee;
                }
                if (target_workItem.Fields.Contains("Called By"))
                {
                    target_workItem.Fields["Called By"].Value = _overrideAssignee;
                }
            }

            var invalidFields = target_workItem.Validate();
            if (invalidFields.Count > 0)
            {
                foreach (Field field in invalidFields)
                {
                    string errorMessage = string.Empty;
                    if (field.Status == FieldStatus.InvalidEmpty)
                    {
                        errorMessage = string.Format("{0} {1} {2}: TF20012: field \"{3}\" cannot be empty."
                            , field.WorkItem.State
                            , field.WorkItem.Type.Name
                            , field.WorkItem.TemporaryId
                            , field.Name);

                        logProgress(string.Format("Validation error: {0}",

                          errorMessage));
                    }
                    else if (field.Status == FieldStatus.InvalidPath)
                    {
                        errorMessage = string.Format("******Iteration:{0}\nArea:{1}\n {2}: field \"{3}\" invalid path.",
                            target_workItem.IterationPath,
                            target_workItem.AreaPath,

                           field.WorkItem.Type.Name

                            , field.Name);

                        logProgress(string.Format("Validation error: {0}",

                          errorMessage));

                    }
                    //... more handling here
                    else
                    {
                        logProgress(string.Format("Validation error: {0} : field \"{1}\"",

                          field.WorkItem.Type.Name

                           , field.Name));
                    }

                   
                }
            }
            else // Validation passed
            {
                target_workItem.Save();

                logProgress("---> Created target workitem ID : " + target_workItem.Id + " from source ID : " + source_workItem.Id + " (From Id stored as : " + target_workItem.Fields["From Id"].Value + ")");
            }
            

           
            return target_workItem;
        }
        private void logCantAssign(string s)
        {


            textBox8.Text = s + "\r\n" + textBox8.Text;


            Application.DoEvents();

        }
        public static void Log(string logMessage, TextWriter w)
        {
        
            w.WriteLine("{0} {1} : {2}", 
                DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(),
                logMessage);
            
        }
        private void updateCreatedCount(string s)
        {

            textBox_CreatedCount.Text = s;


            Application.DoEvents();

        }
        private void logProgress(string s)
        {
            if (_logFile != null && _logFile != "")
            {
                using (StreamWriter w = File.AppendText(_logFile))
                {
                    Log(s, w);

                }
            }
            textBox5.Text = s + "\r\n" + textBox5.Text;


            Application.DoEvents();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            TeamProjectPicker p = new TeamProjectPicker();
            var result = p.ShowDialog();
            if (p.SelectedTeamProjectCollection != null)
            {
                _source_workItemStore = p.SelectedTeamProjectCollection.GetService<WorkItemStore>();
                _source_workItemProject = _source_workItemStore.Projects[p.SelectedProjects[0].Name];

                Uri source_tfsUri = p.SelectedTeamProjectCollection.Uri;
                //     _source_project = GetTestProject(p.SelectedTeamProjectCollection, p.SelectedProjects[0].Name);

                textBox1.Text = p.SelectedTeamProjectCollection.Name;
                textBox2.Text = p.SelectedProjects[0].Name;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TeamProjectPicker p = new TeamProjectPicker();
            var result = p.ShowDialog();
            Uri target_tfsUri;

            if (p.SelectedTeamProjectCollection != null)
            {
                _target_workItemStore = p.SelectedTeamProjectCollection.GetService<WorkItemStore>();
                _target_workItemProject = _target_workItemStore.Projects[p.SelectedProjects[0].Name];
                target_tfsUri = p.SelectedTeamProjectCollection.Uri;
                //   _target_project = GetTestProject(p.SelectedTeamProjectCollection, p.SelectedProjects[0].Name);

                textBox3.Text = p.SelectedTeamProjectCollection.Name;
                textBox4.Text = p.SelectedProjects[0].Name;
            }

        }
        private static Guid FindQuery(QueryFolder folder, string queryName)
        {
            foreach (var item in folder)
            {
                if (item.Name.Equals(queryName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item.Id;
                }

                var itemFolder = item as QueryFolder;
                if (itemFolder != null)
                {
                    var result = FindQuery(itemFolder, queryName);
                    if (!result.Equals(Guid.Empty))
                    {
                        return result;
                    }
                }
            }
            return Guid.Empty;
        }
      
        private void button3_Click(object sender, EventArgs e)
        {

            _fallbackAssignee = textBox_FallbackAsignee.Text;
            _overrideAssignee = textBox_OverrideAll.Text;
            _offsetFromId = textBox_OffsetFromId.Text == "" ? 0 : System.Convert.ToInt32(textBox_OffsetFromId.Text);
            _logFile = textBox_logFilePath.Text;
            logProgress(string.Format(""));
            logProgress(string.Format(""));
            logProgress(string.Format("\tRippleRock workitem migration tool"));
            logProgress(string.Format(""));
            logProgress(string.Format("\t------------------------------STARTED----------------------------------------------"));
           
            logProgress("Source TFS : [" + textBox1.Text + "]");
            logProgress("Source Project: [" + textBox2.Text + "]");
            logProgress("Target TFS : [" + textBox3.Text + "]");
            logProgress("Target Project: [" + textBox4.Text + "]");

            logProgress("Attachments folder : [" + textBox7 + "]");
            logProgress("Fallback Assignee : [" + _fallbackAssignee + "]");
            logProgress("Override Assignee : [" + _overrideAssignee + "]");
            logProgress("Offset From Id : [" + _offsetFromId + "]");
            logProgress("Query : " + textBox6.Text);

            logProgress(string.Format("\t-----------------------------------------------------------------------------------"));
          

            var x = _source_workItemProject.QueryHierarchy;
            var queryId = FindQuery(x, textBox6.Text);
            var queryDefinition = _source_workItemStore.GetQueryDefinition(queryId);

            var variables = new Dictionary<string, string>() { { "project", _source_workItemProject.Name } };

            WorkItemCollection source_wi_list = null;
            if (queryDefinition.QueryType == QueryType.List)
            {

              
                source_wi_list = _source_workItemStore.Query(queryDefinition.QueryText, variables);
           

            }
            else
            {

                Query treeQuery = new Query(_source_workItemStore, queryDefinition.QueryText, variables);
               
                WorkItemLinkInfo[] workItemLinkInfoArray = null;
                workItemLinkInfoArray = treeQuery.RunLinkQuery();
                string csvIDs = "";
                string sep = "";
                foreach(WorkItemLinkInfo wli in workItemLinkInfoArray)
                {
                    csvIDs = csvIDs + sep + "'" + wli.TargetId.ToString() + "'";
                    sep = ",";
                }
                source_wi_list = GetWorkitemsForIDs(_source_workItemProject.Name, csvIDs);

            }

            List<WorkItem> full_list_source_link_workitems = new List<WorkItem>();



            textBox_InitialCount.Text = source_wi_list.Count.ToString();
            logProgress(string.Format("\tQuery workitem count = {0} ", source_wi_list.Count));
            int createdCount = 0;


            foreach (WorkItem source_wi in source_wi_list)
            {
                createdCount++;
                logProgress(string.Format("\tDone workitem count = {0} ", createdCount.ToString()));
                updateCreatedCount(createdCount.ToString());

                //exclude Test types
                if (source_wi.Type.Name.Contains("Test")) { continue; }


                full_list_source_link_workitems.Add(source_wi);

                logProgress(string.Format("\tMigrate source {0} - {1}", source_wi.Id.ToString(), source_wi.Title));

                createTargetWIfromSourceWI(_source_workItemProject, source_wi, _target_workItemProject);
            }


            logProgress(string.Format("\t------------------------------FINISHED----------------------------------------------"));

            logProgress(string.Format("\t------------------------------LINKS----------------------------------------------"));




            DateTime currentDateToCheckAgainstForOlderMigration = DateTime.Today;//If older than today we will not update it




            //This will create workitems at end of links
            _source_link_workitems = new List<WorkItem>();//store the source workitems in this list
            createdCount = 0;
            foreach (WorkItem source_wi in source_wi_list)
            {
                createdCount++;
                logProgress(string.Format("\tDone workitem count = {0} ", createdCount.ToString()));
                updateCreatedCount(createdCount.ToString());



                WorkItem target_wi = CheckForExistingFromId(_target_workItemProject.Name, source_wi.Id);
                if (target_wi != null)
                {


                    if (target_wi.CreatedDate.Date < currentDateToCheckAgainstForOlderMigration)
                    {
                        logProgress(string.Format("Not updating this target workitem Id : " + target_wi.Id +
                            "(Created Date = " + target_wi.CreatedDate.Date.ToLongDateString() + ") as it's created date is older than today's date (" + currentDateToCheckAgainstForOlderMigration.ToLongDateString() + ")."));

                        continue;
                    }


                    logProgress(string.Format("\tSetting links for source Id : {0} to target Id : {1} ({2})", source_wi.Id.ToString(), target_wi.Id.ToString(), target_wi.Title));
                    duplicateOtherLinks(_source_workItemProject, source_wi, _target_workItemProject, target_wi);
                }
            }
            logProgress(string.Format("\t------------------------------FINISHED----------------------------------------------"));


            logProgress(string.Format("\t------------------------------STATES, ASSIGNED TO AND ATTACHMENTS---------------------------------------------"));

            foreach (WorkItem wi in _source_link_workitems)
            {
                bool doadd = true;
                foreach (WorkItem source_wi in full_list_source_link_workitems)
                {
                    if (wi.Id == source_wi.Id)
                    {
                        doadd = false;
                        break;
                    }
                }
                if (doadd)
                {
                    full_list_source_link_workitems.Add(wi);
                }
            }

            textBox_IncludingAdditional.Text = full_list_source_link_workitems.Count.ToString();
            createdCount = 0;
            foreach (WorkItem source_wi in full_list_source_link_workitems)
            {
                createdCount++;
                logProgress(string.Format("\tDone workitem count = {0} ", createdCount.ToString()));
                updateCreatedCount(createdCount.ToString());


                WorkItem target_wi = CheckForExistingFromId(_target_workItemProject.Name, source_wi.Id);
                if (target_wi != null)
                {
                    if(target_wi.CreatedDate.Date < currentDateToCheckAgainstForOlderMigration)
                    {
                        logProgress(string.Format("Not updating this target workitem Id : " + target_wi.Id + 
                            "(Created Date = " + target_wi.CreatedDate.Date.ToLongDateString() + ") as it's created date is older than today's date ("+ currentDateToCheckAgainstForOlderMigration.ToLongDateString() +")."));
                   
                        continue;
                    }
                    logProgress(string.Format("\tUpdating form source Id : {0} to target Id : {1} - ({2})", source_wi.Id.ToString(), target_wi.Id.ToString(), target_wi.Title));
                    logProgress("\t---> Assigned to");
                    fixAssignedTo(_source_workItemProject, source_wi, _target_workItemProject, target_wi);
                    logProgress("\t---> Attachments");
                    fixAttachments(_source_workItemProject, source_wi, _target_workItemProject, target_wi);
                    logProgress("\t---> Story Points");
                    fixSpoints(_source_workItemProject, source_wi, _target_workItemProject, target_wi);
                    logProgress("\t---> State");
                    fixState(_source_workItemProject, source_wi, _target_workItemProject, target_wi);

                }
            }
            logProgress(string.Format("\t------------------------------ALL FINISHED----------------------------------------------"));

        }

        void fixAttachments(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {
            // Get a WebClient object to do the attachment download
            WebClient webClient = new WebClient()
            {
                UseDefaultCredentials = true
            };
            bool addedattachments = false;
            // Loop through each attachment in the work item.
            foreach (Attachment attachment in source_workItem.Attachments)
            {

                string filename = string.Format(@"{0}-{1}", source_workItem.Id, attachment.Name);
                string filenamepath = string.Format(@"{0}\\{1}", textBox7.Text, filename);
                bool exists = false;
                foreach (Attachment existing_target_attachment in target_workItem.Attachments)
                {
                    if (existing_target_attachment.Name == filename)
                    {
                        exists = true; break;
                    }
                }
                if (exists) { continue; }
                // Construct a filename for the attachment


                if (File.Exists(filenamepath))
                    File.Delete(filenamepath);


                // Download the attachment.
                webClient.DownloadFile(attachment.Uri, filenamepath);

                Attachment a = new Attachment(filenamepath);

                if (!addedattachments)
                {
                    target_workItem = openWI(target_workItem);
                }
                addedattachments = true;
                try
                {
                    target_workItem.Attachments.Add(a);
                }
                catch
                {
                    addedattachments = false;
                }

            }
            if (addedattachments)
            {
                target_workItem.Save();
            }
        }
        void fixSpoints(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {
            string f_source = "Story Points";
            string f_target = "Effort";
            try
            {
                if (source_workItem.Fields[f_source].Value != null &&
                                source_workItem.Fields[f_source].Value != target_workItem.Fields[f_target].Value)
                {

                    target_workItem = openWI(target_workItem);
                    target_workItem.Fields[f_target].Value = source_workItem.Fields[f_source].Value;
                    target_workItem.Save();


                }
            }
            catch { 
            
            }
            
        }
        void doAssign(string f,
             string v,
             Project source_project,
             WorkItem source_workItem,
             Project target_project,
             WorkItem target_workItem)
        {
            target_workItem = openWI(target_workItem);
            target_workItem.Fields[f].Value = v;
            target_workItem.Save();
            
        }
        void fixAssignedTo(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {
            string f = "System.AssignedTo";
            if (target_workItem.Fields[f].Value != null && target_workItem.Fields[f].Value.ToString() == _fallbackAssignee) { return; }

            if (source_workItem.Fields[f].Value != null && source_workItem.Fields[f].Value != target_workItem.Fields[f].Value)
            {
                try
                {
                    logProgress(string.Format("\t\tSet target {0} Assigned To {1}", target_workItem.Id, source_workItem.Fields[f].Value.ToString()));
                    doAssign(
                    f,
                    (_overrideAssignee != null && _overrideAssignee != "" ? _overrideAssignee : source_workItem.Fields[f].Value.ToString()),//first assign to the person from source
                    source_project, source_workItem,
                    target_project, target_workItem);

                }
                catch
                {
                    string fallbackAssignee = _fallbackAssignee;
                    
                    logCantAssign(string.Format("ID:{0}>>{1} {2} :used: {3}", 
                        source_workItem.Id, target_workItem.Id,  source_workItem.Fields[f].Value.ToString(), fallbackAssignee));
                    logProgress(string.Format("\t\t****Use fallback user: Set target {0} Assigned To {1}",
                        target_workItem.Id, fallbackAssignee));

                    try
                    {
                        doAssign(
                            f,
                            fallbackAssignee,//use fallback
                            source_project, source_workItem,
                            target_project, target_workItem);

                        
                    }
                    catch (Exception ex)
                    {

                        string s = ((Microsoft.TeamFoundation.Client.TfsConnection)(target_project.Store.TeamProjectCollection)).Uri + "/Backlog/_workitems?id=" + target_workItem.Id.ToString() + "&_a=edit";
                        Form2 fd = new Form2();
                        fd.ShowMyDialogBox(null, s);
                    }
                }
            }
        }

        private WorkItem openWI(WorkItem w)
        {
            return _target_workItemStore.GetWorkItem(w.Id);
        }
        void saveWI(WorkItem w, string state, string reason)
        {
            WorkItem target_workItem = openWI(w);
            if (target_workItem.State != state || target_workItem.Reason != reason)
            {
                target_workItem.State = state;
                target_workItem.Reason = reason;
               
                try { target_workItem.Save(); }
                catch (Exception ex)
                {

                    string s = ((Microsoft.TeamFoundation.Client.TfsConnection)(_target_workItemProject.Store.TeamProjectCollection)).Uri + "/Backlog/_workitems?id=" + target_workItem.Id.ToString() + "&_a=edit";
                    Form2 fd = new Form2();
                    fd.ShowMyDialogBox(ex, s);
                }
            }

        }

        bool processFromStateToState(
               WorkItem source_workItem, string source_state, string source_reason,
               WorkItem target_workItem, string target_state, string target_reason,
                       string firststatechk = null, string firstreasonchk = null, string firststate = null, string firstreason = null)
        {
            if (source_workItem.State == source_state && source_workItem.Reason == source_reason)
            {
                if (firststate != null && firstreason != null)
                {
                    processFromStateToState(target_workItem, firststatechk, firstreasonchk, target_workItem, firststate, firstreason);
                }

                if (
                        !(target_workItem.State == target_state && target_workItem.Reason == target_reason))
                {
                    logProgress(string.Format("\t\tSaving to Target {0} {1} to {2} {3}",
                                source_workItem.State, source_workItem.Reason, target_workItem.State, target_workItem.Reason));
                    saveWI(target_workItem, target_state, target_reason);
                    return true;
                }

            }


            return false;
        }

        void fixState(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {
            logProgress(string.Format("\tUpdating State for Source type : {0} to Target type {1}", source_workItem.Type.Name, target_workItem.Type.Name));

            if (target_workItem.Type.Name == bug)
            {
                fixBugState(source_project, source_workItem, target_project, target_workItem);
            }
            else if (target_workItem.Type.Name == product_backlog_item)
            {
                fixPBIState(source_project, source_workItem, target_project, target_workItem);
            }
            else if (target_workItem.Type.Name == task)
            {
                fixTaskState(source_project, source_workItem, target_project, target_workItem);
            }
            else if (target_workItem.Type.Name == feature)
            {
                fixFeatureState(source_project, source_workItem, target_project, target_workItem);
            }
            else if (target_workItem.Type.Name == epic)
            {
                fixEpicState(source_project, source_workItem, target_project, target_workItem);
            }
            else if (target_workItem.Type.Name == impediment)
            {
                fixImpedimentState(source_project, source_workItem, target_project, target_workItem);
            }
            else
            {
                logProgress(string.Format("\t\t########TYPE NOT HANDLED######## {0}",
                            target_workItem.Type.Name));
            }

        }
        void fixImpedimentState(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {


            if (processFromStateToState(source_workItem, "Closed", "Impediment removed", target_workItem, "Closed", "Impediment removed"))
            {
                return;
            }


            else
            {
                logProgress(string.Format("\t\tState is set OK : Source is : (state) {0} (reason) {1}, Target is correct mapping as : (state) {2} (reason) {3}",
                            source_workItem.State, source_workItem.Reason, target_workItem.State, target_workItem.Reason));

            }





        }
        void fixEpicState(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {


            if (processFromStateToState(source_workItem, "In Progress", "Implementation started", target_workItem, "In Progress", "Implementation started"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Done", "Work finished", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "In Progress", "Additional work found",
            target_workItem,
            "In Progress", "Additional work found",    // [1] target state and reason
            "New", "New epic",           // however if target is set like this
            "Done", "Work finished")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Moved to the backlog",
             target_workItem,
             "New", "Moved to the backlog",    // [1] target state and reason
             "New", "New epic",           // however if target is set like this
             "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Removed", "Removed from the backlog", target_workItem, "Removed", "Removed from the backlog"))
            {
                return;
            }

            else
            {
                logProgress(string.Format("\t\tState is set OK : Source is : (state) {0} (reason) {1}, Target is correct mapping as : (state) {2} (reason) {3}",
                            source_workItem.State, source_workItem.Reason, target_workItem.State, target_workItem.Reason));

            }





        }
        void fixBugState(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {


            if (processFromStateToState(source_workItem, "Active", "New", target_workItem, "Committed", "Commitment made by the team"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Fixed", "As Designed", target_workItem, "Removed", "Not a Bug"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Fixed", "Cannot Reproduce", target_workItem, "Removed", "Not a Bug"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Fixed", "Duplicate", target_workItem, "Removed", "Duplicate"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Fixed", "Fixed", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Fixed", "Deferred", target_workItem, "New", "Moved to the backlog"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Active", "Not fixed",
                target_workItem,
                "New", "Reconsidering backlog item",    // [1] target state and reason
                "New", "New defect reported",           // however if target is set like this
                "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Active", "Regression",
                target_workItem,
                "New", "Reconsidering backlog item",    // [1] target state and reason
                "New", "New defect reported",           // however if target is set like this
                "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Active", "Test Failed",
            target_workItem,
            "New", "Reconsidering backlog item",    // [1] target state and reason
            "New", "New defect reported",           // however if target is set like this
            "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }

//from vsts
            else    if (processFromStateToState(source_workItem, "Committed", "Commitment made by the team", target_workItem, "Committed", "Commitment made by the team"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Removed", "Not a Bug", target_workItem, "Removed", "Not a Bug"))
            {
                return;
            }
            
            else if (processFromStateToState(source_workItem, "Removed", "Duplicate", target_workItem, "Removed", "Duplicate"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Done", "Work finished", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Moved to the backlog", target_workItem, "New", "Moved to the backlog"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "In Progress", "Additional work found",
                            target_workItem,
                            "In Progress", "Additional work found",    // [1] target state and reason
                            "New", "New defect reported",           // however if target is set like this
                            "Done", "Work finished")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Moved to the backlog",
             target_workItem,
             "New", "Moved to the backlog",    // [1] target state and reason
             "New", "New defect reported",           // however if target is set like this
             "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Reconsidering backlog item",
                target_workItem,
                "New", "Reconsidering backlog item",    // [1] target state and reason
                "New", "New defect reported",           // however if target is set like this
                "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }
            
            
       

            else
            {
                logProgress(string.Format("\t\tState is set OK : Source is : (state) {0} (reason) {1}, Target is correct mapping as : (state) {2} (reason) {3}",
                            source_workItem.State, source_workItem.Reason, target_workItem.State, target_workItem.Reason));

            }





        }
        void fixPBIState(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {


            if (processFromStateToState(source_workItem, "New", "New", target_workItem, "New", "New backlog item"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Active", "Implementation started", target_workItem, "Committed", "Commitment made by the team"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Resolved", "Code complete and unit tests pass", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Removed", "Removed from the backlog", target_workItem, "Removed", "Removed from the backlog"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Closed", "Acceptance tests pass", target_workItem, "Done", "Work finished"))
            {
                return;
            }

                //vsts
            if (processFromStateToState(source_workItem, "Committed", "Commitment made by the team", target_workItem, "Committed", "Commitment made by the team"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Done", "Work finished", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Removed", "Removed from the backlog", target_workItem, "Removed", "Removed from the backlog"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Done", "Work finished", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "In Progress", "Additional work found",
                                target_workItem,
                                "In Progress", "Additional work found",    // [1] target state and reason
                                "New", "New backlog item",           // however if target is set like this
                                "Done", "Work finished")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Moved to the backlog",
             target_workItem,
             "New", "Moved to the backlog",    // [1] target state and reason
             "New", "New backlog item",           // however if target is set like this
             "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }

            else
            {

                logProgress(string.Format("\t\tState is set OK : Source is : (state) {0} (reason) {1}, Target is correct mapping as : (state) {2} (reason) {3}",
                            source_workItem.State, source_workItem.Reason, target_workItem.State, target_workItem.Reason));

            }




        }

        void fixTaskState(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {


            if (processFromStateToState(source_workItem, "New", "New", target_workItem, "To Do", "New task"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Work halted",
                target_workItem,
                "To Do", "Work stopped",    // [1] target state and reason
                "To Do", "New task",           // however if target is set like this
                "In Progress", "Work started")) // set target like this before then setting to target state [1]
            
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Active", "Work started", target_workItem, "In Progress", "Work started"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Closed", "Completed", target_workItem, "Done", "Work finished"))
            {
                return;
            }

            else if (processFromStateToState(source_workItem, "Closed", "Cut", target_workItem, "Removed", "Removed from the backlog"))
            {
                return;
            }

            else if (processFromStateToState(source_workItem, "Closed", "Obsolete", target_workItem, "Removed", "Removed from the backlog"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "To Do", "Work stopped",
                target_workItem,
                "To Do", "Work stopped",    // [1] target state and reason
                "To Do", "New task",           // however if target is set like this
                "In Progress", "Work started")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "In Progress", "Work started", target_workItem, "In Progress", "Work started"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Done", "Work finished", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "In Progress", "Additional work found",
                                    target_workItem,
                                    "In Progress", "Additional work found",    // [1] target state and reason
                                    "To Do", "New task",           // however if target is set like this
                                    "Done", "Work finished")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Moved to the backlog",
             target_workItem,
             "New", "Moved to the backlog",    // [1] target state and reason
             "To Do", "New task",           // however if target is set like this
             "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Removed", "Removed from the backlog", target_workItem, "Removed", "Removed from the backlog"))
            {
                return;
            }

            else
            {
                logProgress(string.Format("\t\tState is set OK : Source is : (state) {0} (reason) {1}, Target is correct mapping as : (state) {2} (reason) {3}",
                            source_workItem.State, source_workItem.Reason, target_workItem.State, target_workItem.Reason));

            }





        }
        void fixFeatureState(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {


            if (processFromStateToState(source_workItem, "New", "MMF Created", target_workItem, "New", "New feature"))
            {
                return;
            }

            else if (processFromStateToState(source_workItem, "Active", "Implementation started", target_workItem, "In Progress", "Implementation started"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Resolved", "Stories complete", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Closed", "Acceptance tests pass", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "In Progress", "Implementation started", target_workItem, "In Progress", "Implementation started"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Done", "Work finished", target_workItem, "Done", "Work finished"))
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "In Progress", "Additional work found",
                        target_workItem,
                        "In Progress", "Additional work found",    // [1] target state and reason
                        "New", "New epic",           // however if target is set like this
                        "Done", "Work finished")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "New", "Moved to the backlog",
             target_workItem,
             "New", "Moved to the backlog",    // [1] target state and reason
             "New", "New epic",           // however if target is set like this
             "Removed", "Removed from the backlog")) // set target like this before then setting to target state [1]
            {
                return;
            }
            else if (processFromStateToState(source_workItem, "Removed", "Removed from the backlog", target_workItem, "Removed", "Removed from the backlog"))
            {
                return;
            }

            else
            {
                logProgress(string.Format("\t\tState is set OK : Source is : (state) {0} (reason) {1}, Target is correct mapping as : (state) {2} (reason) {3}",
                            source_workItem.State, source_workItem.Reason, target_workItem.State, target_workItem.Reason));

            }





        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                _logFile = folderBrowserDialog1.SelectedPath;

                textBox_logFilePath.Text = _logFile + (_logFile != null && _logFile != "" ? @"\MigrationToolLog.txt" : "");
            }
            
        }

    }
}
