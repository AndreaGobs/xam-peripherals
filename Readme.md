# Xamarin Forms app

Basic Xamarin Forms app for developing and testing peripheral functionalities.

1. Implemented Bluetooth client and server working both in Low Energy mode and Socket mode for Android.
2. Implemented WebSocket client for Android and iOS via SignalR.

## SignalR

To test the SignalR client is necessary to use the Android emulator connected to the running SignalR server in the localhost *http://localhost:5000*.
```
hubConnection = new HubConnectionBuilder()
	.WithUrl($"http://10.0.2.2:5000/chatHub")
	.Build();
```