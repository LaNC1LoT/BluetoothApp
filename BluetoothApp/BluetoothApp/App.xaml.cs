using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BluetoothApp.Views;
using BluetoothApp.Services;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace BluetoothApp
{
    public partial class App : Application
    {
        IBluetoothService _bluetoothService;
        public App(IBluetoothService bluetoothService)
        {
            _bluetoothService = bluetoothService;
            InitializeComponent();
            MainPage = new MainPage(bluetoothService);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
