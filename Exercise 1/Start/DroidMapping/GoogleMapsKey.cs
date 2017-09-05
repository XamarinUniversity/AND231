using Android.App;

#if RELEASE
[assembly: MetaDataAttribute("com.google.android.geo.API_KEY", Value="release_key_goes_here")]
#else
[assembly: MetaDataAttribute("com.google.android.geo.API_KEY", Value="debug_key_goes_here")]
#endif