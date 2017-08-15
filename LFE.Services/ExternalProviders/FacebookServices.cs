using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Facebook;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;

namespace LFE.Application.Services.ExternalProviders
{
    public class FacebookServices : ServiceBase, IFacebookServices, IFacebookTabAppServices
    {

        #region IFacebookServices implementation
        #region parameters
        private readonly FacebookClient _fbClient;

        private const string COURSE_OBJECT_TYPE            = "learnfromexpirence:lfe_course";        
        private new static readonly string FB_APP_ID       = Utils.GetKeyValue("fbAppId");
        private new static readonly string FB_APP_SECRET   = Utils.GetKeyValue("fbAppSecret");
        private static readonly string FB_APP_FROM_NAME    = Utils.GetKeyValue("fbFromName");
        private static readonly string FB_APP_ACCESS_TOKEN = Utils.GetKeyValue("fbAppAccessToken");
        private static readonly string FB_APP_PAGE_ID      = Utils.GetKeyValue("fbPageId");
        private const string FB_APP_AT = "********************************************"; //access token
        #endregion

        #region  .ctor
        public FacebookServices()
        {
            _fbClient = new FacebookClient
            {
                AppId        = FB_APP_ID
                ,AppSecret   = FB_APP_SECRET
                ,AccessToken = FB_APP_AT
            };
        }
        #endregion

        #region private helpers
        public string GetAppAccessToken()
        {
            try
            {
                dynamic result = _fbClient.Get("oauth/access_token", new
                                                                        {
                                                                            client_id = FB_APP_ID,
                                                                            client_secret = FB_APP_SECRET,
                                                                            grant_type = "client_credentials"
                                                                        });

                //_fbClient.AccessToken = result.access_token;

                return result.access_token;
            }
            catch (Exception ex)
            {
                Logger.Error("set fb access token", ex, null, CommonEnums.LoggerObjectTypes.FB);
                return string.Empty;
            }
        }

        private void UpdateNotificationFbPostState(long notificationId)
        {
            try
            {
                var entity = UserNotificationRepository.GetById(notificationId);

                if (entity == null) return;

                //  entity.FbPostId = postId;
                entity.FbPostSendOn = DateTime.Now;

                UserNotificationRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Update notification FB post state", notificationId, ex, CommonEnums.LoggerObjectTypes.UserNotification);
            }
        }

        #region course object
        public long GetCourseFbObjectId(int courseId)
        {
            try
            {
                var entity = CourseRepository.GetById(courseId);

                if (entity == null) return -1;

                if (entity.FbObjectId != null) return (long)entity.FbObjectId;

                var coursefbToken = CourseRepository.GetCourseToken(DEFAULT_CURRENCY_ID,courseId).CourseEntity2FbToken();

                if (coursefbToken == null) return -1;

                string error;
                long oid;

                var created = CreateCourseObject(coursefbToken, out oid, out error);

                if (!created || oid <= 0) return -1;

                entity.FbObjectId = oid;
                entity.LastModified = DateTime.Now;

                CourseRepository.UnitOfWork.CommitAndRefreshChanges();

                return oid;
            }
            catch (Exception ex)
            {
                Logger.Error("create course fb objects", courseId, ex, CommonEnums.LoggerObjectTypes.FB);
                return -1;
            }

        }

        private bool CreateCourseObject(CourseFbToken token, out long objectId, out string error)
        {
            error = string.Empty;
            objectId = -1;
            try
            {
                #region create object for application
                //  var at = SetAccessToken();

                var parameters = new Dictionary<string, object>
                {
                    {"access_token", _fbClient.AccessToken},
                    {"privacy", "{'value':'EVERYONE'}"},
                    {"object","{\"title\":\"" + token.Name + "\",\"image\":\"" + token.ImageUrl + "\",\"url\":\"" + token.CoursePageUrl + "\",\"description\":\"" + HttpUtility.HtmlEncode(token.Description) + "\"}"} //,\"data\":{\"subscribers\":1}
                };

                dynamic oid = _fbClient.Post(FB_APP_ID + "/objects/" + COURSE_OBJECT_TYPE, parameters);

                Int64.TryParse(oid.id, out objectId);
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("create bf course object", ex, CommonEnums.LoggerObjectTypes.FB);
                return false;
            }
        }
        #endregion

