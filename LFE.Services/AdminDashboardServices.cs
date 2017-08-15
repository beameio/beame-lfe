using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.Web.Mvc;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Model;


namespace LFE.Application.Services
{
    public class AdminDashboardServices : ServiceBase, IAdminDashboardServices
    {
        private readonly IPayoutServices _payoutServices;

        public AdminDashboardServices()
        {
            _payoutServices = DependencyResolver.Current.GetService<IPayoutServices>();
        }

        public AdminDashboardToken GetAdminDashboardToken()
        {
            try
            {
                var now = DateTime.Now;

                var prev = now.AddMonths(-1);

                var rows = _payoutServices.GetPayoutCurrencySummaryRows(now.Year, now.Month, null, null,-1);

                var previous = _payoutServices.GetPayoutCurrencySummaryRows(prev.Year, prev.Month, null, null, -1);

                var p = new List<AdminPayoutToken>();

                var sellerTop = new TopSellersToken();

                foreach (var token in rows)
                {
                    var pt = token.PayoutCurrencySummaryDto2AdminPayoutToken();

                    var row = previous.FirstOrDefault(x => x.Currency.CurrencyId == token.Currency.CurrencyId);

                    var tendency = new TendencyToken();

                    if (row == null)
                    {
                        tendency.Direction = ReportEnums.eTendencyDirections.Up;
                        tendency.Percent = 100;
                    }
                    else
                    {
                        if (token.Currency.CurrencyId == USD_CURRENCY_ID || token.Currency.CurrencyId == EUR_CURRENCY_ID)
                        {
                            var top = row.Rows.OrderByDescending(x=>x.TotalSales).Take(Math.Min(row.Rows.Count,5)).Select(x=>new AuthorPayoutToken
                                                                                                                                                {
                                                                                                                                                    Seller = x.Seller
                                                                                                                                                    ,Currency = x.Currency
                                                                                                                                                    ,Sales = x.TotalSales
                                                                                                                                                }).ToList();
                            if (top.Count < 5)
                            {
                                var toAdd = 5 - top.Count;

                                for (var i = 0; i < toAdd; i++)
                                {
                                    top.Add(new AuthorPayoutToken{Currency = token.Currency});
                                }
                            }

                            switch (token.Currency.CurrencyId)
                            {
                                case USD_CURRENCY_ID:                                    
                                    sellerTop.Top_USD = top;
                                    break;
                                case EUR_CURRENCY_ID:
                                    sellerTop.Top_EUR = top;
                                    break;
                            }
                        }

                        var ptr = row.PayoutCurrencySummaryDto2AdminPayoutToken();

                        var revenueChange = pt.TotalRevenue - ptr.TotalRevenue;

                        if (revenueChange == 0)
                        {
                            tendency.Direction = ReportEnums.eTendencyDirections.Equal;
                            tendency.Percent = 0;
                        }
                        else
                        {
                            tendency.Direction = revenueChange > 0 ? ReportEnums.eTendencyDirections.Up : ReportEnums.eTendencyDirections.Down;

                            tendency.Percent = (ptr.TotalRevenue > 0 ? Math.Abs(100 - Math.Round((pt.TotalRevenue * 100 / ptr.TotalRevenue), 0)) : 0) * (revenueChange > 0 ? 1 : -1);
                        }
                    }

                    pt.Tendency = tendency;

                    p.Add(pt);
                }

                AdjustTopSellers(sellerTop);

                return new AdminDashboardToken
                {
                    NextPayoutList    = p
                    ,TopSellers       = sellerTop
                    ,VideoStats       = GetVideoStats()
                    ,AuthorTotalStats = GetAuthorTotalStats()
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Get Admin Dashboard token",ex,CommonEnums.LoggerObjectTypes.Reports);

                return new AdminDashboardToken{IsValid = false,Message = FormatError(ex)};
            }
        }

        private void AdjustTopSellers(TopSellersToken token)
        {
            if (token.Top_EUR.Count < 5)
            {
                var currency = ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == EUR_CURRENCY_ID);

                var toAdd = 5 - token.Top_EUR.Count;

                for (var i = 0; i < toAdd; i++)
                {
                    token.Top_EUR.Add(new AuthorPayoutToken { Currency = currency });
                }
            }

            if (token.Top_USD.Count < 5)
            {
                var currency = ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == USD_CURRENCY_ID);

                var toAdd = 5 - token.Top_USD.Count;

                for (var i = 0; i < toAdd; i++)
                {
                    token.Top_USD.Add(new AuthorPayoutToken { Currency = currency });
                }
            }
        }

