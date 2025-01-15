using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Scripts.Arc.Common;

public class EnumMenuItem : MenuItem
{
    public static readonly DependencyProperty SelectedEnumProperty = DependencyProperty.Register(nameof(SelectedEnum), typeof(object), typeof(EnumMenuItem));
	
	public object? SelectedEnum
    {
        get => GetValue(SelectedEnumProperty);
        set => SetValue(SelectedEnumProperty, value);
    }

	public Type? EnumType
	{
		get;
		set
		{
			if (value is { IsEnum: false })
			{
				throw new ArgumentException(string.Empty, nameof(value));
			}

			field = value;

			RegenerateEnums();
		}
	}

	public ObservableCollection<EnumItem> EnumItems { get; } = [];

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
				Value = @enum,
				DisplayName = displayName,
			};

			EnumItems.Add(enumItem);
		}
	}
}
