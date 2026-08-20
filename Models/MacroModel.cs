using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DriftLift.Models
{
    public partial class MacroStep : ObservableObject
    {
        [ObservableProperty] private string _buttonName = "Cross";
        [ObservableProperty] private uint _buttonMask = 0x1000;
        [ObservableProperty] private int _delayMs = 50;

        partial void OnButtonNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            string norm = value.Trim().ToUpperInvariant();
            uint bit = norm switch
            {
                "CROSS" or "A" => 0x1000u,
                "CIRCLE" or "B" => 0x2000u,
                "SQUARE" or "X" => 0x4000u,
                "TRIANGLE" or "Y" => 0x8000u,
                "L1" or "LB" => 0x0100u,
                "R1" or "RB" => 0x0200u,
                "L2" or "LT" => 0x0400u,
                "R2" or "RT" => 0x0800u,
                "SHARE" or "BACK" or "SELECT" => 0x0020u,
                "OPTIONS" or "START" => 0x0010u,
                "L3" or "LEFTTHUMB" => 0x0040u,
                "R3" or "RIGHTTHUMB" => 0x0080u,
                "DPADUP" or "UP" => 0x0001u,
                "DPADDOWN" or "DOWN" => 0x0002u,
                "DPADLEFT" or "LEFT" => 0x0004u,
                "DPADRIGHT" or "RIGHT" => 0x0008u,
                _ => 0u
            };
            if (bit != 0) ButtonMask = bit;
        }
    }

    public partial class MacroItem : ObservableObject
    {
        [ObservableProperty] private string _name = "Rapid Fire Combo";
        [ObservableProperty] private string _triggerButtonName = "L1 + R1";
        [ObservableProperty] private uint _triggerMask = 0x0300;
        [ObservableProperty] private bool _isEnabled = true;

        public ObservableCollection<MacroStep> Steps { get; set; } = new();

        partial void OnTriggerButtonNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                TriggerMask = 0;
                return;
            }

            uint mask = 0;
            string[] parts = value.Split(new[] { '+', ',', '&', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                string norm = part.Trim().ToUpperInvariant();
                uint bit = norm switch
                {
                    "CROSS" or "A" => 0x1000u,
                    "CIRCLE" or "B" => 0x2000u,
                    "SQUARE" or "X" => 0x4000u,
                    "TRIANGLE" or "Y" => 0x8000u,
                    "L1" or "LB" => 0x0100u,
                    "R1" or "RB" => 0x0200u,
                    "L2" or "LT" => 0x0400u,
                    "R2" or "RT" => 0x0800u,
                    "SHARE" or "BACK" or "SELECT" => 0x0020u,
                    "OPTIONS" or "START" => 0x0010u,
                    "L3" or "LEFTTHUMB" => 0x0040u,
                    "R3" or "RIGHTTHUMB" => 0x0080u,
                    "DPADUP" or "UP" => 0x0001u,
                    "DPADDOWN" or "DOWN" => 0x0002u,
                    "DPADLEFT" or "LEFT" => 0x0004u,
                    "DPADRIGHT" or "RIGHT" => 0x0008u,
                    _ => 0u
                };
                mask |= bit;
            }
            if (mask != 0)
            {
                TriggerMask = mask;
            }
        }
    }
}
