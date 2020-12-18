using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Bluetooth;
using System.Diagnostics;
using System.Runtime;


namespace loveTalk
{
    public enum loveToyStatus
    {
       OK = 1,
       ERROR = 1 << 1, 
       CONNECTING = 1 << 2,
       DISCONNECT = 1 << 3
    }
    public class loveToy
    {
        public BluetoothDevice btDevice;
        public GattService btSevice;
        public string Model = "UNKNOWN";
        public loveToyStatus Status;
        public loveToyController Controller;

        private GattCharacteristic btTxChr;
        private GattCharacteristic btRxChr;


        private string[] serviceUID = new string[]
        {
            "0000fff0-0000-1000-8000-00805f9b34fb", // V1
            "6e400001-b5a3-f393-e0a9-e50e24dcca9e", // V2
            "50300001-0024-4bd4-bbd5-a6920e4c5653", // V3
            "57300001-0023-4bd4-bbd5-a6920e4c5653", // V4
            "5a300001-0023-4bd4-bbd5-a6920e4c5653", // V5 firmware
            "42300001-0023-4bd4-bbd5-a6920e4c5653"
        };
        private string[] rxCharacteristics = new string[]
        {
            "0000fff1-0000-1000-8000-00805f9b34fb", // V1 toy
            "6e400003-b5a3-f393-e0a9-e50e24dcca9e", // V2 toy
            "50300003-0024-4bd4-bbd5-a6920e4c5653", // V3 toy
            "57300003-0023-4bd4-bbd5-a6920e4c5653", // V4 toy
            "5a300003-0023-4bd4-bbd5-a6920e4c5653", // V5 firmware
            "42300003-0023-4bd4-bbd5-a6920e4c5653"

        };

        private string[] txCharacteristics = new string[]
        {
            "0000fff2-0000-1000-8000-00805f9b34fb", // V1 toy
            "6e400002-b5a3-f393-e0a9-e50e24dcca9e", // V2 toy
            "50300002-0024-4bd4-bbd5-a6920e4c5653", // V3 toy 
            "57300002-0023-4bd4-bbd5-a6920e4c5653", // V4 toy
            "5a300002-0023-4bd4-bbd5-a6920e4c5653", // V5 firmware
            "42300002-0023-4bd4-bbd5-a6920e4c5653",
        };
         

        public loveToy(BluetoothDevice dev)
        {
            btDevice = dev; 
        }

        private string lookupToyName(char model)
        {
            switch (model)
            {
                case 'C':
                case 'A':
                    return "Nora";
                case 'B':
                    return "Max";
                case 'L':
                    return "Ambi";
                case 'S':
                    return "Lush";
                case 'Z':
                    return "Hush";
                case 'W':
                    return "Domi";
                case 'P':
                    return "Edge";
                case 'O':
                default:
                    return "UNKNOWN";
            }
        }

        public async Task<bool> connect()
        {
            Debug.WriteLine("Connecting to GATT");
            await btDevice.Gatt.ConnectAsync();
            for (int i=0; i < rxCharacteristics.Length; i++) {
                var service = serviceUID[i];
                var rxChar = rxCharacteristics[i];
                var txChar = txCharacteristics[i];
                try
                {
                    
                    Debug.WriteLine("Connecting to service");
                    btSevice = await btDevice.Gatt.GetPrimaryServiceAsync(BluetoothUuid.FromGuid(new Guid(service)));
                    if (btSevice == null)
                        continue; // skip iteration, it was bad. 
                } catch { Debug.WriteLine("Failed to connect to toy");  continue ; }
                try
                {
                    Debug.WriteLine("Getting TXRX");
                    btRxChr = (await btSevice.GetCharacteristicAsync(new Guid(rxChar)));
                    btTxChr = (await btSevice.GetCharacteristicAsync(new Guid(txChar)));
                    if (btRxChr == null || btTxChr == null)
                        continue; // skip iteration, not good. 
                    Debug.WriteLine("Success, got TXRX");
                    break;
                }
                catch { Debug.WriteLine("Failed to connect to toy (characteristic)") ;  continue;  }
            }
            if (btSevice == null || btRxChr == null || btTxChr == null)
            {
                Debug.WriteLine("Service or characteristic was null!");
                return false;
            }
            var devInfo = sendCommand("DeviceType;").Result;
            Model = lookupToyName(devInfo[0]);
            Debug.WriteLine($"Toy type connected . . . {Model}");
            return true;
        }

 

        public void sendCommandNoResponse(string command)
        {
            Debug.WriteLine($"sendCommandNR -> {command}");
            var packet = Encoding.ASCII.GetBytes(command);
            btTxChr.WriteValueWithoutResponseAsync(packet);
        }
       
        public async Task<string> sendCommand(string command)
        {
            var packet = Encoding.ASCII.GetBytes(command);
            await btRxChr.StartNotificationsAsync(); // Subscribe to BTCHAR notifications
            await btTxChr.WriteValueWithResponseAsync(packet);
            Debug.WriteLine($"sendCommand -> {command}");
            // xayrga.bop.lengthEncapsulateArray(packet)
            return Encoding.ASCII.GetString(btRxChr.Value);
        }

        private int vibrationDebounce = 0; 
        public void setVibration(float value)
        {
            var normalized = (int)Math.Floor(Math.Min(20, Math.Max(0, value * 20)));
            if (normalized == vibrationDebounce)
                return; 
            sendCommandNoResponse($"Vibrate:{normalized};");
            vibrationDebounce = normalized;
        }

        private int rotateDebounce = 0;
        public void setRotation(float value)
        {
            var normalized = (int)Math.Floor(Math.Min(20, Math.Max(0, value * 20)));
            if (normalized == rotateDebounce)
                return;
            sendCommandNoResponse($"Rotate:{normalized};");
            rotateDebounce = normalized;
        }

        private int airDebounce = 0;
        public void setAirLevelPattern(float value)
        {
            var normalized = (int)Math.Floor(Math.Min(5, Math.Max(0, value * 5)));
            if (normalized == airDebounce)
                return;
            sendCommandNoResponse($"Air:Level:{normalized};");
            airDebounce = normalized;
        }


        private int airStaticDebounce = 0;
        private int airLastRelativeValue = 0; 
        public async void setAirLevelAbsolute(float value)
        {
            var normalized = (int)Math.Floor(Math.Min(8, Math.Max(0, value * 8)));

            var delta = normalized - airLastRelativeValue;
            var newValue = Math.Min(8, Math.Max(0, airLastRelativeValue + delta));
            
            if (newValue == airStaticDebounce)
                return;

            for (int i = 0; i < Math.Abs(delta); i++)
            {
                if (delta > 0)
                {
                    sendCommandNoResponse($"Air:In:1;");
                }
                else if (delta < 0)
                {
                    sendCommandNoResponse($"Air:Out:1;");
                }
                await Task.Delay(100);
            }
            airStaticDebounce = normalized;
        }
    }
}
