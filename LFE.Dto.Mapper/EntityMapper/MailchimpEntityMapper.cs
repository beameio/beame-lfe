using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class MailchimpEntityMapper
    {
        public static CHIMP_UserLists Token2UserListEntity(this ChimpUserListDTO token)
        {
            return new CHIMP_UserLists
            {
                AddOn     = DateTime.Now,
                ApiKey    = token.ApiKey,
                CreatedBy = token.UserId,
                Uid       = token.Uid,
                IsActive  = true,
                UserId    = token.UserId
            };
        }

        public static void UpdateUserListEntity(this CHIMP_UserLists entity, int totalSubscribers)
        {
            entity.TotalSubscribers      = totalSubscribers;
            entity.SubscribersLastUpdate = DateTime.Now;
        }
        public static void UpdateSegemntEntity(this CHIMP_ListSegments entity, int totalSubscribers)
        {
            entity.TotalSubscribers      = totalSubscribers;
            entity.SubscribersLastUpdate = DateTime.Now;
        }
        public static CHIMP_ListSegments Token2SegmentEntity(this CHIMP_MissingSegmentToken token, ChimpSegmentNameToken nameToken)
        {
            return new CHIMP_ListSegments
            {
                AddOn         = DateTime.Now,
                ListId        = token.ListId,
                SegmentTypeId = (byte)nameToken.SegmentType,
                Uid           = nameToken.Uid,
                CourseId      = token.ItemTypeId == 1 ? token.ItemId : null,
                BundleId      = token.ItemTypeId == 2 ? token.ItemId : null,
                Name          = nameToken.Name
            };
        }

        public static CHIMP_ListSegments Token2SegmentEntity(this ChimpListSegmentDTO token)
        {
            return new CHIMP_ListSegments()
            {

            };
        }

    }
}
