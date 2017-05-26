using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Server;
using System.Xml;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;

namespace AreaImportExportTool
{
    public partial class MainForm : Form
    {
        string sourceRoot = "";
        string targetRoot = "";
        string source_team_iteration_backlog_path = null;
        string source_team_iteration_default_path = null;
        List<string> source_team_iteration_paths = null;
        TfsTeamProjectCollection _source_tfs = null;
        WorkItemStore _source_workItemStore = null;
        Project _source_workItemProject = null;

        TfsTeamProjectCollection _target_tfs = null;
        WorkItemStore _target_workItemStore = null;
        Project _target_workItemProject = null;

        public MainForm()
        {

            InitializeComponent();

            chkIterationStructure_CheckedChanged(this, EventArgs.Empty);

            this.source_txtCollectionUri.Text = "";
        }

        private void chkIterationStructure_CheckedChanged(object sender, EventArgs e)
        {
            cmdExport.Enabled = cmdImport.Enabled =
                (chkAreaStructure.Checked || chkIterationStructure.Checked) &&
                (!(String.IsNullOrEmpty(source_txtCollectionUri.Text) ||
                source_txtCollectionUri.Text.Trim().Length == 0) ||
                !Uri.IsWellFormedUriString(source_txtCollectionUri.Text, UriKind.Absolute)) &&
                (!(String.IsNullOrEmpty(source_txtTeamProject.Text) ||
                source_txtTeamProject.Text.Trim().Length == 0));
        }

        private void cmdExport_Click(object sender, EventArgs e)
        {
            Uri collectionUri = new Uri(source_txtCollectionUri.Text);
            string teamProject = source_txtTeamProject.Text;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.CheckPathExists = true;
            dlg.Title = "Export Area/Iteration structure";
            dlg.DefaultExt = ".nodes";
            dlg.Filter = "TFS Node Structure (*.nodes)|*.nodes";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string filename = dlg.FileName;

                              using (WaitState waitState = new WaitState(this))
                {
                  
                    WorkItemStore store = _source_workItemStore;
                    Project proj = _source_workItemProject;

                    using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine("NODES");
                        writer.WriteLine(String.Format("{0}, Version {1}", Application.ProductName, Application.ProductVersion));
                        writer.WriteLine("Copyright (C) 2017 " + Application.CompanyName);

                        if (chkAreaStructure.Checked)
                        {
                            WriteNodes(proj.AreaRootNodes, writer, "A");
                        }

                        if (chkIterationStructure.Checked)
                        {
                            WriteNodes(proj.IterationRootNodes, writer, "I");
                        }

                        writer.Close();
                    }
                }

