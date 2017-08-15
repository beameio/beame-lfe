using System.Web.Mvc;
using LFE.Domain.Model;
using LFE.Infrastructure.NLogger;

namespace LFE.Application.Services.Api
{
    public class ServiceBase
    {
        public NLogLogger Logger { get; set; }

        public IWebStoreRepository WebStoreRepository { get; set; }
        public IWebStoresChangeLogRepository WebStoresChangeLogRepository { get; set; }
        public IWebStoreItemRepository WebStoreItemRepository { get; set; }

        public ICourseRepository CourseRepository { get; set; }
        public ICourseChangeLogRepository CourseChangeLogRepository { get; set; }

        public ServiceBase()
        {
            Logger = DependencyResolver.Current.GetService<NLogLogger>();

            WebStoresChangeLogRepository = DependencyResolver.Current.GetService<IWebStoresChangeLogRepository>();
            WebStoreItemRepository       = DependencyResolver.Current.GetService<IWebStoreItemRepository>();
            WebStoreRepository           = DependencyResolver.Current.GetService<IWebStoreRepository>();

            CourseRepository          = DependencyResolver.Current.GetService<ICourseRepository>();
            CourseChangeLogRepository = DependencyResolver.Current.GetService<ICourseChangeLogRepository>();
        }

        public void Dispose()
        {
            WebStoreRepository.Dispose();
            WebStoreItemRepository.Dispose();
            WebStoresChangeLogRepository.Dispose();

            CourseRepository.Dispose();
            CourseChangeLogRepository.Dispose();
        }
    }    
}