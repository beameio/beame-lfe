using LFE.Application.Services.ExternalProviders;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceBase = LFE.Application.Services.Base.ServiceBase;

namespace LFE.Application.Services
{
    public class PortalAdminServices : ServiceBase
    {
        private readonly IPaypalIpnServices _paypalIpnServices;
        private readonly IPaypalManageServies _paypalManageServies;
        //  private readonly LuckYServices _luckYServices = new LuckYServices();
        public PortalAdminServices()
        {
            _paypalIpnServices = DependencyResolver.Current.GetService<IPaypalIpnServices>();
            _paypalManageServies = DependencyResolver.Current.GetService<IPaypalManageServies>();
        }

        #region image resizing

        //public void DoImageResizing(out string error)
        //{
        //    error = string.Empty;
        //    var total = 0;
        //    var skipped = 0;
        //    var resized = 0;
        //    try
        //    {
        //        var courses = CourseRepository.GetAll().Select(x => new
        //        {
        //            id= x.Id
        //            ,name = x.CourseName
        //            ,image = x.SmallImage
        //        }).ToList();



        //        foreach (var course in courses)
        //        {
        //            if(String.IsNullOrEmpty(course.image)) continue;

        //            total++;

        //            try
        //            {
        //                var url = course.image.ToThumbUrl(ImageBaseUrl);

        //                var original = url.Url2Bitmap();

        //                if (original == null) continue;

        //                if (original.Width <= COURSE_THUMB_W)
        //                {
        //                    skipped++;
        //                    continue;
        //                }

        //                if (original.Height <= COURSE_THUMB_H)
        //                {
        //                    skipped++;
        //                    continue;
        //                }

        //                var msStream = new MemoryStream();

        //                var name = course.image.Replace(Path.GetFileName(course.image), Path.GetFileNameWithoutExtension(course.image) + String.Format(".{0}", ImageFormat.Jpeg.ToString().ToLower()));

        //                var image = ImageHelper.LimitBitmapSize(original, COURSE_THUMB_W, COURSE_THUMB_H);

        //                image.Save(msStream, ImageFormat.Jpeg);

        //                msStream.Seek(0, SeekOrigin.Begin);

        //                _s3Wrapper.Upload(name, "image/jpeg", msStream, out error);

        //                var courseEntity = CourseRepository.GetById(course.id);
        //                if (courseEntity != null)
        //                {
        //                    courseEntity.SmallImage = name;

        //                    CourseRepository.UnitOfWork.CommitAndRefreshChanges();

        //                }

        //                resized++;
        //            }
        //            catch (Exception)
        //            {

        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        error = Utils.FormatError(ex);
        //    }
        //}
        #endregion

        #region sales data conversion
        //public void DoDataMigration(out int totalRows, out int rowsProceed, out string error)
        //{
        //    totalRows = 0;
        //    rowsProceed = 0;
        //    error = string.Empty;

        //    try
        //    {
        //        using (var context = new lfeAuthorEntities())
        //        {
        //            totalRows = context.UserCourses.Count();

        //            foreach (var userCourse in context.UserCourses.OrderBy(x=>x.Created).ThenBy(x=>x.UserId).ThenBy(x=>x.CourseId).ToList())
        //            {
        //                var uc = userCourse;

        //                if(UserCourseRepository.IsAny(x=>x.UserId==uc.UserId && x.CourseId==uc.CourseId)) continue;

        //                var userId = uc.UserId;
        //                var courseId = uc.CourseId;

        //                var crs = CourseRepository.GetById(courseId);

        //                if (crs == null) continue;
        //                var itemName = String.Format("{0} - {1}", crs.CourseName,  "Unlimited Access");
        //                const BillingEnums.ePaymentTerms paymentTerm = BillingEnums.ePaymentTerms.IMMEDIATE;
        //                BillingEnums.ePaymentMethods paymentMethod;
        //                string externalTrxId = null;
        //                decimal price;
        //                int? couponInstanceId = null;
        //                int? storeId = null;
        //                DateTime pd;                        
        //                var trx = context.UserTransactions.Where(x=>x.TransactionUser==uc.UserId && x.TransactionCourse==uc.CourseId).ToArray();

        //                switch (trx.Count())
        //                {
        //                    case 0:
        //                        price = 0;
        //                        paymentMethod = BillingEnums.ePaymentMethods.Charge_Free;
        //                        pd = uc.Created;
        //                        break;
        //                    default:
        //                        var t = trx[0];
        //                        price = t.TransactionAmount > (decimal) 0.01 ? t.TransactionAmount : 0;
        //                        couponInstanceId = t.CouponInstanceId;
        //                        storeId = t.WebStoreID;
        //                        paymentMethod = price > 0 ? BillingEnums.ePaymentMethods.Paypal : BillingEnums.ePaymentMethods.Charge_Free;
        //                        externalTrxId = t.ExternalTransactionID;
        //                        pd = t.TransactionDate;
        //                        break;                            
        //                }

