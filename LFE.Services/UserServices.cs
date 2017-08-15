using DotNetOpenAuth.AspNet;
using LFE.Application.Services.Base;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Application.Services.Security;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayPal.Api;
using WebMatrix.WebData;

namespace LFE.Application.Services
{
	public class WidgetUserServices : ServiceBase, IUserPortalServices, IUserEventLoggerServices, IUserNotificationServices, IAuthorAdminServices, IUserAccountServices, IWidgetUserServices, IWixUserServices, IPortalAdminUserServices
	{
		private readonly IFacebookServices _facebookServices;
		private readonly IEmailServices _emailServices;
		private readonly IAmazonEmailWrapper _amazonEmailWrapper;
		private readonly IS3Wrapper _s3Wrapper;
		private readonly IWidgetCourseServices _itemServices;
	    //private readonly IProvisionUserServices _provisionUserServices;
	  
		public WidgetUserServices()
		{
			_facebookServices      = DependencyResolver.Current.GetService<IFacebookServices>();
			_amazonEmailWrapper    = DependencyResolver.Current.GetService<IAmazonEmailWrapper>();
			_emailServices         = DependencyResolver.Current.GetService<IEmailServices>();
			_s3Wrapper             = DependencyResolver.Current.GetService<IS3Wrapper>();
			_itemServices          = DependencyResolver.Current.GetService<IWidgetCourseServices>();
          //  _provisionUserServices = DependencyResolver.Current.GetService<IProvisionUserServices>();
        }

		#region IUserAccountServices implementation
		private const string DEFAULT_SIMPLE_PASSWORD = "1234567!";
		
		#region private helpers

//	    private void _notifyUser2Provision(int userId)
//	    {
//	        string error;
//	        _provisionUserServices.SaveUser(userId, out error);
//	    }
		private bool VerifyEmailOnRegister(string email)
		{
			return !UserRepository.IsAny(x => x.Email.ToLower() == email.ToLower());
		}

		private static void SaveUserProfilePwd(string userName, string password)
		{
			var pwdToken = WebSecurity.GeneratePasswordResetToken(userName);

			WebSecurity.ResetPassword(pwdToken, password);
		}

		private static void UpdateUserRoles(string userName, ICollection<string> roles)
		{
			var currentRoles = _rolesProvider.GetRolesForUser(userName);

			var rolesToRemove = currentRoles.Where(x => !roles.Contains(x));

			foreach (var role in rolesToRemove)
			{
				_rolesProvider.RemoveUsersFromRoles(new[] { userName }, new[] { role });
			}

			foreach (var role in roles)
			{
				var isUserInRole = _rolesProvider.IsUserInRole(userName, role);

				if (isUserInRole) continue;

				string error;
				AddRole2User(userName, Utils.ParseEnum<CommonEnums.UserRoles>(role), out error);
			}
		}

		private static void AddRole2User(string userName, CommonEnums.UserRoles role, out string error)
		{
			error = string.Empty;

			try
			{
				if (!_rolesProvider.GetRolesForUser(userName).Contains(role.ToString()))
				{
					_rolesProvider.AddUsersToRoles(new[] { userName }, new[] { role.ToString() });
				}

				//return;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				//return;
			}
		}

		private Users VerifyUserOldLogin(string email, string pass)
		{
			var user = UserRepository.Get(x => x.Email == email);

			if (user != null && user.StatusType != (int)UserEnums.eUserStatuses.active) user = null;

			if (user == null) return null;

			var pwdVerified = PasswordHasher.VerifyHash(pass, user.PasswordDigest);

			if (!pwdVerified) return null;

			user.LastLogin = DateTime.Now;

			UserRepository.UnitOfWork.CommitAndRefreshChanges();

			return user;
		}

		private void UpdateLastLoginDate(string email)
		{
			try
			{
				var e = email.TrimString().ToLower();
				var entity = UserProfileRepository.Get(x => x.Email.ToLower() == e);
				if (entity == null)
				{
					Logger.Error("Update last login::"+e+"::profile not found");
					return;
				}

				entity.LastLogin = DateTime.Now;

				UserProfileRepository.UnitOfWork.CommitAndRefreshChanges();
			}
			catch (Exception ex)
			{
				Logger.Error("Update last login::"+email,ex,CommonEnums.LoggerObjectTypes.UserAccount);
			}
		}
		#endregion

