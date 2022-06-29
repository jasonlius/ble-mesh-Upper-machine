// Copyright M. Griffie <nexus@nexussays.com>
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using ble.net.sampleapp.util;
using nexus.core.logging;
using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using Xamarin.Forms;
namespace ble.net.sampleapp.viewmodel
{
   public class BleDeviceScannerViewModel : AbstractScanViewModel
   {
      private readonly Func<BlePeripheralViewModel, Task> m_onSelectDevice;
      private readonly Func<BlePeripheralViewModel, Task> m_offSelectDevice;
      private DateTime m_scanStopTime;
      public int FoundDevicesCount { get; set; }//设备数量
      public int FoundDevicesOnlineCount { get; set; }//设备连接
      public BleDeviceScannerViewModel( IBluetoothLowEnergyAdapter bleAdapter, IUserDialogs dialogs,
                                        Func<BlePeripheralViewModel, Task> onSelectDevice, Func<BlePeripheralViewModel, Task> offSelectDevice)
         : base( bleAdapter, dialogs )
      {
         m_onSelectDevice = onSelectDevice;
         m_offSelectDevice = offSelectDevice;
         FoundDevices = new ObservableCollection<BlePeripheralViewModel>();
         ScanForDevicesCommand =
            new Command( x => { StartScan( x as Double? ?? BleSampleAppUtils.SCAN_SECONDS_DEFAULT ); } );
      }

      public void UpdateFoundDevicesOnlineCount(int foundDevicesOnlineCount)
      {
         FoundDevicesOnlineCount += foundDevicesOnlineCount;
         RaisePropertyChanged(nameof(FoundDevicesOnlineCount));
      }
      public ObservableCollection<BlePeripheralViewModel> FoundDevices { get; }

      public ICommand ScanForDevicesCommand { get; }

      public Int32 ScanTimeRemaining =>
         (Int32)BleSampleAppUtils.ClampSeconds( (m_scanStopTime - DateTime.UtcNow).TotalSeconds );

      private async void StartScan( Double seconds )
        {
         if(IsScanning)
         {
            return;         }

         if(!IsAdapterEnabled)
         {
            m_dialogs.Toast( "Cannot start scan, Bluetooth is turned off" );
            return;
         }

         StopScan();
         IsScanning = true;
         seconds = BleSampleAppUtils.ClampSeconds( seconds );
         m_scanCancel = new CancellationTokenSource( TimeSpan.FromSeconds( seconds ) );
         m_scanStopTime = DateTime.UtcNow.AddSeconds( seconds );

         Log.Trace( "Beginning device scan. timeout={0} seconds", seconds );

         RaisePropertyChanged( nameof(ScanTimeRemaining) );
         // RaisePropertyChanged of ScanTimeRemaining while scan is running
         Device.StartTimer(
            TimeSpan.FromSeconds( 1 ),
            () =>
            {
               RaisePropertyChanged( nameof(ScanTimeRemaining) );
               return IsScanning;
            } );

         await m_bleAdapter.ScanForBroadcasts(

            new ScanSettings()
            {
               // Setting the scan mode is currently only applicable to Android and has no effect on other platforms.
               // If not provided, defaults to ScanMode.Balanced
               Mode = ScanMode.LowPower,

               // Optional scan filter to ensure that the observer will only receive peripherals
               // that pass the filter. If you want to scan for everything around, omit the filter.
               Filter = new ScanFilter()
               {
                  //AdvertisedDeviceName = "foobar",
                  //AdvertisedManufacturerCompanyId = 76,
                  // peripherals must advertise at-least-one of any GUIDs in this list
                  AdvertisedServiceIsInList = new List<Guid>() { new Guid("00001828-0000-1000-8000-00805f9b34fb") },
               },

               // ignore repeated advertisements from the same device during this scan
               IgnoreRepeatBroadcasts = false
            },
            peripheral =>
            {
               Device.BeginInvokeOnMainThread(
                  () =>
                  {
                     var existing = FoundDevices.FirstOrDefault( d => d.Equals( peripheral ) );
                     if(existing != null)
                     {
                        existing.Update( peripheral );
                     }
                     else
                     {
            
                        FoundDevices.Add( new BlePeripheralViewModel( peripheral, m_onSelectDevice ,m_offSelectDevice) );
                        
                     }
                     FoundDevicesCount = FoundDevices.Count();
                     RaisePropertyChanged(nameof(FoundDevicesCount));
                  } );
              
            },
            
            m_scanCancel.Token );
         IsScanning = false;
 
      }
    
   }
}
