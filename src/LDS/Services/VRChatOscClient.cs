// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using CommunityToolkit.Mvvm.Messaging;
using FastOSC;
using Microsoft.UI.Dispatching;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LDS.Messages;
using LDS.Models;
using LDS.Utilities;

namespace LDS.Services.VRChatOSC;

public interface IVRChatOscClient
{
    Task InitializeClient();
    void SendParameter<T>(string parameter, T value);
    void Send(string address, params object?[] values);
    void SendMovement(LeashData data);
    void SendMovement(float verticalOffset, float horizontalOffset, float horizontalLook, bool shouldRun);
    Task StopClient();
}

public partial class VRChatOscClient : IRecipient<ReconnectClientMessage>, IDisposable, IVRChatOscClient
{
    private readonly DispatcherQueue f_dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly HttpClient f_client;
    private readonly OSCSender f_sender;
    private readonly OSCReceiver f_receiver;
    private readonly OSCParameters f_leashContext;
    private readonly ApplicationSettings f_settings;
    private readonly IDebugLogger f_debugLogger;

    public VRChatOscClient(HttpClient client, OSCSender sender, OSCReceiver receiver, OSCParameters leashContext, ApplicationSettings settings, IDebugLogger debugLogger) {
        (f_client, f_sender, f_receiver, f_leashContext, f_settings, f_debugLogger) = (client, sender, receiver, leashContext, settings, debugLogger);
        StrongReferenceMessenger.Default.Register<ReconnectClientMessage>(this);
    }

    /// <summary>
    /// Initializes the client and setup OnMessageReceived handler.
    /// </summary>
    /// <returns></returns>
    public async Task InitializeClient() {
        try {
            f_client.Timeout = TimeSpan.FromMilliseconds(50);
            f_receiver.OnMessageReceived = Received;

            f_receiver.Connect(new IPEndPoint(IPAddress.Loopback, f_settings.ReceivePort));
            await f_sender.ConnectAsync(new IPEndPoint(IPAddress.Loopback, f_settings.SendPort));

            Log.Information("VRChat OSC client initialized successfully.");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to initialize VRChat OSC client.");
            throw;
        }
    }

    /// <summary>
    /// Receives OSC messages from the VRChat OSC client and updates the leash context accordingly.
    /// </summary>
    /// <param name="message"></param>
    private void Received(OSCMessage oscMessage) {
        VRChatOscMessage message = VRChatOscMessage.Create(oscMessage);

        if (message.MessageType != VRChatOscMessage.OscMessageType.AvatarParameter || message.Arguments.Length == 0) {
            return;
        }

        bool taskAdded = message.ParameterName switch {
            OSCParameters.IS_GRABBED => f_dispatcherQueue.TryEnqueue(() => f_leashContext.IsGrabbed = (bool)message.ParameterValue),
            OSCParameters.ANGLE => f_dispatcherQueue.TryEnqueue(() => f_leashContext.Angle = (float)message.ParameterValue),
            OSCParameters.STRETCH => f_dispatcherQueue.TryEnqueue(() => f_leashContext.Stretch = (float)message.ParameterValue),
            OSCParameters.FRONT_COLLIDER => f_dispatcherQueue.TryEnqueue(() => f_leashContext.FrontDistance = (float)message.ParameterValue),
            OSCParameters.BACK_COLLIDER => f_dispatcherQueue.TryEnqueue(() => f_leashContext.BackDistance = (float)message.ParameterValue),
            OSCParameters.RIGHT_COLLIDER => f_dispatcherQueue.TryEnqueue(() => f_leashContext.RightDistance = (float)message.ParameterValue),
            OSCParameters.LEFT_COLLIDER => f_dispatcherQueue.TryEnqueue(() => f_leashContext.LeftDistance = (float)message.ParameterValue),
            _ => true, // Ignore unknown parameters
        };
    }

    /// <summary>
    /// Updates an OSC parameter with the specified value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    public void SendParameter<T>(string parameter, T value) => Send(string.Concat(VRChatOscMessage.PARAMETER_PREFIX, parameter), value);

