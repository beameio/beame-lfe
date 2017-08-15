using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class CourseDtoMapper
    {
        public static ItemListDTO UserItemEntity2ItemListDto(this USER_ItemToken entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId.ToString());

            return new ItemListDTO
            {
                 ItemId                   = entity.ItemId
                ,ItemName                 = entity.ItemName
                ,ItemType                 = type
                ,Description              = entity.ItemDescription
                ,AuthorName               = entity.Entity2AuthorFullName()
                ,AuthorId                 = entity.AuthorId
                ,Price                    = entity.Price
                ,MonthlySubscriptionPrice = entity.MonthlySubscriptionPrice
                ,AffiliateCommission      = entity.AffiliateCommission
                ,Status                   = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId)
                ,ItemPageUrl              = type == BillingEnums.ePurchaseItemTypes.COURSE ? entity.GenerateCoursePageUrl(entity.Entity2AuthorFullName(), entity.ItemName,null) : entity.GenerateBundlePageUrl(entity.Entity2AuthorFullName(), entity.ItemName,null)
                ,SubscribersCount         = entity.NumSubscribers
                ,AddOn                    = entity.Created
                ,IsFree                   = entity.IsFreeCourse ?? true                     
            };
        }
        public static CourseInfoDTO CourseToken2CourseInfoDto(this CRS_CourseToken token, string trackingId)
        {
            if (token == null) return null;

            var dto = new CourseInfoDTO
            {
                CourseId                  =  token.CourseId
                ,uid                      = token.uid
                ,CourseName               = token.CourseName
                ,CourseDescription        = token.CourseDescription                
                ,IsFreeCourse             = token.IsFreeCourse
                ,ThumbUrl                 = token.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)                
                ,Price                    = token.Price
                ,MonthlySubscriptionPrice = token.MonthlySubscriptionPrice
                ,OverviewVideoIdentifier  = token.OverviewVideoIdentifier
                ,DisplayOtherLearnersTab = token.DisplayOtherLearnersTab
                ,MetaTags                 = token.MetaTags
                ,Author                   = new UserInfoDTO
                                                            {
                                                                UserId      = token.AuthorUserId
                                                                ,FirstName  = token.FirstName
                                                                ,LastName   = token.LastName
                                                                ,FullName   = token.Entity2AuthorFullName()
                                                                ,Email      = token.Email
                                                                ,FacebookId = token.FacebookID
                                                            }
            };

            dto.CoursePageUrl = token.GenerateCourseFullPageUrl(dto.Author.FullName, dto.CourseName,trackingId);

            return dto;
        }

        public static BundleInfoDTO BundleToken2BundleInfoDto(this CRS_BundleInfoToken token, string trackingId)
        {
            if (token == null) return null;

            var dto = new BundleInfoDTO
            {
                BundleId                  = token.BundleId
                ,BundleName               = token.BundleName
                ,Description              = token.BundleDescription
                ,ThumbUrl                 = token.BannerImage.ToThumbUrl(Constants.ImageBaseUrl)
                ,PromoVideoIdentifier     = String.IsNullOrEmpty(token.OverviewVideoIdentifier) ? (long?)null : Int64.Parse(token.OverviewVideoIdentifier)
                ,PriceToken               = new PriceBaseDTO
                                                            {
                                                                Price                     = token.Price
                                                                ,MonthlySubscriptionPrice = token.MonthlySubscriptionPrice
                                                            }
                ,Author                   = new UserInfoDTO
                                                            {
                                                                UserId      = token.AuthorId
                                                                ,FirstName  = token.FirstName
                                                                ,LastName   = token.LastName
                                                                ,FullName   = token.Entity2AuthorFullName()
                                                                ,Email      = token.Email
                                                                ,FacebookId = token.FacebookID
                                                            }
            };

            dto.BundlePageUrl = token.GenerateBundleFullPageUrl(dto.Author.FullName, dto.BundleName,trackingId);

            return dto;
        }

        public static BaseListDTO CourseEntity2BaseListDto(this Courses entity)
        {
            return new BaseListDTO
                {
                    id = entity.Id
                    ,uid = entity.uid
                    ,name = entity.CourseName
                };
        }

        public static BaseEntityDTO CourseEntity2BaseEntityDTO(this Courses entity)
        {
            return new BaseEntityDTO
            {
                id = entity.Id
                ,
                Uid = entity.uid
                ,
                name = entity.CourseName
            };
        }
        public static CourseBaseDTO CourseEntity2CourseBaseDTO(this Courses entity)
        {
            return new CourseBaseDTO
            {
                CourseId = entity.Id,
                Uid = entity.uid,
                Name = entity.CourseName,
                AuthorId = entity.AuthorUserId
            };
        }
        public static BaseEntityDTO BundleEntity2BaseEntityDTO(this CRS_Bundles entity)
        {
            return new BaseEntityDTO
            {
                id = entity.BundleId
                ,Uid = entity.uid
                ,name = entity.BundleName
            };
        }

        public static BaseBundleDTO BundleEntity2BaseBundleDto(this CRS_Bundles entity)
        {
            return new BaseBundleDTO
            {
                 BundleId             = entity.BundleId
                ,BundleName           = entity.BundleName
                ,AuthorId             = entity.AuthorId
                ,Description          = entity.BundleDescription
                ,PromoVideoIdentifier = String.IsNullOrEmpty(entity.OverviewVideoIdentifier) ? (long?) null : Int64.Parse(entity.OverviewVideoIdentifier)
            };
        }
        
        public static CourseFbToken CourseEntity2FbToken(this CRS_CourseToken entity)
        {
            if (entity == null) return null;

            return new CourseFbToken
            {
                 CourseId      = entity.CourseId
                ,ImageUrl      = String.IsNullOrEmpty(entity.SmallImage) ? string.Empty : entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl) 
                ,Name          = entity.CourseName
                ,CoursePageUrl = entity.GenerateCourseFullPageUrl(entity.Entity2AuthorFullName(), entity.CourseName,null)
            };
        }

        public static CourseEditDTO CourseEntity2CourseEditDTO(this Courses entity, bool isAnyPrices)
        {
            var token = new CourseEditDTO
                {
                    CourseId                  = entity.Id
                    ,Uid                      = entity.uid
                    ,AuthorId                 = entity.AuthorUserId
                    ,CourseName               = entity.CourseName
                    ,CourseDescription        = entity.Description
                    ,MetaTags                 = entity.MetaTags                    
                    ,Status                   = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId.ToString())
                    ,ThumbName                = entity.SmallImage
                    ,ThumbUrl                 = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                    ,ClassRoomId              = entity.ClassRoomId
                    ,AffiliateCommission      = entity.AffiliateCommission
                    ,IsFree                   = entity.IsFreeCourse
                    ,IsCoursePurchased        = entity.USER_Courses.ToList().Any()
                    ,HasChapters              = entity.CourseChapters.ToList().Count > 0
                    ,IsPriceDefined           = entity.IsFreeCourse || isAnyPrices
                    ,DisplayOtherLearnersTab = entity.DisplayOtherLearnersTab
                    ,Categories               = new List<int>()
                };

            
            if (String.IsNullOrEmpty(entity.OverviewVideoIdentifier))
            {
                token.PromoVideoIdentifier = null;
            }
            else
            {
                long bcId;
                
                var parsed = Int64.TryParse(entity.OverviewVideoIdentifier, out bcId);
                
                if (parsed)
                {
                    token.PromoVideoIdentifier = bcId;
                }
            }

            return token;
        }

        public static BundleEditDTO BundleEntity2BundleEditDto(this CRS_Bundles entity, bool isAnyPrices)
        {
            var token = new BundleEditDTO
                {
                    BundleId                  = entity.BundleId
                    ,Uid                      = entity.uid
                    ,AuthorId                 = entity.AuthorId
                    ,BundleName               = entity.BundleName
                    ,Description              = entity.BundleDescription
                    ,MetaTags                 = entity.MetaTags                    
                    ,Status                   = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId.ToString())
                    ,ThumbName                = entity.BannerImage
                    ,ThumbUrl                 = entity.BannerImage.ToThumbUrl(Constants.ImageBaseUrl)                    
                    ,IsBundlePurchased        = entity.USER_Bundles.ToList().Any()
                    ,IsPriceDefined           = isAnyPrices
                    ,HasCourses               = entity.CRS_BundleCourses.ToList().Any()
                    ,Categories               = new List<int>()
                };

            
            if (String.IsNullOrEmpty(entity.OverviewVideoIdentifier))
            {
                token.PromoVideoIdentifier = null;
            }
            else
            {
                long bcId;
                
                var parsed = Int64.TryParse(entity.OverviewVideoIdentifier, out bcId);
                
                if (parsed)
                {
                    token.PromoVideoIdentifier = bcId;
                }
            }

            return token;
        }

        public static LearnerCourseViewerDTO CourseEntity2CourseViewDto(this Courses entity)
        {
            return new LearnerCourseViewerDTO
                {
                     CourseId             = entity.Id
                    ,CourseName           = entity.CourseName
                    ,MetaTags             = entity.MetaTags
                    ,ThumbUrl             = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                    ,ClassRoomId          = entity.ClassRoomId
                    ,Rating               = entity.Rating ?? 0
                };
        }

        public static CourseListDTO Entity2CourseListDTO(this USER_CourseListToken entity,string priceDisplayName)
        {
            var token = new CourseListDTO
                {
                    CourseId                  = entity.Id
                    ,Uid                      = entity.Uid
                    ,Name                     = entity.CourseName
                    ,Price                    = entity.Price
                    ,MonthlySubscriptionPrice = entity.MonthlySubscriptionPrice
                    ,AuthorId                 = entity.AuthorUserId
                    ,AuthorFullName           = entity.Entity2AuthorFullName()
                    ,LearnerCount             = entity.LearnerCount
                    ,ReviewCount              = entity.ReviewCount
                    ,ImageUrl                 = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                    ,AddOn                    = entity.Created
                    ,Status                   = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId)
                    ,IsFree                   = entity.IsFreeCourse
                    ,IsPurchased              = entity.LearnerCount > 0
                    ,PriceDisplayName               = priceDisplayName
                };

            token.CoursePageUrl = token.GenerateCoursePageUrl(token.AuthorFullName, token.Name,null);

            return token;
        }

        public static CourseBaseToken Entity2CourseBaseToken(this Courses entity, decimal? price, decimal? monthlySubscriptionPrice)
        {
            return new CourseBaseToken
                {
                    CourseId                  = entity.Id
                    ,Uid                      = entity.uid
                    ,Name                     = entity.CourseName
                    ,Description              = entity.Description
                    ,Price                    = price
                    ,MonthlySubscriptionPrice = monthlySubscriptionPrice
                    ,AuthorId                 = entity.AuthorUserId
                    ,ImageUrl                 = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)

                };
        }

        public static CourseListDTO Entity2CourseListDTO(this LRNR_ItemToken entity, decimal? price, decimal? monthlySubscriptionPrice)
        {
            return new CourseListDTO
                {
                    CourseId                  = entity.Id
                    ,Uid                      = entity.Uid
                    ,Name                     = entity.CourseName
                    ,Price                    = price
                    ,MonthlySubscriptionPrice = monthlySubscriptionPrice
                    ,AuthorId                 = entity.AuthorUserId
                    ,AuthorFullName           = entity.Entity2AuthorFullName()
                    ,LearnerCount             = entity.NumSubscribers
                    ,ReviewCount              = entity.ReviewCount
                    ,ImageUrl                 = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                    ,AddOn                    = entity.Created
                };
        }
        public static CourseListDTO Entity2CourseListDTO(this USER_CourseToken entity, decimal? price, decimal? monthlySubscriptionPrice)
        {
            return new CourseListDTO
                {
                    CourseId                  = entity.Id
                    ,Uid                      = entity.Uid
                    ,Name                     = entity.CourseName
                    ,Price                    = price
                    ,MonthlySubscriptionPrice = monthlySubscriptionPrice
                    ,AuthorId                 = entity.AuthorUserId
                    ,AuthorFullName           = entity.Entity2AuthorFullName()
                    ,LearnerCount             = entity.LearnerCount
                    ,ReviewCount              = entity.ReviewCount
                    ,ImageUrl                 = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                    ,AddOn                    = entity.Created
                };
        }
        public static CoursePriceDTO Entity2CoursePriceDTO(this Courses entity, CurrencyDTO currency) //,List<PriceLineDTO> priceLines
        {
            return new CoursePriceDTO
                {
                    CourseId                    = entity.Id
                    ,IsFree                     = entity.IsFreeCourse
                   // ,Price                    = entity.PriceUSD.ItemPrice2DisplayPrice() 
                   // ,MonthlySubscriptionPrice = entity.MonthlySubscriptionPriceUSD.ItemPrice2DisplayPrice() 
                    ,AffiliateCommission        = entity.AffiliateCommission
                    ,Currency                   = currency.ToBaseCurrencyDto()
                  //  ,PriceLines                 = priceLines
                };
        }

        public static BundlePriceDTO Entity2BundlePriceDTO(this CRS_Bundles entity, CurrencyDTO currency)
        {
            return new BundlePriceDTO
                {
                    BundleId                  = entity.BundleId
                  //  ,Price                    = entity.Price.ItemPrice2DisplayPrice() 
                  //  ,MonthlySubscriptionPrice = entity.MonthlySubscriptionPrice.ItemPrice2DisplayPrice() 
                    ,AffiliateCommission      = entity.AffiliateCommission
                    ,Currency                 = currency.ToBaseCurrencyDto()
                };
        }

        public static BundleListDTO Entity2BundleListDTO(this CRS_BundleListToken entity)
        {
            var token = new BundleListDTO
                {
                    BundleId                  = entity.BundleId
                    ,Uid                      = entity.Uid
                    ,Name                     = entity.BundleName
                    ,Price                    = entity.Price
                    ,MonthlySubscriptionPrice = entity.MonthlySubscriptionPrice
                    ,AuthorId                 = entity.AuthorUserId
                    ,AuthorFullName           = entity.Entity2AuthorFullName()
                    ,LearnerCount             = entity.LearnerCount
                    ,ReviewCount              = 0
                    ,ImageUrl                 = entity.BannerImage.ToThumbUrl(Constants.ImageBaseUrl)                    
                    ,AddOn                    = entity.AddOn
                    ,Status                   = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId)
                    ,IsPurchased              = entity.LearnerCount>0
                };

            token.BundlePageUrl = token.GenerateItemPageUrl(token.AuthorFullName, token.Name,BillingEnums.ePurchaseItemTypes.BUNDLE);

            return token;
        }

        public static BundleCourseListDTO Entity2BundleCourseListDTO(this CRS_BundleCourseToken entity,string trackingId)
        {
            return new BundleCourseListDTO
                {
                    id       = entity.CourseId
                    ,rowId   = entity.BundleCourseId
                    ,name    = entity.CourseName
                    ,url     = String.IsNullOrEmpty(entity.SmallImage) ? string.Empty: entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                    ,desc    = entity.CourseDescription
                    ,pageUrl = entity.GenerateItemPageUrl(entity.Entity2AuthorFullName(), entity.CourseName,BillingEnums.ePurchaseItemTypes.COURSE,trackingId)
                    ,status  = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId)
                };
        }

        public static BundleCourseListDTO Entity2BundleCourseListDTO(this CRS_AvailableCourseToken entity)
        {
            return new BundleCourseListDTO
                {
                    id     = entity.Id
                    ,rowId = -1
                    ,name  = entity.CourseName
                    ,url   = String.IsNullOrEmpty(entity.SmallImage) ? string.Empty: entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                    ,status  = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId)
                };
        }

        public static ReviewDTO Entity2ReviewDTO(this USER_ReviewToken entity)
        {
            return new ReviewDTO
                {
                     ReviewId        = entity.ReviewId
                    ,Date            = entity.ReviewDate
                    ,Title           = entity.ReviewTitle
                    ,Text            = entity.ReviewText
                    ,CourseName      = entity.CourseName
                    ,LearnerFullName =  String.Format("{0} {1}", entity.FirstName, entity.LastName)
                };
        }

        public static ReviewDTO UserReviewEntity2ReviewDTO(this USER_ReviewToken entity)
        {
            return new ReviewDTO
            {
                ReviewId          = entity.ReviewId
                ,Date             = entity.ReviewDate
                ,Title            = entity.ReviewTitle
                ,Text             = entity.ReviewText
                ,CourseName       = entity.CourseName
                ,LearnerFullName  = String.Format("{0} {1}", entity.FirstName, entity.LastName)
                ,LearnerNickname  = entity.Nickname
                ,LearnerEmail     = entity.Email
                ,LearnerId        = entity.LearnerId
                ,LearnerFirstName = entity.FirstName
                ,LearnerLastName  = entity.LastName
                ,Rating           = entity.ReviewRating
            };
        }

        //chapters
        public static BaseListDTO ChapterEntity2ListDTO(this CourseChapters entity)
        {
            return new BaseListDTO
            {
                 id   = entity.Id                
                ,name = entity.ChapterName
                ,index = entity.ChapterOrdinal
            };
        }

        public static ChapterEditDTO ChapterEntity2ChapterEditDTO(this CourseChapters entity)
        {
            return new ChapterEditDTO
            {
                 ChapterId   = entity.Id
                ,CourseId    = entity.CourseId
                ,Name        = entity.ChapterName
                ,OrderIndex  = entity.ChapterOrdinal
            };
        }

        //chapter videos
        public static VideoListDto ChapterVideoEntity2ListDTO(this ChapterVideos entity)
        {
            return new VideoListDto
            {
                 id        = entity.Id
                ,ChapterId = entity.CourseChapterId
                ,name      = entity.VideoTitle
                ,index     = entity.Ordinal
            };
        }

        public static ChapterVideoEditDTO ChapterVideoEntity2VideoEditDTO(this ChapterVideos entity)
        {
            return new ChapterVideoEditDTO
            {
                 VideoId  = entity.Id
                ,ChapterId       = entity.CourseChapterId
                ,VideoIdentifier = String.IsNullOrEmpty(entity.VideoSupplierIdentifier) ? (long?)null : Int64.Parse(entity.VideoSupplierIdentifier)
                ,Title           = entity.VideoTitle
                ,SummaryHTML     = entity.VideoSummary
                ,OrderIndex      = entity.Ordinal
                ,IsOpen          = entity.IsOpen == 1
            };
        }


        public static VideoListEditDTO ChapterVideo2ChapterListVideoEditDto(this ChapterVideos entity)
        {
            return new VideoListEditDTO
            {
                 VideoId  = entity.Id
                ,ChapterId       = entity.CourseChapterId
                ,VideoIdentifier = String.IsNullOrEmpty(entity.VideoSupplierIdentifier) ? (long?)null : Int64.Parse(entity.VideoSupplierIdentifier)
                ,Title           = entity.VideoTitle
                ,SummaryHTML     = entity.VideoSummary
                ,OrderIndex      = entity.Ordinal
                ,IsOpen          = entity.IsOpen == 1
            };
        }

         //chapter videos
        public static LinkListDto ChapterLinkEntity2ListDTO(this ChapterLinks entity)
        {
            return new LinkListDto
            {
                 id        = entity.Id
                ,ChapterId = entity.CourseChapterId
                ,name      = entity.LinkText
                ,index     = entity.Ordinal
            };
        }

        public static ChapterLinkEditDTO ChapterLinkEntity2LinkEditDTO(this ChapterLinks entity)
        {
            return new ChapterLinkEditDTO
            {
                 LinkId     = entity.Id
                ,ChapterId  = entity.CourseChapterId
                ,Title      = entity.LinkText
                ,LinkHref   = entity.LinkHref
                ,OrderIndex = entity.Ordinal
                ,Kind       = Utils.ParseEnum<CourseEnums.eChapterLinkKind>(entity.LinkType.ToString())
               // ,KindName   = Utils.ParseEnum<CourseEnums.eChapterLinkKind>(entity.LinkType.ToString()).ToString()
            };
        }

        public static ChapterLinkListToken ChapterLinkEntity2LinkListToken(this ChapterLinks entity)
        {
            return new ChapterLinkListToken
            {
                 name  = entity.LinkText
                ,url   = entity.LinkHref
                ,index = entity.Ordinal
                ,kind  = Utils.ParseEnum<CourseEnums.eChapterLinkKind>(entity.LinkType.ToString())
            };
        }

        //user portal
      
        public static VideoNavigationToken[] ChapterTreeList2VideoNavigation(this  List<ContentTreeViewItemDTO> treeList)
        {
            var array = new List<VideoNavigationToken>();

            var lastVideoId = -1;

            foreach (var chapter in treeList.Where(x=>x.type != CourseEnums.eContentTreeViewItemType.quiz).ToList())
            {
                
                var currentChapterId = chapter.id;

                foreach (var t in chapter.videos)
                {
                    var video = new VideoNavigationToken
                    {
                         chapterId = currentChapterId
                        ,prevId    = lastVideoId
                        ,bcId      = t.bcId //?? -1
                        ,videoId   = t.id
                    };

                    lastVideoId = video.videoId;

                    //update next for previous video
                    if (array.Count > 0)
                    {
                        array[array.Count - 1].nextId = lastVideoId;
                    }

                    array.Add(video);    
                }
            }

            return array.ToArray();
        }

        public static CourseUserReviewDTO Entity2CourseUserReviewDTO(this UserCourseReviews entity)
        {
            return new CourseUserReviewDTO
            {
                 ReviewId = entity.Id
                ,CourseId = entity.CourseId
                ,Title    = entity.ReviewTitle
                ,Text     = entity.ReviewText
                ,Rating   = entity.ReviewRating
            };
        }


        //purchase
        public static ItemPurchaseDataToken CourseInfoDTO2ItemPurchaseDataToken(this CourseInfoDTO token, PriceLineDTO priceToken)
        {
            return new ItemPurchaseDataToken
            {
                ItemId                  = token.CourseId
                ,ItemName               = token.CourseName
                ,Type                   = BillingEnums.ePurchaseItemTypes.COURSE
                ,PriceToken             = priceToken
                ,Author                 = new UserBaseDTO
                                        {
                                            userId    = token.Author.UserId
                                            ,fullName = token.Author.FullName
                                        }
            };
        }

        public static ItemPurchaseDataToken BundleInfoDTO2ItemPurchaseDataToken(this BundleInfoDTO token, PriceLineDTO priceToken)
        {
            return new ItemPurchaseDataToken
            {
                ItemId                  = token.BundleId
                ,ItemName               = token.BundleName
                ,Type                   = BillingEnums.ePurchaseItemTypes.BUNDLE
                ,PriceToken             = priceToken
                ,Author                 = new UserBaseDTO
                                        {
                                            userId    = token.Author.UserId
                                            ,fullName = token.Author.FullName
                                        }
            };
        }

        public static ItemPurchaseCompleteToken CourseInfoDto2ItemPurchaseCompleteToken(this CourseInfoDTO token, PriceLineDTO priceToken, BaseUserInfoDTO buyer, decimal totalPrice)
        {
            return new ItemPurchaseCompleteToken
            {
                ItemId      = token.CourseId
                ,ItemName   = token.CourseName
                ,ItemType   = BillingEnums.ePurchaseItemTypes.COURSE
                ,ThumbUrl   = token.ThumbUrl
                ,FinalPrice = totalPrice
                ,PriceToken = priceToken
                ,BuyerInfo  = buyer
                ,Author     = new BaseUserInfoDTO
                {
                     UserId   = token.Author.UserId
                    ,FullName = token.Author.FullName
                }

            };
        }

        public static ItemPurchaseCompleteToken CourseEntity2ItemPurchaseCompleteToken(this Courses entity, vw_SALE_OrderLines orderLineEntity, BaseUserInfoDTO buyer)
        {
            return new ItemPurchaseCompleteToken
            {
                ItemId      = entity.Id
                ,ItemName   = entity.CourseName
                ,ItemType   = BillingEnums.ePurchaseItemTypes.COURSE
                ,ThumbUrl   = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)
                ,FinalPrice = 0
                ,PriceToken = null
                ,BuyerInfo  = buyer
                ,Author     = new BaseUserInfoDTO
                {
                     UserId   = orderLineEntity.SellerUserId
                    ,FullName = orderLineEntity.Entity2SellerFullName()
                }
            };
        }

        public static ItemPurchaseCompleteToken BundleEntity2ItemPurchaseCompleteToken(this CRS_Bundles entity, vw_SALE_OrderLines orderLineEntity, BaseUserInfoDTO buyer)
        {
            return new ItemPurchaseCompleteToken
            {
                ItemId      = entity.BundleId
                ,ItemName   = entity.BundleName
                ,ItemType   = BillingEnums.ePurchaseItemTypes.COURSE
                ,ThumbUrl   = entity.BannerImage.ToThumbUrl(Constants.ImageBaseUrl)
                ,FinalPrice = 0
                ,PriceToken = null
                ,BuyerInfo  = buyer
                ,Author     = new BaseUserInfoDTO
                {
                     UserId   = orderLineEntity.SellerUserId
                    ,FullName = orderLineEntity.Entity2SellerFullName()
                }
            };
        }

        public static ItemPurchaseCompleteToken BundleInfoDto2ItemPurchaseCompleteToken(this BundleInfoDTO token, PriceLineDTO priceToken, BaseUserInfoDTO buyer, decimal totalPrice)
        {
            return new ItemPurchaseCompleteToken
            {
                ItemId      = token.BundleId
                ,ItemName   = token.BundleName
                ,ItemType   = BillingEnums.ePurchaseItemTypes.BUNDLE
                ,ThumbUrl   = token.ThumbUrl
                ,FinalPrice = totalPrice
                ,PriceToken = priceToken
                ,BuyerInfo  = buyer
                ,Author     = new BaseUserInfoDTO
                {
                     UserId   = token.Author.UserId
                    ,FullName = token.Author.FullName
                }
            };
        }
        
        public static ItemPurchaseDataToken CourseToken2ItemPurchaseDataToken(this CRS_CourseToken token)
        {
            return new ItemPurchaseDataToken
            {
                ItemId                  = token.CourseId
                ,ItemName               = token.CourseName
                ,Type                     = BillingEnums.ePurchaseItemTypes.COURSE
                ,Price                    = token.Price.FormatPrice()
                ,MonthlySubscriptionPrice = token.MonthlySubscriptionPrice.FormatPrice()
                ,Author                   = new UserBaseDTO
                {
                    userId    = token.AuthorUserId
                    ,fullName = token.Entity2AuthorFullName()
                }
            };
        }

        public static ItemPurchaseDataToken BundleToken2ItemPurchaseDataToken(this CRS_Bundles token, decimal? price, decimal? monthlySubscriptionPrice)
        {
            return new ItemPurchaseDataToken
            {
                ItemId                    = token.BundleId
                ,ItemName                 = token.BundleName
                ,Type                     = BillingEnums.ePurchaseItemTypes.BUNDLE
                ,Price                    = price.FormatPrice()
                ,MonthlySubscriptionPrice = monthlySubscriptionPrice.FormatPrice()
                ,Author                   = new UserBaseDTO
                                            {
                                                userId    = token.AuthorId
                                                ,fullName = token.Users.Entity2FullName()
                                            }
            };
        } 
    }
}
