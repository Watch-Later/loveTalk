using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using InTheHand;
using InTheHand.Bluetooth;


namespace loveTalk
{

    // 5a300001-0023-4bd4-bbd5-a6920e4c5653
    public class btCharacteristicSession
    {
        public BluetoothDevice device;
        public GattCharacteristic characteristic;
        
        public bool connectToCharacteristic(BluetoothDevice dev, string svc)
        {
            device = dev;
            dev.Gatt.ConnectAsync().Wait();
            try {
                var devsvc = dev.Gatt.GetPrimaryServiceAsync(BluetoothUuid.FromGuid(new Guid(svc))).Result;
                characteristic = devsvc.GetCharacteristicsAsync().Result[0];
                return true;
            } catch { return false; }
        }

    }

    public static class BTManager
    {
        public static BluetoothDevice[] pairedDevices;
        public static BluetoothDevice[] getDevices()
        {           
            var cl1 = Bluetooth.GetPairedDevicesAsync().Result;
            return cl1.ToArray<BluetoothDevice>();
        }

    }
}
