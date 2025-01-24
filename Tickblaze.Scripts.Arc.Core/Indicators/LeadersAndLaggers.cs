using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class LeadersAndLaggers : Indicator
{
	public LeadersAndLaggers()
	{
		ShortName = "LL";

		Name = "Leaders and Laggers";
	}

	private static readonly Regex _symbolRegex = GetSymbolRegex();

	private int _leadingInstrumentIndex;
	private int _laggingInstrumentIndex;

	[AllowNull]
	private string?[] _symbols;

	[AllowNull]
	private double[] _startingValues;

	[AllowNull]
	private BarSeries?[] _barSeriesCollection;

	[AllowNull]
	private PlotSeries[] _plotSeriesCollection;

	[Parameter("Reset Type", Description = "Type of calculation reset")]
	public ResetType ResetTypeValue { get; set; }

	[Parameter("Reset Time", Description = "Reset time for a custom start type")]
	public DateTime StartTime { get; set; }

	[Parameter("Show Box", GroupName = "Info Box", Description = "Whether info box is shown")]
	public bool ShowBox { get; set; } = true;

	[Parameter("Box Color", GroupName = "Info Box", Description = "Color of the info box background")]
	public Color BoxColor { get; set; } = DrawingColor.Bisque;

	[Parameter("Text Color", GroupName = "Info Box", Description = "Color of the info box text")]
	public Color TextColor { get; set; } = Color.Black;

	[Parameter("Outline Color", GroupName = "Info Box", Description = "Color of the info box outline")]
	public Color OutlineColor { get; set; } = Color.Black;

	[Parameter("Font", GroupName = "Info Box", Description = "Font of the info box text")]
	public Font TextFont { get; set; } = new("Arial", 12);

	[Plot("Plot1")]
	public PlotSeries Plot1 { get; set; } = new(Color.Red, LineStyle.Solid, 3);
	
	[Parameter("Instrument 2", GroupName = "Instruments", Description = "Symbol representing Instrument 2. Futures contract must be in MM-YY format")]
	public string? Symbol2 { get; set; }

	[Plot("Plot2")]
	public PlotSeries Plot2 { get; set; } = new(Color.Blue, LineStyle.Solid, 3);

	[Parameter("Instrument 3", GroupName = "Instruments", Description = "Symbol representing Instrument 3. Futures contract must be in MM-YY format")]
	public string? Symbol3 { get; set; }

	[Plot("Plot3")]
	public PlotSeries Plot3 { get; set; } = new(Color.Yellow, LineStyle.Solid, 3);

	[Parameter("Instrument 4", GroupName = "Instruments", Description = "Symbol representing Instrument 4. Futures contract must be in MM-YY format")]
	public string? Symbol4 { get; set; }

	[Plot("Plot4")]
	public PlotSeries Plot4 { get; set; } = new(Color.Cyan, LineStyle.Solid, 3);

	[Parameter("Instrument 5", GroupName = "Instruments", Description = "Symbol representing Instrument 5. Futures contract must be in MM-YY format")]
	public string? Symbol5 { get; set; }

	[Plot("Plot5")]
	public PlotSeries Plot5 { get; set; } = new(Color.Orange, LineStyle.Solid, 3);

	[Parameter("Instrument 6", GroupName = "Instruments", Description = "Symbol representing Instrument 6. Futures contract must be in MM-YY format")]
	public string? Symbol6 { get; set; }

	[Plot("Plot6")]
	public PlotSeries Plot6 { get; set; } = new(Color.Black, LineStyle.Solid, 3);

	[Parameter("Instrument 7", GroupName = "Instruments", Description = "Symbol representing Instrument 7. Futures contract must be in MM-YY format")]
	public string? Symbol7 { get; set; }

	[Plot("Plot7")]
	public PlotSeries Plot7 { get; set; } = new(DrawingColor.Magenta, LineStyle.Solid, 3);

	[Parameter("Instrument 8", GroupName = "Instruments", Description = "Symbol representing Instrument 8. Futures contract must be in MM-YY format")]
	public string? Symbol8 { get; set; }

	[Plot("Plot8")]
	public PlotSeries Plot8 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 9", GroupName = "Instruments", Description = "Symbol representing Instrument 9. Futures contract must be in MM-YY format")]
	public string? Symbol9 { get; set; }

	[Plot("Plot9")]
	public PlotSeries Plot9 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 10", GroupName = "Instruments", Description = "Symbol representing Instrument 10. Futures contract must be in MM-YY format")]
	public string? Symbol10 { get; set; }

	[Plot("Plot10")]
	public PlotSeries Plot10 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 11", GroupName = "Instruments", Description = "Symbol representing Instrument 11. Futures contract must be in MM-YY format")]
	public string? Symbol11 { get; set; }

	[Plot("Plot11")]
	public PlotSeries Plot11 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 12", GroupName = "Instruments", Description = "Symbol representing Instrument 12. Futures contract must be in MM-YY format")]
	public string? Symbol12 { get; set; }

	[Plot("Plot12")]
	public PlotSeries Plot12 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 13", GroupName = "Instruments", Description = "Symbol representing Instrument 13. Futures contract must be in MM-YY format")]
	public string? Symbol13 { get; set; }

	[Plot("Plot13")]
	public PlotSeries Plot13 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 14", GroupName = "Instruments", Description = "Symbol representing Instrument 14. Futures contract must be in MM-YY format")]
	public string? Symbol14 { get; set; }

	[Plot("Plot14")]
	public PlotSeries Plot14 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 15", GroupName = "Instruments", Description = "Symbol representing Instrument 15. Futures contract must be in MM-YY format")]
	public string? Symbol15 { get; set; }

	[Plot("Plot15")]
	public PlotSeries Plot15 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	[Parameter("Instrument 16", GroupName = "Instruments", Description = "Symbol representing Instrument 16. Futures contract must be in MM-YY format")]
	public string? Symbol16 { get; set; }

	[Plot("Plot16")]
	public PlotSeries Plot16 { get; set; } = new(Color.Transparent, LineStyle.Solid, 3);

	private bool TryGetBarSeriesRequest(string? symbol, [NotNullWhen(true)] out BarSeriesRequest? barSeriesRequest)
	{
		if (symbol is null || _symbolRegex.Match(symbol) is var match && !match.Success)
		{
			barSeriesRequest = default;

			return false;
		}

		const int symbolCodeIndex = 1;
		const int contractMonthIndex = 2;
		const int contractYearIndex = 3;

		var matchGroups = match.Groups;
		var symbolCode = matchGroups[symbolCodeIndex].Value;
		var contractYearString = matchGroups[contractYearIndex].Value;
		var contractMonthString = matchGroups[contractMonthIndex].Value;

		barSeriesRequest = new BarSeriesRequest
		{
			Period = Bars.Period,
			SymbolCode = symbolCode,
		};

		if (int.TryParse(contractMonthString, out var contractMonth)
			&& int.TryParse(contractYearString, out var contractYear))
		{
			barSeriesRequest.Contract = new ContractSettings
			{
				Year = 2_000 + contractYear,
				Month = contractMonth,
				Type = ContractType.SingleContract,
			};
		}

		return true;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (ResetTypeValue is not ResetType.Custom)
		{
			parameters.Remove(nameof(StartTime));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		_symbols =
		[
			Symbol.Code,
			Symbol2,
			Symbol3,
			Symbol4,
			Symbol5,
			Symbol6,
			Symbol7,
			Symbol8,
			Symbol9,
			Symbol10,
			Symbol11,
			Symbol12,
			Symbol13,
			Symbol14,
			Symbol15,
			Symbol16,
		];

		_plotSeriesCollection =
		[
			Plot1,
			Plot2,
			Plot3,
			Plot4,
			Plot5,
			Plot6,
			Plot7,
			Plot8,
			Plot9,
			Plot10,
			Plot11,
			Plot12,
			Plot13,
			Plot14,
			Plot15,
			Plot16,
		];

		_barSeriesCollection = new BarSeries[_plotSeriesCollection.Length];
		_startingValues = [.. Enumerable.Repeat(double.NaN, _plotSeriesCollection.Length)];

		var barSeriesIndex = 0;

		_barSeriesCollection[barSeriesIndex] = Bars;
		
		while (++barSeriesIndex < _plotSeriesCollection.Length)
		{
			var symbol = _symbols[barSeriesIndex];
			
			if (!TryGetBarSeriesRequest(symbol, out var barSeriesRequest))
			{
				continue;
			}

			_barSeriesCollection[barSeriesIndex] = GetBars(barSeriesRequest);
		}
	}

	protected override void Calculate(int index)
	{
		CalculateStartingValues(index);

		var leadingInstrumentValue = double.MinValue;
		var laggingInstrumentValue = double.MaxValue;

		for (var barSeriesIndex = 0; barSeriesIndex < _barSeriesCollection.Length; barSeriesIndex++)
		{
			var startingValue = _startingValues[barSeriesIndex];
			var barSeries = _barSeriesCollection[barSeriesIndex];
			var plotSeries = _plotSeriesCollection[barSeriesIndex];

			if (barSeries is null)
			{
				continue;
			}

			var currentClose = barSeries.Close.GetLastOrDefault(double.NaN);
			var currentValue = 100.0 * (currentClose - startingValue) / startingValue;
			var currentValueReal = double.IsNaN(currentValue) ? default : currentValue;

			plotSeries[index] = currentValueReal;

			if (currentValueReal >= leadingInstrumentValue)
			{
				leadingInstrumentValue = currentValueReal;

				_leadingInstrumentIndex = barSeriesIndex;
			}

			if (currentValueReal <= laggingInstrumentValue)
			{
				laggingInstrumentValue = currentValueReal;

				_laggingInstrumentIndex = barSeriesIndex;
			}
		}
	}

	private void CalculateStartingValues(int barIndex)
    {
		var isResetNeeded = ResetTypeValue is ResetType.ChartStart && barIndex is 1
			|| ResetTypeValue is ResetType.Session && this.IsNewSession(barIndex)
			|| ResetTypeValue is ResetType.Custom && this.IsSteppedThrough(barIndex, StartTime);
		
		if (isResetNeeded)
		{
			for (int barSeriesIndex = 0; barSeriesIndex < _barSeriesCollection.Length; barSeriesIndex++)
			{
				_startingValues[barSeriesIndex] = double.NaN;
			}
		}

		var currentTimeUtc = Bars.Time.Last();
		var session = Symbol.ExchangeCalendar.GetSession(currentTimeUtc)!;
		var sessionStartTimeUtc = session.StartUtcDateTime;

        for (int barSeriesIndex = 0; barSeriesIndex < _barSeriesCollection.Length; barSeriesIndex++)
        {
			var startingValue = _startingValues[barSeriesIndex];
            var barSeries = _barSeriesCollection[barSeriesIndex];

			if (barSeries is not null
				&& double.IsNaN(startingValue)
				&& barSeries.Time.GetLastOrDefault(DateTime.MinValue) is var barTimeUtc
				&& barTimeUtc >= sessionStartTimeUtc)
            {
				_startingValues[barSeriesIndex] = barSeries.Open.GetLastOrDefault(double.NaN);
            }
        }
    }

    public override void OnRender(IDrawingContext context)
	{
		if (!ShowBox)
		{
			return;
		}

		var verticalMargin = 1.5 * VerticalMargin;
		var horizontalMargin = 2.5 * HorizontalMargin;

		var leadingSymbol = _symbols[_leadingInstrumentIndex]!;
		var laggingSymbol = _symbols[_laggingInstrumentIndex]!;

		var leadingText = "Leading: " + leadingSymbol;
		var laggingText = "Lagging: " + laggingSymbol; 

		var leadingSymbolTextSize = context.MeasureText(leadingText, TextFont);
		var laggingSymbolTextSize = context.MeasureText(laggingText, TextFont);

		var boxWidth = 2 * horizontalMargin + Math.Max(laggingSymbolTextSize.Width, leadingSymbolTextSize.Width);
		var boxHeight = 3 * verticalMargin + leadingSymbolTextSize.Height + laggingSymbolTextSize.Height;

		var topLeftPoint = this.GetTopLeft();
		var leadingSymbolTextPoint = new ApiPoint
		{
			X = topLeftPoint.X + horizontalMargin,
			Y = topLeftPoint.Y + verticalMargin,
		};
		var laggingSymbolTextPoint = new ApiPoint
		{
			X = leadingSymbolTextPoint.X,
			Y = leadingSymbolTextPoint.Y + leadingSymbolTextSize.Height + verticalMargin
		};

		context.DrawRectangle(topLeftPoint, boxWidth, boxHeight, BoxColor, OutlineColor);

		context.DrawText(leadingSymbolTextPoint, leadingText, TextColor, TextFont);
		context.DrawText(laggingSymbolTextPoint, laggingText, TextColor, TextFont);
	}

	[GeneratedRegex(@"([^ ]+)(?: (\d{2})-(\d{2}))?")]
	private static partial Regex GetSymbolRegex();

	public enum ResetType
	{
		[DisplayName("Session")]
		Session,

		[DisplayName("Chart Start")]
		ChartStart,

		[DisplayName("Date")]
		Custom,
	}
}