        //                var orderId = Guid.NewGuid();

        //                var orderCreated = DC_CreateSalesOrder(orderId, uc.Created,paymentTerm, userId, itemName, null, price,
        //                                                        courseId, null, (byte) paymentMethod, null, storeId, couponInstanceId, string.Empty,
        //                                                        out error);

        //                if(!orderCreated) continue;

        //                var lineId = GetOrderLineId(orderId, BillingEnums.eOrderLineTypes.SALE);

        //                if (lineId < 0) continue;
        //                if (price > 0)
        //                {
        //                    int paymentId;
        //                    if (DC_CreateOrderLinePayment(lineId, price, pd, pd, 1, BillingEnums.ePaymentStatuses.COMPLETED, BillingEnums.ePaymentTypes.ONE_TIME, out paymentId, out error)) DC_SaveSaleTransaction(lineId,paymentId,BillingEnums.eTransactionTypes.DirectPaymentTransaction, price,pd,externalTrxId,out error);                            
        //                }


        //                if (DC_AttachCourse2User(userId, lineId, courseId, out error))
        //                {
        //                    if (uc.LastChapterID != null)
        //                    {
        //                        UserCourseWatchStateRepository.Add(new USER_CourseWatchState
        //                        {
        //                            CourseId       = courseId
        //                            ,UserId        = userId
        //                            ,LastChapterID = uc.LastChapterID
        //                            ,LastVideoID   = uc.LastVideoID
        //                            ,LastViewDate  = uc.LastViewDate
        //                            ,AddOn         = DateTime.Now
        //                        });
        //                    }
        //                }

        //                rowsProceed++;



        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        error = Utils.FormatError(ex);
        //    }
        //}


        //private bool DC_AttachCourse2User(int userId, int orderLineId, int courseId, out string error)
        //{
        //    UserCourseRepository.Add(UserEntityMapper.NewUserCourseEntity(courseId, orderLineId, userId, null, BillingEnums.eAccessStatuses.ACTIVE));

        //    return UserCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);
        //}


        //private bool DC_CreateSalesOrder(Guid orderId, 
        //                            DateTime orderDate,
        //                            BillingEnums.ePaymentTerms paymentTerm, 
        //                            int userId,
        //                            string itemName, 
        //                            int? addressId, 
        //                            decimal price, 
        //                            int courseId, 
        //                            int? bundleId, 
        //                            byte paymentMethodId, 
        //                            Guid? paymentIntrsrumentId, 
        //                            int? storeId, 
        //                            int? couponInstanceId, 
        //                            string paypalProfileId, 
        //                            out string error)
        //{
        //    error = string.Empty;
        //    try
        //    {
        //        var sellerUserId = GetSellerUserId(courseId, bundleId, out error);

        //        if (sellerUserId < 0)
        //        {
        //            return false;
        //        }

        //        OrderLinePriceDTO priceToken;
        //        if (!ValidateOrderLinePrice(price, courseId, couponInstanceId, out priceToken, out error))
        //        {
        //            priceToken = new OrderLinePriceDTO
        //            {
        //               Price  = CourseRepository.GetById(courseId).PriceUSD ?? price
        //               ,FinalPrice = price
        //            };

        //            priceToken.Discount = priceToken.Price - priceToken.FinalPrice;
        //        }

        //        #region create  header
        //        var orderEntity = new SALE_Orders
        //        {
        //            OrderId          = orderId
        //            ,BuyerUserId     = userId
        //            ,SellerUserId    = sellerUserId
        //            ,PaymentMethodId = paymentMethodId
        //            ,InstrumentId    = paymentIntrsrumentId
        //            ,WebStoreId      = storeId
        //            ,AddressId       = addressId
        //            ,StatusId        = (byte)BillingEnums.eOrderStatuses.COMPLETE
        //            ,OrderDate       = orderDate
        //            ,AddOn           = orderDate
        //            ,CreatedBy       = userId
        //        };

        //        OrderRepository.Add(orderEntity);

        //        if (!OrderRepository.UnitOfWork.CommitAndRefreshChanges(out error))return false;
        //        #endregion

        //        #region create line
        //        var lineSaved = DC_CreateOrderLine(orderId,
        //                                            paymentTerm,
        //                                            BillingEnums.eOrderLineTypes.SALE,
        //                                            sellerUserId,
        //                                            itemName,
        //                                            courseId,
        //                                            bundleId,
        //                                            couponInstanceId,
        //                                            paypalProfileId,
        //                                            priceToken,
        //                                            out error);

        //        if (lineSaved) return true;

        //        OrderRepository.Delete(x=>x.OrderId==orderId);
        //        OrderRepository.UnitOfWork.CommitAndRefreshChanges();

