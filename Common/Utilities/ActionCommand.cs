using System.Windows.Input;

namespace Tickblaze.Community;

public class ActionCommand<TValue> : ICommand
	where TValue: class
{
    private readonly Action<TValue?> _executeAction;
    
	private readonly Func<TValue?, bool> _predicate;

    public ActionCommand(Action<TValue?> executeAction) : this(executeAction, delegate { return true; })
	{

	}

	public ActionCommand(Action<TValue?> executeAction, Func<TValue?, bool> predicate)
	{
		_predicate = predicate;
		
		_executeAction = executeAction;
	}

	public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _predicate(parameter as TValue);
	}

    public void Execute(object? parameter)
    {
		_executeAction(parameter as TValue);
	}
}

public class ActionCommand : ActionCommand<object>
{
    public ActionCommand(Action executeAction) : base(@object => executeAction())
    {

    }
}