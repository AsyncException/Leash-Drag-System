using LDS.Models;
using LDS.Services.VRChatOSC;
using VRChatOSCClient;

namespace LDS.TimerSystem;
internal static class IVRChatClientExtensions
{
    /// <summary>
    /// Sends the Hour Minute and Second of the Timespan as percentages.
    /// </summary>
    /// <param name="storage"></param>
    public static void SendTime(this IVRChatClient client, TimerStorage storage) {
        client.SendParameterChange(OSCParameters.HOUR, storage.Hours * 0.01f);
        client.SendParameterChange(OSCParameters.HOUR, storage.Minutes * 0.01f);
        client.SendParameterChange(OSCParameters.HOUR, storage.Seconds * 0.01f);
    }
}
