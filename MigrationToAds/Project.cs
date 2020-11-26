using System;
using System.Collections.Generic;
using System.Text;

namespace Timelog.Projects
{
    public class Project
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public List<string> Tasks { get; set; } = new List<string>();
    }
}
