using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class ChimpUserListDTO : BaseModelState
    {
        public ChimpUserListDTO()
        {
            ListId = -1;
        }
        
        public int ListId { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "ApiKey required")]
        public string ApiKey { get; set; }
        [Required(ErrorMessage = "ListId required")]
        public string Uid { get; set; }
        public bool IsActive { get; set; }
        public DateTime AddOn { get; set; }
        public DateTime? SubscribersLastUpdate { get; set; }
        public List<ChimpListSegmentDTO> Segments { get; set; }
        public int TotalSubscribers { get; set; }
    }

    public class ChimpListSegmentDTO
    {
        public int SegmentId { get; set; }
        public int ListId { get; set; }
        public int TotalSubscribers { get; set; }
        public string Name { get; set; }
        public eSegmentTypes SegmentType { get; set; }
        public string Uid { get; set; }
        public int? CourseId { get; set; }
        public int? BundleId { get; set; }
        public DateTime AddOn { get; set; }
        public DateTime? SubscribersLastUpdate { get; set; }        
    }

    public class ChimpSegmentNameToken
    {
        public string Name { get; set; }
        public eSegmentTypes SegmentType { get; set; }

        public string Uid { get; set; }
    }
}
