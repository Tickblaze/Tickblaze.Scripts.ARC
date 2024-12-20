namespace Tickblaze.Scripts.Arc;

public partial class BarCloseMarker
{
    public enum MarkerType
    {
        [DisplayName("None")]
        None,
        
        [DisplayName("Price")]
        Price,

        [DisplayName("Extended Lines")]
        ExtendedLines,
    }
}
