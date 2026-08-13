using System;
using System.Collections.Generic;
using System.Linq;
using HidLibrary;
using Vortice.XInput;
using DriftLock.Models;
namespace DriftLock.Core.Input
{
    public class DeviceEnumerator
    {
        private static readonly HashSet<int> PlayStationVendorIds = new() { 0x054C, 0x073A, 0x0F0D, 0x146B, 0x7359 };
        public static List<IPhysicalController> GetConnectedControllers()
        {
            var result = new List<IPhysicalController>();
            var psDevices = new List<PlayStationController>();
            try
            {
                var hidDevices = HidDevices.Enumerate()
                    .Where(d => PlayStationVendorIds.Contains(d.Attributes.VendorId) 
                             || (d.Description != null && (d.Description.Contains("DualSense", StringComparison.OrdinalIgnoreCase) 
                                                        || d.Description.Contains("DualShock", StringComparison.OrdinalIgnoreCase)
                                                        || d.Description.Contains("Wireless Controller", StringComparison.OrdinalIgnoreCase))));
                foreach (var dev in hidDevices)
                {
                    try
                    {
                        var psCtrl = new PlayStationController(dev);
                        if (!psDevices.Any(p => p.DeviceId == psCtrl.DeviceId))
                        {
                            psDevices.Add(psCtrl);
                        }
                    }
                    catch { }
                }
            }
            catch { }
            result.AddRange(psDevices);
            try
            {
                var realXboxVendorIds = new HashSet<int> { 0x045E, 0x0738, 0x0F0D, 0x1532, 0x24C6, 0x1BAD, 0x046D, 0x0079, 0x0E6F };
                int realXboxHidCount = 0;
                try
                {
                    realXboxHidCount = HidDevices.Enumerate()
                        .Where(d => realXboxVendorIds.Contains(d.Attributes.VendorId)
                                 && !PlayStationVendorIds.Contains(d.Attributes.VendorId))
                        .Count();
                }
                catch { }
                int xboxAdded = 0;
                for (uint i = 0; i < 4; i++)
                {
                    if (XInput.GetState(i, out _))
                    {
                        if (psDevices.Count > 0 && realXboxHidCount == 0)
                        {
                            continue;
                        }
                        if (realXboxHidCount > 0 && xboxAdded >= realXboxHidCount)
                        {
                            continue;
                        }
                        result.Add(new XboxController(i));
                        xboxAdded++;
                    }
                }
            }
            catch { }
            return result;
        }
    }
}
