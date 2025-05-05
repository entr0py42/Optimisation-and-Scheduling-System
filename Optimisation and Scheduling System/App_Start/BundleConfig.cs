using System.Web;
using System.Web.Optimization;

namespace Optimisation_and_Scheduling_System
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Disable optimization in development
            BundleTable.EnableOptimizations = false;

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Modified to avoid minification issues
            var bootstrapBundle = new ScriptBundle("~/bundles/bootstrap");
            bootstrapBundle.Include("~/Scripts/bootstrap.js");
            bootstrapBundle.Transforms.Clear();
            bootstrapBundle.Transforms.Add(new JsTransformer());
            bundles.Add(bootstrapBundle);

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.css",
                      "~/Content/site.css"));
        }
    }

    // Custom transformer that doesn't minify
    public class JsTransformer : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            // This transformer doesn't modify the JavaScript content at all
            // It just concatenates the files without minification
        }
    }
}
