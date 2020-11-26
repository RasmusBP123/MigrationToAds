using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Timelog.Projects;

namespace Timelog
{
    public class TimelogDataAccessor
    {
        HttpClient client;
        public TimelogDataAccessor()
        {
            client = new HttpClient();
        }

        public async Task<List<Project>> GetTimelogProjectData()
        {
            var url = @"https://app3.timelog.com/vitec/service.asmx/GetProjectsShortList?sitecode=3a933f7f6a424f4585a53c469&apiID=vitecaloc&apiPassword=5a53c469&status=1&customerID=-1&projectManagerId=-1";

            var result = await client.GetAsync(url);
            var text = await result.Content.ReadAsStringAsync();

            var projectsRawList = XDocument.Parse(text);

            var projectids = new List<string>();
            var name = projectsRawList.Root.Name.Namespace;


            var nodes = projectsRawList.Root.Elements(name + "Project");

            foreach (var node in nodes)
            {
                var id = node.FirstAttribute;
                projectids.Add(id.Value);
            }

            var projects = new List<Project>();
            foreach (var projectId in projectids)
            {
                var result2 = await client.GetAsync($@"https://app3.timelog.com/vitec/service.asmx/GetProjectsRaw?sitecode=3a933f7f6a424f4585a53c469&apiID=vitecaloc&apiPassword=5a53c469&projectID={projectId}&status=1&customerID=-1&projectManagerId=-1");

                var projectRaw = XDocument.Parse(await result2.Content.ReadAsStringAsync());


                var node = projectRaw.Root.Descendants(name + "Project");
                var typeId = node.Elements(name + "ProjectTypeID").FirstOrDefault().Value;
                var projectName = node.Elements(name + "Name").FirstOrDefault().Value;

                var project = new Project
                {
                    ProjectId = projectId,
                    TypeId = typeId,
                    Name = projectName
                };

                var result3 = await client.GetAsync($@"https://app3.timelog.com/vitec/service.asmx/GetTasksRaw?sitecode=3a933f7f6a424f4585a53c469&apiID=vitecaloc&apiPassword=5a53c469&taskID=-1&projectID={projectId}&status=1&taskTypeID=0");

                var taskRaw = XDocument.Parse(await result3.Content.ReadAsStringAsync());

                var tasks = taskRaw.Root.Elements(name + "Task");

                foreach (var task in tasks)
                {
                    var taskId = task.Element(name + "Name").Value;
                    project.Tasks.Add(taskId);

                }

                projects.Add(project);
            }

            return projects;
        }
    }
}
