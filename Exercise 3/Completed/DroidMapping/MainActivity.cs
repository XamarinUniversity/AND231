using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;

namespace DroidMapping
{
    [Activity(Label = "DroidMapping", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback, ILocationListener
    {
        GoogleMap map;
        MapFragment mapFragment;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Find and load the map fragment
            mapFragment = FragmentManager.FindFragmentById(Resource.Id.map) as MapFragment;
            mapFragment.GetMapAsync(this);
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

            LocationManager locManager = LocationManager.FromContext(this);
            locManager.RequestLocationUpdates(5000, 100f, criteria, this, null);
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

