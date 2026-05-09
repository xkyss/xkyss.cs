namespace MewPad.Core;

/// <summary>
/// A simple observable value container for state management.
/// </summary>
public class ObservableValue<T>
{
    private T _value;
    private readonly Subject<T> _subject = new();

    /// <summary>
    /// Get or set the current value.
    /// Triggers Changed observable on value change.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                _subject.OnNext(value);
            }
        }
    }

    /// <summary>
    /// Observable stream of value changes.
    /// </summary>
    public IObservable<T> Changed => _subject;

    /// <summary>
    /// Create an observable value with an initial value.
    /// </summary>
    public ObservableValue(T initialValue)
    {
        _value = initialValue;
    }
}

/// <summary>
/// Simple Subject implementation for IObservable&lt;T&gt;.
/// </summary>
internal class Subject<T> : IObservable<T>
{
    private readonly List<IObserver<T>> _observers = [];
    private bool _isDisposed;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return new Unsubscriber(_observers, observer);
    }

    public void OnNext(T value)
    {
        if (_isDisposed) return;
        foreach (var observer in _observers.ToList())
        {
            observer.OnNext(value);
        }
    }

    public void OnError(Exception error)
    {
        if (_isDisposed) return;
        foreach (var observer in _observers.ToList())
        {
            observer.OnError(error);
        }
    }

    public void OnCompleted()
    {
        if (_isDisposed) return;
        foreach (var observer in _observers.ToList())
        {
            observer.OnCompleted();
        }
        _isDisposed = true;
    }

    private sealed class Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer) : IDisposable
    {
        public void Dispose()
        {
            observers.Remove(observer);
        }
    }
}
