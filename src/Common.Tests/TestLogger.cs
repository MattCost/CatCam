using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace CatCam.Common.Tests;

public static class UnitTestLogger
{
    public static ILogger<T> Create<T>()
    {
        return new UnitTestLogger<T>();
    }
}

public class UnitTestLogger<T> : ILogger<T>, IDisposable
{
    public void Dispose()
    {
        // Console.WriteLine($"********** EndScope {_state} **********");
        // _state = null;
    }
    // private object? _state;
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        // Console.WriteLine($"********** BeginScope {state} ********** ");
        // _state = state;
        return this;
    }

    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Console.WriteLine(formatter(state, exception));
        // => Console.WriteLine( $"{_state ??  "root"}: {formatter(state, exception)}");
}