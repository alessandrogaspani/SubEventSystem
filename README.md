# SubEvent System

A lightweight, thread-safe pub/sub event system with **conditional activation** for .NET.

Subscribers can be **paused and resumed at runtime** without unsubscribing. Producers can **auto-start/stop** based on whether anyone is listening.

## How It Works

```
var token = event.Subscribe(handler)    -> Token created (inactive by default)
token.IsActive = true                   -> Starts receiving events
token.IsActive = false                  -> Stops receiving (still subscribed)
event.Unsubscribe(token)                -> Token removed and disposed
```

## Key Features

| Feature | What it does |
|---------|-------------|
| **Activatable tokens** | Toggle `IsActive` to pause/resume without resubscribing |
| **Conditional activation** | Provide a `Func<bool>` evaluated at each `Invoke` |
| **Listener tracking** | `HasActiveListeners` fires callbacks on first/last toggle |
| **Thread-safe** | Lock + snapshot pattern for concurrent invocation |
| **Zero dependencies** | Pure .NET |

## Quick Example

```csharp
// Producer starts/stops based on listeners
var sensorEvent = new SubEvent<double>(
    OnFirstListenerActivation: () => sensor.StartStreaming(),
    OnLastListenerDeactivation: () => sensor.StopStreaming());

// Consumer subscribes
var token = sensorEvent.Subscribe(value => Console.WriteLine(value), initialState: true);

sensorEvent.Invoke(42.0);  // prints 42
token.IsActive = false;
sensorEvent.Invoke(99.0);  // nothing - token paused
token.IsActive = true;
sensorEvent.Invoke(7.0);   // prints 7
```

## API

### SubEvent<T>

| Method | Description |
|--------|-------------|
| `Subscribe(handler)` | Subscribe inactive |
| `Subscribe(handler, initialState)` | Subscribe with explicit state |
| `Subscribe(handler, activateCondition)` | Subscribe with auto-eval condition |
| `Unsubscribe(token)` | Remove and dispose |
| `Invoke(value)` | Dispatch to active subscribers |
| `Dispose()` | Clean up all |

### SubEventToken<T>

| Member | Description |
|--------|-------------|
| `IsActive` | Toggle activation |
| `RefreshActivation()` | Re-evaluate `ShouldActivate` |
| `Dispose()` | Null references for GC |

## Demo App (WPF)

A visual playground showing SubEvent in action:

- **Producer** generates Temperature/Pressure data every second
- **Widgets** subscribe via `SubEvent<SensorReading>`
- **Checkboxes** toggle `token.IsActive` in real-time
- **Event log** traces delivery
- **LED indicators** reflect `HasActiveListeners`

```bash
dotnet run --project SubEventSystem
```

## Tests

```bash
dotnet test
```

## Project Structure

```
SubEventSystem/
|-- Events/
|   |-- SubEvent.cs
|   |-- SubEventToken.cs
|-- MainWindow.xaml
|-- MainWindow.xaml.cs
|-- SensorReading.cs

SubEventSystemTests/
|-- SubEventTests.cs
```

## Requirements

- .NET 8+
- Windows (WPF demo)

## License

MIT
