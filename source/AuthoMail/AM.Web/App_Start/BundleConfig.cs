using System.Web;
using System.Web.Optimization;

namespace AM.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            //cl jquery
            bundles.Add(new ScriptBundle("~/bundles/cljs").Include(
                //"~/clui/dist/js/vendor/jquery.min.js",
                        "~/clui/dist/js/corelogic-ui.js"//,
                //"~/clui/dist/js/corelogic-ui-module.js",
                //"~/clui/dist/js/components/jquery-ui.js",
                //"~/clui/dist/js/corelogic-ui-vendor.js"//,
                //"~/clui/dist/js/corelogic-ui-no-bootstrap.js",
                //"~/clui/dist/js/vendor/moment.js"
                        ));
            //cl componet
            bundles.Add(new ScriptBundle("~/bundles/clcomponetjs").Include(
                "~/clui/dist/js/components/dateTimePicker.js",

                "~/clui/dist/js/components/toggle.js",
                "~/clui/dist/js/components/tooltip.js",
                "~/clui/dist/js/components/alert.js",
                "~/clui/dist/js/components/panel.js",
                "~/clui/dist/js/components/popover.js",
                "~/clui/dist/js/components/slider.js",
                "~/clui/dist/js/components/table.js",
                "~/clui/dist/js/vendor/moment.js",
                "~/clui/dist/js/vendor/jsondateextensions.js",
                "~/clui/dist/js/components/init.js",
                "~/Content/bootstrap-timepicker/bootstrap-timepicker.js"

                        ));
            //cl vendor
            bundles.Add(new ScriptBundle("~/bundles/clvendorjs").Include(
                        "~/clui/dist/js/vendor/bootstrap.js",
                        "~/clui/dist/js/vendor/jquery.ui.touch-punch.js"
                       ));
            //cl select2
            bundles.Add(new ScriptBundle("~/bundles/clselect2").Include(
                       "~/clui/dist/js/components/select2.js"));

            //cl autocomplete
            bundles.Add(new ScriptBundle("~/bundles/clautocomplete").Include(
                       "~/clui/dist/js/components/autocomplete.js"));

            //cl datepicker
            bundles.Add(new ScriptBundle("~/bundles/cldatepicker").Include(
                      "~/clui/dist/js/components/dateTimePicker.js"
                //,                      "~/clui/dist/js/vendor/daterangepicker.js"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/additional-methods.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
              "~/Content/datatables/datatable.js",
              "~/Content/datatables/datatables.min.js",
              "~/Content/datatables/plugins/bootstrap/datatables.bootstrap.js"
                //"~/Content/datatables/table-datatables-managed.js"
              ));

            bundles.Add(new StyleBundle("~/Content/clcss").Include(
                      "~/clui/dist/css/corelogic-ui.css",
                      "~/clui/dist/css/components/alert.css").Include(
                      "~/clui/dist/css/components/fonts.css", new CssRewriteUrlTransformWrapper()).Include(
                      "~/clui/dist/css/components/footer.css",
                      "~/clui/dist/css/components/forms.css",
                      "~/clui/dist/css/components/labels.css",
                      "~/clui/dist/css/components/metatable.css",
                      "~/clui/dist/css/components/navbar.css",
                      "~/clui/dist/css/components/page-title.css",
                      "~/clui/dist/css/components/panel.css",
                      "~/clui/dist/css/customui.css",
                      "~/clui/dist/css/components/table.css",
                      "~/Content/bootstrap-timepicker/bootstrap-timepicker.css"

                      ));

            bundles.Add(new StyleBundle("~/bundles/datatablescss").Include(
                //"~/Content/datatables/datatables.min.css",
                "~/Content/datatables/datatables.css",
               "~/Content/datatables/plugins/bootstrap/datatables.bootstrap.css"
               ));
        }
    }

    public class CssRewriteUrlTransformWrapper : IItemTransform
    {
        public string Process(string includedVirtualPath, string input)
        {
            return new CssRewriteUrlTransform().Process("~" + VirtualPathUtility.ToAbsolute(includedVirtualPath), input);
        }
    }
}
