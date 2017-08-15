using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class InterfaceFileDTO
    {
        public int fileId { get; set; }
        public int userId { get; set; }
        public ImportJobsEnums.eFileInterfaceStatus status { get; set; }        
        public string name { get; set; }
        public long? bcIdentifier { get; set; }
        public string eTag { get; set; }
        public string contentType { get; set; }
        public string title { get; set; }

        public string tags { get; set; }
    }
}
