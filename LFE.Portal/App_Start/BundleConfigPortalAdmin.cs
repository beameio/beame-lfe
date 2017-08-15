using System.Web.Optimization;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class BundleConfigPortalAdmin
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Areas/PortalAdmin/Content/styles/css").Include("~/Areas/PortalAdmin/Content/styles/site.css"));

            var lfeCommonBundle =  new Bundle("~/Areas/PortalAdmin/bundles/lfe",new JsMinify());

            lfeCommonBundle.Include("~/Areas/PortalAdmin/Scripts/lfe/notif.manager.js").Include("~/Areas/PortalAdmin/Scripts/lfe/lfe.global.js");

            bundles.Add(lfeCommonBundle);
     
          
           
            //user course details
            var courseViewerBundle = new Bundle("~/Areas/PortalAdmin/bundles/courseviewer", new JsMinify());

            courseViewerBundle.Include("~/Areas/PortalAdmin/Scripts/plugins/jquery.mousewheel.js")
                              .Include("~/Areas/PortalAdmin/Scripts/plugins/jquery.nanoscroller.min.js")
                              .Include("~/Areas/PortalAdmin/Scripts/plugins/jquery.rateit.min.js")
                              .Include("~/Areas/PortalAdmin/Scripts/plugins/scrollIt.min.js")
                              .Include("~/Areas/PortalAdmin/Scripts/lfe/lfe.bcl.player.js")
                              .Include("~/Areas/PortalAdmin/Scripts/lfe/lfe.course.detail.js");

            bundles.Add(courseViewerBundle);

            bundles.Add(new StyleBundle("~/Areas/PortalAdmin/Content/styles/courseviewer").Include("~/Areas/PortalAdmin/Content/styles/rateit.css")
                                                                                         .Include("~/Areas/PortalAdmin/Content/styles/nanoscroller.css")
                                                                                         .Include("~/Areas/PortalAdmin/Content/styles/course.css"));


            var lessBundle = new Bundle("~/admin/dash/less").IncludeDirectory("~/Areas/PortalAdmin/Content/styles/less", "admin.dash.less");
            lessBundle.Transforms.Add(new LessTransform());
            lessBundle.Transforms.Add(new CssMinify());

            bundles.Add(lessBundle);

            // Clear all items from the default ignore list to allow minified CSS and JavaScript files to be included in debug mode
            bundles.IgnoreList.Clear();


            // Add back the default ignore list rules sans the ones which affect minified files and debug mode
            bundles.IgnoreList.Ignore("*.intellisense.js");
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
           
        }
    }
}