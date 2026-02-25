using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;

namespace LDS.UI.Services;

public interface IAppResizeService
{
    void SetWindow(Window window);
}

public class AppResizeService(ILogger<AppResizeService> logger , ILiteDatabase database) : IAppResizeService
{
    private readonly ILogger<AppResizeService> _logger = logger;
    private readonly ILiteCollection<AppSize> _collection = database.GetCollection<AppSize>();

    private static AppWindow GetAppWindow(Window window) {
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    public void SetWindow(Window window) {
        window.Closed += Window_Closed;

        AppSize? size = _collection.FindById(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1));
        if (size is not null) {
            _logger.LogDebug("Recovered previous app size. width: {width} height: {height}, position x: {posx} position y: {posy}", size.Width, size.Height, size.PositionX, size.PositionY);

            AppWindow appWindow = GetAppWindow(window);
            appWindow.MoveAndResize(size);
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args) {
        AppWindow appWindow = GetAppWindow((Window)sender);
        AppSize size = new RectInt32(appWindow.Position.X, appWindow.Position.Y, appWindow.Size.Width, appWindow.Size.Height);
        _collection.Upsert(size);
        _logger.LogDebug("Saved window size. width: {width} height: {height}, position x: {posx} position y: {posy}", size.Width, size.Height, size.PositionX, size.PositionY);
    }
}

public class AppSize
{
    [BsonId] public Guid Id { get; set; } = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    public int Width { get; set; }
    public int Height { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public static implicit operator RectInt32(AppSize size) => new(size.PositionX, size.PositionY, size.Width, size.Height);
    public static implicit operator AppSize(RectInt32 rect) => new() { Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1), PositionX = rect.X, PositionY = rect.Y, Width = rect.Width, Height = rect.Height };
}