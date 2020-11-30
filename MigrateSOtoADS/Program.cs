using MigrateSOtoADS.AzureDevopServer;
using MigrateSOtoADS.Superoffice;
using Shared;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MigrateSOtoADS
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var reader = new XmlReader(path: Config.XmlFilePath);
            var projectIds = reader.GetColumns(CsvColumns.SuperOfficeIds).Select(x => Convert.ToInt32(x)); 

            foreach (var projectid in projectIds)
            {
                SuperofficeManager smgr = new SuperofficeManager("Data Source=prodsrv230.aloc.com;Initial Catalog=SuperOffice;User ID=soupdater;Password=7cET!QCM", projectid.ToString());
            
                List<Ticket> tickets = smgr.GetTickets();
                var azure = new AzureDevopManager(Constants.URL_TFS_TEST_VTEC, projectid);
                await azure.MigrateAllTicketsFromAProjectToAzureDevopServer(Config.ProjectName, tickets);
            }


            //var ticket = smgr.GetSingleTicket(87679);
            //await azure.MigrateSingleTicketToAzureDevopsServer(projectName, ticket);
        }
    }
}
