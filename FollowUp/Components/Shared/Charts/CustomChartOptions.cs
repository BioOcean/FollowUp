namespace FollowUp.Components.Shared.Charts;

public sealed class CustomChartOptions
{
    public bool ShowBarValues { get; init; } = false;
    public bool ShowLineValues { get; init; } = true;
    public bool ShowLegend { get; init; } = true;
    public LegendPosition LegendPosition { get; init; } = LegendPosition.Bottom;

    public bool PreserveAspectRatio { get; init; } = true;
    public int Width { get; init; } = 0;
    public int Height { get; init; } = 400;

    public string BarColor { get; init; } = "#3CB371";
    public string LineColor { get; init; } = "#FFA500";

    public double YAxisMaxValue { get; init; } = 0;
    public int YAxisSteps { get; init; } = 5;

    public int LeftPadding { get; init; } = 0;
    public int RightPadding { get; init; } = 0;
    public int TopPadding { get; init; } = 0;
    public int BottomPadding { get; init; } = 0;
}

public enum LegendPosition
{
    Top,
    Bottom,
    Left,
    Right
}

