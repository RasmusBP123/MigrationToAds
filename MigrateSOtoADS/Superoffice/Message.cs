using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateSOtoADS.Superoffice
{
    class Message
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string HtmlBody { get; set; }
        public string Author { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
    }
}
