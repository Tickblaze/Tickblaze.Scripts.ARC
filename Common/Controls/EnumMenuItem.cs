using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Community;

public class EnumMenuItem : MenuItem
{
    public static readonly DependencyProperty SelectedEnumProperty = DependencyProperty.Register(nameof(SelectedEnum), typeof(string), typeof(EnumMenuItem));
	public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumMenuItem), new(OnEnumTypeChanged));

	public string? SelectedEnum
    {
        get => (string) GetValue(SelectedEnumProperty);
        set => SetValue(SelectedEnumProperty, value);
    }

	public Type? EnumType
	{
		get => (Type?) GetValue(EnumTypeProperty);
		set => SetValue(EnumTypeProperty, value);
	}

	public ObservableCollection<EnumItem> EnumItems { get; } = [];

	private static void OnEnumTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
	{
		if (dependencyObject is not EnumMenuItem enumMenuItem
			|| args.NewValue is not Type { IsEnum: true })
		{
			throw new ArgumentException(string.Empty, nameof(args.NewValue));
		}

		enumMenuItem.RegenerateEnums();
	}

	private void RegenerateEnums()
	{
		EnumItems.Clear();

		if (EnumType is null)
		{
			return;
		}

		var enums = Enum.GetValues(EnumType);

		foreach (var @enum in enums)
		{
			var enumString = @enum.ToStringOrEmpty();
			var fieldInfo = EnumType.GetField(enumString);
			var displayNameAttribute = fieldInfo?.GetCustomAttribute<DisplayNameAttribute>();
			var displayName = displayNameAttribute?.DisplayName ?? enumString;
			var enumItem = new EnumItem
			{
				Name = enumString,
				DisplayName = displayName,
			};

			EnumItems.Add(enumItem);
		}
	}
}
