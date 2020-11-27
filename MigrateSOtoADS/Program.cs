using MigrateSOtoADS.AzureDevopServer;
using MigrateSOtoADS.Superoffice;
using Shared.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MigrateSOtoADS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var projectName = "IDEAS - All Projects";
            var projectId = 4743;

            SuperofficeManager smgr = new SuperofficeManager("Data Source=prodsrv230.aloc.com;Initial Catalog=SuperOffice;User ID=soupdater;Password=7cET!QCM", projectId.ToString());
            
            List<Ticket> tickets = smgr.GetTickets();
            var azure = new AzureDevopManager(Constants.URL_TFS_TEST_VTEC, projectId);
            await azure.MigrateAllTicketsFromAProjectToAzureDevopServer(projectName, tickets);

            //var ticket = smgr.GetSingleTicket(87679);
            //await azure.MigrateSingleTicketToAzureDevopsServer(projectName, ticket);
        }
    }
}
