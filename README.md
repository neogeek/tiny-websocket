# 🔧 TinyWebSocket

## Install

### .NET

Download the latest DLL from [releases](https://github.com/neogeek/tiny-websocket/releases/) and include in your project.

Add the following to your project config (changing the directory to match your file structure):

```xml
<ItemGroup>
    <Reference Include="Libs\*.dll" />
</ItemGroup>
```

### Unity

Open the Unity Package Managers, Add new package via git URL and enter the following URL:

```
https://github.com/neogeek/tiny-websocket.git?path=/UnityPackage#v1.0.0
```

## Usage

```csharp
var cancellationTokenSource = new CancellationTokenSource();

var ws = await WebSocket.Connect("ws://localhost:5000", cancellationTokenSource.Token);

await ws.SendMessage("Hello, world!", cancellationTokenSource.Token);

while (ws.IsOpen())
{
    var message = await ws.ListenForNextMessage(cancellationTokenSource.Token);

    Console.WriteLine(message);
}
```
