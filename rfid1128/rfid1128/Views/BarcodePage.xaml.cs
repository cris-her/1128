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
    public partial class BarcodePage : ContentPage
    {
        public BarcodePage()
        {
            InitializeComponent();

            this.BindWithLifecycle(App.ViewModel.Barcode);

        }
    }
}