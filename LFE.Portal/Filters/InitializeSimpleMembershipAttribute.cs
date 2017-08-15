using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using LFE.Core.Enums;
using LFE.Portal.Models;
using WebMatrix.WebData;

namespace LFE.Portal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<UsersContext>(null);

                try
                {
                    using (var context = new UsersContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ( (IObjectContextAdapter)context ).ObjectContext.CreateDatabase();
                        }
                    }
                    
                    #region init first user
                    //if (!WebSecurity.Initialized)
                    //{
                    //    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "Email", autoCreateTables: true);
                    //}
                    //InitUserRoles();                 
                    #endregion
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }
        
            private static void InitUserRoles()
            {
                var roles = (SimpleRoleProvider)Roles.Provider;

                var array = (Enum.GetValues(typeof(CommonEnums.UserRoles)).Cast<CommonEnums.UserRoles>() ).ToArray();

                foreach (var userRole in array)
                {
                    if(userRole.ToString().Equals(CommonEnums.UserRoles.Unknown.ToString())) continue;

                    if (!roles.RoleExists(userRole.ToString()))
                    {
                        roles.CreateRole(userRole.ToString());
                    }
                }
            }

            private static void CreateUser(string username, string password, CommonEnums.UserRoles role)
            {
                var roles = (SimpleRoleProvider)Roles.Provider;

                var membership = (SimpleMembershipProvider)Membership.Provider;

                if (membership.GetUser(username, false) == null)
                {
                    membership.CreateUserAndAccount(username,password);
                }

                if (!roles.GetRolesForUser(username).Contains(role.ToString()))
                {
                    roles.AddUsersToRoles(new[] { username }, new[] { role.ToString() });
                } 
            }
        }
    }
}

