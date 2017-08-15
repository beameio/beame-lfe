using System;
using System.Collections.Generic;
using System.Globalization;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Model;
using PayPal.Api;
using PayPal.PayPalAPIInterfaceService.Model;
using Amount = PayPal.Api.Amount;


namespace LFE.Dto.Mapper.DtoMappers
{
    public static class BillingDtoMapper
    {
        public static Payment BasePaymentDto2RestApiPayment(this BasePaymentDTO dto,
                                                            BillingEnums.ePaymentAction action,
                                                            BillingEnums.ePaymentMethods method,
                                                            bool keepDecimal,
                                                            RedirectUrls urls=null, 
                                                            Courses entity = null,
                                                            CRS_Bundles bundleEntity = null,
                                                            Payer payer = null)
        
        {
            var payment = new Payment
            {
                intent = action.EnumToLowerString(),
                payer = new Payer
                {
                    payment_method = method.EnumToLowerString()
                },
                transactions = new List<Transaction>
                {
                    dto.PaymentDto2TransactionToken(keepDecimal)
                }
               
            };

            if (urls != null)
            {
                payment.redirect_urls = urls;
            }

            if (entity != null)
            {
                payment.transactions[0].item_list = new ItemList
                {
                    items = new List<Item>
                                        {
                                            entity.CourseEntity2PaypalRestApiItem(dto,keepDecimal)
                                        }
                };
            }
            else if(bundleEntity!=null)
            {
                payment.transactions[0].item_list = new ItemList
                {
                    items = new List<Item>
                                        {
                                            bundleEntity.BundleEntity2PaypalRestApiItem(dto,keepDecimal)
                                        }
                };
            }


            if (payer != null)
            {
                payment.payer = payer;
            }

            return payment;
        }
     
        public static Payer CreditCard2RestApiPayer(this PaypalCreditCardPaymentDTO dto,string payerEmail)
        {
            var creditCard = dto.card.CreditCardDto2RestApiCreditCard();

            var fundInstrument = new FundingInstrument { credit_card = creditCard };

            var fundingInstrumentList = new List<FundingInstrument> { fundInstrument };

            return new Payer
            {
                funding_instruments = fundingInstrumentList,
                payment_method      = BillingEnums.ePaymentMethods.Credit_Card.EnumToLowerString(),
                payer_info          = new PayerInfo
                                        {
                                            email = payerEmail
                                        }
            };
        }
        
        public static Payer CreditCard2RestApiPayer(this string cardId)
        {
            var creditCard = new CreditCardToken { credit_card_id = cardId };

            var fundInstrument = new FundingInstrument { credit_card_token = creditCard };

            var fundingInstrumentList = new List<FundingInstrument> { fundInstrument };

            return new Payer { funding_instruments = fundingInstrumentList, payment_method = BillingEnums.ePaymentMethods.Credit_Card.EnumToLowerString() ,payer_info = new PayerInfo()};
        }        

        private static Item CourseEntity2PaypalRestApiItem(this Courses entity, BasePaymentDTO dto,bool keepDecimal)
        {
            return new Item{
                            name      = entity.CourseName
                            ,price    = dto.amount.FormatMoney(keepDecimal ? 2 : 0).ToString(CultureInfo.InvariantCulture)
                            ,currency = dto.currency
                            ,quantity = "1"
                            ,sku      = entity.Id.ToString()
                        };
        }

        private static Item BundleEntity2PaypalRestApiItem(this CRS_Bundles entity, BasePaymentDTO dto, bool keepDecimal)
        {
            return new Item{
                            name      = entity.BundleName
                            ,price    = dto.amount.FormatMoney(keepDecimal ? 2 : 0).ToString(CultureInfo.InvariantCulture)
                            ,currency = dto.currency
                            ,quantity = "1"
                            ,sku      = entity.BundleId.ToString()
                        };
        }

        private static Transaction PaymentDto2TransactionToken(this BasePaymentDTO dto, bool keepDecimal)
        {
            return new Transaction
            {
                amount      = dto.PaymentDto2AmountToken(keepDecimal),
                description = dto.description
            };
        }

        public static Amount PaymentDto2AmountToken(this BasePaymentDTO dto, bool keepDecimal)
        {
            return  new Amount
                        {
                            currency = dto.currency,
                            total    = dto.amount.FormatMoney(keepDecimal ? 2 : 0).ToString(CultureInfo.InvariantCulture),
                            details  = new Details()
                        };
        }

