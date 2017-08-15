using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Application.Services
{
    public class ReportServices : ServiceBase, IReportServices
    {
        private readonly DateTime FIRST_REPORT_DATE = DateTime.Parse("08/01/2012");

        private readonly short[] _kpi_event_types = {
                                                    (short) CommonEnums.eUserEvents.STORE_VIEW
                                                    ,(short) CommonEnums.eUserEvents.COURSE_VIEWER_ENTER
                                                    ,(short) CommonEnums.eUserEvents.VIDEO_PREVIEW_WATCH
                                                    ,(short) CommonEnums.eUserEvents.BUY_PAGE_ENTERED
                                                    ,(short) CommonEnums.eUserEvents.PURCHASE_COMPLETE
                                                    };
        #region private helpers
        private static DataTable ToDataTable(List<SummaryReportRowDTO> data, List<string> periods)// T is any generic type
        {
            var columnCount = 0;
            var table = new DataTable();
            var columns = periods;
            if (columns != null && columns.Count > 0)
            {

                for (var i = 0; i < columns.Count(); i++)
                {
                    table.Columns.Add(columns[i], typeof(string));
                }

                columnCount = columns.Count();
            }

            var rowTypes = Utils.EnumWithDescToList<ReportEnums.eSummaryRows>();

            foreach (var row in rowTypes)
            {
                var values = new object[columnCount];
                var t = Utils.ParseEnum<ReportEnums.eSummaryRows>(row.Value.ToString());
                switch (t)
                {
                    case ReportEnums.eSummaryRows.User:
                        for (var i = 0; i < periods.Count; i++)
                        {
                            var i1 = i;
                            values[i] = data.Where(x => x.Period == periods[i1]).Select(x => x.Users).FirstOrDefault().ToString();
                        }
                        break;
                    case ReportEnums.eSummaryRows.Authors:
                        for (var i = 0; i < periods.Count; i++)
                        {
                            var i1 = i;
                            values[i] = data.Where(x => x.Period == periods[i1]).Select(x => x.Authors).FirstOrDefault().ToString();
                        }
                        break;
                    case ReportEnums.eSummaryRows.Courses:
                        for (var i = 0; i < periods.Count; i++)
                        {
                            var i1 = i;
                            values[i] = data.Where(x => x.Period == periods[i1]).Select(x => x.Courses).FirstOrDefault().ToString();
                        }
                        break;
                    case ReportEnums.eSummaryRows.Sales:
                        for (var i = 0; i < periods.Count; i++)
                        {
                            var i1 = i;
                            values[i] = data.Where(x => x.Period == periods[i1]).Select(x => x.Sales).FirstOrDefault().ToString();
                        }
                        break;
                }

                table.Rows.Add(values);
            }

            return table;
        }
        #endregion

        #region interface implementation
        public List<UserEventDTO> GetUserEventLogs(int page, int size, int? userId, short? eventTypeId, ReportEnums.ePeriodSelectionKinds periodKind, int? courseId, int? bundleId, int? storeId, long? sessionId)
        {

            return GetUserEventLogs(periodKind, userId, eventTypeId, courseId, bundleId,storeId, sessionId);
            //try
            //{
            //    var dates = PeriodSelection2DateRange(periodKind);

            //    return UserRepository.GetEventLogs(userId,eventTypeId, dates.from, dates.to,courseId,bundleId,storeId,sessionId).OrderByDescending(x=>x.EventID).Select(x => x.Entity2UserEventDto()).ToList();
            //    //var rows =   UserRepository.GetEventLogs(userId,eventTypeId, dates.from, dates.to,courseId,bundleId,storeId,sessionId).OrderByDescending(x=>x.EventID).ToList();

            //    //var skip = (page - 1)*size;

            //    //var eventsList = rows.Skip(skip).Take(size).ToArray();
            //    //var events = eventsList.Select(x => x.Entity2UserEventDto()).OrderByDescending(x => x.EventDate).ToList();

            //    //var list = new List<UserEventDTO>();

            //    //for (var i = 0; i < (page - 1) * size; i++)
            //    //{
            //    //    list.Add(new UserEventDTO());
            //    //}

            //    //list.AddRange(events);

            //    //for (var i = page*size + 1; i <= rows.Count; i++)
            //    //{
            //    //    list.Add(new UserEventDTO());
            //    //}

            //    //return list;

            //}
            //catch (Exception ex)
            //{
            //    Logger.Error("get user events log", ex, userId,CommonEnums.LoggerObjectTypes.EventLogs);
            //    return new List<UserEventDTO>();
            //}
        }

        public List<UserEventDTO> GetUserEventLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, short? eventTypeId, int? courseId, int? bundleId,int? storeId, long? sessionId)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);

                return UserRepository.GetEventLogs(userId, eventTypeId, dates.from, dates.to, courseId, bundleId, storeId, sessionId).OrderByDescending(x => x.EventID).Select(x => x.Entity2UserEventDto()).ToList();
              
            }
            catch (Exception ex)
            {
                Logger.Error("get user events log", ex, userId, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<UserEventDTO>();
            }
        }

        public List<SystemLogDTO> GetSystemLogs(ReportEnums.ePeriodSelectionKinds periodKind,CommonEnums.LoggerObjectTypes? module, CommonEnums.eLogLevels? level,int? userId,long? sessionId,string ipAddress)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);

                var l = level == null ? null : level.ToString();
                var m = module == null ? null : module.ToString();
                
                return LogTableRepository.GetSystemLogs(dates.from,dates.to,m,l,userId,sessionId,ipAddress).Select(x => x.Entity2SystemLogDto()).OrderByDescending(x => x.id).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get system log", ex, null, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<SystemLogDTO>();
            }
        }

        public List<VideoDTO> GetVideosReport(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, bool? attachedOnly)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);

                return UserRepository.SearchUserVideos(dates.from, dates.to, userId, attachedOnly).Select(x => x.UserInfoVideoEntity2UserVideoDto()).OrderByDescending(x => x.addon).ToList();

                //var videos = UserVideosRepository.GetMany(x => DbFunctions.TruncateTime(x.CreationDate) >= DbFunctions.TruncateTime(dates.from) 
                //                                            && DbFunctions.TruncateTime(x.CreationDate) <= DbFunctions.TruncateTime(dates.to)  
                //                                            && (userId == null || x.UserId == userId)).ToList();
                
                //return (from video in videos where video.UserId != null select video.UserVideoEntity2UserVideoDto(UserRepository.GetById((int)video.UserId))).OrderByDescending(x=>x.addon).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get videos report", ex, userId, CommonEnums.LoggerObjectTypes.Author);
                return new List<VideoDTO>();
            }
        }

        public List<FileInterfaceLogDTO> GetFileInterfaceLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, ImportJobsEnums.eFileInterfaceStatus? status)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);

                return S3FileInterfaceRepository.GetFileInterfaceReport(dates.from, dates.to, userId,status)
                        .Select(x => x.LogToken2FileInterfaceLogDto())
                        .OrderByDescending(x => x.FileId)
                        .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get file interface log", ex, null, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<FileInterfaceLogDTO>();
            }
        }

        public List<FbPostInterfaceLogDTO> GetFbPostLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, FbEnums.ePostInterfaceStatus? status)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);

                return FacebookPostRepository.GetFbPostInterfaceReport(dates.from, dates.to, userId, status)
                        .Select(x => x.LogToken2FbPostInterfaceLogDto())
                        .OrderByDescending(x => x.PostId)
                        .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get fb posts interface log", ex, null, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<FbPostInterfaceLogDTO>();
            }
        }
        
        public List<EmailInterfaceLogDTO> GetEmailInterfaceLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, EmailEnums.eSendInterfaceStatus? status)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);

                return EmailMessageRepository.GetEmailInterfaceReport(dates.from, dates.to, userId, status)
                        .Select(x => x.LogToken2EmailInterfaceLogDto())
                        .OrderByDescending(x => x.EmailId)
                        .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get emails interface log", ex, null, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<EmailInterfaceLogDTO>();
            }
        }
        
        public List<ItemListDTO> GetAuthorItemsList(int? authorId, int? itemId, CourseEnums.CourseStatus? status)
        {
            return UserRepository.SearchUserItems(DEFAULT_CURRENCY_ID,authorId,itemId, status).Select(x => x.UserItemEntity2ItemListDto()).OrderByDescending(x => x.AddOn).ToList();
            //CourseRepository.GetUserCourses(authorId,status).Select(x => x.Entity2CourseListDTO()).OrderByDescending(x => x.AddOn).ToList();
        }

        public List<BaseListDTO> FindCourses(int? courseId = null, int? userId = null, string name = null)
        {
            try
            {
                if (courseId == null && userId == null && string.IsNullOrEmpty(name) ) return CourseRepository.GetAll().Select(x => x.CourseEntity2BaseListDto()).OrderBy(x => x.name).ToList();

                if (courseId != null)
                {
                    var course = CourseRepository.GetById((int)courseId).CourseEntity2BaseListDto();

                    return new List<BaseListDTO> { course };
                }

                if (userId != null)
                {
                    return CourseRepository.GetMany(x => x.AuthorUserId == userId).Select(x => x.CourseEntity2BaseListDto()).OrderBy(x => x.name).ToList();

                }

                if (!String.IsNullOrEmpty(name))
                {
                    return CourseRepository.GetMany(x => x.CourseName.ToLower().Contains(name.ToLower())).Select(x => x.CourseEntity2BaseListDto()).OrderBy(x => x.name).ToList();
                }

                //shouldn't arrive to this
               return new List<BaseListDTO>();
            }
            catch (Exception ex)
            {

                Logger.Error("search courses for admin", ex, CommonEnums.LoggerObjectTypes.UserAccount);
                return new List<BaseListDTO>();
            }
        }

     

        public List<BaseListDTO> FindBundles(int? bundleId = null, int? userId = null, string name = null)
        {
            try
            {
                if (bundleId == null && userId == null && string.IsNullOrEmpty(name)) return BundleRepository.GetAll().Select(x => x.BundleEntity2BaseListDto()).OrderBy(x => x.name).ToList();

                if (bundleId != null)
                {
                    var course = BundleRepository.GetById((int)bundleId).BundleEntity2BaseListDto();

                    return new List<BaseListDTO> { course };
                }

                if (userId != null)
                {
                    return BundleRepository.GetMany(x => x.AuthorId== userId).Select(x => x.BundleEntity2BaseListDto()).OrderBy(x => x.name).ToList();

                }

                if (!String.IsNullOrEmpty(name))
                {
                    return BundleRepository.GetMany(x => x.BundleName.ToLower().Contains(name.ToLower())).Select(x => x.BundleEntity2BaseListDto()).OrderBy(x => x.name).ToList();
                }

                //shouldn't arrive to this
                return new List<BaseListDTO>();
            }
            catch (Exception ex)
            {

                Logger.Error("search bundles for admin", ex, CommonEnums.LoggerObjectTypes.UserAccount);
                return new List<BaseListDTO>();
            }
        }

        public List<BaseEntityDTO> FindStores(int? storeId = null, int? userId = null, string name = null)
        {
            try
            {
                if (storeId == null && userId == null && string.IsNullOrEmpty(name)) return WebStoreRepository.GetAll().Select(x => x.Entity2BaseEntityDto()).OrderBy(x => x.name).ToList();

                if (storeId != null)
                {
                    var store = WebStoreRepository.GetById((int)storeId).Entity2BaseEntityDto();

                    return new List<BaseEntityDTO> { store };
                }

                if (userId != null)
                {
                    return WebStoreRepository.GetMany(x => x.OwnerUserID == userId && (name == null || x.StoreName.ToLower().Contains(name.ToLower()))).Select(x => x.Entity2BaseEntityDto()).OrderBy(x => x.name).ToList();

                }

                if (!String.IsNullOrEmpty(name))
                {
                    return WebStoreRepository.GetMany(x => x.StoreName.ToLower().Contains(name.ToLower())).Select(x => x.Entity2BaseEntityDto()).OrderBy(x => x.name).ToList();
                }

                //shouldn't arrive to this
                return new List<BaseEntityDTO>();
            }
            catch (Exception ex)
            {

                Logger.Error("search stores for admin", ex, CommonEnums.LoggerObjectTypes.UserAccount);
                return new List<BaseEntityDTO>();
            }
        }

        public List<StoreReportDTO> GetStoresReport(int? ownerId,CommonEnums.eRegistrationSources? source,bool? isAffiliate)
        {
            return WebStoreViewRepository.GetMany(x =>
                                                    (ownerId == null || x.OwnerUserID == ownerId) &&
                                                    (source == null || x.RegistrationSourceId == (short?) source) &&
                                                    (isAffiliate == null || x.AffiliateItems > 0))
                                        .Select(x => x.StoreViewEntity2StoreReportDto())
                                        .OrderByDescending(x=>x.StoreId)
                                        .ToList();
        } 

        public SummaryReportDTO GetSummaryReport(ReportEnums.ePeriodSelectionKinds periodKind, ReportEnums.eChartGroupping groupBy)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind, FIRST_REPORT_DATE,DateTime.Now.AddDays(1));

                var token = new SummaryReportDTO();

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.sp_ADMIN_GetSummaryReport(dates.from,dates.to,groupBy.EnumToLowerString()).ToList();

                    if (rows.Count <= 0)
                    {
                        token.Message = "no rows found";
                        token.IsValid = true;
                        
                        return token;
                    }
                    token.Periods                    = rows.Select(x => x.period).ToList();
                    token.Rows.AddRange(rows.Select(x=>x.Entity2ReportRowDto()));
                    token.DT                         = ToDataTable(token.Rows, token.Periods);
                }

                token.IsValid = true;

                return token;
            }
            catch (Exception ex)
            {
                var error = Utils.FormatError(ex);
                Logger.Error("get summary report", ex, CommonEnums.LoggerObjectTypes.Reports);

                return new SummaryReportDTO
                {
                    IsValid = false
                    ,Message = error
                };
            }
        }


        public List<HostEventDTO> GetHostEventsReport(ReportEnums.ePeriodSelectionKinds periodKind, ReportEnums.eChartGroupping groupBy,string host)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind, FIRST_REPORT_DATE, DateTime.Now.AddDays(1));

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.tvf_FACT_GetHostEvents(dates.from, dates.to, groupBy.EnumToLowerString(),String.IsNullOrEmpty(host) ? null : host).ToList();

                    return rows.Select(x => x.Entity2HostEventDto()).OrderBy(x => x.HostName).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Host events report",ex,CommonEnums.LoggerObjectTypes.Reports);
                return new List<HostEventDTO>();
            }
        }
        public SaleSummaryReportDTO GetSalesSummaryReport(int year, int month, int? userId)
        {
            try
            {
                var token = new SaleSummaryReportDTO();

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.sp_ADMIN_GetSalesSummaryReport(year,month,userId).ToList();

                    if (rows.Count <= 0)
                    {
                        token.Message = "no rows found";
                        token.IsValid = true;

                        return token;
                    }

                    var currencies = rows.GroupBy(x=>new{x.CurrencyId,x.CurrencyName,x.ISO,x.Symbol}).Select(x=>new BaseCurrencyDTO
                                                                                                                {
                                                                                                                    CurrencyId    = x.Key.CurrencyId ?? -1
                                                                                                                    ,CurrencyName = x.Key.CurrencyName ?? string.Empty
                                                                                                                    ,ISO          = x.Key.ISO ?? string.Empty
                                                                                                                    ,Symbol       = x.Key.Symbol ?? string.Empty
                                                                                                                }).ToList();
                    foreach (var currency in currencies)
                    {
                        token.CurrencyRows.Add(new PayoutCurrencySummaryDTO
                        {
                            Currency = currency
                            ,Rows = rows.Where(x => x.CurrencyId != null && x.CurrencyId == currency.CurrencyId).OrderByDescending(x=>x.total).Select(x => x.Entity2ReportRowDto(year, month)).ToList()
                        });
                    }
                }

                
                token.IsValid = true;

                return token;
            }
            catch (Exception ex)
            {
                var error = Utils.FormatError(ex);
                Logger.Error("get sale summary report", ex, CommonEnums.LoggerObjectTypes.Reports);

                return new SaleSummaryReportDTO
                {
                    IsValid = false
                    ,Message = error
                };
            }
        }

        //
        public List<AbandonHostDTO> GetHostAbandon(DateTime from, DateTime last)
        {
            try
            {
               
                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.tvf_FACT_GetAbandonHosts(from,last).ToList();

                    return rows.Select(x => x.Entity2HostEventStatsDto()).OrderByDescending(x => x.LastEventDate).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetHostEventsReportByLastEventDate", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<AbandonHostDTO>();
            }
        }

        #region kpi dashboard

        public KpiDashboardFiltersToken GetKpiDashboardFilters()
        {
            try
            {
                var token = new KpiDashboardFiltersToken();

                var authors = FactEventAggregatesViewRepository.GetMany(x => x.AuthorId != null).GroupBy(x => new {x.AuthorId, x.Nickname, x.FirstName, x.LastName}).ToList();

                foreach (var author in authors.Where(author => author.Key.AuthorId != null))
                {
                    token.Authors.Add(new BaseListDTO
                    {
                        id   = (int)author.Key.AuthorId,
                        name = DtoExtensions.CombineFullName(author.Key.FirstName, author.Key.LastName, author.Key.Nickname)
                    });
                }
               
                var stores = FactEventAggregatesViewRepository.GetMany(x => x.WebStoreId != null).GroupBy(x => new {x.WebStoreId, x.StoreName}).ToList();

                foreach (var store in stores.Where(store => store.Key.WebStoreId != null))
                {
                    token.Stores.Add(new BaseListDTO {id = (int) store.Key.WebStoreId, name = store.Key.StoreName});
                }

                var items = FactEventAggregatesViewRepository.GetMany(x => x.ItemId != null).GroupBy(x => new {x.ItemId, x.ItemName, x.ItemType}).ToList();


                foreach (var item in items.Where(item => item.Key.ItemId != null))
                {
                    token.Items.Add(new EventItemListDTO
                    {
                        id       = (int)item.Key.ItemId,
                        name     = item.Key.ItemName,
                        ItemType = Utils.ParseEnum<CommonEnums.eEventItemTypes>(item.Key.ItemType)
                    });
                }

                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("Kpi dashboard",ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new KpiDashboardFiltersToken();
            }
        }

        public List<KpiViewsChartDTO> GetKpiData(ReportEnums.ePeriodSelectionKinds period, int? authorId, int? storeId,int? itemId, CommonEnums.eEventItemTypes? itemType)
        {
            try
            {
                var dates = PeriodSelection2DateRange(period);

                var type = itemType == null ? string.Empty : itemType.ToString();

                var facts = FactEventAggregatesViewRepository.GetMany(x => DbFunctions.TruncateTime(x.EventDate) >= DbFunctions.TruncateTime(dates.from) && DbFunctions.TruncateTime(x.EventDate) <= DbFunctions.TruncateTime(dates.to)
                                                                && _kpi_event_types.Contains(x.EventTypeID)
                                                                && (authorId == null || x.AuthorId == authorId)
                                                                && (storeId == null || x.WebStoreId == storeId)
                                                                && (itemId == null || String.IsNullOrEmpty(type) || (x.ItemId==itemId && x.ItemType == type))).OrderBy(x=>x.EventDate).ToList();

                var curDate = facts.Min(x => x.EventDate).AddDays(-1);
                var endDate = facts.Max(x => x.EventDate);

                var datePoints = new List<DateTime>();

                while (curDate <= endDate)
                {
                    datePoints.Add(curDate);

                    curDate = curDate.AddDays(1);
                }

                var points = datePoints.ToList().Select(p => new KpiViewsChartDTO
                {
                    date = p,
                    authors = facts.Where(x =>x.AuthorId !=null && x.EventDate.Date == p.Date).Sum(t => t.EventCount),
                    stores = facts.Where(x => x.WebStoreId != null && x.EventDate.Date == p.Date ).Sum(t => t.EventCount),
                   // items = facts.Where(x => x.ItemId != null && x.EventDate.Date == p.Date).Sum(t => t.EventCount),
                    videos = UserVideosRepository.Count(x => DbFunctions.TruncateTime(x.CreationDate) == DbFunctions.TruncateTime(p) && (authorId==null || x.UserId==authorId))
                }).ToList();

                return points;
            }
            catch (Exception ex)
            {
                Logger.Error("Get Kpi data", ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<KpiViewsChartDTO>();
            }
        }

        public List<StatementKpiChartDTO> GetStatementKpiData(AuthorStatementRequestToken token)
        {
            try
            {
                var from = new DateTime(token.year, token.month, 1);

                var dates = new DateRangeToken
                {
                    from = from
                    ,to = from.AddMonths(1).AddSeconds(-1)
                };
                

                var facts = FactEventAggregatesViewRepository.GetMany(x => DbFunctions.TruncateTime(x.EventDate) >= DbFunctions.TruncateTime(dates.from) && DbFunctions.TruncateTime(x.EventDate) <= DbFunctions.TruncateTime(dates.to)
                                                                && _kpi_event_types.Contains(x.EventTypeID)
                                                                && (x.AuthorId == token.userId)).OrderBy(x => x.EventDate).ToList();

                var curDate = facts.Min(x => x.EventDate).AddDays(-1);
                var endDate = facts.Max(x => x.EventDate);

                var datePoints = new List<DateTime>();

                while (curDate <= endDate)
                {
                    datePoints.Add(curDate);

                    curDate = curDate.AddDays(1);
                }

                var points = datePoints.ToList().Select(p => new StatementKpiChartDTO
                {
                    date = p,
                    stores = facts.Where(x => x.WebStoreId != null && x.EventDate.Date == p.Date).Sum(t => t.EventCount),
                    items = facts.Where(x => x.ItemId != null && x.EventDate.Date == p.Date).Sum(t => t.EventCount),
                    sales = OrderRepository.Count(x => x.SellerUserId == token.userId && (DbFunctions.TruncateTime(x.OrderDate) == DbFunctions.TruncateTime(p.Date) ))
                }).ToList();

                return points;
            }
            catch (Exception ex)
            {
                Logger.Error("Get Kpi data", ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<StatementKpiChartDTO>();
            }
        }

        public List<KpiDetailRowDTO> GetKpiDataRows(ReportEnums.ePeriodSelectionKinds period, int? authorId,int? storeId, int? itemId, CommonEnums.eEventItemTypes? itemType)
        {
            try
            {
                var dates = PeriodSelection2DateRange(period);

                var type = itemType == null ? string.Empty : itemType.ToString();

                var facts = FactEventAggregatesViewRepository.GetMany(x => x.EventDate >= dates.from && x.EventDate <= dates.to
                                                                && _kpi_event_types.Contains(x.EventTypeID)
                                                                && x.ItemId != null
                                                                && (authorId == null || x.AuthorId == authorId)
                                                                && (storeId == null || x.WebStoreId == storeId)
                                                                && (itemId == null || String.IsNullOrEmpty(type) || (x.ItemId == itemId && x.ItemType == type))).ToList();


                var rows = facts.Where(x=>_kpi_event_types.Contains(x.EventTypeID))
                                .GroupBy(x => new
                                                {
                                                    x.AuthorId,
                                                    x.FirstName,
                                                    x.LastName,
                                                    x.Nickname,
                                                    x.WebStoreId,
                                                    x.StoreName,
                                                    x.ItemId,
                                                    x.ItemName,
                                                    x.ItemType
                                                })
                                .Select(x => new KpiDetailRowDTO
                                                {
                                                    Views = x.Sum(s=>s.EventCount)
                                                    ,Author = x.Key.AuthorId != null ? new BaseUserInfoDTO{UserId = (int)x.Key.AuthorId,FullName = DtoExtensions.CombineFullName(x.Key.FirstName, x.Key.LastName, x.Key.Nickname)} : new BaseUserInfoDTO{UserId = -1}
                                                    ,Store = x.Key.WebStoreId != null ? new BaseWebStoreDTO{StoreId = (int)x.Key.WebStoreId,Name = x.Key.StoreName} : new BaseWebStoreDTO{StoreId = -1}
                                                    ,Item = x.Key.ItemId != null ? new EventItemListDTO{id=(int)x.Key.ItemId,name = x.Key.ItemName,ItemType = Utils.ParseEnum<CommonEnums.eEventItemTypes>(x.Key.ItemType)} : new EventItemListDTO{id = -1}
                                                })
                                .ToList();

                return rows;
            }
            catch (Exception ex)
            {
                Logger.Error("Get Kpi data rows", ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<KpiDetailRowDTO>();
            }
        }

        public List<FunnelViewsChartDTO> GetFunnelChartData(ReportEnums.ePeriodSelectionKinds period, int? authorId,int? storeId, int? itemId, CommonEnums.eEventItemTypes? itemType)
        {
            try
            {
                var dates = PeriodSelection2DateRange(period);

                var type = itemType == null ? string.Empty : itemType.ToString();

                var facts = FactEventAggregatesViewRepository.GetMany(x => x.EventDate >= dates.from && x.EventDate <= dates.to
                                                               && _kpi_event_types.Contains(x.EventTypeID)
                                                               && (authorId == null || x.AuthorId == authorId)
                                                               && (storeId == null || x.WebStoreId == storeId)
                                                               && (itemId == null || String.IsNullOrEmpty(type) || (x.ItemId == itemId && x.ItemType == type))).ToList();

                var curDate = facts.Min(x => x.EventDate).AddDays(-1);
                var endDate = facts.Max(x => x.EventDate);

                var datePoints = new List<DateTime>();

                while (curDate <= endDate)
                {
                    datePoints.Add(curDate);

                    curDate = curDate.AddDays(1);
                }

                var points = datePoints.ToList().Select(p => new FunnelViewsChartDTO
                {
                    date = p,
                    ProductViews = facts.Where(x => x.EventTypeID == (byte)CommonEnums.eUserEvents.COURSE_VIEWER_ENTER && x.EventDate.Date == p.Date).Sum(t => t.EventCount),
                    ClipViews = facts.Where(x => x.EventTypeID == (byte)CommonEnums.eUserEvents.VIDEO_PREVIEW_WATCH && x.EventDate.Date == p.Date).Sum(t => t.EventCount),
                    BuyEntered = facts.Where(x => x.EventTypeID == (byte)CommonEnums.eUserEvents.BUY_PAGE_ENTERED && x.EventDate.Date == p.Date).Sum(t => t.EventCount),
                    PurchaseCompleted = facts.Where(x => x.EventTypeID == (byte)CommonEnums.eUserEvents.PURCHASE_COMPLETE && x.EventDate.Date == p.Date).Sum(t => t.EventCount)
                }).ToList();

                return points;
            }
            catch (Exception ex)
            {
                Logger.Error("Get funnel data", ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<FunnelViewsChartDTO>();
            }
        } 
        #endregion
        
        #region daily stats

        public List<FactDailyStatsDTO> GetDailyStatsData(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var dates = PeriodSelection2DateRange(period);

                return FactDailyStatsRepository.GetMany(x=>x.FactDate>=dates.from && x.FactDate <= dates.to).Select(x=>x.Entity2FactDailyStatsDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetDailyStatsData", ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<FactDailyStatsDTO>();
            }
        }

        public List<VideoUploadsChartDTO> GetDailyVideoStatsData(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var dates = PeriodSelection2DateRange(period);

                //var videos = UserVideosRepository.GetMany(x => DbFunctions.TruncateTime(x.CreationDate) >= DbFunctions.TruncateTime(dates.from) && DbFunctions.TruncateTime(x.CreationDate) <= DbFunctions.TruncateTime(dates.to)).ToList();
                
                //var curDate = dates.from;
                //var endDate = dates.to;

                //var datePoints = new List<DateTime>();

                //while (curDate <= endDate)
                //{
                //    datePoints.Add(curDate);

                //    curDate = curDate.AddDays(1);
                //}
                //var list = new List<VideoUploadsChartDTO>();
                
                //foreach (var datePoint in datePoints.OrderBy(x=>x.Date).ToList())
                //{
                //    list.Add(new VideoUploadsChartDTO
                //    {
                //         UploadDate      = datePoint
                //        ,TotalVideos     = videos.Count(x=> x.CreationDate.Date == datePoint.Date)
                //        ,TotalUsedVideos = videos.Count(x=> x.CreationDate.Date == datePoint.Date && x.Attached2Chapter)
                //    });
                //}

                var list = new List<VideoUploadsChartDTO>();
                
                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.sp_ADMIN_GetVideoStats(dates.from, dates.to).ToList();

                    list.AddRange(rows.Select(x=>new VideoUploadsChartDTO{UploadDate = x.period,TotalVideos = x.total,TotalUsedVideos = x.used}));
                }

                return list;

            }
            catch (Exception ex)
            {
                Logger.Error("GetDailyVideoStatsData", ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<VideoUploadsChartDTO>();
            }
        }

        public List<VideoDTO> GetDailyStatsVideoData(DateTime date, ReportEnums.eDailyStatsFields field)
        {
            try
            {
                var videos = new List<USER_Videos>();

                switch (field)
                {
                    case ReportEnums.eDailyStatsFields.TotalVideos:
                        videos = UserVideosRepository.GetMany(x => DbFunctions.TruncateTime(x.CreationDate) == DbFunctions.TruncateTime(date)).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.TotalUsedVideos:
                        videos = UserVideosRepository.GetMany(x => DbFunctions.TruncateTime(x.CreationDate) == DbFunctions.TruncateTime(date) && x.Attached2Chapter).ToList();
                        break;
                }

                return (from video in videos where video.UserId != null select video.UserVideoEntity2UserVideoDto(UserRepository.GetById((int)video.UserId))).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get daily stats video data for " + date + " for " + field, ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<VideoDTO>();
            }
        }

        public List<FactDailyTotalsDTO> GetDailyTotalsData(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var dates = PeriodSelection2DateRange(period);

                return FactDailyTotalsRepository.GetMany(x => x.FactDate >= dates.from && x.FactDate <= dates.to).Select(x => x.Entity2FactDailyTotalsDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetDailyTotalsData", ex, CommonEnums.LoggerObjectTypes.EventLogs);
                return new List<FactDailyTotalsDTO>();
            }
        }

        public List<UserGridViewDto> GetDailyStatsUserData(DateTime date, ReportEnums.eDailyStatsFields field)
        {
            try
            {
                var users = new List<UserGridViewDto>();

                switch (field)
                {
                    case ReportEnums.eDailyStatsFields.UsersCreated:
                        users = UserLoginsViewRepository.GetMany(x => DbFunctions.TruncateTime(x.Created) == DbFunctions.TruncateTime(date)).Select(x => x.LoginEntity2UserGridViewDto()).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.WixUsersCreated:
                        users = UserLoginsViewRepository.GetMany(x => x.RegistrationTypeId == (byte)CommonEnums.eRegistrationSources.WIX && DbFunctions.TruncateTime(x.Created) == DbFunctions.TruncateTime(date)).Select(x => x.LoginEntity2UserGridViewDto()).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.UserLogins:
                        users = FactDailyStatsRepository.GetDailyUserLogins(date,false).Select(x => x.LoginEntity2UserGridViewDto()).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.ReturnUsersLogins:
                        users = FactDailyStatsRepository.GetDailyUserLogins(date, true).Select(x => x.LoginEntity2UserGridViewDto()).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.AuthorLogins:
                        users = FactDailyStatsRepository.GetDailyAuthorLogins(date).Select(x => x.LoginEntity2UserGridViewDto()).ToList();
                        break;
                }

                return users;
            }
            catch (Exception ex)
            {
                Logger.Error("get daily stats user data for " + date + " for " + field,ex,CommonEnums.LoggerObjectTypes.Reports);
                return new List<UserGridViewDto>();
            }
        }
        
        public List<ItemListDTO> GetDailyStatsItemData(DateTime date, ReportEnums.eDailyStatsFields field)
        {
            try
            {
                var items = new List<ItemListDTO>();

                switch (field)
                {
                    case ReportEnums.eDailyStatsFields.ItemsCreated:
                        items = FactDailyStatsRepository.GetDailyItemStats(date,false).Select(x=>x.UserItemEntity2ItemListDto()).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.ItemsPublished:
                        items = FactDailyStatsRepository.GetDailyItemStats(date, true).Select(x => x.UserItemEntity2ItemListDto()).ToList();
                        break;
                }

                return items;
            }
            catch (Exception ex)
            {
                Logger.Error("get daily items data for " + date + " for " + field, ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<ItemListDTO>();
            }
        }

        public List<WixStoreDTO> GetDailyStatsStoreData(DateTime date, ReportEnums.eDailyStatsFields field)
        {
            try
            {
                var stores = new List<WixStoreDTO>();

                switch (field)
                {
                    case ReportEnums.eDailyStatsFields.StoresCreated:
                        stores = FactDailyStatsRepository.GetDailyStoreStats(date, false).Select(x => x.Entity2WixStoreDto()).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.WixStoresCreated:
                        stores = FactDailyStatsRepository.GetDailyStoreStats(date, true).Select(x => x.Entity2WixStoreDto()).ToList();
                        break;
                }


                return stores;
            }
            catch (Exception ex)
            {
                Logger.Error("get daily stats stores data for " + date + " for " + field, ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<WixStoreDTO>();
            }
        }

        public List<OrderLineDTO> GetDailyStatsPurchaseData(DateTime date, ReportEnums.eDailyStatsFields field)
        {
            try
            {
                var lines = new List<OrderLineDTO>();

                switch (field)
                {
                    case ReportEnums.eDailyStatsFields.ItemsPurchased:
                        lines = OrderLinesViewRepository.GetMany(x => DbFunctions.TruncateTime(x.OrderDate) == DbFunctions.TruncateTime(date)).Select(x => x.Entity2OrderLineDto()).ToList();
                        break;
                    case ReportEnums.eDailyStatsFields.FreeItemsPurchased:
                        const decimal min = (decimal) 0.01;
                        lines = OrderLinesViewRepository.GetMany(x => DbFunctions.TruncateTime(x.OrderDate) == DbFunctions.TruncateTime(date) && x.TotalPrice <= min).Select(x => x.Entity2OrderLineDto()).ToList();
                        break;
                }

                return lines;
            }
            catch (Exception ex)
            {
                Logger.Error("get daily stats purchase data for " + date + " for " + field, ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<OrderLineDTO>();
            }
        }
        #endregion

        public List<FactOwnerDailyEventStatsDTO> GetOwnerEventStatsData(ReportEnums.ePeriodSelectionKinds period)
        {
            try
            {
                var results = new List<FactOwnerDailyEventStatsDTO>();
                var dates = PeriodSelection2DateRange(period);

                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.tvf_FACT_GetOwnerEventsDailyStats(dates.from, dates.to).ToList();

                    var points = rows.Where(x => x.EventDate != null).GroupBy(x => new { FactDate = DateTime.Parse(x.EventDate.ToString()) }).ToArray();

                    results.AddRange(from point in points
                                             let current = point
                                             let common  = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.OwnerTypeId == (byte)CommonEnums.eEventOwnerTypes.Common)
                                             let learner = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.OwnerTypeId == (byte)CommonEnums.eEventOwnerTypes.Learner)
                                             let author  = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.OwnerTypeId == (byte)CommonEnums.eEventOwnerTypes.Author)
                                        select new FactOwnerDailyEventStatsDTO
                                        {
                                            FactDate      = point.Key.FactDate,
                                            TotalEvents   = rows.Where(x => x.EventDate == current.Key.FactDate).Sum(x => x.cnt),
                                            CommonEvents  = common != null ? common.cnt : 0,
                                            LearnerEvents = learner != null ? learner.cnt : 0,
                                            AuthorEvents  = author != null ? author.cnt : 0
                                        });
                }

                return results;
            }
            catch (Exception ex)
            {
                Logger.Error("get owner events stats data", ex, CommonEnums.LoggerObjectTypes.Reports);
                return new List<FactOwnerDailyEventStatsDTO>();
            }
        }
        public List<FactDailyEventStatsDTO> GetEventStatsData(ReportEnums.ePeriodSelectionKinds period, int? userId, short? eventTypeId, int? courseId, int? bundleId, int? storeId)
        {
            try
            {
                var results = new List<FactDailyEventStatsDTO>();
                var dates = PeriodSelection2DateRange(period);
                
                using (var context = new lfeAuthorEntities())
                {
                    var rows = context.tvf_FACT_GetEventsDailyStats(dates.from, dates.to,userId,courseId,bundleId,storeId,eventTypeId).ToList();

                    var points = rows.Where(x=>x.EventDate != null).GroupBy(x => new {FactDate = DateTime.Parse(x.EventDate.ToString()) }).ToArray();

                    results.AddRange(from point in points
                                            let current              = point
                                            let REGISTRATION_SUCCESS = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.REGISTRATION_SUCCESS)
                                            let LOGIN_SUCCESS        = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.LOGIN_SUCCESS)
                                            let VIDEO_PREVIEW_WATCH  = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.VIDEO_PREVIEW_WATCH)
                                            let VIDEO_COURSE_WATCH   = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.VIDEO_COURSE_WATCH)
                                            let BUY_PAGE_ENTERED     = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.BUY_PAGE_ENTERED)
                                            let PURCHASE_COMPLETE    = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.PURCHASE_COMPLETE)
                                            let DASHBOARD_VIEW       = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.DASHBOARD_VIEW)
                                            let COURSE_CREATED       = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.COURSE_CREATED)
                                            let COURSE_PUBLISHED     = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.COURSE_PUBLISHED)
                                            let COURSE_VIEWER_ENTER  = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.COURSE_VIEWER_ENTER)
                                            let COURSE_PREVIEW_ENTER = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.COURSE_PREVIEW_ENTER)
                                            let VIDEO_UPLOAD         = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.VIDEO_UPLOAD)
                                            let STORE_CREATED        = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.STORE_CREATED)
                                            let WIX_APP_PUBLISHED    = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.WIX_APP_PUBLISHED)
                                            let STORE_VIEW           = rows.FirstOrDefault(x => x.EventDate == current.Key.FactDate && x.TypeId == (int)CommonEnums.eUserEvents.STORE_VIEW)
                                        select new FactDailyEventStatsDTO
                                        {
                                                FactDate             = point.Key.FactDate,
                                                TotalEvents          = rows.Where(x => x.EventDate == current.Key.FactDate).Sum(x=>x.cnt), 
                                                REGISTRATION_SUCCESS = REGISTRATION_SUCCESS != null ? REGISTRATION_SUCCESS.cnt : 0,
                                                LOGIN_SUCCESS        = LOGIN_SUCCESS != null        ? LOGIN_SUCCESS.cnt : 0,
                                                VIDEO_PREVIEW_WATCH  = VIDEO_PREVIEW_WATCH != null  ? VIDEO_PREVIEW_WATCH.cnt : 0,
                                                VIDEO_COURSE_WATCH   = VIDEO_COURSE_WATCH != null   ? VIDEO_COURSE_WATCH.cnt : 0,
                                                BUY_PAGE_ENTERED     = BUY_PAGE_ENTERED != null     ? BUY_PAGE_ENTERED.cnt : 0,
                                                PURCHASE_COMPLETE    = PURCHASE_COMPLETE != null    ? PURCHASE_COMPLETE.cnt : 0,
                                                DASHBOARD_VIEW       = DASHBOARD_VIEW != null       ? DASHBOARD_VIEW.cnt : 0,
                                                COURSE_CREATED       = COURSE_CREATED != null       ? COURSE_CREATED.cnt : 0,
                                                COURSE_PUBLISHED     = COURSE_PUBLISHED != null     ? COURSE_PUBLISHED.cnt : 0,
                                                COURSE_VIEWER_ENTER  = COURSE_VIEWER_ENTER != null  ? COURSE_VIEWER_ENTER.cnt : 0,
                                                COURSE_PREVIEW_ENTER = COURSE_PREVIEW_ENTER != null ? COURSE_PREVIEW_ENTER.cnt : 0,
                                                VIDEO_UPLOAD         = VIDEO_UPLOAD != null         ? VIDEO_UPLOAD.cnt : 0,
                                                STORE_CREATED        = STORE_CREATED != null        ? STORE_CREATED.cnt : 0,
                                                WIX_APP_PUBLISHED    = WIX_APP_PUBLISHED != null    ? WIX_APP_PUBLISHED.cnt : 0,
                                                STORE_VIEW           = STORE_VIEW != null           ? STORE_VIEW.cnt : 0
                                        });
                }

                return results;
            }
            catch (Exception ex)
            {
                Logger.Error("get events stats data",ex,CommonEnums.LoggerObjectTypes.Reports);
                return new List<FactDailyEventStatsDTO>();
            }
        }

        public List<PluginRepDTO> GetInstallationsReport(ReportEnums.ePeriodSelectionKinds? period, int? userId, int? typeId, bool? isactive)
        {
            var dates = period != null ? PeriodSelection2DateRange((ReportEnums.ePeriodSelectionKinds)period) : null;

            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var entityList = context.tvf_APP_PluginsInstallationsRep(
                        period == null || period == ReportEnums.ePeriodSelectionKinds.all ? null : (DateTime?)dates.from,
                        period == null || period == ReportEnums.ePeriodSelectionKinds.all ? null : (DateTime?)dates.to,
                        typeId, userId, isactive).ToList();
                    return entityList.Select(x => x.Entity2PluginRepDTO()).ToList();
                }
            }
            catch(Exception e)
            {
                Logger.Error("get plugin installations report", e, CommonEnums.LoggerObjectTypes.Reports);
                return new List<PluginRepDTO>();
            }
        }


        #endregion

    }
}
