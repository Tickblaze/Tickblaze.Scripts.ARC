using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Community;

public partial class LeadersAndLaggers : Indicator
{
	public LeadersAndLaggers()
	{
		ShortName = "LL";

		Name = "Leaders and Laggers";
	}

	private int _leadingInstrumentIndex;
	private int _laggingInstrumentIndex;

	[AllowNull]
	private SymbolInfo?[] _symbolInfos;

	[AllowNull]
	private double[] _startingValues;

	[AllowNull]
	private BarSeries?[] _barsCollection;

	[AllowNull]
	private PlotSeries[] _plotsCollection;

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

	[Parameter("Instrument 2", GroupName = "Instruments", Description = "Symbol representing Instrument 2")]
	public SymbolInfo? Symbol2 { get; set; }

	[Parameter("Instrument 3", GroupName = "Instruments", Description = "Symbol representing Instrument 3")]
	public SymbolInfo? Symbol3 { get; set; }

	[Parameter("Instrument 4", GroupName = "Instruments", Description = "Symbol representing Instrument 4")]
	public SymbolInfo? Symbol4 { get; set; }

	[Parameter("Instrument 5", GroupName = "Instruments", Description = "Symbol representing Instrument 5")]
	public SymbolInfo? Symbol5 { get; set; }

	[Parameter("Instrument 6", GroupName = "Instruments", Description = "Symbol representing Instrument 6")]
	public SymbolInfo? Symbol6 { get; set; }

	[Parameter("Instrument 7", GroupName = "Instruments", Description = "Symbol representing Instrument 7")]
	public SymbolInfo? Symbol7 { get; set; }

	[Parameter("Instrument 8", GroupName = "Instruments", Description = "Symbol representing Instrument 8")]
	public SymbolInfo? Symbol8 { get; set; }

	[Parameter("Instrument 9", GroupName = "Instruments", Description = "Symbol representing Instrument 9")]
	public SymbolInfo? Symbol9 { get; set; }

	[Parameter("Instrument 10", GroupName = "Instruments", Description = "Symbol representing Instrument 10")]
	public SymbolInfo? Symbol10 { get; set; }

	[Parameter("Instrument 11", GroupName = "Instruments", Description = "Symbol representing Instrument 11")]
	public SymbolInfo? Symbol11 { get; set; }

	[Parameter("Instrument 12", GroupName = "Instruments", Description = "Symbol representing Instrument 12")]
	public SymbolInfo? Symbol12 { get; set; }

	[Parameter("Instrument 13", GroupName = "Instruments", Description = "Symbol representing Instrument 13")]
	public SymbolInfo? Symbol13 { get; set; }

	[Parameter("Instrument 14", GroupName = "Instruments", Description = "Symbol representing Instrument 14")]
	public SymbolInfo? Symbol14 { get; set; }

	[Parameter("Instrument 15", GroupName = "Instruments", Description = "Symbol representing Instrument 15")]
	public SymbolInfo? Symbol15 { get; set; }

	[Parameter("Instrument 16", GroupName = "Instruments", Description = "Symbol representing Instrument 16")]
	public SymbolInfo? Symbol16 { get; set; }

	public PlotLevel ZeroLine { get; set; } = new("Zero Line", 0.0, Color.Transparent);

	public PlotSeries Plot1 { get; set; } = new("Instrument 1", Color.Red, thickness: 3);

	public PlotSeries Plot2 { get; set; } = new("Instrument 2", Color.Blue, thickness: 3);

	public PlotSeries Plot3 { get; set; } = new("Instrument 3", Color.Yellow, thickness: 3);

	public PlotSeries Plot4 { get; set; } = new("Instrument 4", Color.Cyan, thickness: 3);

	public PlotSeries Plot5 { get; set; } = new("Instrument 5", Color.Orange, thickness: 3);

	public PlotSeries Plot6 { get; set; } = new("Instrument 6", Color.Black, thickness: 3);

	public PlotSeries Plot7 { get; set; } = new("Instrument 7", DrawingColor.Magenta, thickness: 3);

	public PlotSeries Plot8 { get; set; } = new("Instrument 8", Color.Transparent, thickness: 3);

	public PlotSeries Plot9 { get; set; } = new("Instrument 9", Color.Transparent, thickness: 3);

	public PlotSeries Plot10 { get; set; } = new("Instrument 10", Color.Transparent, thickness: 3);

	public PlotSeries Plot11 { get; set; } = new("Instrument 11", Color.Transparent, thickness: 3);

	public PlotSeries Plot12 { get; set; } = new("Instrument 12", Color.Transparent, thickness: 3);

	public PlotSeries Plot13 { get; set; } = new("Instrument 13", Color.Transparent, thickness: 3);

	public PlotSeries Plot14 { get; set; } = new("Instrument 14", Color.Transparent, thickness: 3);

	public PlotSeries Plot15 { get; set; } = new("Instrument 15", Color.Transparent, thickness: 3);

