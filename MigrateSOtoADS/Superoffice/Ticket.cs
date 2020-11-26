using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateSOtoADS.Superoffice
{
    public enum CaseTypeEnum
    {
        None,
        RequestForChange,
        ServiceRequest
    }
    public enum ProductsEnum
    {
        Ideas,
        Portman,
        Superport
    }
    class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Licence Licence { get; set; }
        public int ProduktCategory { get; set; }
        public CaseTypeEnum CaseType { get; set; }
        public int State { get; set; }
        public User CreatedBy { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Project { get; set; }
        public string Timelog { get; set; }
        public int Estimate { get; set; }
        public int EstimateRest { get; set; }
        public string PlannedInVersion { get; set; }
        public string ResolvedInVersion { get; set; }
        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string Tag3 { get; set; }
        public List<Message> Messages{ get; set; }
        public string Description { get; set; }
        public ReleaseNotes ReleaseNoteType { get; set; }
        public string ReleaseNoteDescription { get; set; }
    }

    public enum ReleaseNotes
    {
        New,
        Correction,
        Change,
        Other,
        Ignore
    }
}
