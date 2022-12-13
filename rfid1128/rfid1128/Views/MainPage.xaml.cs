using rfid1128.Infrastructure;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace rfid1128.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage
    {
        private string currentPage = "none";
        private NavigationPage lastPage = null;

        public MainPage()
        {
            InitializeComponent();
            this.Appearing += MainPage_Appearing;
            this.Disappearing += MainPage_Disappearing;
            this.CurrentPageChanged += MainPage_CurrentPageChanged;
        }
        private void MainPage_CurrentPageChanged(object sender, EventArgs e)
        {
            //Use the Tabbed page change event to invoke the correct Shown/Hidden methods
            // This worksaround inconsistencies in the Xamarin Appearing/Disappearing events
            // Code assumes that each tab has a Navigation Page at base so may be fragile
            if (this.lastPage != null)
            {
                var np = this.lastPage as NavigationPage;
                var cp = np?.CurrentPage.BindingContext as ILifecycle;

                System.Diagnostics.Debug.WriteLine("Hidden: {0}{1}", cp?.ToString(), "");
                cp?.Hidden();
            }
            if (this.CurrentPage != null)
            {
                System.Diagnostics.Debug.WriteLine("MainPage. CurrentPageChanged {0} => {1}", currentPage, this.CurrentPage.Title);
                currentPage = this.CurrentPage.Title;

                var np = this.CurrentPage as NavigationPage;
                this.lastPage = np;
                var cp = np?.CurrentPage.BindingContext as ILifecycle;

                System.Diagnostics.Debug.WriteLine("Shown: {0}{1}", cp?.ToString(), "");
                cp?.Shown();
            }
        }

        private void MainPage_Disappearing(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MainPage. Hidden");

            // Allow other pages to be Hidden gracefully
            MainPage_CurrentPageChanged(sender, e);
        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MainPage. Shown");

            // Ensure content page Shown events occur on first appearance
            MainPage_CurrentPageChanged(sender, e);
        }
    }
}