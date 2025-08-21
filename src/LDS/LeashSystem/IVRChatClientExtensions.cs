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
        client.SendParameterChange("/input/Vertical",verticalOffset);
        client.SendParameterChange("/input/Horizontal", horizontalOffset);
        client.SendParameterChange("/input/LookHorizontal", horizontalLook);
        client.SendParameterChange("/input/Run", shouldRun);
    }
}