        //        return false;

        //        #endregion
        //    }
        //    catch (Exception )
        //    {
        //        return false;
        //    }
        //}


        //private bool DC_CreateOrderLine(Guid orderId,
        //                                BillingEnums.ePaymentTerms paymentTerm,
        //                                BillingEnums.eOrderLineTypes lineType,
        //                                int sellerUserId,
        //                                string itemName,
        //                                int? courseId,
        //                                int? bundleId,
        //                                int? couponInstanceId,
        //                                string paypalProfileId,
        //                                OrderLinePriceDTO priceToken,
        //                                out string error)
        //{
        //    var lineEntity = new SALE_OrderLines
        //    {
        //        OrderId           = orderId
        //        ,LineTypeId       = (byte)lineType
        //        ,SellerUserId     = sellerUserId
        //        ,ItemName         = itemName
        //        ,CourseId         = courseId
        //        ,BundleId         = bundleId
        //        ,CouponInstanceId = couponInstanceId
        //        ,Price            = priceToken.Price
        //        ,Discount         = priceToken.Discount
        //        ,TotalPrice       = priceToken.FinalPrice
        //        ,PaypalProfileID  = paypalProfileId
        //        ,PaymentTermId    = (byte)paymentTerm
        //        ,AddOn            = DateTime.Now
        //        ,CreatedBy        = CurrentUserId
        //    };

        //    OrderLineRepository.Add(lineEntity);

        //    return OrderLineRepository.UnitOfWork.CommitAndRefreshChanges(out error);
        //}

        //private void DC_SaveSaleTransaction(int lineId, int? paymentId, BillingEnums.eTransactionTypes type, decimal amount, DateTime trxDate, string externalTrxId, out string error)
        //{
        //    TransactionRepository.Add(new SALE_Transactions
        //    {
        //        OrderLineId            = lineId
        //        ,PaymentId             = paymentId				
        //        ,Amount                = amount
        //        ,TransactionTypeId     = (byte)type
        //        ,TransactionDate       = trxDate
        //        ,ExternalTransactionID = externalTrxId
        //        ,Fee                   = 0				
        //        ,AddOn                 = DateTime.Now
        //    });

        //    TransactionRepository.UnitOfWork.CommitAndRefreshChanges(out error);            
        //}

        //private bool DC_CreateOrderLinePayment(int lineId, 
        //                                    decimal amount, 
        //                                    DateTime? paymentDate, 
        //                                    DateTime scheduledDate, 
        //                                    short paymentNum, 
        //                                    BillingEnums.ePaymentStatuses status, 
        //                                    BillingEnums.ePaymentTypes type, 
        //                                    out int paymentId, 
        //                                    out string error)
        //{

        //    paymentId = -1;

        //    var paymentEntity = new SALE_OrderLinePayments
        //    {
        //        OrderLineId    = lineId
        //        ,Amount        = amount
        //        ,Currency      = "USD"
        //        ,PaymentDate   = paymentDate
        //        ,ScheduledDate = scheduledDate
        //        ,PaymentNumber = paymentNum
        //        ,StatusId      = (byte)status
        //        ,TypeId        = (byte)type
        //        ,AddOn         = DateTime.Now
        //    };

        //    OrderLinePaymentRepository.Add(paymentEntity);

        //    if (!OrderLinePaymentRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

        //    paymentId = paymentEntity.PaymentId;

        //    return true;
        //}



        // private bool ValidateOrderLinePrice(decimal? price, int? courseId, int? couponInstanceId, out OrderLinePriceDTO priceToken, out string error)
        //{
        //    priceToken = new OrderLinePriceDTO();
        //    error = string.Empty;

        //    try
        //    {
        //        if (courseId == null)
        //        {
        //            error = "course or bundle required";
        //            return false;
        //        }

        //        if (price == null)
        //        {
        //            error = "price required";
        //            return false;
        //        }
        //        var itemPrice = CourseRepository.GetById((int)courseId).PriceUSD;
        //        if (itemPrice == null)
        //        {
        //            error = "price not defined";
        //            return false;
        //        }
        //        priceToken.Price = (decimal)itemPrice;

        //        if (couponInstanceId != null)
        //        {
        //            var couponResult = ValidateCoupon(courseId, (int)couponInstanceId);

        //            if (!couponResult.IsValid)
        //            {
        //                error = couponResult.Message;
        //                return false;
        //            }

        //            priceToken.Discount = couponResult.Discount;
        //            priceToken.FinalPrice = couponResult.FinalPrice;
        //        }
        //        else
        //        {
        //            priceToken.Discount = 0;
        //            priceToken.FinalPrice = priceToken.Price;
        //        }

        //        if (priceToken.FinalPrice == (decimal)price) return true;

        //        error = "price not matched";
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        error = Utils.FormatError(ex);
        //        return false;
        //    }

