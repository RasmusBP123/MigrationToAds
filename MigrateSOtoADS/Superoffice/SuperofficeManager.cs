using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateSOtoADS.Superoffice
{
    
    class SuperofficeManager
    {
        private Dictionary<int, ProductsEnum> products = new Dictionary<int, ProductsEnum>();
        private Dictionary<int, Licence> licences = new Dictionary<int, Licence>();
        private string connectionString;
        private string projectId;

        public SuperofficeManager(string connectionString, string projectId)
        {
            products.Add(1,ProductsEnum.Ideas);
            products.Add(3, ProductsEnum.Portman);
            products.Add(4, ProductsEnum.Superport);
            this.connectionString = connectionString;
            if (projectId != "")
            {
                var array = projectId.Split(',');
                this.projectId = "AND project_id = "+string.Join(" OR project_id = ", array);
            }
            LicensIds();
        }

        private void LicensIds()
        {
            string query = @"SELECT [ID]
                                   ,[X_CUSTOMER]
                                   ,[X_LICENCE_NO]
                                   ,[X_PRODUCT]
                             FROM[SuperOffice].[CRM7].[Y_PRODUCT_LICENCE] where[x_inactive] = 0";
            SqlConnection Connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, Connection);
            Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Licence licence = new Licence();
                    if (products.ContainsKey((int)reader["X_PRODUCT"]))
                    {
                        licence.Products = products[(int)reader["X_PRODUCT"]];
                    }
                    licence.LicenceNo = reader["X_LICENCE_NO"].ToString();
                    licence.Id = (int)reader["ID"];
                    licence.CustomerId = (int)reader["X_CUSTOMER"];
                    licences.Add((int)reader["ID"], licence);
                }
            }
            Connection.Close();
        }

        public List<Ticket> GetTickets()
        {
            string query = @"SELECT t1.[id],
                                    [title],
                                    [X_PRODUCT_LICENSE],
                                    [created_at],
                                    t1.[last_changed],
                                    [created_by],
                                    [author],
                                    [owned_by],
                                    [category],
                                    t1.[status],
		                            [ticket_status],
		                            [priority],
		                            [deadline],
		                            [filter_address], 							 
		                            [X_SAGSTYPE],
		                            t1.[X_PARENT],
		                            [X_ENVIRONMENT],
		                            [x_timelog_project],
		                            [x_project],
		                            [x_produkt_version],
		                            [x_estimat_opr],
		                            [x_estimat_rest], 
		                            [x_tag],
		                            [x_tag2],
		                            [x_tag3],
		                            [name] as project_name,
		                            a1.[email] as owned_by_email, 
		                            a2.[email] as created_by_email, 
		                            a3.[X_PARENT] as parent_sag,
                                    [x_planned_for_version],
		                            [x_solved_in_version],
                                    y_release_notes.x_description,
									y_release_notes.x_type
                                FROM[SuperOffice].[CRM7].[TICKET] t1
                                    FULL OUTER JOIN[SuperOffice].[CRM7].[PROJECT] ON t1.[x_project] = [SuperOffice].[CRM7].[PROJECT].[project_id]
                                    FULL OUTER JOIN[SuperOffice].[CRM7].[EJUSER] a1 ON a1.[id] = t1.[owned_by]
                                    FULL OUTER JOIN[SuperOffice].[CRM7].[EJUSER] a2 ON a2.[id] = t1.[created_by]
                                    FULL OUTER JOIN (SELECT distinct [X_PARENT] FROM [SuperOffice].[CRM7].[TICKET] t2 WHERE X_SAGSTYPE in ('Problem', 'Incident')) a3 ON a3.[X_PARENT] = t1.id
                                    LEFT OUTER JOIN y_release_notes ON
                                    t1.id = y_release_notes.x_sagsnummer
                                WHERE [closed_at] IS NULL AND NOT ticket_status IN(2, 5, 11, 18)
                                AND category IN(20,3,22,21,24,23,2,4,7)
                                " + projectId+ @"
                                order by created_at desc";


            SqlConnection Connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, Connection);
            Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<Ticket> tickets = new List<Ticket>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Ticket ticket = new Ticket();
                    ticket.Id = (int)reader["id"];
                    ticket.Title = (string)reader["title"];
                    ticket.CreatedBy = new User((int)reader["owned_by"], (string)reader["author"], (string)reader["owned_by_email"], (string)reader["created_by_email"]);
                    ticket.ProduktCategory = (int)reader["category"];
                    
                    if (reader["project_name"] != DBNull.Value)
                    {
                        ticket.Project = (string)reader["project_name"];
                    }
                    ticket.Priority = (int)reader["priority"];
                    ticket.State = (int)reader["ticket_status"];
                    ticket.CreatedAt = DateTime.Parse(reader["created_at"].ToString());
                    ticket.Estimate = DBNull.Value == reader["x_estimat_opr"] ? 0 : (int)reader["x_estimat_opr"];
                    ticket.EstimateRest = DBNull.Value == reader["x_estimat_rest"] ? 0 : (int)reader["x_estimat_rest"];
                    ticket.Timelog = (string)reader["x_timelog_project"];
                    if (reader["X_SAGSTYPE"].ToString().ToLower() == "request for change")
                    {
                        ticket.CaseType = CaseTypeEnum.RequestForChange;
                    }
                    else if (reader["X_SAGSTYPE"].ToString().ToLower() == "service request")
                    {
                        ticket.CaseType = CaseTypeEnum.ServiceRequest;
                    }
                    else
                    {
                        ticket.CaseType = CaseTypeEnum.None;
                    }
                    if (reader["X_PRODUCT_LICENSE"] != DBNull.Value && licences.ContainsKey((int)reader["X_PRODUCT_LICENSE"]))
                    {
                        ticket.Licence = licences[(int)reader["X_PRODUCT_LICENSE"]];
                    }
                    if (reader["deadline"] != DBNull.Value)
                    {
                        ticket.Deadline = DateTime.Parse(reader["deadline"].ToString());
                    }

                    ticket.Tag1 = reader["x_tag"].ToString();
                    ticket.Tag2 = reader["x_tag2"].ToString();
                    ticket.Tag3 = reader["x_tag3"].ToString();
                    ticket.ResolvedInVersion =reader["x_solved_in_version"].ToString();
                    ticket.PlannedInVersion = reader["x_planned_for_version"].ToString();
                    ticket.ReleaseNoteDescription = reader["x_description"].ToString();

                    var releaseNote = DBNull.Value == null ? "" : reader["x_type"].ToString();

                    if (releaseNote == "Nyheder")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Nyheder");
                    if (releaseNote == "Rettelse")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Rettelse");
                    if (releaseNote == "Ændring")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Ændring");
                    if (releaseNote == "Andet")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Andet");
                    if (releaseNote == "Ignorer")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Ignorer");


                    ticket.Messages = GetMessages(ticket.Id);
                    tickets.Add(ticket);
                }
            }
            Connection.Close();
            return tickets;
        }

        private List<Message> GetMessages(int id)
        {
            List<Message> messages = new List<Message>();
            SqlConnection Connection = new SqlConnection(connectionString);
            string query = @"SELECT M.[id], 
                                    U.[id] as user_id, 
                                    M.[body], 
                                    M.[html_body], 
                                    M.[author], 
                                    M.[created_at], 
                                    U.[email]
                             FROM[CRM7].[EJ_MESSAGE] M
                                INNER JOIN[CRM7].[EJUSER] U ON U.[id] = M.[created_by]
                             WHERE[ticket_id] = @ticketid order by M.created_at asc";
            SqlCommand cmd = new SqlCommand(query, Connection);
            cmd.Parameters.AddWithValue("@ticketid", id);
            Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Message msg = new Message();
                    msg.Id = (int)reader["id"];
                    msg.Body = reader["body"].ToString();
                    msg.HtmlBody = reader["html_body"].ToString();
                    msg.Author = reader["author"].ToString();
                    msg.Email = reader["email"].ToString();
                    msg.Created = DateTime.Parse(reader["created_at"].ToString());
                    msg.UserId = (int)reader["user_id"];
                    messages.Add(msg);
                }
            }
            Connection.Close();
            return messages;
        }

        public Ticket GetSingleTicket(int id)
        {
            var query = $@"SELECT t1.[id],
                                    [title],
                                    [X_PRODUCT_LICENSE],
                                    [created_at],
                                    t1.[last_changed],
                                    [created_by],
                                    [author],
                                    [owned_by],
                                    [category],
                                    t1.[status],
		                            [ticket_status],
		                            [priority],
		                            [deadline],
		                            [filter_address], 							 
		                            [X_SAGSTYPE],
		                            t1.[X_PARENT],
		                            [X_ENVIRONMENT],
		                            [x_timelog_project],
		                            [x_project],
		                            [x_produkt_version],
		                            [x_estimat_opr],
		                            [x_estimat_rest], 
		                            [x_tag],
		                            [x_tag2],
		                            [x_tag3],
		                            [name] as project_name,
		                            a1.[email] as owned_by_email, 
		                            a2.[email] as created_by_email, 
		                            a3.[X_PARENT] as parent_sag,
                                    [x_planned_for_version],
		                            [x_solved_in_version]
									x_planned_for_version,
									x_solved_in_version,
									y_release_notes.x_description,
									y_release_notes.x_type
                                FROM[SuperOffice].[CRM7].[TICKET] t1
                                    FULL OUTER JOIN[SuperOffice].[CRM7].[PROJECT] ON t1.[x_project] = [SuperOffice].[CRM7].[PROJECT].[project_id]
                                    FULL OUTER JOIN[SuperOffice].[CRM7].[EJUSER] a1 ON a1.[id] = t1.[owned_by]
                                    FULL OUTER JOIN[SuperOffice].[CRM7].[EJUSER] a2 ON a2.[id] = t1.[created_by]
                                    FULL OUTER JOIN (SELECT distinct [X_PARENT] FROM [SuperOffice].[CRM7].[TICKET] t2 WHERE X_SAGSTYPE in ('Problem', 'Incident')) a3 ON a3.[X_PARENT] = t1.id
									LEFT OUTER JOIN y_release_notes ON 
									y_release_notes.x_sagsnummer = t1.id
                                    WHERE t1.id = {id}";


            SqlConnection Connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, Connection);
            Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            Ticket ticket = new Ticket();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ticket.Id = (int)reader["id"];
                    ticket.Title = (string)reader["title"];
                    ticket.CreatedBy = new User((int)reader["owned_by"], (string)reader["author"], (string)reader["owned_by_email"], (string)reader["created_by_email"]);
                    ticket.ProduktCategory = (int)reader["category"];

                    if (reader["project_name"] != DBNull.Value)
                    {
                        ticket.Project = (string)reader["project_name"];
                    }
                    ticket.Priority = (int)reader["priority"];
                    ticket.State = (int)reader["ticket_status"];
                    ticket.CreatedAt = DateTime.Parse(reader["created_at"].ToString());
                    ticket.Estimate = DBNull.Value == reader["x_estimat_opr"] ? 0 : (int)reader["x_estimat_opr"];
                    ticket.EstimateRest = DBNull.Value == reader["x_estimat_rest"] ? 0 : (int)reader["x_estimat_rest"];
                    ticket.Timelog = DBNull.Value == reader["x_timelog_project"] ? "" : (string)reader["x_timelog_project"];
                    if (reader["X_SAGSTYPE"].ToString().ToLower() == "request for change")
                    {
                        ticket.CaseType = CaseTypeEnum.RequestForChange;
                    }
                    else if (reader["X_SAGSTYPE"].ToString().ToLower() == "service request")
                    {
                        ticket.CaseType = CaseTypeEnum.ServiceRequest;
                    }
                    else
                    {
                        ticket.CaseType = CaseTypeEnum.None;
                    }
                    if (reader["X_PRODUCT_LICENSE"] != DBNull.Value && licences.ContainsKey((int)reader["X_PRODUCT_LICENSE"]))
                    {
                        ticket.Licence = licences[(int)reader["X_PRODUCT_LICENSE"]];
                    }
                    if (reader["deadline"] != DBNull.Value)
                    {
                        ticket.Deadline = DateTime.Parse(reader["deadline"].ToString());
                    }
                    ticket.Tag1 = reader["x_tag"].ToString();
                    ticket.Tag2 = reader["x_tag2"].ToString();
                    ticket.Tag3 = reader["x_tag3"].ToString();
                    ticket.ResolvedInVersion = reader["x_solved_in_version"].ToString();
                    ticket.PlannedInVersion = reader["x_planned_for_version"].ToString();
                    ticket.ReleaseNoteDescription = reader["x_description"].ToString();

                    var releaseNote = reader["x_type"].ToString();

                    if (releaseNote == "Nyheder")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "New");
                    if(releaseNote == "Rettelse")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Correction");
                    if (releaseNote == "Ændring")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Change");
                    if (releaseNote == "Andet")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Other");
                    if (releaseNote == "Ignorer")
                        ticket.ReleaseNoteType = (ReleaseNotes)Enum.Parse(typeof(ReleaseNotes), "Ignore");

                    ticket.Messages = GetMessages(ticket.Id);
                }
            }

            Connection.Close();
            return ticket;
        }

        public async Task<List<TimelogData>> GetTimeLogData()
        {
            using (SqlConnection sql = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = sql;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT[name],[status_idx],project_number,project_id FROM[Superoffice].[CRM7].[PROJECT]";

                    var response = new List<TimelogData>();

                    await sql.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response.Add(MapToTimelogValue(reader));
                        }
                    }

                    return response;
                };
            };
        }

        private TimelogData MapToTimelogValue(SqlDataReader reader)
        {
            return new TimelogData
            {
                Name = reader["name"].ToString(),
                StatusId = (int)reader["status_idx"],
                ProjectNumber = reader["project_number"].ToString(),
                ProjectId = (int)reader["project_id"]
            };
        }
    }
}

