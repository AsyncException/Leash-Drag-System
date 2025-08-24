using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Threading.Tasks;

namespace LDS.Messages;

public class StartUnityMessage : RequestMessage<Task<bool>>;
