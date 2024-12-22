namespace Tickblaze.Scripts.Arc;

public partial class HtfAverages
{
    public enum MovingAverageType
    {
        [DisplayName("SMA")]
        Simple,

        [DisplayName("EMA")]
        Exponential,
    }
}
