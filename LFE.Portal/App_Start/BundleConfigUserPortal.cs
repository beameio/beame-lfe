using System.Web.Optimization;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class BundleConfigUserPortal
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Areas/UserPortal/Content/styles/css").Include("~/Areas/UserPortal/Content/styles/site.css"));

            var lfeCommonBundle =  new Bundle("~/Areas/UserPortal/bundles/lfe",new JsMinify());

            lfeCommonBundle.Include("~/Areas/UserPortal/Scripts/lfe/notif.manager.js").Include("~/Areas/UserPortal/Scripts/lfe/lfe.global.js");

            bundles.Add(lfeCommonBundle);
     
            
            //user course details
            var courseViewerBundle = new Bundle("~/Areas/UserPortal/bundles/courseviewer", new JsMinify());

            courseViewerBundle.Include("~/Areas/UserPortal/Scripts/plugins/jquery.rateit.min.js")                            
                              .Include("~/Areas/UserPortal/Scripts/plugins/jquery.fieldselection.js")
                              .Include("~/Areas/UserPortal/Scripts/plugins/jquery.scrollTo.js")
                              .Include("~/Areas/UserPortal/Scripts/plugins/jquery.tagmate.js")
                              .Include("~/Areas/UserPortal/Scripts/plugins/jquery.autosize.min.js")
                              .Include("~/Areas/UserPortal/Scripts/plugins/scrollIt.min.js")
                              .Include("~/Areas/UserPortal/Scripts/lfe/lfe.bcl.player.js")
                              .Include("~/Areas/UserPortal/Scripts/lfe/lfe.course.detail.js")
                              .Include("~/Areas/UserPortal/Scripts/lfe/lfe.discuss.js");

            bundles.Add(courseViewerBundle);

            bundles.Add(new StyleBundle("~/Areas/UserPortal/Content/styles/courseviewer").Include("~/Areas/UserPortal/Content/styles/rateit.css")                                                                                                                                                                                  
                                                                                         .Include("~/Areas/UserPortal/Content/styles/course.css")
                                                                                         );
            //            

            // Clear all items from the default ignore list to allow minified CSS and JavaScript files to be included in debug mode
            bundles.IgnoreList.Clear();


            // Add back the default ignore list rules sans the ones which affect minified files and debug mode
            bundles.IgnoreList.Ignore("*.intellisense.js");
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
           
        }
    }
}