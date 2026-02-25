namespace LDS.Core.CommunicationObjects;

public interface IConnectionStatus
{
    bool IsConnected { get; }
    bool IsUnityConnected { get; }
    int ReceivePort { get; }
    int SendPort { get; }

    event EventHandler<bool> OnIsConnectedChanged;
    event EventHandler<bool> OnIsUnityConnectedChanged;
    event EventHandler<int> OnReceivePortChanged;
    event EventHandler<int> OnSendPortChanged;
}

public sealed class ConnectionStatus : IConnectionStatus
{

    public event EventHandler<bool> OnIsUnityConnectedChanged = delegate { };
    public bool IsUnityConnected {
        get;
        set {
            field = value;
            OnIsUnityConnectedChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<bool> OnIsConnectedChanged = delegate { };
    public bool IsConnected {
        get;
        set {
            field = value;
            OnIsConnectedChanged?.Invoke(this, value);
        }
    }


    public event EventHandler<int> OnReceivePortChanged = delegate { };
    public int ReceivePort {
        get;
        set {
            field = value;
            OnReceivePortChanged?.Invoke(this, value);
        }
    }


    public event EventHandler<int> OnSendPortChanged = delegate { };
    public int SendPort {
        get;
        set {
            field = value;
            OnSendPortChanged?.Invoke(this, value);
        }
    }
}