    /// <summary>
    /// Updates an OSC address with the specified values.
    /// </summary>
    /// <param name="address">The full address if the parameter</param>
    /// <param name="values">The value to set it to</param>
    public void Send(string address, params object?[] values) => f_sender.Send(new(address, values));

    /// <summary>
    /// Sends movement data to the VRChat OSC client.
    /// </summary>
    /// <param name="data"></param>
    public void SendMovement(LeashData data) {
        f_debugLogger.LogSend(JsonSerializer.Serialize(data));
        SendMovement(data.VerticalOffset, data.HorizontalOffset, data.HorizontalLook, data.ShouldRun);
    }

    /// <summary>
    /// Sends movement data to the VRChat OSC client with specified offsets and run state.
    /// </summary>
    /// <param name="verticalOffset"></param>
    /// <param name="horizontalOffset"></param>
    /// <param name="horizontalLook"></param>
    /// <param name="shouldRun"></param>
    public void SendMovement(float verticalOffset, float horizontalOffset, float horizontalLook, bool shouldRun) {
        Send(address: "/input/Vertical", values: verticalOffset);
        Send(address: "/input/Horizontal", values: horizontalOffset);
        Send(address: "/input/LookHorizontal", values: horizontalLook);
        Send(address: "/input/Run", values: shouldRun);
    }

    /// <summary>
    /// Attempts to reconnect the VRChat OSC client by disconnecting the current receiver and sender, then reconnecting them.
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<ReconnectClientMessage>.Receive(ReconnectClientMessage message) {
        try {
            await f_receiver.DisconnectAsync();
            f_sender.Disconnect();
            Log.Information("Reconnecting. Disconnected successfully");
            f_debugLogger.LogApp("Reconnecting. Disconnected successfully");
        }
        catch (Exception ex) {
            //Log the error but do not throw, and attempt to reconnect the client anyways.
            Log.Error(ex, "Failed to disconnect VRChat OSC client.");
            f_debugLogger.LogApp("Failed to disconnect VRChat OSC client.");
        }

        try {
            f_receiver.Connect(new IPEndPoint(IPAddress.Loopback, f_settings.ReceivePort));
            await f_sender.ConnectAsync(new IPEndPoint(IPAddress.Loopback, f_settings.SendPort));
            Log.Information("Reconnecting. Reconnected successfully");
            f_debugLogger.LogApp("Reconnecting. Disconnected successfully");
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to reconnect VRChat OSC client.");
            f_debugLogger.LogApp("Failed to disconnect VRChat OSC client.");

            message.Reply(false);
            return;
        }

        message.Reply(true);
    }

    /// <summary>
    /// Stops the VRChat OSC client by disconnecting the receiver and sender.
    /// </summary>
    /// <returns></returns>
    public async Task StopClient() {
        try {
            f_receiver.OnMessageReceived -= Received;

            await f_receiver.DisconnectAsync();
            f_sender.Disconnect();
        }
        catch (Exception ex) {
            Log.Error(ex, "Failed to stop VRChat OSC client.");
            f_debugLogger.LogApp("Failed to stop VRChat OSC client.");
        }
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        StrongReferenceMessenger.Default.Unregister<ReconnectClientMessage>(this);
    }
}

file record VRChatOscMessage(string Address, object?[] Arguments, string ParameterName, VRChatOscMessage.OscMessageType MessageType)
{
    public const string PARAMETER_PREFIX = "/avatar/parameters/";

    /// <summary>
    /// Creates a new instance of <see cref="VRChatOscMessage"/> from an <see cref="OSCMessage"/>.
    /// </summary>
    /// <param name="data">An instance of <see cref="OSCMessage"/> that should be converted</param>
    /// <returns></returns>
    public static VRChatOscMessage Create(OSCMessage data) {
        OscMessageType messageType = data.Address.StartsWith(PARAMETER_PREFIX) ? OscMessageType.AvatarParameter : OscMessageType.Other;
        string parameterName = messageType is OscMessageType.AvatarParameter ? data.Address[PARAMETER_PREFIX.Length..] : string.Empty;
        return new VRChatOscMessage(data.Address, data.Arguments, parameterName, messageType);
    }

    public object ParameterValue => Arguments[0]!;

    public enum OscMessageType { AvatarParameter, Other }
}

