using System.Collections.Generic;
using System.Text;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Model;
using MailChimp.Helper;
using MailChimp.Lists;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class MailchimpDtoMapper
    {
        public static ChimpUserListDTO Entity2UserListDto(this CHIMP_UserLists entity)
        {
            return new ChimpUserListDTO
            {
                AddOn                 = entity.AddOn,
                ApiKey                = entity.ApiKey,
                IsActive              = entity.IsActive,
                ListId                = entity.ListId,
                SubscribersLastUpdate = entity.SubscribersLastUpdate,
                Uid                   = entity.Uid,
                UserId                = entity.UserId,
                TotalSubscribers      = entity.TotalSubscribers 
            };
        }

        public static ChimpListSegmentDTO Entity2SegmentDto(this CHIMP_ListSegments entity)
        {
            return new ChimpListSegmentDTO
            {
                AddOn                 = entity.AddOn,
                Name                  = entity.Name,
                BundleId              = entity.BundleId,
                CourseId              = entity.CourseId,
                ListId                = entity.ListId,
                SegmentId             = entity.SegmentId,
                SegmentType           = Utils.ParseEnum<eSegmentTypes>(entity.SegmentTypeId),
                SubscribersLastUpdate = entity.SubscribersLastUpdate,
                Uid                   = entity.Uid,
                TotalSubscribers      = entity.TotalSubscribers
            };
        }

        public static EmailParameter GetEmailParameter(this string email)
        {
            return new EmailParameter { Email = email };
        }

        public static BatchEmailParameter GetBatchEmailParameter(this CHIMP_ItemSubscribers entity)
        {
            return new BatchEmailParameter
            {
                Email = GetEmailParameter(entity.Email),
                EmailType = "html",
                MergeVars = new { fname = entity.FirstName, lname = entity.LastName }
            };
        }

        public static IEnumerable<ChimpSegmentNameToken> ToSegmentNameTokenList(this CHIMP_MissingSegmentToken token)
        {
            var nameList = new List<ChimpSegmentNameToken>();
            var type = Utils.ParseEnum<eSegmentTypes>(token.SegmentTypeId);
            switch (type)
            {
                case eSegmentTypes.Active:
                    nameList.Add(new ChimpSegmentNameToken {Name = "My active learners", SegmentType = type});
                    break;
                case eSegmentTypes.InActive:
                    nameList.Add(new ChimpSegmentNameToken {Name = "My non active learners", SegmentType = type});
                    break;
                default:
                    nameList.Add(new ChimpSegmentNameToken { Name = string.Format("{0} Subscribers", token.ItemName), SegmentType = eSegmentTypes.Item });
                    nameList.Add(new ChimpSegmentNameToken { Name = string.Format("{0} New learners", token.ItemName), SegmentType = eSegmentTypes.ItemNew });
                    break;
            }
            return nameList;
        }
    
    }
}
