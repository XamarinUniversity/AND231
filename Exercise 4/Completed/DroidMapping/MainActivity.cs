using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Gms.Maps;
using System.Threading.Tasks;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Runtime;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Common;
using Android.Gms.Plus;

namespace DroidMapping
{
    [Activity(Label = "DroidMapping", MainLauncher = true)]
    public class MainActivity : Activity, IOnMapReadyCallback,
        Android.Locations.ILocationListener,
        Android.Gms.Location.ILocationListener,
        GoogleApiClient.IConnectionCallbacks,
        GoogleApiClient.IOnConnectionFailedListener
    {
        MappingPermissionsHelper permissionHelper;
        MapFragment mapFragment;
        GoogleMap map;
        Task<bool> getLocationPermissionsAsync;
        Marker currentLocationMarker;
        GoogleApiClient apiClient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            permissionHelper = new MappingPermissionsHelper(this);
            getLocationPermissionsAsync = permissionHelper.CheckAndRequestPermissions();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mapFragment = FragmentManager.FindFragmentById(Resource.Id.map) as MapFragment;

            mapFragment.GetMapAsync(this);

            if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this) == 0)
            {
                apiClient = new GoogleApiClient.Builder(this)
                    .AddConnectionCallbacks(this)
                    .AddOnConnectionFailedListener(this)
                    .AddApi(LocationServices.API)
                    // Uncomment to test the error recovery code.
                    //.AddApi(PlusClass.API)
                    //.AddScope(PlusClass.ScopePlusLogin)
                    .Build();
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (apiClient != null && map != null)
            {
                apiClient.Connect();
            }
        }
        protected override void OnStop()
        {
            base.OnStop();
            if (apiClient != null)
            {
                apiClient.Disconnect();
            }
        }

        public async void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;

            map.MapType = GoogleMap.MapTypeHybrid;
            map.UiSettings.ZoomControlsEnabled = true;

            // Make sure we have user permission for location use before we call any methods requiring permission.
            await getLocationPermissionsAsync;

            Criteria criteria = new Criteria
            {
                Accuracy = Accuracy.Fine,
                PowerRequirement = Power.NoRequirement,
            };

            if (apiClient != null)
            {
                if (!apiClient.IsConnected)
                    apiClient.Connect();
            }
            else
            {
                LocationManager locManager = LocationManager.FromContext(this);
                locManager.RequestLocationUpdates(5000, 100f, criteria, this, null);
            }
        }

        public void OnLocationChanged(Location location)
        {
            LatLng coord = new LatLng(location.Latitude, location.Longitude);
            CameraUpdate update = CameraUpdateFactory.NewLatLngZoom(coord, 17);
            map.AnimateCamera(update);

            if (currentLocationMarker != null)
            {
                currentLocationMarker.Position = coord;
            }
            else
            {
                var markerOptions = new MarkerOptions()
                    .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue))
                    .SetPosition(coord)
                    .SetTitle("Current Position")
                    .SetSnippet("This is where you are now");
                currentLocationMarker = map.AddMarker(markerOptions);
            }
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            permissionHelper.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnConnected(Bundle connectionHint)
        {
            LocationRequest locationRequest = LocationRequest.Create()
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetInterval(5000)
                .SetSmallestDisplacement(100f);

            LocationServices.FusedLocationApi.RequestLocationUpdates(apiClient, locationRequest, this);
        }

        public void OnConnectionSuspended(int cause)
        {
        }

        bool resolvingError;
        const int ResolvePlayErrorRequestId = 1001;

        public void OnConnectionFailed(ConnectionResult result)
        {
            if (!result.HasResolution)
            {
                GoogleApiAvailability.Instance.GetErrorDialog(this, result.ErrorCode, 0).Show();
                return;
            }

            if (!resolvingError && result.HasResolution)
            {
                resolvingError = true;
                try
                {
                    result.StartResolutionForResult(this,
                        ResolvePlayErrorRequestId);
                }
                catch (Android.Content.IntentSender.SendIntentException)
                {
                    // Canceled - restart connection
                    resolvingError = false;
                    apiClient.Connect();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (requestCode == ResolvePlayErrorRequestId)
            {
                resolvingError = false;
                if (resultCode == Android.App.Result.Ok)
                {
                    if (!apiClient.IsConnecting
                        && !apiClient.IsConnected)
                    {
                        apiClient.Connect();
                    }
                }
                else if (resultCode == Android.App.Result.Canceled)
                {
                    // Make sure you have enabled Google Plus API access, and added a corresponding OAuth credential, in the Google developer console.
                    // https://console.developers.google.com/
                    // Without the API access, user sign-in will loop between a Canceled result and OnConnectionFailed.
                }
            }
        }
    }
}
