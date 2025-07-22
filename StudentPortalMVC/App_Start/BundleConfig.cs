using System.Web;
using System.Web.Optimization;

namespace StudentPortalMVC // This namespace is usually correct for the root of your project
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // JQuery Bundle
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // JQuery Validation Bundle
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Modernizr Bundle
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Bootstrap JS Bundle (using bootstrap.bundle.js for full features like dropdowns)
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.js"));

            // SB Admin JS Bundle
            bundles.Add(new ScriptBundle("~/bundles/sbadmin").Include(
                      "~/contentsource/js/scripts.js")); // Path to your SB Admin JS within 'contentsource'

            // NEW: Bundles for Chart.js
            bundles.Add(new ScriptBundle("~/bundles/charts").Include(
                      "~/contentsource/js/Chart.min.js", // Assuming Chart.min.js is directly in contentsource/js
                      "~/contentsource/assets/demo/chart-area-demo.js", // Assuming these demo scripts are in contentsource/assets/demo
                      "~/contentsource/assets/demo/chart-bar-demo.js"));

            // NEW: Bundles for Simple DataTables
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                      "~/contentsource/js/simple-datatables.min.js", // Assuming simple-datatables.min.js is directly in contentsource/js
                      "~/contentsource/js/datatables-simple-demo.js"));

            // Main CSS Bundle: Contains default Bootstrap and site-specific CSS
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // SB Admin Stylesheet Bundle
            bundles.Add(new StyleBundle("~/Content/sbadminstyles").Include(
                      "~/contentsource/css/styles.css")); // Path to your SB Admin CSS within 'contentsource'

            // NEW: Style for Simple DataTables (Ensure you copy this CSS file to contentsource/css)
            bundles.Add(new StyleBundle("~/Content/datatablescss").Include(
                      "~/contentsource/css/simple-datatables/style.min.css")); // IMPORTANT: Verify this path and file
        }
    }
}