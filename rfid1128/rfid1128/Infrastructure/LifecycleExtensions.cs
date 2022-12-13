using Xamarin.Forms;

namespace rfid1128.Infrastructure
{
    public static class LifecycleExtensions
    {
        public static void BindWithLifecycle(this Page page, ILifecycle lifecycleViewModel)
        {
            page.BindingContext = lifecycleViewModel;
            page.Appearing += (sender, e) =>
             {
                 System.Diagnostics.Debug.WriteLine("Page Appearing: {0}", page.Title);
//                 lifecycleViewModel.Shown();
             };
            page.Disappearing += (sender, e) =>
              {
                  System.Diagnostics.Debug.WriteLine("Page Disappearing: {0}", page.Title);
//                  lifecycleViewModel.Hidden();
              };
        }
    }
}