        //}

        //public CouponValidationToken ValidateCoupon(int? courseId,int couponInstanceId)
        //{
        //    var result = new CouponValidationToken
        //    {
        //        IsValid = false
        //        ,Discount = 0
        //    };

        //    decimal basePrice;

        //    if (courseId != null)
        //    {

        //        var course = CourseRepository.GetById((int) courseId);

        //        if (course == null)
        //        {
        //            result.Message = "course not found";
        //            return result;
        //        }

        //        var itemPrice = course.CourseEntity2Price(false);

        //        if (itemPrice == null)
        //        {
        //            result.Message = "invalid price";
        //            return result;
        //        }

        //        basePrice = (decimal) itemPrice;
        //    }
        //     else
        //    {
        //        result.Message = "course or bundle required";                
        //        return result;
        //    }

        //    result.OriginalPrice = basePrice;
        //    result.FinalPrice    = basePrice;

        //    var inst = CouponInstanceRepository.GetById(couponInstanceId);
        //    if (inst == null)
        //    {
        //        result.Message = "coupon instance not found";
        //        return result;
        //    }

        //    var coupon = CouponRepository.GetById(inst.CouponId).Entity2CourseCouponDTO();
        //    if (coupon == null)
        //    {
        //        result.Message = "coupon not found";
        //        return result;
        //    }

        //    switch (coupon.Type)
        //    {
        //        case CourseEnums.CouponType.PERCENT:                
        //            result.Discount = (decimal)(coupon.Amount != null ? basePrice * (coupon.Amount / 100) : 0);
        //            result.FinalPrice = CalculateDiscountedPrice(basePrice, coupon.Amount ?? 0, CourseEnums.CouponType.PERCENT);                   
        //            break;
        //        case CourseEnums.CouponType.FIXED:
        //                result.Discount = coupon.Amount ?? 0;
        //                result.FinalPrice = CalculateDiscountedPrice(basePrice, coupon.Amount ?? 0, CourseEnums.CouponType.FIXED);                        
        //            break;
        //        case CourseEnums.CouponType.FREE:
        //            result.Discount = basePrice;
        //            result.FinalPrice = 0;
        //            break;
        //    }

        //    return result;
        //}

        //private static decimal CalculateDiscountedPrice(decimal price, decimal couponDiscountAmount, CourseEnums.CouponType type)
        //{
        //    try
        //    {
        //        var finalPrice = price;

        //        switch (type)
        //        {
        //            case CourseEnums.CouponType.PERCENT:
        //                finalPrice = price - (price * (couponDiscountAmount / 100));
        //                break;
        //            case CourseEnums.CouponType.FIXED:
        //                finalPrice = Math.Max(price - couponDiscountAmount, 0);
        //                break;
        //            case CourseEnums.CouponType.FREE:
        //                return 0;
        //        }

        //        return decimal.Round(finalPrice, 2, MidpointRounding.AwayFromZero);
        //    }
        //    catch (Exception)
        //    {
        //        return price;
        //    }
        //} 

        public void RunIpnTrxDetailRequests()
        {
            try
            {
                var requests = PaypalIpnLogRepository.GetMany(x => x.txn_type == "cart" || x.txn_type == "paypal_here").ToList();
                string error;
                foreach (var log in requests)
                {
                    var token = log.response.Qs2IpnResponseToken(out error);

                    if (token == null) continue;

                    _paypalIpnServices.HandleIpnResponse(token);
                }
            }
            catch (Exception)
            {

            }
        }

        public void UpdateMerchantTrxFee()
        {

            try
            {
                var t = _paypalManageServies.GetMerchantTrxDetails("7GX11831J5208734L");
                return;
                //				var types = new List<byte>
                //				{
                //					(byte) BillingEnums.eTransactionTypes.Refund,
                //					(byte) BillingEnums.eTransactionTypes.PartialRefund
                //				};
                //
                //				var list =TransactionsViewRepository.GetMany( x => x.Fee == 0 && x.Amount > 0 &&
                //																	x.TransactionDate.Year >= 2014 &&
                //																	types.Contains(x.TransactionTypeId)).ToList();
                //				var cnt = 0;
                //
                //				foreach (var row in list)
                //				{
                //					var trx = _paypalManageServies.GetMerchantTrxDetails(row.ExternalTransactionID);
                //		
                //					if(trx ==null || trx.Ack != AckCodeType.SUCCESS) continue;
                //
                //					var fee = trx.PaymentTransactionDetails.PaymentInfo.FeeAmount.value;
                //
                //					if(fee==null) continue;
                //
                //					var entity = TransactionRepository.GetById(row.TransactionId);
                //
                //					if(entity==null) continue;
                //
                //					entity.Fee = Convert.ToDecimal(fee);
                //
                //					entity.UpdateDate = DateTime.Now;
                //
                //					if(!TransactionRepository.UnitOfWork.CommitAndRefreshChanges()) continue;
                //					cnt++;
                //				}
            }
            catch (Exception)
            {

            }
        }

