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
        string _logFile = @"C:\Migration\Logs\TestPlanMigrationToolLog.txt";

        const string issue = "Issue";
        const string user_story = "User Story";
        const string product_backlog_item = "Product Backlog Item";
        const string bug = "Bug";
        const string task = "Task";
        const string feature = "Feature";
        const string epic = "Epic";
        const string impediment = "Impediment";
        List<WorkItem> addedFromSourceWI = null;
        public Form1()
        {
            InitializeComponent();

            textBox_FallbackAsignee.Text = _fallbackAssignee;
            textBox_OverrideAll.Text = _overrideAssignee;
            textBox_OffsetFromId.Text = _offsetFromId.ToString();
            textBox_logFilePath.Text = _logFile;

        }

        private class casemap
        {
            public ITestCase source;
            public ITestCase2 target;

        }
        private class wimap
        {
            public WorkItem source;
            public WorkItem target;

        }
        WorkItemStore _source_workItemStore = null;
        Project _source_workItemProject = null;
        ITestManagementTeamProject2 _source_project = null;

        WorkItemStore _target_workItemStore = null;
        Project _target_workItemProject = null;
        ITestManagementTeamProject2 _target_project = null;
        List<casemap> caseMapping = new List<casemap>();
        List<wimap> wiMapping = new List<wimap>();

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

        void listExistingPoints(ITestManagementTeamProject2 pj, WorkItemCollection source_wi_list)
        {
            List<ITestPlan2> plansForCases = new List<ITestPlan2>();
            List<int> casesMatchedToPlan = new List<int>();
            List<List<int>> casesMatchedToPlanByPlan = new List<List<int>>();
            foreach (ITestPlan2 tp in pj.TestPlans.Query("Select * From TestPlan"))
            {
                logProgress(string.Format("Plan - {0} : {1}", tp.Id, tp.Name));
                casesMatchedToPlan = new List<int>();

                foreach (ITestPoint point in tp.QueryTestPoints(
                    "SELECT * FROM TestPoint"))
                {

                    bool foundPlan = false;
                    foreach (WorkItem w in source_wi_list)
                    {
                        if (point.TestCaseWorkItem.Id == w.Id)
                        {
                            if (casesMatchedToPlan.Contains(point.TestCaseWorkItem.Id) == false)
                            {
                                casesMatchedToPlan.Add(point.TestCaseWorkItem.Id);
                            }


                            foundPlan = true;
                            if (plansForCases.Contains(tp) == false)
                            {
                                casesMatchedToPlanByPlan.Add(casesMatchedToPlan);
                                plansForCases.Add(tp);
                            }

                            logProgress(string.Format("\t\tTestCase {0}  PLAN = {1}  iteration {2}  area {3}",
                                point.TestCaseWorkItem.Id,
                                tp.Id,
                                tp.Iteration,
                                tp.AreaPath));


                            break;
                        }
                    }




                }

            }
            foreach (WorkItem w in source_wi_list)
            {
                bool foundMatch = false;
                foreach (List<int> casesInPlan in casesMatchedToPlanByPlan)
                {
                    foreach (int matchedId in casesInPlan)
                    {
                        if (matchedId == w.Id)
                        {
                            foundMatch = true;
                            break;
                        }

                    }

                    if (foundMatch == true) { break; }
                }

                if (foundMatch == false)
                {
                    logProgress(string.Format("\t\t****No Plan**** for {0} {1} : {2}",
                        w.Type.Name, w.Id, w.Title));
                }
            }
            int i = 0;
            foreach (ITestPlan2 tpForCases in plansForCases)
            {
                logProgress(string.Format("\tPLAN = {0}:{1} (iteration {2}  area {3})",
                                    tpForCases.Id,
                                    tpForCases.Name,
                                    tpForCases.Iteration,
                                    tpForCases.AreaPath));

                foreach (int x in casesMatchedToPlanByPlan[i])
                {
                    logProgress(string.Format("\t\tCASE = {0}", x));
                }
                i++;
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
        IStaticTestSuite processSuite(
            //Source
            ITestManagementTeamProject2 source_project,
            ITestPlan2 source_plan,
            IStaticTestSuite source_suite,
            bool top,
            //Target
            ITestManagementTeamProject2 target_project,
            ITestPlan2 target_plan,
            Dictionary<string, int> targetConfigs,
            IStaticTestSuite target_parent_suite, bool childSuite)
        {
            logProgress(string.Format("SOURCE Test Suite: {0}  {1}", source_suite.Id, source_suite.Title));


            IStaticTestSuite target_new_suite = null;
            if (top == true)
            {
                target_new_suite = target_parent_suite;

                //update the From Id field
                WorkItem target_workItem = _target_workItemStore.GetWorkItem(target_new_suite.Id);

                WorkItem source_workItem = _source_workItemStore.GetWorkItem(source_suite.Id);
                addToListOfSourceWI(source_workItem);
                target_workItem.Fields["From Id"].Value = (getSourceId(source_workItem)).ToString();



                sourceFieldToTargetField(source_project, target_project, source_workItem, target_workItem, "System.AreaPath");
                sourceFieldToTargetField(source_project, target_project, source_workItem, target_workItem, "System.IterationPath");

                target_workItem.Save();

                logProgress(string.Format("----> Created Root Test Suite: {0}  {1}", target_workItem.Id, target_workItem.Title));
            }
            else
            {
                //CREATE
                target_new_suite = createNewSuite(
                    source_project,
                    source_suite,
                    target_project,
                    target_plan,
                    targetConfigs,
                    target_parent_suite);//creates the suite as an entry on the target_parent_suite

            }

            logProgress(string.Format("\tTARGET Test Suite: {0}", target_new_suite.Title));

            //Query the Test Points 
            //A Test Point represents a Test Case to be run against a Test Configuration in a Test Suite.
            //In MTLM, if you go to the Testing Center –> Plan tab –> Contents, you see the list of Test Case’s in each suite.  
            //One of the columns in the grid is “Configurations” which shows a count of how many Test Configurations that 
            //Test Case is tested against in that suite.  
            //When you go to Testing Center –> Test tab –> Run Tests and click on the same suite you will see 
            //an entry in the grid there for each Test Case / Test Configuration combination.  These are Test Points.

            List<int> doneCaseIds = new List<int>();

            foreach (Microsoft.TeamFoundation.TestManagement.Client.ITestSuiteEntry ttc in ((IStaticTestSuite)source_suite).Entries)
            {
                if (ttc.EntryType == TestSuiteEntryType.TestCase)
                {
                    //CREATE
                    createTestCase(
                        source_project,
                        ttc.TestCase,
                        target_project,
                        target_plan,
                        target_new_suite);
                }
            }


            ITestSuiteCollection source_sub_suites = ((Microsoft.TeamFoundation.TestManagement.Client.IStaticTestSuite)(source_suite)).SubSuites;
            foreach (var source_sub_suite_var in source_sub_suites)
            {
                if (source_sub_suite_var.TestSuiteType == TestSuiteType.StaticTestSuite)
                {
                    IStaticTestSuite source_sub_suite = (IStaticTestSuite)source_sub_suite_var;


                    processSuite(
                    source_project,
                    source_plan,
                    source_sub_suite,
                    false,
                    target_project,
                    target_plan,
                    targetConfigs,
                    target_new_suite, true);//target_new_suite is the parent in this call

                }
                else if (source_sub_suite_var.TestSuiteType == TestSuiteType.RequirementTestSuite)
                {
                    IRequirementTestSuite source_sub_suite = (IRequirementTestSuite)source_sub_suite_var;


                    processReqSuite(
                    source_project,
                    source_plan,
                    source_sub_suite,
                    false,
                    target_project,
                    target_plan,
                    targetConfigs,
                    target_new_suite);//target_new_suite is the parent in this call

                }
            }

            return target_new_suite;
        }
        IRequirementTestSuite processReqSuite(
            //Source
            ITestManagementTeamProject2 source_project,
            ITestPlan2 source_plan,
            IRequirementTestSuite source_suite,
            bool top,
            //Target
            ITestManagementTeamProject2 target_project,
            ITestPlan2 target_plan,
            Dictionary<string, int> targetConfigs,
            IStaticTestSuite target_parent_suite)
        {
            logProgress(string.Format("\tSOURCE Requirement Test Suite: {0}", source_suite.Title));


            IRequirementTestSuite target_new_suite = createNewReqSuite(
                    source_project,
                    source_suite,
                    target_project,
                    target_plan,
                    targetConfigs,
                    target_parent_suite);//creates the suite as an entry on the target_parent_suite
            if (target_new_suite == null)
            {
                return null;
            }

            logProgress(string.Format("\tTARGET Requirement Test Suite: {0}", target_new_suite.Title));

            foreach (Microsoft.TeamFoundation.TestManagement.Client.ITestSuiteEntry ttc in source_suite.TestCases)
            {
                if (ttc.EntryType == TestSuiteEntryType.TestCase)
                {
                    //CREATE
                    createTestCase(
                        source_project,
                        ttc.TestCase,
                        target_project,
                        target_plan,
                        null, target_new_suite);
                }
            }


            return target_new_suite;
        }
        void processSuites(
            //Source
            ITestManagementTeamProject2 source_project,
            ITestPlan2 source_plan,
            IStaticTestSuite source_parent_suite,
            bool top,
            //Target
            ITestManagementTeamProject2 target_project,
            ITestPlan2 target_plan,
            Dictionary<string, int> targetConfigs,
            IStaticTestSuite target_parent_suite)
        {
            target_parent_suite = processSuite(
                     source_project,
                     source_plan,
                     source_parent_suite,
                     top,
                     target_project,
                     target_plan,
                     targetConfigs,
                     target_parent_suite, false);


        }
        ITestConfiguration createTestConfiguration(
            ITestManagementTeamProject2 source_project,
            ITestManagementTeamProject2 target_project,
            ITestConfiguration source_config)
        {
            //check to see if it already exists
            foreach (ITestConfiguration config in target_project.TestConfigurations.Query("Select * from TestConfiguration"))
            {
                if (config.Name == source_config.Name)
                {
                    logProgress(string.Format("Config exists\"{0}\" with Id: {1}", config.Name, config.Id));
                    return config;
                }
            }


            ITestConfiguration target_config = target_project.TestConfigurations.Create();
            target_config.AreaPath = replaceRootInPath(source_project.TeamProjectName, target_project.TeamProjectName, source_config.AreaPath);// source_config.AreaPath is the source path e.g. Backlog

            target_config.Description = source_config.Description;
            target_config.IsDefault = source_config.IsDefault;
            target_config.Name = source_config.Name;
            target_config.State = source_config.State;
            foreach (KeyValuePair<string, string> x in source_config.Values)
            {
                target_config.Values.Add(x);
            }
            target_config.Save();
            logProgress(string.Format("----> Created Config \"{0}\" with Id: {1}", target_config.Name, target_config.Id));
            return (target_config);
        }
        ITestPlan2 createNewPlan(ITestManagementTeamProject2 source_project, ITestManagementTeamProject2 target_project, ITestPlan2 source_plan)
        {
            WorkItem source_plan_workItem = _source_workItemStore.GetWorkItem(source_plan.Id);
            addToListOfSourceWI(source_plan_workItem);

            //check to see if plan already exists, return it if it does
            foreach (ITestPlan2 t_plan in target_project.TestPlans.Query("Select * From TestPlan"))
            {
           //     if (t_plan.Name == source_plan.Name)
                {
                    //check the from id
                    WorkItem target_plan_workItem = _target_workItemStore.GetWorkItem(t_plan.Id);

                    string from_id = null;
                    try
                    {
                        from_id = target_plan_workItem.Fields["From Id"].Value.ToString();
                    }
                    catch { }

                    if (from_id != null && from_id == (getSourceId(source_plan.Id)).ToString())
                    {

                        logProgress(string.Format("Plan exists\"{0}\" with Id: {1}", t_plan.Name, t_plan.Id));
                        return t_plan;
                    }


                }
            }


            ITestPlan2 target_plan = (ITestPlan2)target_project.TestPlans.Create();
            target_plan.Name =  greekString( source_plan.Name);


            if (source_plan.StartDate < DateTime.Now.AddYears(-100))
            {

            }
            else
            {
                target_plan.StartDate = source_plan.StartDate;
            }

            if (source_plan.EndDate < DateTime.Now.AddYears(-100))
            {

            }
            else
            {
                target_plan.EndDate = source_plan.EndDate;
            }



            //TBD - Area and Iteration
            target_plan.AreaPath = replaceRootInPath(source_project.TeamProjectName, target_project.TeamProjectName, source_plan.AreaPath);
            target_plan.Iteration = replaceRootInPath(source_project.TeamProjectName, target_project.TeamProjectName, source_plan.Iteration);



            target_plan.AutomatedTestEnvironmentId = source_plan.AutomatedTestEnvironmentId;
            target_plan.AutomatedTestSettingsId = source_plan.AutomatedTestSettingsId;
            target_plan.Status = source_plan.Status;
            target_plan.Description = greekString(source_plan.Description);
            target_plan.UserData = source_plan.UserData;

            target_plan.Save();

            logProgress(string.Format("----> Created new Plan \"{0}\" with Id: {1}", target_plan.Name, target_plan.Id));


            //update the From Id field
            WorkItem target_workItem = _target_workItemStore.GetWorkItem(target_plan.Id);

            target_workItem.Fields["From Id"].Value = (source_plan.Id + _offsetFromId).ToString();
            target_workItem.Save();

            logProgress(string.Format("\t\tSet From Id for Plan \"{0}\" with Id: {1} = From Id ({2})",
                target_plan.Name,
                target_plan.Id,
                target_workItem.Fields["From Id"].Value));

            return (target_plan);
        }
        public static char GetLetter(char s)
        {
            string chars = "";
            string sel1 = "vwkbcdeijlmqarstuvwxyfghznopz";
            string sel2 = "RSTHIJKLMPQUVWXYNOZABCDEFGQJZ";
            if (sel1.Contains(s))
            {
                chars = sel1;
            }
            else if (sel2.Contains(s))
            {
                chars = sel2;
            }
            else { return s; }


            int index = (int)s % 32;
        
            return chars[index];
        }
        string greekString(string s)
        {
            if (_overrideAssignee == "") { return s; }

            string ret = "";
            foreach (char c in s)
            {
                char newc = GetLetter(c);

                ret += newc;

            }

            return ret;
        }
        IStaticTestSuite createNewSuite(
            //Source
            ITestManagementTeamProject2 source_project,
            IStaticTestSuite source_suite,
            //Target
            ITestManagementTeamProject2 target_project,
            ITestPlan2 target_plan,
            Dictionary<string, int> targetConfigs,
            IStaticTestSuite target_parent_suite)
        {
            IStaticTestSuite staticSuite = source_suite as IStaticTestSuite;
            WorkItem source_workItem = _source_workItemStore.GetWorkItem(source_suite.Id);
            addToListOfSourceWI(source_workItem);
            //check to see if exists; return it if it does
            foreach (Microsoft.TeamFoundation.TestManagement.Client.ITestSuiteEntry ttc in target_parent_suite.Entries)
            {
              //  if (ttc.EntryType == TestSuiteEntryType.StaticTestSuite && ttc.Title == source_suite.Title)
                {
                    //check the from id
                    WorkItem target_suite_workItem = _target_workItemStore.GetWorkItem(ttc.TestSuite.Id);
                    string from_id = null;
                    try
                    {
                        from_id = target_suite_workItem.Fields["From Id"].Value.ToString();
                    }
                    catch { }

                    if (from_id != null && from_id == (source_suite.Id + _offsetFromId).ToString())
                    {
                        IStaticTestSuite ex = ttc.TestSuite as IStaticTestSuite;
                        logProgress(string.Format("Suite exists\"{0}\" with Id: {1}", ex.Title, ex.Id));
                        return ex;
                    }


                }
            }


            IStaticTestSuite newSuite = target_project.TestSuites.CreateStatic();
            newSuite.Title = greekString(source_suite.Title);
            newSuite.Description = greekString(source_suite.Description);

            newSuite.State = source_suite.State;//TBD


            newSuite.UserData = source_suite.UserData;

            if (source_suite.DefaultConfigurations != null)
            {
                List<IdAndName> nlist = new List<IdAndName>();
                foreach (IdAndName idandname in source_suite.DefaultConfigurations)
                {

                    if (targetConfigs.ContainsKey(idandname.Name))
                    {
                        int v;//needs the new target config ids
                        targetConfigs.TryGetValue(idandname.Name, out v);
                        nlist.Add(new IdAndName(v, idandname.Name));
                    }
                }
                newSuite.SetDefaultConfigurations(nlist);
            }

            //add suite into the parent suite
            try
            {
                target_parent_suite.Entries.Add(newSuite);
                logProgress(string.Format("Created new Suite \"{0}\" with Id: {1}", newSuite.Title, newSuite.Id));

                //update the From Id field
                WorkItem target_workItem = _target_workItemStore.GetWorkItem(newSuite.Id);



                target_workItem.Fields["From Id"].Value = (source_workItem.Id + _offsetFromId).ToString();



                sourceFieldToTargetField(source_project, target_project, source_workItem, target_workItem, "System.AreaPath");
                sourceFieldToTargetField(source_project, target_project, source_workItem, target_workItem, "System.IterationPath");

                target_workItem.Save();

                target_plan.Save();
                logProgress(string.Format("Saved plan with new suite"));
                return (newSuite);
            }
            catch (Exception ex)
            {
                logProgress(string.Format("***Did not create suite {0} {1} {2}",
                    source_suite.Id.ToString(), source_suite.Title, ex.Message));
                return null;
            }

        }

        IRequirementTestSuite createNewReqSuite(
            //Source
            ITestManagementTeamProject2 source_project,
            IRequirementTestSuite source_suite,
            //Target
            ITestManagementTeamProject2 target_project,
            ITestPlan2 target_plan,
            Dictionary<string, int> targetConfigs,
            IStaticTestSuite target_parent_suite)
        {


            WorkItem target_workItem = CheckForExistingFromId(target_project.TeamProjectName, source_suite.RequirementId);
            WorkItem source_suite_workItem = _source_workItemStore.GetWorkItem(source_suite.Id);
            addToListOfSourceWI(source_suite_workItem);

            if (target_workItem != null)
            {
                logProgress(string.Format("Requirement exists\"{0}\" with Id: {1}", target_workItem.Title, target_workItem.Id));
                //check to see if exists; return it if it does
                foreach (Microsoft.TeamFoundation.TestManagement.Client.ITestSuiteEntry ttc in target_parent_suite.Entries)
                {

                    if (ttc.EntryType == TestSuiteEntryType.RequirementTestSuite)
                    {
                        IRequirementTestSuite ex = ttc.TestSuite as IRequirementTestSuite;


                        //check the from id
                        WorkItem chk_target_suite_workItem = _target_workItemStore.GetWorkItem(ttc.TestSuite.Id);
                        string from_id = null;
                        try
                        {
                            from_id = chk_target_suite_workItem.Fields["From Id"].Value.ToString();
                        }
                        catch { }

                        if (from_id != null && from_id == (source_suite.Id + _offsetFromId).ToString())
                        {
                            logProgress(string.Format("Suite exists\"{0}\" with Id: {1}", ex.Title, ex.Id));
                            return ex;
                        }



                    }

                    // return null;
                }

            }




            WorkItem source_workItem = _source_workItemStore.GetWorkItem(source_suite.RequirementId);

            if (source_workItem.Type.Name == "Task")
            { //not sure yet but have found some Tasks as requirement suite
                return null;
            }

            logProgress(string.Format("Creating Req Suite PBI \"from ID {0}\" with Type: {1}", source_workItem.Id, source_workItem.Type.Name));
            target_workItem = createTargetWIfromSourceWI(source_project, source_workItem, target_project, new List<string>());//does links


            IRequirementTestSuite newSuite = target_project.TestSuites.CreateRequirement(target_workItem);


            string a_str = source_suite.Title;

            string[] a = a_str.Split(':');

            if (a.Count() > 1)
            {
                a_str = source_suite.Title.Substring(a[0].Length + 1).TrimStart();
            }


            newSuite.Title = target_workItem.Id.ToString() + " : " + greekString(a_str.TrimStart());
            logProgress(string.Format("Create Req Suite new title = {0}", newSuite.Title));
            newSuite.Description = greekString(source_suite.Description);

            newSuite.State = source_suite.State;//TBD

            newSuite.UserData = source_suite.UserData;

            if (source_suite.DefaultConfigurations != null)
            {
                List<IdAndName> nlist = new List<IdAndName>();
                foreach (IdAndName idandname in source_suite.DefaultConfigurations)
                {

                    if (targetConfigs.ContainsKey(idandname.Name))
                    {
                        int v;//needs the new target config ids
                        targetConfigs.TryGetValue(idandname.Name, out v);
                        nlist.Add(new IdAndName(v, idandname.Name));
                    }
                }
                newSuite.SetDefaultConfigurations(nlist);
            }



            //add suite into the parent suite
            target_parent_suite.Entries.Add(newSuite);
            logProgress(string.Format("Created new Requirement Suite \"{0}\" with Id: {1} and Requirement Workitem ID {2}", newSuite.Title, newSuite.Id, newSuite.RequirementId));

            //update the From Id field
            WorkItem target_suite_workItem = _target_workItemStore.GetWorkItem(newSuite.Id);

            target_suite_workItem.Fields["From Id"].Value = (source_suite_workItem.Id + _offsetFromId).ToString();

            sourceFieldToTargetField(source_project, target_project, source_suite_workItem, target_suite_workItem, "System.AreaPath");
            sourceFieldToTargetField(source_project, target_project, source_suite_workItem, target_suite_workItem, "System.IterationPath");

            target_suite_workItem.Save();




            target_plan.Save();
            logProgress(string.Format("Saved plan with suite added"));
            return (newSuite);
        }

        void duplicateOtherLinks(ITestManagementTeamProject2 source_project, WorkItem source_workItem, ITestManagementTeamProject2 target_project, WorkItem target_workItem)
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
                if (s_link.BaseType != BaseLinkType.RelatedLink) { continue; }//only do related links. if there are no related liks nothing will be created on the target
                RelatedLink source_link = (RelatedLink)s_link;

                WorkItem source_link_workItem = _source_workItemStore.GetWorkItem(source_link.RelatedWorkItemId);//get workitem from source
                //if it does not yet exist create it on target if requied
                WorkItem target_link_workitem = createTargetWIfromSourceWI(source_project, source_link_workItem, target_project, new List<string>(),
                    false);//false = do not do links
                if (target_link_workitem.Id == 0) { return; }
                //is link already there?
                bool createLink = true;

                if (source_link.LinkTypeEnd.Name == "Parent" || source_link.LinkTypeEnd.Name == "Acceptance Test")
                {
                    //add to the parent
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

        void addToListOfSourceWI(WorkItem w)
        {
            foreach (WorkItem ww in addedFromSourceWI)
            {
                if (ww.Id == w.Id)
                {
                    return;
                }
            }
            addedFromSourceWI.Add(w);
        }
        WorkItem createTargetWIfromSourceWI(ITestManagementTeamProject2 source_project, WorkItem source_workItem,
            ITestManagementTeamProject2 target_project, List<string> fields, bool doLinks = true)
        {

            addToListOfSourceWI(source_workItem);


            WorkItem alreadyExistsAs = CheckForExistingFromId(target_project.TeamProjectName, source_workItem.Id);


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
                            target_workItem.Fields[source_field.ReferenceName].Value = replaceRootInPath(source_project.TeamProjectName, target_project.TeamProjectName, source_field.Value.ToString());
                        }
                        else if (source_field.ReferenceName == "System.AssignedTo")
                        {

                        }
                        else if (source_field.ReferenceName == "Microsoft.VSTS.Common.ActivatedBy")
                        {

                        }
                        else if (source_field.ReferenceName == "System.IterationPath")
                        {
                            target_workItem.Fields[source_field.ReferenceName].Value = replaceRootInPath(source_project.TeamProjectName, target_project.TeamProjectName, source_field.Value.ToString());
                        }
                        else if (source_field.ReferenceName == "System.Title")
                        {
                            target_workItem.Fields[source_field.ReferenceName].Value = greekString(source_field.Value.ToString());
                        }
                        else if (source_field.ReferenceName == "System.Description")
                        {
                            target_workItem.Fields[source_field.ReferenceName].Value = greekString(source_field.Value.ToString());
                        }
                        else
                        {
                            target_workItem.Fields[source_field.ReferenceName].Value = source_field.Value;
                        }
                    }
                    catch
                    {
                        //logProgress(string.Format("ERROR : Target cannot accept \"{0}\" in : {1}", source_field.ReferenceName, target_workItemType.Name));
                    }


                }
                else
                {
                    //  logProgress(string.Format("Target has no field match (or skipping) for\"{0}\" in : {1}", source_field.ReferenceName, target_workItemType.Name));
                }
            }

            foreach (string f in fields)
            {
                target_workItem.Fields[f].Value = _fallbackAssignee;
            }


            target_workItem.Fields["From Id"].Value = source_workItem.Id + _offsetFromId;//offset for migration from another source

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

                        if (field.Name == "Activated By")
                        {
                            target_workItem.Fields["Microsoft.VSTS.Common.ActivatedBy"].Value = _fallbackAssignee;
                            try
                            {
                                target_workItem.Save();
                            }
                            catch { }


                        }
                    }


                }
            }
            else // Validation passed
            {
                try
                {
                    target_workItem.Save();
                    logProgress("---> Created target workitem ID : " + target_workItem.Id + " from source ID : " + source_workItem.Id + " (From Id stored as : " + target_workItem.Fields["From Id"].Value + ")");


                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Activated By"))
                    {
                        target_workItem.Fields["Microsoft.VSTS.Common.ActivatedBy"].Value = _fallbackAssignee;


                        try { target_workItem.Save(); }
                        catch (Exception exx)
                        {
                            if (exx.Message.Contains("Closed By"))
                            {
                                target_workItem.Fields["Microsoft.VSTS.Common.ClosedBy"].Value = _fallbackAssignee;


                                target_workItem.Save();
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    else if (ex.Message.Contains("Closed By"))
                    {
                        target_workItem.Fields["Microsoft.VSTS.Common.ClosedBy"].Value = _fallbackAssignee;

                        try { target_workItem.Save(); }
                        catch (Exception exx)
                        {
                            if (exx.Message.Contains("Activated By"))
                            {
                                target_workItem.Fields["Microsoft.VSTS.Common.ActivatedBy"].Value = _fallbackAssignee;


                                target_workItem.Save();
                            }
                            else
                            {
                                throw;
                            }
                        }

                    }
                    else
                    {
                        throw;
                    }
                    //string s = ((Microsoft.TeamFoundation.Client.TfsConnection)(target_project.WitProject.Store.TeamProjectCollection)).Uri + "/Backlog/_workitems?id=" + target_workItem.Id.ToString() + "&_a=edit";
                    //Form2 fd = new Form2();
                    //fd.ShowMyDialogBox(ex, s);
                }
            }

            if (target_workItem.Id > 0)
            {

                target_workItem = _target_workItemStore.GetWorkItem(target_workItem.Id);


                //immediate links only
                if (doLinks == true)
                {
                    duplicateOtherLinks(source_project, source_workItem, target_project, target_workItem);
                }

            }


            return target_workItem;
        }

        void sourceFieldToTargetField(ITestManagementTeamProject2 source_project, ITestManagementTeamProject2 target_project, WorkItem source, WorkItem target, string f)
        {
            if (f == "System.AreaPath" || f == "System.IterationPath")
            {
                target.Fields[f].Value = replaceRootInPath(source_project.TeamProjectName, target_project.TeamProjectName, source.Fields[f].Value.ToString());
            }
            else
            {
                target.Fields[f].Value = source.Fields[f].Value;
            }

        }

        int getFromId(WorkItem w)
        {
            int v = System.Convert.ToInt32(w.Fields["From Id"].Value.ToString());

            return v;
        }
        int getSourceId(WorkItem w)
        {
            int v = System.Convert.ToInt32(w.Id.ToString());

            return v + _offsetFromId;
        }
        int getSourceId(int i)
        {


            return i + _offsetFromId;
        }
        ITestCase createTestCase(
            //Source
            ITestManagementTeamProject2 source_project,
            ITestCase source_case,
            //Target
            ITestManagementTeamProject2 target_project,
            ITestPlan2 target_plan,
            IStaticTestSuite target_Suite = null, IRequirementTestSuite target_ReqSuite = null)
        {
            addToListOfSourceWI(source_case.WorkItem);

            ITestCase2 tc = null;

            casemap foundMapping = null;

            foreach (casemap chk in caseMapping)
            {
                if (chk.source.Id == source_case.Id)
                {

                    //is a duplicate
                    foundMapping = chk;
                    break;
                }
            }


            if (target_Suite != null)
            {
                foreach (Microsoft.TeamFoundation.TestManagement.Client.ITestSuiteEntry ttc in target_Suite.Entries)
                {
                    if (ttc.EntryType == TestSuiteEntryType.TestCase)
                    {
                        tc = (ITestCase2)ttc.TestCase;
                        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                        WorkItem target_c_workItem = _target_workItemStore.GetWorkItem(tc.WorkItem.Id);

                        string from_id = null;
                        try
                        {

                            from_id = target_c_workItem.Fields["From Id"].Value.ToString();
                        }
                        catch { }

                        if (from_id != null && from_id == (getSourceId(source_case.WorkItem)).ToString())
                        {
                            logProgress(string.Format("Test Case exists\"{0}\" with Id: {1}", ttc.TestCase.Title, ttc.TestCase.Id));

                            sourceFieldToTargetField(source_project, target_project, source_case.WorkItem, tc.WorkItem, "System.AreaPath");
                            sourceFieldToTargetField(source_project, target_project, source_case.WorkItem, tc.WorkItem, "System.IterationPath");

                            tc.Save();


                            return (ITestCase)ttc.TestCase;

                        }

                    }
                }
            }
            else if (target_ReqSuite != null)
            {
                foreach (Microsoft.TeamFoundation.TestManagement.Client.ITestSuiteEntry ttc in target_ReqSuite.TestCases)
                {
                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    if (ttc.EntryType == TestSuiteEntryType.TestCase)//&& ttc.Title == source_case.Title
                    {
                        tc = (ITestCase2)ttc.TestCase;
                        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                        WorkItem target_c_workItem = _target_workItemStore.GetWorkItem(tc.WorkItem.Id);

                        string from_id = null;
                        try
                        {
                            from_id = target_c_workItem.Fields["From Id"].Value.ToString();
                        }
                        catch { }

                        if (from_id != null && from_id == (getSourceId(source_case.WorkItem)).ToString())
                        {
                            logProgress(string.Format("Test Case exists\"{0}\" with Id: {1}", ttc.TestCase.Title, ttc.TestCase.Id));
                            return (ITestCase)ttc.TestCase;
                        }

                    }
                }
            }
            else
            {
                return null;
            }

            if (foundMapping == null)
            {
                var target_cases = target_project.TestCases.Query(String.Format("Select [System.Id] From WorkItems WHERE [System.WorkItemType] = 'Test Case' AND [Team Project] = '{0}' AND [From Id] = '{1}'", target_project.TeamProjectName, getSourceId(source_case.WorkItem).ToString()));
                foreach (ITestCase ttc in target_cases)
                {
                    tc = (ITestCase2)ttc;
                    WorkItem target_c_workItem = _target_workItemStore.GetWorkItem(tc.WorkItem.Id);
                    string from_id = null;
                    try
                    {
                        from_id = target_c_workItem.Fields["From Id"].Value.ToString();
                    }
                    catch { }

                    if (from_id != null && from_id == (getSourceId(source_case.WorkItem)).ToString())
                    {
                        logProgress(string.Format("Test Case exists\"{0}\" with Id: {1}", target_c_workItem.Title, target_c_workItem.Id));

                        if (target_ReqSuite != null)
                        {
                            AddTestCaseToRequirementSuite(target_ReqSuite, tc, source_case);
                            target_plan.Save();

                        }


                        return null;// (ITestCase)ttc.TestCase;
                    }
                }


                tc = (ITestCase2)target_project.TestCases.Create();


                foreach (var a in source_case.Actions)
                {

                    tc.Actions.Add(a.CopyToNewOwner(tc));
                }

                tc.Title = greekString(source_case.Title);

                //TBD
                tc.Area = replaceRootInPath(source_project.TeamProjectName, target_project.TeamProjectName, source_case.Area);


                tc.UserData = source_case.UserData;

                tc.Priority = source_case.Priority;
                tc.Implementation = source_case.Implementation;
                tc.Description = greekString(source_case.Description);

                try
                {
                    tc.Save();


                    //update the From Id field
                    tc.WorkItem.Fields["From Id"].Value = (getSourceId(source_case.WorkItem)).ToString();

                    sourceFieldToTargetField(source_project, target_project, source_case.WorkItem, tc.WorkItem, "System.AreaPath");
                    sourceFieldToTargetField(source_project, target_project, source_case.WorkItem, tc.WorkItem, "System.IterationPath");

                    tc.Save();



                    caseMapping.Add(new casemap
                    {
                        source = source_case,
                        target = tc
                    });
                }
                catch
                {
                    tc = null;
                    logProgress(string.Format("\t**** Could not create test case : {0} {1}", source_case.WorkItem.Id.ToString(), source_case.Title));
                }
            }
            else
            {
                tc = foundMapping.target;
            }


            if (target_Suite != null && tc != null)
            {
                target_Suite.Entries.Add(tc);
                target_plan.Save();
            }
            else if (target_ReqSuite != null)
            {
                AddTestCaseToRequirementSuite(target_ReqSuite, tc, source_case);
                target_plan.Save();

            }




            return tc;

        }
        public static void AddTestCaseToRequirementSuite(IRequirementTestSuite target_ReqSuite, ITestCase testCase, ITestCase sourcetestCase)
        {



            WorkItemStore store = target_ReqSuite.Project.WitProject.Store;
            WorkItem tfsRequirement = store.GetWorkItem(target_ReqSuite.RequirementId);

            tfsRequirement.Links.Add(new RelatedLink(store.WorkItemLinkTypes.LinkTypeEnds["Tested By"], testCase.WorkItem.Id));




            tfsRequirement.Save();

            target_ReqSuite.Repopulate();
        }
        static ITestManagementTeamProject2 GetTestProject(TfsTeamProjectCollection tfs,
            string project)
        {

            ITestManagementService2 tms = tfs.GetService<ITestManagementService2>();

            return tms.GetTeamProject(project);
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
                _source_project = GetTestProject(p.SelectedTeamProjectCollection, p.SelectedProjects[0].Name);

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
                _target_project = GetTestProject(p.SelectedTeamProjectCollection, p.SelectedProjects[0].Name);

                textBox3.Text = p.SelectedTeamProjectCollection.Name;
                textBox4.Text = p.SelectedProjects[0].Name;
            }


            foreach (ITestPlan2 source_plan in _source_project.TestPlans.Query("Select * From TestPlan"))
            {

                listBox1.Items.Add(string.Format("{0}:{1}", source_plan.Id, source_plan.Name));
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
            logProgress(string.Format("\tRippleRock test plan hierarchy migration tool"));
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


            caseMapping = new List<casemap>();
            wiMapping = new List<wimap>();

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
                foreach (WorkItemLinkInfo wli in workItemLinkInfoArray)
                {
                    csvIDs = csvIDs + sep + "'" + wli.TargetId.ToString() + "'";
                    sep = ",";
                }
                source_wi_list = GetWorkitemsForIDs(_source_workItemProject.Name, csvIDs);

            }




            //string sel = listBox1.SelectedItem.ToString();
            //if (sel == null || sel.Length <= 0)
            //{
            //    return;
            //};

            //string[] itms = sel.Split(':');
            //string id = itms[0];


            //        listExistingConfigs(_target_project);

            foreach (ITestConfiguration source_config in _source_project.TestConfigurations.Query(
                "Select * from TestConfiguration"))
            {
                logProgress(string.Format("\tTest Config: {0}", source_config.Name));
                //CREATE
                ITestConfiguration target_config = createTestConfiguration(_source_project, _target_project, source_config);

            }

            Dictionary<string, int> targetConfigs = listExistingConfigs(_target_project);

            //    listExistingPlans(_target_project);

            addedFromSourceWI = new List<WorkItem>();


            var source_plans = _source_project.TestPlans.Query("Select * From TestPlan");
            int planCount = 0;
            foreach (ITestPlan2 source_plan in source_plans)
            {
                bool useThis = false;
                foreach (WorkItem source_wi in source_wi_list)
                {
                    if (source_plan.Id == source_wi.Id)
                    {
                        useThis = true;
                        break;
                    }
                }

                if (useThis == true)
                {
                    planCount++;
                }
            }

            textBox_InitialCount.Text = planCount.ToString();
            logProgress(string.Format("\tQuery plan count = {0} ", planCount));
            int createdCount = 0;

            foreach (ITestPlan2 source_plan in source_plans)
            {
                bool useThis = false;
                foreach (WorkItem source_wi in source_wi_list)
                {
                    if (source_plan.Id == source_wi.Id)
                    {
                        useThis = true;
                        break;
                    }
                }

                if (useThis == true)
                {
                    createdCount++;
                    logProgress(string.Format("\tDone plan count = {0} ", createdCount.ToString()));
                    updateCreatedCount(createdCount.ToString());



                    logProgress(string.Format("Migrate Plan - {0} : {1} : {2} : {3}", source_plan.Id,
                        source_plan.Name,
                        source_plan.AreaPath,
                        source_plan.Iteration));

                    //Create target plan as copy of the source plan
                    //CREATE
                    ITestPlan2 target_plan = createNewPlan(_source_project, _target_project, source_plan);

                    processSuites(
                        _source_project,
                        source_plan,
                        source_plan.RootSuite,
                        true,//top
                        _target_project,
                        target_plan,
                        targetConfigs,
                        target_plan.RootSuite);

                }
            }

            logProgress(string.Format("\t------------------------------FINISHED----------------------------------------------"));
            listExistingPlans(_target_project);


            logProgress(string.Format("\t------------------------------STATES, ASSIGNED TO AND ATTACHMENTS---------------------------------------------"));

            DateTime currentDateToCheckAgainstForOlderMigration = DateTime.Today;//If older than today we will not update it

            textBox_IncludingAdditional.Text = addedFromSourceWI.Count.ToString();
            createdCount = 0;

            foreach (WorkItem source_wi in addedFromSourceWI)
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
            logProgress(string.Format("\t------------------------------FINISHED----------------------------------------------"));
        }
        void fixSpoints(Project source_project, WorkItem source_workItem, Project target_project, WorkItem target_workItem)
        {
            string f_source = "Story Points";
            string f_target = "Effort";
            try
            {
                if (source_workItem.Fields.Contains(f_source) && source_workItem.Fields[f_source].Value != null &&
                         target_workItem.Fields.Contains(f_target) && source_workItem.Fields[f_source].Value != target_workItem.Fields[f_target].Value)
                {

                    target_workItem = openWI(target_workItem);
                    target_workItem.Fields[f_target].Value = source_workItem.Fields[f_source].Value;
                    target_workItem.Save();


                }
                else
                {
                    logProgress(string.Format("\tNo points to update."));
                }
            }
            catch
            {

            }

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
            //  if (target_workItem.Fields[f].Value != null && target_workItem.Fields[f].Value.ToString() == "Tom Brewer") { return; }

            if (source_workItem.Fields[f].Value != null
                && source_workItem.Fields[f].Value != target_workItem.Fields[f].Value

                )
            {

                try
                {
                    logProgress(string.Format("\t\tSet target {0} Assigned To {1}", target_workItem.Id, source_workItem.Fields[f].Value.ToString()));
                    doAssign(
                    f,
                    (_overrideAssignee != null ? _overrideAssignee : source_workItem.Fields[f].Value.ToString()),
                        //first assign to the person from source

                    source_project, source_workItem,
                    target_project, target_workItem);

                }
                catch
                {
                    string fallbackAssignee = _fallbackAssignee;
                    if (source_workItem.Fields[f].Value.ToString() == "Alejandro Cabanas" ||
                        source_workItem.Fields[f].Value.ToString() == "Rebecca Lau" ||
                        source_workItem.Fields[f].Value.ToString() == "Saishree Narayanan")
                    {
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
                    //if(textBox6.Text.Contains("Rooks"))
                    //{
                    //    fallbackAssignee = "Nigel Steane";
                    //}
                    //else if(textBox6.Text.Contains("Lannister"))
                    //{
                    //    fallbackAssignee = "Harpreet Sidhu";
                    //}
                    //else if (textBox6.Text.Contains("Custard"))
                    //{
                    //    fallbackAssignee = "Harpreet Sidhu";
                    //}
                    //else if (textBox6.Text.Contains("Morph"))
                    //{
                    //    fallbackAssignee = "Paul Schoolden";
                    //}
                    //else if (textBox6.Text.Contains("Roobarb"))
                    //{
                    //    fallbackAssignee = "Russell Rhodes";
                    //}
                    logCantAssign(string.Format("ID:{0}>>{1} {2} :used: {3}",
                        source_workItem.Id, target_workItem.Id, source_workItem.Fields[f].Value.ToString(), fallbackAssignee));
                    //logProgress(string.Format("\t\t****Use fallback user: Set target {0} Assigned To {1}",
                    //    target_workItem.Id, fallbackAssignee));

                    //try
                    //{
                    //    doAssign(
                    //        f,
                    //        fallbackAssignee,//use fallback
                    //        source_project, source_workItem,
                    //        target_project, target_workItem);
                    //}
                    //catch (Exception ex)
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
            else if (processFromStateToState(source_workItem, "Committed", "Commitment made by the team", target_workItem, "Committed", "Commitment made by the team"))
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

                textBox_logFilePath.Text = _logFile + (_logFile != null && _logFile != "" ? @"\TestPlanMigrationToolLog.txt" : "");
            }
        }
        WorkItemCollection GetWorkitemsForIDs(string projectID, string csvIDs)
        {

            string wiqlQuery = "Select ID from Workitems Where [System.TeamProject] ='" + projectID + "' AND [System.Id] IN (" + csvIDs + ")";

            // Execute the query.
            WorkItemCollection wCollection = _source_workItemStore.Query(wiqlQuery);



            return wCollection;
        }
        private void button5_Click(object sender, EventArgs e)
        {

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
                foreach (WorkItemLinkInfo wli in workItemLinkInfoArray)
                {
                    csvIDs = csvIDs + sep + "'" + wli.TargetId.ToString() + "'";
                    sep = ",";
                }
                source_wi_list = GetWorkitemsForIDs(_source_workItemProject.Name, csvIDs);

            }




            listExistingPoints(_source_project, source_wi_list);
            //   listExistingPlans(_source_project);
        }
    }



}
