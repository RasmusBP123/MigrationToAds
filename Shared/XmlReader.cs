using Shared.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class XmlReader
    {
        string _path;
        public XmlReader(string path)
        {
            _path = path;
        }

        public List<string> GetColumns(CsvColumns column)
        {
            var rows = new List<string>();

            using (var reader = new StreamReader(_path, Encoding.UTF8))
            {
                var file = File.ReadAllLines(_path);
                var lines = file.Select(x => x.Split(";"));

                while (!reader.EndOfStream)
                {
                    var list = reader.ReadLine();
                    var values = list.Split(';');

                    if (!string.IsNullOrWhiteSpace(values[(int)column]))
                    {
                        rows.Add(values[(int)column]);
                    }
                }
            }

            //Two first rows are names of the column and we will skip them
            return rows.Skip(2).ToList();
        }
    }
}
