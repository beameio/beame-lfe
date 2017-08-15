using System.Web.Optimization;
using LFE.Core.Utils;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            var isDebugMode = bool.Parse(Utils.GetKeyValue("isDebugMode"));
            var kendoVersion = Utils.GetKeyValue("kendoVersion");

            bundles.UseCdn =  !isDebugMode;

            BundleTable.EnableOptimizations = !isDebugMode;
            
            //jquery
            //const string jqueryCdnPath             = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.min.js";
            //const string jqueryMigrateCdnPath      = "http://ajax.aspnetcdn.com/ajax/jquery.migrate/jquery-migrate-1.2.1.min.js";
            //const string jqueryValidateCdnPath     = "http://ajax.aspnetcdn.com/ajax/jquery.validate/1.12.0/jquery.validate.min.js";
            //var jqueryValidateUnobstructiveCdnPath = "http://ajax.aspnetcdn.com/ajax/mvc/4.0/jquery.validate.unobtrusive.min.js";
           // const string modernizrCdnPath = "//cdnjs.cloudflare.com/ajax/libs/modernizr/2.8.2/modernizr.min.js";
            //const string bootsrapJsCdnPath         = "http://ajax.aspnetcdn.com/ajax/bootstrap/3.1.1/bootstrap.min.js";
            //const string bootsrapCssCdnPath        = "http://ajax.aspnetcdn.com/ajax/bootstrap/3.1.1/css/bootstrap.min.css";


            //bundles.Add(new ScriptBundle("~/bundles/jquery", jqueryCdnPath).Include("~/Scripts/jquery/jquery-{version}.min.js"));
            //bundles.Add(new ScriptBundle("~/bundles/jquery/jquery/migrate", jqueryMigrateCdnPath).Include("~/Scripts/jquery/jquery.migrate-{version}.min.js"));

            var modernizrBundle = new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-2.8.2.js");

            bundles.Add(modernizrBundle);

            #region jquery
            var jqueryBundle = new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery/jquery-1.9.1.min.js")
                                                                    .Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js");
            bundles.Add(jqueryBundle);

            var jqueryValBundle = new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery/jquery-1.9.1.min.js")
                                                                        .Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js")
                                                                        .Include("~/Scripts/jquery/jquery.validate.min.js", "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery/jquery.validate.unobtrusive.min.js");
            bundles.Add(jqueryValBundle); 
            #endregion
            
            #region kendo

            #region scripts
            bundles.Add(new ScriptBundle("~/bundles/kendo")
                    .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.web.min.js") // or kendo.all.*
                    .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.aspnetmvc.min.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/kendoall")
                   .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.all.min.js") // or kendo.all.*
                   .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.aspnetmvc.min.js")
           );

            //listview +pager, combo, ddl,window, autocomplete,tooltip,mvc,effects,core,mvvm
            bundles.Add(new ScriptBundle("~/bundles/kendo/widget")
                    .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.custom.widget.min.js")                   
            ); 
            #endregion

            #region styles
            bundles.Add(new StyleBundle("~/Content/kendo/" + kendoVersion + "/css")
                    .Include("~/Content/kendo/" + kendoVersion + "/kendo.common.min.css")
                    .Include("~/Content/kendo/" + kendoVersion + "/kendo.default.min.css")
            );

            bundles.Add(new StyleBundle("~/Content/kendo/widget" + kendoVersion + "/css")
                 .Include("~/Content/kendo/" + kendoVersion + "/kendo.common.min.css")
                 .Include("~/Content/kendo/" + kendoVersion + "/kendo.default.min.css")
                 .Include("~/Content/kendo/" + kendoVersion + "/kendo.bootstrap.min.css")
            );

            bundles.Add(new StyleBundle("~/styles/kendo")
                .Include("~/Content/kendo/" + kendoVersion + "/kendo.common.min.css")
                .Include("~/Content/kendo/" + kendoVersion + "/kendo.default.min.css")
                .Include("~/Content/kendo/" + kendoVersion + "/kendo.bootstrap.min.css")
           ); 
            #endregion
            #endregion

            #region widget
            #region scripts
            #region common scripts
            var pluginAppBundle = new ScriptBundle("~/bundles/plugins").Include("~/Scripts/bootstrap/bootstrap.min.js")
                                                                        .Include("~/Scripts/plugins/jquery.mousewheel.js", "~/Scripts/plugins/jquery.nanoscroller.min.js") //, "~/Scripts/plugins/overthrow.min.js"
                                                                        .Include("~/Scripts/plugins/jquery.rateit.min.js", "~/Scripts/plugins/jquery.placeholder.min.js")
                                                                        .Include("~/Areas/Widget/Scripts/Html5PlaceHolder");

            bundles.Add(pluginAppBundle);


            var widgetCommonBundle = new ScriptBundle("~/bundles/widget/common").Include("~/Scripts/lfe/lfe.global.js")
                                                                                .Include("~/Areas/Widget/Scripts/lfe/notif.manager.js");

            bundles.Add(widgetCommonBundle); 
            #endregion

            #region viewer
            var widgetViewerBundle = new ScriptBundle("~/bundles/widget/viewer").Include("~/Areas/Widget/Scripts/plugins/jquery.fieldselection.js")
                                                                                    .Include("~/Areas/Widget/Scripts/plugins/jquery.scrollTo.js")
                                                                                    .Include("~/Areas/Widget/Scripts/plugins/jquery.tagmate.min.js")
                                                                                    .Include("~/Areas/Widget/Scripts/plugins/jquery.autosize.min.js")
                                                                                    .Include("~/Areas/Widget/Scripts/plugins/scrollIt.min.js")
                                                                                    .Include("~/Areas/Widget/Scripts/plugins/countdown.min.js")
                                                                                    .Include("~/Areas/Widget/Scripts/lfe/lfe.item.bc.player")
                                                                                    .Include("~/Areas/Widget/Scripts/lfe/lfe.item.viewer.js")
                                                                                    .Include("~/Areas/Widget/Scripts/lfe/lfe.discuss.js");

            bundles.Add(widgetViewerBundle); 
            #endregion

            #region catalog
            var widgetCatalogBundle = new ScriptBundle("~/bundles/widget/catalog").Include("~/Scripts/lfe/lfe.login.js")
                                                                                      .Include("~/Areas/Widget/Scripts/lfe/lfe.item.helper.js");

            bundles.Add(widgetCatalogBundle); 
            #endregion
            
            #region purchase
            var scCommonBundle = new ScriptBundle("~/bundles/lfe/common/sc").Include("~/Scripts/lfe/lfe.global.js").Include("~/Areas/Widget/Scripts/lfe/notif.manager.js").Include("~/Areas/Widget/Scripts/lfe/lfe.sc.helper.js");

            bundles.Add(scCommonBundle);
            
            #endregion
            #endregion

            #region widget styles
            #region common
            var widgetStylesBundle = new StyleBundle("~/bundles/widget").Include("~/Content/styles/reset.css")
                                                                        .Include("~/Content/styles/nanoscroller.css")
                                                                        .Include("~/Content/styles/rateit.css")
                                                                        .Include("~/Content/kendo/" + kendoVersion + "/kendo.common.min.css")
                                                                        .Include("~/Content/kendo/" + kendoVersion + "/kendo.bootstrap.min.css")
                                                                        .Include("~/Content/styles/bootstrap/bootstrap.min.css")
                                                                        .Include("~/Content/styles/bootstrap/bootstrap.custom.size.min.css")
                                                                        .Include("~/Content/styles/login.css")
                                                                        .Include("~/Content/styles/site.css")
                                                                        .Include("~/Areas/Widget/Content/styles/b-main.css")
                                                                        .Include("~/Content/styles/sweet-alert.css");
            bundles.Add(widgetStylesBundle);

            #endregion

            #region purchase
            var purchaseStylesBundle = new StyleBundle("~/bundles/widget/sc").Include("~/Content/kendo/" + kendoVersion + "/kendo.common.min.css")
                                                                                  .Include("~/Content/kendo/" + kendoVersion + "/kendo.bootstrap.min.css")
                                                                                  .Include("~/Content/styles/bootstrap/bootstrap.min.css")
                                                                                  .Include("~/Content/styles/bootstrap/bootstrap.custom.size.min.css")
                                                                                  .Include("~/Areas/Widget/Content/styles/purchase.css");
            bundles.Add(purchaseStylesBundle); 

            var purchaseCompleteStylesBundle = new StyleBundle("~/bundles/widget/postpurchase").Include("~/Content/styles/reset.css")
                                                                                                .Include("~/Content/styles/bootstrap/bootstrap.min.css")
                                                                                                .Include("~/Content/styles/bootstrap/bootstrap.custom.size.min.css")
                                                                                                .Include("~/Areas/Widget/Content/styles/b-main.css")
                                                                                                .Include("~/Areas/Widget/Content/styles/purchase.complete.css");
            bundles.Add(purchaseCompleteStylesBundle); 
            #endregion
            
            #region catalog
            var widgetCatalogStylesBundle = new StyleBundle("~/Areas/Widget/Content/styles/catalog").Include("~/Areas/Widget/Content/styles/catalog.css")
                                                                                                    .Include("~/Areas/Widget/Content/styles/item-page.css")
                                                                                                    .Include("~/Content/styles/sweet-alert.css");
            bundles.Add(widgetCatalogStylesBundle); 
            #endregion

            #region viewer
            var widgetViewerStylesBundle = new StyleBundle("~/bundles/widget/viewer").Include("~/Areas/Widget/Content/styles/item-viewer.css")
                                                                                                  .Include("~/Areas/Widget/Content/styles/item-page.css")
                                                                                                  .Include("~/Content/styles/sweet-alert.css");
            bundles.Add(widgetViewerStylesBundle);   
            #endregion
            #endregion
            #endregion

            #region common layout
            var commonBundle = new ScriptBundle("~/bundles/scripts/common").Include("~/Scripts/lfe/lfe.global.js")
                                                                           .Include("~/Scripts/lfe/lfe.login.js")
                                                                           .Include("~/Scripts/bootstrap/bootstrap.min.js");
            bundles.Add(commonBundle);

            bundles.Add(new StyleBundle("~/bundles/styles/common").Include("~/Content/styles/reset.css")                                                                    
                                                                    .Include("~/Content/styles/bootstrap/bootstrap.min.css")
                                                                    .Include("~/Content/styles/bootstrap/bootstrap.custom.size.min.css")
                                                                    .Include("~/Content/styles/site.css")
                                                                    .Include("~/Content/styles/common-layout.css")
                                                                    .Include("~/Content/styles/login.css"));


            #endregion

            #region pages
            var headerScriptsBundle = new ScriptBundle("~/bundles/header").Include("~/Scripts/modernizr-2.8.2.js")
                                                                            .Include("~/Scripts/plugins/htmlEncode.min.js")
                                                                            .Include("~/Scripts/plugins/swfobject.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/lfe.item.bc.player.template.js"); 
            bundles.Add(headerScriptsBundle);

            var userAccLayoutBundle = new ScriptBundle("~/bundles/useracc").Include("~/Scripts/modernizr-2.8.2.js").Include("~/Scripts/jquery/jquery-1.9.1.min.js")
                                                                                .Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js")
                                                                                .Include("~/Scripts/jquery/jquery.validate.min.js", "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery/jquery.validate.unobtrusive.min.js")
                                                                                .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.web.min.js")
                                                                                .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.aspnetmvc.min.js")
                                                                                .Include("~/Scripts/bootstrap/bootstrap.min.js")
                                                                                .Include("~/Scripts/plugins/jquery.mousewheel.js", "~/Scripts/plugins/jquery.nanoscroller.min.js")
                                                                                .Include("~/Scripts/lfe/lfe.global.js")
                                                                                .Include("~/Areas/UserPortal/Scripts/lfe/notif.manager.js").Include("~/Areas/UserPortal/Scripts/lfe/lfe.global.js");
            bundles.Add(userAccLayoutBundle);

            var commonLayoutBundle = new ScriptBundle("~/bundles/common/layout").Include("~/Scripts/modernizr-2.8.2.js").Include("~/Scripts/jquery/jquery-1.9.1.min.js")
                                                                                .Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js")
                                                                                .Include("~/Scripts/jquery/jquery.validate.min.js", "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery/jquery.validate.unobtrusive.min.js")
                                                                                .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.core.min.js")
                                                                                .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.fx.min.js")
                                                                                .Include("~/Scripts/facebookApi.js")
                                                                                .Include("~/Scripts/lfe/lfe.global.js")
                                                                                .Include("~/Scripts/lfe/lfe.login.js");
            bundles.Add(commonLayoutBundle);


            var catalogScriptsBundle = new ScriptBundle("~/bundles/catalog").Include("~/Scripts/modernizr-2.8.2.js").Include("~/Scripts/jquery/jquery-1.9.1.min.js")
                                                                            .Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js")
                                                                            .Include("~/Scripts/jquery/jquery.validate.min.js", "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery/jquery.validate.unobtrusive.min.js")
                                                                            .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.custom.widget.min.js")
                                                                            .Include("~/Scripts/lfe/lfe.global.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/notif.manager.js")
                                                                            .Include("~/Scripts/bootstrap/bootstrap.min.js")
                                                                            .Include("~/Scripts/plugins/jquery.mousewheel.js", "~/Scripts/plugins/jquery.nanoscroller.min.js")
                                                                            .Include("~/Scripts/plugins/jquery.rateit.min.js", "~/Scripts/plugins/jquery.placeholder.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/Html5PlaceHolder.js")
                                                                            .Include("~/Scripts/facebookApi.js")
                                                                            .Include("~/Scripts/lfe/lfe.login.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/lfe.item.helper.js")
                                                                            .Include("~/Scripts/plugins/sweet-alert.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/countdown.min.js");

            bundles.Add(catalogScriptsBundle);

            var checkoutScriptsBundle = new ScriptBundle("~/bundles/checkout").Include("~/Scripts/kendo/" + kendoVersion + "/kendo.custom.widget.min.js")
                                                                            .Include("~/Scripts/lfe/lfe.global.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/notif.manager.js")
                                                                            .Include("~/Scripts/bootstrap/bootstrap.min.js")
                                                                            .Include("~/Scripts/plugins/jquery.mousewheel.js", "~/Scripts/plugins/jquery.nanoscroller.min.js")
                                                                            .Include("~/Scripts/plugins/jquery.rateit.min.js", "~/Scripts/plugins/jquery.placeholder.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/Html5PlaceHolder.js")
                                                                            .Include("~/Scripts/facebookApi.js")
                                                                            .Include("~/Scripts/lfe/lfe.login.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/lfe.item.helper.js");
            bundles.Add(checkoutScriptsBundle);

            var viewerScriptsBundle = new ScriptBundle("~/bundles/viewer").Include("~/Scripts/modernizr-2.8.2.js").Include("~/Scripts/jquery/jquery-1.9.1.min.js")
                                                                            .Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js")
                                                                            .Include("~/Scripts/jquery/jquery.validate.min.js", "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery/jquery.validate.unobtrusive.min.js")
                                                                            .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.custom.widget.min.js")
                                                                            .Include("~/Scripts/lfe/lfe.global.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/notif.manager.js")
                                                                            .Include("~/Scripts/bootstrap/bootstrap.min.js")
                                                                            .Include("~/Scripts/plugins/jquery.mousewheel.js", "~/Scripts/plugins/jquery.nanoscroller.min.js")
                                                                            .Include("~/Scripts/plugins/jquery.rateit.min.js", "~/Scripts/plugins/jquery.placeholder.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/Html5PlaceHolder.js")
                                                                            .Include("~/Scripts/facebookApi.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/jquery.fieldselection.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/jquery.scrollTo.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/jquery.tagmate.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/jquery.autosize.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/scrollIt.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/countdown.min.js")                                                                            
                                                                            .Include("~/Areas/Widget/Scripts/lfe/lfe.item.bc.player.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/lfe.item.viewer.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/lfe.discuss.js")
                                                                            .Include("~/Areas/Widget/Scripts/lfe/lfe.quiz.helper.js")
                                                                            .Include("~/Scripts/plugins/sweet-alert.min.js")
                                                                            .Include("~/Areas/Widget/Scripts/plugins/countdown.min.js");
            bundles.Add(viewerScriptsBundle);

            var fbFramesScriptsBundle = new ScriptBundle("~/bundles/fb").Include("~/Scripts/modernizr-2.8.2.js").Include("~/Scripts/jquery/jquery-1.9.1.min.js")
                                                                        .Include("~/Scripts/facebookApi.js")
                                                                          .Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js")
                                                                          .Include("~/Scripts/jquery/jquery.validate.min.js", "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery/jquery.validate.unobtrusive.min.js")
                                                                          .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.web.min.js")
                                                                          .Include("~/Scripts/kendo/" + kendoVersion + "/kendo.aspnetmvc.min.js")
                                                                          .Include("~/Scripts/lfe/lfe.global.js")
                                                                          .Include("~/Scripts/bootstrap/bootstrap.min.js");
            bundles.Add(fbFramesScriptsBundle);
            #endregion
            ////////////////////


            bundles.Add(new ScriptBundle("~/bundles/jquery/jquery").Include("~/Scripts/jquery/jquery-1.9.1.min.js").Include("~/Scripts/jquery/jquery.migrate.1.2.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery/jqueryval").Include("~/Scripts/jquery/jquery.validate.min.js", "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery/jquery.validate.unobtrusive.min.js"));

           // bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-2.7.2.min*"));

            //bootstrap
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap/bootstrap.min.js"));
            bundles.Add(new StyleBundle("~/bundles/styles/bootstrap").Include("~/Content/styles/bootstrap/bootstrap.min.css").Include("~/Content/styles/bootstrap/bootstrap.custom.size.min.css"));


          


            //common css bundle
            bundles.Add(new StyleBundle("~/Content/styles/common").Include("~/Content/styles/common.css"));

            var lfeCommonBundle = new ScriptBundle("~/bundles/lfe/common").Include("~/Scripts/lfe/lfe.global.js").Include("~/Areas/Widget/Scripts/lfe/notif.manager.js")
                                                                        .Include("~/Areas/Widget/Scripts/lfe/lfe.item.helper.js");

            bundles.Add(lfeCommonBundle);

            //scroll bundle
            bundles.Add(new StyleBundle("~/Content/plugin/scroll").Include("~/Content/styles/nanoscroller.css"));

            var scrollBundle = new ScriptBundle("~/bundles/plugin/scroll").Include("~/Scripts/plugins/jquery.mousewheel.js").Include("~/Scripts/plugins/jquery.nanoscroller.min.js");

            bundles.Add(scrollBundle);

            //signalR
            bundles.Add(new ScriptBundle("~/bundles/signalR").Include("~/Scripts/jquery.signalR-{version}.min.js"));

            

            bundles.IgnoreList.Clear();
        }
    }
}