        #region send posts
        private bool SendPostMessage(FB_PostInterface entity, string user_access_token, out string postId, out string error)
        {
            error = string.Empty;
            postId = string.Empty;
            try
            {

                dynamic messagePost = new ExpandoObject();

                messagePost.link = entity.LinkedName;
                messagePost.name = entity.Title;
                messagePost.message = entity.Message;

                if (!String.IsNullOrEmpty(entity.Caption)) messagePost.caption = entity.Caption;
                if (!String.IsNullOrEmpty(entity.Description)) messagePost.description = entity.Description;
                if (!string.IsNullOrEmpty(entity.ImageUrl)) messagePost.picture = entity.ImageUrl;

                // Logger.Debug("post app message with at::" + FB_APP_ACCESS_TOKEN + ", appPageId::" + FB_APP_PAGE_ID + ", msg::" + JsSerializer.Serialize(messagePost));

                messagePost.@from = new { id = FB_APP_ID, name = FB_APP_FROM_NAME };

                var fb = new FacebookClient(user_access_token);

                var post = fb.Post(entity.FbUid + "/feed", messagePost);

                postId = post.id;

                return true;

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("post message to FB", entity.PostId, ex, CommonEnums.LoggerObjectTypes.FB);
                return false;
            }
        }

        private bool SendAppPostMessage(FB_PostInterface entity, out string postId, out string error)
        {
            error = string.Empty;
            postId = string.Empty;
            try
            {

                dynamic messagePost = new ExpandoObject();

                messagePost.name = entity.Title;
                messagePost.message = entity.Message;

                if (!String.IsNullOrEmpty(entity.Caption)) messagePost.caption = entity.Caption;
                if (!String.IsNullOrEmpty(entity.Description)) messagePost.description = entity.Description;
                if (!string.IsNullOrEmpty(entity.ImageUrl)) messagePost.picture = entity.ImageUrl;


                // Logger.Debug("post app message with at::" + FB_APP_ACCESS_TOKEN + ", appPageId::" + FB_APP_PAGE_ID + ", msg::" + JsSerializer.Serialize(messagePost));

                var fb = new FacebookClient(FB_APP_ACCESS_TOKEN);

                var post = fb.Post(FB_APP_PAGE_ID + "/feed", messagePost);

                postId = post.id;

                return true;

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("post app message to FB", entity.PostId, ex, CommonEnums.LoggerObjectTypes.FB);
                return false;
            }
        }

        private bool SendStoryMessage(FB_PostInterface entity, string user_access_token, out string postId, out string error)
        {
            dynamic messagePost = new ExpandoObject();
            error = string.Empty;
            postId = string.Empty;
            try
            {
                if (String.IsNullOrEmpty(user_access_token))
                {
                    error = "user_access_token required";
                    return false;
                }

                if (entity.ActionId == null)
                {
                    error = "Action not defined";
                    return false;
                }

                if (entity.CourseId == null)
                {
                    error = "CourseId required";
                    return false;
                }

                var objectId = GetCourseFbObjectId((int)entity.CourseId);

                if (objectId < 0)
                {
                    error = "course objectId not found";
                    return false;
                }

                var action = Utils.ParseEnum<FbEnums.eFbActions>(entity.ActionId.ToString());



                messagePost.lfe_course = objectId;
                messagePost.message = entity.Message;

                var fb = new FacebookClient(user_access_token);

                var post = fb.Post("me/learnfromexpirence:" + action, messagePost);

                postId = post.id;

                return true;

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("post app message to FB", entity.PostId, ex, CommonEnums.LoggerObjectTypes.FB);
                return false;
            }
        }

