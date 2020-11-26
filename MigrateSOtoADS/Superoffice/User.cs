using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateSOtoADS.Superoffice
{
    class User
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            private set { _email = value; }
        }

        private string _emailoriginal;

        public string EmailOriginal
        {
            get { return _emailoriginal; }
            set { _emailoriginal = value; }
        }

        public User(int id, string name, string email, string created_by_original)
        {
            Id = id;
            Name = name;
            Email = email;
            EmailOriginal = created_by_original;
        }

    }
}
