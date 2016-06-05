using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Views;

namespace BindingInflatePerformanceTest.Android
{
	[Activity(
		Label = "BindingInflatePerformanceTest.Android"
		, MainLauncher = true
		, Icon = "@mipmap/icon"
		, Theme = "@style/Theme.Splash"
		, NoHistory = true
		, ScreenOrientation = ScreenOrientation.Portrait)]
	public class SplashScreen : MvxSplashScreenActivity
	{
		public SplashScreen()
			: base(Resource.Layout.SplashScreen)
		{
		}
	}
}

