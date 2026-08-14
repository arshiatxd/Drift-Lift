using System;
using System.Collections.Generic;
using System.Linq;
using HidLibrary;
using Vortice.XInput;
using DriftLift.Models;

namespace DriftLift.Core.Input
{
    public class DeviceEnumerator
    {
        // ##== Vendor and Product IDs ==##
        public static readonly HashSet<int> PlayStationVendorIds = new() { 0x054C, 0x073A, 0x0F0D, 0x146B, 0x7359 };
        private static readonly HashSet<int> Xbox360ProductIds = new() { 0x028E, 0x028F, 0x0291, 0x02A1, 0x0719, 0x02A0 };
        private static readonly HashSet<int> Xbox360VendorIds = new() { 0x1BAD, 0x0738, 0x0E6F, 0x24C6, 0x1689 };

        public static bool IsVirtualDevice(HidDevice? device)
        {
            if (device == null) return false;
            string path = (device.DevicePath ?? string.Empty).ToLowerInvariant();
            string desc = (device.Description ?? string.Empty).ToLowerInvariant();

            // Filter out virtual/emulated buses (ViGEmBus, Nefarius, virtual systems)
            if (path.Contains("root#system") || path.Contains(@"root\system") || path.Contains("vigem") 
                || path.Contains("virtual") || path.Contains("nsoftware") || path.Contains("spaceport")
                || path.Contains("amdxe") || path.Contains("rainway"))
            {
                return true;
            }

            if (desc.Contains("virtual") || desc.Contains("emulation") || desc.Contains("vigem"))
            {
                return true;
            }

            return false;
        }

        public static string ExtractInstanceId(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            string p = path.Trim();
            if (p.StartsWith(@"\\?\") || p.StartsWith(@"\\.\")) p = p[4..];
            else if (p.StartsWith(@"\??\")) p = p[4..];

            // Truncate the trailing device interface GUID #{...}
            int lastHashIndex = p.LastIndexOf('#');
            if (lastHashIndex > 0 && p.IndexOf('{', lastHashIndex) > 0)
            {
                p = p[..lastHashIndex];
            }
            else
            {
                int guidStart = p.IndexOf('{');
                if (guidStart > 0 && p.EndsWith("}"))
                {
                    int prevHash = p.LastIndexOf('#', guidStart);
                    if (prevHash > 0) p = p[..prevHash];
                }
            }

            return p.Replace('#', '\\').ToUpperInvariant();
        }

        public static List<string> GetAllPlayStationDeviceInstanceIds()
        {
            var list = new List<string>();
            try
            {
                var allHid = HidDevices.Enumerate();
                foreach (var dev in allHid)
                {
                    if (IsVirtualDevice(dev)) continue;

                    int vid = dev.Attributes.VendorId;
                    string desc = dev.Description ?? string.Empty;

                    if (PlayStationVendorIds.Contains(vid)
                        || desc.Contains("DualSense", StringComparison.OrdinalIgnoreCase)
                        || desc.Contains("DualShock", StringComparison.OrdinalIgnoreCase)
                        || desc.Contains("Wireless Controller", StringComparison.OrdinalIgnoreCase))
                    {
                        string id = ExtractInstanceId(dev.DevicePath);
                        if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                        {
                            list.Add(id);
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        public static List<IPhysicalController> GetConnectedControllers()
        {
            var result = new List<IPhysicalController>();

            // ##== PlayStation Enumeration ==##
            var psDevices = new List<PlayStationController>();
            try
            {
                var hidDevices = HidDevices.Enumerate()
                    .Where(d => !IsVirtualDevice(d) && (PlayStationVendorIds.Contains(d.Attributes.VendorId) 
                             || (d.Description != null && (d.Description.Contains("DualSense", StringComparison.OrdinalIgnoreCase) 
                                                         || d.Description.Contains("DualShock", StringComparison.OrdinalIgnoreCase)
                                                         || d.Description.Contains("Wireless Controller", StringComparison.OrdinalIgnoreCase)))));
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

            // ##== Xbox Enumeration ==##
            try
            {
                var realXboxVendorIds = new HashSet<int> { 0x045E, 0x0738, 0x0F0D, 0x1532, 0x24C6, 0x1BAD, 0x046D, 0x0079, 0x0E6F };
                var allHid = HidDevices.Enumerate().ToList();

                var xboxHids = allHid
                    .Where(d => !IsVirtualDevice(d)
                             && realXboxVendorIds.Contains(d.Attributes.VendorId)
                             && !PlayStationVendorIds.Contains(d.Attributes.VendorId))
                    .ToList();

                int realXboxHidCount = xboxHids.Count;

                int xboxAdded = 0;
                for (uint i = 0; i < 4; i++)
                {
                    if (XInput.GetState(i, out _))
                    {
                        // If no physical Xbox HID hardware exists, any XInput slot is a virtual pad (e.g. ViGEm)
                        if (realXboxHidCount == 0)
                        {
                            continue;
                        }
                        if (xboxAdded >= realXboxHidCount)
                        {
                            continue;
                        }

                        // Determine if connected device is Xbox 360
                        bool is360 = false;
                        string devName = $"Xbox Controller ({i + 1})";

                        if (xboxHids.Count > xboxAdded)
                        {
                            var matchingHid = xboxHids[xboxAdded];
                            int vid = matchingHid.Attributes.VendorId;
                            int pid = matchingHid.Attributes.ProductId;
                            string desc = matchingHid.Description ?? string.Empty;

                            if (Xbox360VendorIds.Contains(vid) 
                                || (vid == 0x045E && Xbox360ProductIds.Contains(pid))
                                || desc.Contains("360", StringComparison.OrdinalIgnoreCase))
                            {
                                is360 = true;
                                devName = $"Xbox 360 Controller ({i + 1})";
                            }
                            else
                            {
                                devName = $"Xbox Wireless Controller ({i + 1})";
                            }
                        }
                        else
                        {
                            if (xboxHids.Any(h => Xbox360ProductIds.Contains(h.Attributes.ProductId) 
                                               || Xbox360VendorIds.Contains(h.Attributes.VendorId) 
                                               || (h.Description != null && h.Description.Contains("360", StringComparison.OrdinalIgnoreCase))))
                            {
                                is360 = true;
                                devName = $"Xbox 360 Controller ({i + 1})";
                            }
                        }

                        var ctrlType = is360 ? ControllerType.Xbox360 : ControllerType.Xbox;
                        result.Add(new XboxController(i, ctrlType, devName));
                        xboxAdded++;
                    }
                }
            }
            catch { }

            return result;
        }
    }
}