        public static CreditCard CreditCardDto2RestApiCreditCard(this CreditCardDTO dto,bool setAddress=true)
        {
            var cc = new CreditCard
                                {
                                    type            = dto.Type.EnumToLowerString(),
                                    number          = dto.CardNumber,
                                    cvv2            = dto.CVV2,
                                    expire_month    = dto.ExpireMonth,
                                    expire_year     = dto.ExpireYear,
                                    first_name      = dto.FirstName,
                                    last_name       = dto.LastName
                                };

            if (setAddress) cc.billing_address = dto.BillingAddress.AddresToken2PaypalAddress();

            return cc;
        }

        public static CreditCardDTO RectApiCreditCard2CreditCardDto(this CreditCard card)
        {
            return new CreditCardDTO
            {
                Type         = Utils.ParseEnum<BillingEnums.eCreditCardType>(card.type)
                ,CardNumber  = card.number
                ,CVV2        = card.cvv2
                ,ExpireMonth = card.expire_month
                ,ExpireYear  = card.expire_year
                ,FirstName   = card.first_name
                ,LastName    = card.last_name
            };
        }

        public static PaymentInstrumentDTO Entity2PaymentInstrumentDto(this USER_PaymentInstruments entity)
        {
            return new PaymentInstrumentDTO
            {
                InstrumentId = entity.InstrumentId
                ,DisplayName = $"{entity.CreditCardType}: {entity.DisplayName}"
            };
        }

        public static PaymentInstrumentViewDTO RectApiCreditCard2PaymentInstrumentDto(this CreditCard card, Guid instrumentId, string displayName)
        {

            return new PaymentInstrumentViewDTO
            {
                InstrumentId = instrumentId
                ,DisplayName = $"{card.type.CapitalizeWord()}, card no {displayName}, valid until {card.expire_month}/{card.expire_year}, card holder name {card.first_name.CapitalizeWord()} {card.last_name.CapitalizeWord()}"
                ,State       = card.state
            };
        }

        public static Address AddresToken2PaypalAddress(this BillingAddressDTO token)
        {
            return new Address
            {
                country_code = token.CountryCode.TrimString()
                ,city        = token.City.TrimString()
                ,postal_code = token.PostalCode.TrimString()
                ,line1       = token.Street1.TrimString()
                ,line2       = string.IsNullOrEmpty(token.Street2) ? token.Street1.TrimString() : token.Street2.TrimString()
                ,state       = string.IsNullOrEmpty(token.StateCode) ? null : token.StateCode.TrimString()
            };
        }

