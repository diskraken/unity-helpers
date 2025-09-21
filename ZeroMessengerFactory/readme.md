# ZeroMessengerFactory

Just another simple factory, created to be used together with **ZeroMessenger** and **Reflex** in Unity in mind.
Providing lightweight helper solution, perfect for quick setup of your next project! :)

---

## Dependencies

| **ZeroMessenger**     | https://github.com/annulusgames/ZeroMessenger |
|-----------------------|-----------------------------------------------|
| **Reflex** (_Optional_) | **https://github.com/gustavopsantos/Reflex**      |

## Getting started

The easiest way would be registering both factory and interface in your existing installer as singleton (code below applies for Reflex):
```csharp
builder.AddSingleton(typeof(ZeroMessengerFactory), typeof(IMessagingFactory));
```

Inject the messaging factory into your classes the usual way:

```csharp
[Inject] private IMessagingFactory _messaging;
```

## Basic usage

### Publish and Subscribe

```csharp
_messaging.Publish(new T());
```

```csharp
var handle = _messaging.Subscribe<T>(t => {
    // ...
});
```
Always remember to unsubscribe when no longer needed:
```
handle?.Dispose();
```

### Accessing Publisher / Subscriber

```csharp
_messaging.GetPublisher<T>().Publish(new T());
_messaging.GetSubscriber<T>().Subscribe<T>(t => {
    // ...
});
```