	public PlotSeries Plot16 { get; set; } = new("Instrument 16", Color.Transparent, thickness: 3);

	private static string GetFormattedSymbolCode(SymbolInfo symbolInfo)
	{
		var symbolCode = symbolInfo.SymbolCode;

		if (symbolInfo.InstrumentType is InstrumentType.Future)
		{
			var contract = symbolInfo.Contract;

			symbolCode += $" {contract.Month:00}-{contract.Year % 100:00}";
		}

		return symbolCode;
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
		var currentSymblInfo = new SymbolInfo
		{
			SymbolCode = Symbol.Code,
			Exchange = Symbol.Exchange,
			InstrumentType = Symbol.Type,
			Contract = Bars.ContractSettings,
		};

		_symbolInfos =
		[
			currentSymblInfo,
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

		InitializePlotsCollection();

		InitializeBarsCollection();
	}

	private void InitializePlotsCollection()
	{
		_plotsCollection =
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

		_startingValues = [.. Enumerable.Repeat(double.NaN, _plotsCollection.Length)];

		for (var plotIndex = 0; plotIndex < _plotsCollection.Length; plotIndex++)
		{
			var plot = _plotsCollection[plotIndex];
			var symbolInfo = _symbolInfos[plotIndex];

			if (string.IsNullOrEmpty(symbolInfo?.SymbolCode))
			{
				continue;
			}

			plot.PriceMarker.Formatter = barIndex => GetFormattedSymbolCode(symbolInfo);
		}
	}

	private void InitializeBarsCollection()
	{
		var barSeriesIndex = 0;

		_barsCollection = new BarSeries[_plotsCollection.Length];

		_barsCollection[barSeriesIndex] = Bars;

		while (++barSeriesIndex < _plotsCollection.Length)
		{
			var symbolInfo = _symbolInfos[barSeriesIndex];

			if (string.IsNullOrEmpty(symbolInfo?.SymbolCode))
			{
				continue;
			}

			var barSeriesInfo = new BarSeriesInfo
			{
				Period = Bars.Period,
				SymbolInfo = symbolInfo
			};

			_barsCollection[barSeriesIndex] = GetBars(barSeriesInfo);
		}
	}

	protected override void Calculate(int index)
	{
		CalculateStartingValues(index);

		var leadingInstrumentValue = double.MinValue;
		var laggingInstrumentValue = double.MaxValue;

		for (var barSeriesIndex = 0; barSeriesIndex < _barsCollection.Length; barSeriesIndex++)
		{
			var startingValue = _startingValues[barSeriesIndex];
			var barSeries = _barsCollection[barSeriesIndex];
			var plotSeries = _plotsCollection[barSeriesIndex];

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
		var isResetNeeded = ResetTypeValue is ResetType.ChartStart && barIndex is 0
			|| ResetTypeValue is ResetType.Session && this.IsNewSession(barIndex)
			|| ResetTypeValue is ResetType.Custom && this.IsSteppedThrough(barIndex, StartTime);

		if (isResetNeeded)
		{
			for (int barSeriesIndex = 0; barSeriesIndex < _barsCollection.Length; barSeriesIndex++)
			{
				_startingValues[barSeriesIndex] = double.NaN;
			}
		}

		var currentTimeUtc = Bars.Time.Last();
		var session = Symbol.ExchangeCalendar.GetSession(currentTimeUtc)!;
		var sessionStartTimeUtc = session.StartUtcDateTime;

		for (int barSeriesIndex = 0; barSeriesIndex < _barsCollection.Length; barSeriesIndex++)
		{
			var startingValue = _startingValues[barSeriesIndex];
			var barSeries = _barsCollection[barSeriesIndex];

			var isBarTimeInSession = Bars.Period.Source is SourceBarType.Day
				|| barSeries is not null
				&& barSeries.Time.GetLastOrDefault(DateTime.MinValue) >= sessionStartTimeUtc;

			if (barSeries is not null
				&& isBarTimeInSession
				&& double.IsNaN(startingValue))
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

		var leadingSymbol = _symbolInfos[_leadingInstrumentIndex]!;
		var laggingSymbol = _symbolInfos[_laggingInstrumentIndex]!;

		var leadingText = "Leading: " + GetFormattedSymbolCode(leadingSymbol);
		var laggingText = "Lagging: " + GetFormattedSymbolCode(laggingSymbol);

		var leadingSymbolTextSize = context.MeasureText(leadingText, TextFont);
		var laggingSymbolTextSize = context.MeasureText(laggingText, TextFont);

		var boxWidth = 2 * horizontalMargin + Math.Max(laggingSymbolTextSize.Width, leadingSymbolTextSize.Width);
		var boxHeight = 3 * verticalMargin + leadingSymbolTextSize.Height + laggingSymbolTextSize.Height;

		var topLeftPoint = this.GetTopLeft();

		topLeftPoint.X += horizontalMargin;
		topLeftPoint.Y += verticalMargin;

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