                MessageBox.Show("Export successful.");
            }
        }
        private static Node GetNode(NodeCollection nodes, string pthToFind)
        {
            foreach (Node node in nodes)
            {
                string pth = node.Path.Substring(node.Path.IndexOf(@"\"));
                if (pthToFind == pth)
                {
                    return node;
                }
                if (node.ChildNodes.Count > 0)
                {
                    Node n = GetNode(node.ChildNodes, pthToFind);
                    if (n != null) { return n; }
                }
            }

            return null;
        }
        private static void WriteNodes(NodeCollection nodes, StreamWriter writer, string prefix)
        {
            foreach (Node node in nodes)
            {
                writer.Write(prefix);
                writer.WriteLine(node.Path.Substring(node.Path.IndexOf(@"\")));

                if (node.ChildNodes.Count > 0)
                {
                    WriteNodes(node.ChildNodes, writer, prefix);
                }
            }
        }

        private void cmdImport_Click(object sender, EventArgs e)
        {
            Uri collectionUri = new Uri(target_txtCollectionUri.Text);
            string teamProject = target_txtTeamProject.Text;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Title = "Import Area/Iteration structure";
            dlg.DefaultExt = ".nodes";
            dlg.Filter = "TFS Node Structure (*.nodes)|*.nodes";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string filename = dlg.FileName;


                using (WaitState waitState = new WaitState(this))
                {
                    

                    WorkItemStore store = _target_workItemStore;
                    Project proj = _target_workItemProject;


                    NodeInfo rootAreaNode = null;
                    NodeInfo rootIterationNode = null;
                    NodeInfo source_rootAreaNode = null;
                    NodeInfo source_rootIterationNode = null;
                    ICommonStructureService4 css = (ICommonStructureService4)_target_tfs.GetService(typeof(ICommonStructureService4));
                    ICommonStructureService4 source_css = (ICommonStructureService4)_source_tfs.GetService(typeof(ICommonStructureService4));
                    foreach (NodeInfo info in css.ListStructures(proj.Uri.ToString()))
                    {




                        if (info.StructureType == "ProjectModelHierarchy")
                        {
                            rootAreaNode = info;
                        }
                        else if (info.StructureType == "ProjectLifecycle")
                        {
                            rootIterationNode = info;
                        }
                    }
                    foreach (NodeInfo info in source_css.ListStructures(_source_workItemProject.Uri.ToString()))
                    {




                        if (info.StructureType == "ProjectModelHierarchy")
                        {
                            source_rootAreaNode = info;
                        }
                        else if (info.StructureType == "ProjectLifecycle")
                        {
                            source_rootIterationNode = info;
                        }
                    }

                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string nextLine = reader.ReadLine();
                        if (nextLine != "NODES")
                        {
                            MessageBox.Show("Wrong file format!");
                            return;
                        }

                        reader.ReadLine();
                        reader.ReadLine();

                        while (!reader.EndOfStream)
                        {
                            nextLine = reader.ReadLine();
                            if (nextLine.StartsWith("A") && chkAreaStructure.Checked)
                            {
                                CreateNode(source_css, css, nextLine.Substring(2), source_rootAreaNode, rootAreaNode);
                            }
                            else if (nextLine.StartsWith("I") && chkIterationStructure.Checked)
                            {
                                CreateNode(source_css, css, nextLine.Substring(2), source_rootIterationNode, rootIterationNode, true);
                            }
                            else
                            { // Ignore other lines
                            }
                        }
                        reader.Close();
                    }

                    MessageBox.Show("Import successful.");
                }
            }
        }
        private void teamInfo()
        {


       

            TeamSettingsConfigurationService tscs = _source_tfs.GetService<TeamSettingsConfigurationService>();
            var tservice = _source_tfs.GetService<TfsTeamService>();
            var teams = tservice.QueryTeams(_source_workItemProject.Uri.ToString());
            source_team_iteration_paths = new List<string>();

            foreach (var t in teams)
            {
                if (t.Name != textBox2.Text) { continue; }


                logInfo("  {0}", t.Name);
                IEnumerable<TeamConfiguration> teamconfigs = tscs.GetTeamConfigurations(new[] { t.Identity.TeamFoundationId });
                TeamConfiguration tconfig = null;
                foreach (TeamConfiguration tcon in teamconfigs)
                {
                    tconfig = tcon; break;
                }
                logInfo("  {0}", tconfig.TeamName);
                foreach (string sc in tconfig.TeamSettings.IterationPaths)
                {
                    source_team_iteration_paths.Add(sc);


                    logInfo("  {0}", sc);
                }
                source_team_iteration_backlog_path = tconfig.TeamSettings.BacklogIterationPath;
                source_team_iteration_default_path = tconfig.TeamSettings.CurrentIterationPath;

         
                break;
            }
        }

        private void setTargetTeamInfo()
        {


         


            TeamSettingsConfigurationService tscs = _target_tfs.GetService<TeamSettingsConfigurationService>();
            var tservice = _target_tfs.GetService<TfsTeamService>();
            var teams = tservice.QueryTeams(_target_workItemProject.Uri.ToString());
            
            sourceRoot = _source_workItemProject.Name;
            targetRoot = _target_workItemProject.Name;

            foreach (var t in teams)
            {
                if (t.Name != textBox5.Text) { continue; }

                logInfo("  {0}", t.Name);

                IEnumerable<TeamConfiguration> teamconfigs = tscs.GetTeamConfigurations(new[] { t.Identity.TeamFoundationId });
                TeamConfiguration tconfig = null;
                foreach (TeamConfiguration tcon in teamconfigs)
                {
                    tconfig = tcon; break;
                }
                logInfo("  {0}", tconfig.TeamName);
                List<string> adjustedPaths = new List<string>();
                foreach(string p in source_team_iteration_paths)
                {
                    
                    adjustedPaths.Add(p.Replace(sourceRoot,targetRoot));
                }


                tconfig.TeamSettings.IterationPaths = adjustedPaths.ToArray();
                tconfig.TeamSettings.BacklogIterationPath = source_team_iteration_backlog_path.Replace(sourceRoot,targetRoot);
             
                tscs.SetTeamSettings(tconfig.TeamId, tconfig.TeamSettings);


                break;
            }
        }

        private void logInfo(string s, string ss = null)
        {
            string o = s;
            if(ss!=null)
            {

                o = string.Format(s, ss);
            }
           

            textBox1.Text = o + "\r\n" + textBox1.Text;


            Application.DoEvents();

        }

        private bool nodeExists(ICommonStructureService4 css, string nodeName, NodeInfo rootNode)
        {
            try
            {
                if (css.GetNodeFromPath(rootNode.Path + "\\" + nodeName) != null)
                {
                    return true;//exists allready
                }
            }
            catch
            {

            }
            return false;//does not exist
        }
        private void CreateNode(ICommonStructureService4 source_css, ICommonStructureService4 target_css, string nodeName, NodeInfo source_rootNode, NodeInfo target_rootNode, bool iteration = false)
        {

            logInfo(string.Format("{0} {1}", (iteration==true?"I":"A"), nodeName));


            NodeInfo source_node = null;
            if (iteration)
            {

                source_node = source_css.GetNodeFromPath(source_rootNode.Path + "\\" + nodeName);
            }






            if (!nodeName.Contains("\\"))
            {
                //if exists allready do not create
                try
                {
                    target_css.CreateNode(nodeName, target_rootNode.Uri);
                }
                catch { }

            }
            else
            {


                int lastBackslash = nodeName.LastIndexOf("\\");
                NodeInfo info = target_css.GetNodeFromPath(target_rootNode.Path + "\\" + nodeName.Substring(0, lastBackslash));


                try
                {
                    target_css.CreateNode(nodeName.Substring(lastBackslash + 1), info.Uri);

                }
                catch { }

                if (iteration && source_node != null)
                {
                    NodeInfo new_node_info = target_css.GetNodeFromPath(target_rootNode.Path + "\\" + nodeName);
                    if (new_node_info.StartDate == null && new_node_info.FinishDate == null)
                    {
                        target_css.SetIterationDates(new_node_info.Uri, source_node.StartDate, source_node.FinishDate);
                    }
                }

            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private class WaitState : IDisposable
        {
            private Form parent;

            public WaitState(Form parent)
            {
                parent.Cursor = Cursors.WaitCursor;
                parent.Enabled = false;
                this.parent = parent;
            }

            void IDisposable.Dispose()
            {
                parent.Cursor = Cursors.Default;
                parent.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TeamProjectPicker p = new TeamProjectPicker();
            var result = p.ShowDialog();
            if (p.SelectedTeamProjectCollection != null)
            {
                _source_tfs = p.SelectedTeamProjectCollection;
                _source_workItemStore = p.SelectedTeamProjectCollection.GetService<WorkItemStore>();
                _source_workItemProject = _source_workItemStore.Projects[p.SelectedProjects[0].Name];

                Uri source_tfsUri = p.SelectedTeamProjectCollection.Uri;

                this.source_txtCollectionUri.Text = source_tfsUri.ToString();
                source_txtTeamProject.Text = p.SelectedProjects[0].Name;



            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            TeamProjectPicker p = new TeamProjectPicker();
            var result = p.ShowDialog();
            if (p.SelectedTeamProjectCollection != null)
            {
                _target_tfs = p.SelectedTeamProjectCollection;
                _target_workItemStore = p.SelectedTeamProjectCollection.GetService<WorkItemStore>();
                _target_workItemProject = _target_workItemStore.Projects[p.SelectedProjects[0].Name];

                Uri target_tfsUri = p.SelectedTeamProjectCollection.Uri;

                this.target_txtCollectionUri.Text = target_tfsUri.ToString();
                target_txtTeamProject.Text = p.SelectedProjects[0].Name;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //migrate the team paths
            source_team_iteration_backlog_path = null;
            source_team_iteration_default_path = null;
            source_team_iteration_paths = null;
            teamInfo();
            setTargetTeamInfo();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}