        public IntegrationStatsToken GetIntegrationStatsToken(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var reportPeriod = PeriodSelection2DateRange(period);

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.tvf_FACT_DASH_GetNewPeriodTotals(reportPeriod.from,reportPeriod.to).ToList();

                    return new IntegrationStatsToken
                    {
                        TotalMailchimp = rows.Sum(x => x.NewMailchimpLists)
                        ,MbgJoined     = rows.Sum(x => x.NewMBGJoined)
                        ,MbgCanceled   = rows.Sum(x => x.MBGCancelled)
                    };

                    
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Get Admin Dashboard integration stats", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new IntegrationStatsToken();
            }   
        }

        public AdminVideoStatsToken GetVideoStats()
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var totals = context.tvf_FACT_DASH_GetTotals().FirstOrDefault();

                    if (totals == null) return new AdminVideoStatsToken();

                    return new AdminVideoStatsToken
                    {
                        TotalUploaded   = totals.TotalVideos ?? 0
                        ,CourseAttached = totals.TotalAttached2ActiveCourses ?? 0
                        ,NotAttached    = totals.UnattachedVideos ?? 0
                        ,TotalPreviews  = totals.VideoPreviews ?? 0

                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Admin Dashboard video stats",ex,CommonEnums.LoggerObjectTypes.Reports);
                return new AdminVideoStatsToken();
            }
        }

        public AuthorTotalStatsToken GetAuthorTotalStats()
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var totals = context.tvf_FACT_DASH_GetAuthorTotalStats().FirstOrDefault();

                    if (totals == null) return new AuthorTotalStatsToken();

                    return new AuthorTotalStatsToken
                    {
                        AverageCourseChapters        = totals.AverageCourseChapters.FormatDecimal(2)
                        ,AverageCoursesPerAuthor     = totals.AverageCoursesPerAuthor.FormatDecimal(2)
                        ,AverageBundlesPerAuthor     = totals.AverageBundlesPerAuthor.FormatDecimal(2)
                        ,AverageFreeCoursesPerAuthor = totals.AverageFreeCoursesPerAuthor.FormatDecimal(2)
                        ,TotalFreeCourses            = totals.TotalFreeCourses
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Admin Dashboard author total stats",ex,CommonEnums.LoggerObjectTypes.Reports);
                return new AuthorTotalStatsToken();
            }
        }

        public List<PlatformStatsToken> GetPlatformStats(ReportEnums.ePeriodSelectionKinds period)
        {
            var list = new List<PlatformStatsToken>();

            try
            {
                var reportPeriod = PeriodSelection2DateRange(period);

                var previousPeriod = Period2Previous(period);

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.tvf_FACT_DASH_GetPlatformStats(reportPeriod.from, reportPeriod.to).ToList();

                    var previousRows = context.tvf_FACT_DASH_GetPlatformStats(previousPeriod.from, previousPeriod.to).ToList();

                    var platformIds = rows.GroupBy(x => new { id = x.RegistrationTypeId }).Select(s=>s.Key.id).ToArray();

                    foreach (var platformId in platformIds)
                    {
                        var platform        = Utils.ParseEnum<CommonEnums.eRegistrationSources>(platformId.ToString());
                        var subList         = rows.Where(x => x.RegistrationTypeId == (int)platform).OrderBy(x=>x.FactDate).ToList();
                        var previousSubList = previousRows.Where(x => x.RegistrationTypeId == (int)platform).ToList();


                        var token = new PlatformStatsToken
                        {
                            Platform          = platform
                            ,Index            = platform.RegistrationSource2AdminDashboardIndex()
                            ,Stats            = new List<PlatformStatsBoxToken>()
                            ,TotalPlatformNew =  subList.Sum(x=>x.TotalPlatformNew)                            
                        };

                        token.Tendency = token.TotalPlatformNew.Value2TendencyToken(previousSubList.Sum(x => x.TotalPlatformNew));

                        #region
                        Parallel.Invoke(
                            () =>
                            {
                                
                                var box = new PlatformStatsBoxToken
                                {
                                    Type    = ReportEnums.eStatsTypes.Authors
                                    ,Index  = 1
                                    ,Total  = subList[subList.Count-1].TotalAuhtors
                                    ,New    = subList.Sum(x => x.NewAuthors)   
                                    ,Points = new List<BaseChartPointToken>()
                                };

                                box.Tendency = box.New.Value2TendencyToken(previousSubList.Sum(x => x.NewAuthors));

                                foreach (var p in subList.OrderBy(x=>x.FactDate).ToList())
                                {
                                    box.Points.Add(new BaseChartPointToken
                                    {
                                     date = p.FactDate
                                     ,value = p.NewAuthors
                                    });
                                }

                                token.Stats.Add(box);
                            },
                            () =>
                            {
                                var box = new PlatformStatsBoxToken
                               {
                                   Type    = ReportEnums.eStatsTypes.Items
                                   ,Index  = 2
                                   ,Total  = subList[subList.Count-1].TotalItems
                                   ,New    = subList.Sum(x => x.NewItems)   
                                   ,Points = new List<BaseChartPointToken>()
                               };

                                box.Tendency = box.New.Value2TendencyToken(previousSubList.Sum(x => x.NewItems));

                                foreach (var p in subList.OrderBy(x=>x.FactDate).ToList())
                                {
                                    box.Points.Add(new BaseChartPointToken
                                    {
                                     date = p.FactDate
                                     ,value = p.NewItems
                                    });
                                }

                                token.Stats.Add(box);
                            },
                            () =>
                            {
                                var box = new PlatformStatsBoxToken
                                {
                                    Type    = ReportEnums.eStatsTypes.Stores
                                    ,Index  = 3
                                    ,Total  = subList[subList.Count-1].TotalStores
                                    ,New    = subList.Sum(x => x.NewStores)   
                                    ,Points = new List<BaseChartPointToken>()
                                };

                                box.Tendency = box.New.Value2TendencyToken(previousSubList.Sum(x => x.NewStores));

                                foreach (var p in subList.OrderBy(x=>x.FactDate).ToList())
                                {
                                    box.Points.Add(new BaseChartPointToken
                                    {
                                        date = p.FactDate
                                        ,value = p.NewStores
                                    });
                                }

                                token.Stats.Add(box);
                            },
                            () =>
                            {
                                var box = new PlatformStatsBoxToken
                                {
                                   Type    = ReportEnums.eStatsTypes.Learners
                                   ,Index  = 4
                                   ,Total  = subList[subList.Count-1].TotalLearners
                                   ,New    = subList.Sum(x => x.NewLearners)   
                                   ,Points = new List<BaseChartPointToken>()
                                };

                                box.Tendency = box.New.Value2TendencyToken(previousSubList.Sum(x => x.NewLearners));

                                foreach (var p in subList.OrderBy(x=>x.FactDate).ToList())
                                {
                                    box.Points.Add(new BaseChartPointToken
                                    {
                                     date = p.FactDate
                                     ,value = p.NewLearners
                                    });
                                }

                                token.Stats.Add(box);
                            },
                            () =>
                            {
                                var box = new PlatformStatsBoxToken
                                {
                                    Type    = ReportEnums.eStatsTypes.Sales
                                    ,Index  = 5
                                    ,Total  = subList[subList.Count-1].TotalSales
                                    ,New    = subList.Sum(x => x.NewSales)   
                                    ,Points = new List<BaseChartPointToken>()
                                };

                                box.Tendency = box.New.Value2TendencyToken(previousSubList.Sum(x => x.NewSales));

                                foreach (var p in subList.OrderBy(x=>x.FactDate).ToList())
                                {
                                    box.Points.Add(new BaseChartPointToken
                                    {
                                        date = p.FactDate
                                        ,value = p.NewSales
                                    });
                                }

                                token.Stats.Add(box);
                            });
                        #endregion

                        list.Add(token);
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("Get platform stats for admin dashboard",ex,CommonEnums.LoggerObjectTypes.Reports);
                return list;
            }
        }

        public List<TotalsBoxToken> GetTotals(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var reportPeriod = PeriodSelection2DateRange(period);

                var previousPeriod = Period2Previous(period);

                using (var context = new lfeAuthorEntities())
                {
                    var totals = context.tvf_FACT_DASH_GetTotals().FirstOrDefault();

                    if (totals == null) return new List<TotalsBoxToken>();

                    var rows = context.tvf_FACT_DASH_GetNewPeriodTotals(reportPeriod.from, reportPeriod.to).ToList();
                    var previousRows = context.tvf_FACT_DASH_GetNewPeriodTotals(previousPeriod.from, previousPeriod.to).ToList();
                    
                    var list = new List<TotalsBoxToken>
                    {
                        ReportEnums.eStatsTypes.Stores.Type2TotalsBoxToken(1, totals.TotalStores,rows.Sum(x => x.NewStores), previousRows.Sum(x => x.NewStores)),
                        ReportEnums.eStatsTypes.Courses.Type2TotalsBoxToken(2, totals.TotalCourses,rows.Sum(x => x.NewCourses), previousRows.Sum(x => x.NewCourses)),
                        ReportEnums.eStatsTypes.Bundles.Type2TotalsBoxToken(3, totals.TotalBundles,rows.Sum(x => x.NewBundles), previousRows.Sum(x => x.NewBundles)),
                        ReportEnums.eStatsTypes.Learners.Type2TotalsBoxToken(4, totals.TotalLearners,rows.Sum(x => x.NewLearners), previousRows.Sum(x => x.NewLearners)),
                        ReportEnums.eStatsTypes.Authors.Type2TotalsBoxToken(5, totals.TotalAuthors,rows.Sum(x => x.NewAuthors), previousRows.Sum(x => x.NewAuthors))
                    };

                    return list;
                  
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Get totals stats for admin dashboard", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<TotalsBoxToken>();
            }
        }

        public List<AuthorPeriodStatsBoxToken> GetAuthorPeriodStats(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var reportPeriod = PeriodSelection2DateRange(period);
                var previousPeriod = Period2Previous(period);

                using (var context = new lfeAuthorEntities())
                {
                    
                    var totals = context.tvf_FACT_DASH_GetAuthorPeriodStats(reportPeriod.from,reportPeriod.to).FirstOrDefault();
                    var previousTotals = context.tvf_FACT_DASH_GetAuthorPeriodStats(previousPeriod.from, previousPeriod.to).FirstOrDefault();

                    if (totals == null) return new List<AuthorPeriodStatsBoxToken>();

                    var list = new List<AuthorPeriodStatsBoxToken>
                    {
                        ReportEnums.eStatsTypes.ActiveAuthors.Type2AuthorPeriodStatsBoxToken(1, totals.ActiveAuthors,previousTotals != null ? previousTotals.ActiveAuthors : 0),
                        ReportEnums.eStatsTypes.AvgAuthorLogins.Type2AuthorPeriodStatsBoxToken(2, totals.AvgAuthorLogins.FormatDecimal(2),previousTotals != null ? previousTotals.AvgAuthorLogins.FormatDecimal(2) : 0),
                        ReportEnums.eStatsTypes.DashboardViews.Type2AuthorPeriodStatsBoxToken(3, totals.DashboardViews,previousTotals != null ? previousTotals.DashboardViews : 0),
                        ReportEnums.eStatsTypes.CouponsCreated.Type2AuthorPeriodStatsBoxToken(4, totals.TotalCouponsCreated,previousTotals != null ? previousTotals.TotalCouponsCreated : 0)
                        
                    };

                    return list;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Admin Dashboard author period stats",ex,CommonEnums.LoggerObjectTypes.Reports);
                return new List<AuthorPeriodStatsBoxToken>();
            }
        }

        public List<LearnerPeriodStatsBoxToken> GetLearnerPeriodStats(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var reportPeriod = PeriodSelection2DateRange(period);
                var previousPeriod = Period2Previous(period);

                using (var context = new lfeAuthorEntities())
                {

                    var totals = context.tvf_FACT_DASH_GetLearnerPeriodStats(reportPeriod.from, reportPeriod.to).FirstOrDefault();
                   
                    if (totals == null) return new List<LearnerPeriodStatsBoxToken>();

                    var previousTotals = context.tvf_FACT_DASH_GetLearnerPeriodStats(previousPeriod.from, previousPeriod.to).FirstOrDefault();

                    var avgLogin     = ReportEnums.eStatsTypes.AvgLearnerLogin.Type2LearnerPeriodStatsBoxToken(0, totals.AvgLearnerLogin.FormatDecimal(2), previousTotals != null ? previousTotals.AvgLearnerLogin.FormatDecimal(2) : 0);
                    var buyCompltete = ReportEnums.eStatsTypes.PurchaseComplete.Type2LearnerPeriodStatsBoxToken(2, totals.TotalPurchaseComplete,previousTotals != null ? previousTotals.TotalPurchaseComplete: 0);

                    decimal avgWatched     = totals.TotalCoursesWatched > 0 ? totals.TotalVideosWatched/totals.TotalCoursesWatched : 0;
                    decimal prevAvgWatched = previousTotals != null ? (previousTotals.TotalCoursesWatched > 0 ? previousTotals.TotalVideosWatched / previousTotals.TotalCoursesWatched : 0) : 0;

                    var list = new List<LearnerPeriodStatsBoxToken>
                    {
                        ReportEnums.eStatsTypes.ActiveLearners.Type2LearnerPeriodStatsBoxToken(1, totals.TotalActiveLearners,previousTotals != null ? previousTotals.TotalActiveLearners : 0,avgLogin),
                        ReportEnums.eStatsTypes.VideoPreviewWatched.Type2LearnerPeriodStatsBoxToken(2, totals.TotalVideoPreveiwWatched,previousTotals != null ? previousTotals.TotalVideoPreveiwWatched : 0),
                        ReportEnums.eStatsTypes.CoursePreviewEntered.Type2LearnerPeriodStatsBoxToken(3, totals.TotalCoursePreviewEntered,previousTotals != null ? previousTotals.TotalCoursePreviewEntered: 0),
                        ReportEnums.eStatsTypes.PurchasePageEntered.Type2LearnerPeriodStatsBoxToken(4, totals.TotalPurchasePageEntered,previousTotals != null ? previousTotals.TotalPurchasePageEntered: 0,buyCompltete),
                        ReportEnums.eStatsTypes.AvgVideosWatchedPerCourse.Type2LearnerPeriodStatsBoxToken(5, avgWatched.FormatDecimal(2),prevAvgWatched.FormatDecimal(2)),
                        ReportEnums.eStatsTypes.TotalVideosWatched.Type2LearnerPeriodStatsBoxToken(6, totals.TotalVideosWatched,previousTotals != null ? previousTotals.TotalVideosWatched: 0)
                    };

                    return list;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Admin Dashboard learner period stats", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<LearnerPeriodStatsBoxToken>();
            }
        }

        public LearnerPeriodStatsBoxToken GetLearnerPeriodCouponStats(AdminDashboardFiltersToken filter)
        {
            try
            {
                var reportPeriod = PeriodSelection2DateRange(filter.period);
                var previousPeriod = Period2Previous(filter.period);

                using (var context = new lfeAuthorEntities())
                {

                    var totals = context.tvf_FACT_DASH_GetLearnerPeriodCouponStats(filter.currencyId,reportPeriod.from, reportPeriod.to).FirstOrDefault();
                    var currency = ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == filter.currencyId);
                    
                    if (totals == null || currency == null) return new LearnerPeriodStatsBoxToken();

                    var previousTotals = context.tvf_FACT_DASH_GetLearnerPeriodCouponStats(filter.currencyId, previousPeriod.from, previousPeriod.to).FirstOrDefault();

                    var couponValue = ReportEnums.eStatsTypes.LearnerCouponsClaimedValue.Type2LearnerPeriodStatsBoxToken(1, totals.TotalDiscount.FormatDecimal(2), previousTotals != null ? previousTotals.TotalDiscount.FormatDecimal(2) : 0);

                    couponValue.DisplayedValue = String.Format("{0}{1}",currency.Symbol,totals.TotalDiscount.FormatDecimal(2));

                    var box = ReportEnums.eStatsTypes.LearnerCouponsClaimed.Type2LearnerPeriodStatsBoxToken(1, totals.TotalCouponsClaimed,previousTotals != null ? previousTotals.TotalCouponsClaimed : 0,couponValue);

                    return box;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Admin Dashboard learner period coupon stats", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new LearnerPeriodStatsBoxToken();
            }
        }

        public List<SalesTotalsBoxToken> GetSalesTotals(AdminDashboardFiltersToken filter)
        {
            

            try
            {
                var reportPeriod = PeriodSelection2DateRange(filter.period);

                var previousPeriod = Period2Previous(filter.period);

                using (var context = new lfeAuthorEntities())
                {
                    var totals = context.tvf_FACT_DASH_GetPeriodSalesTotals(filter.currencyId,reportPeriod.from,reportPeriod.to).FirstOrDefault();

                    if (totals == null) return new List<SalesTotalsBoxToken>();

                    var rows = context.tvf_FACT_DASH_GetPeriodSalesStats(filter.currencyId,reportPeriod.from, reportPeriod.to).FirstOrDefault();
                    var previousRows = context.tvf_FACT_DASH_GetPeriodSalesStats(filter.currencyId, previousPeriod.from, previousPeriod.to).FirstOrDefault();
                    var currency     = ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == filter.currencyId);


                    var list = new List<SalesTotalsBoxToken>
                    {
                        ReportEnums.eStatsTypes.OneTimeSales.Type2SalesTotalsBoxToken(currency,1,totals.total_onetime_sales,rows != null ? rows.total_onetime_qty  : 0,previousRows != null ? previousRows.total_onetime_qty  : 0),
                        ReportEnums.eStatsTypes.Subscription.Type2SalesTotalsBoxToken(currency,2,totals.total_subscription_sales,rows != null ? rows.total_subscription_qty  : 0,previousRows != null ? previousRows.total_subscription_qty  : 0),
                        ReportEnums.eStatsTypes.Rental.Type2SalesTotalsBoxToken(currency,3,totals.total_rental_sales,rows != null ? rows.total_rental_qty  : 0,previousRows != null ? previousRows.total_rental_qty  : 0),
                        ReportEnums.eStatsTypes.Free.Type2SalesTotalsBoxToken(currency,4,0,rows != null ? rows.total_free_qty  : 0,previousRows != null ? previousRows.total_free_qty  : 0),
                        ReportEnums.eStatsTypes.MBG.Type2SalesTotalsBoxToken(currency,5,0,rows != null ? rows.total_mbg_qty  : 0,previousRows != null ? previousRows.total_mbg_qty  : 0)
                    };

                    return list;
              }
              
            }
            catch (Exception ex)
            {
                Logger.Error("Get sales totals stats for admin dashboard", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<SalesTotalsBoxToken>();
            }
        }
        
    }

    static class Extensions
    {
        public static TotalsBoxToken Type2TotalsBoxToken(this ReportEnums.eStatsTypes type,short index,int? total,int totalNew,int previousNew)
        {
            var box = new TotalsBoxToken
            {
                Type    = type
                ,Index  = index
                ,Total  = total ?? 0
                ,New    = totalNew
            };

            box.Tendency = box.New.Value2TendencyToken(previousNew);

            return box;
        }

        public static SalesTotalsBoxToken Type2SalesTotalsBoxToken(this ReportEnums.eStatsTypes type, BaseCurrencyDTO currency, short index, decimal? total, int totalNew, int previousNew)
        {
            var box = new SalesTotalsBoxToken
            {
                Type          = type
                ,Index        = index
                ,TotalIncome  = total ?? 0
                ,New          = totalNew
                ,Currency     = currency
            };

            box.Tendency = box.New.Value2TendencyToken(previousNew);

            return box;
        }

        public static AuthorPeriodStatsBoxToken Type2AuthorPeriodStatsBoxToken(this ReportEnums.eStatsTypes type,short index, int value, int previousValue)
        {
            return new AuthorPeriodStatsBoxToken
            {
                Type            = type
                ,Index          = index
                ,DisplayedValue = value.ToString()
                ,Tendency    = value.Value2TendencyToken(previousValue)
            };
        }
        
        public static AuthorPeriodStatsBoxToken Type2AuthorPeriodStatsBoxToken(this ReportEnums.eStatsTypes type,short index, decimal value, decimal previousValue)
        {
            return new AuthorPeriodStatsBoxToken
            {
                Type            = type
                ,Index          = index
                ,DisplayedValue = value.ToString()
                ,Tendency    = value.Value2TendencyToken(previousValue)
            };
        }

        public static LearnerPeriodStatsBoxToken Type2LearnerPeriodStatsBoxToken(this ReportEnums.eStatsTypes type, short index, int value, int previousValue,LearnerPeriodStatsBoxToken related = null)
        {
            return new LearnerPeriodStatsBoxToken
            {
                Type            = type
                ,Index          = index
                ,DisplayedValue = value.ToString()
                ,Tendency    = value.Value2TendencyToken(previousValue)
                ,Related        = related
            };
        }

        public static LearnerPeriodStatsBoxToken Type2LearnerPeriodStatsBoxToken(this ReportEnums.eStatsTypes type, short index, decimal value, decimal previousValue)
        {
            return new LearnerPeriodStatsBoxToken
            {
                Type            = type
                ,Index          = index
                ,DisplayedValue = value.ToString()
                ,Tendency    = value.Value2TendencyToken(previousValue)
            };
        }
    }
}
