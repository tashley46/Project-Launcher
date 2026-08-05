namespace ProjectLauncher.Core;

public interface ICommand;

public interface ICommand<out TResult> : ICommand;

