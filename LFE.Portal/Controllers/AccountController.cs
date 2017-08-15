using DotNetOpenAuth.AspNet;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.App_Start;
using LFE.Portal.Helpers;
using LFE.Portal.Models;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.IdentityModel.Services;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Resources;
using WebMatrix.WebData;
using LFE.Portal.Areas.AuthorAdmin.Models;

namespace LFE.Portal.Controllers
{
    public class AccountController : BaseController
    {
        private const bool IS_PERSISTENT_COOKIE= true;

        private readonly IUserAccountServices _userAccountServices;
        private readonly IPaypalManageServies _paypalManageServies;
        
        public AccountController()
        {
            _userAccountServices = DependencyResolver.Current.GetService<IUserAccountServices>();
            _paypalManageServies = DependencyResolver.Current.GetService<IPaypalManageServies>();        
        }

        [ChildActionOnly]
        public ActionResult _UserIndicator()
        {
            return PartialView();
         }
        
        #region private helpers
       
        /// <summary>
        /// return view on login/register error
        /// </summary>
        /// <param name="error"></param>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        private ActionResult ReturnLoginError(string error, string redirectUrl = null)
        {
            ModelState.AddModelError("", error);
            SignUserOut();
            ViewBag.LoginError = error ?? "Login error. Please contact support team";
            return View("Login", new LoginWindowToken { Mode = eLoginWindowMode.Login, ReturnUrl = redirectUrl, Message = ViewBag.LoginError = error ,IsValid = false});
            //if (String.IsNullOrEmpty(redirectUrl)) return View("Login", new LoginWindowToken { Mode = eLoginWindowMode.Login,ReturnUrl = redirectUrl});

            //return Redirect(redirectUrl);
        }
        
        /// <summary>
        /// crate authentication ticket and redirect after successfully login
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        private ActionResult SignInUser(LoginDTO model, string ReturnUrl)
        {
            string error;

            return CreateAuthenticationTicket(model.Email, model.Password,null, out error) ? RedirectToLocal(ReturnUrl) : ReturnLoginError(error, ReturnUrl);
        }


