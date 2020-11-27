using MigrateSOtoADS.AzureDevopServer;
using Shared;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timelog;
using Timelog.Projects;

namespace TimelogProcesser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var projectName = "7.25 Leverance TEST";

            var wit = new WitAdminService();
            //wit.ExportTfsXmlDocument(Constants.URL_TFS_VITEC, projectName, Constants.CMMI);


            //var reader = new XmlReader(path: "C:\\Users\\virpen\\OneDrive - Vitecsoftware Group AB\\Desktop\\ideas migrering mapning.csv");

            //var projectids = reader.GetColumns(CsvColumns.TimelogProjectIds);
            //var product = reader.GetColumns(CsvColumns.AdsProjectName).FirstOrDefault();

            var timelogAccessor = new TimelogDataAccessor();
            var projects = await timelogAccessor.GetAllTimelogProjects();

            var sortedProducts = new List<Project>();

            switch ("PORTMAN")
            {
                case "PORTMAN":
                    sortedProducts = projects.Where(x => x.TypeId == "252").ToList(); break;
                case "IDEAS":
                    sortedProducts = projects.Where(x => x.TypeId == "253").ToList(); break;
                case "DATA":
                    sortedProducts = projects.Where(x => x.TypeId == "254").ToList(); break;
                case "SUPERPORT":
                    sortedProducts = projects.Where(x => x.TypeId == "256").ToList(); break;
                default:
                    break;
            }

            wit.ImportCustomWitAdminXmlFileToTFS(Constants.URL_TFS_VITEC, projectName, sortedProducts);
            Console.WriteLine("Hello World!");
        }
    }
}
