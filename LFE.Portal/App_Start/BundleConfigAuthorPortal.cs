using System.Web.Optimization;
using LFE.Core.Utils;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class BundleConfigAuthorPortal
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            var kendoVersion = Utils.GetKeyValue("kendoVersion");
            

            bundles.Add(new StyleBundle("~/Areas/AuthorAdmin/Content/styles/css").Include("~/Areas/AuthorAdmin/Content/styles/site.css").Include("~/Areas/AuthorAdmin/Content/styles/common.css"));

            var lfeCommonBundle =  new Bundle("~/Areas/AuthorAdmin/bundles/lfe",new JsMinify());

            lfeCommonBundle.Include("~/Areas/AuthorAdmin/Scripts/lfe/notif.manager.js").Include("~/Areas/AuthorAdmin/Scripts/lfe/lfe.global.js");

            bundles.Add(lfeCommonBundle);
          
            //dashboard bundle grid+chart+tooltip+window+listview+dropdownlist+mvc wrapper; vml required for old IE support
            bundles.Add(new ScriptBundle("~/Areas/AuthorAdmin/bundles/kendowiz").Include("~/Areas/AuthorAdmin/Scripts/kendo/" + kendoVersion + "/kendo.custom.dash.min.js"));

     
            //course
            var courseBundle = new ScriptBundle("~/Areas/AuthorAdmin/bundles/course")
                                                .Include("~/Areas/AuthorAdmin/Scripts/plugins/jquery-ui-1.10.3.custom.min.js")
                                                .Include("~/Areas/AuthorAdmin/Scripts/lfe/lfe.video.js")
                                                .Include( "~/Areas/AuthorAdmin/Scripts/lfe/lfe.course.js")
                                                .Include( "~/Areas/AuthorAdmin/Scripts/lfe/lfe.form.helper.js");

            bundles.Add(courseBundle);

            //wizard
            var wizardBundle = new ScriptBundle("~/Areas/AuthorAdmin/bundles/wizard").Include("~/Areas/AuthorAdmin/Scripts/lfe/lfe.form.helper.js")
                                                                                     .Include("~/Areas/AuthorAdmin/Scripts/plugins/jquery-ui-1.10.3.custom.min.js")
                                                                                     .Include("~/Areas/AuthorAdmin/Scripts/lfe/lfe.video.js")
                                                                                     .Include("~/Areas/AuthorAdmin/Scripts/lfe/lfe.wizard.js");
            bundles.Add(wizardBundle);

            //webstore
            var webStoreBundle = new Bundle("~/Areas/AuthorAdmin/bundles/webstore", new JsMinify());

            webStoreBundle.Include("~/Areas/AuthorAdmin/Scripts/plugins/jquery-ui-1.10.4.custom.min.js"
                                  ,"~/Areas/AuthorAdmin/Scripts/lfe/lfe.form.helper.js"
                                  ,"~/Areas/AuthorAdmin/Scripts/lfe/lfe.ws.js");

            bundles.Add(webStoreBundle);


            //g2t less
            var lessBundle = new Bundle("~/admin/g2t/less").IncludeDirectory("~/Areas/AuthorAdmin/Content/styles/less", "g2t.less");
            lessBundle.Transforms.Add(new LessTransform());
            lessBundle.Transforms.Add(new CssMinify());

            bundles.Add(lessBundle);

            //quiz less
            var quizBundle = new Bundle("~/admin/quiz/less").IncludeDirectory("~/Areas/AuthorAdmin/Content/styles/less", "quiz.less");
            quizBundle.Transforms.Add(new LessTransform());
            quizBundle.Transforms.Add(new CssMinify());

            bundles.Add(quizBundle);

            //quiz less
            var courseEditBundle = new Bundle("~/admin/course/less").IncludeDirectory("~/Areas/AuthorAdmin/Content/styles/less", "course.edit.less");
            courseEditBundle.Transforms.Add(new LessTransform());
            courseEditBundle.Transforms.Add(new CssMinify());

            bundles.Add(courseEditBundle);

            //cert less
            var certBundle = new Bundle("~/admin/cert/less").IncludeDirectory("~/Areas/AuthorAdmin/Content/styles/less", "cert.less");
            certBundle.Transforms.Add(new LessTransform());
            certBundle.Transforms.Add(new CssMinify());

            bundles.Add(certBundle);

            // Clear all items from the default ignore list to allow minified CSS and JavaScript files to be included in debug mode
            bundles.IgnoreList.Clear();


            // Add back the default ignore list rules sans the ones which affect minified files and debug mode
            bundles.IgnoreList.Ignore("*.intellisense.js");
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
           
        }
    }
}