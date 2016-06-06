using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;

namespace BindingInflatePerformanceTest.Core.ViewModels
{
	public class SecondViewModel
		: MvxViewModel
	{
		private string _hello = "Hello MvvmCross";
		public string Hello
		{
			get { return _hello; }
			set { SetProperty(ref _hello, value); }
		}

		public void CloseMe()
		{
			Close(this);
		}
	}
}