        /// <summary>
        /// check external and create user profile (account) for old lfe users
        /// </summary>
        /// <param name="email"></param>
        /// <param name="error"></param>
        /// <param name="trackingId"></param>
        /// <param name="registrationSource"></param>
        /// <returns></returns>
        private bool CreateOrUpdateExternalAccountAndLoginUser(out string email, out string error, string trackingId, CommonEnums.eRegistrationSources registrationSource)
        {
            email = string.Empty;
            try
            {
                var result = OAuthWebSecurity.VerifyAuthentication();

                if (!result.IsSuccessful || result.UserName == null)
                {
                    error = result.Error != null ? result.Error.Message : "Authentication failed";
                    return false;
                }

                //check if email registered in LFE
                var user = _userAccountServices.FindUserByEmail(result.UserName);

                if (user == null) return CreateOrUpdateExternalAccountAndLoginUser(result, out email, out error, trackingId,registrationSource);

                email = user.Email;
                
                return CreateAuthenticationTicket(user.Email, string.Empty, trackingId, out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool CreateOrUpdateExternalAccountAndLoginUser(AuthenticationResult result, out string email, out string error, string trackingId, CommonEnums.eRegistrationSources registrationSource)
        {
            return _userAccountServices.CreateOrUpdateExternalAccountAndLoginUser(result, out email, out error, trackingId,registrationSource, GetReferrer());            
        }

        /// <summary>
        /// check login credentials and create user profile (account) for old lfe users
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="error"></param>        
        /// <returns></returns>
        private bool CreateOrUpdateLfeAccountAndLoginUser(string email, string password, out string error)
        {
            if (String.IsNullOrEmpty(email))
            {
                error = "login email required";
                return false;
            }

            if (String.IsNullOrEmpty(password))
            {
                error = "password required";
                return false;
            }

            var successLogin = _userAccountServices.CreateOrUpdateLfeAccountAndLoginUser(email, password, out error);

            if (successLogin) return true;

            error = GetLoginErrorMessage(email);

            return false;
        }


        /// <summary>
        /// create authentication ticket
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="trackingId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool CreateAuthenticationTicket(string email,string password,string trackingId, out string error)
        {
            try
            {
                var userToken = _userAccountServices.GetUserDataByEmail(email, out error);

                //TODO
                #region  temp logic for SMP
                if (userToken == null && !String.IsNullOrEmpty(password)) //use only in case of lfe login
                {
                    if (_userAccountServices.VerifyOldLogin(email, password))
                    {
                        //get token again
                        userToken = _userAccountServices.GetUserDataByEmail(email, out error);
                    }
                }
                #endregion

                if (userToken == null)
                {
                    error = "user data for email " + email + " not found";
                    return false;
                }
                
                FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
                
                var securityToken = userToken.GetSecurityToken();
                FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(securityToken);

                if (Session == null) return true;

                try
                {
                    SaveUserEvent(CommonEnums.eUserEvents.LOGIN_SUCCESS,"",trackingId);

                    _userAccountServices.SaveLogin(new LoginLogoutLogToken
                    {
                        UserId    = userToken.UserId,
                        SessionId = Session.SessionID,
                        HostName  = GetReferrer()
                    }, out error);
                }
                catch (Exception){/**/}


                //2013-12-26
               // this.CachUserData(userToken);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        public bool ForceLoginUser(int? userId, string trackingId, out string error)
        {
            if (userId == null)
            {
                error = "userId required";
                return false;
            }

            error = string.Empty;

            //user already logged-in
            if (CurrentUserId >= 0 && CurrentUserId == (int) userId) return true;

            var email = _userAccountServices.GetUserEmail((int) userId);

            if (String.IsNullOrEmpty(email))
            {
                error = "user email not found";
                return false;
            }

            return CreateAuthenticationTicket(email, string.Empty, trackingId, out error);
        }

        /// <summary>
        /// parse return url qs params for WIX login/register process
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private static WixLoginToken ParseReturnUrlQS(string returnUrl)
        {
            try
            {
                var qs = string.Join(string.Empty, returnUrl.Split('?').Skip(1));

                var items = HttpUtility.ParseQueryString(qs);

                var instanceToken = items["instanceToken"] ?? items["instance"];

                var uid = Guid.Empty;
                if (!String.IsNullOrEmpty(items["uid"])) Guid.TryParse(items["uid"], out uid);

                var instanceId = Guid.Empty;
                if (!String.IsNullOrEmpty(items["instanceId"])) Guid.TryParse(items["instanceId"], out instanceId);

                var compIdToken     =  !String.IsNullOrEmpty(items["compId"]) ? items["compId"] : "";
                var origCompIdToken = !String.IsNullOrEmpty(items["origCompId"]) ? items["origCompId"] : "";

                return new WixLoginToken
                {
                    instanceToken = instanceToken
                    ,uid          = uid
                    ,instanceId   = instanceId
                    ,compId       = compIdToken
                    ,origCompId   = origCompIdToken
                };
            }
            catch (Exception)
            {
                return new WixLoginToken();
            }
        }

        /// <summary>
        /// get error message, when login failed
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private string GetLoginErrorMessage(string email)
        {
            string message;

            if (!WebSecurity.UserExists(email))
            {
                message = "The user name or password provided is incorrect.";
            }
            else if (!WebSecurity.IsConfirmed(email))
            {
                message = "You user has not been activated, please check your email and spam folder";
            }
            else
            {
                message = "The user name or password provided is incorrect.";
            }

            EventLoggerService.Report(new ReportToken
            {
                UserId = null,
                EventType = CommonEnums.eUserEvents.LOGIN_FAILED,
                NetSessionId = Session.SessionID,
                AdditionalMiscData = email,
                HostName = GetReferrer()
            });

            return message;
        }
        #endregion

        #region views
        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl = null, string error = null)
        {
            var token = new LoginWindowToken
            {
                Mode                 = eLoginWindowMode.Login,
                RequiredConfirmation = true,
                IsValid              = true,
                ReturnUrl            = ReturnUrl
            };

            if (String.IsNullOrEmpty(error)) return View("Login", token);

            token.IsValid = false;
            token.Message = error;

            return View("Login", token);
        }
        
        [AllowAnonymous]
        public ActionResult Register(string ReturnUrl = null)
        {
            var token = new LoginWindowToken
            {
                Mode                 = eLoginWindowMode.Register,
                IsValid              = true,
                RequiredConfirmation = true,
                ReturnUrl            = ReturnUrl
            };

            if (TempData["LoginError"] == null) return View("Login", token);

            token.IsValid          = false;
            token.Message          = TempData["LoginError"].ToString();
            ViewBag.LoginError     = token.Message;
            TempData["LoginError"] = null;

            return View("Login", token);
        }

      

        public ActionResult LoginWindow(eLoginWindowMode? mode, string returnUrl = null,bool requiredConfirmation = true,bool isPlugin = false, string Uid = null,byte? typeId = 0,string trackingId = null)
        {
            if (!String.IsNullOrEmpty(returnUrl)) returnUrl = returnUrl.DecodeUrl();

            var type = Utils.ParseEnum<CommonEnums.eRegistrationSources>(typeId.ToString());

            var token = new LoginWindowToken
            {
                Mode                  = mode ?? eLoginWindowMode.Login
                ,RequiredConfirmation = requiredConfirmation   
                ,IsPlugin             = isPlugin
                ,Uid                  = Uid
                ,RegistrationSource     = type
                ,TrackingId           = trackingId
            };

            if (!String.IsNullOrEmpty(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            else if (!String.IsNullOrEmpty(Request.QueryString["returnUrl"]))
            {
                ViewBag.ReturnUrl = Request.QueryString["returnUrl"];
            }
            
            token.ReturnUrl = ViewBag.ReturnUrl;

            return View(token);
        }
        
        public ActionResult UserEndPoint()
        {
            return View();
        }

        public ActionResult RegisterSuccess(string name)
        {
            return View(name);
        }
        
        public ActionResult RegisterConfirm(string key, string returnUrl = null)
        {
            if (String.IsNullOrEmpty(key)) return View();

            var confirmed =  WebSecurity.ConfirmAccount(key);

            if (confirmed)
            {
                var userDto = _userAccountServices.FindUserByConfirmationKey(key);

                if (userDto != null)
                {
                    string error;
                    _userAccountServices.SendRegistrationWelocmeEmail(userDto, out error);
                }
            }
            else
            {
                ViewBag.LoginError = "Something wend wrong. Please contact support team";
            }

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }
        
        public ActionResult _LoginPartial(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("Account/_LoginPartial");
        }
        
        public ActionResult _ExternalLoginPartial(string returnUrl = null)
        {
            if(!string.IsNullOrEmpty(returnUrl)) ViewBag.ReturnUrl = returnUrl;

            return PartialView("Account/_ExternalLoginPartial");
        }        
        
        public ActionResult _RegisterForm(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("Account/_RegisterForm");
        }
        
        public ActionResult _SuccessRegistrationMessage(string name)
        {
            return PartialView("Account/_SuccessRegistrationMessage", name);
        }
        
        public ActionResult GetBillingAddressEditForm(int? addressId)
        {
            var address = addressId != null  && addressId >= 0 ? _userAccountServices.GetBillingAddress((int) addressId) : new BillingAddressDTO();

            return PartialView("EditorTemplates/BillingAddressDTO", address);
        }
        #endregion

        #region login
        public ActionResult SignOff(string returnUrl = null)
        {
            SignUserOut();

            if (String.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Login", "Account",new{area=""});
            }
            
            return RedirectPermanent(returnUrl);
            //return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        public int MainSiteLogout()
        {
            SignUserOut();

            return 1;
        }

        public void SignUserOut()
        {
            try
            {
                this.RemoveCachedUserData();

                // For Claims-Cookie
                FederatedAuthentication.SessionAuthenticationModule.SignOut();
                WebSecurity.Logout(); // for SimpleMembership

                if (Session == null) return;

                try
                {
                    string error;
                    _userAccountServices.SaveLogout(new LoginLogoutLogToken
                    {
                        SessionId = Session.SessionID,
                        UserId = CurrentUserId
                    }, out error);

                    Session.Abandon();
                    Session.Clear();
                }
                catch (Exception e)
                {
                    Logger.Error("User sign out session",e,CommonEnums.LoggerObjectTypes.UserAccount);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("User sign out",ex,CommonEnums.LoggerObjectTypes.UserAccount);
            }


            // clear authentication cookie
            //var authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            //authCookie.Expires = DateTime.Now.AddDays(-1);
            //Response.Cookies.Add(authCookie);
        }
        
        /// <summary>
        /// post register form
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult RegisterUser(RegisterDTO token, string ReturnUrl)
        {
            string error;
            string confirmationToken;
            int? userId;
            token.RegisterHostName = GetReferrer();
            var userRegistered = _userAccountServices.RegisterNewUser(token,out userId, out confirmationToken, out error);

            if (!userRegistered)
            {
                TempData["LoginError"] = error ?? "Register error. Please contact support team";
                return RedirectToAction("Register", new { ReturnUrl });
            }

            SaveUserRegistrationEvent(userId,additionalData: String.Format("User {0} with UserId {1}",token.Email,userId),trackingID: token.TrackingID);

            if (token.RequiredConfirmation) return RedirectToAction("RegisterSuccess");

            if (!WebSecurity.Login(token.Email, token.Password,IS_PERSISTENT_COOKIE)) return ReturnLoginError("Something wend wrong. Please contact support team", ReturnUrl);
                  
  
            if(!CreateAuthenticationTicket(token.Email, token.Password,null, out error)) return ReturnLoginError(error, ReturnUrl);

            if (token.IsPluginRegistration && !string.IsNullOrEmpty(ReturnUrl))
                return RedirectToAction("SavePluginOwner", "Home", new{area="WidgetEndPoint" ,id=token.Uid });

            return RedirectToAction("RegisterConfirm", new { returnUrl = ReturnUrl});
           
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AjaxRegisterUser(RegisterDTO token, string ReturnUrl)
        {
            string error;
            string confirmationToken;
            int? userId;
            token.RegisterHostName = GetReferrer();
            var userRegistered = _userAccountServices.RegisterNewUser(token, out userId, out confirmationToken, out error);

            if (!userRegistered)
            {
                return PartialView("Account/Login/_RegistrationResult",new RegistrationConfirmToken{IsValid = false,Message = error});
            }

            SaveUserRegistrationEvent(userId, additionalData: String.Format("User {0} with UserId {1}", token.Email, userId), trackingID: token.TrackingID);

            if (token.RequiredConfirmation) return PartialView("Account/Login/_RegistrationResult", new RegistrationConfirmToken { IsValid = true, ConfirmationMessage = "Thanks for registering ! <br> A confirmation Email was sent to the Email address you provided.Once received, click on the account activation link to complete your registration" });

            if (!WebSecurity.Login(token.Email, token.Password, IS_PERSISTENT_COOKIE)) return PartialView("Account/Login/_RegistrationResult", new RegistrationConfirmToken { IsValid = false, Message = "Something wend wrong. Please contact support team" });

            return CreateAuthenticationTicket(token.Email, token.Password, null, out error) ? PartialView("Account/Login/_RegistrationResult", new RegistrationConfirmToken { IsValid = true, ConfirmationMessage = "Thanks for registering !" }) : PartialView("Account/Login/_RegistrationResult", new RegistrationConfirmToken { IsValid = false, Message = error });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AjaxRegisterScUser(RegisterDTO token, string ReturnUrl)
        {
            string error;
            string confirmationToken;
            int? userId;
            token.RegisterHostName = GetReferrer();
            var userRegistered = _userAccountServices.RegisterNewUser(token, out userId, out confirmationToken, out error);

            if (!userRegistered)
            {
                return ErrorResponse(error);
            }

            SaveUserRegistrationEvent(userId, additionalData: String.Format("User {0} with UserId {1}", token.Email, userId), trackingID: token.TrackingID);


            if (!WebSecurity.Login(token.Email, token.Password, IS_PERSISTENT_COOKIE)) return ErrorResponse("Something wend wrong. Please contact support team" );

            return CreateAuthenticationTicket(token.Email, token.Password, null, out error) ? Json(new JsonResponseToken{success = true,result = new{returnUrl = ReturnUrl}}) : ErrorResponse(error);
        }

        /// <summary>
        /// register user from Wix end-point
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult RegisterWixUser(RegisterDTO token, string ReturnUrl)
        {
            string error;
            string confirmationToken;
            var loginResult = false;
            int? userId;
            token.RegistrationSource = CommonEnums.eRegistrationSources.WIX;
            token.RegisterHostName   = GetReferrer();

            var userRegistered = _userAccountServices.RegisterNewUser(token,out userId, out confirmationToken, out error);

            if (userRegistered)
            {
                SaveUserRegistrationEvent(userId, additionalData: String.Format("User {0} with UserId {1}", token.Email, userId), trackingID: token.TrackingID);

                //make login
                loginResult = WebSecurity.Login(token.Email, token.Password,IS_PERSISTENT_COOKIE);

                //create authentication ticket
                if (loginResult) loginResult = CreateAuthenticationTicket(token.Email, token.Password,token.TrackingID, out error);
            }

            var qs_params = ParseReturnUrlQS(ReturnUrl);

            Response.Headers.Set("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            return RedirectToAction("WixLoginResultHandler", "Account", new { area = "WixEndPoint", success = loginResult, qs_params.instanceToken, qs_params.uid, qs_params.instanceId, origCompIdToken = qs_params.origCompId, compIdToken = qs_params.compId, error });
        }

        /// <summary>
        /// called from widget account controller on widget user register
        /// </summary>
        /// <param name="token"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool RegisterWidgetUser(RegisterDTO token, out string error)
        {
            string confirmationToken;
            int? userId;
            token.RegisterHostName = GetReferrer();
            var userRegistered = _userAccountServices.RegisterNewUser(token,out userId, out confirmationToken, out error);

            if (!userRegistered) return false;

            SaveUserRegistrationEvent(userId, additionalData: String.Format("User {0} with UserId {1}", token.Email, userId), trackingID: token.TrackingID);

            if (Response != null) { Response.Headers.Set("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\""); }

            if (token.RequiredConfirmation) return true;
    
            //when confirmation not required => force user login
            if (WebSecurity.Login(token.Email, token.Password,IS_PERSISTENT_COOKIE)) return CreateAuthenticationTicket(token.Email, token.Password,token.TrackingID, out error);

            error = "Oooopps, something went wrong on login. Please contact support team";
            return false;
        }

        /// <summary>
        /// handle LFE user login
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        //[HttpPost]
        [AllowAnonymous]
        public ActionResult LfeLogin(LoginDTO model, string ReturnUrl)
        {
            string error;

            var isLoginSucces = CreateOrUpdateLfeAccountAndLoginUser(model.Email, model.Password, out error);

            return isLoginSucces ? SignInUser(model, ReturnUrl) : ReturnLoginError(error,ReturnUrl);          
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AjaxLfeLogin(LoginDTO model,string redirectUrl = null)
        {
            string error;

            var result = LoginWithLfeCredentials(model,out error);

            return Json(new JsonResponseToken {success = result, error = error,result = new{returnUrl = redirectUrl ?? model.RedirectUrl}}, JsonRequestBehavior.AllowGet);
        }

        private bool LoginWithLfeCredentials(LoginDTO model,out string error)
        {
            var isLoginSucces = CreateOrUpdateLfeAccountAndLoginUser(model.Email, model.Password, out error);

            var result = isLoginSucces && CreateAuthenticationTicket(model.Email, model.Password, null, out error);

            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AjaxFbApproveLfeLogin(LoginDTO model)
        {
            string error;

            var result = LoginWithLfeCredentials(model, out error);

            int? userId = null;

            if (!result) return Json(new JsonResponseToken {success = false, result = null, error = error},JsonRequestBehavior.AllowGet);

            var user = _userAccountServices.GetUserDataByEmail(model.Email, out error);
            if (user != null) userId = user.UserId;

            return Json(new JsonResponseToken {success = userId != null,result = userId, error = error ?? "Unexpected error. Contact Support team."}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// handle login from wix
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult WixLfeLogin(LoginDTO model, string returnUrl)
        {
            string error;
          
            var qs_params = ParseReturnUrlQS(returnUrl);

            var loginResult = CreateOrUpdateLfeAccountAndLoginUser(model.Email, model.Password, out error);

            if (loginResult) loginResult = CreateAuthenticationTicket(model.Email, model.Password,model.TrackingID, out error);

            Response.Headers.Set("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            Response.Headers.Set("P3P", "CP=\"NOI CURa ADMa DEVa TAIa OUR BUS IND UNI COM NAV INT\"");
       
            return RedirectToAction("WixLoginResultHandler", "Account", new { area = "WixEndPoint", success = loginResult, qs_params.instanceToken, qs_params.uid, qs_params.instanceId, origCompIdToken = qs_params.origCompId, compIdToken = qs_params.compId, error });
        }

        /// <summary>
        /// called from toolbar login window
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult WidgetLfeLogin(LoginDTO model)
        {
            string error;
            
            var loginResult = CreateOrUpdateLfeAccountAndLoginUser(model.Email, model.Password, out error);

            if (loginResult) loginResult = CreateAuthenticationTicket(model.Email, model.Password,model.TrackingID, out error);

            var widgetAccountController = new Areas.Widget.Controllers.AccountController();

            return widgetAccountController.WidgetLoginResultHandler(model.Email
                                                                  ,loginResult
                                                                  ,model.IsWidget
                                                                  ,model.TrackingID
                                                                  ,model.ParentWindowURL
                                                                  ,error);

        }

        /// <summary>
        /// force login for wix user , if it's already related to wix member
        /// </summary>
        /// <param name="email"></param>
        /// <param name="returnUrl"></param>
        /// <param name="trackingId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ForceWixUserSignIn(string email, string returnUrl, string trackingId = null)
        {
            string error = null;
            bool loginResult;

            var qs_params = ParseReturnUrlQS(returnUrl);

            //check if external login
            var externalLogin = OAuthWebSecurity.GetAccountsFromUserName(email).FirstOrDefault();

            if (externalLogin != null)
            {
                var result = externalLogin;
                loginResult = OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, IS_PERSISTENT_COOKIE) && CreateAuthenticationTicket(email, string.Empty,trackingId, out error);
            }
            else
            {
                loginResult = CreateAuthenticationTicket(email, string.Empty,trackingId, out error);
            }
            
            //ALWAYS return true  , otherwise it could create endless loop on trying to login
            return RedirectToAction("WixLoginResultHandler", "Account", new { area = "WixEndPoint", success = true, qs_params.instanceToken, qs_params.uid, qs_params.instanceId, origCompIdToken = qs_params.origCompId, compIdToken = qs_params.compId,  error = error + (loginResult ? "" : "something went wrong") });
        }

        /// <summary>
        /// on social login action
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="returnUrl"></param>
        /// <param name="trackingId"></param>
        /// <param name="registrationSource"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl, string trackingId, CommonEnums.eRegistrationSources registrationSource = CommonEnums.eRegistrationSources.LFE)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { returnUrl,trackingId,registrationType = registrationSource }));
        }

        /// <summary>
        /// callback from ExternalLogin
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="trackingId"></param>
        /// <param name="registrationSource"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl, string trackingId, CommonEnums.eRegistrationSources registrationSource = CommonEnums.eRegistrationSources.LFE)
        {
            string error;
            
            string email;

            var isAccountCreated = CreateOrUpdateExternalAccountAndLoginUser(out email, out error,trackingId,registrationSource);

            if (isAccountCreated && !String.IsNullOrEmpty(email))
            {
                return SignInUser(new LoginDTO { Email = email }, returnUrl);
            }

            return ReturnLoginError(error);            
        }

        public AuthenticationResult VerifyFbUserAccessToken(string access_token)
        {
            var fbAuthClient = new FacebookScopedClient();

            var result = fbAuthClient.VerifyAuthentication(access_token);

            return result;
        }

        public bool CreateOrUpdateFbAccountAndLoginWixUser(string access_token, out string error,string trackingId = null)
        {
            try
            {
                
                var result = VerifyFbUserAccessToken(access_token);

                if (!result.IsSuccessful|| result.UserName == null)
                {
                    Logger.Debug("wix fb login::access_token:" + access_token + "::" + JsSerializer.Serialize(result));
                    error = result.Error != null ? result.Error.Message : "Authentication failed";
                    return false;
                }

                //check if email registered in LFE
                var user = _userAccountServices.FindUserByEmail(result.UserName);

                if (user != null)
                {
                    return CreateAuthenticationTicket(user.Email, string.Empty, trackingId, out error);
                }

                string email;

                var loginResult = CreateOrUpdateExternalAccountAndLoginUser(result, out email, out error, trackingId, CommonEnums.eRegistrationSources.WIX);

                if (!loginResult) return false;
                
                loginResult = CreateAuthenticationTicket(email, string.Empty,trackingId, out error);
                
                return loginResult;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        /// <summary>
        /// Create/Update account for facebook login from widget toolbar. 
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="registrationSource"></param>
        /// <param name="email"></param>
        /// <param name="error"></param>
        /// <param name="trackingId"></param>
        /// <returns></returns>
        public bool CreateOrUpdateFbAccountAndLoginWidgetUser(string access_token, CommonEnums.eRegistrationSources registrationSource, out string email, out string error, string trackingId = null) //, bool tryMatch = false
        {
            email = string.Empty;
            try
            {
                var result = VerifyFbUserAccessToken(access_token);

                if (!result.IsSuccessful || result.UserName == null)
                {
                    error = result.Error != null ? result.Error.Message : "Authentication failed";
                    return false;
                }

                //if (tryMatch)
                //{
                bool providerApproved;
                //try match by social credentials
                var user = _userAccountServices.TryFoundUserBySocialCredentials(result.UserName,result.ProviderUserId, CommonEnums.SocialProviders.Facebook,out providerApproved, out error);

                if (user != null)
                {
                    if (user.FacebookId == null)
                    {
                        _userAccountServices.UpdateUserFbUserId(user.UserId, result.ProviderUserId);
                    }

                    return CreateAuthenticationTicket(user.Email, string.Empty, trackingId, out error);
                }
                //}

                return CreateOrUpdateExternalAccountAndLoginUser(result, out email, out error, trackingId,registrationSource) && CreateAuthenticationTicket(email, string.Empty,trackingId, out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

       
        #endregion

        #region account settings manage
        [Authorize]
        public ActionResult UserSettings()
        {
           // string error;
            var user = this.CurrentUser();

            if (user == null) return RedirectToAction("Index", "Home", new {area = ""});

          //  var token = _userAccountServices.GetSettingsToken(this.CurrentUser().UserId,out error);
            return View("Settings",new AccountSettingsDTO());
        }

        [Authorize]
        public ActionResult _EditAccountSettings(CommonEnums.UserRoles Role)
        {
            string error;
            var token = _userAccountServices.GetSettingsToken(this.CurrentUser().UserId, out error);
            token.Role = Role;
            return PartialView("Account/_EditAccountSettings", token);
        }

        [Authorize]
        public ActionResult _EditCommunicationSettings(CommonEnums.UserRoles Role)
        {
            string error;
            var token = _userAccountServices.GetSettingsToken(this.CurrentUser().UserId, out error);
            token.Role = Role;
            return PartialView("Account/_EditCommunicationSettings", token);
        }

        [Authorize]
        public ActionResult _EditPaymentMethods()
        {
            var list = _paypalManageServies.GetUserPaymentInstruments(CurrentUserId);
            return PartialView("Account/_EditPaymentMethods",list);
        }

        [Authorize]
        public ActionResult _PayoutSettings()
        {
            var token = _userAccountServices.GetPayoutSettings(CurrentUserId);
            return PartialView("Account/_PayoutSettings",token);
        }

          [HttpPost]
        [Authorize]
        public ActionResult SavePayoutSettings(UserPayoutSettingsDTO token, BillingAddressDTO address)
        {
            string error;


            switch (token.PayoutType)
            {
                case BillingEnums.ePayoutTypes.PAYPAL:
                    if(String.IsNullOrEmpty(token.Email)) return ErrorResponse("email required");
                    break;
                case BillingEnums.ePayoutTypes.CHEQUE:
                    if (address==null) return ErrorResponse("address required");
                    token.BillingAddress = address;
                    break;
                default:
                    return ErrorResponse("Unknown payout type");                    
            }

            int? addressId;
            token.UserId = CurrentUserId;
            var isUpdated = _userAccountServices.SavePayoutSetting(token,out addressId, out error);

            if(isUpdated) token.UpdateUserClaims();

            return Json(new JsonResponseToken
            {
                success = isUpdated
                ,error  = error
                ,result = new { addressId }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult UpdateAccountSettings(AccountSettingsDTO token)
        {
            string error;
            var isUpdated = _userAccountServices.UpdateAccountSettings(token, out error);
            return Json(new JsonResponseToken
            {
                success = isUpdated
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UpdateCommunicationSettings(AccountSettingsDTO token)
        {
            string error;
            var isUpdated = _userAccountServices.UpdateCommunicationSettings(token, out error);
            return Json(new JsonResponseToken
            {
                success = isUpdated
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteStoredPM([DataSourceRequest] DataSourceRequest request, PaymentInstrumentViewDTO dto)
        {
            if (dto != null)
            {
                string error;
                _paypalManageServies.DeleteCreditCard(dto.InstrumentId, out error);
            }

            //Return any validation errors if any
            return Json(ModelState.ToDataSourceResult());


        }


        #region refund program
        private ActionResult _RefundSettingsView(RefundSettingsModel model)
        {
            ViewBag.ShowJoinedSuccess = model != null && model.ShowJoinedSuccess;
            ViewBag.ShowRemovedSuccess = model != null && model.ShowRemovedSuccess;
            ViewBag.IsValid = model == null || model.IsValid;
            ViewBag.Error = model != null ? model.Error : string.Empty;

            var token = _userAccountServices.GetAuthorRefundProgramStatus(CurrentUserId);
            token.Checked = false;
            return PartialView("Account/_RefundProgram", token);
        }

        public ActionResult _RefundProgram()
        {
            return _RefundSettingsView(null);
        }

        [HttpPost]
        public ActionResult AddToRefundProgram(AuhorRefundProgramDTO token)
        {
            var model = new RefundSettingsModel();

            if (!token.Checked) return _RefundSettingsView(null);

            string error;

            model.IsValid = _userAccountServices.AddAuthorToRefundProgram(CurrentUserId, out error);
            
            model.Error = error;
            
            model.ShowJoinedSuccess = model.IsValid;


            if (model.IsValid)
            {
                SaveUserEvent(CommonEnums.eUserEvents.MBG_JOIN);
            }

            return _RefundSettingsView(model);
        }

        [HttpPost]
        public ActionResult RemoveFromRefundProgram(AuhorRefundProgramDTO token)
        {
            var model = new RefundSettingsModel();

            if (!token.Checked) return _RefundSettingsView(null);

            string error;

            model.IsValid = _userAccountServices.RemoveAuthorFromRefundProgram(CurrentUserId, out error);
            model.Error = error;
            model.ShowRemovedSuccess = model.IsValid;

            if (model.IsValid)
            {
                SaveUserEvent(CommonEnums.eUserEvents.MBG_CANCEL);
            }

            return _RefundSettingsView(model);
        }

        #endregion

        #endregion

        #region forgotten password
        public ActionResult ResetPassword(string key, string returnUrl = null)
        {
            if (String.IsNullOrEmpty(key))
            {
                var token = new ResetPasswordPageToken
                {
                     IsValid            = false
                    ,PasswordResetToken = null
                    ,Error              = GlobalResources.ACC_PWD_ResetKeyMissing
                };

                return View(token);
            }
            else
            {
                var id = WebSecurity.GetUserIdFromPasswordResetToken(key);

                var token = new ResetPasswordPageToken
                {
                    IsValid             = !(id<0)
                    ,PasswordResetToken = id < 0 ? null : key
                    ,Error              = id<0 ? "Invalid token." : ""
                };

                return View(token);    
            }
        }

        public ActionResult ResetPasswordSuccess()
        {
            var dto = new ResetPasswordPageToken
            {
                IsValid          = true
                ,PasswordChanged = true
                ,Message         = GlobalResources.ACC_PWD_SuccessChanged
            };

            return View("ResetPassword",dto);
        }

        public ActionResult ResetPasswordFailed()
        {
            var dto = new ResetPasswordPageToken {IsValid = false, Error = GlobalResources.ERR_CommonMessage};

            return View("ResetPassword",dto);
        }

        [HttpPost]
        public ActionResult DoResetPassword(ResetLocalPasswordToken token)
        {
            if (!ModelState.IsValid) return RedirectToAction("ResetPasswordFailed");

            var passwordChanged = WebSecurity.ResetPassword(token.PasswordResetToken, token.NewPassword);

            return RedirectToAction(passwordChanged ? "ResetPasswordSuccess" : "ResetPasswordFailed");
        }

        public JsonResult SendForgottenPasswordEmail(string email,string  returnUrl = null)
        {
            string error;
            var result = _userAccountServices.SendResetPasswordEmail(email, out error, returnUrl);

            return Json(new JsonResponseToken {success = result, error = error},JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region internal class
        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }
        #endregion

        #region fb helpers

        public bool ConnectSocialLoginToLfeAccount(int userId, string uid, CommonEnums.SocialProviders provider,out string error)
        {
            return _userAccountServices.ConnectSocialLoginToLfeAccount(userId, uid, provider, out error);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ValidateFbAdminUser(long? id, string email)
        {
            if (id == null && String.IsNullOrEmpty(email)) return ErrorResponse("fb userId or email should be supplied");

            string error;
            bool providerApproved;
            
            var currentUser = this.CurrentUser();

            var fbUser = _userAccountServices.TryFoundUserBySocialCredentials(email, id == null ? string.Empty : id.ToString(), CommonEnums.SocialProviders.Facebook, out providerApproved, out error);

            var authenticationResult = new FbAdminAuthenticationResult{state = FbPageAppAdminMatchResults.Unknown};
          
            if (fbUser == null)
            {
                authenticationResult.state = currentUser != null ? FbPageAppAdminMatchResults.NotFoundAuthenticated : FbPageAppAdminMatchResults.NotFoundNotAuthenticated;
            }
            else
            {
                authenticationResult.fbUserId    = fbUser.UserId;
                authenticationResult.fbUserEmail = fbUser.Email;
                authenticationResult.state       = providerApproved ? FbPageAppAdminMatchResults.FoundAndProviderdApproved : FbPageAppAdminMatchResults.FoundAndMatchedByEmail;
            }

            return Json(new JsonResponseToken { success = true, result = authenticationResult, error = error }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}

 