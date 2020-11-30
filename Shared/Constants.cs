using System.Collections.Generic;

namespace Shared.Utilities
{
    public static class Constants
    {
        public static string URL_TFS_VITEC { get; set; } = "https://tfs.vitec.se/tfs/aloc/";
        public static string URL_TFS_TEST_VTEC { get; set; } = "https://tfstest.vitec.se/tfs/aloc/";

        public static string SCRUM { get; set; } = "";

        public static string CMMI { get; set; } = "Requirement";
        public static string AGILE { get; set; } = "Product Backlog Item";

    }
    public enum CsvColumns : int
    {
        TimelogProjectIds = 0,
        TimeLogProjectName = 1,
        SuperOfficeIds = 2, 
        SuperOfficeProjectNames = 3,
        AdsProjectName = 4,
        AdsArea = 5,
        AdsIteration = 6
    }

    public enum Products
    {
        PORTMAN,
        IDEAS,
        DATA,
        SUPERPORT,
    }
}
