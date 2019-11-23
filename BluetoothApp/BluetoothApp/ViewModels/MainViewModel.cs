using BluetoothApp.Services;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BluetoothApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private ObservableCollection<string> _devices = new ObservableCollection<string>();
        public ObservableCollection<string> Device
        {
            get => _devices;
            set { SetProperty(ref _devices, value); }
        }

        private bool _isEnabledScan = false;
        public bool IsEnabledScan
        {
            get => _isEnabledScan;
            set { SetProperty(ref _isEnabledScan, value); }
        }

        private bool _isEnabledStop = false;
        public bool IsEnabledStop
        {
            get => _isEnabledStop;
            set { SetProperty(ref _isEnabledStop, value); }
        }

        private string _btnScanText = "Scan";
        public string BtnScanText
        {
            get => _btnScanText;
            set { SetProperty(ref _btnScanText, value); }
        }



        public ICommand ScanCommand { protected set; get; }
        public ICommand StopCommand { protected set; get; }
        private readonly IBluetoothService BluetoothService;


        public MainViewModel(IBluetoothService bluetoothService)
        {
            BluetoothService = bluetoothService;
            ScanCommand = new Command(Scan);
            StopCommand = new Command(Stop);

            BluetoothService.OnDeviceLoaded += (s, e) =>
            {
                Device.Add(string.IsNullOrWhiteSpace(e.Device.Name) ? "Unknown" : e.Device.Name);
            };

            IsEnabledScan = IsEnabledStop = BluetoothService.IsActive;

            BluetoothService.OnStatusChanged += (s, e) =>
            {
                if (e.State == BluetoothState.On || e.State == BluetoothState.TurningOn)
                    IsEnabledScan = IsEnabledStop = true;
                else
                {
                    Stop();
                } 
            };
        }

        private async void ShowMessage(string title, string message)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }

        private CancellationTokenSource cancelTokenSource;
        private CancellationToken token;
        private async void Scan()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;

            try
            {
                IsEnabledScan = false;
                Device.Clear();

                if (!await BluetoothService.CheckPermissionAsync())
                    ShowMessage("Bluetooth", "Вклюючите права в настройках!");
                else
                {
                    BtnScanText = "Scan Started";
                    await Task.WhenAll(TimeWork(token), BluetoothService.StartScan(token));
                    BtnScanText = "Scan Finished";
                }
            }
            catch (OperationCanceledException)
            {
                BtnScanText = "Scan Stoped";
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка!", $"{ex.InnerException?.Message ?? ex.Message}");
            }
            finally
            {
                IsEnabledStop = IsEnabledScan = BluetoothService.IsActive;
            }
        }

        private async Task TimeWork(CancellationToken token)
        {
            var time = BluetoothService.TimeScan / 1000;
            while (time >= 0)
            {
                await Task.Delay(1000);
                BtnScanText = $"Scan({--time})";
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Операция прервана токеном");
                }
            }
        }

        private void Stop()
        {
            try
            {
                IsEnabledStop = false;
                if(!token.IsCancellationRequested)
                    cancelTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка!", $"{ex.InnerException?.Message ?? ex.Message}");
            }
            finally
            {
                cancelTokenSource?.Dispose();
                
                IsEnabledStop = IsEnabledScan = BluetoothService.IsActive;
            }
        }
    }
}
