using System;
using Autofac;
using LFE.Cach.Provider;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class ServicesConfig
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.Register(c => new InMemoryCacheService()).AsImplementedInterfaces().SingleInstance();

            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                   .Where(a => a.Name.EndsWith("Wrapper") || a.Name.EndsWith("Services"))
                   .AsImplementedInterfaces()
                   .InstancePerRequest();

//            //external providers
//            builder.RegisterType<S3Wrapper>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<AmazonEmailWrapper>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<FacebookServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<PaypalServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//    
//            //common
//            builder.RegisterType<WidgetUserServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<CourseServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<CourseWizardServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<CouponServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<DiscussionServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<EmailServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<ReportServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<SettingsServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<WebStoreServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<WidgetServices>().AsImplementedInterfaces().InstancePerHttpRequest();            
//            builder.RegisterType<BillingServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<GeoServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<WidgetEndpointServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<MailchimpServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<DashboardService>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<PayoutServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<HubServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<AdminDashboardServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//
//
//            builder.RegisterType<QuizServices>().AsImplementedInterfaces().InstancePerHttpRequest();
//            builder.RegisterType<CertificateServices>().AsImplementedInterfaces().InstancePerHttpRequest();
        }
    }
}