		#region interface implementation
		public string GetUserEmail(int id)
		{
			try
			{
				var userEntity = UserRepository.Get(x => x.Id == id);

				return userEntity != null ? userEntity.Email : null;
			}
			catch (Exception ex)
			{
				Logger.Error("get user email by id " + id, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}

		public UserInfoDTO TryFoundUserBySocialCredentials(string email, string providerUserId, CommonEnums.SocialProviders provider, out bool providerApproved, out string error)
		{
			error = string.Empty;
			providerApproved = false;
			try
			{
				var userEntity = UserRepository.Get(x => x.FacebookID == providerUserId);

				if (userEntity != null)
				{
					providerApproved = true;
					return userEntity.Entity2UserInfoDto();
				} 

				var user = FindUserByEmail(email);

				providerApproved = user != null &&  user.FacebookId == providerUserId;

				return user != null ? user.UserDto2UserInfoDto() : null;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("TryFoundUserBySocialCredentials::" + providerUserId +"::" + provider, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}

		public bool ConnectSocialLoginToLfeAccount(int userId, string uid, CommonEnums.SocialProviders provider, out string error)
		{
			error = string.Empty;
			try
			{
				var userEntity = UserRepository.GetById(userId);

				if (userEntity == null)
				{
					error = "user entity not found";
					return false;
				}

				switch (provider)
				{
				   case CommonEnums.SocialProviders.Facebook:
                        //TODO add event to provision
						userEntity.UpdateFbId(uid);
						if(!UserRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                        //_notifyUser2Provision(userId);
				        return true;
                    default:
						error = "Provider not supported";
						return false;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Connect "+provider+" 2 LFE User " + userId + ":: uid=" + uid, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}    
		}

		public UserDTO FindUserByEmail(string email)
		{
			try
			{
				var userEntity = UserRepository.Get(x => x.Email == email);

			   return userEntity?.Entity2UserDto();
			}
			catch (Exception ex)
			{
				Logger.Error("find user by email " + email,ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}    
		}

//	    public UserDTO RenewSecurityToken(string email)
//	    {
//            try
//            {
//                string error;
//
//                var token = GetUserDataByEmail(email, out error);
//
//                if (token == null) return null;
//
//                var securityToken = token.GetSecurityToken();
//
//                FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
//                FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(securityToken);
//
//                return token;
//            }
//            catch (Exception ex)
//            {
//                Logger.Error("Renew security token",ex,CommonEnums.LoggerObjectTypes.UserAccount);
//                return null;
//            }
//        }

		public UserDTO GetUserDataByEmail(string email, out string error)
		{
			error = string.Empty;

			try
			{
				if (UserRepository != null)
				{
					var user = UserRepository.GetUser(email.ToLower(),out error);

					if (!string.IsNullOrEmpty(error))
					{
						Logger.Error("GetUserDataByEmail::"+email+"::get user error::"+error);
					}

					if(user != null) return user.ViewEntity2UserDto();
				}
				else
				{
					Logger.Error("GetUserDataByEmail::" + email + "::user repository is null");    
				}
				
				using (var context = new lfeAuthorEntities())
				{
					var user = context.vw_USER_UsersLib.SingleOrDefault(x => x.Email.ToLower() == email.ToLower());

					return user?.ViewEntity2UserDto();
				}
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("GetUserDataByEmail::" + email, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}     

		public AccountSettingsDTO GetSettingsToken(int userId, out string error)
		{
			error = string.Empty;
			try
			{
				var entity = UserRepository.GetById(userId);

				var token = entity?.UserEntity2AccountSettingsDTO();

				if (token == null)
				{
					error = "User entity not found";
					return null;
				}

				using (var context = new lfeAuthorEntities())
				{
					var profile = context.UserProfile.FirstOrDefault(x => x.RefUserId == token.UserId);

					if (profile == null)
					{
						error = "Profile not found";
						return null;

					}

					token.IsSocialLogin = context.webpages_OAuthMembership.FirstOrDefault(x=>x.UserId == profile.UserId) != null;
				}
				
				if (token.IsSocialLogin && token.FbUid != null)
				{
					token.FbUser = _facebookServices.GetFbUser((long)token.FbUid);
				}

				return token;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("get user settings token",ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}

		public bool UpdateAccountSettings(AccountSettingsDTO dto, out string error)
		{
			error = string.Empty;
			try
			{

				var entity = UserRepository.GetById(dto.UserId);

				if (entity == null)
				{
					error = "User entity not found";
					return false;
				}

				entity.UpdateAcountEntity(dto);

				if (!string.IsNullOrEmpty(dto.Password))
				{
					SaveUserProfilePwd(entity.Email, dto.Password);

					//Old password
					//entity.PasswordDigest = PasswordHasher.ComputeHash(dto.Password);
				}

				UserRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("update account settings", dto.UserId, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public bool UpdateAccountSettings(WizardAboutAuthorDTO dto, out string error)
		{
			error = string.Empty;
			try
			{

				var entity = UserRepository.GetById(dto.UserId);

				if (entity == null)
				{
					error = "User entity not found";
					return false;
				}

				entity.UpdateAcountEntity(dto);

				UserRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("update wizard account settings", dto.UserId, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public bool UpdateCommunicationSettings(AccountSettingsDTO dto, out string error)
		{
			error = string.Empty;
			try
			{

				var entity = UserRepository.GetById(dto.UserId);

				if (entity == null)
				{
					error = "User entity not found";
					return false;
				}

				entity.UpdateCommunicationSettings(dto);

				UserRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("update communication settings", dto.UserId, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public Users GetUserByFacebookId(string facebookid)
		{
			return UserRepository.Get(x=>x.FacebookID==facebookid);
		}

		public UserDTO FindUserByConfirmationKey(string key)
		{
			try
			{
				using (var context = new lfeAuthorEntities())
				{
					var entity = context.webpages_Membership.FirstOrDefault(x => x.ConfirmationToken == key);

					var profile = context.UserProfile.FirstOrDefault(x=>x.UserId==entity.UserId);
				  
					if (profile == null) return null;

					var userId = entity != null ? profile.RefUserId : null;

					return userId == null ? null : UserRepository.GetById((int)userId).Entity2UserDto();
				}
			}
			catch (Exception ex)
			{

				Logger.Error("find user by confirmation key " + key,ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}

		public Users RegisterFacebookUser(  string email, 
											string facebookId,
											string firstname, 
											string lastname, 
											string birthdate, 
											string gender,
											string trackingId,
											CommonEnums.eRegistrationSources registrationSource,
											string hostName,
											out string error
										  )
		{
			error = string.Empty;
			try
			{
				//prevent double registration with FB and LFE login on same email
				var user = UserRepository.Get(u => u.Email == email);

				if (user == null)
				{
					DateTime dtTemp;
					DateTime? userBirthday = null;

					if (DateTime.TryParseExact(birthdate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtTemp))
					{
						userBirthday = dtTemp;
					}

					int? userGender = gender.ToLower() == CommonEnums.eGenders.male.ToString() ? (int)CommonEnums.eGenders.male : (int)CommonEnums.eGenders.female;
					
					var registerToken = new RegisterDTO
					{
						Email            = email,
						FbUid            = facebookId,
						Nickname         = $"{firstname} {lastname}",
						FirstName        = firstname,
						LastName         = lastname,
						Birthday         = userBirthday,
						Gender           = userGender,
						RegistrationSource = registrationSource,
						RegisterHostName = hostName                        
					};

					user = registerToken.RegisterDto2UsersEntity(FindStoreId(trackingId));
                    //TODO add event to provision
                    UserRepository.Add(user);

					if(!UserRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return null;

                    //_notifyUser2Provision(user.Id);
				}
				else
				{
					if(user.FacebookID == facebookId)  return user;

					error = $"User already registered with {email}, please use LFE Login.";

					return null;
				}

				return UserRepository.Get(x => x.FacebookID == facebookId);
			  
			}
			catch (Exception)
			{
				return null;
			}
		  
		}

		public void UpdateUserFbAccessToken(string fbUid, string accessToken)
		{
			try
			{
				if (string.IsNullOrEmpty(accessToken)) return;

				DateTime? expires;
				var at = _facebookServices.GetUserLongLivedAccessToken(accessToken, out expires);

				if (string.IsNullOrEmpty(at)) return;

				var entity = UserRepository.Get(x=>x.FacebookID==fbUid);

				if (entity == null) return;

				entity.FbAccessToken = at;
				if (expires != null) entity.FbAccessTokenExpired = expires;

				UserRepository.UnitOfWork.CommitAndRefreshChanges();
			}
			catch (Exception ex)
			{
				Logger.Error("update user FB access token (" + fbUid + ")", ex, CommonEnums.LoggerObjectTypes.UserAccount);
			}
		}
		
		public UserDTO CreateLfeUser(RegisterDTO token, out string error)
		{

			try
			{
				if (!VerifyEmailOnRegister(token.Email.TrimString()))
				{
					error = "Email already exists";
					return null;
				}

				var storeId = FindStoreId(token.TrackingID);

				var entity = token.RegisterDto2UsersEntity(storeId);//PasswordHasher.ComputeHash(token.Password)
                //TODO add event to provision
                UserRepository.Add(entity);

				if(!UserRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return null;

                //_notifyUser2Provision(entity.Id);

			    return UserRepository.GetById(entity.Id).Entity2UserDto();
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("register user " + token.Email,ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}

		public FbResponse GetFbUserToken(string accessToken, out string error)
		{
			try
			{
				return _facebookServices.GetFbUserToken(accessToken, out error);                
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("get Fb user token by access token", ex, CommonEnums.LoggerObjectTypes.FB);
				return null;
			}
		}
		
		public List<UserBaseDTO> FindUsers(string text)
		{
			try
			{
				if(string.IsNullOrEmpty(text)) return new List<UserBaseDTO>();

				var t = text.TrimString().ToLower();
			   
				return
					UserRepository.GetMany(x =>
												x.FirstName.ToLower().Contains(t) || 
												x.LastName.ToLower().Contains(t) ||
												x.Nickname.ToLower().Contains(t)
											).Select(x => x.Entity2UserBaseDto()).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("find user by " + text, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return new List<UserBaseDTO>();
			}
		}
		#endregion
		
		#region simple membership provider services
		
		private void CreateUserRolesOnConvert2NewLogin(int userId)
		{
			string error;

			var entity = UserRepository.GetById(userId);

			if (entity == null) return;

			if (CourseRepository.IsAny(x => x.AuthorUserId == userId)) AddRole2User(entity.Email, CommonEnums.UserRoles.Author, out error);

			if (UserCourseRepository.IsAny(x => x.UserId == userId)) AddRole2User(entity.Email, CommonEnums.UserRoles.Learner, out error);
			
			//admin
			if (entity.UserTypeID == 2) AddRole2User(entity.Email, CommonEnums.UserRoles.Admin, out error);
		}

		public bool CreateUser(RegisterDTO token, int? RefUserId, out string confirmationToken, out string error, bool updateRoles = false, bool requiredConfirmation = false)
		{
			error = string.Empty;
			confirmationToken = string.Empty;
			try
			{
				confirmationToken = WebSecurity.CreateUserAndAccount(token.Email
												 ,token.Password
												 ,new
												 {
													RefUserId
												 }, requiredConfirmation);
				
				if(RefUserId!=null && updateRoles) CreateUserRolesOnConvert2NewLogin((int) RefUserId);

				return true;

			}
			catch (MembershipCreateUserException e)
			{
				error = e.StatusCode.ErrorCodeToString();
				Logger.Error("create user " + token.Email, e, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("create user " + token.Email, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public bool CreateOrUpdateUser(string email, int? RefUserId, out string error, bool updateRoles = false)
		{
			error = string.Empty;

			try
			{
				//check if profile record created
				var entity = UserProfileRepository.Get(x => x.Email == email);

				//create new profile
				if (entity == null)
				{
					entity=new UserProfile
					{
						Email = email
						,RefUserId = RefUserId
						,AddOn = DateTime.Now
					};

					UserProfileRepository.Add(entity);                    
				}
				//update RefUserId
				else
				{
					entity.RefUserId = RefUserId;
				}

				UserProfileRepository.UnitOfWork.CommitAndRefreshChanges();

				if (RefUserId != null && updateRoles) CreateUserRolesOnConvert2NewLogin((int)RefUserId);

				return true;

			}           
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("create oAuth user " + email, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		/// <summary>
		/// uses by portal admin login manage
		/// </summary>
		/// <param name="token"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool UpdateUser(UserEditDTO token, out string error)
		{
			error = string.Empty;
			try
			{

				var entity = UserRepository.GetById(token.UserId);

				if (entity == null)
				{
					error = "User entity not found";
					return false;
				}

				entity.StatusType = (int)token.Status;

				if (token.UserProfileId < 0)
				{
					if (!string.IsNullOrEmpty(token.OldPassword)) entity.PasswordDigest = PasswordHasher.ComputeHash(token.OldPassword);
				}
				else
				{
					if (!string.IsNullOrEmpty(token.Password)) SaveUserProfilePwd(entity.Email, token.Password);

					UpdateUserRoles(token.Email,token.Roles);
				}


				UserRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("update user from admin", token.UserId, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public bool DeleteUser(int id, out string error)
		{
			error = string.Empty;
			try
			{
				using (var context = new lfeAuthorEntities())
				{
					context.sp_ADMIN_DeleteUser(id);
				}

				return true;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("Delete user",ex,id,CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		#region settings
		public void UpdatePassword(string email, string password)
		{
			var pwdToken = WebSecurity.GeneratePasswordResetToken(email);

			WebSecurity.ResetPassword(pwdToken, password);
		}

		public bool IsPayoutTypeDefined(int userId)
		{
			try
			{
				var entity =  UserRepository.GetById(userId);
				
				return entity !=null &&  entity.PayoutTypeId != null;
			}
			catch (Exception ex)
			{
				Logger.Error("IsPayoutTypeDefined",ex,userId,CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public UserPayoutSettingsDTO GetPayoutSettings(int userId)
		{
			try
			{
				var userEntity = UserRepository.GetById(userId);
				
				if (userEntity == null)
				{
					return new UserPayoutSettingsDTO
					{
						IsValid = false
						,Message = "User entity not found"
					};
				}

				var type = userEntity.PayoutTypeId !=null ? Utils.ParseEnum<BillingEnums.ePayoutTypes>(userEntity.PayoutTypeId.ToString()) : BillingEnums.ePayoutTypes.PAYPAL;

				return new UserPayoutSettingsDTO
				{
					IsValid         = true
					,PayoutType     = type
					,Email          = string.IsNullOrEmpty(userEntity.PaypalEmail) ? userEntity.Email : userEntity.PaypalEmail
					,BillingAddress = userEntity.PayoutAddressID !=null ? UserAddressRepository.GetUserAddresses(null,userEntity.PayoutAddressID).FirstOrDefault().AddressEntity2BillingAddressDto() : new BillingAddressDTO()
				};
			}
			catch (Exception ex)
			{
			 
				Logger.Error("Get payout setting",userId,ex,CommonEnums.LoggerObjectTypes.UserAccount); 
				return new UserPayoutSettingsDTO
				{
					IsValid = false
					,Message = Utils.FormatError(ex)
				};
			}
		}

		public bool SavePayoutSetting(UserPayoutSettingsDTO token, out int? addressId, out string error)
		{
			addressId = null;
			try
			{

				var userEntity = UserRepository.GetById(token.UserId);

				if (userEntity == null)
				{
					error = "User entity not found";
					return false;
				}

				switch (token.PayoutType)
				{
				   case BillingEnums.ePayoutTypes.PAYPAL:
						if (string.IsNullOrEmpty(token.Email))
						{
							error = "Email required";
							return false;        
						}

						userEntity.PaypalEmail = token.Email;
						break;
				   case BillingEnums.ePayoutTypes.CHEQUE:
						var billingAddressDto    = token.BillingAddress;
						billingAddressDto.UserId = token.UserId;
						var addressSaved = SaveUserBillingAddress(ref billingAddressDto, out error);
						if (!addressSaved) return false;
						userEntity.PayoutAddressID = billingAddressDto.AddressId;
						addressId = userEntity.PayoutAddressID;
						break;
					default:
						error = "Unknown payout type";
						return false;
				}

				userEntity.PayoutTypeId = (byte) token.PayoutType;

				return UserRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);                
				Logger.Error("save payout setting",token.UserId,ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public void UpdateUserFbUserId(int userId, string providerUserId)
		{
			try
			{
				var entity = UserRepository.GetById(userId);
				
				if (entity == null)
				{
					Logger.Warn("Update Fb UserId::User entity not found::" + userId + "::" + providerUserId);
					return;
				}

				entity.FacebookID = providerUserId;
				entity.LastModified = DateTime.Now;
                //TODO add event to provision
                string error;
				if(UserRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return;

				Logger.Warn("Update Fb UserId::Update User entity failed::" + userId + "::" + providerUserId + "::" + error);

			}
			catch (Exception ex)
			{
				Logger.Error("UpdateFbId::" + userId + "::" + providerUserId, ex, CommonEnums.LoggerObjectTypes.UserAccount);               
			}
		}

		public UserInfoDTO FindFbUser(long? fbUserId, string email, out string error)
		{
			error = string.Empty;
			try
			{
				var entity = UserRepository.Get(x => x.Email == email);
				if (entity != null)
				{
					return entity.Entity2UserInfoDto();
				}

				if (fbUserId == null) return null;

				entity = UserRepository.Get(x => x.FacebookID == fbUserId.ToString());
				
				if (entity != null)
				{
					return entity.Entity2UserInfoDto();
				}
				return null;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("FindFbUser::" + email + "::" + fbUserId, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}

		public bool UpdateRefUserId(string email, int userId, out string error)
		{
			error = string.Empty;
			try
			{
				var entity = UserProfileRepository.Get(x => x.Email == email);
				if (entity == null)
				{
					error = "user profile entity not found";
					return false;
				}

				entity.RefUserId = userId;

				UserProfileRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("update refuserid" + email, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}
		

		#endregion
		#endregion

		#region login and registration services
		public bool SaveLogin(LoginLogoutLogToken token, out string error)
		{
			try
			{
				var sessionList = UserLoginsRepository.GetMany(x => x.NetSessionId == token.SessionId);
				if (sessionList.Any())
				{
					error = "record with this sessionId already exists";
					return false;
				}
				UserLoginsRepository.Add(token.LoginLog2USER_Logins());
				return UserLoginsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception e)
			{
				Logger.Error("RegisterLogin", e, CommonEnums.LoggerObjectTypes.UserAccount);
				error = FormatError(e);
				return false;
			}
		}

		public bool SaveLogout(LoginLogoutLogToken token, out string error)
		{
			try
			{
				var sessionList = UserLoginsRepository.GetMany(x => x.NetSessionId == token.SessionId && x.UserId == token.UserId).ToList();
				if (sessionList.Any())
				{
					var sessionRecord         = sessionList[sessionList.Count - 1];
					sessionRecord.SignOutDate = DateTime.Now;

					UserLoginsRepository.Update(sessionRecord);
					
					return UserLoginsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
				}
				error = "corresponding login record was not found";
				return false;
			}
			catch (Exception e)
			{
				Logger.Error("RegisterLogout", e, CommonEnums.LoggerObjectTypes.UserAccount);
				error = FormatError(e);
				return false;
			}
		}

		/// <summary>
		/// register new user , send email when confirmation required 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="userId"></param>
		/// <param name="confirmationToken"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool RegisterNewUser(RegisterDTO token, out int? userId, out string confirmationToken, out string error)
		{
			confirmationToken = string.Empty;
			userId = null;
			try
			{
				if (!token.Email.IsValidEmail())
				{
					error =  "invalid email format";
					return false;
				}

				if (string.IsNullOrEmpty(token.FirstName))
				{
					error = "first name required";
					return false;
				}

				if (string.IsNullOrEmpty(token.LastName))
				{
					error = "last name required";
					return false;
				}

				var user = CreateLfeUser(token, out error);

				if (user == null) return false;

				userId = user.UserId;

				var accountCrerated = CreateUser(new RegisterDTO
														{
															Email = token.Email
															,Password = token.Password
														}
														,user.UserId
														,out confirmationToken
														,out error
														,updateRoles: false
														// ReSharper disable once RedundantArgumentName
														,requiredConfirmation: token.RequiredConfirmation);

				if (!accountCrerated) return false;

				return token.RequiredConfirmation ? SendActivationEmail(new UserDTO
																				{
																					Email = user.Email
																					,FullName = user.FullName
																					,UserId = user.UserId
																				}
																				, confirmationToken
																				, out error
																				// ReSharper disable once RedundantArgumentName
																				, parentWindowURL: token.ParentWindowURL) :
													SendRegistrationWelocmeEmail(new UserDTO
																				{
																					Email = user.Email
																					,FullName = user.FullName
																					,UserId = user.UserId
																				}
																				, out error
																				// ReSharper disable once RedundantArgumentName
																				, parentWindowURL: token.ParentWindowURL);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("register user",ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		/// <summary>
		/// check login credentials and create user profile (account) for old lfe users
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="error"></param>        
		/// <returns></returns>
		public bool CreateOrUpdateLfeAccountAndLoginUser(string email, string password, out string error)
		{
			error = string.Empty;

			//or user registered and credentials are valid, or if it's old login => convert to new and try login again
			var isLogged =  WebSecurity.Login(email, password) || (VerifyOldLogin(email, password) && WebSecurity.Login(email, password));

			if(isLogged) UpdateLastLoginDate(email);

			return isLogged;
		}

		/// <summary>
		/// create LFE user record and user profile record on external provider login
		/// </summary>
		/// <param name="result"></param>
		/// <param name="error"></param>
		/// <param name="userId"></param>
		/// <param name="trackingId"></param>
		/// <param name="registrationSource"></param>
		/// <param name="hostName"></param>
		/// <returns></returns>
		public bool RegisterUserExternalLogin(AuthenticationResult result, out string error, out int? userId, string trackingId, CommonEnums.eRegistrationSources registrationSource = CommonEnums.eRegistrationSources.LFE, string hostName = null)
		{
			userId = null;
			try
			{
				#region TEMP , moving to SMP logic
				//check if user already exists as LFE user
				var user = GetUserByFacebookId(result.ProviderUserId);
				if (user != null)
				{
					//create oAuth account
					userId = user.Id;
					return CreateOrUpdateUser(result.UserName, user.Id, out error, updateRoles: true);
				}
				#endregion

				//register lfe user profile                  
				user = RegisterFacebookUser(result.ExtraData[FbEnums.eFbResultFields.email.ToString()],
																 result.ExtraData[FbEnums.eFbResultFields.id.ToString()],
																 result.ExtraData[FbEnums.eFbResultFields.first_name.ToString()],
																 result.ExtraData[FbEnums.eFbResultFields.last_name.ToString()],
																 result.ExtraData[FbEnums.eFbResultFields.birthday.ToString()],
																 result.ExtraData[FbEnums.eFbResultFields.gender.ToString()],
																 trackingId,
																 registrationSource,
																 hostName,
																 out error);

				if (user == null) return false;

				//register membership user profile
				// ReSharper disable once RedundantArgumentDefaultValue
				var success = CreateOrUpdateUser(result.UserName, user.Id, out error, updateRoles: false);

				userId = success ? user.Id : (int?)null;

				return success;

			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}
		}

		/// <summary>
		/// create account for social login
		/// </summary>
		/// <param name="result"></param>
		/// <param name="email"></param>
		/// <param name="error"></param>
		/// <param name="trackingId"></param>
		/// <param name="registrationSource"></param>
		/// <param name="hostName"></param>
		/// <returns></returns>
		public bool CreateOrUpdateExternalAccountAndLoginUser(AuthenticationResult result, out string email, out string error, string trackingId, CommonEnums.eRegistrationSources registrationSource, string hostName)
		{
			email = string.Empty;
			try
			{
				var isRegistered = CreateOrUpdateExternalAccount(result, out email, out error, trackingId,registrationSource, hostName);

				if (!isRegistered) return false;

				//try login again
				if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, IS_PERSISTENT_COOKIE))
				{
					//try update FB long live access token
					if (result.Provider == "facebook") UpdateUserFbAccessToken(result.ProviderUserId, result.ExtraData["access_token"]);

					UpdateLastLoginDate(email);

					return true;
				}

				error = "Unsuccessful " + result.Provider + " login";
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}
		}

		public bool CreateOrUpdateExternalAccount(AuthenticationResult result, out string email, out string error, string trackingId, CommonEnums.eRegistrationSources registrationSource, string hostName)
		{
			error = string.Empty;
			email = string.Empty;
			try
			{
				if (!result.IsSuccessful)
				{
					error = "Unsuccessful " + result.Provider + " login";
					return false;
				}

				email = result.UserName;

				//1. check if user exists
				var isUserRegistered = OAuthWebSecurity.GetUserName(result.Provider, result.ProviderUserId) != null;

				if (!isUserRegistered)
				{
					int? userId;
					if (!RegisterUserExternalLogin(result, out error, out userId, trackingId,registrationSource, hostName)) return false;

					var userToken = FindUserByEmail(email);

					if (userToken != null) SendRegistrationWelocmeEmail(userToken, out error);

				}

				//update oAuth account
				OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, result.UserName);

				return true;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				return false;
			}
		}

		/// <summary>
		/// verify old login credentials
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public bool VerifyOldLogin(string email, string password)
		{
			try
			{
				var user = VerifyUserOldLogin(email, password);

				if (user == null) return false;

				//create new user
				string error;
				string confirmationToken;

				return CreateUser(new RegisterDTO  {
														Email = email
														,Password = password
														,RequiredConfirmation = false
													}
													, user.Id
													, out confirmationToken
													, out error
													// ReSharper disable once RedundantArgumentDefaultValue
													, updateRoles: true); //updates roles
			}
			catch (Exception)
			{
				return false;
			}
		}
		#endregion

		#region forgotten password

		private bool ConfirmAccount(string email,out string error)
		{
			error = string.Empty;
			try
			{
				var profile = UserProfileRepository.Get(x => x.Email == email);
				if (profile == null)
				{
					error = "User profile not found";
					return false;
				}

				using (var context = new lfeAuthorEntities())
				{
					var membership = context.webpages_Membership.SingleOrDefault(x => x.UserId == profile.UserId);

					if (membership == null)
					{
						error = "User membership record not found";
						return false;
					}

					membership.IsConfirmed = true;
					context.SaveChanges();
					
				}

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}
		}
		public bool SendResetPasswordEmail(string email, out string error, string parentWindowURL = null)
		{
			error = string.Empty;

			try
			{
				var entity = UserRepository.Get(x => x.Email == email);

				if (entity == null)
				{
					error = "This mail address was not found. Please register";
					return false;
				}

				//1. check if login is converted
				var isExists = WebSecurity.UserExists(email);

				if (!isExists)
				{
					// check if social login
					if (!string.IsNullOrEmpty(entity.FacebookID))
					{
						error = "You are registered with Facebook login";
						return false;
					}

					string confirmationToken;
					var isRegistered = CreateUser(new RegisterDTO  {
														Email = email
														,Password = DEFAULT_SIMPLE_PASSWORD
														,RequiredConfirmation = false
													}
													, entity.Id
													, out confirmationToken
													, out error
													// ReSharper disable once RedundantArgumentDefaultValue
													, updateRoles: true);

					if (!isRegistered) return false;
				}

				if (OAuthWebSecurity.GetAccountsFromUserName(email).Any())
				{
					error = "You are registered with Facebook login";
					return false;
				}

				//check state
				if (!WebSecurity.IsConfirmed(email))
				{
					if (!ConfirmAccount(email, out error)) return false;
				}

				//2. create password reset token
				// ReSharper disable once RedundantArgumentDefaultValue
				var passwordResetToken = WebSecurity.GeneratePasswordResetToken(email, 60 * 24);


				//3. save reset password message
				var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

				var resetPwdHref = url.Action("ResetPassword", "Account", new { area = "", key = passwordResetToken }, HttpContext.Current.Request.Url.Scheme);

				if (!string.IsNullOrEmpty(parentWindowURL)) resetPwdHref += "&returnUrl=" + parentWindowURL.OptimizedUrl();


				var token = new EmailForgottenPasswordToken
				{
					UserID            = entity.Id
					,ToEmail          = entity.Email
					,FullName         = entity.FirstName + " " + entity.LastName
					,ResetPasswordURL = resetPwdHref
					,SentDate         = DateTime.Now
				};

				long emailId;

				_emailServices.SaveResetPasswordMessage(token, out emailId,out error);

				//4. send email
				return emailId >= 0 && _amazonEmailWrapper.SendEmail(emailId, out error);
			}
			catch (Exception ex)
			{
				Logger.Error("send reset password email",ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

	   
		#endregion

		#region activation email
		public bool SendActivationEmail(UserDTO user,string confirmationToken,out string error, string parentWindowURL = null)
		{
			error = string.Empty;

			try
			{
				var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

				var activationLinkHref = url.Action("RegisterConfirm", "Account", new {area="", key = confirmationToken }, HttpContext.Current.Request.Url.Scheme);

				if(!string.IsNullOrEmpty(parentWindowURL))  activationLinkHref +=  "&returnUrl=" + parentWindowURL.OptimizedUrl();

				var token = new EmailRegistrationToken
				{
					UserID         = user.UserId
					,ToEmail       = user.Email
					,FullName      = user.FullName
					,ActivationURL = activationLinkHref
					,SentDate      = DateTime.Now
				};

				long emailId;

				_emailServices.SaveRegistrationActivationMessage(token, out emailId);

				return emailId >= 0 && _amazonEmailWrapper.SendEmail(emailId, out error);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("send register confirmation email",ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}
		}

		public bool SendRegistrationWelocmeEmail(UserDTO user,out string error, string parentWindowURL = null)
		{
			error = string.Empty;

			try
			{
				var token = new EmailRegistrationToken
				{
					UserID         = user.UserId
					,ToEmail       = user.Email
					,FullName      = user.FullName
					,SentDate      = DateTime.Now
				};

				long emailId;

				_emailServices.SaveRegistrationMessage(token, out emailId);

				return emailId >= 0 && _amazonEmailWrapper.SendEmail(emailId, out error);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("send register confirmation email",ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return false;
			}


		}
		#endregion

		#region billing 
		public List<PaymentInstrumentDTO> GetUserSavedCardsLOV(int userId)
		{
			try
			{
				var cards = UserPaymentInstrumentsRepository.GetMany(x => x.UserId == userId && x.PaymentMethodId==(short)BillingEnums.ePaymentMethods.Credit_Card && x.IsActive).ToList();//.Select(x => x.Entity2PaymentInstrumentDto()).ToList();

				var apiContext = PayPalConfiguration.GetAPIContext();

				var list = new List<PaymentInstrumentDTO>(); // (from card in cards let cc = CreditCard.Get(apiContext, card.PaypalCcToken) select cc.RectApiCreditCard2PaymentInstrumentDto(card.InstrumentId, card.DisplayName)).ToList();

				foreach (var card in cards)
				{
					try
					{
						var cc = CreditCard.Get(apiContext, card.PaypalCcToken);

						if (cc.state.ToLower() != "ok") continue;

						list.Add(card.Entity2PaymentInstrumentDto());
					}
					catch (Exception)
					{/**/}
				}

				return list.ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get user payment instruments", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<PaymentInstrumentDTO>();
			}
		}
		
		public List<BillingAddressViewToken> GetUserBillingAddresses(int userId)
		{
			try
			{
				return UserAddressRepository.GetUserAddresses(userId, null).Select(x => x.AddressEntity2BillingAddressDto()).OrderByDescending(x=>x.IsDefault).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get user billing addresses", ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<BillingAddressViewToken>();
			}
		}

		public BillingAddressDTO GetBillingAddress(int addressId)
		{
			try
			{
				return UserAddressRepository.GetUserAddresses(null,addressId).FirstOrDefault().AddressEntity2BillingAddressDto();
			}
			catch (Exception ex)
			{
				Logger.Error("get billing address::" + addressId, ex, CommonEnums.LoggerObjectTypes.Billing);
				return new BillingAddressDTO();
			}
		}

		public bool SaveUserBillingAddress(ref BillingAddressDTO address, out string error)
		{
			try
			{
				var userId = address.UserId;
				var isFirst = !UserAddressRepository.IsAny(x => x.UserId == userId);

				USER_Addresses entity;
				if (address.AddressId < 0)
				{
					address.IsDefault = isFirst;
					entity = address.AddressDto2UserAddressEntity();
					UserAddressRepository.Add(entity);
					var saved = UserAddressRepository.UnitOfWork.CommitAndRefreshChanges(out error);
					if (!saved) return false;

					address.AddressId = entity.AddressId;
					return true;
				}

				entity = UserAddressRepository.GetById(address.AddressId);

				if (entity == null)
				{
					error = "address entity not found";
					return false;
				}

				entity.UpdateAddressEntity(address);

				return UserAddressRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save user billing address", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}
		#endregion

		#region refund program
		public bool AddAuthorToRefundProgram(int userId, out string error)
		{
			try
			{
				var userEntity = UserRepository.GetById(userId);
				var refundEntity = GetRefundProgramLastRevision(userId);
				var isConsistent = IsConsistentRefundRecords(userEntity, refundEntity);
				if (!isConsistent)
				{
					error = "inconsistent data";
					return false;
				}

				if (!userEntity.JoinedToRefundProgram)
				{
					userEntity.JoinedToRefundProgram = true;
					UserRepository.Update(userEntity);
					if (!UserRepository.UnitOfWork.CommitAndRefreshChanges(out error))
						return false;

					refundEntity = new USER_RefundProgramRevisions
					{
						FromDate = DateTime.Now,
						UserId = userId
					};
					
					UserRefundProgramRevisionsRepository.Add(refundEntity);
					
					if(!UserRefundProgramRevisionsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;




					var email = new EmailBaseToken
					{
						UserID = userEntity.Id
						,FullName   = userEntity.FirstName + " " + userEntity.LastName
						,ToEmail    = userEntity.Email
					};

					long emailId;
					_emailServices.SaveJoinRefundProgramMessage(email, out emailId);

					if (emailId < 0) return true;

					_amazonEmailWrapper.SendEmail(emailId, out error);

					return true;
				}

				error = "author already registered to refund program";
				return false;
			}
			catch(Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("add author to refund program", userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return false;
			}
		}

		public bool RemoveAuthorFromRefundProgram(int userId, out string error)
		{
			try
			{
				var userEntity = UserRepository.GetById(userId);
				var refundEntity = GetRefundProgramLastRevision(userId);
				var isConsistent = IsConsistentRefundRecords(userEntity, refundEntity);
				if (!isConsistent)
				{
					error = "inconsistent data";
					return false;
				}

				if (userEntity.JoinedToRefundProgram)
				{
					userEntity.JoinedToRefundProgram = false;
					UserRepository.Update(userEntity);
					if (!UserRepository.UnitOfWork.CommitAndRefreshChanges(out error))
						return false;

					refundEntity.ToDate = DateTime.Now;
					UserRefundProgramRevisionsRepository.Update(refundEntity);
					return UserRefundProgramRevisionsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
				}

				error = "author is not registered to refund program";
				return false;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("remove author from refund program", userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return false;
			}
		}

		public AuhorRefundProgramDTO GetAuthorRefundProgramStatus(int userId)
		{
			var token = new AuhorRefundProgramDTO();
			try
			{
				var userEntity = UserRepository.GetById(userId);
				var refundEntity = GetRefundProgramLastRevision(userId);
				token.JoinedToRefundProgram = userEntity.JoinedToRefundProgram;
				token.IsValid = IsConsistentRefundRecords(userEntity, refundEntity);

				if (token.IsValid)
					return token;

				token.Error = "inconsistent data";
				return token;
			}
			catch (Exception ex)
			{
				Logger.Error("get author refund program status", userId, ex, CommonEnums.LoggerObjectTypes.Author);
				token.Error = FormatError(ex);
				return token;
			}
		}

		#endregion
		#endregion

		#region IUserEventLoggerServices implementation
		#region  interface implementation
		public bool Report(ReportToken token)
		{
			return WriteEventRecord(token.UserId, token.EventType, token.NetSessionId, token.AdditionalMiscData, token.TrackingID, token.CourseId, token.BundleId, token.BcId, token.HostName);
		}
		#endregion
		#endregion

		#region IUserNotificationServices implementation
		public bool SaveNotification(int userId, long? messageId, out string error)
		{
		  return _SaveNotification(userId,messageId,out error);
		}

		public List<UserNotificationDTO> GetUserNotifications(int userId)
		{
			try
			{
				return UserNotificationRepository.GetUserNotifications(userId).Select(x => x.Entity2UserNotificationDto()).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("Get user notifications",userId,ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return new List<UserNotificationDTO>();
			}
		}

		public UserNotificationDTO GetNotificationToken(int notifId)
		{
			try
			{
				return UserNotificationRepository.GetUserNotification(notifId).Entity2UserNotificationDto();
			}
			catch (Exception ex)
			{
				Logger.Error("Get user notification", notifId, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}

		public int GetUserUnreadNotificationsCount(int userId)
		{
			return UserNotificationRepository.GetUserNotifications(userId).Count(x => !x.IsRead);
		}

		public void UpdateUserNotificationStatus(int userId)
		{
			UserNotificationRepository.UpdateUserNotificationStatus(userId);
		}
		#endregion

		#region IAuthorAdminServices implementation
		#region private helpers
		private bool IsConsistentRefundRecords(Users userEntity, USER_RefundProgramRevisions refundEntity)
		{
			return (userEntity.JoinedToRefundProgram && refundEntity != null && refundEntity.ToDate == null) ||
				   (!userEntity.JoinedToRefundProgram && (refundEntity == null || refundEntity.ToDate != null));
		}
		private USER_RefundProgramRevisions GetRefundProgramLastRevision(int userId)
		{
			var list = UserRefundProgramRevisionsRepository.GetMany(x => x.UserId == userId).OrderByDescending(x => x.RevisionId);
			return list.Any() ? list.First() : null;
		}
		private static string GetUserVideoListCacheKey(int userId)
		{
			return $"{"AVL"}:{userId}";
		}
		#endregion

		/// <summary>
		/// Find users by id/status/type
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="status"></param>
		/// <param name="type"></param>
		/// <returns>enumerable UserViewDTO</returns>
		public List<UserViewDto> FindUsers(int? userId, UserEnums.eUserStatuses? status, UserEnums.eUserTypes? type)
		{
			return UserRepository.FindUsers(userId, (int?)status, (int?)type).Select(user => user.Entity2UserViewDTO()).ToList();
		}

		/// <summary>
		/// get author transactions by period
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="periodKind"></param>
		/// <returns></returns>
		public List<OrderLineDTO> GetAuthorSales(int userId, ReportEnums.ePeriodSelectionKinds periodKind)
		{
			try
			{
				var dates = PeriodSelection2DateRange(periodKind);
				// ReSharper disable once RedundantArgumentName
				return SearchOrderLines(dates.@from, dates.to, sellerId: userId, buyerId: null, storeOwnerId: null, courseId: null, bundleId: null, storeId: null, lineType: null).Select(x => x.Entity2OrderLineDto()).ToList(); 
					//UserRepository.GetAuthorSales(userId, dates.@from, dates.to,null,null).Select(x => x.Entity2OrderLineDto()).OrderByDescending(x => x.OrderDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get author order lines", userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return new  List<OrderLineDTO>();
			}
		}

		public List<OrderLineDTO> GetAuthorSales(int authorId, ReportEnums.ePeriodSelectionKinds periodKind, BillingEnums.eOrderLineTypes lineType, int? storeId)
		{
			try
			{
				var dates = PeriodSelection2DateRange(periodKind);
				return SearchOrderLines(dates.@from, dates.to, sellerId: authorId, buyerId: null, storeOwnerId: null, courseId: null, bundleId: null, storeId: storeId, lineType: lineType).Select(x => x.Entity2OrderLineDto()).ToList();  
					//UserRepository.GetAuthorSales(authorId, dates.@from, dates.to,lineType,storeId).Select(x => x.Entity2OrderLineDto()).OrderByDescending(x => x.OrderDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get author order lines", authorId, ex, CommonEnums.LoggerObjectTypes.Author);
				return new List<OrderLineDTO>();
			}
		}

		/// <summary>
		/// using in web store add by author tab
		/// </summary>
		/// <returns></returns>
		public List<BaseListDTO> GetAuthorsLOV(bool onlyPublished = false)
		{
			return UserRepository.GetAuthorsLOV(onlyPublished).Select(user => user.AuthorEntity2BaseListDto()).OrderBy(x=>x.name).ToList();
		}

		public List<SubscriberDTO> GetAuthorSubscribers(int userId, int? courseId = null)
		{
			try
			{
				return (courseId == null ? UserRepository.GetAuthorSubscribers(userId) : CourseRepository.GetCourseSubscribers((int)courseId)).Select(x => x.LearnerToken2SubscriberDto()).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get author subscribers", userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return new List<SubscriberDTO>();
			}
		}      

		#region videos

		/// <summary>
		/// get user videos from brightcove
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="useCache"></param>
		/// <returns></returns>
		public List<UserVideoDto> GetAuthorVideos(int userId, bool useCache = false)
		{
			try
			{
                //var cacheKey = GetUserVideoListCacheKey(userId);

                //if (useCache)
                //{
                //    var result = GetCachedListByKey<UserVideoDto>(cacheKey, CacheProxy);

                //    if (result != null) return result.ToList();
                //}

           
                var videos = UserVideosRepository.GetMany(x => x.UserId == userId).ToList();
                var bcVideos = videos.Select(x => x.VideoEntity2VideoDTO(userId, _GetVideoChapterUsage(x.BcIdentifier))).ToList(); 

			
                foreach (var video in bcVideos)
			    {
			        if (string.IsNullOrEmpty(video.identifier)) continue;

			        var token = GetVideoInfoToken(long.Parse(video.identifier));

			        if (!token.IsValid) continue;

			        if (!token.Renditions.Any())
			        {
                        video.videoUrl = string.Empty;
			            video.thumbUrl = DefaultVideoThumbUrl;
                        video.stillUrl = DefaultVideoThumbUrl;
			        }
			        else
			        {
                        var rendition = token.Renditions.OrderBy(x => x.EncodingRate).FirstOrDefault();

                        if (rendition == null) continue;

                        video.thumbUrl = token.ThumbUrl;
                        video.videoUrl = rendition.CloudFrontPath;
			        }
			        			        
			    }

                //if (!useCache) return bcVideos;

                //CacheProxy.Remove(cacheKey);
                //CacheProxy.Add(cacheKey, bcVideos, DateTimeOffset.Now.AddDays(1));

				return bcVideos;
			}
			catch (Exception ex)
			{
				Logger.Error("get author videos", ex, userId, CommonEnums.LoggerObjectTypes.Author);
				return new List<UserVideoDto>();
			}
		}

		public List<FileInterfaceLogDTO> GetAuthorFileInterfaceLogs(int userId)
		{
			try
			{
				var dates = PeriodSelection2DateRange(ReportEnums.ePeriodSelectionKinds.all);

				return S3FileInterfaceRepository.GetFileInterfaceReport(dates.from, dates.to, userId, null)
						.Select(x => x.LogToken2FileInterfaceLogDto())
						.OrderByDescending(x => x.FileId)
						.ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get author file interface log", ex, userId, CommonEnums.LoggerObjectTypes.EventLogs);
				return new List<FileInterfaceLogDTO>();
			}
		}

		public int GetAuthorVideosCount(int userId, bool useCache = false)
		{
			return GetAuthorVideos(userId, useCache).Count();
		}


	    /// <summary>
		/// get videos from interface table
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public List<UserVideoDto> GetAuthorUnporcessedVideos(int userId)
		{


			var interfaceStatuses = new[]
			{
				ImportJobsEnums.eFileInterfaceStatus.Waiting.ToString(),
				ImportJobsEnums.eFileInterfaceStatus.InProgress.ToString(),
				ImportJobsEnums.eFileInterfaceStatus.WaitingForBrightcoveUpload.ToString(),
				ImportJobsEnums.eFileInterfaceStatus.WaitingForBrightcoveSynch.ToString()
			};

			var bcVideos = S3FileInterfaceRepository.GetMany(x => x.UserId == userId && (interfaceStatuses.Contains(x.Status))).Select(x => x.InterfacedVideo2VideoDTO(userId, userId.UserId2Tag())).ToList();

			return bcVideos;
		}

		/// <summary>
		/// get video token for edit
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public UserVideoDto GetVideoToken(long identifier, int userId)
		{
            return _GetVideoToken(identifier,userId);            
		}

	    public VideoInfoToken GetVideoToken(long bcId)
	    {
	        return GetVideoInfoToken(bcId);
	    }
		/// <summary>
		/// Save uploaded to S3 file to interface table for future uploading to brightcove
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="fileName"></param>
		/// <param name="contentType"></param>
		/// <param name="size"></param>
		/// <param name="status"></param>
		/// <param name="fileId"></param>
		/// <param name="error"></param>
		/// <param name="eTag"></param>
		/// <param name="tags"></param>
		/// <returns></returns>
		public bool SaveAuthorS3File(int userId, string fileName, string eTag, string contentType, long? size, ImportJobsEnums.eFileInterfaceStatus status, out int fileId, out string error, string tags = null)
		{
			
			fileId = -1;
			try
			{
				var entity = fileName.FileName2FileInterfaceEntity(userId, eTag,null, contentType, size, status,tags);

				S3FileInterfaceRepository.Add(entity);

				if (!S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

				fileId = entity.FileId;

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error(error, userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return false;
			}
		}

		public bool SaveS3VideoFile(int userId, string fileName, string refId, string contentType, long? size, ImportJobsEnums.eFileInterfaceStatus status, string tags, out int fileId, out string error)
		{
			fileId = -1;
			try
			{
				var entity = fileName.FileName2FileInterfaceEntity(userId, null,refId,contentType, size, status, tags);

				S3FileInterfaceRepository.Add(entity);

				if(!S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
				
				fileId = entity.FileId;
				
				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error(error, userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return false;
			}
		}

        public bool SaveS3VideoFile(VideoUploadToken token,int userId,ImportJobsEnums.eFileInterfaceStatus status, out int fileId, out string error)
        {
            fileId = -1;
            error = string.Empty;

            try
            {
                var refId = userId.User2UploadRefId(token.bcId.ToString());

                if (S3FileInterfaceRepository.IsAny(x => x.BcIdentifier == token.bcId))
                {
                    var userS3FileInterface = S3FileInterfaceRepository.GetMany(x => x.BcIdentifier == token.bcId).OrderByDescending(x => x.AddOn).FirstOrDefault();
                    if (userS3FileInterface != null) fileId = userS3FileInterface.FileId;
                    return fileId>0;
                }

                var entity = token.VideoUploadToken2FileInterfaceEntity(userId,refId,status);

                S3FileInterfaceRepository.Add(entity);

                if (!S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                fileId = entity.FileId;

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error(error, userId, ex, CommonEnums.LoggerObjectTypes.Author);
                return false;
            }
        }

		public void UpdateS3InterfaceRecord(int fileId, ImportJobsEnums.eFileInterfaceStatus status, string error)
		{
			var entity = S3FileInterfaceRepository.GetById(fileId);

			if(entity == null) return;

			entity.Status   = status.ToString();
			entity.UpdateOn = DateTime.Now;
			if(!string.IsNullOrEmpty(error)) entity.Error    = error;

			S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges();
			
		}

        public bool UpdateS3InterfaceRecord(string refId, ImportJobsEnums.eFileInterfaceStatus status, out int fileId, out long bcId, out string error)
        {
            var entity = S3FileInterfaceRepository.Get(x=>x.BcRefId == refId);

            if (entity == null)
            {
                fileId = -1;
                bcId   = -1;
                error  = "entity not found";
                return false;
            }

            bcId            = entity.BcIdentifier ?? -1;
            fileId          = entity.FileId;
            entity.Status   = status.ToString();
            entity.UpdateOn = DateTime.Now;

            return S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges(out error);

        }

        public bool UpdateS3InterfaceRecord(long bcId, ImportJobsEnums.eFileInterfaceStatus status, out int fileId, out string error)
	    {
            var entities = S3FileInterfaceRepository.GetMany(x => x.BcIdentifier == bcId).OrderByDescending(x=>x.AddOn).ToList();

            if (!entities.Any())
            {
                fileId = -1;
                error = "entity not found";
                return false;
            }

            var entity = entities.First();
            if (entities.Count> 1)
            {
                //delete unused interface records

                for (var i = 1; i < entities.Count; i++)
                {
                    S3FileInterfaceRepository.Delete(entities[i]);
                }
            }

            if (entity != null)
            {
                fileId = entity.FileId;
                entity.Status = status.ToString();
                entity.UpdateOn = DateTime.Now;
                return S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }

            fileId = -1;
            error = "entity not found";
            return false;
	    }

	    public bool SaveUserVideoFromInterface(int fileId, int userId,out string error)
        {
            try
            {
                var interfaceEntity = S3FileInterfaceRepository.GetById(fileId);

                if (interfaceEntity == null)
                {
                    error = "Interface entity not found";
                    return false;
                }

                var bcId = interfaceEntity.BcIdentifier;

                if (UserVideosRepository.IsAny(x => x.BcIdentifier == bcId))
                {
                    error = "Video entity already exists";
                    return false;
                }
                
                var videoEntity = interfaceEntity.InterfaceRecord2UserVideoEntity(userId);

                UserVideosRepository.Add(videoEntity);

                return UserVideosRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                return false;
            }
            
        }



//		public void CallCopyToS3Proc(int userId,long bcId)
//		{
//			var paramters = new NameValueCollection
//			{
//				{"bcId", bcId.ToString()},
//                {"userId", userId.ToString()},
//				{"host", Utils.GetKeyValue("appHost")},
//                {"port", Utils.GetKeyValue("appPort")},				
//				{"path","/Upload/S3CopyCallback"}
//			};
//
//            DoHttpPost(NODE_BC_DOWNLOADER_URL, paramters);
//		}

        //public void HandleFtpCallback(long? id = null, string referenceId = null, string entity = null, string action = null, string error = null, string status = null)
        //{
        //    try
        //    {
        //        #region update interface record
        //        if (id == null)
        //        {
        //            Logger.Warn("FTP callback::id not supplied");
        //            return;
        //        }
        //        if (referenceId == null)
        //        {
        //            Logger.Warn("FTP callback::refid not supplied");
        //            return;
        //        }

        //        var fileEntity = S3FileInterfaceRepository.Get(x => x.BcRefId == referenceId);

        //        if (fileEntity == null)
        //        {
        //            Logger.Warn("FTP callback::refId " + referenceId + "::interface record not found");
        //            return;
        //        }

        //        var entityStatus = status != null && status.ToLower() == "success" ? ImportJobsEnums.eFileInterfaceStatus.WaitingForBrightcoveSynch : ImportJobsEnums.eFileInterfaceStatus.Failed;

        //        if (entityStatus != ImportJobsEnums.eFileInterfaceStatus.WaitingForBrightcoveSynch)
        //        {
        //            Logger.Warn("FTP callback ::refId " + referenceId + "::error " + error);
        //        }

        //        fileEntity.Status       = entityStatus.ToString();
        //        fileEntity.BcIdentifier = id;
        //        fileEntity.Error        = error;
        //        fileEntity.UpdateOn     = DateTime.Now;

        //        S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges(); 
        //        #endregion

        //        //#region wait for sync
        //        //var found = false;

        //        //while (found == false)
        //        //{
        //        //    var bcId = (long)id;
        //        //    try
        //        //    {

        //        //        var video = _brightcoveWrapper.FindVideoById(bcId);

        //        //        if (video != null && video.Id == bcId)
        //        //        {
        //        //            found = true;
        //        //            UpdateS3InterfaceRecord(fileEntity.FileId, ImportJobsEnums.eFileInterfaceStatus.Transferred, null);
        //        //            SaveUserVideo(video, fileEntity.UserId);
        //        //            Logger.Debug("Video is found in brightcove. Setting status changed to transferred  " + fileEntity.FilePath);
        //        //            CallCopyToS3Proc(fileEntity.UserId,bcId);
        //        //        }
        //        //        else
        //        //        {
        //        //            Thread.Sleep(1000);
        //        //        }
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        Logger.Error("FTP callback ::find video by Id::" + bcId, fileEntity.FileId, ex, CommonEnums.LoggerObjectTypes.ServiceUploader);
        //        //        Thread.Sleep(1000);
        //        //    }

        //        //} 
        //        //#endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Handle ftp callback::"+referenceId,ex,CommonEnums.LoggerObjectTypes.Author);
        //    }
        //}

		
		/// <summary>
		/// delete file waiting for upload , both from db and S3
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool DeleteWaitingVideo(int fileId, out string error)
		{
			error = string.Empty;

			try
			{
				var entity = S3FileInterfaceRepository.GetById(fileId);

				if (entity == null)
				{
					error = "entity not found";
					return false;
				}

				_s3Wrapper.RemoveFile(entity.FilePath);

				S3FileInterfaceRepository.Delete(entity);

				S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error(error, fileId, ex, CommonEnums.LoggerObjectTypes.Video);
				return false;
			}
		}

		public string GetInterfacedFileName(int fileId)
		{
			var entity = S3FileInterfaceRepository.GetById(fileId);

			return entity == null ? string.Empty : entity.FilePath;
		}

		public bool SaveVideo(UserVideoDto dto, out string error)
		{
			error = string.Empty;
			var cacheKey = GetUserVideoListCacheKey(dto.userId);

			try
			{
				if (string.IsNullOrEmpty(dto.identifier) || dto.identifier == "-1") //insert mode
				{
					if (dto.fileId == null || dto.fileId < 0)
					{
						error = "fileId missing";
						return false;
					}

					var entity = S3FileInterfaceRepository.GetById((int)dto.fileId);

					if (entity == null)
					{
						error = "interface file entity not found";
						return false;
					}

					entity.UpdateFileInterfaceEntity(dto.title, ImportJobsEnums.eFileInterfaceStatus.Waiting);

					S3FileInterfaceRepository.UnitOfWork.CommitAndRefreshChanges();

					//clear user videos cache
					CacheProxy.Remove(cacheKey);

					return true;
				}

				var bcId = long.Parse(dto.identifier);

			    var videoEntity = UserVideosRepository.Get(x => x.BcIdentifier == bcId);

			    if (videoEntity != null)
			    {
			        videoEntity.Name = dto.title;
                    videoEntity.ShortDescription = dto.title;
			        return UserVideosRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			    }

			    error = "video entity not found";

			    return false;

                //var updated = _brightcoveWrapper.UpdateVideoTitle((long)dto.identifier, dto.title, out error);

                //if (!updated) return false;

                ////clear user videos cache
                //CacheProxy.Remove(cacheKey);

                //return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save video "+ dto.identifier, ex, CommonEnums.LoggerObjectTypes.Video);
				return false;
			}
		}

		public bool SaveVideoThumb(long bcId,string fileName,Stream stream, out string error)
		{
			try
			{

                var video = UserVideosRepository.Get(x => x.BcIdentifier == bcId);

                if (video == null || video.UserId == null)
                {
                    error = "video not found on Brightcove";
                    return false;
                }

				var original = new Bitmap(stream);

				var msStream = new MemoryStream();

				var image = ImageHelper.LimitBitmapSize(original, VIDEO_THUMB_W, VIDEO_THUMB_H);

				image.Save(msStream, ImageFormat.Jpeg);

				msStream.Seek(0, SeekOrigin.Begin);


                var savedThumb = _s3Wrapper.UploadVideoImage(bcId.CombimeVideoPictureKey((int)video.UserId, CommonEnums.eVideoPictureTypes.Thumb), "image/jpg", msStream, out error);

                //var savedThumb = SaveVideoImage(new VideoImageDto
                //{
                //    identifier = bcId
                //    ,ImageName = "thumb_" + fileName
                //    ,_Stream = msStream
                //    ,Type = FileEnums.ImageType.Thumbnail

                //}, out error);

				msStream.Dispose();
				msStream.Flush();

				var msStream2 = new MemoryStream();

				image = ImageHelper.LimitBitmapSize(original, VIDEO_STILL_W,VIDEO_STILL_H);

				image.Save(msStream2, ImageFormat.Jpeg);

				msStream2.Seek(0, SeekOrigin.Begin);

                var savedStill = _s3Wrapper.UploadVideoImage(bcId.CombimeVideoPictureKey((int)video.UserId, CommonEnums.eVideoPictureTypes.Still), "image/jpg", msStream2, out error);

                //var savedStill = SaveVideoImage(new VideoImageDto
                //{
                //    identifier = bcId
                //    ,ImageName = "still_" + fileName
                //    ,_Stream = msStream2
                //    ,Type = FileEnums.ImageType.VideoStill

                //}, out error);

				msStream2.Dispose();
				msStream2.Flush();


				return savedThumb && savedStill;
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				Logger.Error("Save Video Thumb");
				return false;
			}
		}

		public bool DeleteVideo(long identifier, out string error)
		{
			try
			{
			    var video = UserVideosRepository.Get(x => x.BcIdentifier == identifier); //_brightcoveWrapper.FindVideoById(identifier);
			    int userId;

			    if (video?.UserId == null)
			    {
			        userId = CurrentUserId;
			    }
			    else
			    {
			        userId = (int) video.UserId;
                    UserVideosRepository.Delete(x => x.BcIdentifier == identifier);
                    UserVideosRepository.UnitOfWork.CommitAndRefreshChanges(out error);
                }
                
                
                var deleted = _s3Wrapper.RemoveVideoFolder(userId,identifier,out error);

				return deleted;                

			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error(error, identifier, ex, CommonEnums.LoggerObjectTypes.Video);
				return false;
			}
		}

        #region S3 services
        public bool SaveS3TranscoderResponse(string data, out string error)
	    {
           
	        var MSG_PREFIX         = "Parse elastic transcoder response:: ";

	        try
	        {
	            if (string.IsNullOrEmpty(data))
	            {
	                error = "response is null";
                    Logger.Warn(MSG_PREFIX + error);
	                return false;
	            }

                //var dict = JsSerializer.Deserialize<Dictionary<string, object>>(data);

                var serializerSettings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    ReferenceLoopHandling      = ReferenceLoopHandling.Serialize,
                    Formatting                 = Formatting.Indented
                };

                var t = JObject.Parse(data);

                var token = JsonConvert.DeserializeObject<Message>(t["Message"].ToString(), serializerSettings);

	            if (token.state != "COMPLETED")
	            {
                    error = "state is not completed";
                    Logger.Warn(MSG_PREFIX + error);
                    return false;
	            }

	            if (!token.outputs.Any())
	            {
	                error = "outputs not found";
                    Logger.Warn(MSG_PREFIX + error);
                    return false;
	            }

                var defaultRendition = token.outputs.FirstOrDefault(x => x.presetId == S3VideoUtils.ES3Presets.GEN_1080P.GetPresetIdByType());

	            if (defaultRendition == null)
	            {
	                error = "Default(Gen-1080P) not found";
                    Logger.Warn(MSG_PREFIX + error);
                    return false;
	            }

                //find USER_Videos entity
	            long bcId;
	            if (!long.TryParse(token.userMetadata.bcId, out bcId))
	            {
                    error = "bcId from userMetadata not found";
                    Logger.Warn(MSG_PREFIX + error);
                    return false;
	            }
	            
                var entity = UserVideosRepository.Get(x => x.BcIdentifier == bcId);
	            
                if (entity == null)
	            {
                    error = "user video entity not found";
                    Logger.Warn(MSG_PREFIX + error);
                    return false;
	            }

                entity.PlaylistUrl = token.playlists.Any() ? (token.playlists[0].status.ToLower() == "complete" ? token.playlists[0].name.CombineVideoiPlayListFileUrl(S3_UPLOAD_BUCKET_NAME,token.outputKeyPrefix) : "") : "";

	            long length;

	            if (long.TryParse(defaultRendition.duration, out length))
	            {
	                var videoLength = length*1000;//milliseconds
	                var duration = length.VideoLengthSeconds2Duration().Duration2HoursString();

	                entity.Length = videoLength;
	                entity.Duration = duration;
	            }
	            else
	            {
                    Logger.Warn(MSG_PREFIX + "duration for default rendition invalid");
	            }
	            if (!UserVideosRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                //create thumbs
	            if (!_s3Wrapper.CreateVideoThumbs(S3_UPLOAD_BUCKET_NAME, token.outputKeyPrefix, out error))
	            {
	                
                    Logger.Warn(MSG_PREFIX + "create video thumbs for " + bcId + " error::" + error);
	            }
	            
                //save renditions
	            var renditionsSaved = 0;
	            var totalRenditionsToSave = 0;
	            foreach (var rendition in token.outputs)
	            {
                    try
                    {
                        int w;
                        int h;

                        int.TryParse(rendition.width, out w);
                        int.TryParse(rendition.height, out h);

                        var type = rendition.presetId.GetTypeByPresetId();

                        string container;
                        long renditionId;

                        int encodingRate;

                        string rendId;
                        switch (type)
                        {
                            case S3VideoUtils.ES3Presets.GEN_1080P:
                            case S3VideoUtils.ES3Presets.GEN_720P:
                            case S3VideoUtils.ES3Presets.GEN_480P:
                            case S3VideoUtils.ES3Presets.GEN_360P:
                                container = "MP4";
                                rendId = Path.GetFileNameWithoutExtension(rendition.key.Substring(rendition.key.LastIndexOf(Convert.ToChar("_")) + 1));
                                int.TryParse(type.ToString().Replace("GEN_", "").Replace("P", ""),out encodingRate);
                                totalRenditionsToSave++;
                                break;
                            case S3VideoUtils.ES3Presets.HLS_TWO_M:
                            case S3VideoUtils.ES3Presets.HLS_ONE_HALF_M:
                            case S3VideoUtils.ES3Presets.HLS_ONE_M:
                            case S3VideoUtils.ES3Presets.HLS_600_K:
                            case S3VideoUtils.ES3Presets.HLS_400_K:
                                continue;
                            default:
                                Logger.Warn(MSG_PREFIX + "renditionId unknown type" + type + " for key " + rendition.key);
                                continue;

                        }

                        if (!long.TryParse(rendId, out renditionId))
                        {
                            Logger.Warn(MSG_PREFIX + "renditionId not found for key " + rendition.key);
                            continue;                            
                        }

                        var size = 0;

                        if (container == "MP4")
                        {
                            var meta = _s3Wrapper.GetS3FileMetaData(token.outputKeyPrefix + rendition.key,S3_UPLOAD_BUCKET_NAME);

                            if (meta != null)
                            {
                                size = (int)meta.ContentLength;
                            }    
                        }
                        
                        var renditionEntity = new USER_VideosRenditions
                        {
                            RenditionId    = renditionId,
                            VideoId        = entity.VideoId,
                            Location       = token.outputKeyPrefix,
                            S3Url          = $"{S3_ROOT_URL}{S3_UPLOAD_BUCKET_NAME}/{token.outputKeyPrefix}{rendition.key}",
                            CloudFrontPath = $"{S3_CLOUDFRONT_ROOT}{token.outputKeyPrefix}{rendition.key}",
                            DisplayName    = $"{rendition.key.Substring(rendition.key.LastIndexOf(Convert.ToChar("/")) + 1)}",
                            FrameWidth     = w,
                            FrameHeight    = h,
                            Size           = size,
                            EncodingRate   = encodingRate,
                            VideoDuration  = string.IsNullOrEmpty(rendition.duration) ? 0 : long.Parse(rendition.duration),
                            VideoContainer = container,
                            InsertDate     = DateTime.Now
                        };

                        UserVideosRenditionsRepository.Add(renditionEntity);

                        if (UserVideosRenditionsRepository.UnitOfWork.CommitAndRefreshChanges(out error))
                        {
                            renditionsSaved++;
                        }
                        else
                        {
                            Logger.Warn(MSG_PREFIX + "renditionId not saved key " + rendition.key + " with error " + error); 
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(MSG_PREFIX + "save rendition::" + rendition.key,ex,CommonEnums.LoggerObjectTypes.Video);
                    }
	            }

                if (renditionsSaved < totalRenditionsToSave)
	            {
                    Logger.Warn(MSG_PREFIX + " renditions saved partially " + bcId + " error::" + error);
	            }

                Logger.Info("Transcoding end successfully::" + renditionsSaved + " renditions created");

	            return true;
	        }
	        catch (Exception ex)
	        {
	            Logger.Error(MSG_PREFIX,ex,CommonEnums.LoggerObjectTypes.Video);
	            error = FormatError(ex);
	            return false;
	        }
	    }

	    public bool SaveS3TranscodeJob(int userId,string id, out string error)
	    {
	        try
	        {
	            if (string.IsNullOrEmpty(id))
	            {
	                error = "id required";
	                return false;
	            }

	            long bcId;
	            if (!long.TryParse(id, out bcId))
	            {
	                error = "invalid id";
	                return false;
	            }

	            var entity = UserVideosRepository.Get(x => x.UserId == userId && x.BcIdentifier == bcId);

	            if (entity == null)
	            {
	                error = "video entity not found";
	                return false;
	            }

                entity.LastTranscodeDate  = DateTime.Now;
                entity.UpdateDate = DateTime.Now;

	            return UserVideosRepository.UnitOfWork.CommitAndRefreshChanges(out error);

	        }
	        catch (Exception ex)
	        {
                Logger.Error("Save transcode job", ex, CommonEnums.LoggerObjectTypes.Video);
                error = FormatError(ex);
                return false;
	        }
	    }
        #endregion 
        #endregion

        #region dashboard services

        /// <summary>
		/// get number of author courses and bundles for dashboard
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public AuthorStatisticSummaryDTO GetAuthorStatistic(int userId)
		{
			try
			{
				var dto = new AuthorStatisticSummaryDTO(userId)
				{
					courses  = CourseRepository.GetMany(c => c.AuthorUserId == userId).Count(),
					bundles  = BundleRepository.GetMany(c => c.AuthorId == userId).Count(),
					stores   = _GetOwnerStores(userId).Count(),
					comments = _GetAuthorReviews(userId, ReportEnums.ePeriodSelectionKinds.all).Count()
				};


				//Parallel.Invoke(
				//    () =>
				//    {
				//        dto.courses = CourseRepository.GetMany(c => c.AuthorUserId == userId).Count();
				//    },
				//    () =>
				//    {
				//        dto.bundles = BundleRepository.GetMany(c => c.AuthorId == userId).Count();
				//    },
				//    () =>
				//    {
				//        dto.stores = _webStoreServices.GetOwnerStores(userId).Count();
				//    },
				//    () =>
				//    {
				//        dto.comments = _courseServices.GetAuthorReviews(userId, ReportEnums.ePeriodSelectionKinds.all).Count();
				//    });

				return dto;
			}
			catch (Exception ex)
			{
				Logger.Error("get course statistic for dashboard", userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return new AuthorStatisticSummaryDTO(userId);
			}
		}

		/// <summary>
		/// get data for sales chart on dashboard
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="periodSelectionKind"></param>
		/// <param name="groupBy"></param>
		/// <returns></returns>
		public List<SalesAnalyticChartDTO> GetSalesChartData(int userId, ReportEnums.ePeriodSelectionKinds periodSelectionKind, ReportEnums.eChartGroupping groupBy)
		{
			var dates = PeriodSelection2DateRange(periodSelectionKind);
			var trx = SearchOrderLines(dates.@from, dates.to, sellerId: userId, buyerId: null, storeOwnerId: null, courseId: null, bundleId: null, storeId: null, lineType: null).Select(x => x.Entity2OrderLineDto()).ToList(); 
				//UserRepository.GetAuthorSales(userId, dates.@from, dates.to,null,null).Select(x => x.Entity2OrderLineDto()).ToList();

			if (trx.Count == 0) return new List<SalesAnalyticChartDTO>();


			var curDate = trx.Min(x => x.OrderDate).AddDays(-1);
			var endDate = trx.Max(x => x.OrderDate);

			var datePoints = new List<DateTime>();

			while (curDate <= endDate)
			{
				datePoints.Add(curDate);

				curDate = curDate.AddDays(1);
			}

			var points = datePoints.ToList().Select(period => new SalesAnalyticChartDTO
			{
				date = period,
				total = trx.Where(x => x.OrderDate.Date == period.Date).Sum(t => t.TotalPrice)
			}).ToList();

			//var to = points.Sum(o => o.total);

			return points;            
		}

		#endregion

		#endregion
		
		#region IUserPortalServices implementation
		public List<OrderLineDTO> GetUserPurchases(int userId)
		{
			try
			{
				var dates = PeriodSelection2DateRange(ReportEnums.ePeriodSelectionKinds.all);
				return SearchOrderLines(dates.@from, dates.to, sellerId: null, buyerId: userId, storeOwnerId: null, courseId: null, bundleId: null, storeId: null, lineType: null).Select(x => x.Entity2OrderLineDto()).ToList(); 
					//UserRepository.GetUserPurchases(userId).Select(x => x.Entity2OrderLineDto()).OrderByDescending(x => x.OrderDate).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get user purchases", userId, ex, CommonEnums.LoggerObjectTypes.Author);
				return new List<OrderLineDTO>();
			}
		}

		public LearnerListItemDTO GetLearnerListItemDTO(int userId)
		{
			var entity = UserRepository.GetById(userId);

			return entity == null ? new LearnerListItemDTO() : entity.Entity2LearnerListItemDTO();
		}

		public List<CourseListDTO> GetLearnerCourses(int learnerId, int? userId)
		{
			return UserCourseRepository.GetLearnerCourses(learnerId, userId ?? -1).Select(x => x.Entity2CourseListDTO(x.Price,x.MonthlySubscriptionPrice)).ToList();
		}

		public List<CourseListDTO> GetAuthorCourses(int authorId, int? userId)
		{
			return UserRepository.GetAuthorCourses(authorId, userId ?? -1).Select(x => x.Entity2CourseListDTO(x.Price,x.MonthlySubscriptionPrice)).ToList();
		}

		public void UpdateCourseStateAndCreateStory(int courseId, int userId, int chapterId, int videoId, long? bcId = null, bool createStory = true)
		{
			try
			{
				var entity = UserCourseWatchStateRepository.Get(x => x.CourseId == courseId && x.UserId == userId);

				if (entity == null)
				{
					entity = new USER_CourseWatchState
					{
						UserId         = userId
						,CourseId      = courseId
						,LastChapterID = chapterId
						,LastVideoID   = videoId
						,LastViewDate  = DateTime.Now
						,AddOn         = DateTime.Now
						,CreatedBy     = CurrentUserId
					};   

					UserCourseWatchStateRepository.Add(entity);
				}
				else
				{
					entity.LastChapterID = chapterId;
					entity.LastVideoID   = videoId;
					entity.LastViewDate  = DateTime.Now;
					entity.UpdateDate    = DateTime.Now;
					entity.UpdatedBy     = CurrentUserId;
				}


				UserCourseWatchStateRepository.UnitOfWork.CommitAndRefreshChanges();

				//if (!createStory || bcId == null) return;

				//_brightcoveWrapper.FindVideoById((long)bcId);

				//save watch video story
			  //  _facebookServices.CreateUserFbStory(userId, courseId, FbEnums.eFbActions.view, videoId, additionalMsg: v.Name);
			}
			catch (Exception ex)
			{
				Logger.Error("update course state", chapterId, ex, CommonEnums.LoggerObjectTypes.Learner);
			}
		}

		public void CreateStoryView(int courseId, int userId, int chapterId, int videoId, long? bcId = null)
		{
			try
			{
				if (bcId == null) return;

				//var v = _brightcoveWrapper.FindVideoById((long)bcId);
			    var v = UserVideosRepository.Get(x => x.BcIdentifier == bcId);

				//save watch video story
				_facebookServices.CreateUserFbStory(userId, courseId, FbEnums.eFbActions.view, videoId, additionalMsg: v != null ? v.Name : "");
			}
			catch (Exception ex)
			{
				Logger.Error("Publish story view", chapterId, ex, CommonEnums.LoggerObjectTypes.Learner);
			}
		}

		public UserProfileDTO GetUserProfileDto(int id)
		{
			try
			{
				var entity = UserRepository.GetById(id);

				return entity != null ? entity.Entity2ProfileDto() : null;
			}
			catch (Exception ex)
			{
				Logger.Error("Get User profile DTO",id,ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}
		#endregion

		#region IWidgetUserServices implementation

		public List<WidgetItemListDTO> GetAuthorItems(int authorId,int? userId,string trackingId)
		{
			try
			{

				var currencyId = _GetStoreCurrencyByTrackingId(trackingId);

				var items = UserRepository.SearchUserItems(currencyId, authorId, null, CourseEnums.CourseStatus.Published).Select(item => item.Entity2WidgetItemListDto(userId != null && userId >= 0 && _itemServices.IsItemAccessAllowed4User(userId, item.ItemId, (byte) item.ItemTypeId),GetItemDefaultPrices(item.ItemId, (byte)item.ItemTypeId, currencyId))).ToList();

				return items;
			}
			catch (Exception ex)
			{
				Logger.Error("Get Author items::"+ authorId,ex,authorId,CommonEnums.LoggerObjectTypes.Widget);
				return new List<WidgetItemListDTO>();
			}
		}

		public List<WidgetItemListDTO> GetLearnerPurchasedItems(int learnerId, int? userId)
		{
			return UserCourseRepository.GetLearnerCourses(learnerId, userId ?? -1).Select(item => item.Entity2WidgetItemListDto(userId != null && userId >= 0 && _itemServices.IsItemAccessAllowed4User(userId, item.Id, (byte)BillingEnums.ePurchaseItemTypes.COURSE), GetStoreItemPrices(item.Id, (byte)BillingEnums.ePurchaseItemTypes.COURSE, DEFAULT_CURRENCY_ID))).ToList();
		}

		public UserProfileDTO GetAuthorProfileDto(int id)
		{
			try
			{
				var entity = UserRepository.GetById(id);

				return entity != null ? entity.Entity2ProfileDto() : null;
			}
			catch (Exception ex)
			{
				Logger.Error("Get User profile DTO", id, ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return null;
			}
		}
			 
		public bool SaveUserVideoStats(int userId, VideoStatsToken token, out Guid sessionId, out string error)
		{
			sessionId = Guid.Empty;
			error = string.Empty;
			try
			{
				USER_VideoStats entity;
				switch (token.action)
				{
					case UserEnums.eVideoActions.Play:
						if (token.bcId == null)
						{
							error = "Save video Play for " + userId + ":: BcId not supplied";
							Logger.Warn(error);
							return false;
						}

						entity = token.StatsToken2UserVideoStats(userId, (long) token.bcId);

						UserVideoStatsRepository.Add(entity);

						if(!UserVideoStatsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

						sessionId = entity.SessionId;

						return true;
					case UserEnums.eVideoActions.Stop:
						if (token.sessionId == null)
						{
							error = "Save video Stop for " + userId + ":: SessionId not supplied";
							Logger.Warn(error);
							return false;
						}
						entity = UserVideoStatsRepository.Get(x => x.SessionId == token.sessionId);

						if (entity == null)
						{
							error = "Stats Entity not found";
							Logger.Warn(error);
							return false;
						}

						entity.UpdateVideoStatsEntity(token);

						
						if(!UserVideoStatsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

						sessionId = (Guid) token.sessionId;

						return true;
				}

				error = "Incorrect action type";

				return false;
			}
			catch (Exception ex)
			{
				Logger.Error("save video stats", userId, ex, CommonEnums.LoggerObjectTypes.Video);
				return false;
			}
		}

		public bool UpdateVideoStatsReason(VideoStatsReasonToken token, out string error)
		{
			error = string.Empty;
			try
			{
				USER_VideoStats entity;
				if (token.sessionId == null)
				{
					error = "Update video stats reason ::" + token.sessionId + ":: SessionId not supplied";
					Logger.Warn(error);
					return false;
				}
				entity = UserVideoStatsRepository.Get(x => x.SessionId == token.sessionId);

				if (entity == null)
				{
					error = "Stats Entity not found";
					Logger.Warn(error);
					return false;
				}

				if (string.IsNullOrEmpty(token.reason))
				{
					error = "Reason required";
					Logger.Warn(error);
					return false;
				}

				switch (token.action)
				{
					case UserEnums.eVideoActions.Play:
						token.startReason = token.reason;
						break;
					case UserEnums.eVideoActions.Stop:
						token.endReason = token.reason;
						break;
					default:
						error = "Unknown action";
						return false;
				}

				entity.UpdateVideoStatsEntityReason(token);


				return UserVideoStatsRepository.UnitOfWork.CommitAndRefreshChanges(out error);

			}
			catch (Exception ex)
			{
				Logger.Error("update video stats reason::"+token.sessionId, ex, CommonEnums.LoggerObjectTypes.Video);
				return false;
			}
		}

	    public VideoInfoToken GetVideoRenditions(long bcId)
	    {
            return GetVideoInfoToken(bcId);
	    }

		#endregion

		#region IWixUserServices implementation
	   
		public string GetWixZombieTrackingId(Guid instanceId, int userId)
		{
			return "zombie_" + userId + "_" + instanceId;
		}

		public WidgetWebStoreDTO GetAndUpdateZombieStore(Guid instanceId, int userId)
		{
			try
			{
				var trackingID = GetWixZombieTrackingId(instanceId, userId);

				var webstore = WebStoreRepository.Get(x => x.TrackingID.ToLower() == trackingID.ToLower() && x.OwnerUserID == userId);

				if (webstore == null)
				{
					return null;
				}
				
				webstore.WixInstanceId = instanceId;
				webstore.TrackingID    = instanceId.ToString();

				WebStoreRepository.UnitOfWork.CommitAndRefreshChanges();

				var instanceIdStr = instanceId.ToString().ToLower();
				
				webstore = WebStoreRepository.Get(x => x.TrackingID.ToLower() == instanceIdStr && x.OwnerUserID == userId);

				return webstore.Entity2WidgetStoreDto();
			}
			catch (Exception ex)
			{
				Logger.Error("Get and update wix zombie store instanceId:" + instanceId + "; user:" + userId, ex, CommonEnums.LoggerObjectTypes.WebStore);
				return null;
			}
		}

		public UserDTO FindUserDtoByWixInstanceId(Guid wixInstaceId)
		{
			try
			{
				var webStore = WebStoreRepository.Get(x => x.WixInstanceId == wixInstaceId);

				return webStore == null ? null : UserViewRepository.Get(x => x.UserId == webStore.OwnerUserID).ViewEntity2UserDto();
			}
			catch (Exception ex)
			{
				Logger.Error("Find user by instanceId:" + wixInstaceId, ex, CommonEnums.LoggerObjectTypes.WebStore);
				return null;
			}
		}
		public bool DisconnectWixUser(int userId, Guid instanceId)
		{
			try
			{               
				var trackingID = GetWixZombieTrackingId(instanceId, userId);
				var instanceIdStr = instanceId.ToString().ToLower();


				var webstore = WebStoreRepository.Get(x => x.TrackingID.ToLower() == instanceIdStr && x.OwnerUserID == userId);

				if (webstore != null)
				{
					webstore.TrackingID = trackingID;
					webstore.WixInstanceId = null;

					WebStoreRepository.UnitOfWork.CommitAndRefreshChanges();
				}
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("disconnect wix user", userId, ex, CommonEnums.LoggerObjectTypes.WebStore);
				return false;
			}
		}

	  
		public int? FindSotreByInstanceId(int userId,Guid instanceId,out string error)
		{
			error = string.Empty;
			try
			{
				var storeEntity = WebStoreRepository.Get(x=>x.OwnerUserID==userId && (x.WixInstanceId!=null && x.WixInstanceId==instanceId));

			
				return storeEntity==null ? (int?) null : storeEntity.StoreID;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("find store by instanceId:" + instanceId + "; user:" + userId,ex,CommonEnums.LoggerObjectTypes.WebStore);
				return null;
			}
		}
		#endregion

		#region IPortalAdminUserServices implementation
	   
		public List<UserGridViewDto> GetUsers(int? profileUserId = null, int? userId = null, string name = null, string email = null)
		{
			try
			{
				if (profileUserId == null && userId == null && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email)) return UserLoginsViewRepository.GetAll().Select(x => x.LoginEntity2UserGridViewDto()).OrderBy(x => x.FullName).ToList();

				if (profileUserId != null)
				{
					var user = UserLoginsViewRepository.Get(x => x.Id == profileUserId).LoginEntity2UserGridViewDto();

					return new List<UserGridViewDto> { user };
				}

				if (userId != null)
				{
					var user = UserLoginsViewRepository.Get(x => x.UserId == userId).LoginEntity2UserGridViewDto();

					return new List<UserGridViewDto> { user };
				}

				if (!string.IsNullOrEmpty(email))
				{
					return UserLoginsViewRepository.GetMany(x => x.Email.ToLower().Contains(email.ToLower())).Select(x => x.LoginEntity2UserGridViewDto()).OrderBy(x => x.FullName).ToList();
				}

				if (!string.IsNullOrEmpty(name))
				{
					var  list =  UserLoginsViewRepository.GetMany(x => x.FirstName.ToLower().Contains(name.ToLower()) || x.LastName.ToLower().Contains(name.ToLower()) || x.Nickname.ToLower().Contains(name.ToLower())).ToList();
					return list.Select(x => x.LoginEntity2UserGridViewDto()).OrderBy(x => x.FullName).ToList();
				}
				
				//shouldn't arrive to this
				return new List<UserGridViewDto>();
			}
			catch (Exception ex)
			{
				
				Logger.Error("get users for admin",ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return new List<UserGridViewDto>();
			}
		}

		public List<UserGridViewDto> SearchUsers(int? userId, int? typeId, DateTime? logFrom, DateTime? logTo, DateTime? regFrom, DateTime? regTo, bool isGrp, int? roleId)
		{
			try
			{
				return UserRepository.SearchUsers(userId, typeId, logFrom, logTo, regFrom, regTo, isGrp, roleId).Select(x => x.LoginEntity2UserGridViewDto()).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("Search users",ex,CommonEnums.LoggerObjectTypes.UserAccount);
				return new List<UserGridViewDto>();
			}
		}

		public UserEditDTO GetUserEditDTO(int userId)
		{
			try
			{
				var entity = UserLoginsViewRepository.Get(x => x.UserId == userId);

				if (entity == null) return new UserEditDTO("user not found");

				var dto = entity.LoginEntity2UserEditDto();

				//Old login
				if (dto.UserProfileId < 0) return dto;

				var roles = _rolesProvider.GetRolesForUser(dto.Email);

				if (roles.Any()) dto.Roles = roles.ToList();

				return dto;

			}
			catch (Exception ex)
			{

				Logger.Error("get user for admin", ex, CommonEnums.LoggerObjectTypes.UserAccount);
				return new UserEditDTO("user not found");
			}
		}

		public UserStatisticToken GetUserStatisticToken(int userId)
		{
			try
			{
				return UserRepository.GetUserStatistic(userId).Entoty2UserStatisticToken();
			}
			catch (Exception ex)
			{
				Logger.Error("Get user statistics",userId,ex,CommonEnums.LoggerObjectTypes.UserAccount);

				return new UserStatisticToken
				{
					UserId = userId
				};
			}
		}
		public List<UserStatisticToken> GetUsersStatistic()
		{
			try
			{
				return UserRepository.GetUsersStatistic().Select(x=>x.Entoty2UserStatisticToken()).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("Get users statistics", ex, CommonEnums.LoggerObjectTypes.UserAccount);

				return new List<UserStatisticToken>();
			}
		}
		#endregion
	}    
}
