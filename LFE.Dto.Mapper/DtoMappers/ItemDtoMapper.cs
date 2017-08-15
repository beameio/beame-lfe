using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class ItemDtoMapper
    {
        public static ItemInfoToken BundleEntity2ItemInfoToken(this CRS_Bundles entity,Users author, bool isUnderRGP)
        {
            return new ItemInfoToken
            {
                 ItemId                  = entity.BundleId
                ,ItemName                = entity.BundleName
                ,ItemType                = BillingEnums.ePurchaseItemTypes.BUNDLE       
                ,Uid                     = entity.uid
                ,IntroHtml               = entity.BundleDescription
                ,OverviewVideoIdentifier = string.IsNullOrEmpty(entity.OverviewVideoIdentifier) ? (long?) null : long.Parse(entity.OverviewVideoIdentifier)
                ,ThumbUrl                = entity.BannerImage.ToThumbUrl(Constants.ImageBaseUrl) 
                ,Author                  = new BaseUserInfoDTO
                                            {
                                                UserId = author.Id
                                                ,FullName = author.Entity2FullName()
                                            }
                ,IsAuthorUnderRGP              = isUnderRGP
            };
        }

        public static ItemInfoToken CourseEntity2ItemInfoToken(this Courses entity,Users author, bool isUnderRGP)
        {
            return new ItemInfoToken
            {
                 ItemId                  = entity.Id
                ,ItemName                = entity.CourseName
                ,ProvisionUid            = entity.ProvisionUid
                ,ItemType                = BillingEnums.ePurchaseItemTypes.COURSE
                ,Uid                     = entity.uid
                ,IntroHtml               = entity.Description
                ,Rating                  = entity.Rating ?? 0
                ,IsFreeItem              = entity.IsFreeCourse
                ,DisplayOtherLearnersTab = entity.DisplayOtherLearnersTab
                ,ClassRoomId             = entity.ClassRoomId
                ,OverviewVideoIdentifier = string.IsNullOrEmpty(entity.OverviewVideoIdentifier) ? (long?) null : long.Parse(entity.OverviewVideoIdentifier)
                ,ThumbUrl                = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl) 
                ,Author                  = new BaseUserInfoDTO
                                            {
                                                UserId = author.Id
                                                ,FullName = author.Entity2FullName()
                                            }
                ,IsAuthorUnderRGP              = isUnderRGP                
            };
        }

        public static RenditionDTO Entity2RenditionDto(this USER_VideosRenditions token)
        {
            return new RenditionDTO
            {
                RenditionId     = token.RenditionId
               // ,S3Url          = token.S3Url
                ,CloudFrontPath = token.CloudFrontPath.Replace("http://uservideos.lfe.com/",Constants.S3_CLOUDFRONT_ROOT)                
                ,VideoCodec     = token.VideoCodec
                ,VideoContainer = token.VideoContainer
                ,FrameHeight    = token.FrameHeight
                ,FrameWidth     = token.FrameWidth
                ,EncodingRate   = token.EncodingRate
            };
        }

        public static ItemInfoToken ItemEntity2ItemInfoToken(this WebStoreItems entity, Users author, bool IsUnderRGP)
        {
            var type        = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId);
            var itemId      = -1;
            int? raiting    = null;
            string intro    = null;
            var isFree      = false;
            int? roomId     = null;
            long? videoId   = null;
            string thumbUrl = null;
            Guid? uid       = null;
            long videoIdentifierId;
            var isPublished = false;
            switch (type)
            {
                case BillingEnums.ePurchaseItemTypes.COURSE:
                    var courseEntity = entity.Courses;                    
                    itemId   = entity.CourseId ?? -1;
                    uid      = entity.Courses.uid;
                    raiting  = courseEntity.Rating;
                    intro    = courseEntity.Description;
                    isFree   = courseEntity.IsFreeCourse;
                    roomId   = courseEntity.ClassRoomId;
                    videoId  = long.TryParse(courseEntity.OverviewVideoIdentifier, out videoIdentifierId) ? videoIdentifierId : (long?)null;
                    thumbUrl = courseEntity.SmallImage;
                    isPublished = courseEntity.StatusId == (byte) CourseEnums.CourseStatus.Published;
                    break;
               
                case BillingEnums.ePurchaseItemTypes.BUNDLE:
                    var bundleEntity = entity.CRS_Bundles;
                    uid      = entity.CRS_Bundles.uid;
                    itemId   = entity.BundleId ?? -1;
                    intro    = entity.CRS_Bundles.BundleDescription;
                    videoId  = long.TryParse(entity.CRS_Bundles.OverviewVideoIdentifier, out videoIdentifierId) ? videoIdentifierId : (long?)null;
                    thumbUrl = bundleEntity.BannerImage;
                    isPublished = bundleEntity.StatusId == (byte)CourseEnums.CourseStatus.Published;
                    break;
            }

            return new ItemInfoToken
            {
                 ItemId                  = itemId
                ,ItemName                = entity.ItemName
                ,ItemType                = type
                ,Uid                     = uid
                ,IntroHtml               = intro
                ,Rating                  = raiting ?? 0
                ,IsFreeItem              = isFree
                ,IsPublished             = isPublished
                ,ClassRoomId             = roomId
                ,OverviewVideoIdentifier = videoId
                ,ThumbUrl                = thumbUrl.ToThumbUrl(Constants.ImageBaseUrl) 
                ,Author                  = new BaseUserInfoDTO
                                            {
                                                UserId = author.Id
                                                ,FullName = author.Entity2FullName()
                                            }
                ,IsAuthorUnderRGP              = IsUnderRGP
            };
        }
        public static ItemProductPageToken ItemInfoToken2ItemProductPageToken(this ItemInfoToken token,ItemAccessStateToken accessState,string trackingId,int subscribers)
        {
            return new ItemProductPageToken
            {
                 ItemId                  = token.ItemId
                ,ProvisionUid            = token.ProvisionUid
                ,ItemName                = token.ItemName
                ,ItemType                = token.ItemType
                ,Uid                     = token.Uid
                ,IntroHtml               = token.IntroHtml
                ,Rating                  = token.Rating
                ,IsFreeItem              = token.IsFreeItem
                ,ClassRoomId             = token.ClassRoomId
                ,OverviewVideoIdentifier = token.OverviewVideoIdentifier
                ,Author                  = token.Author
                ,ItemState               = accessState
                ,TrackingID              = trackingId
                ,NumOfSubscribers        = subscribers
                ,ThumbUrl                = token.ThumbUrl
                ,IsAuthorUnderRGP        = token.IsAuthorUnderRGP                
                ,VideoInfoToken          = token.VideoInfoToken
            };
        }

        public static ItemViewerPageToken ItemInfoToken2ItemViewerPageToken(this ItemInfoToken token, ItemAccessStateToken accessState, string trackingId)
        {
            return new ItemViewerPageToken
            {
                 ItemId                  = token.ItemId
                ,ProvisionUid            = token.ProvisionUid
                ,ItemName                = token.ItemName
                ,ItemType                = token.ItemType
                ,Uid                     = token.Uid
                ,IntroHtml               = token.IntroHtml
                ,Rating                  = token.Rating
                ,IsFreeItem              = token.IsFreeItem
                ,ClassRoomId             = token.ClassRoomId
                ,OverviewVideoIdentifier = token.OverviewVideoIdentifier
                ,Author                  = token.Author                
                ,TrackingID              = trackingId
                ,ThumbUrl                = token.ThumbUrl
                ,ItemState               = accessState
                ,IsAuthorUnderRGP        = token.IsAuthorUnderRGP
                ,DisplayOtherLearnersTab = token.DisplayOtherLearnersTab
                ,VideoInfoToken          = token.VideoInfoToken
            };
        }
    }
}
