using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LDS.Interface.Messages;

public class ToggleUnityMessage : RequestMessage<Task<bool>>;
