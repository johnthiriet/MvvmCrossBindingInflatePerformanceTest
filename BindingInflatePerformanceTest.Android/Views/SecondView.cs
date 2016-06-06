using Android.App;
using Android.OS;
using BindingInflatePerformanceTest.Core.ViewModels;
using MvvmCross.Droid.Views;

namespace BindingInflatePerformanceTest.Android
{
	[Activity(Label = "View for SecondViewModel")]
	public class SecondView : MvxActivity<SecondViewModel>
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.FirstView);
		}

		protected async override void OnResume()
		{
			base.OnResume();
			await System.Threading.Tasks.Task.Delay(1000);
			ViewModel.CloseMe();
		}
	}
}

