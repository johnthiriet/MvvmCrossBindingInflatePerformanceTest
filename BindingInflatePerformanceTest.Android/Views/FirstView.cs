using Android.App;
using Android.OS;
using BindingInflatePerformanceTest.Core.ViewModels;
using MvvmCross.Droid.Views;

namespace BindingInflatePerformanceTest.Android
{
	[Activity(Label = "View for FirstViewModel")]
	public class FirstView : MvxActivity<FirstViewModel>
	{
		private int i = 0;
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.FirstView);
		}

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet();
		}

		protected override void OnResume()
		{
			base.OnResume();
			ShowSecondViewModel();
		}

		private async void ShowSecondViewModel()
		{
			if (ViewModel != null && i++ < 20)
			{
				await System.Threading.Tasks.Task.Delay(100);
				ViewModel.ShowSecondViewModel();
			}
		}
	}
}

