using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LDS.Interface.Messages;

internal class InvokeExitMessage(ExitMessageData value) : ValueChangedMessage<ExitMessageData>(value);
internal record ExitMessageData(string SenderName);
