using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Application.Services
{
    public class CouponServices : ServiceBase, IAuthorAdminCouponServices, ICouponWidgetServices
    {

        #region private helpers
      
        private bool IsCourseCouponNameValid(int couponId, int courseId, string couponName, out string error)
        {
            error = string.Empty;
            try
            {
                if (couponId < 0)
                {
                    return !CouponRepository.IsAny(x => x.CourseId == courseId && x.CouponName == couponName);
                }

                return !CouponRepository.IsAny(x => x.CourseId == courseId && x.CouponName == couponName && x.Id != couponId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool IsBundleCouponNameValid(int couponId,int bundleId,string couponName, out string error)
        {
            error = string.Empty;
            try
            {
                if (couponId < 0)
                {
                    return !CouponRepository.IsAny(x => x.BundleId == bundleId && x.CouponName == couponName);
                }

                return !CouponRepository.IsAny(x => x.BundleId == bundleId && x.CouponName == couponName && x.Id != couponId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool IsCouponValid(ref AuthorCouponDTO dto, out string error)
        {
            error = string.Empty;
            try
            {
                switch (dto.Kind)
                {
                    case CourseEnums.eCouponKinds.Author:
                        dto.CourseId = null;
                        dto.BundleId = null;
                    
                        if (dto.OwnerUserId == null || dto.OwnerUserId < 0)
                        {
                            error = "owner user required";
                            return false;
                        }
                        
                        var couponDto = dto;

                        if (dto.CouponId < 0)
                        {
                           
                            return !CouponRepository.IsAny(x => x.OwnerUserId == couponDto.OwnerUserId && x.CouponName == couponDto.CouponName);
                        }

                        return !CouponRepository.IsAny(x => x.OwnerUserId == couponDto.OwnerUserId && x.CouponName == couponDto.CouponName && x.Id != couponDto.CouponId);
                    
                    case CourseEnums.eCouponKinds.Course:
                        dto.OwnerUserId = null;
                        dto.BundleId = null;

                        if (dto.CourseId != null)
                        {
                            if (IsCourseCouponNameValid(dto.CouponId, (int) dto.CourseId, dto.CouponName, out error)) return true;

                            error = "coupon name already exists";
                            return false;
                        }
                        
                        error = "course required";
                        return false;
                    
                    case CourseEnums.eCouponKinds.Bundle:
                        dto.OwnerUserId = null;
                        dto.CourseId = null;

                        if (dto.BundleId != null)
                        {
                            if (IsBundleCouponNameValid(dto.CouponId, (int) dto.BundleId, dto.CouponName, out error)) return true;

                            error = "coupon name already exists";
                            return false;
                        }
                        
                        error = "bundle required";
                        return false;
                }

                error = "invalid kind";
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool IsCouponCodeValid(string couponCode,int couponId, out string error)
        {
            error = string.Empty;
            try
            {
                return couponId < 0 ? !CouponInstanceRepository.IsAny(x => x.CouponCode == couponCode)  : !CouponInstanceRepository.IsAny(x => x.CouponCode == couponCode && x.CouponId != couponId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool SaveCouponInstance(CouponDTO dto, out string error)
        {            
            try
            {
                var entities = CouponInstanceRepository.GetMany(x => x.CouponId == dto.CouponId).ToList();

                if (!entities.Any())
                {
                    CouponInstanceRepository.Add(dto.CourseCouponDto2CouponInstanceEntity());
                }
                else
                {
                    if (entities.Count().Equals(1))
                    {
                        var entity = entities[0];
                        entity.UpdateCouponInstanceEntity(dto);
                    }
                    //else
                    //{
                        //TODO ask for logic
                    //}
                }

                return CouponInstanceRepository.UnitOfWork.CommitAndRefreshChanges(out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool SaveUniqueCouponInstance(int couponId,string code, out string error)
        {
            try
            {
                var entities = CouponInstanceRepository.GetMany(x => x.CouponId == couponId && x.CouponCode == code).ToList();

                if (!entities.Any())
                {
                    CouponInstanceRepository.Add((new CouponDTO
                    {
                       CouponId = couponId,
                       CouponCode = code,
                       UsageLimit = 1
                    }).CourseCouponDto2CouponInstanceEntity(code));
                }
                else
                {
                    error = "already exists";
                    return false;
                }

                return CouponInstanceRepository.UnitOfWork.CommitAndRefreshChanges(out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private void GetCouponProperties(int couponId, out int usageLimit, out int actualUsage, out string couponCode)
        {
            
            var instances = CouponInstanceRepository.GetMany(x => x.CouponId == couponId).ToList();
            var ids = instances.Select(i => i.Id).ToList();
            usageLimit = instances.Any() ? (instances.Count == 1 ? instances[0].UsageLimit : instances.Count) : 0;
            couponCode = instances.Any() ? (instances.Count == 1 ? instances[0].CouponCode : string.Empty) : string.Empty;
            actualUsage = instances.Any() ? OrderLineRepository.Count(x => x.CouponInstanceId != null && ids.Contains((int)x.CouponInstanceId)) : 0;
        }

        #endregion

        #region IAuthorAdminCouponServices implementation
        #region course
        /// <summary>
        /// get course coupons for tab
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public List<CourseCouponDTO> GetCourseCoupons(int courseId)
        {
            return CouponRepository.GetCourseCoupons(courseId).Select(x => x.Entity2CouponDTO()).ToList();
        }
        /// <summary>
        /// get coupon dto for course coupon edit form
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public CourseCouponDTO GetCourseCoupon(int couponId, int courseId)
        {
            if (couponId < 0) return new CourseCouponDTO(courseId);

            var entity = CouponRepository.Get(x => x.Id == couponId);

            if (entity == null) return new CourseCouponDTO(couponId, courseId);

            var dto = entity.Entity2CourseCouponDTO();

            int usageLimit;
            int actualUsage;
            string couponCode;
            
            GetCouponProperties(couponId,out usageLimit,out actualUsage,out couponCode);

            dto.UsageLimit  = usageLimit;
            dto.CouponCode  = couponCode;
            dto.ActualUsage = actualUsage;

            return dto;
        }        
        /// <summary>
        /// save course coupon
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveCourseCoupon(ref CourseCouponDTO dto, out string error)
        {

            if (dto.CourseId == null)
            {
                error = "courseId missing";
                return false;
            }

            if (!IsCourseCouponNameValid(dto.CouponId, (int)dto.CourseId, dto.CouponName, out error))
            {
                error = String.IsNullOrEmpty(error) ? "Coupon Name already exists" : error;
                return false;
            }

            //if (!IsCouponCodeValid(dto.CouponCode, dto.CouponId, out error))
            //{
            //    error = String.IsNullOrEmpty(error) ? "Coupon code already exists" : error;
            //    return false;
            //}

            try
            {
                if (dto.CouponId < 0) //new course
                {
                    var entity = dto.CourseCouponDto2CouponEntity();
                    CouponRepository.Add(entity);

                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    dto.CouponId = entity.Id;
                }
                else
                {
                    var entity = CouponRepository.GetById(dto.CouponId);

                    if (entity == null)
                    {
                        error = "Course entity not found";
                        return false;
                    }

                    entity.UpdateCouponEntity(dto);

                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                }

                return SaveCouponInstance(dto, out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course coupon dto", dto.CourseId ?? -1, ex, CommonEnums.LoggerObjectTypes.Coupon);
                return false;
            }
        } 
        #endregion
        
        #region bundle
        public List<BundleCouponDTO> GetBundleCoupons(int bundleId)
        {
            return CouponRepository.GetBundleCoupons(bundleId).Select(x => x.Entity2BundleCouponDTO()).ToList();
        }

        public BundleCouponDTO GetBundleCoupon(int couponId, int bundleId)
        {
            if (couponId < 0) return new BundleCouponDTO(bundleId);

            var entity = CouponRepository.Get(x => x.Id == couponId);

            if (entity == null) return new BundleCouponDTO(couponId, bundleId);

            var dto = entity.Entity2BundleCouponDTO();

            int usageLimit;
            int actualUsage;
            string couponCode;

            GetCouponProperties(couponId, out usageLimit, out actualUsage, out couponCode);

            dto.UsageLimit  = usageLimit;
            dto.CouponCode  = couponCode;
            dto.ActualUsage = actualUsage;

            return dto;
        }

        public bool SaveBundleCoupon(ref BundleCouponDTO dto, out string error)
        {
            if (dto.BundleId == null)
            {
                error = "bundleId missing";
                return false;
            }

            if (!IsBundleCouponNameValid(dto.CouponId, (int)dto.BundleId, dto.CouponName, out error))
            {
                error = String.IsNullOrEmpty(error) ? "Coupon Name already exists" : error;
                return false;
            }

            if (!IsCouponCodeValid(dto.CouponCode, dto.CouponId, out error))
            {
                error = String.IsNullOrEmpty(error) ? "Coupon code already exists" : error;
                return false;
            }

            try
            {
                if (dto.CouponId < 0) //new course
                {
                    var entity = dto.BundleCouponDto2CouponEntity();
                    CouponRepository.Add(entity);

                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    dto.CouponId = entity.Id;
                }
                else
                {
                    var entity = CouponRepository.GetById(dto.CouponId);

                    if (entity == null)
                    {
                        error = "Course entity not found";
                        return false;
                    }

                    entity.UpdateCouponEntity(dto);
                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                }

                //var name = dto.CouponName;
                //var courseId = dto.CourseId;

                //dto.CouponId = CouponRepository.Get(x => x.CourseId == courseId && x.CouponName == name).Id;

                return SaveCouponInstance(dto, out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save bundle coupon dto", dto.BundleId ?? -1, ex, CommonEnums.LoggerObjectTypes.Coupon);
                return false;
            }
        } 
        #endregion
        
        #region author coupons
        public List<AuthorCouponDTO> GetAuthorCoupons(int authorId)
        {
            return CouponRepository.GetAuthorCoupons(authorId).Select(x => x.Entity2AuthorCouponDTO(authorId)).ToList();
        }

        public AuthorCouponDTO GetAuthorCoupon(int couponId, int userId)
        {
            if (couponId < 0) return new AuthorCouponDTO(userId);

            var entity = CouponRepository.Get(x => x.Id == couponId);

            if (entity == null) return new AuthorCouponDTO(couponId, userId);

            var dto = entity.Entity2AuthorCouponDTO(userId);

            int usageLimit;
            int actualUsage;
            string couponCode;

            GetCouponProperties(couponId, out usageLimit, out actualUsage, out couponCode);

            dto.UsageLimit = usageLimit;
            dto.CouponCode = couponCode;
            dto.ActualUsage = actualUsage;

            return dto;
        }

        public bool SaveAuthorCoupon(ref AuthorCouponDTO dto, out string error)
        {

            if (dto.TotalInstances > 1) return SaveAuthorCoupon(ref dto, dto.TotalInstances, out error);

            if (!IsCouponValid(ref dto, out error)) return false;

            if (!IsCouponCodeValid(dto.CouponCode, dto.CouponId, out error))
            {
                error = String.IsNullOrEmpty(error) ? "Coupon code already exists" : error;
                return false;
            }

            try
            {
                if (dto.CouponId < 0) //new course
                {
                    var entity = dto.AuthorCouponDto2CouponEntity();
                    CouponRepository.Add(entity);

                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    dto.CouponId = entity.Id;
                }
                else
                {
                    var entity = CouponRepository.GetById(dto.CouponId);

                    if (entity == null)
                    {
                        error = "Course entity not found";
                        return false;
                    }

                    entity.UpdateCouponEntity(dto);
                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                }
                return SaveCouponInstance(dto, out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save author coupon dto", dto.BundleId ?? -1, ex, CommonEnums.LoggerObjectTypes.Coupon);
                return false;
            }
        }

        private string GenerateInstanceCode(int couponId, int attempt)
        {
            var code = (ShortGuid.NewGuid()).ToString().Substring(0, 10);
            string error;
            if (IsCouponCodeValid(code,couponId, out error))
            {
                return code.Replace("-","").ToUpper();
            }

            if (attempt >= 5) return string.Empty;

            var next = attempt+1;

            return GenerateInstanceCode(couponId, next);
        }

        public bool SaveAuthorCoupon(ref AuthorCouponDTO dto,int totalInstances, out string error)
        {

            if (!IsCouponValid(ref dto, out error)) return false;

            try
            {

                var instanceCodes = new List<string>();

                for (var i = 0; i < totalInstances; i++)
                {
                    var code = GenerateInstanceCode(dto.CouponId, 0);
                    if (!String.IsNullOrEmpty(code))
                    {
                        instanceCodes.Add(code);
                    }
                    else
                    {
                        error = "Instance code creation failed. Please contact support team";
                        return false;
                    }
                }


                if (dto.CouponId < 0) //new course
                {
                    var entity = dto.AuthorCouponDto2CouponEntity();
                    CouponRepository.Add(entity);

                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    dto.CouponId = entity.Id;
                }
                else
                {
                    var entity = CouponRepository.GetById(dto.CouponId);

                    if (entity == null)
                    {
                        error = "Course entity not found";
                        return false;
                    }

                    entity.UpdateCouponEntity(dto);
                    if (!CouponRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                }
                var totalCreated = 0;
                for (var i = 0; i < totalInstances; i++)
                {
                    var saved = SaveUniqueCouponInstance(dto.CouponId, instanceCodes[i], out error);

                    if (saved) totalCreated++;
                }

                error = totalCreated == totalInstances ? "" : String.Format("Total {0} coupons created",totalCreated);

                return totalCreated == totalInstances;

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save author coupon dto", dto.BundleId ?? -1, ex, CommonEnums.LoggerObjectTypes.Coupon);
                return false;
            }
        } 
        #endregion

        //common        
        /// <summary>
        /// delete coupon
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool DeleteCoupon(int couponId, out string error)
        {
            error = string.Empty;

            try
            {
                var entity = CouponRepository.GetById(couponId);

                if (entity == null)
                {
                    error = "Coupon entity not found";
                    return false;
                }

                CouponRepository.Delete(entity);

                CouponRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete coupon", couponId, ex, CommonEnums.LoggerObjectTypes.Coupon);
                return false;
            }
        }
        #endregion

        #region ICouponWidgetServices implementation

        public CouponBaseDTO GetCouponBaseToken(int couponInstanceId, out string error)
        {
            error = string.Empty;

            var inst = CouponInstanceRepository.GetById(couponInstanceId);
            if (inst == null)
            {
                error = "coupon instance not found";
                return null;
            }

            var coupon = CouponRepository.GetById(inst.CouponId);
            if (coupon == null)
            {
                error = "coupon not found";
                return null;
            }

            return coupon.Entity2CouponBaseDTO();
        }

        public CouponValidationToken ValidateCoupon(int priceLineId,int couponOwnerId, int? courseId, int? bundleId, string couponCode)
        {
            var result = new CouponValidationToken
            {
                IsValid   = false
                ,Discount = 0
            };

            if (string.IsNullOrEmpty(couponCode))
            {
                result.Message = "coupon code required";
                return result;
            }
           
            var code = couponCode.TrimString().OptimizedUrl();            
            var inst = CouponInstanceRepository.GetMany(x=>x.CouponCode.ToLower() == code 
                                                    && ( 
                                                            (courseId!= null && x.Coupons.CourseId==courseId) || 
                                                            (bundleId!= null && x.Coupons.BundleId==bundleId) || 
                                                            (x.Coupons.OwnerUserId != null && x.Coupons.OwnerUserId==couponOwnerId)
                                                        )
                                                    ).ToList();
            
            if (inst.Count() == 1) return ValidateCoupon(priceLineId,couponOwnerId,courseId, bundleId, inst[0].Id);
            
            result.Message = inst.Count.Equals(0) ? "coupon instance not found" : "multiple coupon instances found. please contact author";
            return result;
        }

        public CouponValidationToken ValidateCoupon(int priceLineId, int couponOwnerId, int? courseId, int? bundleId, int couponInstanceId)
        {
            var result = new CouponValidationToken
            {
                IsValid = false
                ,Discount = 0
            };

            var inst = CouponInstanceRepository.GetById(couponInstanceId);
            if (inst == null)
            {
                result.Message = "coupon instance not found";
                return result;
            }

            var coupon = CouponRepository.GetById(inst.CouponId).Entity2AuthorCouponDTO();
            if (coupon == null)
            {
                result.Message = "coupon not found";
                return result;
            }

            var priceToken = GetItemPriceToken(priceLineId);
            if (priceToken == null)
            {
                result.Message = "Price Line not found";
                return result;
            }

            decimal basePrice;

            if (courseId != null)
            {

                var course = CourseRepository.GetById((int) courseId);

                if (course == null)
                {
                    result.Message = "course not found";
                    return result;
                }

                if (coupon.CourseId != null)
                {
                    if (coupon.CourseId != courseId)
                    {
                        result.Message = "coupon not allowed to this course";
                        return result;
                    }                    
                }
                else
                {
                    if (coupon.OwnerUserId == null || (coupon.OwnerUserId != course.AuthorUserId))
                    {
                        result.Message = "coupon not allowed to this course";
                        return result;
                    }
                }

                var itemPrice = priceToken.Price;

                if (itemPrice == 0)
                {
                    result.Message = "invalid price";
                    return result;
                }

                basePrice = itemPrice;
            }
            else if (bundleId != null)
            {
                var bundle = BundleRepository.GetById((int) bundleId);

                if (bundle == null)
                {
                    result.Message = "bundle not found";
                    return result;
                }

                if (coupon.BundleId != null)
                {
                    if (coupon.BundleId != bundleId)
                    {
                        result.Message = "coupon not allowed to this bundle";
                        return result;
                    }
                }
                else
                {
                    if (coupon.OwnerUserId == null || (coupon.OwnerUserId != bundle.AuthorId))
                    {
                        result.Message = "coupon not allowed to this bundle";
                        return result;
                    }
                }

                var itemPrice = priceToken.Price;

                if (itemPrice == 0)
                {
                    result.Message = "invalid price";
                    return result;
                }

                basePrice = itemPrice;
            }
            else
            {
                result.Message = "course or bundle required";                
                return result;
            }

            var objectName = courseId!= null ? "course" : "bundle";

            result.OriginalPrice = basePrice;
            result.FinalPrice    = basePrice;

            


            if ((coupon.CourseId.HasValue && coupon.CourseId != courseId))
            {
                result.Message = "This coupon is not allowed for this " + objectName;
            } 
            else if (coupon.ExpirationDate < DateTime.Now.AddDays(-1))
            {
                result.Message = "This coupon is expired";
            }
            else if (inst.UsageLimit != -1 && inst.SALE_OrderLines.Count >= inst.UsageLimit)
            {
                result.Message = "This coupon is no longer valid";
            }
            else
            {
                if (priceToken.PriceType == BillingEnums.ePricingTypes.SUBSCRIPTION && coupon.Type != CourseEnums.CouponType.SUBSCRIPTION)
                {
                    result.Message = "This coupon is not allowed for this " + objectName;
                    return result;
                }

                result.IsValid = true;

                switch (coupon.Type)
                {
                    case CourseEnums.CouponType.PERCENT:
                    case CourseEnums.CouponType.SUBSCRIPTION:
                        result.Discount   = (decimal) (coupon.Amount!= null ? basePrice * (coupon.Amount / 100) : 0);
                        result.FinalPrice = CalculateDiscountedPrice(basePrice, coupon.Amount ?? 0, CourseEnums.CouponType.PERCENT);
                        result.IsFree = result.FinalPrice == 0;
                        result.Message = priceToken.PriceType == BillingEnums.ePricingTypes.SUBSCRIPTION ? String.Format("The price of this {2} is now {1:0.00} for the initial {0} months of your subscription. After that, regular rates shall apply", coupon.SubscriptionMonths ?? 0, result.FinalPrice, objectName) :
                                                            String.Format("Coupon code is valid for a discount of {0}%. Your price is: {1} {2:0.00}", coupon.Amount,priceToken.Currency.ISO, result.FinalPrice);
                        break;
                    case CourseEnums.CouponType.FIXED:
                        if (priceToken.PriceType == BillingEnums.ePricingTypes.SUBSCRIPTION)
                        {
                            result.IsValid = false;
                            result.Message = "Fixed amount coupon can't be applied to subscription";
                        }
                        else
                        {
                            result.Discount   = coupon.Amount ?? 0;
                            result.FinalPrice = CalculateDiscountedPrice(basePrice, coupon.Amount ?? 0, CourseEnums.CouponType.FIXED);
                            result.IsFree     = result.FinalPrice == 0;
                            result.Message = string.Format("Coupon code is valid for a discount of {1} {0:0.00}, Your price is: {1} {2:0.00}", result.Discount, priceToken.Currency.ISO, result.FinalPrice);
                        }

                        break;
                    case CourseEnums.CouponType.FREE:
                        result.Discount   = basePrice;
                        result.FinalPrice = 0;
                        result.IsFree     = true;
                        result.Message = "This coupon entitles you to get this course for free. Click the button below to complete the process.";
                        break;
                }
            }

            return result;
        }

        private static decimal CalculateDiscountedPrice(decimal price, decimal couponDiscountAmount, CourseEnums.CouponType type)
        {
            try
            {
                var finalPrice = price;

                switch (type)
                {
                    case CourseEnums.CouponType.PERCENT:
                        finalPrice = price - (price * (couponDiscountAmount / 100));
                        break;
                    case CourseEnums.CouponType.FIXED:
                        finalPrice = Math.Max(price - couponDiscountAmount, 0);
                        break;
                    case CourseEnums.CouponType.FREE:
                        return 0;
                }

                return decimal.Round(finalPrice, 2, MidpointRounding.AwayFromZero);
            }
            catch (Exception)
            {
                return price;
            }
        }
        #endregion

    }
}
