using System.Collections.ObjectModel;
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

	public ObservableCollection<object> Enums { get; } = [];

	private void RegenerateEnums()
	{
		Items.Clear();

		if (EnumType is null)
		{
			return;
		}

		var enums = Enum.GetValues(EnumType);

		foreach (var @enum in enums)
		{
			Enums.Add(@enum);
		}
	}
}
