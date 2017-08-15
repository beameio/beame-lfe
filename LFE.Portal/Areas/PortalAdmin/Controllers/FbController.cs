using System;
using System.Dynamic;
using System.Web.Mvc;
using Facebook;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.PortalAdmin.Models;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    public class FbController : BaseController
    {
        public ActionResult PostMessage(FbTesterToken token)
        {
            if (String.IsNullOrEmpty(token.BaseAccessToken)) return ErrorResponse("base access_token required");

            var _fbClient = new FacebookClient(token.BaseAccessToken);

            dynamic messagePost = new ExpandoObject();

            messagePost.message = String.IsNullOrEmpty(token.Message) ? "default test msg" : token.Message;
            
            messagePost.name = "some title";
            

            messagePost.caption = "capt";
            messagePost.description = "desc";
            messagePost.picture = "https://s3.amazonaws.com/lfe-production/Course/5/9/2013/yMsQb5yePkC2VZpilIKkRg.jpg";


            if (!String.IsNullOrEmpty(token.MessageAccessToken))
            {
                messagePost.access_token = token.MessageAccessToken;
            }

            var from = (token.UsePageId ? "384502031623764" : "me");

            Logger.Debug("post app message with at::" + token.BaseAccessToken + ", from::" + from + ", msg::" + JsSerializer.Serialize(messagePost));



            var post = _fbClient.Post( from + "/feed", messagePost);

            return Json(new JsonResponseToken
                        {
                            success = true
                            ,result = post.id
                        },JsonRequestBehavior.AllowGet);
        }


        public ActionResult PostStory(FbTesterToken token)
        {
            if (String.IsNullOrEmpty(token.BaseAccessToken)) return ErrorResponse("base access_token required");

            var _fbClient = new FacebookClient(token.BaseAccessToken);

            dynamic storyPost = new ExpandoObject();

            storyPost.message = String.IsNullOrEmpty(token.Message) ? "default test story msg" : token.Message;

            if (!String.IsNullOrEmpty(token.MessageAccessToken))
            {
                storyPost.access_token = token.MessageAccessToken;
            }

            storyPost.lfe_course = token.ObjectId.ToString();

            try
            {
                var post = _fbClient.Post((token.UsePageId ? "384502031623764" : "me") + "/learnfromexpirence:" + token.Action, storyPost);

                return Json(new JsonResponseToken
                            {
                                success = true
                                ,result = post.id
                            }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new JsonResponseToken
                            {
                                success = true
                                ,result = Utils.FormatError(ex)
                            }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
