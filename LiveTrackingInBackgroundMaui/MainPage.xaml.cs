
using GeolocatorPlugin;
using Microsoft.Maui.Devices.Sensors;

namespace LiveTrackingInBackgroundMaui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Subscribe<LocationMessage>(this, "Location", message =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        locationLabel.Text += $"{Environment.NewLine}{message.Latitude}, {message.Longitude}, {DateTime.Now.ToLongTimeString()}";

                        Console.WriteLine($"{message.Latitude}, {message.Longitude}, {DateTime.Now.ToLongTimeString()}");
                    });
                });

                MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        locationLabel.Text = "Location Service has been stopped!";
                    });
                });

                MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError", message =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        locationLabel.Text = "There was an error updating location!";
                    });
                });

                if (Preferences.Get("LocationServiceRunning", false) == true)
                {
                    StartService();
                }
            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            var permission = await Permissions.RequestAsync<Permissions.LocationAlways>();

            if (permission == PermissionStatus.Denied)
            {
                // TODO Let the user know they need to accept
                return;
            }
            if (Device.RuntimePlatform == Device.iOS)
            {
                if (CrossGeolocator.Current.IsListening)
                {
                    await CrossGeolocator.Current.StopListeningAsync();
                    CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;

                    return;
                }

                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 10, false, new GeolocatorPlugin.Abstractions.ListenerSettings
                {
                    ActivityType = GeolocatorPlugin.Abstractions.ActivityType.AutomotiveNavigation,
                    AllowBackgroundUpdates = true,
                    DeferLocationUpdates = false,
                    DeferralDistanceMeters = 10,
                    DeferralTime = TimeSpan.FromSeconds(5),
                    ListenForSignificantChanges = true,
                    PauseLocationUpdatesAutomatically = true
                });

                CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                if (Preferences.Get("LocationServiceRunning", false) == false)
                {
                    StartService();
                }
                else
                {
                    StopService();
                }
            }
        }

        private void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
            Preferences.Set("LocationServiceRunning", true);
            locationLabel.Text = "Location Service has been started!";
        }

        private void StopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
            Preferences.Set("LocationServiceRunning", false);
        }

        private async Task<PermissionStatus> RequestLocationPermission()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                // Prompt the user with additional information as to why the permission is needed
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status;
        }

        private void Current_PositionChanged(object sender, GeolocatorPlugin.Abstractions.PositionEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                locationLabel.Text += $"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}{Environment.NewLine}";
                Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}");
            });
        }
    }


}