        //public void UpdateRestTrxFee()
        //{

        //    try
        //    {
        //        var list =TransactionsViewRepository.GetMany(
        //                    x => x.Fee == 0 && x.Amount > 0 
        //                        && x.TransactionDate.Year >= 2014
        //                    //x.TransactionTypeId == (byte) BillingEnums.eTransactionTypes.InitialSubscriptionPayment &&
        //                       && (x.PaymentMethodId == (byte)BillingEnums.ePaymentMethods.Credit_Card) || x.PaymentMethodId == (byte)BillingEnums.ePaymentMethods.Saved_Instrument).ToList();
        //        var cnt = 0;

        //        foreach (var row in list)
        //        {
        //            var trx = _paypalManageServies.GetRestPaymentDetails(row.ExternalTransactionID);

        //            //if(trx ==null || trx.Ack != AckCodeType.SUCCESS) continue;

        //            //var fee = trx.PaymentTransactionDetails.PaymentInfo.FeeAmount.value;

        //            //if(fee==null) continue;

        //            //var entity = TransactionRepository.GetById(row.TransactionId);

        //            //if(entity==null) continue;

        //            //entity.Fee = Convert.ToDecimal(fee);

        //            //entity.UpdateDate = DateTime.Now;

        //            //if(!TransactionRepository.UnitOfWork.CommitAndRefreshChanges()) continue;
        //            cnt++;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        //public void ImportPrices()
        //{
        //    using (var context = new lfeAuthorEntities())
        //    {
        //        #region courses
        //        var courses = context.Courses.Where(x => !x.IsFreeCourse).ToList();

        //        foreach (var course in courses)
        //        {
        //            var courseId = course.Id;

        //            BILL_ItemsPriceList entity;

        //            if (course.PriceUSD != null)
        //            {
        //                if (context.BILL_ItemsPriceList.Any(x => x.ItemId == courseId 
        //                                                        && x.ItemTypeId == (byte)BillingEnums.ePurchaseItemTypes.COURSE 
        //                                                        && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.ONE_TIME 
        //                                                        && x.CurrencyId == DEFAULT_CURRENCY_ID)
        //                                                    ) continue;


        //                entity = new BILL_ItemsPriceList
        //                {
        //                    ItemId       = courseId
        //                    ,ItemTypeId  = (byte)BillingEnums.ePurchaseItemTypes.COURSE
        //                    ,PriceTypeId = (byte)BillingEnums.ePricingTypes.ONE_TIME
        //                    ,Price       = (decimal) course.PriceUSD
        //                    ,CurrencyId  = DEFAULT_CURRENCY_ID
        //                    ,IsDeleted   = false
        //                    ,AddOn       = DateTime.Now
        //                };

        //                context.BILL_ItemsPriceList.Add(entity);

        //                context.SaveChanges();
        //            }

        //            if (course.MonthlySubscriptionPriceUSD == null) continue;


        //            if (context.BILL_ItemsPriceList.Any(x => x.ItemId == courseId 
        //                                                     && x.ItemTypeId == (byte)BillingEnums.ePurchaseItemTypes.COURSE 
        //                                                     && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.SUBSCRIPTION
        //                                                     && x.PeriodTypeId == (byte)BillingEnums.eBillingPeriodType.MONTH
        //                                                     && x.CurrencyId == DEFAULT_CURRENCY_ID)
        //                ) continue;


        //                entity = new BILL_ItemsPriceList
        //                {
        //                    ItemId        = courseId
        //                    ,ItemTypeId   = (byte)BillingEnums.ePurchaseItemTypes.COURSE
        //                    ,PriceTypeId  = (byte)BillingEnums.ePricingTypes.SUBSCRIPTION
        //                    ,PeriodTypeId = (byte)BillingEnums.eBillingPeriodType.MONTH
        //                    ,Price        = (decimal) course.MonthlySubscriptionPriceUSD
        //                    ,CurrencyId   = DEFAULT_CURRENCY_ID
        //                    ,IsDeleted    = false
        //                    ,AddOn        = DateTime.Now
        //                };

        //                context.BILL_ItemsPriceList.Add(entity);

        //                context.SaveChanges();
        //        }
        //        #endregion

        //        #region bundle
        //        var bundles = context.CRS_Bundles.ToList();

        //        foreach (var bundle in bundles)
        //        {
        //            var courseId = bundle.BundleId;

        //            BILL_ItemsPriceList entity;

