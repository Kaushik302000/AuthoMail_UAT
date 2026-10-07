using System.Web;
using System.Web.Optimization;

namespace AM.Scanner
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/IndexPage").Include(
                "~/Content/module.admin.page.index.min.css"
                ));
            bundles.Add(new ScriptBundle("~/bundles/LoginPage").Include(
                "~/Content/module.admin.page.index.min.css"
                ));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
              "~/Content/datatables/datatable.js",
              "~/Content/datatables/datatables.min.js",
              "~/Content/datatables/plugins/bootstrap/datatables.bootstrap.js"//,
                //"~/Content/datatables/table-datatables-managed.js"
              ));

            bundles.Add(new StyleBundle("~/bundles/datatablescss").Include(
                //"~/Content/datatables/datatables.min.css",
               "~/Content/datatables/datatables.css",
              "~/Content/datatables/plugins/bootstrap/datatables.bootstrap.css"
              ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
