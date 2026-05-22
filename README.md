# SubEvent System

A lightweight, thread-safe pub/sub event system with **conditional activation** for .NET.

## What is it?

A custom event aggregator where subscribers can be **activated/deactivated at runtime** without unsubscribing. Useful when you need fine-grained control over who receives events and when.

## Key Features

- **Activatable tokens** — toggle `IsActive` to start/stop receiving events
- **Conditional activation** — provide a `Func<bool>` to auto-evaluate activation
- **Active listener tracking** — `HasActiveListeners` notifies when first/last listener toggles
- **Thread-safe** — lock + snapshot pattern for safe concurrent invocation
- **Zero dependencies** — pure .NET, no external packages

## Quick Start

```csharp
var sensorEvent = new SubEvent<double>(
    OnFirstListenerActivation: () => Console.WriteLine("Streaming started"),
    OnLastListenerDeactivation: () => Console.WriteLine("Streaming stopped"));

var token = sensorEvent.Subscribe(value =>
{
    Console.WriteLine($"Received: {value}");
}, initialState: true);

sensorEvent.Invoke(42.0); // prints "Received: 42"

token.IsActive = false;
sensorEvent.Invoke(99.0); // nothing happens