        private void UpdateRowStatus(FB_PostInterface post, bool postSaved, string postId, string postError)
        {
            if (postSaved)
            {
                post.FbPostId = postId;
                post.Status = FbEnums.ePostInterfaceStatus.Posted.ToString();
                post.PostOn = DateTime.Now;
                post.UpdateOn = DateTime.Now;
            }
            else
            {
                post.Status = FbEnums.ePostInterfaceStatus.Failed.ToString();
                post.Error = postError;
                post.UpdateOn = DateTime.Now;
            }

            FacebookPostRepository.UnitOfWork.CommitAndRefreshChanges();

            if (postSaved && post.NotificationId != null)
            {
                UpdateNotificationFbPostState((long)post.NotificationId);
            }
        }

        private static string GetUserAccessToken(Users userEntity, out string error)
        {
            error = null;

            var user_access_token = userEntity.FbAccessToken;

            if (userEntity.FbAccessTokenExpired == null) return user_access_token;

            if (((DateTime)userEntity.FbAccessTokenExpired).Date < DateTime.Now.Date)
            {
                error = "user_access_token expired";
                user_access_token = string.Empty;
            }

            return user_access_token;
        }
        #endregion
        #endregion

        #region interface implementation

        //public void GetCurrentUser(string accessToken)
        //{

        //    var client = new FacebookClient
        //    {
        //        AppId        = FB_APP_ID
        //        ,AppSecret   = FB_APP_SECRET
        //        ,AccessToken = accessToken
        //    };

