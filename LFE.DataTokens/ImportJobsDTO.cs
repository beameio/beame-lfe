using System;
using LFE.Core.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace LFE.DataTokens
{
    public class ImportChapterToken
    {
        public ImportChapterToken()
        {
            Videos = new List<ImportFileToken>();
            Documents = new List<ImportFileToken>();
        }

        [DisplayName("Chapter Name")]
        public string Name { get; set; }

        [AllowHtml]
        public string Description { get; set; }

        public string Path { get; set; }

        public int? ChapterId { get; set; }

        public string RefChapterId { get; set; }

        public List<ImportFileToken> Videos { get; set; }

        public List<ImportFileToken> Documents { get; set; }
    }

    public class ImportFileToken
    {
        public ImportJobsEnums.eAssetTypes Type { get; set; }


        public int? ChapterId { get; set; }
     

        public string Name { get; set; }

        public string Path { get; set; }

        public int? Index { get; set; }
        
    }

    public class ImportJobFileDTO
    {
        public int FileId { get; set; }
        public int JobId { get; set; }
        public ImportJobsEnums.eAssetTypes Type { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public long? BcIdentifier { get; set; }
        public string SavedToPath { get; set; }        
        public ImportJobsEnums.eFileInterfaceStatus Status { get; set; }
        public DateTime AddOn { get; set; }
        public DateTime? DownloadOn { get; set; }
        public DateTime? UploadOn { get; set; }
        public DateTime? UpdateOn { get; set; }
        public DateTime? SyncOn { get; set; }
    }
}
