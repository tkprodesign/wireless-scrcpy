using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Notifications;

public sealed record NotificationMessage(string Title, string Body, Severity Severity);
