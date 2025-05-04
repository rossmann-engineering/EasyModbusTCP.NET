using System;
using System.IO;

namespace ModbusTCP;

/// <summary>
/// Store Log-Data in a File
/// </summary>
public sealed class StoreLogData {

    private static volatile StoreLogData instance;
    private static object syncObject = new();

    /// <summary>
    /// Gets or Sets the Filename to Store Strings in a File
    /// </summary>
    public string Filename { get; set; } = null;

    /// <summary>
    /// Private constructor; Ensures the access of the class only via "instance"
    /// </summary>
    private StoreLogData() { }

    /// <summary>
    /// Returns the instance of the class (singleton)
    /// </summary>
    /// <returns>instance (Singleton)</returns>
    public static StoreLogData Instance {
        get {
            if (instance == null) {
                lock (syncObject) {
                    instance ??= new StoreLogData();
                }
            }

            return instance;
        }
    }

    /// <summary>
    /// Store message in Log-File
    /// </summary>
    /// <param name="message">Message to append to the Log-File</param>
    public void Store(string message) {
        if (Filename == null)
            return;

        using StreamWriter file = new(Filename, true);
        file.WriteLine(message);
    }

    /// <summary>
    /// Store message in Log-File including Timestamp
    /// </summary>
    /// <param name="message">Message to append to the Log-File</param>
    /// <param name="timestamp">Timestamp to add to the same Row</param>
    public void Store(string message, DateTime timestamp) {
        try {
            using StreamWriter file = new(Filename, true);
            file.WriteLine(timestamp.ToString("dd.MM.yyyy H:mm:ss.ff ") + message);
        }
        catch { }
    }
}
