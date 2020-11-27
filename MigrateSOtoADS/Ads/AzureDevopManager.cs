using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using MigrateSOtoADS.Superoffice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrateSOtoADS.AzureDevopServer
{
    class AzureDevopManager
    {
        private VssConnection connection;
        VssCredentials creds = new VssBasicCredential(string.Empty, "yziwndm3ubidxgtuen7ngnv7kjmxxrvyx52ear2j77jzwcfz6cyq");
        //string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("kervt4vhkhgmav2gz5rctsh2t2ppn2d5sfbklhbqlyzjo3auojja"));
        WorkItemTrackingHttpClient _workItemTrackingHttpClient;
        private readonly int _projectId;

        public AzureDevopManager(string collectionUri, int projectId)
        {
            connection = new VssConnection(new Uri(collectionUri), creds);
            _workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();
            _projectId = projectId;
        }

        public async Task MigrateAllTicketsFromAProjectToAzureDevopServer(string projectName, List<Ticket> tickets)
        {
            var counter = 0;

            foreach (var ticket in tickets)
            {
                try
                {
                    var patchDocument = CreateJsonPatchDocument(ticket);

                    //ticket.Project = ConvertDanishLettersToEnglish(project);

                    await SetAreaForTicket(ticket, patchDocument, projectName);
                    await CreateIterationIfNonExists(projectName, ticket);
                    await SetIterationForTicket(ticket, patchDocument, projectName);
                    
                    SetTicketStatus(ticket, patchDocument);

                    string workItemType = "Requirement";
                    if (ticket.CaseType == CaseTypeEnum.ServiceRequest)
                        workItemType = "Feature";

                    WorkItem workItem = await _workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, projectName, workItemType, null, true);

                    Console.WriteLine($"{counter} Bug Successfully Created: Requirement #{workItem.Id}");
                    counter++;

                    await CreateMessagesOnATicket(ticket, workItem);
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Error creating Requirement: {0}", ex.InnerException.Message);
                    //_logger.LogInformation(ex.InnerException.Message);
                }
            }
        }

        public async Task MigrateSingleTicketToAzureDevopsServer(string projectName, Ticket ticket)
        {
            try
            {
                var patchDocument = CreateJsonPatchDocument(ticket);

                await SetAreaForTicket(ticket, patchDocument, projectName);

                await CreateIterationIfNonExists(projectName, ticket);
                await SetIterationForTicket(ticket, patchDocument, projectName);
                
                SetTicketStatus(ticket, patchDocument);

                string workItemType = "Requirement";
                if (ticket.CaseType == CaseTypeEnum.ServiceRequest)
                    workItemType = "Feature";

                WorkItem workItem = await _workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, projectName, workItemType, null, true);

                Console.WriteLine($"Bug Successfully Created: Requirement #{workItem.Id}");
                await CreateMessagesOnATicket(ticket, workItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating Requirement: {0}", ex.InnerException.Message);
                throw;
            }
        }

        private JsonPatchDocument CreateJsonPatchDocument(Ticket ticket)
        {
            //Message on an rfc, can be created before a ticket is created. If that is the case, we set the ticket creation date to the date of the first message created.
            var createdAt = SetTicketCreatedToFirstMessageCreated(ticket);
            var deadline = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(ticket.Deadline, "UTC").ToString("yyyy-MM-ddTHH:mm:ss.ff") + "Z";
            var priority = ticket.Priority.ToString();

            var license = "";
            if (ticket.Licence != null)
                license = ticket.Licence.LicenceNo;

            if (ticket.Priority >= 6)
                priority = "";

            var patchDocument = new JsonPatchDocument
            {
                CreateField("/fields/System.Title", ticket.Title),
                CreateTags("/fields/System.Tags", new List<string>{ "RFC " + ticket.Id.ToString(), ticket.Tag1, ticket.Tag2, ticket.Tag3, license }),
                CreateField("/fields/System.AssignedTo", GetUniqueName(ticket.CreatedBy.Email)),
                CreateField("/fields/System.CreatedDate", createdAt),
                CreateField("/fields/System.ChangedDate", createdAt),
                CreateField("/fields/Microsoft.VSTS.Scheduling.FinishDate", deadline),
                CreateField("/fields/System.ChangedBy", GetUniqueName(ticket.CreatedBy.EmailOriginal)),
                CreateField("/fields/System.CreatedBy", GetUniqueName(ticket.CreatedBy.EmailOriginal)),
                CreateField("/fields/Microsoft.VSTS.Scheduling.OriginalEstimate", ticket.Estimate.ToString()),
                CreateField("/fields/Microsoft.VSTS.Common.Priority", priority),
            };

            if (ticket.ReleaseNoteDescription.Length == 0 && 1 == 2)
            {
                patchDocument.Add(CreateField("/fields/Aloc.ReleaseNoteDescription", ticket.Description ?? ""));
                patchDocument.Add(CreateField("/fields/Aloc.ReleaseNoteType", Enum.GetName(typeof(ReleaseNotes), ticket.ReleaseNoteType)));
            }

            return patchDocument;
        }
        private async Task CreateMessagesOnATicket(Ticket ticket, WorkItem workItem)
        {
            if (ticket.Messages != null)
            {
                foreach (Message msg in ticket.Messages)
                {
                    var messageCreated = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(msg.Created, "UTC").ToString("yyyy-MM-ddTHH:mm:ss.ff") + "Z";

                    JsonPatchDocument message = new JsonPatchDocument
                    {
                        CreateField("/fields/System.History", msg.HtmlBody),
                        CreateField("/fields/System.ChangedBy", GetUniqueName(msg.Email)),
                        CreateField("/fields/System.ChangedDate", messageCreated)
                    };

                    WorkItem rs = await _workItemTrackingHttpClient.UpdateWorkItemAsync(message, workItem.Id.Value, null, true);
                    Console.WriteLine("Bug Successfully Created: Comment #{0}", rs.Id);
                }
            }
        }

        private async Task SetAreaForTicket(Ticket ticket, JsonPatchDocument patchDocument, string projectName)
        {
            if (string.IsNullOrEmpty(ticket.Project))
                return;
            try
            {
                //It will throw an error if project does not exists and be created under a special area.
                //Only way to check if the tickets project is in an invalid state..
                var area = await _workItemTrackingHttpClient.GetClassificationNodeAsync(projectName, TreeStructureGroup.Areas, "");
                if (area != null)
                    patchDocument.Add(CreateField("/fields/System.AreaPath", projectName + "\\" + ""));
            }
            catch (Exception ex)
            {
                //Maps the project to a dump area, when no relation between an area and a project cannot be found
                patchDocument.Add(CreateField("/fields/System.AreaPath", projectName + "\\" + "Skraldespanden"));
                //_logger.LogInformation(ex.Message, "Project could not be found: " + projectName);
            }
        }

        private async Task SetIterationForTicket(Ticket ticket, JsonPatchDocument patchDocument, string projectName)
        {
            if (! string.IsNullOrWhiteSpace(ticket.PlannedInVersion))
            {
                try
                {
                    //It will throw an error if project does not exists and be created under a special area.
                    //Only way to check if the tickets iteration exists is in an invalid state..
                    var area = await _workItemTrackingHttpClient.GetClassificationNodeAsync(projectName, TreeStructureGroup.Iterations, ticket.PlannedInVersion);
                    if (area != null)
                        patchDocument.Add(CreateField("/fields/System.IterationPath", projectName + "\\" + ticket.PlannedInVersion));
                }
                catch (Exception ex)
                {
                    //Maps the project to a dump area, when no relation between an iter and a project cannot be found
                    //_logger.LogInformation(ex.Message, "Project could not be found: " + projectName);
                }
            }
        }

        private void SetTicketStatus(Ticket ticket, JsonPatchDocument patchDocument)
        {
            //13 == Klar til test 17 == Klar til levering 
            if (ticket.State == 13 || ticket.State == 17)
                patchDocument.Add(CreateField("/fields/System.State", "Resolved"));

            //30 == Afventer felrettelse 28 == Under implementering 24 == Eskaleret
            if (ticket.State == 30 || ticket.State == 28 || ticket.State == 24)
                patchDocument.Add(CreateField("/fields/System.State", "Active"));

            //18 == Leveret 11 == Løst 12 == Testet og godkendt
            if (ticket.State == 18 || ticket.State == 11 || ticket.State == 12)
                patchDocument.Add(CreateField("/fields/System.State", "Closed"));

            else
                patchDocument.Add(CreateField("/fields/System.State", "Proposed"));
        }

        private string SetTicketCreatedToFirstMessageCreated(Ticket ticket)
        {
            var createdAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(ticket.CreatedAt, "UTC").ToString("yyyy-MM-ddTHH:mm:ss.ff") + "Z";

            //Message on an rfc, can be created before a ticket is created. If that is the case, we set the ticket creation date to the date of the first message created.
            if (ticket.CreatedAt > ticket.Messages.FirstOrDefault().Created)
            {
                createdAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(ticket.Messages.FirstOrDefault().Created, "UTC").ToString("yyyy-MM-ddTHH:mm:ss.ff") + "Z";
            }

            return createdAt;
        }

        private JsonPatchOperation CreateField(string path, string value)
        {
            return new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = path,
                Value = value
            };
        }

        private JsonPatchOperation CreateTags(string path, List<string> tags)
        {
            var cleanList = tags.Where(x => x.Length > 0);

            var mytags =  new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = path,
                Value = string.Join(",", cleanList),
            };

            return mytags;
        }

        // Get TFS Display Name from and Email Address
        private string GetUniqueName(string emailAddress)
        { 
            IdentityHttpClient identity = connection.GetClient<IdentityHttpClient>();
            var userIdentity = identity.ReadIdentitiesAsync(IdentitySearchFilter.MailAddress, emailAddress).Result.FirstOrDefault();
            if (userIdentity!=null)
            {
                return userIdentity.Properties.GetValue("Domain", "VITEC") + "\\" + userIdentity.Properties.GetValue("DirectoryAlias", "vihle");
            }
            return "System";
        }

        //private string ConvertDanishLettersToEnglish(string project)
        //{
        //    var regex = new Regex("ø|Ø|æ|Æ|å|Å");
        //    var projectContainsAny = regex.IsMatch(project);

        //    if (projectContainsAny)
        //    {
        //        project.Replace("ø", "o").Replace("Ø", "O").Replace("æ", "ae").Replace("Æ", "Ae").Replace("å", "aa").Replace("Å", "Aa");
        //    }

        //    return project;
        //}

        private async Task CreateIterationIfNonExists(string project, Ticket ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket.PlannedInVersion))
                return;

            var iterations = await _workItemTrackingHttpClient.GetClassificationNodeAsync(project,
                                                                        TreeStructureGroup.Iterations,
                                                                        depth: 1);
            if (iterations.HasChildren == true)
            {
                var iteration = iterations.Children.FirstOrDefault(x => x.Name == ticket.PlannedInVersion);
                if (iteration == null)
                {
                    var response = await _workItemTrackingHttpClient.CreateOrUpdateClassificationNodeAsync(new WorkItemClassificationNode { Name = ticket.PlannedInVersion },
                                                                                                            project,
                                                                                                            TreeStructureGroup.Iterations);
                }
            }
        }
    }
}
