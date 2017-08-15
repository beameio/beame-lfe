using System.Web.Optimization;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class BundleConfigWidget
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            var lfeCommonBundle =  new Bundle("~/Areas/Widget/bundles/lfe",new JsMinify());

            lfeCommonBundle.Include("~/Areas/Widget/Scripts/lfe/notif.manager.js").Include("~/Areas/Widget/Scripts/lfe/lfe.global.js");

            bundles.Add(lfeCommonBundle);


            //user course details
            var courseViewerBundle = new Bundle("~/Areas/Widget/bundles/WidgetCourse", new JsMinify());

            courseViewerBundle.Include("~/Areas/Widget/Scripts/plugins/jquery.rateit.min.js")
                              .Include("~/Areas/Widget/Scripts/plugins/jquery.fieldselection.js")
                              .Include("~/Areas/Widget/Scripts/plugins/jquery.scrollTo.js")
                              .Include("~/Areas/Widget/Scripts/plugins/jquery.tagmate.js")
                              .Include("~/Areas/Widget/Scripts/plugins/scrollIt.min.js")
                              .Include("~/Areas/Widget/Scripts/lfe/lfe.bcl.player.js")
                              .Include("~/Areas/Widget/Scripts/lfe/lfe.course.detail.js")
                              .Include("~/Areas/Widget/Scripts/lfe/lfe.discuss.js");

            var lessCatalogBundle = new Bundle("~/widget/catalog/less").IncludeDirectory("~/Areas/Widget/Content/styles/less", "widget.catalog.less");
            lessCatalogBundle.Transforms.Add(new LessTransform());
            lessCatalogBundle.Transforms.Add(new CssMinify());

            bundles.Add(lessCatalogBundle);

            var lessViewerBundle = new Bundle("~/widget/viewer/less").IncludeDirectory("~/Areas/Widget/Content/styles/less", "widget.viewer.less");
            lessViewerBundle.Transforms.Add(new LessTransform());
            lessViewerBundle.Transforms.Add(new CssMinify());

            bundles.Add(lessViewerBundle);

            var lessCertBundle = new Bundle("~/widget/cert/less").IncludeDirectory("~/Areas/Widget/Content/styles/less", "cert.less");
            lessCertBundle.Transforms.Add(new LessTransform());
            lessCertBundle.Transforms.Add(new CssMinify());

            bundles.Add(lessCertBundle);
          
        }
    }
}