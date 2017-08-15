using System;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class ReportDtoMapper
    {
        public static SummaryReportRowDTO Entity2ReportRowDto(this ADMIN_SummaryReportToken entity)
        {
            return new SummaryReportRowDTO
            {
                Period   = entity.period
                ,Users   = entity.users
                ,Courses = entity.courses
                ,Authors = entity.authors
                ,Sales   = entity.total.FormatMoney(2)
            };
        }

      

        public static FactDailyStatsDTO Entity2FactDailyStatsDto(this FACT_DailyStats entity)
        {
            return new FactDailyStatsDTO
            {
                FactDate            = entity.FactDate
                ,ItemsCreated       = entity.ItemsCreated
                ,ItemsPublished     = entity.ItemsPublished
                ,UsersCreated       = entity.UsersCreated
                ,WixUsersCreated    = entity.WixUsersCreated
                ,UserLogins         = entity.UserLogins
                ,AuthorLogins       = entity.AuthorLogins
                ,ReturnUsersLogins  = entity.ReturnUsersLogins
                ,StoresCreated      = entity.StoresCreated
                ,WixStoresCreated   = entity.WixStoresCreated
                ,ItemsPurchased     = entity.ItemsPurchased
                ,FreeItemsPurchased = entity.FreeItemsPurchased                
            };
        }

        public static FactDailyTotalsDTO Entity2FactDailyTotalsDto(this FACT_DailyTotals entity)
        {
            return new FactDailyTotalsDTO
            {
                FactDate            = entity.FactDate
                ,TotalItems         = entity.TotalItems
                ,TotalPublished     = entity.TotalPublished
                ,TotalUsers         = entity.TotalUsers
                ,TotalAuthors       = entity.TotalAuthors
                ,TotalLearners      = entity.TotalLearners
                ,Attached2Stores    = entity.Attached2Stores
                ,Attached2WixStores = entity.Attached2WixStores
                ,StoresCreated      = entity.StoresCreated
                ,WixStoresCreated   = entity.WixStoresCreated
                ,ItemsPurchased     = entity.ItemsPurchased
                ,FreeItemsPurchased = entity.FreeItemsPurchased
            };
        }

        public static VideoDTO UserVideoEntity2UserVideoDto(this USER_Videos entity, Users user)
        {
            return new VideoDTO
            {
                user        = new BaseUserInfoDTO
                                                {
                                                    UserId = user.Id
                                                    ,FullName = user.Entity2FullName()
                                                }
               ,videoId     = entity.VideoId
               ,identifier  = entity.BcIdentifier
               ,name        = entity.Name
               ,inUse       = entity.Attached2Chapter
               ,addon       = entity.CreationDate
               ,duration    = entity.Duration
               ,thumb       = entity.ThumbUrl
            };
           
        }

        public static VideoDTO UserInfoVideoEntity2UserVideoDto(this USER_VideoInfoToken entity)
        {
            return new VideoDTO
            {
                user        = new BaseUserInfoDTO
                                                {
                                                    UserId = entity.UserId
                                                    ,FullName = entity.Entity2FullName()
                                                }
               ,videoId     = entity.VideoId
               ,identifier  = entity.BcIdentifier
               ,name        = entity.Name
               ,inUse       = entity.Attached2Chapter
               ,addon       = entity.CreationDate
               ,duration    = entity.Duration
               ,thumb       = entity.ThumbUrl
            };
           
        }

       
        public static StoreReportDTO StoreViewEntity2StoreReportDto(this vw_WS_StoresLib entity)
        {
            return new StoreReportDTO
            {
                StoreId              = entity.StoreID
                ,TrackingID          = entity.TrackingID
                ,Name                = entity.StoreName
                ,TotalOwnedItems     = entity.OwnedItems
                ,TotalAffiliateItems = entity.AffiliateItems
                ,TotalItems          = entity.OwnedItems + entity.AffiliateItems
                ,IsOwnerAdmin        = entity.IsOwnerAdmin ?? false
                ,IsAffiliate         = entity.AffiliateItems > 0
                ,RegistrationSource  = entity.RegistrationSourceId != null ? Utils.ParseEnum<CommonEnums.eRegistrationSources>(entity.RegistrationSourceId.ToString()) : CommonEnums.eRegistrationSources.Unknown
                ,SiteUrl             = entity.SiteUrl ?? entity.WixSiteUrl
                ,AddOn               = entity.AddOn
                ,Owner               = new BaseUserInfoDTO
                {
                    Email = entity.OwnerEmail
                    ,UserId = entity.OwnerUserID
                    ,FullName = entity.Entity2FullName()
                }
            };
        }

        public static AbandonHostDTO Entity2HostEventStatsDto(this FACT_AbandonHostToken entity)
        {
            return new AbandonHostDTO
            {
                HostName        = entity.HostName
                ,FirstEventDate = entity.FirstEventDate
                ,LastEventDate  = entity.LastEventDate
                ,PreviewCount   = entity.PreviewCount ?? 0
                ,TotalCourses   = entity.TotalCourses
                ,User           = new BaseUserInfoDTO
                {
                    UserId = entity.UserId
                    ,Email = entity.Email
                    ,FullName = entity.FirstName + " " + entity.LastName
                }
                ,TotalEvents = entity.TotalEvents ?? 0
            };
        }

        public static HostEventDTO Entity2HostEventDto(this FACT_HostEventToken entity)
        {
            return new HostEventDTO
            {
                Period                = entity.period
                ,HostName             = entity.HostName
                ,BUY_PAGE_ENTERED     = entity.BUY_PAGE_ENTERED
                ,CHECKOUT_REGISTER    = entity.CHECKOUT_REGISTER
                ,COURSE_PREVIEW_ENTER = entity.COURSE_PREVIEW_ENTER
                ,COURSE_VIEWER_ENTER  = entity.COURSE_VIEWER_ENTER
                ,PURCHASE_COMPLETE    = entity.PURCHASE_COMPLETE
                ,REGISTRATION_SUCCESS = entity.REGISTRATION_SUCCESS
                ,STORE_VIEW           = entity.STORE_VIEW
                ,TotalEvents          = entity.BUY_PAGE_ENTERED + entity.REGISTRATION_SUCCESS + entity.COURSE_PREVIEW_ENTER + entity.PURCHASE_COMPLETE + entity.STORE_VIEW
            };
        }

        public static short RegistrationSource2AdminDashboardIndex(this CommonEnums.eRegistrationSources source)
        {
            switch (source)
            {
                case CommonEnums.eRegistrationSources.LFE:
                    return 1;
                case CommonEnums.eRegistrationSources.WIX:
                    return 2;
                case CommonEnums.eRegistrationSources.FB:
                    return 3;
                case CommonEnums.eRegistrationSources.WORDPRESS:
                    return 4;
                default:
                    return 999;
            }
        }

        public static TendencyToken Value2TendencyToken(this int value1, int? value2)
        {
            var tendency = new TendencyToken();

            if (value2 == null)
            {
                tendency.Direction = ReportEnums.eTendencyDirections.Up;
                tendency.Percent = 100;
            }
            else
            {
                var difference = value1 - value2;

                if (difference == 0)
                {
                    tendency.Direction = ReportEnums.eTendencyDirections.Equal;
                    tendency.Percent = 0;
                }
                else
                {
                    tendency.Direction = difference > 0 ? ReportEnums.eTendencyDirections.Up : ReportEnums.eTendencyDirections.Down;

// ReSharper disable once PossibleLossOfFraction
                    tendency.Percent = (value2 > 0 ? Math.Abs(100 - Math.Round( (decimal)(value1 * 100 / value2), 0)) : 0) * (difference > 0 ? 1 : -1);
                }
            }

            tendency.Tooltip = (value2 ?? 0).ToString();

            return tendency;
        }

        public static TendencyToken Value2TendencyToken(this decimal value1, decimal? value2)
        {
            var tendency = new TendencyToken();

            if (value2 == null)
            {
                tendency.Direction = ReportEnums.eTendencyDirections.Up;
                tendency.Percent = 100;
            }
            else
            {
                var difference = value1 - value2;

                if (difference == 0)
                {
                    tendency.Direction = ReportEnums.eTendencyDirections.Equal;
                    tendency.Percent = 0;
                }
                else
                {
                    tendency.Direction = difference > 0 ? ReportEnums.eTendencyDirections.Up : ReportEnums.eTendencyDirections.Down;

                    // ReSharper disable once PossibleLossOfFraction
                    tendency.Percent = (value2 > 0 ? Math.Abs(100 - Math.Round((decimal)(value1 * 100 / value2), 0)) : 0) * (difference > 0 ? 1 : -1);
                }
            }
            tendency.Tooltip = (value2 != null ? value2.FormatdDecimal(2) : 0).ToString();
            return tendency;
        }
    }
}