        //    dynamic response = client.Get("me", new { fields = "verified,email" });
        //    dynamic facebookVerified;
        //    facebookVerified = response.ContainsKey("verified") && response["verified"];
        //}
        public FbUser GetFbUser(long fbUid)
        {
            try
            {
                dynamic results = _fbClient.Get(fbUid.ToString());

                return new FbUser
                {
                    name = results.name,
                    username = results.username
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SavePostMessage(PostMessageDTO postToken, out string error, long? notificationId = null)
        {
            error = string.Empty;

            try
            {
                var entity = postToken.Dto2FbPostInterfaceEntity(notificationId);

                FacebookPostRepository.Add(entity);

                FacebookPostRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save  FB message", postToken.MessageId ?? -1, ex, CommonEnums.LoggerObjectTypes.FB);
            }
        }

        public void CreateUserFbStory(int userId, int courseId, FbEnums.eFbActions action, int? chapterVideoId = null, string additionalMsg = null)
        {
            try
            {
                string error;

                var storyOwner = UserRepository.GetById(userId);

                if (storyOwner == null)
                {
                    Logger.Warn("Create FB story::userId::" + userId +"::user not found");
                    return;
                }

                //TODO switch to oAuth 
                if (String.IsNullOrEmpty(storyOwner.FacebookID)) return;

                var course = CourseRepository.GetCourseToken(DEFAULT_CURRENCY_ID,courseId).CourseEntity2FbToken();

                if (course == null) return;

                PostMessageDTO storyToken;
                switch (action)
                {
                    case FbEnums.eFbActions.publish_course:
                        storyToken = course.CourseDto2PublishStoryDto(storyOwner);
                        break;
                    case FbEnums.eFbActions.purchase_course:
                        storyToken = course.CourseDto2PurchaseStoryDto(storyOwner);
                        break;
                    case FbEnums.eFbActions.review:
                        storyToken = course.CourseDto2ReviewStoryDto(storyOwner);
                        break;
                    case FbEnums.eFbActions.comment:
                        storyToken = course.CourseDto2CommentStoryDto(storyOwner);
                        break;
                    case FbEnums.eFbActions.view:
                        storyToken = course.CourseDto2WatchStoryDto(storyOwner, additionalMsg ?? "video", chapterVideoId);
                        break;
                    default:
                        return;
                }

                SavePostMessage(storyToken, out error);
            }
            catch (Exception ex)
            {
                Logger.Error("save fb story", ex, userId, CommonEnums.LoggerObjectTypes.UserCourse);
            }
        }

        public void SendWaitingPosts()
        {
            try
            {
                var waiting = FbEnums.ePostInterfaceStatus.Waiting.ToString();

                //get all waiting posts
                var posts = FacebookPostRepository.GetMany(x => x.Status == waiting && x.FbPostId == null).ToList();

                if (posts.Count.Equals(0))
                {
                    return;
                }

                Logger.Debug("FB posts found " + posts.Count + " on " + DateTime.Now);

                string postId;
                string postError;

                foreach (var post in posts.Where(post => !post.IsAppPagePost && post.UserId != null && (post.FbUid != null || !String.IsNullOrEmpty(post.FbUserName))))
                {

                    try
                    {
                        // ReSharper disable once PossibleInvalidOperationException => filtered in LINQ
                        var userEntity = UserRepository.GetById((int)post.UserId);

                        if (userEntity == null)
                        {
                            UpdateRowStatus(post, false, null, "user entity not found");
                            continue;
                        }

                        var user_access_token = GetUserAccessToken(userEntity, out postError);


                        if (String.IsNullOrEmpty(user_access_token))
                        {
                            UpdateRowStatus(post, false, null, postError ?? "user_access_token not found");
                            continue;
                        }

                        var postSaved = post.ActionId == null ? SendPostMessage(post, user_access_token, out postId, out postError) : SendStoryMessage(post, user_access_token, out postId, out postError);

                        UpdateRowStatus(post, postSaved, postId, postError);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("send user fb post or story", post.PostId, ex, CommonEnums.LoggerObjectTypes.FB);
                    }
                }

                #region post app messages
                foreach (var post in posts.Where(post => post.IsAppPagePost))
                {
                    var postSaved = SendAppPostMessage(post, out postId, out postError);

                    UpdateRowStatus(post, postSaved, postId, postError);
                }
                #endregion
                //posts stories
            }
            catch (Exception ex)
            {
                Logger.Error("post FB messages service", ex, CommonEnums.LoggerObjectTypes.FB);
            }
        }

        #region user access token
        public string GetUserLongLivedAccessToken(string accessToken, out DateTime? expires)
        {
            expires = null;
            dynamic result = _fbClient.Get("oauth/access_token", new
            {
                client_id = FB_APP_ID,
                client_secret = FB_APP_SECRET,
                grant_type = "fb_exchange_token",
                fb_exchange_token = accessToken
            });

            var token = result.access_token;

            var number = result.expires;

            try
            {
                if (number != null)
                {
                    expires = DateTime.Now.AddSeconds(number).Date;
                }
            }
            catch (Exception) {/**/}

            return token;

        }

        public FbResponse GetFbUserToken(string accessToken, out string error)
        {
            error = string.Empty;

            try
            {
                var fbc = new FacebookClient { AppId = FB_APP_ID, AppSecret = FB_APP_SECRET, AccessToken = accessToken };
                dynamic result = fbc.Get("me");
                var fbToken = JsSerializer.Deserialize<FbResponse>(result.ToString());
                return fbToken;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error(ex);
                return null;
            }
        }

        public string GetAccessTokenFromCode(string code, string redirect_url, out string error)
        {
            error = string.Empty;
            try
            {
                dynamic result = _fbClient.Get("oauth/access_token", new
                {
                    client_id = FB_APP_ID,
                    client_secret = FB_APP_SECRET,
                    redirect_uri = redirect_url,
                    code
                });

                return result.access_token;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("get access token from code", ex, CommonEnums.LoggerObjectTypes.FB);
                return string.Empty;
            }
        }
        #endregion

        //public void CreateStoryWithAction()
        //{
        //    #region create story with object and action user
        //    var fb1 = new FacebookClient("CAAEWkqzCCHYBAFNgZCcyw6p0xC5AD9yHiiqrQUcC3tieMmDX48aY5QL0RjECsIw1xwHUVx6UkcJvBMOaTlVWEZAsBEsDiAFFN1MuKyJBZBYczo28xCpjZCcrEUqXPPutT7kmLfowqI7YqMhdlBbxThMgwKgIxrfAxrJSWbUCwYxZBhwl9SBCc1eDpnugKrCkZD");
        //    dynamic obj = new ExpandoObject();
        //    obj.lfe_course = "436868233080348";
        //    var post = fb1.Post("me/learnfromexpirence:publish_course", obj); 
        //    #endregion
        //}
        #endregion


        public bool ValidatePage(long pageId)
        {
            try
            {
                dynamic results = _fbClient.Get(pageId.ToString());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region IFacebookTabAppServices implementation
        #endregion
    }
}

