using rfid1128.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace rfid1128.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryPage : ContentPage
    {
        public InventoryPage()
        {
            InitializeComponent();

            this.BindWithLifecycle(App.ViewModel.Inventory);
        }

        private string SelectedTransponder { get; set; }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            //var model = this.BindingContext as ViewModels.TransportsViewModel;
            var item = args.SelectedItem as Models.IdentifiedItem;
            if (item != null)
            {
                this.SelectedTransponder = item.Identifier;
            }
        }
    }
}