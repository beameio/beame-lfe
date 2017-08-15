using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    //s3 file
    public interface IS3FileInterfaceRepository : IRepository<UserS3FileInterface>
    {
        IEnumerable<LOG_FileInterfaceToken> GetFileInterfaceReport(DateTime from, DateTime to, int? userId, ImportJobsEnums.eFileInterfaceStatus? status);
    }

    //email
    public interface IEmailMessageRepository : IRepository<EMAIL_Messages>
    {
        IEnumerable<EMAIL_NotificationMessageToken> GetMessageNotifications(long messageId);
        IEnumerable<LOG_EmailInterfaceToken> GetEmailInterfaceReport(DateTime from, DateTime to, int? userId, EmailEnums.eSendInterfaceStatus? status);
    }

    public interface IEmailTemplateRepository : IRepository<EMAIL_Templates>
    {

    }

    //fb
    public interface IFacebookPostRepository : IRepository<FB_PostInterface>
    {
        IEnumerable<LOG_FbPostInterfaceToken> GetFbPostInterfaceReport(DateTime from, DateTime to, int? userId, FbEnums.ePostInterfaceStatus? status);
    }

    //log table
    public interface ILogTableRepository : IRepository<LogTable>
    {
      IEnumerable<ADMIN_SystemLogToken> GetSystemLogs(DateTime from, DateTime to,string module,string level, int? userId,long? sessionId,string ip);
    }

    //parameters
    public interface IGeoCountriesRepository : IRepository<GEO_CountriesLib> { }
    public interface IGeoStatesRepository : IRepository<GEO_States> { }
    public interface ICustomEventsRepository : IRepository<DB_CustomEvents> { }
}
