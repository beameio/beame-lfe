using System;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class MessageDtoMapper
    {
        private static readonly string AppId = Utils.GetKeyValue("fbAppId");

        #region FB titles        
        private static readonly string FB_REVIEW_SUB_TITLE     = $"{Constants.APP_OFFICIAL_NAME} - Learning from Experience - is the place for experts of all sorts to create courses and make their knowledge work for them.";
        private const string FB_REVIEW_AUTHOR_TITLE  = "{0} has posted a Review on your course, {1}: {2}";
        private static readonly string FB_REVIEW_LEARNER_TITLE = "{0} has posted a Review on an " + Constants.APP_OFFICIAL_NAME + " course by {3}, {1}: {2}";
        private static readonly string FB_REVIEW_APP_TITLE = "{0} has posted a Review on an " + Constants.APP_OFFICIAL_NAME + " course by {3}, {1}: {2}";
        #endregion

        #region purchase consts
        private static readonly string LEARNER_PURCHASE_FACEBOOK_MESSAGE = "{0} has just purchased an " + Constants.APP_OFFICIAL_NAME + " course by {1}, \"{2}\". " + Constants.APP_OFFICIAL_NAME + " is the place for experts of all sorts to create online courses and make their knowledge work for them.";
        private static readonly string AUTHOR_PURCHASE_FACEBOOK_MESSAGE  = "{0} has just purchased your " + Constants.APP_OFFICIAL_NAME + " course, \"{1}\". " + Constants.APP_OFFICIAL_NAME + " is the place for experts of all sorts to create online courses and make their knowledge work for them.";
        private static readonly string LFE_PURCHASE_FACEBOOK_MESSAGE     = "{0} has just purchased an " + Constants.APP_OFFICIAL_NAME + "course by {1}, \"{2}\". " + Constants.APP_OFFICIAL_NAME + " is the place for experts of all sorts to create online courses and make their knowledge work for them.";
        #endregion

        #region registration consts
        private const string USER_REGISTRATION_FACEBOOK_MESSAGE = "{0} has signed up to LFE.COM - Learning from Experience. LFE.com is the place for experts of all sorts to create courses and make their knowledge work for them.";
        #endregion

        #region stories
        public static PostMessageDTO CourseDto2ReviewStoryDto(this CourseFbToken course,Users storyOwner)
        {
            return new PostMessageDTO
                    {
                        UserId       = storyOwner.Id
                        ,UserFbId    = Convert.ToInt64(storyOwner.FacebookID)
                        ,MessageText = "Wrote review on " + course.Name + "\r\n" + course.CoursePageUrl
                        ,Action      = FbEnums.eFbActions.review 
                        ,CourseId    = course.CourseId
                    };
        }

        public static PostMessageDTO CourseDto2CommentStoryDto(this CourseFbToken course, Users storyOwner)
        {
            return new PostMessageDTO
                    {
                        UserId       = storyOwner.Id
                        ,UserFbId    = Convert.ToInt64(storyOwner.FacebookID)
                        ,MessageText = "Wrote comment on " + course.Name + "\r\n" + course.CoursePageUrl
                        ,Action      = FbEnums.eFbActions.comment
                        ,CourseId    = course.CourseId
                    };
        }

        public static PostMessageDTO CourseDto2WatchStoryDto(this CourseFbToken course, Users storyOwner, string videoName, int? chapterVideoID)
        {
            return new PostMessageDTO
                    {
                        UserId       = storyOwner.Id
                        ,UserFbId    = Convert.ToInt64(storyOwner.FacebookID)
                        ,MessageText = "Just view " + videoName + " on " + course.Name + "\r\n" + course.CoursePageUrl
                        //,Action      = FbEnums.eFbActions.watch_chapter_video
                        ,Action       = FbEnums.eFbActions.view
                        ,CourseId    = course.CourseId
                        ,ChapterVideoID = chapterVideoID
                    };
        }

        public static PostMessageDTO CourseDto2PublishStoryDto(this CourseFbToken course, Users storyOwner)
        {
            return new PostMessageDTO
                    {
                        UserId       = storyOwner.Id
                        ,UserFbId    = Convert.ToInt64(storyOwner.FacebookID)
                        ,MessageText = "Published " + course.Name + "\r\n" + course.CoursePageUrl
                        ,Action      = FbEnums.eFbActions.publish_course
                        ,CourseId    = course.CourseId
                    };
        }

        public static PostMessageDTO CourseDto2PurchaseStoryDto(this CourseFbToken course, Users storyOwner)
        {
            return new PostMessageDTO
                    {
                        UserId       = storyOwner.Id
                        ,UserFbId    = Convert.ToInt64(storyOwner.FacebookID)
                        ,MessageText = "Purchased " + course.Name + "\r\n" + course.CoursePageUrl
                        ,Action      = FbEnums.eFbActions.purchase_course
                        ,CourseId    = course.CourseId
                    };
        }
        #endregion

        //private static string CourseUrlName2PageUrl(this string courseUrlName)
        //{
        //    return String.Format(DtoExtensions.COURSE_PAGE_URL, Utils.GetKeyValue("baseUrl"), courseUrlName);
        //}

        public static PostMessageDTO Entity2PostMessageDto(this DSC_NotificationsFbToken entity)
        {
            long fbUid;

            var parsed = Int64.TryParse(entity.FacebookID, out fbUid);

            if (!parsed) return null;

            return new PostMessageDTO
            {
                 MessageId    = entity.MessageId
                ,UserId       = entity.UserId
                ,UserFbId     = fbUid
                ,MessageText  = entity.MessageText + " \r\n " + String.Format(DtoExtensions.MESSAGE_PAGE_URL, Utils.GetKeyValue("baseUrl"), entity.Uid)
                ,ImageUrl     = String.IsNullOrEmpty(entity.CourseThumbUrl) ? string.Empty : Constants.ImageBaseUrl + entity.CourseThumbUrl
                ,Description  = entity.CourseDescription
                ,Caption      = entity.Entity2AuthorFullName()
                ,MessageUrl   = entity.GenerateCourseFullPageUrl(entity.Entity2AuthorFullName(), entity.CourseName,null)
                ,MessageTitle = entity.CourseName
            };
        }

        public static PostMessageDTO Token2AuthorPostMessageDto(this ReviewMessageDTO token)
        {
            if (token.Author.fbUid == null || token.Author.fbUid < 0) return null;

            return new PostMessageDTO
            {
                UserId        = token.Author.id
                ,UserFbId     = token.Author.fbUid
                ,MessageText  = String.Format(FB_REVIEW_AUTHOR_TITLE, token.Writer.name, token.Item.name, token.ReviewText) + " \r\n " + FB_REVIEW_SUB_TITLE
                ,ImageUrl     = token.Item.thumbUrl
                ,Description  = token.Item.desc
                ,Caption      = token.Author.name
                ,MessageUrl   = token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name,null)
                ,MessageTitle = token.Item.name
            };
        }

        public static PostMessageDTO Token2AppPostMessageDto(this ReviewMessageDTO token)
        {
            return new PostMessageDTO
            {
                MessageText = String.Format(FB_REVIEW_APP_TITLE, token.Writer.name, token.Item.name, token.ReviewText, token.Author.name) + " \r\n " + token.Item.itemUrlName//token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name,null)
                ,ImageUrl      = token.Item.thumbUrl
                ,Description   = token.Item.desc
                ,Caption       = token.Author.name
                ,MessageUrl    = token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name,null)
                ,MessageTitle  = token.Item.name
                ,IsAppPagePost = true
            };
        }

        public static PostMessageDTO Token2LearnerPostMessageDto(this ReviewMessageDTO token)
        {
            if (token.Learner.fbUid == null || token.Learner.fbUid < 0) return null;

            return new PostMessageDTO
            {
                UserId        = token.Learner.id
                ,UserFbId     = token.Learner.fbUid
                ,MessageText  = String.Format(FB_REVIEW_LEARNER_TITLE, token.Writer.name, token.Item.name, token.ReviewText, token.Author.name) + " \r\n " + FB_REVIEW_SUB_TITLE
                ,ImageUrl     = token.Item.thumbUrl
                ,Description  = token.Item.desc
                ,Caption      = token.Author.name
                ,MessageUrl   = token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name,null)
                ,MessageTitle = token.Item.name
            };
        }

        public static PostMessageDTO Token2LfePostMessageDto(this ReviewMessageDTO token)
        {
            return new PostMessageDTO
            {
                UserFbId      = Convert.ToInt64(AppId)
                ,MessageText  = String.Format(FB_REVIEW_LEARNER_TITLE, token.Writer.name, token.Item.name, token.ReviewText, token.Author.name) + " \r\n " + FB_REVIEW_SUB_TITLE
                ,ImageUrl     = token.Item.thumbUrl
                ,Description  = token.Item.desc
                ,Caption      = token.Author.name
                ,MessageUrl   = token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name, null)
                ,MessageTitle = token.Item.name
            };
        }


        public static ReviewMessageDTO AuthorDto2ReviewMessageDto(this CRS_ReviewAuthorMessageToken token)
        {
            if (token == null) return null;

            return new ReviewMessageDTO
            {
                AddOn       = token.ReviewDate ?? DateTime.Now
                ,ReviewText = token.ReviewText
                ,Item       = new ItemMessageDTO
                               {
                                   id             = token.CourseId
                                   ,name          = token.CourseName
                                   ,itemUrlName   = token.CourseUrlName
                                   ,desc          = token.CourseDescription
                                   ,thumbUrl      = String.IsNullOrEmpty(token.CourseThumbUrl) ? string.Empty : Constants.ImageBaseUrl + token.CourseThumbUrl
                               }
                ,Author = new MessageUserDTO
                               {
                                   id     = token.AuthorUserId
                                   ,email = token.AuthorEmail
                                   ,fbUid = !String.IsNullOrEmpty(token.AuthorFacebookID) ? Int64.Parse(token.AuthorFacebookID) : (long?)null
                                   ,name  = token.Entity2AuthorFullName()
                               }
                ,Writer = new MessageUserDTO
                               {
                                   id    = token.ReviewWriterId
                                   ,name = token.Entity2WriterFullName()
                               }
            };
        }

        public static ReviewMessageDTO LearnerDto2ReviewMessageDto(this CRS_ReviewLearnerMessageToken token)
        {
            if (token == null) return null;

            return new ReviewMessageDTO
            {
                AddOn       = token.ReviewDate ?? DateTime.Now
                ,ReviewText = token.ReviewText
                ,Item       = new ItemMessageDTO
                               {
                                   id             = token.CourseId
                                   ,name          = token.CourseName
                                   ,itemUrlName   = token.CourseUrlName
                                   ,desc          = token.CourseDescription
                                   ,thumbUrl      = String.IsNullOrEmpty(token.CourseThumbUrl) ? string.Empty : Constants.ImageBaseUrl + token.CourseThumbUrl
                               }
                ,Author = new MessageUserDTO
                               {
                                   id     = token.AuthorUserId
                                   ,fbUid = !String.IsNullOrEmpty(token.AuthorFacebookID) ? Int64.Parse(token.AuthorFacebookID) : (long?)null
                                   ,name  = token.Entity2AuthorFullName()
                               }
                ,Writer = new MessageUserDTO
                               {
                                   id    = token.ReviewWriterId
                                   ,name = token.Entity2WriterFullName()
                               }
                ,Learner = new MessageUserDTO
                               {
                                   id     = token.LearnerUserId
                                   ,fbUid = !String.IsNullOrEmpty(token.LearnerFacebookID) ? Int64.Parse(token.LearnerFacebookID) : (long?)null
                                   ,name  = token.Entity2LearnerFullName()
                                   ,email = token.LearnerEmail
                               }
            };
        }

        #region purchase

        public static PostMessageDTO LearnerPurchase2MessageDTO(this PurchaseMessageDTO token)
        {
            if (token.Buyer.fbUid == null || token.Buyer.fbUid < 0) return null;

            return new PostMessageDTO
            {
                UserId        = token.Buyer.id
                ,UserFbId     = token.Buyer.fbUid
                ,MessageText  = String.Format(LEARNER_PURCHASE_FACEBOOK_MESSAGE, token.Buyer.name, token.Author.name, token.Item.name) + " \r\n " + FB_REVIEW_SUB_TITLE
                ,ImageUrl     = String.IsNullOrEmpty(token.Item.thumbUrl) ? string.Empty : Constants.ImageBaseUrl + token.Item.thumbUrl
                ,Description  = token.Item.desc
                ,Caption      = token.Author.name
                ,MessageUrl   = token.Item.itemUrlName //token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name,null)
                ,MessageTitle = token.Item.name
            };
        }

        public static PostMessageDTO AuthorPurchaser2MessageDto(this PurchaseMessageDTO token)
        {
            if (token.Author.fbUid == null || token.Author.fbUid < 0) return null;

            return new PostMessageDTO
            {
                UserId        = token.Author.id
                ,UserFbId     = token.Author.fbUid
                ,MessageText  = String.Format(AUTHOR_PURCHASE_FACEBOOK_MESSAGE, token.Buyer.name, token.Item.name) + " \r\n " + FB_REVIEW_SUB_TITLE
                ,ImageUrl     = String.IsNullOrEmpty(token.Item.thumbUrl) ? string.Empty : Constants.ImageBaseUrl + token.Item.thumbUrl
                ,Description  = token.Item.desc
                ,Caption      = token.Author.name
                ,MessageUrl   = token.Item.itemUrlName //token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name,null)
                ,MessageTitle = token.Item.name
            };
        }

        public static PostMessageDTO LfePurchase2MessageDto(this PurchaseMessageDTO token)
        {
            return new PostMessageDTO
            {
                UserId         = token.Buyer.id,
                UserFbId       = Convert.ToInt64(AppId),
                MessageText    = String.Format(LFE_PURCHASE_FACEBOOK_MESSAGE, token.Buyer.name, token.Author.name, token.Item.name) + " \r\n " + token.Item.itemUrlName,//token.GenerateCourseFullPageUrl(token.Author.name, token.Item.name, null),
                ImageUrl       = String.IsNullOrEmpty(token.Item.thumbUrl) ? string.Empty : Constants.ImageBaseUrl + token.Item.thumbUrl,
                Description    = token.Item.desc,
                Caption        = token.Author.name,
                MessageUrl     = token.Item.itemUrlName, //token.GenerateFullCoursePageUrl(token.Author.name, token.Course.name),
                MessageTitle   = token.Item.name,
                IsAppPagePost  = true
            };
        }

        #endregion

        #region registration
        public static PostMessageDTO UserRegistration2MessageDTO(this ReviewMessageDTO token)
        {
            if (token.Learner.fbUid == null || token.Learner.fbUid < 0) return null;

            return new PostMessageDTO
            {
                UserId        = token.Learner.id
                ,UserFbId     = token.Learner.fbUid
                ,MessageText  = String.Format(USER_REGISTRATION_FACEBOOK_MESSAGE, token.Learner.name) + " \r\n "
                ,ImageUrl     = ""
                ,Description  = FB_REVIEW_SUB_TITLE
                ,Caption      = "courses.beame.io"
                ,MessageUrl   = Utils.GetKeyValue("baseUrl")
                ,MessageTitle = "Registration to " + Constants.APP_OFFICIAL_NAME
            };
        }

        #endregion
    }
}
