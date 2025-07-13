namespace Scabra.Observer.Subscriber
{
    public interface IScabraObserverSubscriberProvider
    {
        IScabraObserverSubscriber GetSubscriber(string name);
    }
}
