using MigrateSOtoADS.AzureDevopServer;
using MigrateSOtoADS.Superoffice;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timelog;

namespace MigrateSOtoADS
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var wit = new WitAdminService();
            var projectName = "7.25 Leverance TEST";
            //wit.ExtractXmlProductBackLogItemsIntoDirectory(Constants.URL_TFS_VITEC, project, Constants.CMMI);

            //wit.ImportCustomFieldListItemsToTFS(Constants.URL_TFS_VITEC, projectName);
            //Console.ReadLine();
            var projectId = "2742";

            SuperofficeManager smgr = new SuperofficeManager("Data Source=prodsrv230.aloc.com;Initial Catalog=SuperOffice;User ID=soupdater;Password=7cET!QCM", projectId);
            List<Ticket> tickets = smgr.GetTickets();

            //var ticket = smgr.GetSingleTicket(17502);

            var azure = new AzureDevopManager(Constants.URL_TFS_VITEC);


            //await azure.MigrateSingleTicketToAzureDevopsServer(projectName, ticket);
            await azure.MigrateAllTicketsFromAProjectToAzureDevopServer(projectName, tickets);

            //var result = await smgr.GetTimeLogData();

            //var timelogAccessor = new TimelogDataAccessor();
            //var projects = await timelogAccessor.GetTimelogProjectData();

            //foreach (var project in projects)
            //{
            //    foreach (var task in project.Tasks)
            //    {
            //        Console.WriteLine(project.Name + " " + task);
            //    }
            //}
            //Console.ReadLine();
            //Console.WriteLine("Hello World!");
        }
    }
}
