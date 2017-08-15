using LFE.Application.Services.Base;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Application.Services
{
    public class DashboardServices : ServiceBase, IDashboardServices
    {

        #region interface implementation
        public DashboardStatsToken GetAuthorDashboardStats(int userId)
        {
            try
            {
                var dto = new DashboardStatsToken
                {
                    Courses            = CourseRepository.Count(c => c.AuthorUserId == userId),
                    UnpublishedCourses = CourseRepository.Count(c => c.AuthorUserId == userId && c.StatusId == (short)CourseEnums.CourseStatus.Draft),
                    Bundles            = BundleRepository.Count(c => c.AuthorId == userId),
                    Stores             = _GetOwnerStores(userId).Count(),
                    ActiveSubscribers  = OrderLinesViewRepository.Count(x => x.SellerUserId == userId && x.LineTypeId == (byte)BillingEnums.eOrderLineTypes.SUBSCRIPTION && x.OrderStatusId == (byte)BillingEnums.eOrderStatuses.ACTIVE)
                };                
                return dto;
            }
            catch (Exception ex)
            {
                Logger.Error("get course statistic for dashboard", userId, ex, CommonEnums.LoggerObjectTypes.Dashboard);
                return new DashboardStatsToken();
            }
        }

        public List<DashboardKpiChartDTO> GetChartData(FiltersToken filter, int userId, bool isCompareChart)
        {
            try
            {
                var period = Utils.ParseEnum<ReportEnums.ePeriodSelectionKinds>(filter.PeriodTypeId.ToString());
                var results = new List<DashboardKpiChartDTO>();

                var dates = PeriodKindToDateRange(period, isCompareChart);

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.tvf_FACT_GetEventsDailyLiveAggregates(dates.from, dates.to, userId, filter.StoreId).ToList();

                    var points = rows.Where(x => x.EventDate != null).GroupBy(x => new { FactDate = DateTime.Parse(x.EventDate.ToString()) }).ToArray();

                    results.AddRange(from point in points
                                     let current          = point
                                     let videoWatch       = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.VIDEO_PREVIEW_WATCH)
                                     let buyEntered       = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.BUY_PAGE_ENTERED)
                                     let purchaseComplete = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.PURCHASE_COMPLETE)
                                     let itemPreview      = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.COURSE_PREVIEW_ENTER)
                                     let storeViews       = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.STORE_VIEW)
                                     select new DashboardKpiChartDTO
                                     {
                                         date                = point.Key.FactDate,
                                         video_preview_watch = videoWatch != null ? videoWatch.cnt : 0,
                                         buy_entered         = buyEntered != null ? buyEntered.cnt : 0,
                                         purchase_complete   = purchaseComplete != null ? purchaseComplete.cnt : 0,
                                         items               = itemPreview != null ? itemPreview.cnt : 0,
                                         stores              = storeViews != null ? storeViews.cnt : 0
                                     });
                }

                return results;
            }
            catch (Exception ex)
            {
                Logger.Error("Get Dashboard Chart data", ex, CommonEnums.LoggerObjectTypes.Dashboard);
                return new List<DashboardKpiChartDTO>();
            }
        }

        public DashboardSaleBoxTokenLists GetSales(FiltersToken filter, int userId)
        {
            var token = new DashboardSaleBoxTokenLists
            {
                Currency      = ActiveCurrencies.FirstOrDefault(x=>x.CurrencyId == filter.CurrencyId),
                CompareToList = new List<DashboardSaleBoxToken>(),
                Filters       = filter
            };

            try
            {
                var period = Utils.ParseEnum<ReportEnums.ePeriodSelectionKinds>(filter.PeriodTypeId.ToString());

                var dates = PeriodKindToDateRange(period, false);

                var mainList = getDashboardSaleStats(dates,userId, filter.CurrencyId, filter.StoreId);

                var secondDates = PeriodKindToDateRange(period, true);

                var secondList = getDashboardSaleStats(secondDates, userId, filter.CurrencyId, filter.StoreId);

                setSalesBoxesComparsion(mainList, secondList);

                token.List = mainList;

                if (!filter.IsCompareMode) return token;

                var thirdDates = PreviousRangeFromRange(secondDates);

                var thirdList = getDashboardSaleStats(thirdDates,userId, filter.CurrencyId, filter.StoreId);

                setSalesBoxesComparsion(secondList,thirdList);

                token.CompareToList = secondList;

                return token;

            }
            catch (Exception ex)
            {
                Logger.Error("get sales statistic for dashboard", CurrentUserId, ex, CommonEnums.LoggerObjectTypes.Dashboard);
                return token;
            }
           
        }

        public List<BaseCurrencyDTO> GetUserCurrencies(int userId)
        {
            return _GetUserCurrencies(userId, null, null);
        } 

        public List<DashboardPayoutToken> GetNextPayout(int userId)
        {

            try
            {
                var now = DateTime.Now;
                var previous = now.AddMonths(-1);
             
                using (var context = new lfeAuthorEntities())
                {
                    var currentPayout = context.sp_PO_GetMonthlyPayoutReport(now.Year, now.Month, LFE_COMMISSION_PERCENT, userId, null).Select(x => x.Entity2DashboardPayoutToken()).ToList();

                    if(currentPayout.Count.Equals(0)) return new List<DashboardPayoutToken>
                    {
                        new DashboardPayoutToken
                        {
                            Sales = 0
                            ,Fees = 0
                            ,Mbg = 0
                           // ,TotalPayout = 0
                            ,Currency = ActiveCurrencies.FirstOrDefault(x=>x.CurrencyId == DEFAULT_CURRENCY_ID)
                        }
                    };

                    var previousPayout = context.sp_PO_GetMonthlyPayoutReport(previous.Year, previous.Month, LFE_COMMISSION_PERCENT, userId, null).Select(x => x.Entity2DashboardPayoutToken()).ToList();

                    foreach (var token in currentPayout)
                    {
                        var t = token;
                        var previousToken = previousPayout.FirstOrDefault(x => x.Currency.CurrencyId == t.Currency.CurrencyId);

                        token.IsUp = previousToken == null || previousToken.TotalPayout <= t.TotalPayout;
                    }


                    return currentPayout;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetNextPayout", ex, CommonEnums.LoggerObjectTypes.Dashboard);

                return new List<DashboardPayoutToken>();
            }

        }

        #region sales boxes details

        public List<BaseOrderLineDTO> GetSalesRows(int userId,DashboardEnums.eSaleBoxType type,FiltersToken filter)
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {

                    string paymentSource = null;
                    byte? lineTypeId = null;


                    switch (type)
                    {
                       case DashboardEnums.eSaleBoxType.ONE_TIME:
                            paymentSource = DashboardEnums.eSaleSources.AS.ToString();
                            lineTypeId = (byte) BillingEnums.eOrderLineTypes.SALE;
                            break;
                        case DashboardEnums.eSaleBoxType.SUBSCRIPTION:
                            paymentSource = DashboardEnums.eSaleSources.AS.ToString();
                            lineTypeId = (byte) BillingEnums.eOrderLineTypes.SUBSCRIPTION;
                            break;
                        case DashboardEnums.eSaleBoxType.RENTAL:
                            paymentSource = DashboardEnums.eSaleSources.AS.ToString();
                            lineTypeId = (byte) BillingEnums.eOrderLineTypes.RENTAL;
                            break;
                        case DashboardEnums.eSaleBoxType.SALES_BY_AFFILIATES:
                            paymentSource = DashboardEnums.eSaleSources.BAFS.ToString();
                            break;
                        case DashboardEnums.eSaleBoxType.AFFILIATE_SALES:
                            paymentSource = DashboardEnums.eSaleSources.AFS.ToString();
                            break;
                    }

                    var result = context.tvf_DB_GetUserSalesDetails(filter.DateRange.from,filter.DateRange.to,userId,filter.CurrencyId,filter.StoreId,paymentSource,lineTypeId).ToList(); 
                    var lines = result.Select(x=>x.DbSaleEntity2BaseOrderLineDto()).OrderByDescending(x=>x.OrderNumber).ToList();

                    return lines;
                }
            }
            catch (Exception ex)
            {

                Logger.Error("GetSalesRows",ex,CommonEnums.LoggerObjectTypes.Dashboard);

                return new List<BaseOrderLineDTO>();
            }
        }

        public List<BaseOrderLineDTO> GetCouponRows(int userId,FiltersToken filter)
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var result = context.tvf_DB_GetUserCouponUsageDetails(filter.DateRange.from,filter.DateRange.to,userId,filter.CurrencyId,filter.StoreId,null,null).Where(x=>x.CouponInstanceId != null).ToList();
                    var lines = result.Select(x => x.DbCouponEntity2BaseOrderLineDto()).OrderByDescending(x => x.OrderNumber).ToList();

                    return lines;
                }
            }
            catch (Exception ex)
            {

                Logger.Error("GetSalesRows",ex,CommonEnums.LoggerObjectTypes.Dashboard);

                return new List<BaseOrderLineDTO>();
            }
        }

        public List<DbRefundDetailToken> GetRefundRows(int userId, FiltersToken filter)
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var result = context.tvf_DB_GetUserRefundDetails(filter.DateRange.from,filter.DateRange.to,userId,filter.CurrencyId,filter.StoreId,null,null).ToList(); 
                    var lines = result.Select(x=>x.DbRefundEntity2BaseOrderLineDto()).OrderByDescending(x=>x.OrderNumber).ToList();

                    return lines;
                }
            }
            catch (Exception ex)
            {

                Logger.Error("GetRefundRows", ex, CommonEnums.LoggerObjectTypes.Dashboard);

                return new List<DbRefundDetailToken>();
            }
        }

        public List<DbSubscriptionDetailToken> GetSubscriptionCancelRows(int userId, FiltersToken filter)
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var result = context.tvf_DB_GetUserSubscriptionsCancelDetails(filter.DateRange.from, filter.DateRange.to, userId, filter.CurrencyId, filter.StoreId, null, null).ToList(); 
                    var lines = result.Select(x => x.DbCancelEntity2BaseOrderLineDto()).OrderByDescending(x => x.OrderNumber).ToList();

                    return lines;
                }
            }
            catch (Exception ex)
            {

                Logger.Error("GetSubscriptionCancelRows", ex, CommonEnums.LoggerObjectTypes.Dashboard);

                return new List<DbSubscriptionDetailToken>();
            }
        }

        public List<DbSubscriptionDetailToken> GetActiveSubscribers(int userId)
        {
            try
            {
                var result = OrderLinesViewRepository.GetMany(x => x.SellerUserId == userId && x.LineTypeId == (byte)BillingEnums.eOrderLineTypes.SUBSCRIPTION && x.OrderStatusId == (byte)BillingEnums.eOrderStatuses.ACTIVE).ToList();
                var lines = result.Select(x => x.Entity2DbSubscriptionDetailToken()).OrderByDescending(x => x.OrderNumber).ToList();

                return lines;
            }
            catch (Exception ex)
            {

                Logger.Error("GetActiveSubscribers", ex, CommonEnums.LoggerObjectTypes.Dashboard);

                return new List<DbSubscriptionDetailToken>();
            }
        }
        #endregion

        #region Events
        public List<DashboardEventToken> GetDashboardEvents(int userId, DateRangeToken dates)
        {
            try
            {
                return new List<DashboardEventToken>
                {
                    GetDashboardEventToken(userId,DashboardEnums.eDbEventTypes.NewItem,dates),
                    GetDashboardEventToken(userId,DashboardEnums.eDbEventTypes.NewChapter,dates),
                    GetDashboardEventToken(userId,DashboardEnums.eDbEventTypes.NewFbStore,dates),
                    GetDashboardEventToken(userId,DashboardEnums.eDbEventTypes.NewStore,dates),
                    GetDashboardEventToken(userId,DashboardEnums.eDbEventTypes.NewMailchimp,dates)
                };
            }
            catch (Exception ex)
            {
                Logger.Error("GetDashboardEvents",ex,CommonEnums.LoggerObjectTypes.Author);
                return new List<DashboardEventToken>();
            }
        }

        public DashboardEventToken GetDashboardEventToken(int userId, DashboardEnums.eDbEventTypes type, DateRangeToken dates, string eventName = null)
        {
            var token = new DashboardEventToken
                {
                     Uid      = Guid.NewGuid()
                    ,Type     = type
                    ,Name     = eventName ?? Utils.GetEnumDescription(type)
                    ,Color    = type.EventType2Color()
                    ,Enabled  = true
                    ,IsStatic = false
                };

            if (type == DashboardEnums.eDbEventTypes.Custom || type == DashboardEnums.eDbEventTypes.NewMailchimp) return token;

            token.IsStatic = true;

            using (var context = new lfeAuthorEntities())
            {
                var evenStats = context.tvf_DB_GetAuthorEventStats(userId).FirstOrDefault();

                if (evenStats == null) return token;

                DateTime? date = null;

                switch (type)
                {
                    case DashboardEnums.eDbEventTypes.NewItem:
                        var cd = evenStats.LastCoursePublish;
                        var bd = evenStats.LastBundlePublish;
                        
                        if (cd == null && bd == null) return token;

                        date = (cd ?? DateTime.MinValue).CompareToDate(bd ?? DateTime.MinValue);                        
                        break;
                    case DashboardEnums.eDbEventTypes.NewChapter:
                        date = evenStats.LastChaperCreated;
                        break;
                    case DashboardEnums.eDbEventTypes.NewFbStore:
                        date = evenStats.LastChaperCreated;
                        break;
                    case DashboardEnums.eDbEventTypes.NewStore:
                        date = evenStats.LastChaperCreated;
                        break;                   
                }

                token.Date    = date;
                token.Enabled = date != null && (((DateTime)date).Ticks >= dates.from.Ticks && ((DateTime)date).Ticks <= dates.to.Ticks);

                return token;
            }
        }
       
        //public DashboardEventToken GetDashboardCustomEventToken(DateTime date, string name)
        //{
        //    return new DashboardEventToken
        //    {
        //        Uid       = Guid.NewGuid()
        //        ,Type     = DashboardEnums.eDbEventTypes.Custom
        //        ,Name     = name
        //        ,Date     = date
        //        ,Enabled  = true
        //        ,IsStatic = false
        //    };
        //}

        #region Custom events
        public List<DashboardEventToken> CustomEventGetList(int userId)
        {
            try
            {
                var entityList = CustomEventsRepository.GetMany(x => x.UserId == userId);
                return entityList.Select(x => x.Entity2Token()).ToList();
            }
            catch (Exception e)
            {
                Logger.Error("get list custom event", e, CommonEnums.LoggerObjectTypes.Dashboard);
                return new List<DashboardEventToken>();
            }
        }
        
        public bool CustomEventAdd(int userId, DashboardEventToken token)
        {
            try
            {
                var entity = token.Token2Entity(userId);
                token.Uid = Guid.Parse(entity.Uid);
                CustomEventsRepository.Add(entity);
                return CustomEventsRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch(Exception e)
            {
                Logger.Error("add custom event", e, CommonEnums.LoggerObjectTypes.Dashboard);
                return false;
            }
        }

        public bool CustomEventRemove(int userId, DashboardEventToken token)
        {
            try
            {
                var uid = token.Uid.ToString();
                var entity = CustomEventsRepository.Get(x => x.Uid == uid && x.UserId == userId);
                
                if (entity == null) return false;
                
                CustomEventsRepository.Delete(entity);
                
                return CustomEventsRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception e)
            {
                Logger.Error("remove custom event", e, CommonEnums.LoggerObjectTypes.Dashboard);
                return false;
            }
        }

        public bool CustomEventUpdate(int userId, DashboardEventToken token)
        {
            try
            {
                var uid = token.Uid.ToString();
                var entity = CustomEventsRepository.Get(x => x.Uid == uid && x.UserId == userId);
                
                if (entity == null) return false;
                
                entity.Name  = token.Name;
                entity.Color = token.Color;
                if (token.Date != null) entity.Date = (DateTime)token.Date;

                CustomEventsRepository.Update(entity);
                return CustomEventsRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception e)
            {
                Logger.Error("edit custom event", e, CommonEnums.LoggerObjectTypes.Dashboard);
                return false;
            }
        }
        #endregion

        #endregion

        

        #endregion

        #region private helpers
       
        private static List<DashboardSaleBoxToken> EmptySalesBoxesList
        {
            get
            {
                return new List<DashboardSaleBoxToken>
                {
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.ONE_TIME),
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.SUBSCRIPTION),
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.RENTAL),
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.SALES_BY_AFFILIATES),
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.AFFILIATE_SALES),
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.SUBSCRIPTION_CANCELLATION),
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.REFUNDS),
                    new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.COUPONS_USED)
                };
            }
        }

        private List<DashboardSaleBoxToken> getDashboardSaleStats(DateRangeToken dates,int userId, short currencyId, int? storeId)
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var stats = context.tvf_DB_GetAuthorSalesStats(dates.from,dates.to,userId,currencyId,storeId).FirstOrDefault();

                    if (stats == null) return EmptySalesBoxesList;

                    var list = new List<DashboardSaleBoxToken>
                    {
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.ONE_TIME, stats.AuthorTotalOnetimeSales,stats.AuthorTotalOnetimeQty),
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.SUBSCRIPTION,stats.AuthorTotalSubscriptionSales,stats.AuthorTotalSubscriptionQty),
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.RENTAL,stats.AuthorTotalRentalSales,stats.AuthorTotalRentalQty),
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.SALES_BY_AFFILIATES,stats.ByAffiliateTotalSales,stats.ByAffiliateTotalQty),
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.AFFILIATE_SALES,stats.AffiliateTotalSales,stats.AffiliateTotalQty),
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.SUBSCRIPTION_CANCELLATION,stats.TotalCancelled,stats.TotalCancelledQty),
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.REFUNDS,stats.TotalRefund,stats.TotalRefundQty),
                        new DashboardSaleBoxToken(DashboardEnums.eSaleBoxType.COUPONS_USED,stats.TotalCouponValue,stats.TotalCouponQty)
                    };

                    return list;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("getDashboardSaleStats",ex,CommonEnums.LoggerObjectTypes.Dashboard);
                return EmptySalesBoxesList;
            }
        }

        private void setSalesBoxesComparsion(IEnumerable<DashboardSaleBoxToken> first, List<DashboardSaleBoxToken> second)
        {
            foreach (var token in first)
            {
                var prev = second.FirstOrDefault(x => x.Type == token.Type);
                token.IsUp = prev == null || prev.Total <= token.Total;
            }
        }
        #endregion

    }
}
