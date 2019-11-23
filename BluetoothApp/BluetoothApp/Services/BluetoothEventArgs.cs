using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothApp.Services
{
    public class BluetoothEventArgs : EventArgs
    {
        public IDevice Device { get; }
        public BluetoothEventArgs(IDevice device)
        {
            Device = device;
        }
    }

    public class StatusBluetoothEventArgs : EventArgs
    {
        public BluetoothState State { get; }
        public StatusBluetoothEventArgs(BluetoothState state)
        {
            State = state;
        }
    }

    public interface IBluetoothService
    {
        event EventHandler<BluetoothEventArgs> OnDeviceLoaded;
        event EventHandler<StatusBluetoothEventArgs> OnStatusChanged;

        int TimeScan { get; }
        bool IsActive { get; }

        Task<bool> CheckPermissionAsync();
        Task StartScan(CancellationToken token);
        Task StopScan();
    }

}
