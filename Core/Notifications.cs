using GameOverlay.Drawing;

namespace CS2.Core;

public enum NotificationType
{
    Success,
    Danger,
    Warning,
    Info
}

public class Notifications
{
    private class Notification
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public NotificationType Type { get; set; } = NotificationType.Success;
        public DateTime Time { get; set; }
        public int Stage { get; set; }

        public string TypeString => Type switch
        {
            NotificationType.Success => "notification-success",
            NotificationType.Danger => "notification-danger",
            NotificationType.Warning => "notification-warning",
            NotificationType.Info => "notification-info",
            _ => "notification-info"
        };
    }

    private static List<Notification> _notifications = new();
    private static List<Notification> _toRemove = new();
    private static DateTime _lastNotification = DateTime.Now;

    public static void Draw(Overlay overlay, Graphics gfx, System.Drawing.Point cursorPos)
    {
        var y = 10;
        _toRemove.Clear();

        foreach (var notification in _notifications)
        {
            DrawNotification(overlay, gfx, notification, ref y);
        }

        foreach (var notification in _toRemove)
        {
            _notifications.Remove(notification);
        }
    }

    private static void DrawNotification(Overlay overlay, Graphics gfx, Notification notification, ref int y)
    {
        var titleSize = gfx.MeasureString(overlay.fonts["arial-bold"], 16, notification.Title);
        var messageSize = gfx.MeasureString(overlay.fonts["arial"], 14, notification.Message);

        var width = 310;
        width = Math.Max(width, (int)(titleSize.X + 14));
        width = Math.Max(width, (int)(messageSize.X + 14));

        var height = (int)(titleSize.Y + messageSize.Y + 16);
        var x = gfx.Width - width - 10;

        // update stages, 0 - in, 1 - wait 3 s, 2 - out
        if (notification.Stage == 0)
        {
            var progress = (DateTime.Now - notification.Time).TotalMilliseconds / 300;
            if (progress >= 1)
                notification.Stage = 1;
            else
                x = gfx.Width - (int)((width + 10) * progress);
        }
        else if (notification.Stage == 1)
        {
            if (DateTime.Now > notification.Time.AddSeconds(3))
                notification.Stage = 2;
        }
        else if (notification.Stage == 2)
        {
            var progress = (DateTime.Now - notification.Time.AddSeconds(3)).TotalMilliseconds / 300;
            if (progress >= 1)
            {
                // _notifications.Remove(notification);
                _toRemove.Add(notification);
                return;
            }
            else
                x = gfx.Width - (int)((width + 10) * (1 - progress));
        }

        // DrawRoundedRectangle(Graphics gfx, int x, int y, int width, int height, IBrush brush, int radius)
        Windows.DrawRoundedRectangle(gfx, x, y, width, height, overlay.gradients[notification.TypeString], 5);

        gfx.DrawText(overlay.fonts["arial-bold"], 16, overlay.colors[notification.TypeString], x + 7, y + 7, notification.Title);
        gfx.DrawText(overlay.fonts["arial"], 14, overlay.colors["white2"], x + 7, y + 9 + titleSize.Y, notification.Message);

        y += height + 5;
    }

    public static void AddNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        _notifications.Add(new Notification
        {
            Title = title,
            Message = message,
            Time = DateTime.Now,
            Stage = 0,
            Type = type
        });
    }
}