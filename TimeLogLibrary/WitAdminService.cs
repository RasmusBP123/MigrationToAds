using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Timelog.Extensions;
using System.Linq;
using System.Xml.Linq;
using Timelog.Projects;

namespace MigrateSOtoADS.AzureDevopServer
{
    public class WitAdminService
    { 
        string cmdText = "\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer\\witadmin.exe\"";
        string fullPath;

        public WitAdminService()
        {
            fullPath = ConfigurationManager.AppSettings["filePathToWitAdmin"].EscapeString();
        }

        public void ImportCustomWitAdminXmlFileToTFS(string url, string project)
        {
            var directory = ConfigurationManager.AppSettings["ImportDirectory"].EscapeString();
            var argument = $"importwitd /collection:{url} /p:\"{project}\" /f:{directory}";
            StartCommand(fullPath, argument);
        }

        public void ExtractXmlProductBackLogItemsIntoDirectory(string url, string project, string projectType)
        {
            var directory = ConfigurationManager.AppSettings["OutputDirectory"].EscapeString();
            var argument = $"exportwitd /collection:{url} /p:\"{project}\" /n:\"{projectType}\" /f:{directory}";
            StartCommand(fullPath, argument);
        }

        public void OverwriteXmlDocument(List<Project> projects)
        {
            var document = XDocument.Load("C:\\Users\\virpen\\OneDrive - Vitecsoftware Group AB\\Desktop\\xml\\productBacklog12.xml");
            var fields = document.Descendants("FIELDS");

            var listItems = fields.Elements("FIELD").Where(x => x.FirstAttribute.Value == "Timelog").Elements("SUGGESTEDVALUES").FirstOrDefault();
            listItems.RemoveAll();

            var concatList = new List<string>();
            foreach (var project in projects)
            {
                foreach (var task in project.Tasks)
                {
                    concatList.Add(project.Name + " " + task);
                }
            }

            var removeDuplicates = concatList.Distinct();
            foreach (var project in removeDuplicates)
            {
                Console.WriteLine(project);
                var xelement = new XElement("LISTITEM", new XAttribute("value", project));
                listItems.Add(xelement);
            }

            document.Save("C:\\Users\\virpen\\OneDrive - Vitecsoftware Group AB\\Desktop\\xml\\productBacklog12.xml");
            Console.WriteLine("DONE");
        }


        //public void GetProductBacklogDirectory(List<TimelogData> data)
        //{
        //    var document = XDocument.Load("C:\\Users\\virpen\\OneDrive - Vitecsoftware Group AB\\Desktop\\xml\\productBacklog.xml");
        //    var fields = document.Descendants("FIELDS");

        //    var customModulesListItems = fields.Elements("FIELD").Where(x => x.FirstAttribute.Value == "Custom_modules").Elements("ALLOWEDVALUES").FirstOrDefault();
        //    customModulesListItems.RemoveAll();
        //    foreach (var item in data)
        //    {

        //        var xelement = new XElement("LISTITEM", new XAttribute("value", item.Name));
        //        customModulesListItems.Add(xelement);
        //    }

        //    //var node = fields.ToList().First().Descendants("FIELD").Where(x => x.FirstAttribute.Value == "Custom_modules").Descendants("ALLOWEDVALUES").Nodes().ToList();
        //}

        private void StartCommand(string cmdText, string argument)
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = cmdText,
                Arguments = argument,
            };

            using (var proc = new Process())
            {
                proc.StartInfo = startInfo;
                proc.Start();

                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
            }
        }
    }
}
