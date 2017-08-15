using LFE.Core.Enums;
using LFE.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace LFE.DataTokens
{
    public interface IWizardStep
    {
        int CourseId { get; set; }
        Guid Uid { get; set; }
        String Title { get; set; }
        bool IsValid { get; set; }
        String Message { get; set; }
        StepDataToken Data { get; set; }
        IWizardStep LoadStep();
    }
    
    #region wizard manage
    public class BreadcrumbStepDTO
    {
        public string StepTitle { get; set; }
        public string SpanCssClass { get; set; }        
        public CourseEnums.eWizardSteps Step { get; set; }
        public CourseEnums.eWizardSetpModes Mode { get; set; }     
    }
    
    public class WizardManageDTO : CourseBaseDTO
    {
        public CourseEnums.eWizardSteps LastCompletedStep { get; set; }
        public CourseEnums.eWizardSteps CurrentWizardStep { get; set; }
        public CourseEnums.eWizardSteps? BackWizardStep { get; set; }
        public CourseEnums.eWizardSteps NextWizardStep { get; set; }

        public string StepTitle { get; set; }
        public string StepTooltip { get; set; }

        public bool IsSaveAndNext{ get; set; }
        public bool IsNextAllowed { get; set; }
        public string NextButtonTitle { get; set; }

        public string ErrorMessage { get; set; }
        public bool IsValid { get; set; }
        public bool CheckVideoState { get; set; }

        public bool IsPublishAllowed { get; set; }
    }

    public class CourseWizardDto : WizardManageDTO
    {
        public CourseWizardDto()
        {
            Uid          = Guid.Empty;
            IsValid      = false;
            LoadFromHash = false;
        }
        
        public CourseWizardDto(Guid uid,int userId)
        {
            Uid               = uid;
            AuthorId          = userId;
            IsValid           = true;
            CourseId          = -1;
            LastCompletedStep = CourseEnums.eWizardSteps.Introduction;           
            CurrentWizardStep = CourseEnums.eWizardSteps.CourseName;
            NextButtonTitle   = "save & next";
            LoadFromHash      = false;
        }

        public int ChapersCount { get; set; }

        public int VideosCount { get; set; }

        public bool IsAnyContentsCreated { get; set; }

        public bool LoadFromHash { get; set; }

        public IWizardStep CurrentStep { get; set; }
    }  
    #endregion

    #region steps

    public class StepDataToken
    {
        public StepDataToken()
        {
            CourseChapters = new List<BaseListDTO>();
        }
        public CourseEditDTO Course { get; set; }
        public AccountSettingsDTO User { get; set; }
        public IEnumerable<BaseListDTO> CourseChapters { get; set; }
        public int ChapersCount { get; set; }
        public bool AnyContents { get; set; }
    }

    //introduction
    public class WizardIntroDTO : IWizardStep
    {
        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return new WizardIntroDTO
            {
                 Uid      = Uid
                ,CourseId = CourseId
            };
        }
        #endregion
    }

    //course info
    public class WizardCourseNameDTO : IWizardStep
    {
        public WizardCourseNameDTO()
        {
            DisplayOtherLearnersTab = true;
        }

        [Required]
        [DisplayName("Course Name")]
        [RegularExpression(Constants.REGEX_NAME_FORMAT, ErrorMessage = Constants.REGEX_NAME_FORMAT_ERROR)]
        public string CourseName { get; set; }

       
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("CourseDescription")]
        [AllowHtml]
        public string CourseDescription { get; set; }

        [Display(Name = "Display Other Learners Tab")]
        public bool DisplayOtherLearnersTab { get; set; }

        [DisplayName("Course Categories (Multi-select)")]
        public List<int> Categories { get; set; }

        public IEnumerable<CategoryViewDTO> CategoriesData { get; set; }

        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return Data.Course.CourseEditDto2CourseNameDto(); 
        }
        #endregion
    }

    public class WizardCourseVisualsDTO : IWizardStep
    {
        [Required]
        [DisplayName("Course Video Promo")]
        public long? PromoVideoIdentifier { get; set; }

        [Required]
        [DisplayName("Course Thumbnail")]
        public string ThumbName { get; set; }

        public string ThumbUrl { get; set; }

        public UserVideoDto PromoVideo { get; set; }

        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }        
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return Data.Course.CourseEditDto2CourseVisualsDto();
        }
        #endregion
    }

    public class WizardCourseMetaDTO : IWizardStep
    {
        [DisplayName("Course Meta-data (Free text)")]
        public string MetaTags { get; set; }

       

        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }        
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return Data.Course.CourseEditDto2CourseMetaDto();
        }
        #endregion
    }

    public class WizardAboutAuthorDTO : IWizardStep
    {
        public int UserId { get; set; }

        [DisplayName("About You (Up to 200 words)")]
        [AllowHtml]
        public string BioHtml { get; set; }

        public bool IsSocialLogin { get; set; }
        public long? FbUid { get; set; }
        public FbUser FbUser { get; set; }

        public string PictureURL { get; set; }
        public string PictureName { get; set; }

        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return Data.User.AccountSettingsDto2WizardAboutAuthorDto();
        }
        #endregion
    }

    //course contents
    public class WizardVideoManageDTO : IWizardStep
    {
        public UserViewDto UserViewDto { get; set; }

        [AllowHtml]
        public string Info { get; set; }

        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return new WizardVideoManageDTO
            {
                UserViewDto = Data.User.AccountSettingsDto2UserVideoDto()
                ,Uid        = Uid
                ,CourseId   = CourseId
            };
        }
        #endregion
    }

    //public class WizardChapterManageDTO : IWizardStep
    //{
    //    public BaseEntityDTO CourseEntity { get; set; }
        
    //    public string StepTooltip { get; set; }

    //    #region wizard step implementation
    //    public int CourseId { get; set; }
    //    public Guid Uid { get; set; }
    //    public string Title { get; set; }
    //    public bool IsValid { get; set; }
    //    public string Message { get; set; }
    //    public StepDataToken Data { get; set; }
    //    public IWizardStep LoadStep()
    //    {
    //        return new WizardChapterManageDTO
    //        {
    //            CourseEntity = Data.Course != null ? Data.Course.CourseEditDto2BaseEntityDto() : new BaseEntityDTO{id=-1}
    //            ,Uid      = Uid
    //            ,CourseId = CourseId
    //        };
    //    }
    //    #endregion
    //}

    public class WizardChapterContentManageDTO : IWizardStep
    {
        public IEnumerable<BaseListDTO> Chapters { get; set; }
        public int SelectedChapterId { get; set; }

        #region wizard step implementation
        public int CourseId { get; set; }
        public int TotalQuizzes { get; set; }
        public string CourseName { get; set; }

        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return new WizardChapterContentManageDTO
                                                {
                                                    IsValid           = true
                                                    ,CourseId          = Data.Course.CourseId
                                                    ,CourseName        = Data.Course.CourseName
                                                };
                
                                                //Data.CourseChapters.Any() ? 
                                                //new WizardChapterContentManageDTO
                                                //{
                                                //    Chapters           = Data.CourseChapters.OrderBy(x => x.index).ToArray()
                                                //    ,SelectedChapterId = Data.CourseChapters.OrderBy(x => x.index).ToArray()[0].id
                                                //    ,IsValid           = true
                                                //    ,CourseId          = Data.Course.CourseId
                                                //    ,CourseName        = Data.Course.CourseName
                                                //}
                                                //: new WizardChapterContentManageDTO
                                                //{
                                                //    Chapters           = new List<BaseListDTO>()
                                                //    ,SelectedChapterId = -1
                                                //    ,Message           = "Please, create chapters"
                                                //    ,IsValid           = false
                                                //    ,CourseId          = Data.Course.CourseId
                                                //    ,CourseName        = Data.Course.CourseName
                                                //};

        }
        #endregion
    }

    //settings
    public class WizardCoursePricingDTO : PriceBaseDTO, IWizardStep
    {
       
        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            return Data.Course.CourseEditDto2WizardCoursePricingDto();
        }
        #endregion
    }

    public class WizardCoursePublishDTO : IWizardStep
    {
        [DisplayName("Status")]
        public CourseEnums.CourseStatus Status { get; set; }

        public List<PublishChecklistToken> Checklist { get; set; }

        public string CourseName { get; set; }

        public bool Ready2Publish { get; set; }

        public bool IsCoursePurchased { get; set; }

        #region wizard step implementation
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public StepDataToken Data { get; set; }
        public IWizardStep LoadStep()
        {
            var ready = IsPublishAllowed(Data);

            var token = Data.Course.CourseEditDto2WizardCoursePublishDto(ready,Checklist);

            return token;
        }
        #endregion

        private enum eStatusCssNames
        {
            pass,
            req,
            warn
        }

        
        private bool IsPublishAllowed(StepDataToken data)
        {
            var isReady2Publish = true;
           

            var checklist = new List<PublishChecklistToken>
            {
                new PublishChecklistToken
                {
                     Kind     = CourseEnums.ePublichChecklist.Course
                    ,Pass     = true
                    ,Name     = Utils.GetEnumDescription(CourseEnums.ePublichChecklist.Course)
                    ,CssClass = eStatusCssNames.pass.ToString()
                }
            };

           

            //1. check chapters
            if (data.ChapersCount.Equals(0))
            {
                isReady2Publish = false;
            }

            checklist.Add(new PublishChecklistToken
                {
                     Kind     = CourseEnums.ePublichChecklist.Chapters
                    ,Pass     = data.ChapersCount > 0
                    ,Name     = Utils.GetEnumDescription(CourseEnums.ePublichChecklist.Chapters)
                    ,CssClass = data.ChapersCount > 0 ? eStatusCssNames.pass.ToString() : eStatusCssNames.req.ToString()
                });

            //2. check videos
            if (!data.AnyContents)
            {
                isReady2Publish = false;
            }

            checklist.Add(new PublishChecklistToken
                {
                     Kind     = CourseEnums.ePublichChecklist.AnyContents
                    ,Pass     = data.AnyContents
                    ,Name     = Utils.GetEnumDescription(CourseEnums.ePublichChecklist.AnyContents)
                    ,CssClass = data.AnyContents ? eStatusCssNames.pass.ToString() : eStatusCssNames.req.ToString()
                });

            //3. check visuals
            if (String.IsNullOrEmpty(data.Course.ThumbUrl))
            {
                isReady2Publish = false;
            }

            checklist.Add(new PublishChecklistToken
                {
                     Kind     = CourseEnums.ePublichChecklist.CourseThumb
                    ,Pass     = !String.IsNullOrEmpty(data.Course.ThumbUrl)
                    ,Name     = Utils.GetEnumDescription(CourseEnums.ePublichChecklist.CourseThumb)
                    ,CssClass = !String.IsNullOrEmpty(data.Course.ThumbUrl) ? eStatusCssNames.pass.ToString() : eStatusCssNames.req.ToString()
                });

            if (data.Course.PromoVideoIdentifier == null)
            {
                isReady2Publish = false;
            }

            checklist.Add(new PublishChecklistToken
                {
                     Kind     = CourseEnums.ePublichChecklist.CoursePromoVideo
                    ,Pass     = data.Course.PromoVideoIdentifier != null
                    ,Name     = Utils.GetEnumDescription(CourseEnums.ePublichChecklist.CoursePromoVideo)
                    ,CssClass = data.Course.PromoVideoIdentifier != null ? eStatusCssNames.pass.ToString() : eStatusCssNames.req.ToString()
                });

            //4. check price
            var isFree = data.Course.IsFree;
            var isPriceValid = isFree || data.Course.PriceLines.Any();
            var price = data.Course.PriceDisplayName;


            //if (isFree)
            //{
            //    isPriceValid = true;
            //    price = " Free course";
            //}
            //else 
            //{
            //    if (!data.Course.PriceLines.Any())
            //    {
            //        isReady2Publish = false;
            //        isPriceValid = false;
            //        price = " not defined   ";    
            //    }
            //    else
            //    {
            //        isPriceValid = true;
            //        // String.Format(" ${0:0.00}", data.Course.PriceLines[0].Price);
            //            //Price != null ? String.Format(" ${0:0.00}", data.Course.Price) : String.Format(" ${0:0.00} for month", data.Course.MonthlySubscriptionPrice);
            //    }
            //}
            
            checklist.Add(new PublishChecklistToken
            {
                Kind      = CourseEnums.ePublichChecklist.Price
                ,Pass     = isPriceValid
                ,Name     = String.Format("{0} {1}",Utils.GetEnumDescription(CourseEnums.ePublichChecklist.Price) , price) //(data.Course.Price > 0 ? "" : ": $0.00")
                ,CssClass = isPriceValid ? eStatusCssNames.pass.ToString() : eStatusCssNames.warn.ToString()                    
            });

            Checklist = checklist;

            return isReady2Publish;
        }
    } 
    #endregion

    #region helpers

    public class WizardStepTooltipDTO
    {
        public byte StepId { get; set; }
        public string Name { get; set; }

        [AllowHtml]
        public string TooltipHtml { get; set; }
    }

    public class PublishChecklistToken
    {
        public bool Pass { get; set; }
        public bool PassButWarning { get; set; }
        public CourseEnums.ePublichChecklist Kind { get; set; }
        public string Name { get; set; }
        public string CssClass { get; set; }
    }
    #endregion
}
