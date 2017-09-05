using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Common;

namespace DroidMapping
{
    [Activity(Label = "DroidMapping", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback, 
                Android.Locations.ILocationListener,
                Android.Gms.Location.ILocationListener,
				GoogleApiClient.IConnectionCallbacks,
				GoogleApiClient.IOnConnectionFailedListener
    {
        GoogleMap map;
        MapFragment mapFragment;
        GoogleApiClient apiClient;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Find and load the map fragment
            mapFragment = FragmentManager.FindFragmentById(Resource.Id.map) as MapFragment;
            mapFragment.GetMapAsync(this);

            if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this) == 0) 
			{
                apiClient = new GoogleApiClient.Builder(this)
                    .AddApi(LocationServices.API)
					.AddOnConnectionFailedListener(this)
					.AddConnectionCallbacks(this)
// Uncomment to test the error recovery code.
//                    .AddApi(PlusClass.API)
//                    .AddScope(PlusClass.ScopePlusLogin)
                    .Build();
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (apiClient != null && map != null) {
                apiClient.Connect();
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (apiClient != null) {
                apiClient.Disconnect();
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;

            map.MapType = GoogleMap.MapTypeHybrid;
            map.UiSettings.ZoomControlsEnabled = true;

            Criteria criteria = new Criteria {
                Accuracy = Accuracy.Fine,
                PowerRequirement = Power.NoRequirement,
            };

            if (apiClient != null) {
                if (!apiClient.IsConnected)
                    apiClient.Connect();
            }
            else {
                LocationManager locManager = LocationManager.FromContext(this);
                locManager.RequestLocationUpdates(5000, 100f, criteria, this, null);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            LocationRequest locationRequest = LocationRequest.Create()
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetInterval(5000)
                .SetSmallestDisplacement(100f);
            
            LocationServices.FusedLocationApi.RequestLocationUpdates(
                apiClient, locationRequest, this);
        } 

        public void OnConnectionSuspended(int cause)
        {
        }

        bool resolvingError;
        const int ResolvePlayErrorRequestId = 1001;

        public void OnConnectionFailed(ConnectionResult result)
        {
            if (result.HasResolution) {
                if (!resolvingError) {
                    resolvingError = true;
                    try {
                    result.StartResolutionForResult(this, 
                        ResolvePlayErrorRequestId);
                    }
                    catch (Android.Content.IntentSender.SendIntentException) {
                        // Canceled - restart connection
                        resolvingError = false;
                        apiClient.Connect();
                    }
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (requestCode == ResolvePlayErrorRequestId) {
                resolvingError = false;
                if (resultCode == 0) {
                    if (!apiClient.IsConnecting 
                        && !apiClient.IsConnected) {
                        apiClient.Connect();
                    }
                }
            }
        }

        Marker currentLocationMarker;

        public void OnLocationChanged(Location location)
        {
            LatLng coord = new LatLng(location.Latitude, location.Longitude);
            CameraUpdate update = CameraUpdateFactory.NewLatLngZoom(coord, 17);
            map.AnimateCamera(update);

            if (currentLocationMarker != null) {
                currentLocationMarker.Position = coord;
            }
            else {
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

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }
    }
}

