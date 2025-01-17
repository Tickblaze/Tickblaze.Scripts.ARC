namespace Tickblaze.Scripts.Arc.Common;

public static class ComparisonExtensions
{
	public static readonly double Epsilon = 1e-10;

	public static bool IsInRange<TComparable>(this TComparable value, TComparable minimum, TComparable maximum)
		where TComparable : IComparable<TComparable>
	{
		var comparer = Comparer<TComparable>.Default;

		return comparer.Compare(value, minimum) >= 0 && comparer.Compare(maximum, value) >= 0;
	}

	public static bool EnumEquals<TEnum>(this TEnum firstEnum, TEnum secondEnum)
		where TEnum : Enum
	{
		return EqualityComparer<TEnum>.Default.Equals(firstEnum, secondEnum);
	}

	public static int EnumCompareTo<TEnum>(this TEnum firstEnum, TEnum secondEnum)
		where TEnum : Enum
	{
		return Comparer<TEnum>.Default.Compare(firstEnum, secondEnum);
	}

	public static int EpsilonCompare(this double firstDouble, double secondDouble, double epsilon = 1e-10)
	{
		if (Math.Abs(firstDouble - secondDouble) < epsilon)
		{
			return 0;
		}

		return firstDouble.CompareTo(secondDouble);
	}

	public static bool EpsilonGreaterThan(this double firstDouble, double secondDouble, double epsilon = 1e-10)
	{
		return EpsilonCompare(firstDouble, secondDouble, epsilon) > 0;
	}

	public static bool EpsilonGreaterThanOrEquals(this double firstDouble, double secondDouble, double epsilon = 1e-10)
	{
		return EpsilonCompare(firstDouble, secondDouble, epsilon) >= 0;
	}

	public static bool EpsilonLessThan(this double firstDouble, double secondDouble, double epsilon = 1e-10)
	{
		return EpsilonCompare(firstDouble, secondDouble, epsilon) < 0;
	}

	public static bool EpsilonLessThanOrEquals(this double firstDouble, double secondDouble, double epsilon = 1e-10)
	{
		return EpsilonCompare(firstDouble, secondDouble, epsilon) <= 0;
	}
}
