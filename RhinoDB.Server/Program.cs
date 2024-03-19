﻿using Serilog;
using Serilog.Events;
using System.IO.Compression;
using RhinoDB.Server.Data;

namespace RhinoDB.Server;

internal static class Program
{
    public static void Main(string[] args)
    {
        ApplicationConfiguration.Instance.Initialize(Files.ApplicationConfiguration);
        ConfigureLogging();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSerilog();
        builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
        builder.Services.AddRazorPages().WithRazorPagesRoot("/Pages");

        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseStatusCodePagesWithRedirects("/error/{0}");
            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
        }


        // Add the middleware to the pipeline.
        app.UseRouting();
        app.MapControllers();
        app.MapRazorPages();
        app.UseAuthorization();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Handle application exit.
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            ApplicationConfiguration.Instance.Save();

            Log.Debug("Application exiting after {TIME}.", ApplicationData.UpTime);
            Log.CloseAndFlush();
        };


        // Handle unhandled exceptions.
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Fatal(exception, "Unhandled exception: {REPORT}", CrashHandler.Report(exception));
            }
        };

        app.Run($"http://localhost:{ApplicationConfiguration.Instance.Port}");
    }

    /// <summary>
    /// Configures the logging for the application.
    /// </summary>
    private static void ConfigureLogging()
    {
        // Initialize Logging
        string[] logs = Directory.GetFiles(Directories.Logs, "*.log");
        if (logs.Any())
        {
            using ZipArchive archive = ZipFile.Open(Path.Combine(Directories.Logs, $"logs-{DateTime.Now:MM-dd-yyyy HH-mm-ss.ffff}.zip"), ZipArchiveMode.Create);
            foreach (string log in logs)
            {
                archive.CreateEntryFromFile(log, Path.GetFileName(log));
                File.Delete(log);
            }
        }

        TimeSpan flushTime = TimeSpan.FromSeconds(30);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(ApplicationConfiguration.Instance.LogLevel, outputTemplate: $"[{ApplicationData.ApplicationName}] [{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}")
            .WriteTo.File(Files.DebugLog, LogEventLevel.Verbose, buffered: true, flushToDiskInterval: flushTime)
            .WriteTo.File(Files.LatestLog, LogEventLevel.Information, buffered: true, flushToDiskInterval: flushTime)
            .WriteTo.File(Files.ErrorLog, LogEventLevel.Error, buffered: false)
            .CreateLogger();
    }
}