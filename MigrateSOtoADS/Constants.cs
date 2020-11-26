using System.Collections.Generic;

namespace MigrateSOtoADS.Superoffice
{
    public static class Constants
    {
        private const string PORTMAN = "7.25 Leverance TEST";
        private const string IDEAS = "IDEAS 7.35";
        private const string SUPERPORT = "Superport_Test";
        private const string DATA = "Data";

        public static Dictionary<int, string> Container { get; set; } = new Dictionary<int, string>
        {
            {3, PORTMAN },
            {20, PORTMAN },
            {21, PORTMAN },
            {22, PORTMAN },
            {26, PORTMAN },
            {28, PORTMAN },

            {2, IDEAS },
            {23, IDEAS },
            {24, IDEAS },
            
            {4, SUPERPORT },

            {7, DATA }
        };

        public static string URL_TFS_VITEC { get; set; } = "https://tfs.vitec.se/tfs/aloc/";
        public static string URL_TFS_TEST_VTEC { get; set; } = "https://tfstest.vitec.se/tfs/aloc/";

        public static string SCRUM { get; set; } = "";

        public static string CMMI { get; set; } = "Requirement";
        public static string AGILE { get; set; } = "Product Backlog Item";

    }
}
