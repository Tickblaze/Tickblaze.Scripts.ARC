namespace Tickblaze.Community;

public partial class HtfAverages
{
	public class HtfPlotSeries : PlotSeries
	{
        public HtfPlotSeries(string name, Color color, HtfAverages htfAverages) : base(name, color)
		{
			_htfAverages = htfAverages;
        }

        private readonly HtfAverages _htfAverages;

		public Interval LastLevelInterval => _htfAverages._htfInterval;

		public double LastValue
		{
			get;
			set
			{
				if (field.Equals(value))
				{
					return;
				}

				field = value;

				FurthestUpdateIndex = LastLevelInterval.StartBarIndex;
			}
		} = double.NaN;

		public override double this[int index]
		{
			get
			{
				ArgumentOutOfRangeException.ThrowIfNegative(index);

				return index >= LastLevelInterval.StartBarIndex ? LastValue : base[index];
			}
			set => base[index] = value;
		}
	}
}