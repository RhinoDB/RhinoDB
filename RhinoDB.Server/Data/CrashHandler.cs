using Newtonsoft.Json;
using Serilog;

namespace RhinoDB.Server.Data;

public static class CrashHandler
{
    /// <summary>
    /// Reports an exception and generates a crash report. This method logs an error message indicating that a crash report has been generated, then exits the application with a status code
    /// of 1.
    /// </summary>
    /// <param name="ex">The exception to report.</param>
    public static void ReportAndExit(Exception ex)
    {
        string file = Report(ex);

        Log.Error("A crash report has been generated at {file}", file);
        Environment.Exit(1);
    }

    /// <summary>
    /// Reports an exception and generates a crash report.
    /// </summary>
    /// <param name="ex">The exception to report.</param>
    /// <returns>The path to the generated crash report file.</returns>
    public static string Report(Exception ex)
    {
        DateTime now = DateTime.Now;
        string formattedTime = now.ToString("dddd MMMM dd, yyyy - hh-mm-ss.fff tt");
        string file = Path.Combine(Directories.Root, $"crash ({formattedTime}).txt");

        using FileStream fs = new(file, FileMode.Create);
        using StreamWriter writer = new(fs);
        writer.WriteLine($"{ApplicationData.ApplicationName} Crash Report - {formattedTime}");
        writer.WriteLine("\nApplication Data:");
        writer.WriteLine($"\tOS: {Environment.OSVersion.VersionString}");
        writer.WriteLine($"\tVersion: {ApplicationData.Version}");
        writer.WriteLine("\nCrash Data:");
        writer.WriteLine($"\tMessage: {ex.Message}");
        writer.WriteLine($"\tSource: {ex.Source}");
        writer.WriteLine($"\tData: {JsonConvert.SerializeObject(ex.Data)}");

        writer.WriteLine($"Stack Trace:\n{ex.StackTrace}");

        return file;
    }
}