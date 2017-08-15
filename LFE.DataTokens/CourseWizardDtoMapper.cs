using System;
using System.Collections.Generic;
using LFE.Core.Utils;
using LFE.Model;

namespace LFE.DataTokens
{
    public static class CourseWizardDtoMapper
    {
        public static UserViewDto AccountSettingsDto2UserVideoDto(this AccountSettingsDTO token)
        {
            return new UserViewDto
            {
                userId    = token.UserId
                ,nickname = token.Nickname
                ,fullName = token.FirstName + " " + token.LastName
            };
        }

        public static BaseEntityDTO CourseEditDto2BaseEntityDto(this CourseEditDTO token)
        {
            return new BaseEntityDTO
            {
                id    = token.CourseId
                ,name = token.CourseName                
            };
        }

        public static WizardCourseVisualsDTO CourseEditDto2CourseVisualsDto(this CourseEditDTO token)
        {
           return new WizardCourseVisualsDTO
                {
                    CourseId              = token.CourseId
                    ,Uid                  = token.Uid
                    ,ThumbName            = token.ThumbName
                    ,ThumbUrl             = token.ThumbUrl
                    ,PromoVideoIdentifier = token.PromoVideoIdentifier
                };                    
        }

        public static WizardCourseNameDTO CourseEditDto2CourseNameDto(this CourseEditDTO token)
        {
            return new WizardCourseNameDTO
            {
                CourseId                 = token.CourseId
                ,Uid                     = token.Uid
                ,CourseName              = String.IsNullOrEmpty(token.CourseName) || token.CourseName == token.Uid.ToString() ? string.Empty : token.CourseName
                ,Description             = token.Description
                ,CourseDescription       = token.CourseDescription
                ,DisplayOtherLearnersTab = token.DisplayOtherLearnersTab
                ,Categories              = token.Categories
            };
        }

        public static WizardCourseMetaDTO CourseEditDto2CourseMetaDto(this CourseEditDTO token)
        {
            return new WizardCourseMetaDTO
            {
                CourseId     = token.CourseId
                ,Uid         = token.Uid
                ,MetaTags    = token.MetaTags
            };
        }

        public static WizardCoursePricingDTO CourseEditDto2WizardCoursePricingDto(this CourseEditDTO token)
        {
            return new WizardCoursePricingDTO
            {
                CourseId                    = token.CourseId
                ,Uid                        = token.Uid
                ,IsFree                     = token.IsFree
               // ,Price                      = token.Price.ItemPrice2DisplayPrice() 
               // ,MonthlySubscriptionPrice   = token.MonthlySubscriptionPrice.ItemPrice2DisplayPrice()
                ,AffiliateCommission        = token.AffiliateCommission
            };
        }

        public static WizardCoursePublishDTO CourseEditDto2WizardCoursePublishDto(this CourseEditDTO token,bool ready,List<PublishChecklistToken> list)
        {
            return new WizardCoursePublishDTO
            {
                CourseId           = token.CourseId
                ,CourseName        = token.CourseName
                ,Uid               = token.Uid
                ,Status            = token.Status
                ,Ready2Publish     = ready
                ,IsCoursePurchased = token.IsCoursePurchased
                ,Checklist         = list
            };
        }

        public static WizardAboutAuthorDTO AccountSettingsDto2WizardAboutAuthorDto(this AccountSettingsDTO token)
        {
            return new WizardAboutAuthorDTO
            {
                UserId         = token.UserId
                ,BioHtml       = token.BioHtml
                ,PictureName   = token.PictureName
                ,PictureURL    = token.PictureURL
                ,FbUid         = token.FbUid
                ,IsSocialLogin = token.IsSocialLogin
                ,FbUser        = token.FbUser
                
            };
        }

        public static WizardStepTooltipDTO Entity2StepTooltipDto(this CRS_WizardStepsLOV entity)
        {
            return new WizardStepTooltipDTO
            {
                StepId       = entity.StepId
                ,Name        = entity.StepName
                ,TooltipHtml = entity.TooltipHTML
            };
        }
    }
}
