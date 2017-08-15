using LFE.DataTokens;
using LFE.Portal.Areas.WixEndPoint.Models;
using LFE.Portal.Helpers;
using LFE.Portal.Models;
using WebMatrix.WebData;

namespace LFE.Portal.Areas.Widget.Helpers
{
    public static class WidgetExtensions
    {

        public static SettingsViewToken ToDefaultSettingsToken(this string trackingId)
        {
            return new SettingsViewToken
            {
                IsShowBorder      = false,
                IsShowTitleBar    = true,
                IsTransparent     = false,
                BackgroundColor   = "#FFFFFF",
                FontColor         = "#000000",
                TabsFontColor     = "#006699",
                StoreId           = null,
                UniqueId          = "",
                TrackingID        = trackingId
            };
        }

        public static UserIndicatorViewModel GetUserIndicatorViewModel(this object obj)
        {
            var model = new UserIndicatorViewModel { IsLoggedIn = false };

            if (!WebSecurity.IsAuthenticated) return model;

            var user = model.CurrentUser();

            if (user == null) return model;

            model.IsLoggedIn = true;
            model.Id = user.UserId;
            model.Email = user.Email;
            model.DisplayName = user.FullName;
            model.LastLogin = user.LastLogin;

            return model;
        }
    }
}