        //            if (bundle.Price != null)
        //            {
        //                if (context.BILL_ItemsPriceList.Any(x => x.ItemId == courseId 
        //                                                        && x.ItemTypeId == (byte)BillingEnums.ePurchaseItemTypes.BUNDLE 
        //                                                        && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.ONE_TIME 
        //                                                        && x.CurrencyId == DEFAULT_CURRENCY_ID)
        //                                                    ) continue;


        //                entity = new BILL_ItemsPriceList
        //                {
        //                    ItemId       = courseId
        //                    ,ItemTypeId  = (byte)BillingEnums.ePurchaseItemTypes.BUNDLE
        //                    ,PriceTypeId = (byte)BillingEnums.ePricingTypes.ONE_TIME
        //                    ,Price       = (decimal) bundle.Price
        //                    ,CurrencyId  = DEFAULT_CURRENCY_ID
        //                    ,IsDeleted   = false
        //                    ,AddOn       = DateTime.Now
        //                };

        //                context.BILL_ItemsPriceList.Add(entity);

        //                context.SaveChanges();
        //            }

        //            if (bundle.MonthlySubscriptionPrice == null) continue;


        //            if (context.BILL_ItemsPriceList.Any(x => x.ItemId == courseId 
        //                                                     && x.ItemTypeId == (byte)BillingEnums.ePurchaseItemTypes.BUNDLE 
        //                                                     && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.SUBSCRIPTION
        //                                                     && x.PeriodTypeId == (byte)BillingEnums.eBillingPeriodType.MONTH
        //                                                     && x.CurrencyId == DEFAULT_CURRENCY_ID)
        //                ) continue;


        //                entity = new BILL_ItemsPriceList
        //                {
        //                    ItemId        = courseId
        //                    ,ItemTypeId   = (byte)BillingEnums.ePurchaseItemTypes.BUNDLE
        //                    ,PriceTypeId  = (byte)BillingEnums.ePricingTypes.SUBSCRIPTION
        //                    ,PeriodTypeId = (byte)BillingEnums.eBillingPeriodType.MONTH
        //                    ,Price        = (decimal) bundle.MonthlySubscriptionPrice
        //                    ,CurrencyId   = DEFAULT_CURRENCY_ID
        //                    ,IsDeleted    = false
        //                    ,AddOn        = DateTime.Now
        //                };

        //                context.BILL_ItemsPriceList.Add(entity);

        //                context.SaveChanges();
        //        }
        //        #endregion
        //    }
        //}

