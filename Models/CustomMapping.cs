using CommunityToolkit.Mvvm.ComponentModel;
namespace DriftLock.Models
{
    public partial class CustomMapping : ObservableObject
    {
        [ObservableProperty]
        private string _sourceButton;
        [ObservableProperty]
        private string _targetButton;
        [ObservableProperty]
        private string _mappingType = "Button";
        [ObservableProperty]
        private string _turboMode = "None";
        [ObservableProperty]
        private string _macroName = "None";
    }
}
