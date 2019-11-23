using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using BluetoothApp.Services;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.CurrentActivity;
using IAdapter = Plugin.BLE.Abstractions.Contracts.IAdapter;

namespace BluetoothApp.Droid.Services
{
    public class BluetoothService: IBluetoothService
    {
        private readonly IBluetoothLE Bluetooth;
        private readonly IAdapter Adapter;

        public event EventHandler<BluetoothEventArgs> OnDeviceLoaded;
        public event EventHandler<StatusBluetoothEventArgs> OnStatusChanged;

        public BluetoothService()
        {
            Bluetooth = CrossBluetoothLE.Current;
            Adapter = CrossBluetoothLE.Current.Adapter;

            Adapter.DeviceDiscovered += (s, e) =>
            {
                OnDeviceLoaded?.Invoke(this, new BluetoothEventArgs(e.Device));
            };

            Bluetooth.StateChanged += (s, e) =>
            {
                OnStatusChanged?.Invoke(this, new StatusBluetoothEventArgs(e.NewState));
            };

        }

        public bool IsActive => Bluetooth.IsOn;

        public int TimeScan => Adapter.ScanTimeout;

        public async Task<bool> CheckPermissionAsync()
        {
            return await RequestPermissionAsync();
        }

        public Task StartScan(CancellationToken token)
        {
            return Adapter.StartScanningForDevicesAsync(cancellationToken: token);
        }

        public Task StopScan()
        {
            return Adapter.StopScanningForDevicesAsync();
        }

        #region Permission

        public const int RequestBluetooth = 1239;
        private static TaskCompletionSource<bool> contactPermissionTcs;
        public static void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == RequestBluetooth)
            {
                if (PermissionUtil.VerifyPermissions(grantResults))
                {
                    contactPermissionTcs.TrySetResult(true);
                }
                else
                {
                    contactPermissionTcs.TrySetResult(false);
                }
            }
        }

        private static readonly string[] PrimissionBluetooth =
        {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.AccessFineLocation
        };
        private async void RequestContactsPermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.Bluetooth)
                || ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.BluetoothAdmin)
                || ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.AccessCoarseLocation)
                || ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.AccessFineLocation)
                )
            {
                await UserDialogs.Instance.AlertAsync("Bluetooth Permission", "This action requires bluetooth permission", "Ok");
            }
            else
            {
                ActivityCompat.RequestPermissions(CrossCurrentActivity.Current.Activity, PrimissionBluetooth, RequestBluetooth);
            }
        }

        public async Task<bool> RequestPermissionAsync()
        {
            contactPermissionTcs = new TaskCompletionSource<bool>();

            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.Activity, Manifest.Permission.Bluetooth) != (int)Permission.Granted
                || Android.Support.V4.Content.ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.Activity, Manifest.Permission.BluetoothAdmin) != (int)Permission.Granted
                || Android.Support.V4.Content.ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.Activity, Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted
                || Android.Support.V4.Content.ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.Activity, Manifest.Permission.AccessFineLocation) != (int)Permission.Granted
                )
            {
                RequestContactsPermissions();
            }
            else
            {
                contactPermissionTcs.TrySetResult(true);
            }

            return await contactPermissionTcs.Task;
        }

        #endregion

    }
}