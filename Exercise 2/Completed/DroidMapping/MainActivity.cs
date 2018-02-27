using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Gms.Maps;
using System.Threading.Tasks;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Runtime;

namespace DroidMapping
{
    [Activity(Label = "DroidMapping", MainLauncher = true)]
    public class MainActivity : Activity, IOnMapReadyCallback, ILocationListener
    {
        MappingPermissionsHelper permissionHelper;
        MapFragment mapFragment;
        GoogleMap map;
        Task<bool> getLocationPermissionsAsync;
        Marker currentLocationMarker;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            permissionHelper = new MappingPermissionsHelper(this);
            getLocationPermissionsAsync = permissionHelper.CheckAndRequestPermissions();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mapFragment = FragmentManager.FindFragmentById(Resource.Id.map) as MapFragment;

            mapFragment.GetMapAsync(this);
        }

        public async void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;

            map.MapType = GoogleMap.MapTypeHybrid;
            map.UiSettings.ZoomControlsEnabled = true;

            // Make sure we have user permission for location use before we call any methods requiring permission.
            await getLocationPermissionsAsync;
            LocationManager locManager = LocationManager.FromContext(this);
            locManager.RequestLocationUpdates(LocationManager.GpsProvider, 5000, 100f, this);
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
    }
}
