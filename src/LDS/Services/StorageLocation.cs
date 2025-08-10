using System;
using System.IO;

namespace LDS.Services;
public static class StorageLocation
{
    const string APPNAME = "Leash Drag System";
    static string f_appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string GetAppdataPath() => Path.Combine(f_appdataPath, APPNAME);

    public static string GetLogPath() => Path.Combine(GetAppdataPath(), "Logs");
    public static string GetLogFile() => Path.Combine(GetLogPath(), "app.log");

    public static string GetDatabasePath() => Path.Combine(GetAppdataPath(), "database.db");

    public static void EnsureAppdataPathExists() {
        if (!Directory.Exists(GetAppdataPath())) {
            Directory.CreateDirectory(GetAppdataPath());
        }

        if (!Directory.Exists(GetLogPath())) {
            Directory.CreateDirectory(GetLogPath());
        }
    }
}
