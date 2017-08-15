using System;

namespace LFE.DataTokens
{
    #region dashboard tokens
    public class AuthorStatisticSummaryDTO
    {
        public AuthorStatisticSummaryDTO(int id)
        {
            userId   = id;
            courses  = 0;
            bundles  = 0;
            stores = 0;
            comments = 0;
        }
        public AuthorStatisticSummaryDTO()
        {
            courses  = 0;
            bundles  = 0;
            stores   = 0;
            comments = 0;
        }

        public int userId { get; set; }
        public int courses { get; set; }
        public int bundles { get; set; }
        public int stores { get; set; }
        public int comments { get; set; }
    }

    public class CommentViewDTO
    {
        public int commentId { get; set; }
        public string Comment { get; set; }
        public string CourseName { get; set; }
        public DateTime? date { get; set; }
    }

    public class SalesAnalyticChartDTO
    {
        public SalesAnalyticChartDTO(){}

        public SalesAnalyticChartDTO(DateTime datePoint, decimal? total)
        {
            date       = datePoint;
            this.total = total ?? 0;
        }
        public DateTime date { get; set; }
        public decimal total { get; set; }
    }

    public class AuhorRefundProgramDTO
    {
        public AuhorRefundProgramDTO()
        {
            IsValid = false;
        }

        public bool Checked { get; set; }
        public bool IsValid { get; set; }
        public string Error { get; set; }
        public bool JoinedToRefundProgram { get; set; }
    }
   
    #endregion

    public class VideoUploadToken
    {
        public Guid? uid { get; set; }
        public long? bcId { get; set; }
        public string fileName { get; set; }
        public string contentType { get; set; }
        public long? size { get; set; }
        public string tags { get; set; }

    }

}
