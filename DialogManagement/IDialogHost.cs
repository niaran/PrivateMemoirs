using System.Windows;

namespace DialogManagement
{
	interface IDialogHost
	{
		void ShowDialog(DialogBaseControl dialog);
		void HideDialog(DialogBaseControl dialog);
		FrameworkElement GetCurrentContent();
	}
}