using LiteDB;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;

namespace LDS.Services;

public interface IAppResizeService
{
    void SetWindow(Window window);
}

public class AppResizeService(ILiteDatabase database) : IAppResizeService
{
    private readonly ILiteCollection<AppSize> _collection = database.GetCollection<AppSize>();

    private static AppWindow GetAppWindow(Window window) {
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    public void SetWindow(Window window) {
        window.Closed += Window_Closed;

        AppSize? sizing = _collection.FindById(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1));
        if (sizing is not null) {
            AppWindow appWindow = GetAppWindow(window);
            appWindow.MoveAndResize(sizing);
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args) {
        AppWindow appWindow = GetAppWindow((Window)sender);
        AppSize size = new RectInt32(appWindow.Position.X, appWindow.Position.Y, appWindow.Size.Width, appWindow.Size.Height);
        _collection.Upsert(size);
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