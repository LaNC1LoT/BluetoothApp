using BluetoothApp.Services;
using BluetoothApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BluetoothApp.Views
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage(IBluetoothService bluetoothService)
        {
            BindingContext = new MainViewModel(bluetoothService);
            InitializeComponent();
        }
    }
}