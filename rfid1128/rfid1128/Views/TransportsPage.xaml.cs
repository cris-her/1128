using rfid1128.Infrastructure;
using rfid1128.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace rfid1128.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransportsPage : ContentPage
    {
        TransportsViewModel viewModel;
        public TransportsPage()
        {
            InitializeComponent();

            this.BindWithLifecycle(viewModel = App.ViewModel.Transports);
        }
        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var transportModel = args.SelectedItem as ViewModels.TransportViewModel;
            if (transportModel != null)
            {
                await Navigation.PushAsync(new TransportDetailPage() { ViewModel = transportModel });

                this.TransportsListView.SelectedItem = null; // deselect for next time
            }
        }
    }
}