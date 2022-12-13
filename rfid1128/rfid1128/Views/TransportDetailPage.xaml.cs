using rfid1128.ViewModels;
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
    public partial class TransportDetailPage : ContentPage
    {
        TransportViewModel viewModel;

        public TransportViewModel ViewModel
        {
            get => this.viewModel;
            set => this.BindingContext = this.viewModel = value;
        }
        public TransportDetailPage()
        {
            InitializeComponent();
        }
    }
}