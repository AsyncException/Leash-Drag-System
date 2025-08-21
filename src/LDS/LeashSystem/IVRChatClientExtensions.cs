using LDS.Services.VRChatOSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRChatOSCClient;
using VRChatOSCClient.OSCConnections;

namespace LDS.LeashSystem;
internal static class IVRChatClientExtensions
{
    public static void SendMovement(this IVRChatClient client, MovementData data) => client.SendMovement(data.VerticalOffset, data.HorizontalOffset, data.HorizontalLook, data.ShouldRun);
    public static void SendMovement(this IVRChatClient client, float verticalOffset, float horizontalOffset, float horizontalLook, bool shouldRun) {
        client.Send(new Message("/input/Vertical", [verticalOffset]));
        client.Send(new Message("/input/Horizontal", [horizontalOffset]));
        client.Send(new Message("/input/LookHorizontal", [horizontalLook]));
        client.Send(new Message("/input/Run", [shouldRun]));
    }
}
