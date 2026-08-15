using CommunityToolkit.Mvvm.ComponentModel;
namespace DriftLift.Models
{
    public partial class CustomMapping : ObservableObject
    {
        [ObservableProperty]
        private string _sourceButton = string.Empty;
        [ObservableProperty]
        private string _targetButton = string.Empty;
        [ObservableProperty]
        private string _mappingType = "Button";
        [ObservableProperty]
        private string _turboMode = "None";
        [ObservableProperty]
        private string _macroName = "None";
    }
}
