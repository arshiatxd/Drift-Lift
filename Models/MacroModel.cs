using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DriftLift.Models
{
    public partial class MacroStep : ObservableObject
    {
        [ObservableProperty] private string _buttonName = "Cross";
        [ObservableProperty] private ushort _buttonMask = 0x1000;
        [ObservableProperty] private int _delayMs = 50;

        partial void OnButtonNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            string norm = value.Trim().ToUpperInvariant();
            ushort bit = norm switch
            {
                "CROSS" or "A" => 0x1000,
                "CIRCLE" or "B" => 0x2000,
                "SQUARE" or "X" => 0x4000,
                "TRIANGLE" or "Y" => 0x8000,
                "L1" or "LB" => 0x0100,
                "R1" or "RB" => 0x0200,
                "L2" or "LT" => 0x0400,
                "R2" or "RT" => 0x0800,
                "SHARE" or "BACK" or "SELECT" => 0x0020,
                "OPTIONS" or "START" => 0x0010,
                "L3" or "LEFTTHUMB" => 0x0040,
                "R3" or "RIGHTTHUMB" => 0x0080,
                "DPADUP" or "UP" => 0x0001,
                "DPADDOWN" or "DOWN" => 0x0002,
                "DPADLEFT" or "LEFT" => 0x0004,
                "DPADRIGHT" or "RIGHT" => 0x0008,
                _ => 0
            };
            if (bit != 0) ButtonMask = bit;
        }
    }

    public partial class MacroItem : ObservableObject
    {
        [ObservableProperty] private string _name = "Rapid Fire Combo";
        [ObservableProperty] private string _triggerButtonName = "L1 + R1";
        [ObservableProperty] private ushort _triggerMask = 0x0300;
        [ObservableProperty] private bool _isEnabled = true;

        public ObservableCollection<MacroStep> Steps { get; set; } = new();

        partial void OnTriggerButtonNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                TriggerMask = 0;
                return;
            }

            ushort mask = 0;
            string[] parts = value.Split(new[] { '+', ',', '&', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                string norm = part.Trim().ToUpperInvariant();
                mask |= norm switch
                {
                    "CROSS" or "A" => 0x1000,
                    "CIRCLE" or "B" => 0x2000,
                    "SQUARE" or "X" => 0x4000,
                    "TRIANGLE" or "Y" => 0x8000,
                    "L1" or "LB" => 0x0100,
                    "R1" or "RB" => 0x0200,
                    "L2" or "LT" => 0x0400,
                    "R2" or "RT" => 0x0800,
                    "SHARE" or "BACK" or "SELECT" => 0x0020,
                    "OPTIONS" or "START" => 0x0010,
                    "L3" or "LEFTTHUMB" => 0x0040,
                    "R3" or "RIGHTTHUMB" => 0x0080,
                    "DPADUP" or "UP" => 0x0001,
                    "DPADDOWN" or "DOWN" => 0x0002,
                    "DPADLEFT" or "LEFT" => 0x0004,
                    "DPADRIGHT" or "RIGHT" => 0x0008,
                    _ => 0
                };
            }
            if (mask != 0)
            {
                TriggerMask = mask;
            }
        }
    }
}
