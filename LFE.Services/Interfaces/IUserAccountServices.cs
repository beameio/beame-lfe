using System;
using System.Collections.Generic;
using DotNetOpenAuth.AspNet;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Model;

namespace LFE.Application.Services.Interfaces
{
    public interface IUserAccountServices : IDisposable
    {
        string GetUserEmail(int id);
        UserInfoDTO TryFoundUserBySocialCredentials(string email, string providerUserId, CommonEnums.SocialProviders provider,out bool providerApproved, out string error);
        bool ConnectSocialLoginToLfeAccount(int userId, string uid, CommonEnums.SocialProviders provider, out string error);
        AccountSettingsDTO GetSettingsToken(int userId,out string error);
        bool UpdateAccountSettings(AccountSettingsDTO dto, out string error);
        bool UpdateAccountSettings(WizardAboutAuthorDTO dto, out string error);
        bool UpdateCommunicationSettings(AccountSettingsDTO dto, out string error);

        UserDTO CreateLfeUser(RegisterDTO token, out string error);
       // Users VerifyUserOldLogin(string email, string pass);
        Users GetUserByFacebookId(string facebookid);
        UserDTO FindUserByConfirmationKey(string key);
        FbResponse GetFbUserToken(string accessToken, out string error);
       // Users RegisterFacebookUser(FbResponse fbUser, out string error);

        Users RegisterFacebookUser( string email,
                                    string facebookId,
                                    string firstname,
                                    string lastname,
                                    string birthdate,
                                    string gender,
                                    string trackingId,
                                    CommonEnums.eRegistrationSources registrationSource,
                                    string hostName,
                                    out string error
                                    );
        List<UserBaseDTO> FindUsers(string text);

        //TODO change to long
        void UpdateUserFbAccessToken(string fbUid, string accessToken);

        //SMP
        bool CreateUser(RegisterDTO token, int? RefUserId, out string confirmationToken, out string error, bool updateRoles = false,bool requiredConfirmation = false);
        bool CreateOrUpdateUser(string email, int? RefUserId, out string error, bool updateRoles = false);
        UserDTO GetUserDataByEmail(string email, out string error);
        UserDTO FindUserByEmail(string email);
        bool SendActivationEmail(UserDTO user, string confirmationToken, out string error, string parentWindowURL = null);
        bool SendRegistrationWelocmeEmail(UserDTO user,out string error, string parentWindowURL = null);
        bool UpdateUser(UserEditDTO token, out string error);
        bool DeleteUser(int id, out string error);
        //login helpers
        bool VerifyOldLogin(string email, string password);
        bool RegisterNewUser(RegisterDTO token,out int? userId, out string confirmationToken, out string error);
        bool CreateOrUpdateLfeAccountAndLoginUser(string email, string password, out string error);
        bool RegisterUserExternalLogin(AuthenticationResult result, out string error, out int? userId, string trackingId, CommonEnums.eRegistrationSources registrationSource, string hostName = null);
        bool CreateOrUpdateExternalAccountAndLoginUser(AuthenticationResult result, out string email, out string error, string trackingId, CommonEnums.eRegistrationSources registrationSource, string hostName);
        bool CreateOrUpdateExternalAccount(AuthenticationResult result, out string email, out string error, string trackingId, CommonEnums.eRegistrationSources registrationSource, string hostName);

        bool SendResetPasswordEmail(string email, out string error, string parentWindowURL = null);

        //billing
        List<PaymentInstrumentDTO> GetUserSavedCardsLOV(int userId);
        List<BillingAddressViewToken> GetUserBillingAddresses(int userId);
        BillingAddressDTO GetBillingAddress(int addressId);
        bool SaveUserBillingAddress(ref BillingAddressDTO address, out string error);        

        bool IsPayoutTypeDefined(int userId);
        UserPayoutSettingsDTO GetPayoutSettings(int userId);

        bool SavePayoutSetting(UserPayoutSettingsDTO token,out int? addressId, out string error);

        //FB support
        void UpdateUserFbUserId(int userId, string providerUserId);
        UserInfoDTO FindFbUser(long? fbUserId, string email, out string error);

        //refund program
        bool AddAuthorToRefundProgram(int userId, out string error);
        bool RemoveAuthorFromRefundProgram(int userId, out string error);
        AuhorRefundProgramDTO GetAuthorRefundProgramStatus(int userId);

        //temp
        bool UpdateRefUserId(string email, int userId, out string error);
        void UpdatePassword(string email, string password);

        //login/logout
        bool SaveLogin(LoginLogoutLogToken token, out string error);
        bool SaveLogout(LoginLogoutLogToken token, out string error);
    }
}
