namespace MewPad.Core;

/// <summary>
/// Extension methods for IObservable&lt;T&gt; to support lambda-based subscriptions
/// without requiring System.Reactive.
/// </summary>
public static class ObservableExtensions
{
    /// <summary>Subscribe with an onNext action.</summary>
    public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
        => observable.Subscribe(new ActionObserver<T>(onNext));
}

/// <summary>
/// Minimal IObserver&lt;T&gt; adapter for a plain Action delegate.
/// </summary>
internal sealed class ActionObserver<T>(Action<T> onNext) : IObserver<T>
{
    public void OnNext(T value) => onNext(value);
    public void OnError(Exception error) { }
    public void OnCompleted() { }
}
