using rfid1128.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace rfid1128.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReadersPage : ContentPage
    {
        ReadersViewModel viewModel;
        public ReadersPage()
        {
            InitializeComponent();
            this.BindingContext = this.viewModel = App.ViewModel.Readers;
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            this.viewModel.SelectedReader = args.SelectedItem as ReaderViewModel;
        }
    }
}