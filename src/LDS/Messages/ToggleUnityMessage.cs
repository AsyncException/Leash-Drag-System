using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Threading.Tasks;

namespace LDS.UI.Messages;

public class ToggleUnityMessage : RequestMessage<Task<bool>>;
