//todo: remove this if we are not going to use it

public class AppState : IObservable<string>
{
    private readonly List<IObserver<string>> observers = new List<IObserver<string>>();

    private string dealerJwtToken;
    public string DealerJwtToken
    {
        get => dealerJwtToken;
        set
        {
            dealerJwtToken = value;
            NotifyObservers(dealerJwtToken);
        }
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);
        return new Unsubscriber(observers, observer);
    }

    private void NotifyObservers(string value)
    {
        foreach (var observer in observers)
            observer.OnNext(value);
    }

    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<string>> _observers;
        private readonly IObserver<string> _observer;

        public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
