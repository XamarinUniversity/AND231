using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace DroidMapping
{
    [Activity(Label = "DroidMapping", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback
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

            // Center on Xamarin HQ
            LatLng coord = new LatLng(37.797679, -122.401816);
            CameraUpdate update = CameraUpdateFactory.NewLatLngZoom(coord, 17);
            map.AnimateCamera(update);
        }
    }
}

