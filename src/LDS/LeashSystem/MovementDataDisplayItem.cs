using CommunityToolkit.Mvvm.ComponentModel;

namespace LDS.LeashSystem;

public partial class MovementDataViewModel : ObservableObject {
    [ObservableProperty] public partial float VerticalOffset { get; set; } = 0f;
    [ObservableProperty] public partial float HorizontalOffset { get; set; } = 0f;
    [ObservableProperty] public partial float HorizontalLook { get; set; } = 0f;
    [ObservableProperty] public partial bool ShouldRun { get; set; } = false;

    public void RenewData(MovementData data) {
        if (VerticalOffset != data.VerticalOffset) { VerticalOffset = data.VerticalOffset; }
        if (HorizontalOffset != data.HorizontalOffset) { HorizontalOffset = data.HorizontalOffset; }
        if (HorizontalLook != data.HorizontalLook) { HorizontalLook = data.HorizontalLook; }
        if(ShouldRun !=  data.ShouldRun) { ShouldRun = data.ShouldRun; }
    }
}
