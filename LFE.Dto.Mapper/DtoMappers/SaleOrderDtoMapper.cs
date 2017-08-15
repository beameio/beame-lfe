using System;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class SaleOrderDtoMapper
    {
        #region private helpers
        private static bool IsPaymentRefundAllowed(this vw_SALE_OrderLinePayments entity)
        {
            var lineType = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId);
            var status = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString());

            if (entity.Amount == entity.TotalRefunded || status != BillingEnums.ePaymentStatuses.COMPLETED || entity.PaymentDate==null || lineType==BillingEnums.eOrderLineTypes.SUBSCRIPTION) return false;

            return (DateTime)entity.PaymentDate >= DateTime.Now.AddMonths(-1); //(lineType==BillingEnums.eOrderLineTypes.SALE) && 
        }

        private static bool IsCustomTrxAllowed(this vw_SALE_OrderLinePayments entity)
        {
            var status = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString());
            var lineType = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.PaymentTypeId);

            if(lineType != BillingEnums.ePaymentTypes.INTIAL_SUBSCRIPTION && lineType != BillingEnums.ePaymentTypes.PERIOD_SUBSCRIPTION) return false;

            if (status == BillingEnums.ePaymentStatuses.COMPLETED) return false;

            return entity.ScheduledDate < DateTime.Now;
        }

        private static string OrderLineEntity2CouponDisplayValue(this vw_SALE_OrderLines entity)
        {
            return entity.CouponTypeId == null ? string.Empty : CombineCouponDisplayValue((byte)entity.CouponTypeId, entity.CouponTypeAmount);
        }

        private static string OrderLineEntity2CouponDisplayValue(this DB_SaleDetailsToken entity)
        {
            return entity.CouponTypeId == null ? string.Empty : CombineCouponDisplayValue((byte)entity.CouponTypeId, entity.CouponTypeAmount);
        }
        private static string OrderLineEntity2CouponDisplayValue(this DB_CouponUsageToken entity)
        {
            return entity.CouponTypeId == null ? string.Empty : CombineCouponDisplayValue((byte)entity.CouponTypeId, entity.CouponTypeAmount);
        }
        private static string CombineCouponDisplayValue(byte typeId,double? amount)
        {
            var type = Utils.ParseEnum<CourseEnums.CouponType>(typeId.ToString());

            switch (type)
            {
                case CourseEnums.CouponType.PERCENT:
                case CourseEnums.CouponType.SUBSCRIPTION:
                    return String.Format("{0}%", amount);
                case CourseEnums.CouponType.FIXED:
                    return String.Format("{0} USD", amount);
                case CourseEnums.CouponType.FREE:
                    return "Free Course";
            }

            return string.Empty;
        }

        private static string OrderLineEntity2CouponDisplayValue(this SALE_OrderLineViewToken entity)
        {
            return entity.CouponTypeId == null ? string.Empty : CombineCouponDisplayValue((byte) entity.CouponTypeId, entity.CouponTypeAmount);
        }

        #endregion 

        public static OrderDTO Entity2OrderDto(this vw_SALE_Orders entity, BaseCurrencyDTO currency)
        {
            var status = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.StatusId);

            return new OrderDTO
            {
                 OrderNumber            = entity.Sid
                ,OrderId                = entity.OrderId
                ,OrderDate              = entity.OrderDate
                ,TotalAmount            = entity.TotalAmount
                ,Currency               = currency
                ,PaymentMethod          = Utils.ParseEnum<BillingEnums.ePaymentMethods>(entity.PaymentMethodId)
                ,PaymentMethodName      = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.ePaymentMethods>(entity.PaymentMethodId))
                ,PaymentInstrumentName  = entity.DisplayName
                ,OrderStatus            = status
                ,StatusName             = Utils.GetEnumDescription(status)
                ,Buyer                  = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller                 = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
                ,WebStore               = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
            };
        }
        public static OrderLineDTO Entity2OrderLineDto(this vw_SALE_OrderLines entity)
        {
            var pm = Utils.ParseEnum<BillingEnums.ePaymentTerms>(entity.PaymentTermId);
            return new OrderLineDTO
            {
                LineId              = entity.LineId
                ,OrderNumber        = entity.OrderNumber
                ,OrderId            = entity.OrderId
                ,OrderDate          = entity.OrderDate
                ,ItemName           = entity.ItemName
                ,Price              = entity.Price
                ,Discount           = entity.Discount
                ,TotalPrice         = entity.TotalPrice
                ,PaymentMethod      = Utils.ParseEnum<BillingEnums.ePaymentMethods>(entity.PaymentMethodId)
                ,OrderStatus        = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId)
                ,Status             = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId))
                ,LineType           = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId)
                ,Currency           = entity.Entity2BaseCurrencyDto()
                ,PaymentTerm        = pm
                ,PaymentTermName    = Utils.GetEnumDescription(pm)
                ,PaypalProfileID    = entity.PaypalProfileID 
                ,TotalRefunded      = entity.TotalRefunded
                ,CouponValue        = entity.OrderLineEntity2CouponDisplayValue()
                ,Buyer              = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller             = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
                ,WebStore           = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
            };
        }

        public static DbSubscriptionDetailToken Entity2DbSubscriptionDetailToken(this vw_SALE_OrderLines entity)
        {
           return new DbSubscriptionDetailToken
            {
                 LineId                 = entity.LineId
                ,OrderNumber            = entity.OrderNumber
                ,OrderDate              = entity.OrderDate
                ,ItemName               = entity.ItemName
                ,Price                  = entity.Price
                ,Discount               = entity.Discount
                ,TotalPrice             = entity.TotalPrice
                ,TotalAmount            = entity.TotalAmountPayed
                ,OrderStatus            = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId)
                ,Status                 = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId))
                ,LineType               = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId)
                ,Currency               = entity.Entity2BaseCurrencyDto()
                ,PaypalProfileID        = entity.PaypalProfileID
                ,AffiliateCommisssion   = entity.AffiliateCommission
                ,CancelledOn            = entity.CancelledOn
                ,Buyer                  = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller                 = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
               ,WebStoreOwner           = entity.StoreOwnerUserId != null ? new BaseUserInfoDTO
                                                                    {
                                                                        UserId = (int) entity.StoreOwnerUserId
                                                                        ,FullName = entity.Entity2StoreOwnerFullName()
                                                                        ,Email    = entity.StoreOwnerEmail

                                                                    } : new BaseUserInfoDTO()
                ,WebStore               = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
            };
        }


        public static BaseOrderLineDTO DbSaleEntity2BaseOrderLineDto(this DB_SaleDetailsToken entity)
        {
            return new BaseOrderLineDTO
            {
                 LineId                 = entity.OrderLineId
                ,OrderNumber            = entity.OrderNumber
                ,OrderDate              = entity.OrderDate
                ,ItemName               = entity.ItemName
                ,Price                  = entity.Price
                ,Discount               = entity.Discount
                ,TotalPrice             = entity.TotalPrice
                ,TotalAmount            = entity.TotalAmountPayed ?? entity.TotalPrice
                ,OrderStatus            = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId)
                ,Status                 = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId))
                ,LineType               = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId)
                ,Currency               = entity.Entity2BaseCurrencyDto()        
                ,PaypalProfileID        = entity.PaypalProfileID 
                ,TotalRefunded          = entity.TotalRefunded
                ,AffiliateCommisssion   = entity.AffiliateCommission
                ,CouponValue            = entity.OrderLineEntity2CouponDisplayValue()
                ,Buyer                  = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller                 = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
               ,WebStoreOwner           = entity.StoreOwnerUserId != null ? new BaseUserInfoDTO
                                                                    {
                                                                        UserId = (int) entity.StoreOwnerUserId
                                                                        ,FullName = entity.Entity2StoreOwnerFullName()
                                                                        ,Email    = entity.StoreOwnerEmail

                                                                    } : new BaseUserInfoDTO()
                ,WebStore               = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
            };
        }

        public static BaseOrderLineDTO DbCouponEntity2BaseOrderLineDto(this DB_CouponUsageToken entity)
        {
            return new BaseOrderLineDTO
            {
                 LineId                 = entity.LineId
                ,OrderNumber            = entity.OrderNumber
                ,OrderDate              = entity.OrderDate
                ,ItemName               = entity.ItemName
                ,Price                  = entity.Price
                ,Discount               = entity.Discount
                ,TotalPrice             = entity.TotalPrice
                ,TotalAmount            = entity.TotalAmountPayed
                ,OrderStatus            = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId)
                ,Status                 = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId))
                ,LineType               = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId)
                ,Currency               = entity.Entity2BaseCurrencyDto()        
                ,PaypalProfileID        = entity.PaypalProfileID 
                ,TotalRefunded          = entity.TotalRefunded
                ,AffiliateCommisssion   = entity.AffiliateCommission
                ,CouponValue            = entity.OrderLineEntity2CouponDisplayValue()
                ,Buyer                  = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller                 = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
               ,WebStoreOwner           = entity.StoreOwnerUserId != null ? new BaseUserInfoDTO
                                                                    {
                                                                        UserId = (int) entity.StoreOwnerUserId
                                                                        ,FullName = entity.Entity2StoreOwnerFullName()
                                                                        ,Email    = entity.StoreOwnerEmail

                                                                    } : new BaseUserInfoDTO()
                ,WebStore               = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
            };
        }

        public static DbSubscriptionDetailToken DbCancelEntity2BaseOrderLineDto(this DB_SubscriptionCancelToken entity)
        {
            return new DbSubscriptionDetailToken
            {
                 LineId                 = entity.LineId
                ,OrderNumber            = entity.OrderNumber
                ,OrderDate              = entity.OrderDate
                ,ItemName               = entity.ItemName
                ,Price                  = entity.Price
                ,Discount               = entity.Discount
                ,TotalPrice             = entity.TotalPrice
                ,TotalAmount            = entity.TotalAmountPayed
                ,OrderStatus            = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId)
                ,Status                 = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId))
                ,LineType               = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId)
                ,Currency               = entity.Entity2BaseCurrencyDto()
                ,PaypalProfileID        = entity.PaypalProfileID
                ,AffiliateCommisssion   = entity.AffiliateCommission
                ,CancelledOn            = entity.CancelledOn
                ,Buyer                  = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller                 = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
               ,WebStoreOwner           = entity.StoreOwnerUserId != null ? new BaseUserInfoDTO
                                                                    {
                                                                        UserId = (int) entity.StoreOwnerUserId
                                                                        ,FullName = entity.Entity2StoreOwnerFullName()
                                                                        ,Email    = entity.StoreOwnerEmail

                                                                    } : new BaseUserInfoDTO()
                ,WebStore               = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
            };
        }

        public static DbRefundDetailToken DbRefundEntity2BaseOrderLineDto(this DB_RefundDetailToken entity)
        {
            return new DbRefundDetailToken
            {
                 LineId                 = entity.OrderLineId
                ,OrderNumber            = entity.OrderNumber
                ,OrderDate              = entity.OrderDate
                ,ItemName               = entity.ItemName
                ,Price                  = entity.Price
                ,Discount               = entity.Discount
                ,TotalPrice             = entity.TotalPrice
                ,OrderStatus            = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId)
                ,Status                 = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId))
                ,LineType               = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId)
                ,Currency               = entity.Entity2BaseCurrencyDto()
                ,PaypalProfileID        = entity.PaypalProfileID 
                ,RefundAmount           = entity.RefundAmount
                ,RefundDate             = entity.RefundDate
                ,AffiliateCommisssion   = entity.AffiliateCommission
                ,Buyer                  = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller                 = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
               ,WebStoreOwner           = entity.StoreOwnerUserId != null ? new BaseUserInfoDTO
                                                                    {
                                                                        UserId = (int) entity.StoreOwnerUserId
                                                                        ,FullName = entity.Entity2StoreOwnerFullName()
                                                                        ,Email    = entity.StoreOwnerEmail

                                                                    } : new BaseUserInfoDTO()
                ,WebStore               = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
            };
        }
        

        public static OrderLineDTO Entity2OrderLineDto(this SALE_OrderLineViewToken entity)
        {
            var pm = Utils.ParseEnum<BillingEnums.ePaymentTerms>(entity.PaymentTermId);
            return new OrderLineDTO
            {
                LineId              = entity.LineId
                ,OrderNumber        = entity.OrderNumber
                ,OrderId            = entity.OrderId
                ,OrderDate          = entity.OrderDate
                ,ItemName           = entity.ItemName
                ,Price              = entity.Price
                ,Discount           = entity.Discount
                ,TotalPrice         = entity.TotalPrice
                ,TotalAmount        = entity.TotalAmountPayed
                ,Currency           = entity.Entity2BaseCurrencyDto()
                ,PaymentMethod      = Utils.ParseEnum<BillingEnums.ePaymentMethods>(entity.PaymentMethodId)
                ,OrderStatus        = Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId)
                ,Status             = Utils.GetEnumDescription(Utils.ParseEnum<BillingEnums.eOrderStatuses>(entity.OrderStatusId))
                ,LineType           = Utils.ParseEnum<BillingEnums.eOrderLineTypes>(entity.LineTypeId)
                ,PaymentTerm        = pm
                ,PaymentTermName    = Utils.GetEnumDescription(pm)
                ,PaypalProfileID    = entity.PaypalProfileID 
                ,TotalRefunded      = entity.TotalRefunded
                ,CouponValue        = entity.OrderLineEntity2CouponDisplayValue()
                ,Buyer              = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                            ,Email    = entity.BuyerEmail
                                                        }
                ,Seller             = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,FullName = entity.Entity2SellerFullName()
                                                            ,Email    = entity.SellerEmail
                                                        }
                ,WebStore           = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                                    {
                                                                        StoreId = (int) entity.WebStoreId
                                                                        ,TrackingID = entity.TrackingID
                                                                        ,Name = entity.StoreName

                                                                    } : new BaseWebStoreDTO()
                ,IsUnderGRP         = entity.IsUnderRGP
                ,IsGRPRefundable    = entity.IsUnderRGP && DateTime.Now.Subtract(entity.OrderDate).Days <= 30 && entity.TotalRefunded == 0
            };
        }

        public static TransactionSummaryToken Entity2TransactionSummaryToken(this vw_SALE_Transactions entity)
        {
            var trxType = Utils.ParseEnum<BillingEnums.eTransactionTypes>(entity.TransactionTypeId.ToString());

            return new TransactionSummaryToken
            {
                 TrxId              = entity.TransactionId
                ,TrxDate            = entity.TransactionDate
                ,TrxType            = trxType
                ,TrxTypeName        = Utils.GetEnumDescription(trxType)
                ,ItemName           = entity.ItemName
                ,Amount             = entity.Amount
                ,OrderNumber        = entity.OrderNumber
                ,OrderLineId        = entity.LineId
                ,PaymentNumber      = entity.PaymentNumber
                ,ExternalTrxId      = entity.ExternalTransactionID
                ,Fee                = entity.Fee
                ,Currency           = entity.Entity2BaseCurrencyDto()
                ,Course             = entity.CourseId != null  ? new  BaseCourseInfoDTO
                                                        {
                                                            CourseId    = (int)entity.CourseId
                                                            ,CourseName = entity.CourseName
                                                        } : new BaseCourseInfoDTO()
                ,Buyer              = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,Email    = entity.BuyerEmail
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                        }
                ,Seller             = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,Email    = entity.SellerEmail
                                                            ,FullName = entity.Entity2SellerFullName()
                                                        }
            };
        }

        public static LinePaymentDTO Entity2LinePaymentDto(this vw_SALE_OrderLinePayments entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.PaymentTypeId.ToString());

            return new LinePaymentDTO
            {
                 OrderId            = entity.OrderId
                ,LineId             = entity.OrderLineId
                ,PaymentId          = entity.PaymentId
                ,Number             = entity.PaymentNumber
                ,Amount             = entity.Amount
                ,Fee                = entity.Fee
                ,Status             = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString())
                ,Currency           = entity.Entity2BaseCurrencyDto()
                ,Type               = type                
                ,TypeName           = Utils.GetEnumDescription(type)
                ,ScheduledDate      = entity.ScheduledDate
                ,CompletedDate      = entity.PaymentDate
                ,IsRefundable       = entity.IsPaymentRefundAllowed()
                ,TotalRefunded      = entity.TotalRefunded
                ,IsCustomTrxAllowed = entity.IsCustomTrxAllowed()
            };
        }
      
        public static PaymentDTO Entity2PaymentDto(this vw_SALE_OrderLinePayments entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.PaymentTypeId.ToString());

            return new PaymentDTO
            {
                 OrderId             = entity.OrderId
                 ,OrderNumber        = entity.OrderNumber
                 ,ItemName           = entity.ItemName
                ,LineId              = entity.OrderLineId
                ,PaymentId           = entity.PaymentId
                ,Number              = entity.PaymentNumber
                ,Amount              = entity.Amount
                ,Fee                 = entity.Fee
                ,Status              = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString())
                ,Currency            = entity.Entity2BaseCurrencyDto()
                ,Type                = type
                ,TypeName            = Utils.GetEnumDescription(type)
                ,ScheduledDate       = entity.ScheduledDate
                ,CompletedDate       = entity.PaymentDate
                ,IsRefundable        = entity.IsPaymentRefundAllowed()
                ,TotalRefunded       = entity.TotalRefunded
                ,AffiliateCommission = entity.AffiliateCommission
            };
        }

        public static CustomTrxDTO Entity2CustomTrxDto(this vw_SALE_OrderLinePayments entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.PaymentTypeId.ToString());

            return new CustomTrxDTO
            {
                 LineId              = entity.OrderLineId
                ,PaymentId           = entity.PaymentId
                ,Amount              = entity.Amount
                ,Status              = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString())
                ,Currency            = entity.Entity2BaseCurrencyDto()
                ,Type                = type
                ,ScheduledDate       = entity.ScheduledDate         
            };
        }

        public static PaymentViewDTO Entity2PaymentViewDto(this vw_SALE_OrderLinePayments entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.PaymentTypeId.ToString());

            var token =  new PaymentViewDTO
            {
                 OrderId             = entity.OrderId
                ,OrderNumber         = entity.OrderNumber
                ,ItemName            = entity.ItemName
                ,LineId              = entity.OrderLineId
                ,PaymentId           = entity.PaymentId
                ,Number              = entity.PaymentNumber
                ,Amount              = entity.Amount
                ,Fee                 = entity.Fee                
                ,Status              = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString())
                ,Currency            = entity.Entity2BaseCurrencyDto()
                ,Type                = type
                ,TypeName            = Utils.GetEnumDescription(type)
                ,ScheduledDate       = entity.ScheduledDate
                ,CompletedDate       = entity.PaymentDate
                ,OrderDate           = entity.OrderDate
                ,IsRefundable        = entity.IsPaymentRefundAllowed()
                ,TotalRefunded       = entity.TotalRefunded
                ,TrxId               = entity.ExternalTransactionID
                ,AffiliateCommission = entity.AffiliateCommission
                ,Buyer               = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,Email    = entity.BuyerEmail
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                        }
                ,Seller              = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.SellerUserId
                                                            ,Email    = entity.SellerEmail
                                                            ,FullName = entity.Entity2SellerFullName()
                                                        }
                ,StoreOwner = entity.StoreOwnerUserId != null ? new BaseUserInfoDTO{UserId = (int)entity.StoreOwnerUserId,FullName = entity.Entity2StoreOwnerFullName()} : new BaseUserInfoDTO{FullName = string.Empty,UserId = -1}
            };
            token.Commission = token.Amount.ToLfeCommission(token.Fee ?? 0, token.TotalRefunded);
            token.Payout     = token.Amount.ToPayout(token.Fee ?? 0, (decimal) token.Commission  , token.TotalRefunded);
            return token;
        }

        public static PaymentRefundDTO Entity2PaymentRefundDto(this SALE_OrderLinePaymentRefunds entity)
        { 
            var type = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.TypeId.ToString());
            
            return new PaymentRefundDTO
            {
                RefundId       = entity.RefundId
                ,PaymentId     = entity.PaymentId
                ,Amount        = entity.Amount
                ,RefundDate    = entity.RefundDate
                ,Type          = type
                ,TypeName      = Utils.GetEnumDescription(type)
            };
        }

        public static RefundDTO Entity2RefundDto(this vw_SALE_OrderLinePaymentRefunds entity)
        { 
            var type = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.PaymentTypeId.ToString());
            var refundType = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.RefundTypeId.ToString());
            return new RefundDTO
            {
                 RefundId            = entity.RefundId
                ,PaymentId           = entity.PaymentId
                ,RefundAmount        = entity.RefundAmount
                ,RefundDate          = entity.RefundDate
                ,RefundType          = refundType
                ,RefundTypeName      = Utils.GetEnumDescription(refundType)
                ,OrderNumber         = entity.OrderNumber
                ,ItemName            = entity.ItemName
                ,LineId              = entity.OrderLineId
                ,Number              = entity.PaymentNumber
                ,Amount              = entity.Amount
                ,Status              = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString())
                ,Currency            = entity.Entity2BaseCurrencyDto()
                ,Type                = type
                ,TypeName            = Utils.GetEnumDescription(type)
                ,ScheduledDate       = entity.ScheduledDate
                ,CompletedDate       = entity.PaymentDate
            };
        }

         public static RefundViewDTO Entity2RefundViewDto(this vw_SALE_OrderLinePaymentRefunds entity)
        { 
            var type = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.PaymentTypeId.ToString());
            var refundType = Utils.ParseEnum<BillingEnums.ePaymentTypes>(entity.RefundTypeId.ToString());
            return new RefundViewDTO
            {
                 RefundId            = entity.RefundId
                ,PaymentId           = entity.PaymentId
                ,RefundAmount        = entity.RefundAmount
                ,RefundDate          = entity.RefundDate
                ,RefundType          = refundType
                ,RefundTypeName      = Utils.GetEnumDescription(refundType)
                ,OrderNumber         = entity.OrderNumber
                ,ItemName            = entity.ItemName
                ,LineId              = entity.OrderLineId
                ,Number              = entity.PaymentNumber
                ,Amount              = entity.Amount
                ,Status              = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString())
                ,Currency            = entity.Entity2BaseCurrencyDto()
                ,Type                = type
                ,TypeName            = Utils.GetEnumDescription(type)
                ,ScheduledDate       = entity.ScheduledDate
                ,CompletedDate       = entity.PaymentDate
                 ,Buyer              = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.BuyerUserId
                                                            ,Email    = entity.BuyerEmail
                                                            ,FullName = entity.Entity2BuyerFullName()
                                                        }
            };
        }

        public static ScheduledPaymentSummaryToken PaymentEntity2ScheduledPaymentSummaryToken(this vw_SALE_OrderLinePayments entity)
        {
            try
            {
                var pm = Utils.ParseEnum<BillingEnums.ePaymentMethods>(entity.PaymentMethodId);
                return new ScheduledPaymentSummaryToken
                {
                    LineId                  = entity.OrderLineId
                    ,PaymentId              = entity.PaymentId
                    ,OrderNumber            = entity.OrderNumber
                    ,OrderId                = entity.OrderId
                    ,ItemName               = entity.ItemName
                    ,Amount                 = entity.Amount
                    ,Number                 = entity.PaymentNumber
                    ,Status                 = Utils.ParseEnum<BillingEnums.ePaymentStatuses>(entity.StatusId.ToString())
                    ,Currency               = entity.Entity2BaseCurrencyDto()
                    ,ScheduledDate          = entity.ScheduledDate
                    ,PaymentMethod          = pm
                    ,PaymentMethodName      = Utils.GetEnumDescription(pm)
                    ,PaypalProfileID        = entity.PaypalProfileID
                    ,PaymentInstrumentName  = entity.DisplayName
                    ,AutoBill               = !String.IsNullOrEmpty(entity.PaypalProfileID)
                    ,Buyer                  = new BaseUserInfoDTO
                                                            {
                                                                UserId    = entity.BuyerUserId
                                                                ,FullName = entity.Entity2BuyerFullName()
                                                                ,Email    = entity.BuyerEmail
                                                            }
                    ,Seller                 = new BaseUserInfoDTO
                                                            {
                                                                UserId    = entity.SellerUserId
                                                                ,FullName = entity.Entity2SellerFullName()
                                                                ,Email    = entity.SellerEmail
                                                            }
                    ,WebStore               = entity.WebStoreId != null ? new BaseWebStoreDTO
                                                            {
                                                                StoreId = (int) entity.WebStoreId
                                                                ,TrackingID = entity.TrackingID
                                                                ,Name = entity.StoreName

                                                            } : new BaseWebStoreDTO()
                };
            }
            catch (Exception )
            {
                return new ScheduledPaymentSummaryToken();
            }
        }

        public static RefundOrderLinePaymentDTO PaymentEntity2RefundOrderLinePaymentDto(this vw_SALE_OrderLinePayments entity)
        {
            if (entity == null)
            {
                return new RefundOrderLinePaymentDTO
                {
                    IsValid = false
                    ,Message = "payment Entity not found"
                };
            }

            return new RefundOrderLinePaymentDTO
            {
                LineId                 = entity.OrderLineId
                ,PaymentId             = entity.PaymentId
                ,TransactionId         = entity.TransactionId
                ,ExternalTransactionId = entity.ExternalTransactionID
                ,Amount                = entity.Amount - entity.TotalRefunded
                ,IsValid               = entity.TransactionId != null
                ,Message               = entity.TransactionId != null ? string.Empty : "related transaction not found"
            };
        }
    }
}
