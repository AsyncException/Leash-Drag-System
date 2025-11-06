using Serilog;
using System;
using System.IO;

namespace LDS.Services;
public static class StorageLocation
{
    const string APPNAME = "Leash Drag System";
    static readonly string _appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    static readonly string _appPath = AppDomain.CurrentDomain.BaseDirectory;
    public static string GetAppdataPath() => Path.Combine(_appdataPath, APPNAME);

    public static string GetManifestPath() => Path.Combine(_appPath, "app.vrmanifest");

    public static string GetLogPath() => Path.Combine(GetAppdataPath(), "Logs");
    public static string GetLogFile() => Path.Combine(GetLogPath(), "app.log");

    public static string GetDatabasePath() => Path.Combine(GetAppdataPath(), "database.db");

    public static bool ManifestPathExists() => File.Exists(GetManifestPath());

    public static void EnsureAppdataPathExists() {
        if (!Directory.Exists(GetAppdataPath())) {
            Log.Logger.Debug("Creating AppData folder");
            Directory.CreateDirectory(GetAppdataPath());
        }

        if (!Directory.Exists(GetLogPath())) {
            Log.Logger.Debug("Creating Log folder");
            Directory.CreateDirectory(GetLogPath());
        }
    }
}
