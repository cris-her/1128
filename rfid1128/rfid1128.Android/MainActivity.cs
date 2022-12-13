using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using TechnologySolutions.Rfid.AsciiProtocol.Extensions;
using rfid1128.Infrastructure;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;

namespace rfid1128.Droid
{
    [Activity(Label = "rfid1128", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IAndroidLifecycle lifecyle;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        //
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        //
        private IAndroidLifecycle TslLifecycle
        {
            get
            {
                if (this.lifecyle == null)
                {
                    AsciiTransportsManager manager = Locator.Default.Locate<IAsciiTransportsManager>() as AsciiTransportsManager;

                    // AndrdoidLifecycleNone provides a no action IAndroidLifecycle instance to call in OnPause, OnResume so we don't keep
                    // attempting to resolve the AsciiTransportManager as the IAndroidLifecycle if it is not being used in this project
                    this.lifecyle = (IAndroidLifecycle)manager ?? new AndroidLifecycleNone();

                    // If the HostBarcodeHandler has been registered with the locator then it will be the Android type that needs IAndroidLifecycle calls
                    // Register the HostBarcodeHandler lifecycle with the AsciiTransportsManager
                    manager.RegisterLifecycle(Locator.Default.Locate<IHostBarcodeHandler>() as HostBarcodeHandler);
                }

                return this.lifecyle;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            (this.Lifecycle as IDisposable).Dispose();
        }

        protected override void OnPause()
        {
            base.OnPause();

            this.TslLifecycle.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.TslLifecycle.OnResume(this);
        }

    }
}