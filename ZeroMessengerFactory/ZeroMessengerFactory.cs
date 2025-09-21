using System;
using System.Collections.Generic;
using ZeroMessenger;

public interface IMessagingFactory {
    IMessagePublisher<T> GetPublisher<T>();
    IMessageSubscriber<T> GetSubscriber<T>();

    IDisposable Subscribe<T>(Action<T> callback);
    void Publish<T>(T message);
}

public class ZeroMessengerFactory : IMessagingFactory {
    private readonly Dictionary<Type, object> _messageBrokers = new();
    
    public IMessagePublisher<T> GetPublisher<T>() {
        if (!_messageBrokers.ContainsKey(typeof(T))) {
            _messageBrokers[typeof(T)] = new MessageBroker<T>();
        }

        return (MessageBroker<T>)_messageBrokers[typeof(T)];
    }

    public IMessageSubscriber<T> GetSubscriber<T>() {
        return GetPublisher<T>() as IMessageSubscriber<T>;
    }

    public void Publish<T>(T message) {
        GetPublisher<T>().Publish(message);
    }

    public IDisposable Subscribe<T>(Action<T> handler) {
        return GetSubscriber<T>().Subscribe(handler);
    }
}