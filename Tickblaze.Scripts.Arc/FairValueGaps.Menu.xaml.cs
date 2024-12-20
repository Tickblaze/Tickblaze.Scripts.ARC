using System.Windows.Controls;

namespace Tickblaze.Scripts.Arc;

/// <summary>
/// Interaction logic for FairValueGaps.xaml
/// </summary>
public partial class FairValueGapsMenu : Menu
{
    private FairValueGaps _fairValueGaps;

    public FairValueGapsMenu()
    {
        _fairValueGaps = default!;

        InitializeComponent();
    }

    public FairValueGapsMenu(FairValueGaps fairValueGaps)
    {
        _fairValueGaps = fairValueGaps;
    }
}
