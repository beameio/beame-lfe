using System.Globalization;
using LFE.Application.Services.Base;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using PayPal.PayPalAPIInterfaceService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LFE.Application.Services
{
	public class BillingServices : ServiceBase, IBillingServices,IUserSubscriptionsManageServices, IBillingManageServices
	{
		private const string PURCHASE_ERROR_MSG_PREFIX = "Purchase process error";

		private readonly IEmailServices _emailServices;
		private readonly IAmazonEmailWrapper _amazonEmailWrapper;
		private readonly IWidgetCourseServices _courseServices;
		private readonly ICouponWidgetServices _couponWidgetServices;
		private readonly IPaypalManageServies _paypalManageServices;
		private readonly IPayoutServices _payoutServices;
		public BillingServices()
		{
			_emailServices         = DependencyResolver.Current.GetService<IEmailServices>();
			_amazonEmailWrapper    = DependencyResolver.Current.GetService<IAmazonEmailWrapper>();
			_courseServices        = DependencyResolver.Current.GetService<IWidgetCourseServices>();
			_couponWidgetServices  = DependencyResolver.Current.GetService<ICouponWidgetServices>();
			_paypalManageServices  = DependencyResolver.Current.GetService<IPaypalManageServies>();
			_payoutServices        = DependencyResolver.Current.GetService<IPayoutServices>();
		}

		#region private helpers
		private bool RefundRequestEntitySave(SALE_RefundRequests entity, int buyerId, int paymentId, out string error)
		{
			try
			{
				entity.AddOn        = DateTime.Now;
				entity.CreatedBy    = buyerId;
				entity.PaymentId    = paymentId;
				entity.ReferenceKey = Guid.NewGuid();
				entity.StatusId     = (byte)BillingEnums.eRefundRequestStatus.SUBMITTED;

				RefundRequestsRepository.Add(entity);
				
				return RefundRequestsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception e)
			{
				Logger.Error("refund request save", e, CommonEnums.LoggerObjectTypes.Billing);
				error = FormatError(e);
				return false;
			}
		}

		private void RefundRequestEntityError(SALE_RefundRequests entity, string error)
		{
			try
			{
				entity.Error    = error;
				entity.StatusId = (byte)BillingEnums.eRefundRequestStatus.ERROR;

				RefundRequestsRepository.Update(entity);
				
				RefundRequestsRepository.UnitOfWork.CommitAndRefreshChanges();
			  
			}
			catch (Exception e)
			{
				Logger.Error("refund request error", e, CommonEnums.LoggerObjectTypes.Billing);              
			}
		}

		private void RefundRequestEntitySuccess(SALE_RefundRequests entity, int refundId)
		{
			try
			{
				entity.StatusId = (byte)BillingEnums.eRefundRequestStatus.REFUNDED;
				entity.RefundId = refundId;

				RefundRequestsRepository.Update(entity);
				
				RefundRequestsRepository.UnitOfWork.CommitAndRefreshChanges();
			}
			catch (Exception e)
			{
				Logger.Error("refund request success", e, CommonEnums.LoggerObjectTypes.Billing);              
			}
		}

		private RequestPurchaseItemNameToken GetRequestPurchaseItemNameTokenFropPaypalRequest(PAYPAL_PaymentRequests request, out string error)
		{
			if (request == null)
			{
				error = "request entity not found";
				return null;
			}

			if (request.PriceLineId == null)
			{
				error = "priceLineId required";
				return null;
			}

			var priceToken = GetPriceLineToken((int)request.PriceLineId, out error);

			if (priceToken == null)
			{
				error = "price token not found";
				return null;
			}

			int itemId;
			BillingEnums.ePurchaseItemTypes type;

			if (request.CourseId != null)
			{
				itemId = (int)request.CourseId;
				type = BillingEnums.ePurchaseItemTypes.COURSE;
			}
			else if (request.BundleId != null)
			{
				itemId = (int)request.BundleId;
				type = BillingEnums.ePurchaseItemTypes.BUNDLE;
			}
			else
			{
				error = "courseId or bundleId required";
				return null;
			}

			return new RequestPurchaseItemNameToken
			{
				itemId        = itemId
				,type         = type
				,priceLineDto = priceToken
			};
		}
		private bool ValidatePaypalRequestEntity(PAYPAL_PaymentRequests request, out string itemName,out decimal amount, out string error)
		{
			itemName = string.Empty;
			amount = 0;

			var requestNameToken = GetRequestPurchaseItemNameTokenFropPaypalRequest(request, out error);

			if (requestNameToken == null) return false;

			if (request.Amount == null)
			{
				error = "amount required";
				return false;
			}

			amount = (decimal) request.Amount;

			itemName = CombineOrderItemName(requestNameToken);

			if (!String.IsNullOrEmpty(itemName)) return true;

			error = "item name not found";
			//  SendAdminMail(PURCHASE_ERROR_MSG_PREFIX, "immediate payment", error);
			return false;
		}

		private bool ValidateFreeCouponPurchase(ItemPurchaseDataToken token, out string itemName, out string error)
		{
			error = string.Empty;
			itemName = string.Empty;

			if (String.IsNullOrWhiteSpace(token.CouponCode))
			{
				error = "coupon code required";
				return false;
			}

			var requestToken = new RequestPurchaseItemNameToken
			{
				itemId        = token.ItemId
				,type         = token.Type
				,priceLineDto = token.PriceToken
			};

			itemName = CombineOrderItemName(requestToken);

			if (!String.IsNullOrEmpty(itemName)) return true;

			error = "item name not found";
			//  SendAdminMail(PURCHASE_ERROR_MSG_PREFIX, "immediate payment", error);
			return false;
		}

		public decimal? GetMerchantTransactionFee(string paypalTrxId)
		{
			try 
			{	        
				var details = _paypalManageServices.GetMerchantTransactionDetails(paypalTrxId);

				return details.Ack == AckCodeType.SUCCESS ? Decimal.Parse(details.PaymentTransactionDetails.PaymentInfo.FeeAmount.value) : (decimal?) null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public DateTime? ToItemAccessEndDate(int? priceLineId)
		{
			if (priceLineId == null) return null;

			string error;
			
			var priceLineToken = GetPriceLineToken((int)priceLineId, out error);
				
			if (priceLineToken == null) return null;

			if (priceLineToken.PriceType != BillingEnums.ePricingTypes.RENTAL) return null;

			if (priceLineToken.PeriodType != null && priceLineToken.NumOfPeriodUnits != null) return (Utils.ParseEnum<BillingEnums.eBillingPeriodType>(priceLineToken.PeriodType.ToString())).ToRentalEndDate((short)priceLineToken.NumOfPeriodUnits);

			return null;
		}

		//public decimal? GetRestTransactionFee(string paypalTrxId)
		//{
		//    try
		//    {
		//        var details = _paypalManageServies.GetRestSaleDetails(paypalTrxId);

		//        if (details.amount.details == null)
		//        {
		//            var p = _paypalManageServies.GetRestPaymentDetails(details.parent_payment);

		//            if (p.transactions.Any())
		//            {
		//                var trx = p.transactions[0];

		//                if (trx.amount.details != null) return trx.amount.details.fee != null ? Decimal.Parse(trx.amount.details.fee) : (decimal?)null;
		//            }
		//        }

		//        return details.amount.details != null && details.amount.details.fee != null ? Decimal.Parse(details.amount.details.fee) : (decimal?)null;
		//    }
		//    catch (Exception)
		//    {
		//        return null;
		//    }
		//}
		#endregion

		#region IUserBillingServices implementation
		#region helpers
		private void SendPurchaseNotifications(int orderLineId)
		{
		   // return;

			try
			{
				var lineEntity = OrderLinesViewRepository.Get(x => x.LineId == orderLineId);

				if (lineEntity == null)
				{
					Logger.Warn("purchase notification failed for orderLineId " + orderLineId);
					return;
				}

				
				var buyerToken = new UserInfoDTO
				{
					UserId     = lineEntity.BuyerUserId,
					FullName   = lineEntity.Entity2BuyerFullName(),
					FirstName  = lineEntity.BuyerFirstName,
					Email      = lineEntity.BuyerEmail,
					FacebookId = lineEntity.BuyerFacebookID
				};

				var sellerToken = new UserInfoDTO
				{
					UserId     = lineEntity.SellerUserId,
					FullName   = lineEntity.Entity2SellerFullName(),
					FirstName  = lineEntity.SellerFirstName,
					Email      = lineEntity.SellerEmail,
					FacebookId = lineEntity.SellerFacebookID
				};

				var itemToken = new BaseItemDTO {itemId = -1};

				if (lineEntity.CourseId != null)
				{
					var course = _courseServices.GetCourseToken(DEFAULT_CURRENCY_ID, (int)lineEntity.CourseId,lineEntity.TrackingID);

					if(course==null) return;

					itemToken.itemId   = course.CourseId;
					itemToken.name     = course.CourseName;
					itemToken.desc     = course.CourseDescription;
					itemToken.thumbUrl = course.ThumbUrl;
					itemToken.pageUrl  = course.CoursePageUrl;
					itemToken.type     = BillingEnums.ePurchaseItemTypes.COURSE;
					
				}


				if (lineEntity.BundleId != null)
				{
					var bundle = _courseServices.GetBundleToken(DEFAULT_CURRENCY_ID, (int)lineEntity.BundleId,lineEntity.TrackingID);

					itemToken.itemId   = bundle.BundleId;
					itemToken.name     = bundle.BundleName;
					itemToken.desc     = bundle.Description;
					itemToken.thumbUrl = bundle.ThumbUrl;
					itemToken.pageUrl  = bundle.BundlePageUrl;
					itemToken.type     = BillingEnums.ePurchaseItemTypes.BUNDLE;
				}
				
				CouponBaseDTO coupon = null;

				if (lineEntity.CouponInstanceId != null)
				{
					string error;
					coupon = _couponWidgetServices.GetCouponBaseToken((int) lineEntity.CouponInstanceId, out error);
				}
				
				var token = new EmailPurchaseToken
					{
						Buyer            = buyerToken
						,Seller          = sellerToken
						,Item            = itemToken
						,SentDate        = DateTime.Now
						,PaymentMethod   = Utils.ParseEnum<BillingEnums.ePaymentMethods>(lineEntity.PaymentMethodId)
						,PaymentTerm     = Utils.ParseEnum<BillingEnums.ePaymentTerms>(lineEntity.PaymentTermId)
						,Coupon          = coupon
						,PriceLineToken  = lineEntity.PriceLineId != null ? GetItemPriceToken((int)lineEntity.PriceLineId) : null
						,Price           = lineEntity.Price.FormatPrice()
						,Discount        = lineEntity.Discount.FormatPrice()
						,TotalAmount     = lineEntity.TotalPrice.FormatPrice()
					};

					_emailServices.SavePurchaseMessages(token,true);


					//post facebook messages
					var buyerMessageToken = new MessageUserDTO
					{
						id = lineEntity.BuyerUserId
						,name  = lineEntity.Entity2BuyerFullName()
						,email = lineEntity.BuyerEmail
						,fbUid = !string.IsNullOrEmpty(lineEntity.BuyerFacebookID) ? Int64.Parse(lineEntity.BuyerFacebookID) : (long?)null
					};


					var sellerNessageToken = new MessageUserDTO
					{
						id     = sellerToken.UserId
						,name  = sellerToken.FullName
						,email = sellerToken.Email
						,fbUid = !string.IsNullOrEmpty(sellerToken.FacebookId) ? Int64.Parse(sellerToken.FacebookId) : (long?)null
					};


					var itemMessageToken = new ItemMessageDTO
					{
						id             = itemToken.itemId
						,name          = itemToken.name
						,itemUrlName   = itemToken.pageUrl
						,desc          = itemToken.desc
						,thumbUrl      = itemToken.thumbUrl
					};

					var faceBookMessage = new PurchaseMessageDTO
					{
						AddOn   = DateTime.Now
						,Item   = itemMessageToken
						,Author = sellerNessageToken
						,Buyer  = buyerMessageToken
					};

					_courseServices.PostFasebookPurchaseMessages(faceBookMessage);
			}
			catch (Exception ex)
			{
				Logger.Error("save purchase notifications",ex,CommonEnums.LoggerObjectTypes.Billing);
			}
		}

		
		#endregion
		public bool SaveBillingAgreementToken(int userId,string token, out string error)
		{
			try
			{
				var entity = UserPaymentInstrumentsRepository.Get(x => x.UserId == userId);

				if (entity == null)
				{
					UserPaymentInstrumentsRepository.Add(userId.UserIdToUserPrefrenceEntityWithAgreementToken(token));
				}
				else
				{
					entity.UpdateAgreementToken(token);
				}

				return UserPaymentInstrumentsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save user agreement token", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}		
		public bool IsCreditCardExists(int userId)
		{
			try
			{
				var entity = UserPaymentInstrumentsRepository.Get(x => x.UserId == userId);
				return entity != null && !String.IsNullOrEmpty(entity.PaypalCcToken);
			}
			catch (Exception ex)
			{
				Logger.Error("saved cc::check if card exists::", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}

		public bool CompleteFreeCouponRequest(ItemPurchaseDataToken token, int buyerId, out int orderNo, out string error)
		{
			orderNo = -1;

			try
			{
				string itemName;
				

				if (!ValidateFreeCouponPurchase(token, out itemName, out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::validation failed", "free coupon payment", error);
					return false;
				}
			  
				
				var courseId         = token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?)null;
				var bundleId         = token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?)null;
				var couponInstanceId = RequestEntity2CouponInstanceId(  token.Author.userId, 
																		token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?)null, 
																		token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?)null, 
																		token.CouponCode);

				if (couponInstanceId == null)
				{
					error = "coupon instance not found";
					return false;
				}

				var couponResult = _couponWidgetServices.ValidateCoupon(token.PriceToken.PriceLineID,token.Author.userId,courseId, bundleId, (int)couponInstanceId);

				if (!couponResult.IsValid || !couponResult.IsFree)
				{
					error = String.IsNullOrEmpty(couponResult.Message) ? "coupon validation failure. please contact support team" : couponResult.Message;
					return false;
				}

				var storeId          = RequestEntity2WebStoreId(token.TrackingID);
				var orderId          = Guid.NewGuid();
			   

				if (!CreateSalesOrder(  orderId,
										token.PriceToken.PriceLineID,
										BillingEnums.ePaymentTerms.IMMEDIATE, 
										buyerId,
										itemName,
										null, 
										0, 
										courseId,
										bundleId,
										BillingEnums.ePaymentMethods.Charge_Free, 
										null, 
										storeId, 
										couponInstanceId, 
										string.Empty,
										out orderNo,
										out error)) return false;

				var lineId = GetOrderLineId(orderId, BillingEnums.eOrderLineTypes.FREE);

				if (lineId < 0)
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::order line not found", "free coupon payment", error);
					return false;
				}

				 //create user transaction record                
				var transactionSaved =  SaveSaleTransaction(lineId,null,null,BillingEnums.eTransactionTypes.FreeCouponCourse,0,DateTime.Now,null,null,null,"free coupon payment",null,out error);

				if (transactionSaved)
				{
					SendPurchaseNotifications(lineId);
                    var validUntil = ToItemAccessEndDate(token.PriceToken.PriceLineID);
					return AttachCourseOrBundle2User(buyerId, lineId,courseId, bundleId,validUntil, out error);
				}

				SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create sale transaction failed", "free coupon payment", error);
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::Complete free coupon request", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}

		public bool CompleteFreeCourseRequest(int courseId, int buyerId,int priceLineId, string trackingId, out int orderNo, out string error)
		{
			error = string.Empty;
			orderNo = -1;
			try
			{
				var entity = CourseRepository.GetById(courseId);

				if (entity == null)
				{
					error = "course entity not found";
					return false;
				}

				if (!entity.IsFreeCourse)
				{
					error = "course not free";
					return false;
				}

				if (UserCourseRepository.IsAny(x => x.CourseId == courseId && x.UserId == buyerId)) return true;

				var itemName         = entity.CourseName;
				var storeId          = RequestEntity2WebStoreId(trackingId);
				var orderId          = Guid.NewGuid();
				

				if (!CreateSalesOrder(  orderId,
										priceLineId,
										BillingEnums.ePaymentTerms.IMMEDIATE, 
										buyerId,
										itemName,
										null, 
										0, 
										courseId,
										null,
										BillingEnums.ePaymentMethods.Charge_Free, 
										null, 
										storeId, 
										null, 
										string.Empty,
										out orderNo,
										out error)) return false;

				var lineId = GetOrderLineId(orderId, BillingEnums.eOrderLineTypes.SALE);

				if (lineId < 0)
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::order line not found", "free course payment", error);
					return false;
				}

				 //create user transaction record                
				var transactionSaved =  SaveSaleTransaction(lineId,null,null,BillingEnums.eTransactionTypes.FreeCourse,0,DateTime.Now,null,null,null,"free course purchase",null,out error);

				if (transactionSaved)
				{
					if(!AttachCourseOrBundle2User(buyerId, lineId,courseId, null,null, out error)) return false;

					SendPurchaseNotifications(lineId);

					return true;
				}

				SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create sale transaction failed", "free course purchase", error);
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::Complete free course request", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}
		public bool CompletePaymentRequest(Guid requestId, out int orderNo, out string error)
		{
			orderNo = -1;
			try
			{
				var request = PaypalPaymentRequestsRepository.GetById(requestId);
				string itemName;
				decimal amount;
				
				if (!ValidatePaypalRequestEntity(request, out itemName,out amount, out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::paypal request validation failed", "immediate payment", error);
					return false;
				}

				var type = Utils.ParseEnum<BillingEnums.ePaypalRequestTypes>(request.RequestTypeId);
				
				//get coupon and webstore if exists
				var sellerUserId = GetSellerUserId(request.CourseId, request.BundleId, out error);
				
				var couponInstanceId = RequestEntity2CouponInstanceId(sellerUserId,
																	  request.CourseId,
																	  request.BundleId, 
																	  request.CouponCode);

				var storeId          = RequestEntity2WebStoreId(request.TrackingID);
				var orderId          = Guid.NewGuid();

				if (!CreateSalesOrder(  orderId,
										request.PriceLineId,
										BillingEnums.ePaymentTerms.IMMEDIATE, 
										request.UserId,
										itemName,
										request.AddressId, 
										request.Amount, 
										request.CourseId, 
										request.BundleId,
										Utils.ParseEnum<BillingEnums.ePaymentMethods>(request.PaymentMethodId), 
										request.InstrumentId, 
										storeId, 
										couponInstanceId, 
										string.Empty,
										out orderNo,
										out error)) return false;

				var lineId = GetOrderLineId(orderId, BillingEnums.eOrderLineTypes.SALE);

				if (lineId < 0)
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::order line not found", "immediate payment", error);
					return false;
				}

				int paymentId;


				if (!CreateOrderLinePayment(lineId, amount, DateTime.Now, DateTime.Now, 1,BillingEnums.ePaymentStatuses.COMPLETED,BillingEnums.ePaymentTypes.ONE_TIME , out paymentId, out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create order line payment failed", "immediate payment", error);
					return false;
				}
				//create user transaction record
				var trxType = type.PaypalRequestType2TransactionType();
				//var fee = GetRestTransactionFee(request.TransactionId);                
				var transactionSaved =  SaveSaleTransaction(lineId,paymentId,null,trxType,amount,DateTime.Now,request.TransactionId,null,requestId,Utils.GetEnumDescription(trxType) + ":: immediate payment",null,out error);

				if (transactionSaved)
				{
					if (request.CourseId != null || request.BundleId != null)
					{
						SendPurchaseNotifications(lineId);
					}

					var validUntil = ToItemAccessEndDate(request.PriceLineId);
					
					return AttachCourseOrBundle2User(request.UserId, lineId, request.CourseId, request.BundleId,validUntil, out error);
				}

				SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create sale transaction failed", "immediate payment", error);
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::Complete payment request", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}
		public bool CompleteSubscriptionRequest(RecurringPaymentExecutionResultToken result_token, out int orderNo, out string error)
		{
			orderNo = -1;
			try
			{
				var request = PaypalPaymentRequestsRepository.GetById(result_token.RequestId);

				var requestNameToken = GetRequestPurchaseItemNameTokenFropPaypalRequest(request, out error);

				if (requestNameToken == null) return false;

				var itemName = CombineOrderItemName(requestNameToken);
				
				
				if (String.IsNullOrEmpty(itemName))
				{
					error = "item name not found";
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX, "subscription payment", error);
					return false;
				}

				var sellerUserId     = GetSellerUserId(request.CourseId, request.BundleId, out error);
				var couponInstanceId = RequestEntity2CouponInstanceId(sellerUserId,request.CourseId,request.BundleId,request.CouponCode);
				var storeId          = RequestEntity2WebStoreId(request.TrackingID);
				var orderId          = Guid.NewGuid();

				if (!CreateSalesOrder(orderId,
									   request.PriceLineId, 
									   BillingEnums.ePaymentTerms.EVERY_30,
									   request.UserId,
									   itemName,
									   request.AddressId,
									   result_token.BillingAmount,
									   request.CourseId,
									   request.BundleId,
									   Utils.ParseEnum<BillingEnums.ePaymentMethods>(request.PaymentMethodId),
									   request.InstrumentId,
									   storeId,
									   couponInstanceId,
									   request.RecurringRequestToken,
									   out orderNo,
									   out error)) return false;


				var lineId = GetOrderLineId(orderId, BillingEnums.eOrderLineTypes.SUBSCRIPTION);

				if (lineId < 0)
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::order line not found", "subscription payment", error);
					return false;
				}

				int initialPaymentId;

				if (!CreateOrderLinePayment(lineId, result_token.InitialAmount, DateTime.Now, DateTime.Now, 1, BillingEnums.ePaymentStatuses.SCHEDULED,BillingEnums.ePaymentTypes.INTIAL_SUBSCRIPTION, out initialPaymentId, out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create order line initial payment failed", "subscription payment", error);
					return false;
				}

				int paymentId;
				if (!CreateOrderLinePayment(lineId, result_token.BillingAmount, null, result_token.FirstBillingDate, 2, BillingEnums.ePaymentStatuses.SCHEDULED,BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION, out paymentId, out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create order line payment failed", "subscription payment", error);
					return false;
				}

				var courseSaved = AttachCourseOrBundle2User(request.UserId, lineId, request.CourseId, request.BundleId,null,out error);

				if (courseSaved)
				{
					if (request.CourseId != null || request.BundleId != null)
					{
						SendPurchaseNotifications(lineId);
					}
				}

				return courseSaved;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::Complete subscription request", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}
		public bool CreateSubscriptionWithSavedCard(SubscriptionWithSavedCardDTO token, Guid initialPaymentRequestId, out int orderNo, out string error)
		{
			orderNo = -1;
			try
			{
				var request = PaypalPaymentRequestsRepository.GetById(initialPaymentRequestId);

				if (request == null)
				{
					error = "request entity not found. please, contact support team";
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::initial payment request", "subscription payment", error);
					return false;
				}

				var requestNameToken = GetRequestPurchaseItemNameTokenFropPaypalRequest(request, out error);

				if (requestNameToken == null) return false;

				var itemName = CombineOrderItemName(requestNameToken);

				if (String.IsNullOrEmpty(itemName))
				{
					error = "item name not found";
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX, "subscription payment", error);
					return false;
				}
				
				var sellerUserId     = GetSellerUserId(token.courseId,token.bundleId, out error);
				var couponInstanceId = RequestEntity2CouponInstanceId(sellerUserId,token.courseId,token.bundleId, token.couponCode);
				var storeId          = RequestEntity2WebStoreId(token.trackingId);
				var orderId          = Guid.NewGuid();

				if (!CreateSalesOrder(orderId,
									   token.priceLineId, 
									   BillingEnums.ePaymentTerms.EVERY_30,
									   token.UserId,
									   itemName,
									   token.addressId,
									   token.BillingAmount,
									   token.courseId,
									   token.bundleId,
									   BillingEnums.ePaymentMethods.Saved_Instrument,
									   token.PaymentInstrumentId,
									   storeId,
									   couponInstanceId,
									   string.Empty,
									   out orderNo,
									   out error)) return false;


				var lineId = GetOrderLineId(orderId, BillingEnums.eOrderLineTypes.SUBSCRIPTION);

				if (lineId < 0)
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::order line not found", "subscription payment", error);
					return false;
				}

				int initialPaymentId;

				if (!CreateOrderLinePayment(lineId, token.InitialAmount, DateTime.Now, DateTime.Now, 1, BillingEnums.ePaymentStatuses.SCHEDULED,BillingEnums.ePaymentTypes.INTIAL_SUBSCRIPTION, out initialPaymentId, out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create order line initial payment failed", "subscription payment", error);
					return false;
				}

				int paymentId;
				if (!CreateOrderLinePayment(lineId, token.BillingAmount,null, token.FirstBillingDate, 2, BillingEnums.ePaymentStatuses.SCHEDULED,BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION, out paymentId, out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create order line payment failed", "subscription payment", error);
					return false;
				}

				var transactionSaved = SaveSaleTransaction(lineId, 
														   initialPaymentId,
														   null, 
														   BillingEnums.eTransactionTypes.InitialSubscriptionPayment, 
														   token.InitialAmount, 
														   DateTime.Now, 
														   request.TransactionId, 
														   null, 
														   initialPaymentRequestId, 
														   Utils.GetEnumDescription(BillingEnums.eTransactionTypes.InitialSubscriptionPayment) + ":: subscription payment",
														   null,
														   out error);
			   
				if (!transactionSaved)
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::create initial payment sale transaction failed", "subscription payment", error);
					return false;
				}
				
				var completed = AttachCourseOrBundle2User(request.UserId, lineId, request.CourseId, request.BundleId,null, out error) && UpdateSubscriptionPayment(initialPaymentId, DateTime.Now, BillingEnums.ePaymentStatuses.COMPLETED, out error);

				if (completed)
				{
					if (request.CourseId != null || request.BundleId != null)
					{
						SendPurchaseNotifications(lineId);
					}
					return true;
				}

				SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::complete subscription error", "subscription payment", error);
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::Complete subscription request", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}

		public string GetItemPageUrlFromPaypalRequest(Guid requestId)
		{
			try
			{
				var request = PaypalPaymentRequestsRepository.GetById(requestId);
				if (request == null)
				{
					Logger.Warn("Get item url from paypal request::paypal request entity not found::" + requestId);
					return string.Empty;
				}

				if (request.CourseId == null && request.BundleId == null) return string.Empty;

				if (request.CourseId != null)
				{
					var entity = CourseRepository.GetById((int) request.CourseId);

					if (entity == null) return string.Empty;

					var user = UserRepository.GetById(entity.AuthorUserId);

					return request.GenerateCourseFullPageUrl(String.Format("{0} {1}",user.FirstName,user.LastName),entity.CourseUrlName,request.TrackingID);
				}

				if (request.BundleId != null)
				{
					var entity = BundleRepository.GetById((int)request.BundleId);

					if (entity == null) return string.Empty;

					var user = UserRepository.GetById(entity.AuthorId);

					return request.GenerateBundleFullPageUrl(String.Format("{0} {1}", user.FirstName, user.LastName), entity.BundleUrlName, request.TrackingID);
				}

				return string.Empty;
			}
			catch (Exception ex)
			{
				Logger.Error("Get item url from paypal request",requestId,ex,CommonEnums.LoggerObjectTypes.Billing);
				return string.Empty;
			}
		}

		public PriceLineDTO GetPriceLineToken(int lineId, out string error)
		{
			error = string.Empty;
			try
			{
				return GetItemPriceToken(lineId);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("Get price line token",lineId,ex,CommonEnums.LoggerObjectTypes.Billing);
				return null;
			}
		}
		#endregion

		#region IUserSubscriptionsManageServices implementation
		//public List<UserSubscriptionSummaryToken> GetUserSubscriptions(ReportEnums.ePeriodSelectionKinds periodKind, Guid? subscriptionId = null, int? authorUserId = null, int? subscriberUserId = null, int? courseId = null, BillingEnums.ePaymentMethods? paymentMethod = null, BillingEnums.eSubscriptionStatuses? status = null)
		//{
		//    try
		//    {
		//        var dates = PeriodSelection2DateRange(periodKind);
		//        return BillingUserSubscriptionsRepository.SearchUserSubscriptions(dates.from, dates.to,subscriptionId, authorUserId, subscriberUserId, courseId, (byte?)paymentMethod,(byte?)status).Select(x => x.Entity2UserSubscriptionDto()).OrderByDescending(x => x.AddOn).ToList();
		//    }
		//    catch (Exception ex)
		//    {
		//        Logger.Error("get user subscriptions",ex,CommonEnums.LoggerObjectTypes.Billing);
		//        return new List<UserSubscriptionSummaryToken>();
		//    }
		//}

		//public List<SubscriptionPaymentSummaryToken> GetSubscriptionPayments(Guid? subscriptionId)
		//{
		//    try
		//    {

		//        return subscriptionId==null ? new List<SubscriptionPaymentSummaryToken>() : BillingUserSubscriptionpPaymentRepository.GetSubscriptionPayments((Guid) subscriptionId).Select(x=>x.Entity2SubscriptionPaymentSummaryToken()).OrderBy(x=>x.Number).ToList();
		//    }
		//    catch (Exception ex)
		//    {
		//        Logger.Error("get user subscription payments", subscriptionId, ex, CommonEnums.LoggerObjectTypes.Billing);
		//        return new List<SubscriptionPaymentSummaryToken>();
		//    }
		//}

		public List<ScheduledPaymentSummaryToken> SearchPayments(   ReportEnums.ePeriodSelectionKinds periodKind,
																	Guid? orderId,
																	int? orderNum,
																	int? sellerUserId,
																	int? buyerUserId,
																	int? courseId,
																	int? bundleId,
																	BillingEnums.ePaymentStatuses? paymentStatus,
																	BillingEnums.eOrderStatuses? orderStatus, 
																	bool? onlySavedCards)
		{
			try
			{
				var dates = PeriodSelection2DateRange(periodKind);
				var onlyCC = onlySavedCards != null && (bool) onlySavedCards;

				return OrderLinePaymentsViewRepository.GetMany(x => (x.ScheduledDate >= dates.from && x.ScheduledDate <= dates.to)
																	&& (orderId == null || x.OrderId == orderId)
																	&& (orderNum == null || x.OrderNumber == orderNum)
																	&& (sellerUserId == null || x.SellerUserId == sellerUserId)
																	&& (buyerUserId == null || x.BuyerUserId == buyerUserId)
																	&& (courseId == null || x.CourseId == courseId)
																	&& (bundleId == null || x.BundleId == bundleId)
																	&& (paymentStatus == null || x.StatusId == (byte)paymentStatus)
																	&& (orderStatus == null || x.OrderStatusId == (byte)orderStatus)
																	&& (
																		 (onlyCC && x.PaymentMethodId == (byte)BillingEnums.ePaymentMethods.Saved_Instrument)
																		 || (!onlyCC)
																	   )
																   )
														.Select(x => x.PaymentEntity2ScheduledPaymentSummaryToken())
														.OrderByDescending(x => x.ScheduledDate)
														.ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get scheduled payments", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<ScheduledPaymentSummaryToken>();
			}
		} 

		#endregion      

		#region IBillingManageServices implementation

		public TransactionFiltersLOV GetTransactionFiltersLov()
		{
			try
			{

				var token = new TransactionFiltersLOV();



				token.AuthorsLOV = (from trx in TransactionsViewRepository.GetAll()
										group trx by new{ trx.SellerUserId,trx.SellerFirstName, trx.SellerLastName,trx.SellerNickName}
										into x select new SelectListItem
										{
											Value = x.Key.SellerUserId.ToString()
											,Text = DtoExtensions.CombineFullName(x.Key.SellerFirstName, x.Key.SellerLastName,x.Key.SellerNickName)
										}).ToList();
					
				token.BuyersLOV  =  (from trx in TransactionsViewRepository.GetAll()
										group trx by new { trx.BuyerUserId, trx.BuyerFirstName, trx.BuyerLastName, trx.BuyerNickName }
										into x select new SelectListItem
										{
											Value = x.Key.BuyerUserId.ToString()
											,Text = DtoExtensions.CombineFullName(x.Key.BuyerFirstName, x.Key.BuyerLastName,x.Key.BuyerNickName)
										}).ToList();
					
				token.CoursesLOV  =  (from trx in TransactionsViewRepository.GetAll()
										where trx.CourseId != null
										group trx by new { trx.CourseId, trx.CourseName }
										into x select new SelectListItem
										{
											Value = x.Key.CourseId.ToString()
											,Text = x.Key.CourseName
										}).ToList();

				token.BundlesLOV   = (from trx in TransactionsViewRepository.GetAll()
									where trx.BundleId != null
									group trx by new { trx.BundleId, trx.BundleName}
										into x
										select new SelectListItem
										{
											Value = x.Key.BundleId.ToString()
											,Text = x.Key.BundleName
										}).ToList();
				
				return token;
			}
			catch (Exception ex)
			{
				Logger.Error("get transaction filter LOV's", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new TransactionFiltersLOV();
			}
		}
		public List<TransactionSummaryToken> SearchTransactions(ReportEnums.ePeriodSelectionKinds periodKind, int? authorId = null, int? buyerId = null, int? courseId = null, int? bundleId = null, BillingEnums.eTransactionTypes? trxType = null)
		{
			try
			{
				var dates = PeriodSelection2DateRange(periodKind);
				return TransactionRepository.SearchTransactions(dates.from, dates.to, authorId, buyerId, courseId,bundleId, (int?)trxType).Select(x => x.Entity2TransactionSummaryToken()).OrderByDescending(x => x.TrxDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("search transactions",  ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<TransactionSummaryToken>();
			}
		}
		public List<OrderDTO> SearchOrders(ReportEnums.ePeriodSelectionKinds periodKind, int? buyerId = null, int? sellerId = null, int? courseId = null, int? bundleId = null, int? storeId = null, bool? isSubscription = null, BillingEnums.eOrderStatuses? status = null)
		{
			try
			{
				var dates = PeriodSelection2DateRange(periodKind);

				return OrderRepository.SearchOrders(dates.from, dates.to, buyerId, sellerId, courseId, bundleId, storeId, isSubscription, status).Select(x => x.Entity2OrderDto(GetCurrencyTokenByIso(x.ISO))).OrderByDescending(x => x.OrderDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("search orders", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<OrderDTO>();
			}
		}
		public List<OrderLineDTO> GetOrderLines(Guid orderId)
		{
			try
			{
				return OrderLinesViewRepository.GetMany(x => x.OrderId==orderId).Select(x => x.Entity2OrderLineDto()).OrderByDescending(x => x.OrderDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get order lines", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<OrderLineDTO>();
			}
		}
		public List<LinePaymentDTO> GetOrderLinePayments(int lineId)
		{
			try
			{
				return OrderLinePaymentsViewRepository.GetMany(x => x.OrderLineId== lineId).Select(x => x.Entity2LinePaymentDto()).OrderBy(x => x.Number).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get order line payments", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<LinePaymentDTO>();
			}
		}

		public List<PaymentViewDTO> GetCompletedPayments(ReportEnums.ePeriodSelectionKinds periodKind, BillingEnums.ePaymentTypes? type = null,int? orderNum = null)
		{
			try
			{
				var dates = PeriodSelection2DateRange(periodKind);

				return OrderLinePaymentsViewRepository.GetMany(x => x.StatusId == (byte)BillingEnums.ePaymentStatuses.COMPLETED
																	&& (x.PaymentDate>=dates.from && x.PaymentDate<=dates.to)
																	&& (type==null || x.PaymentTypeId == (byte?)type)
																	&& (orderNum==null || x.OrderNumber==orderNum)
															  ).Select(x => x.Entity2PaymentViewDto()).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get completed payments", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<PaymentViewDTO>();
			}
		}

		public List<PaymentViewDTO> GetSellerPayments(int sellerId, int year, int month, short currencyId)
		{
			try
			{
				return OrderLinePaymentsViewRepository.GetMany(x => x.SellerUserId == sellerId && (x.WebStoreId == null || x.StoreOwnerUserId == x.SellerUserId) && x.CurrencyId==currencyId && ((DateTime)x.PaymentDate).Year == year && ((DateTime)x.PaymentDate).Month == month).Select(x => x.Entity2PaymentViewDto()).OrderBy(x => x.Number).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get user payments", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<PaymentViewDTO>();
			}
		}

		public List<PaymentViewDTO> GetAffiliatePayments(int sellerId, int year, int month, short currencyId)
		{
			try
			{
				var list  = OrderLinePaymentsViewRepository.GetMany(x =>  (
																			(x.WebStoreId != null) && 
																			(x.StoreOwnerUserId != x.SellerUserId) && 
																			(x.StoreOwnerUserId == sellerId || x.SellerUserId == sellerId)
																			) && x.CurrencyId == currencyId && ((DateTime)x.PaymentDate).Year == year && ((DateTime)x.PaymentDate).Month == month).ToList();

				return list.Select(x => x.Entity2PaymentViewDto()).OrderBy(x => x.Number).ToList();

			}
			catch (Exception ex)
			{
				Logger.Error("get user payments", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<PaymentViewDTO>();
			}
		}

		public List<PaymentViewDTO> GetRefundProgramPayments(int sellerId, int year, int month, short currencyId,bool released)
		{
			try
			{
				if(!released)
				{
				   return OrderLinePaymentsViewRepository.GetMany(x =>  x.IsUnderRGP
																		  && ( (x.SellerUserId == sellerId && (x.WebStoreId == null || x.StoreOwnerUserId == x.SellerUserId)) || (x.WebStoreId != null && x.StoreOwnerUserId != x.SellerUserId && (x.StoreOwnerUserId == sellerId) || x.SellerUserId==sellerId) )
																		  && x.CurrencyId == currencyId && ((DateTime)x.PaymentDate).Year == year && ((DateTime)x.PaymentDate).Month == month).Select(x => x.Entity2PaymentViewDto()).OrderBy(x => x.Number).ToList();
				}

				var currentMonth = new DateTime(year, month, 1);
				var previousMonth = currentMonth.AddMonths(-1);

				return  OrderLinePaymentsViewRepository.GetMany(x => x.IsUnderRGP
																	 && x.TotalRefunded == 0
																	 && ((x.SellerUserId == sellerId && (x.WebStoreId == null || x.StoreOwnerUserId == x.SellerUserId)) || (x.WebStoreId != null && x.StoreOwnerUserId != x.SellerUserId && (x.StoreOwnerUserId == sellerId) || x.SellerUserId == sellerId))
																	 && x.CurrencyId == currencyId && ((DateTime)x.PaymentDate).Year == previousMonth.Year && ((DateTime)x.PaymentDate).Month == previousMonth.Month).Select(x => x.Entity2PaymentViewDto()).OrderBy(x => x.Number).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get user payments", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<PaymentViewDTO>();
			}
		}

		public List<RefundViewDTO> GetSellerRefunds(int sellerId, int year, int month, short currencyId)
		{
			try
			{
				return OrderLinePaymentRefundsViewRepository.GetMany(x => x.SellerUserId == sellerId && (x.CurrencyId == currencyId) && (x.RefundDate).Year == year && (x.RefundDate).Month == month).Select(x => x.Entity2RefundViewDto()).OrderBy(x => x.RefundDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get seller view refunds", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<RefundViewDTO>();
			}
		}

		public List<PaymentRefundDTO> GetOrderLinePaymentRefunds(int paymentId)
		{
			try
			{
				return OrderLinePaymentRefundsRepository.GetMany(x => x.PaymentId == paymentId).Select(x => x.Entity2PaymentRefundDto()).OrderBy(x => x.RefundDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get order line payment refunds", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<PaymentRefundDTO>();
			}
		}
		public RefundOrderLinePaymentDTO GetPaymentRefundDTO(int? paymentId)
		{
			try
			{
				return paymentId != null ? OrderLinePaymentsViewRepository.Get(x => x.PaymentId == paymentId).PaymentEntity2RefundOrderLinePaymentDto() : new RefundOrderLinePaymentDTO
																																														{
																																															IsValid = false
																																															,Message = "paymentId required"
																																														};
			}
			catch (Exception ex)
			{
				Logger.Error("get order line payment refund token", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new RefundOrderLinePaymentDTO
				{
					IsValid = false
					,Message = Utils.FormatError(ex)
				};
			}
		}
		public List<TransactionSummaryToken> GetOrderLineTransactions(int lineId)
		{
			try
			{
				return TransactionsViewRepository.GetMany(x => x.LineId == lineId).Select(x => x.Entity2TransactionSummaryToken()).OrderByDescending(x => x.TrxDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get order line transactions", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<TransactionSummaryToken>();
			}
		}

		public bool ProcessRefundGRP(GRPRequestToken token, out string error)
		{
			var refundEntity = new SALE_RefundRequests();
			try
			{
				var lineEntity = OrderLinesViewRepository.Get(x => x.LineId == token.OrderLineId);
				if (lineEntity == null)
				{
					error = "Order line not found. Please contact support team";
					return false;
				}
				if (!lineEntity.IsUnderRGP || DateTime.Now.Subtract(lineEntity.OrderDate).Days > 30)
				{
					error = "Your 30 days refund guaranteed period is over. Please contact support team";
					return false;
				}
				if (lineEntity.TotalRefunded > 0)
				{
					error = "You've been partly refunded. Please contact support team";
					return false;
				}

				var payment = GetOrderLinePayments(token.OrderLineId).FirstOrDefault(x => x.Type == BillingEnums.ePaymentTypes.ONE_TIME);
				if (payment == null)
				{
					error = "Payment not found. Please contact support team";
					return false;
				}

				if (!RefundRequestEntitySave(refundEntity, lineEntity.BuyerUserId, payment.PaymentId, out error))
					return false;

				var refundToken          = GetPaymentRefundDTO(payment.PaymentId);
				refundToken.DeniedAccess = true;
				refundToken.Amount       = payment.Amount;

				int refundId;
				var result = _paypalManageServices.RefundPaymentTransaction(refundToken, out refundId, out error) && BlockUserCourseAccessAndCancelOrderOnRefund(refundToken, out error);
				if (!result)
				{
					RefundRequestEntityError(refundEntity, error);
					return false;
				}
				RefundRequestEntitySuccess(refundEntity, refundId);

				var email = new EmailGRPSubmitted
				{
					UserID       = lineEntity.SellerUserId,
					ToEmail      = lineEntity.SellerEmail,
					FullName     = lineEntity.Entity2SellerFullName(),
					LearnerName  = lineEntity.Entity2BuyerFullName(),
					LearnerEmail = lineEntity.BuyerEmail,
					ItemName     = lineEntity.ItemName,
					ReasonText   = token.ReasonText
				};

				long emailId;
				_emailServices.SaveGRPRefundSubmitted(email, out emailId);

				if (emailId > 0)
					_amazonEmailWrapper.SendEmail(emailId, out error);

				return true;
			}
			catch(Exception e)
			{
				Logger.Error("process refund GRP", e, CommonEnums.LoggerObjectTypes.Billing);
				error = FormatError(e);
				RefundRequestEntityError(refundEntity, error);
				return false;
			}
		}

		public bool BlockUserCourseAccessAndCancelOrderOnRefund(RefundOrderLinePaymentDTO token, out string error)
		{
			error = string.Empty;
			return !token.DeniedAccess || (CancelOrder(token.LineId,BillingEnums.eOrderStatuses.CANCELED, out error) && BlockUserCourseAccess(token.LineId,false, out error));
		}

		public bool BlockUserCourseAccessAndCancelOrderOnSubscriptionCancel(int lineId, out string error)
		{
			var paymentDone = _IsMonthlySubscriptionPaymentDone(lineId, out error);

			if (!String.IsNullOrEmpty(error)) return false;

			var status = paymentDone ? BillingEnums.eOrderStatuses.SUSPENDED : BillingEnums.eOrderStatuses.CANCELED;

			var canceled = CancelOrder(lineId,status, out error) && BlockUserCourseAccess(lineId,true, out error);

			if (!canceled) return false;

			SendSubscriptionCancelNotification(lineId);

			return true;
		}

		private void SendSubscriptionCancelNotification(int lineId)
		{
			try
			{
				var line = OrderLinesViewRepository.Get(x => x.LineId == lineId);

				if( line == null) return;

				var emailToken = new EmailSubscriptionCancelToken
				{
					UserID        = line.SellerUserId
					,ToEmail      = line.SellerEmail
					,FullName     = line.Entity2SellerFullName()
					,AuthorName   = line.Entity2SellerFullName()
					,LearnerName  = line.Entity2BuyerFullName()
					,LearnerEmail = line.BuyerEmail
					,CourseName   = line.ItemName
					,Month        = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.AddMonths(1).Month)
				};

				long emailId;
				_emailServices.SaveSubscriptionCancelMessage(emailToken, out emailId);

				if (emailId < 0) return;

				string error;
				_amazonEmailWrapper.SendEmail(emailId, out error);
			}
			catch (Exception ex)
			{
				Logger.Error("SendSubscriptionCancelNotification",lineId,ex,CommonEnums.LoggerObjectTypes.Billing);
			}
		}
		public bool BlockUserCourseAccess(int lineId, bool isSubscription, out string error)
		{
			return SHARED_BlockUserCourseAccess(lineId,isSubscription, out error);
		}

		private bool CancelOrder(int lineId,BillingEnums.eOrderStatuses status, out string error)
		{
			return SHARED_CancelOrder(lineId, out error,status);
		}

		public MonthlyStatementDTO GetAuthorMonthlyStatement(AuthorStatementRequestToken request)
		{
			try
			{
				var userEntity = UserRepository.GetById(request.userId);
				
				if(userEntity==null) return new MonthlyStatementDTO
				{
					IsValid = false
					,Message = "user not found"
				};

				var token = new MonthlyStatementDTO
				{
					User               = userEntity.Entity2BaseUserInfoDto()
					,PaymentMethodName = userEntity.PayoutTypeId == null ? "NOT DEFINED" : Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.ePayoutTypes>(userEntity.PayoutTypeId.ToString()))
					,PayoutTypeId      = userEntity.PayoutTypeId
					,Currency          = ActiveCurrencies.FirstOrDefault(x=>x.CurrencyId == request.currencyId)
					,Year              = request.year
					,Month             = request.month
				};
				
				var stats = new MonthlyStatementStatsDTO();

				var sales = UserRepository.GetUserMonthlyStatementSales(request.userId,request.year,request.month,request.currencyId).ToList();

				foreach (var sale in sales)
				{
					var source = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(sale.PaymentSource);

					switch (source)
					{
						case BillingEnums.eOrderLineTypes.SALE:
							stats.TotalOneTimeSales = sale.total.FormatMoney(2);
							break;
						case BillingEnums.eOrderLineTypes.SUBSCRIPTION:
							stats.TotalSubscriptionPayments = sale.total.FormatMoney(2);
							break;
						case BillingEnums.eOrderLineTypes.RENTAL:
							stats.TotalRentalSales = sale.total.FormatMoney(2);
							break;
					}
					//stats.TotalSales += sale.total.FormatMoney(2);
					//stats.TotalPaypalFees += sale.fee.FormatMoney(2);
				}

				//var refund = UserRepository.GetUserMonthlyStatementRefunds(request.userId, request.year, request.month, request.currencyId);

				//if (refund != null)
				//{
				//    stats.TotalRefundFees = refund.fee.FormatMoney(2);
				//    stats.TotalRefunds = refund.total.FormatMoney(2);
				//}


				var payoutToken = _payoutServices.GetMonthlyPayoutReport(request.year,request.month,request.userId,request.currencyId);

				if (!payoutToken.IsValid) return new MonthlyStatementDTO
											{
												IsValid = false
												,Message = payoutToken.Message
											};

				var t = payoutToken.CurrencyRows.FirstOrDefault(x => x.Currency.CurrencyId == request.currencyId);

				if(t == null) return new MonthlyStatementDTO
											{
												IsValid = false
												,Message = "Currency rows not found"
											};

				var st = t.Rows.FirstOrDefault();

				if(st == null) return new MonthlyStatementDTO
											{
												IsValid = false
												,Message = "Currency token not found"
											};

				stats.AffiliateSales          = st.AffiliateSales;
				stats.AffiliateCommission     = st.AffiliateCommission;
				stats.AffiliateFees           = st.AffiliateFees;

				stats.TotalSales              = st.TotalSales;

				stats.Fees                    = st.Fees;
				stats.TotalRefunds            = st.Refund;
				stats.TotalRefundFees         = st.RefundFees;

				stats.RefundProgrammToHold    = st.RefundProgrammToHold;
				stats.RefundProgrammToRelease = st.RefundProgrammToRelease;
				
				stats.Balance                 = st.Balance;
				stats.TotalLfeCommission      = st.LfeCommission;
				stats.TotalPayout             = st.Payout;
				
				//stats.Balance = stats.TotalSales.SalesToBalance(stats.TotalPaypalFees,stats.TotalRefunds,stats.TotalRefundFees);

				//stats.TotalLfeCommission = stats.Balance.BalanceToLfeCommission(); //stats.TotalSales.ToLfeCommission(stats.TotalPaypalFees ,stats.TotalRefunds);

				//stats.TotalPayout = stats.Balance.BalanceToPayout(stats.TotalLfeCommission);//stats.TotalSales.ToPayout(stats.TotalPaypalFees,stats.TotalLfeCommission,stats.TotalRefunds);

				token.Stats = stats;

				token.IsValid = true;

			

				return token;
			}
			catch (Exception ex)
			{
				Logger.Error(String.Format("Get monthly stats for user {0} on {1}-{2}", request.userId, request.year, request.month), ex, CommonEnums.LoggerObjectTypes.Billing);
				return new MonthlyStatementDTO
				{
					IsValid = false
					,Message = Utils.FormatError(ex)
				};
			}
		}

		public MonthlyStatmentPrintToken GetAuthorMonthlyStatementsPrintToken(AuthorStatementRequestToken request)
		{
			
			try
			{
				var from = new DateTime(request.year, request.month, 1);
				var to = from.AddMonths(1).AddSeconds(-1);

				var currencies = _GetUserCurrencies(request.userId, from, to);

				if(!currencies.Any()) return new MonthlyStatmentPrintToken{IsValid = false,Message = "No payments found for requested period"};

				var statments = new List<MonthlyStatementDTO>();

				foreach (var currency in currencies)
				{
					request.currencyId = currency.CurrencyId;

					var statment = GetAuthorMonthlyStatement(request);
					
					if(statment.IsValid) statments.Add(statment);
				}

				return new MonthlyStatmentPrintToken
				{
					IsValid    = statments.Any()
					,Message   = statments.Any() ? "" : "No payments found for requested period"
					,Statments = statments
				};
			}
			catch (Exception ex)
			{
				Logger.Error(String.Format("Get monthly statements for user {0} on {1}-{2}", request.userId, request.year, request.month), ex, CommonEnums.LoggerObjectTypes.Billing);
				return new MonthlyStatmentPrintToken
				{
					IsValid = false
					,Message = FormatError(ex)
					,Statments = new List<MonthlyStatementDTO>()
				};
			}
		}

		public bool SendAuthorMonthlyStatement(MonthlyStatementDTO token, string messageBody,out string error)
		{
			long emailId;
			_emailServices.SaveMonthlyStatementMessage(token,messageBody, out emailId, out error);

			//4. send email
			return emailId >= 0 && _amazonEmailWrapper.SendEmail(emailId, out error);

		}

		#region custom trx

		public CustomTrxDTO GetCustomTrxToken(int? paymentId)
		{
			try
			{
				if (paymentId == null) return new CustomTrxDTO { IsValid = false, Message = "PaymentId required" };

				var paymentEntity = OrderLinePaymentsViewRepository.Get(x => x.PaymentId == paymentId);

				return paymentEntity != null ? paymentEntity.Entity2CustomTrxDto() : new CustomTrxDTO { IsValid = false, Message = "Payment entity not found" };
			}
			catch (Exception ex)
			{
				return new CustomTrxDTO { IsValid = false, Message = FormatError(ex)};
			}
		}

		public bool SaveCustomTrx(CustomTrxDTO token, out string error)
		{
			
			try
			{
				BillingEnums.eTransactionTypes trxType;

				switch (token.Type)
				{
					case BillingEnums.ePaymentTypes.INTIAL_SUBSCRIPTION:
						trxType = BillingEnums.eTransactionTypes.InitialSubscriptionPayment;
						break;
					case BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION:
						trxType = BillingEnums.eTransactionTypes.PeriodSubscriptionPayment;
						break;                    
					default:
						error = "For this Payment type custom trx not allowed";
						return false;
				}

				var transactionSaved = SaveSaleTransaction(token.LineId,
														   token.PaymentId,
														   null,
														   trxType,
														   token.Amount,
														   DateTime.Now,
														   token.ExternalTrxID,
														   token.Fee,
														   null,
														   Utils.GetEnumDescription(trxType) + ":: subscription payment",
														   null,
														   out error);

				if(!transactionSaved) return false;

				var paymentEntity = OrderLinePaymentRepository.GetById(token.PaymentId);

				if (paymentEntity == null)
				{
					error = "payment entity not found";
					return false;
				}

				paymentEntity.StatusId = (byte) BillingEnums.ePaymentStatuses.COMPLETED;
				paymentEntity.PaymentDate = DateTime.Now;
				paymentEntity.UpdateDate = DateTime.Now;

				return OrderLinePaymentRepository.UnitOfWork.CommitAndRefreshChanges(out error);

			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				return false;
			}
		}

		#endregion
		#endregion

		#region ISalesOrderServices implementation

		private bool ValidateOrderLinePrice(int? priceLineId,int authorId,BillingEnums.ePaymentTerms paymentTerm, decimal? price, int? courseId, int? bundleId, int? couponInstanceId, out OrderLinePriceDTO priceToken, out string error)
		{
			priceToken = new OrderLinePriceDTO();
			
			try
			{
				if (priceLineId == null)
				{
					error = "priceLineId required";
					return false;
				}

				if (courseId == null && bundleId == null)
				{
					error = "course or bundle required";
					return false;
				}

				if (price == null)
				{
					error = "price required";
					return false;
				}

				var priceLineToken = GetPriceLineToken((int) priceLineId, out error);

				//bool isSubscription;
				switch (paymentTerm)
				{
					case BillingEnums.ePaymentTerms.IMMEDIATE:
						
						decimal? itemPrice;

						if (courseId != null)
						{
							var entity = CourseRepository.GetById((int)courseId);
							if (entity == null)
							{
								error = "course entity not found";
								return false;
							}
							itemPrice = entity.IsFreeCourse ? 0 : priceLineToken.Price; //entity.PriceUSD;
						}
						else
						{
							itemPrice = priceLineToken.Price;
						}
						
						priceToken.Price = (decimal)itemPrice;
						//isSubscription = false;
						break;
					case BillingEnums.ePaymentTerms.EVERY_30:
						var subscriptionPrice = GetItemMonthlySubscriptionPrice(priceLineToken.ItemId,priceLineToken.ItemType,priceLineToken.Currency.CurrencyId);
						
						//courseId != null ? CourseRepository.GetById((int)courseId).MonthlySubscriptionPriceUSD : BundleRepository.GetById((int)bundleId).MonthlySubscriptionPrice;
						
						if (subscriptionPrice == null)
						{
							error = "subscription price not found";
							return false;
						}
						
						priceToken.Price = (decimal) subscriptionPrice;
						//isSubscription = true;
						break;
					default:
						error = "payment term not supported";
						return false;
				}

				if (couponInstanceId != null)
				{
					var couponResult = _couponWidgetServices.ValidateCoupon((int) priceLineId,authorId,courseId, bundleId, (int)couponInstanceId);

					if (!couponResult.IsValid)
					{
						error = couponResult.Message;
						return false;
					}

					priceToken.Discount = couponResult.Discount;
					priceToken.FinalPrice = couponResult.FinalPrice;
				}
				else
				{
					priceToken.Discount = 0;
					priceToken.FinalPrice = priceToken.Price;
				}
				
				if (priceToken.FinalPrice == (decimal) price) return true;

				error = "price not matched";
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}

		}
		private bool CreateSalesOrder(Guid orderId,
									int? priceLineId,
									BillingEnums.ePaymentTerms paymentTerm, 
									int userId,
									string itemName, 
									int? addressId, 
									decimal? price, 
									int? courseId, 
									int? bundleId, 
									//byte paymentMethodId, 
									BillingEnums.ePaymentMethods paymentMethod,
									Guid? paymentIntrsrumentId, 
									int? storeId, 
									int? couponInstanceId, 
									string paypalProfileId, 
									out int orderNo,
									out string error)
		{
			error = string.Empty;
			orderNo = -1;
			try
			{
				var sellerUserId = GetSellerUserId(courseId, bundleId, out error);

				if (sellerUserId < 0)
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::sellerId not found", "sellerId not found", error);
					return false;
				}

				if (priceLineId == null)
				{
					error = "priceLineId required";
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX, "create sales order", error);
					return false;
				}

				var pt = GetItemPriceToken((int)priceLineId);

				if (pt == null)
				{
					error = "price line token not found";
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX, "create sales order", error);
					return false;
				}

				OrderLinePriceDTO priceToken;
				
				BillingEnums.eOrderLineTypes orderLineType;

				switch (pt.PriceType)
				{
					case BillingEnums.ePricingTypes.ONE_TIME:
						orderLineType = BillingEnums.eOrderLineTypes.SALE;
						break;
					case BillingEnums.ePricingTypes.RENTAL:
						orderLineType = BillingEnums.eOrderLineTypes.RENTAL;
						break;
					case BillingEnums.ePricingTypes.FREE:
						orderLineType = BillingEnums.eOrderLineTypes.FREE;
						break;
					case BillingEnums.ePricingTypes.SUBSCRIPTION:
						orderLineType = BillingEnums.eOrderLineTypes.SUBSCRIPTION;
						break;
					default:
						orderLineType = BillingEnums.eOrderLineTypes.SALE;
						break;
				}

				if (paymentMethod != BillingEnums.ePaymentMethods.Charge_Free)
				{
					if (!ValidateOrderLinePrice(priceLineId, sellerUserId, paymentTerm, price, courseId, bundleId,couponInstanceId, out priceToken, out error)) return false;                    
				}
				else
				{
					priceToken = new OrderLinePriceDTO { Price = pt.Price, Discount = pt.Price, FinalPrice = 0 };
					
				}

				#region create  header
				//var paymentMethod = Utils.ParseEnum<BillingEnums.ePaymentMethods>(paymentMethodId);
				paymentMethod = paymentMethod == BillingEnums.ePaymentMethods.Paypal ? paymentMethod : (paymentIntrsrumentId == null ? paymentMethod : BillingEnums.ePaymentMethods.Saved_Instrument);
				var orderEntity = new SALE_Orders
				{
					OrderId          = orderId
					,BuyerUserId     = userId
					,SellerUserId    = sellerUserId
					,PaymentMethodId = (byte)paymentMethod
					,InstrumentId    = paymentIntrsrumentId
					,WebStoreId      = storeId
					,AddressId       = addressId
					,StatusId        = (byte)(paymentTerm == BillingEnums.ePaymentTerms.IMMEDIATE ? BillingEnums.eOrderStatuses.COMPLETE : BillingEnums.eOrderStatuses.ACTIVE)
					,OrderDate       = DateTime.Now
					,AddOn           = DateTime.Now
					,CreatedBy       = CurrentUserId
				};

				OrderRepository.Add(orderEntity);

				if (!OrderRepository.UnitOfWork.CommitAndRefreshChanges(out error))
				{
					SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::order creation error",string.Empty,error);
					return false;
				}

				orderNo = orderEntity.Sid;

				#endregion

				#region create line

				//var seller = UserRepository.GetById(sellerUserId);

				var affiliateCommission = GetItemAffiliateCommission(pt.ItemId, pt.ItemType, sellerUserId);// seller != null ? seller.AffiliateCommission : AFFILIATE_COMMISSION_DEFAULT;
				
				var lineSaved = CreateOrderLine(orderId,    
												priceLineId,                        
												paymentTerm,
												orderLineType,	//paymentTerm == BillingEnums.ePaymentTerms.IMMEDIATE ? BillingEnums.eOrderLineTypes.SALE : BillingEnums.eOrderLineTypes.SUBSCRIPTION,
												sellerUserId,
												itemName,
												courseId,
												bundleId,
												couponInstanceId,
												paypalProfileId,
												priceToken,
												affiliateCommission,
											  //  null,
												out error);

				if (lineSaved) return true;

				OrderRepository.Delete(x=>x.OrderId==orderId);
				OrderRepository.UnitOfWork.CommitAndRefreshChanges();
				SendAdminMail(PURCHASE_ERROR_MSG_PREFIX + "::order line creation error", string.Empty, error);
				orderNo = -1;

				return false;

				#endregion
			}
			catch (Exception ex)
			{
				Logger.Error(PURCHASE_ERROR_MSG_PREFIX + "::create sales order::", ex, CommonEnums.LoggerObjectTypes.SalesOrder);
				return false;
			}
		}

		private bool CreateOrderLine(Guid orderId,
									int? priceLineId,
									BillingEnums.ePaymentTerms paymentTerm,
									BillingEnums.eOrderLineTypes lineType,
									int sellerUserId,
									string itemName,
									int? courseId,
									int? bundleId,
									int? couponInstanceId,
									string paypalProfileId,
									OrderLinePriceDTO priceToken,
									decimal affiliateCommission,
									out string error)
		{
			//TODO get refund program flag by sellerUserId

			var lineEntity = new SALE_OrderLines
			{
				OrderId              = orderId
				,LineTypeId          = (byte)lineType
				,SellerUserId        = sellerUserId
				,ItemName            = itemName
				,PriceLineId         = priceLineId
				,CourseId            = courseId
				,BundleId            = bundleId
				,CouponInstanceId    = couponInstanceId
				,Price               = priceToken.Price
				,Discount            = priceToken.Discount
				,TotalPrice          = priceToken.FinalPrice
				,PaypalProfileID     = paypalProfileId
				,PaymentTermId       = (byte)paymentTerm
				,AffiliateCommission = affiliateCommission
				,AddOn               = DateTime.Now
				,CreatedBy           = CurrentUserId
				,IsUnderRGP          = IsUnderRGP(sellerUserId)
			};

			OrderLineRepository.Add(lineEntity);

			return OrderLineRepository.UnitOfWork.CommitAndRefreshChanges(out error);		
		}
		#endregion

	}
}
