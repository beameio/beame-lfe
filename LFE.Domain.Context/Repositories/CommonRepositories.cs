using System.Linq;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;
using System;
using System.Collections.Generic;

namespace LFE.Domain.Context.Repositories
{
   //s3 file
    public class S3FileInterfaceRepository : Repository<UserS3FileInterface>, IS3FileInterfaceRepository
    {
        public S3FileInterfaceRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IEnumerable<LOG_FileInterfaceToken> GetFileInterfaceReport(DateTime from, DateTime to, int? userId, ImportJobsEnums.eFileInterfaceStatus? status)
        {
            return DataContext.tvf_LOG_FileInterface(from, to, userId, status == null ? null : status.ToString());
        }
    }

    //fb
    public class FacebookPostRepository : Repository<FB_PostInterface>, IFacebookPostRepository
    {
        public FacebookPostRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IEnumerable<LOG_FbPostInterfaceToken> GetFbPostInterfaceReport(DateTime from, DateTime to, int? userId, FbEnums.ePostInterfaceStatus? status)
        {
            return DataContext.tvf_LOG_FbPostInterface(from, to, userId, status == null ? null : status.ToString());
        }
    }

    //email
    public class EmailMessageRepository : Repository<EMAIL_Messages>, IEmailMessageRepository
    {
        public EmailMessageRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IEnumerable<EMAIL_NotificationMessageToken> GetMessageNotifications(long messageId)
        {
            return DataContext.tvf_EMAIL_GetNotificationMessages(messageId);
        }

        public IEnumerable<LOG_EmailInterfaceToken> GetEmailInterfaceReport(DateTime from, DateTime to, int? userId, EmailEnums.eSendInterfaceStatus? status)
        {
            return DataContext.tvf_LOG_EmailInterface(from, to, userId, status == null ? null : status.ToString());
        }
    }
    public class EmailTemplateRepository : Repository<EMAIL_Templates>, IEmailTemplateRepository
    {
        public EmailTemplateRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }

    //log table
    public class LogTableRepository : Repository<LogTable>, ILogTableRepository
    {
        public LogTableRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ADMIN_SystemLogToken> GetSystemLogs(DateTime from, DateTime to, string module, string level, int? userId,long? sessionId, string ip)
        {
            var events =  DataContext.tvf_ADMIN_GetSystemLogs(from, to, module, level, userId, sessionId, String.IsNullOrEmpty(ip.TrimString()) ? null : ip.TrimString() );
            var a = events.Count();
            return events;
        }
    }

   //Geo
    public class GeoCountriesRepository : Repository<GEO_CountriesLib>, IGeoCountriesRepository
    {
        public GeoCountriesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class GeoStatesRepository : Repository<GEO_States>, IGeoStatesRepository
    {
        public GeoStatesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class CustomEventsRepository : Repository<DB_CustomEvents>, ICustomEventsRepository
    {
        public CustomEventsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
}