        public void UpdateFreeCourses()
        {
            try
            {
                var lines = CourseRepository.GetMany(x => x.IsFreeCourse).ToList();

                foreach (var course in lines)
                {
                    var current = course;
                    var isExists = PriceListRepository.IsAny(x => x.ItemId == current.Id
                                                      && x.ItemTypeId == (byte)BillingEnums.ePurchaseItemTypes.COURSE
                                                      && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.FREE
                                                      && !x.IsDeleted);

                    if (isExists) continue;

                    var dto = new PriceLineDTO
                    {
                        ItemId = course.Id
                        ,
                        ItemType = BillingEnums.ePurchaseItemTypes.COURSE
                        ,
                        PriceType = BillingEnums.ePricingTypes.FREE
                        ,
                        Price = 0
                        ,
                        Currency = new BaseCurrencyDTO { CurrencyId = DEFAULT_CURRENCY_ID }
                    };

                    var entity = dto.Token2PriceLineEntity();

                    PriceListRepository.Add(entity);

                    PriceListRepository.UnitOfWork.CommitAndRefreshChanges();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        public void FixBundleCourseAccessProblem(out string emails)
        {
            emails = string.Empty;
            var from = DateTime.Now.AddDays(-3);
            var bundlePurchases = OrderLinesViewRepository.GetMany(x => x.BundleId != null && x.OrderDate > from).OrderByDescending(x => x.OrderDate).ToList();

            foreach (var orderLine in bundlePurchases)
            {
                var line = orderLine;
                var bundleCourses = BundleCourseRepository.Count(x => x.BundleId == line.BundleId);
                var userCourses = UserCourseRepository.Count(x => x.UserId == line.BuyerUserId && x.OrderLineId == line.LineId);

                if (bundleCourses == userCourses) continue;

                string error;
                AttachCourseOrBundle2User(line.BuyerUserId, line.LineId, null, line.BundleId, null, out error);
                emails += String.Format("{0}-{1}-{2}-{3}-{4},", line.BuyerEmail, line.SellerFirstName + " " + line.SellerLastName, line.OrderDate, bundleCourses, userCourses);
            }
        }
        #endregion

        #region  video 

        public long GetTotalVideosDuration()
        {
            long totalSeconds = 0;
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var videos = context.USER_Videos.ToList();

                    foreach (var video in videos)
                    {
                        var duration = video.Duration;

                        totalSeconds += duration.Duration2Seconds();
                    }

                    return totalSeconds;
                }

            }
            catch (Exception)
            {

                return -1;
            }
        }


        public void UpdateVideosState()
        {
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var videos = context.USER_Videos.ToList();

                    foreach (var video in videos)
                    {
                        var bcid = video.BcIdentifier.ToString();
                        var attached2Chapter = isVideoAttached(bcid);

                        video.Attached2Chapter = attached2Chapter;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public int FindMissingRenditions()
        {
            var cnt = 0;
            var s3 = new S3Wrapper();
            using (var ctx = new lfeAuthorEntities())
            {
                var renditions = ctx.USER_VideosRenditions.OrderByDescending(x => x.InsertDate).ToList();

                foreach (var rend in renditions)
                {
                    var key = rend.CloudFrontPath.Replace("https://courses-videos-prod.beame.io/","").Replace("http://uservideos.lfe.com/","");
                    var meta = s3.GetS3FileMetaData(key, S3_VIDEO_BUCKET_NAME);
                    if (meta == null)
                    {
                        cnt++;
                    }
                }

            }

            return cnt;
        }
        #endregion

        #region stores

        private void UpdateStoreRegistrationSource(WebStores entity, CommonEnums.eRegistrationSources source)
        {
            entity.RegistrationSourceId = (byte)source;
            WebStoreRepository.UnitOfWork.CommitAndRefreshChanges();
        }
        public void UpdateStoreRegistrationSources()
        {
            try
            {
                var _wepServices = new WidgetEndpointServices();
                var _fbServices = new FacebookServices();

                var stores = WebStoreRepository.GetMany(x => x.RegistrationSourceId == null).ToList().OrderByDescending(x => x.StoreID).ToArray();

                foreach (var store in stores)
                {
                    //check Wix
                    string error;
                    if (store.WixInstanceId != null || store.TrackingID.LastIndexOf("zombie", StringComparison.Ordinal) >= 0)
                    {
                        UpdateStoreRegistrationSource(store, CommonEnums.eRegistrationSources.WIX);

                        if (store.WixInstanceId != null)
                        {
                            Guid instanceId;
                            if (Guid.TryParse(store.WixInstanceId.ToString(), out instanceId))
                            {
                                if (instanceId != Guid.Empty)
                                {
                                    _wepServices.SavePluginInstallaltion(new PluginInstallationDTO
                                    {
                                        Type = PluginEnums.ePluginType.WIX
                                        ,
                                        Uid = instanceId.ToString()
                                        ,
                                        UserId = store.OwnerUserID
                                        ,
                                        AddOn = store.AddOn
                                        ,
                                        IsActive = true
                                    }, out error);

                                }
                            }
                        }
                        else
                        {
                            //check Zombie
                            var uid = store.TrackingID.Substring(store.TrackingID.LastIndexOf("_", StringComparison.Ordinal) + 1);

                            _wepServices.SavePluginInstallaltion(new PluginInstallationDTO
                            {
                                Type = PluginEnums.ePluginType.WIX
                                ,
                                Uid = uid
                                ,
                                UserId = store.OwnerUserID
                                ,
                                AddOn = store.AddOn
                                ,
                                IsActive = false
                            }, out error);
                        }

                        continue;
                    }


                    //check FB
                    long pageId;

                    if (Int64.TryParse(store.TrackingID, out pageId))
                    {

                        if (pageId > 1000000 && _fbServices.ValidatePage(pageId)) //fbPageUrl.IsUrlValid()
                        {
                            UpdateStoreRegistrationSource(store, CommonEnums.eRegistrationSources.FB);

                            var fbPageUrl = Utils.PageId2FacebookPageUrl(pageId);

                            var appUrl = fbPageUrl.FacebookPageUrl2AppUrl(pageId);

                            _wepServices.SavePluginInstallaltion(new PluginInstallationDTO
                            {
                                Type = PluginEnums.ePluginType.FB
                                ,
                                Uid = pageId.ToString()
                                ,
                                UserId = store.OwnerUserID
                                ,
                                AddOn = store.AddOn
                                ,
                                Domain = appUrl
                                ,
                                IsActive = !String.IsNullOrEmpty(appUrl)
                            }, out error);

                            continue;

                        }
                    }

                    //update as LFE store by default
                    UpdateStoreRegistrationSource(store, CommonEnums.eRegistrationSources.LFE);

                }
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateStoreRegistrationSources", ex, CommonEnums.LoggerObjectTypes.Application);
            }
        }
        #endregion

//        #region lucky api
//        public async Task ImportUserDataToProvision(int? userId)
//        {
//            var provisionUserServices = new ProvisionUserServices();
//
//            Logger.Debug("Begin import process to provision system on " + DateTime.Now);
//
//            using (var ctx = new lfeAuthorEntities())
//            {
//                var users = ctx.Users.Where(x => userId == null || x.Id == userId).ToList();
//
//                Logger.Debug("import process to provision system , total users found " + users.Count);
//
//                var usersImported = 0;
//
//                foreach (var user in users)
//                {
//                    string error;
//                    var saved = provisionUserServices.SaveUser(user.Id, out error);
//
//                    if (saved) usersImported++;
//                }
//
//                users = ctx.Users.Where(x => userId == null || x.Id == userId).ToList();
//
//                Logger.Debug("import process to provision system , total users imported " + usersImported);
//
//                var userDataImported = 0;
//
//                foreach (var user in users)
//                {
//                    var saved = await provisionUserServices.ImportUserData(user.Id);
//
//                    if (saved) userDataImported++;
//                }
//
//                Logger.Debug("import process to provision system , total users data imported " + userDataImported);
//            }
//
//            Logger.Debug("End import process to provision system on " + DateTime.Now);
//        }
//
//        public async Task ImportProducts(int? userId, int? courseId)
//        {
//            var provisionProductServices = new ProvisionProductServices();
//
//            Logger.Debug("Begin product import process to provision system on " + DateTime.Now);
//
//            using (var ctx = new lfeAuthorEntities())
//            {
//                var courses = ctx.Courses.Where(x => (userId == null || x.AuthorUserId == userId) && (courseId == null || x.Id == courseId)).ToList();
//
//                Logger.Debug("import products process to provision system , total courses found " + courses.Count);
//
//                var coursesImported = 0;
//
//                foreach (var course in courses)
//                {
//                    var saved = await provisionProductServices.SaveCourse(course.Id, true);
//
//                    if (saved) coursesImported++;
//                }
//
//                Logger.Debug("import products process to provision system , total courses imported " + coursesImported);
//
//                var bundles = ctx.CRS_Bundles.Where(x => userId == null || x.AuthorId == userId).ToList();
//
//                Logger.Debug("import products process to provision system , total bundles found " + bundles.Count);
//
//                var bundlesImported = 0;
//
//                foreach (var bundle in bundles)
//                {
//                    var saved = await provisionProductServices.SaveCourse(bundle.BundleId, true);
//
//                    if (saved) bundlesImported++;
//                }
//
//                Logger.Debug("import products process to provision system , total bundles imported " + bundlesImported);
//            }
//
//            Logger.Debug("End import products process to provision system on " + DateTime.Now);
//        }
//        #endregion
    }

    public static class IpnExtensions
    {
        public static NameValueCollection Qs2NameValueCollection(this string qs, out string error)
        {
            error = string.Empty;
            try
            {
                var dict = new Dictionary<string, string>();
                var paramsArray = qs.Split(Convert.ToChar("&"));

                foreach (var s in paramsArray)
                {
                    var p = s.Split(Convert.ToChar("="));

                    string val;
                    if (dict.TryGetValue(p[0], out val)) continue;

                    if (p.Length < 2) continue;

                    dict.Add(p[0], HttpUtility.UrlDecode(p[1]));
                }

                var collection = new NameValueCollection();

                foreach (var pair in dict)
                {
                    collection.Add(pair.Key, pair.Value);
                }

                return collection;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return null;
            }
        }
        public static Dictionary<string, string> Qs2Dictionary(this string qs, out string error)
        {
            error = string.Empty;
            try
            {
                var dict = new Dictionary<string, string>();
                var paramsArray = qs.Split(Convert.ToChar("&"));

                foreach (var s in paramsArray)
                {
                    var p = s.Split(Convert.ToChar("="));

                    string val;
                    if (dict.TryGetValue(p[0], out val)) continue;

                    if (p.Length < 2) continue;

                    dict.Add(p[0], HttpUtility.UrlDecode(p[1]));
                }


                return dict;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return null;
            }
        }


        public static MassPaymentItemToken Dict2PaymentItemToken(this Dictionary<string, string> dict)
        {
            return dict.DictionaryToObject<MassPaymentItemToken>();
        }

        public static IpnResponseToken Qs2IpnResponseToken(this string qs, out string error)
        {
            error = string.Empty;
            try
            {
                var dict = new Dictionary<string, string>();
                var paramsArray = qs.Split(Convert.ToChar("&"));

                foreach (var s in paramsArray)
                {
                    var p = s.Split(Convert.ToChar("="));

                    string val;
                    if (dict.TryGetValue(p[0], out val)) continue;

                    if (p.Length < 2) continue;

                    dict.Add(p[0], HttpUtility.UrlDecode(p[1]));
                }

                var token = dict.DictionaryToObject<IpnResponseToken>();

                token.response_string = qs;

                return token;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return null;
            }
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection source)
        {
            var dict = new Dictionary<string, string>();

            foreach (var s in source.AllKeys)
            {
                var l = source[s].Split(Convert.ToChar(","));

                if (l.Count().Equals(0)) continue;
                dict.Add(s, l[0]);
            }

            return dict;
        }


    }
}