        public static PaypalPaymentRequestDTO DirectCcToken2PaypalPaymentRequestDto(this PaypalCreditCardPaymentDTO dto,int userId,Guid requestId,BillingEnums.ePaypalRequestTypes type)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId             = requestId
                ,UserId              = userId
                ,PaymentMethod       = BillingEnums.ePaymentMethods.Credit_Card
                ,PaypalRequestType   = type
                ,Amount              = dto.amount
                ,PriceLineId         = dto.priceLineId
                ,CourseId            = dto.courseId
                ,BundleId            = dto.bundleId
                ,TrackingID          = dto.trackingId
                ,AddressId           = dto.addressId
                ,PaymentInstrumentId = dto.paymentInstrumentId
                ,CouponCode          = dto.couponCode
            };
        }

        public static PaypalPaymentRequestDTO DirectCcToken2PaypalPaymentRequestDto(this SubscriptionWithSavedCardDTO dto,decimal amount,Guid requestId,BillingEnums.ePaypalRequestTypes type)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId             = requestId
                ,UserId              = dto.UserId
                ,PaymentMethod       = BillingEnums.ePaymentMethods.Credit_Card
                ,PaypalRequestType   = type
                ,Amount              = amount
                ,PriceLineId         = dto.priceLineId
                ,CourseId            = dto.courseId
                ,BundleId            = dto.bundleId
                ,TrackingID          = dto.trackingId
                ,AddressId           = dto.addressId
                ,PaymentInstrumentId = dto.paymentInstrumentId
                ,CouponCode          = dto.couponCode
            };
        }

        public static PaypalPaymentRequestDTO OrderLine2PaypalCancelSubscriptionRequestDto(this vw_SALE_OrderLines request, Guid requestId,BillingEnums.ePaypalRequestTypes type)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId             = requestId
                ,UserId              = request.BuyerUserId
                ,PaymentMethod       = Utils.ParseEnum<BillingEnums.ePaymentMethods>(request.PaymentMethodId)
                ,PaypalRequestType   = type
                ,Amount              = 0
                ,CourseId            = request.CourseId          
                ,BundleId            = request.BundleId
                ,TrackingID          = request.TrackingID
                ,AddressId           = request.AddressId
                ,PaymentInstrumentId = request.InstrumentId
                ,CouponCode          = request.CouponCode
            };
        }

        public static PaypalPaymentRequestDTO SourceRequest2PaypalPaymentRequestDto(this PAYPAL_PaymentRequests request, Guid requestId, decimal amount, BillingEnums.ePaypalRequestTypes type)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId             = requestId
                ,UserId              = request.UserId
                ,PaymentMethod       = Utils.ParseEnum<BillingEnums.ePaymentMethods>(request.PaymentMethodId)
                ,PaypalRequestType   = type
                ,Amount              = amount
                ,PriceLineId         = request.PriceLineId
                ,CourseId            = request.CourseId
                ,BundleId            = request.BundleId
                ,TrackingID          = request.TrackingID
                ,AddressId           = request.AddressId
                ,PaymentInstrumentId = request.InstrumentId
                ,CouponCode          = request.CouponCode
                ,SourceReuqstId      = request.RequestId
            };
        }
        
        public static PaypalPaymentRequestDTO PaypalPaymentRequestDTO2PaypalPaymentRequestDto(this PaypalPaymentRequestDTO dto, int userId, Guid requestId, BillingEnums.ePaypalRequestTypes type)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId             = requestId
                ,UserId              = userId
                ,PaymentMethod       = BillingEnums.ePaymentMethods.Credit_Card
                ,PaypalRequestType   = type
                ,Amount              = dto.Amount
                ,PriceLineId         = dto.PriceLineId
                ,CourseId            = dto.CourseId
                ,BundleId            = dto.BundleId
                ,TrackingID          = dto.TrackingID
                ,AddressId           = dto.AddressId                
                ,CouponCode          = dto.CouponCode
            };
        }        

        public static PaypalPaymentRequestDTO AccountPaymentToken2PaypalPaymentRequestDto(this PayPalCreatePaymentDTO dto, int userId, Guid requestId,string payment_id, BillingEnums.ePaypalRequestTypes type)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId             = requestId
                ,UserId              = userId
                ,CreatePaymentId     = payment_id
                ,PaymentMethod       = BillingEnums.ePaymentMethods.Paypal
                ,PaypalRequestType   = type
                ,Amount              = dto.amount
                ,PriceLineId         = dto.priceLineId
                ,CourseId            = dto.courseId
                ,BundleId            = dto.bundleId
                ,TrackingID          = dto.trackingId
                ,AddressId           = dto.addressId
                ,PaymentInstrumentId = dto.paymentInstrumentId
                ,CouponCode          = dto.couponCode
            };
        }

        public static PaypalPaymentRequestDTO SaveCCRequest2PaypalPaymentRequestDto(this  Guid requestId,int userId)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId           = requestId
                ,UserId            = userId
                ,PaymentMethod     = BillingEnums.ePaymentMethods.Credit_Card
                ,PaypalRequestType = BillingEnums.ePaypalRequestTypes.CC_SAVE
            };
        }

        public static PaypalPaymentRequestDTO PaymentAgreementRequest2PaypalPaymentRequestDto(this PayPalAgreementDTO dto, Guid requestId, int userId)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId               = requestId
                ,UserId                = userId
                ,PaymentMethod         = dto.method
                ,PaypalRequestType     = BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_AGREEMENT
                ,Amount                = dto.amount
                ,PriceLineId           = dto.priceLineId
                ,CourseId              = dto.courseId
                ,BundleId              = dto.bundleId
                ,TrackingID            = dto.trackingId
                ,AddressId             = dto.addressId
                ,PaymentInstrumentId   = dto.paymentInstrumentId.Equals(Guid.Empty) ? null : dto.paymentInstrumentId
                ,CouponCode            = dto.couponCode
            };
        }

        public static PaypalPaymentRequestDTO PaypalRequestEntity2PaypalPaymentRequestDto(this PAYPAL_PaymentRequests entity, Guid requestId)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId               = requestId
                ,UserId                = entity.UserId
                ,PaymentMethod         = Utils.ParseEnum<BillingEnums.ePaymentMethods>(entity.PaymentMethodId)
                ,PaypalRequestType     = BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_EXECUTION
                ,Amount                = entity.Amount ?? 0
                ,PriceLineId           = entity.PriceLineId
                ,CourseId              = entity.CourseId
                ,BundleId              = entity.BundleId
                ,TrackingID            = entity.TrackingID
                ,AddressId             = entity.AddressId
                ,PaymentInstrumentId   = entity.InstrumentId
                ,CouponCode            = entity.CouponCode
                ,SourceReuqstId        = entity.RequestId
            };
        }

        //merchant
        public static CreditCardDetailsType CreditCardDto2MerchantApiCreditCard(this CreditCardDTO dto)
        {
            return new CreditCardDetailsType
            {
                CreditCardType = Utils.ParseEnum<CreditCardTypeType>(dto.Type.ToString().ToUpper()),
                CreditCardNumber = dto.CardNumber,
                CVV2             = dto.CVV2,
                ExpMonth         = dto.ExpireMonth,
                ExpYear          = dto.ExpireYear,
                CardOwner = new PayerInfoType
                {
                    Payer    = dto.FirstName + " " + dto.LastName                    
                    ,Address = new AddressType
                    {
                        Name = dto.FirstName + " " + dto.LastName
                    }
                },                
            };
        }

        public static int RecurringPaymentPeriod2Frequency(this BillingEnums.eBillingPeriodType type)
        {
            return 1;
            //TODO future logic for subscription planes
            //switch (type)
            //{
            //    case BillingEnums.eBillingPeriodType.DAY:
            //        return 365;
            //    case BillingEnums.eBillingPeriodType.WEEK:
            //        return 52;
            //    case BillingEnums.eBillingPeriodType.SEMIMONTH:
            //        return 1;
            //    case BillingEnums.eBillingPeriodType.MONTH:
            //        return 1;
            //    default:
            //        return 1;
            //}
        }

        public static string PriceLineToken2Title(this PriceLineDTO token)
        {
            switch (token.PriceType)
            {
                case BillingEnums.ePricingTypes.ONE_TIME:
                    return "One time payment";
                case BillingEnums.ePricingTypes.SUBSCRIPTION:
                    return $"{token.PeriodType.PeriodTypeTranslate()} Subscription";
                case BillingEnums.ePricingTypes.RENTAL:
                    return "Rental";
                case BillingEnums.ePricingTypes.FREE:
                    return "Free";
            }

            return string.Empty;
        }

        public static string PriceLineToken2Name(this PriceLineDTO token)
        {
            switch (token.PriceType)
            {
                case BillingEnums.ePricingTypes.ONE_TIME:
                    return "One-time";
                case BillingEnums.ePricingTypes.SUBSCRIPTION:
                    return token.PeriodType != null ? token.PeriodType.PeriodTypeTranslate() : "Monthly";
                case BillingEnums.ePricingTypes.RENTAL:
                    return token.Name;
            }

            return string.Empty;
        }

        public static string PriceLineToken2PurchaseNameSuffix(this PriceLineDTO token)
        {
            switch (token.PriceType)
            {
                case BillingEnums.ePricingTypes.ONE_TIME:
                    return "Unlimited Access";
                case BillingEnums.ePricingTypes.SUBSCRIPTION:
                    return
                        $"{(token.PeriodType != null ? token.PeriodType.PeriodTypeTranslate() : "Monthly")} subscription";
                case BillingEnums.ePricingTypes.RENTAL:
                    return $"{token.Name} Rental";
            }

            return string.Empty;
        }

        public static string PriceLineToken2PurchaseEmailPaymentMethod(this PriceLineDTO token)
        {
            switch (token.PriceType)
            {
                case BillingEnums.ePricingTypes.ONE_TIME:
                    return "One-time payment";
                case BillingEnums.ePricingTypes.SUBSCRIPTION:
                    return
                        $"{(token.PeriodType != null ? token.PeriodType.PeriodTypeTranslate() : "Monthly")} subscription";
                case BillingEnums.ePricingTypes.RENTAL:
                    return $"{token.Name} Rental";
            }

            return string.Empty;
        }

        private static string PeriodTypeTranslate(this BillingEnums.eBillingPeriodType? type)
        {
            switch (type)
            {
                case BillingEnums.eBillingPeriodType.DAY:
                    return "Daily";
                case BillingEnums.eBillingPeriodType.WEEK:
                    return "Weekly";
                case BillingEnums.eBillingPeriodType.SEMIMONTH:
                    return "Semi-Monthly";
                case BillingEnums.eBillingPeriodType.MONTH:
                    return "Monthly";
                default:
                    return string.Empty;
            }
        }

        public static PaypalRecurringProfileToken PaypalResponse2PaypalRecurringProfileToken(this GetRecurringPaymentsProfileDetailsResponseType response)
        {
            var token = response.GetRecurringPaymentsProfileDetailsResponseDetails;

            return new PaypalRecurringProfileToken
            {
                ProfileID         = token.ProfileID
                ,Description      = token.Description
                ,SubscriberName   = token.RecurringPaymentsProfileDetails.SubscriberName
                ,Status           = token.ProfileStatus.ToString()
                ,BillingFrequency = token.RegularRecurringPaymentsPeriod.BillingFrequency
                ,BillingPeriod    = token.RegularRecurringPaymentsPeriod.BillingPeriod.ToString()
                ,CompletedCycles  = token.RecurringPaymentsSummary.NumberCyclesCompleted
                ,AmountPaid       = Convert.ToDecimal(token.RegularAmountPaid.value)
                ,BillingStartDate = Convert.ToDateTime(token.RecurringPaymentsProfileDetails.BillingStartDate)
                ,NextPaymentDate  = Convert.ToDateTime(token.RecurringPaymentsSummary.NextBillingDate)
                ,LastPaymentDate  = Convert.ToDateTime(token.RecurringPaymentsSummary.LastPaymentDate)
            };
        }

        public static SubscriptionWithSavedCardDTO PaymentEntity2SubscriptionWithSavedCardDto(this vw_SALE_OrderLinePayments token,string description, Guid instrumentId)
        {
            return new SubscriptionWithSavedCardDTO
            {
                 UserId                = token.BuyerUserId
                ,BillingAmount         = token.Amount
                ,amount                = token.Amount    
                ,currency              = token.ISO ?? "USD"
                ,courseId              = token.CourseId
                ,bundleId              = token.BundleId
                ,PaymentInstrumentId   = instrumentId
                ,trackingId            = token.TrackingID                                
                ,ProfileDescription    = token.ItemName
                ,description           = description                
            };
        }

        //billing manage services

        public static PriceLineDTO Entity2PriceLineDto(this BILL_ItemsPriceList entity,CurrencyDTO currency)
        {
            var token =  new PriceLineDTO
            {
                PriceLineID       = entity.PriceLineId
                ,ItemId           = entity.ItemId
                ,ItemType         = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId)
                ,PriceType        = Utils.ParseEnum<BillingEnums.ePricingTypes>(entity.PriceTypeId)
                ,Price            = entity.Price.FormatPrice(currency.KeepDecimal ? 2 : 0)
                ,Name             = entity.Name
                ,NumOfPeriodUnits = entity.NumOfPeriodUnits
                ,Currency         = currency.ToBaseCurrencyDto()
            };
            
            token.Title = token.PriceLineToken2Title();

            if(entity.PeriodTypeId != null) token.PeriodType = Utils.ParseEnum<BillingEnums.eBillingPeriodType>(entity.PeriodTypeId.ToString());

            token.Name = token.PriceLineToken2Name();

            return token;
        }

        public static BaseCurrencyDTO ToBaseCurrencyDto(this CurrencyDTO token)
        {
            return new BaseCurrencyDTO
            {
                CurrencyId    = token.CurrencyId
                ,CurrencyName = token.CurrencyName
                ,ISO          = token.ISO
                ,Symbol       = string.IsNullOrEmpty(token.Symbol) ? token.ISO : token.Symbol
            };
        }

        public static CurrencyDTO Entity2CurrencyDTO(this BASE_CurrencyLib entity)
		{
			return new CurrencyDTO
						{
							CurrencyId          = entity.CurrencyId
							,CurrencyCode       = entity.CurrencyCode
							,CurrencyName       = entity.CurrencyName
							,CountryId          = entity.CountryId
							,ISO                = entity.ISO
							,IsActive           = entity.IsActive
							,Symbol             = entity.Symbol	
                            ,KeepDecimal		= entity.KeepDecimal
						};
		}

    }
}
