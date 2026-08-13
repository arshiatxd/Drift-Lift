using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DriftLock.Models
{
    // ##== Macro Step Model ==##
    public partial class MacroStep : ObservableObject
    {
        [ObservableProperty] private string _buttonName = "Cross";
        [ObservableProperty] private ushort _buttonMask = 0x1000;
        [ObservableProperty] private int _delayMs = 50;
    }

    // ##== Macro Item Model ==##
    public partial class MacroItem : ObservableObject
    {
        [ObservableProperty] private string _name = "Rapid Fire Combo";
        [ObservableProperty] private string _triggerButtonName = "R2 + L1";
        [ObservableProperty] private ushort _triggerMask = 0x0300;
        [ObservableProperty] private bool _isEnabled = true;

        public ObservableCollection<MacroStep> Steps { get; set; } = new();
    }
}
