using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Unosquare.FFME;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private readonly CultureInfo cultureOverride = new CultureInfo("en");

        protected override void OnStartup(StartupEventArgs e)
        {
#if !DEBUG
            this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;
#endif
            if (Thread.CurrentThread.CurrentUICulture.ToString().IndexOf("de-DE", StringComparison.CurrentCultureIgnoreCase) == -1 && cultureOverride != null)
            {
                Thread.CurrentThread.CurrentUICulture = cultureOverride;
                Thread.CurrentThread.CurrentCulture = cultureOverride;
            }

            //if (Debugger.IsAttached && cultureOverride != null)
            //{
            //    Thread.CurrentThread.CurrentUICulture = cultureOverride;
            //    Thread.CurrentThread.CurrentCulture = cultureOverride;
            //}

            //MessageBox.Show($@"{Environment.CurrentDirectory}\ffmpeg");
            Library.FFmpegDirectory = $@"{Environment.CurrentDirectory}\ffmpeg";

            Library.FFmpegLoadModeFlags = FFmpegLoadMode.LibraryFlags["avcodec"] | FFmpegLoadMode.LibraryFlags["avfilter"] | FFmpegLoadMode.FullFeatures;

            // Pre-load FFmpeg libraries in the background. This is optional.
            // FFmpeg will be automatically loaded if not already loaded when you try to open
            // a new stream or file. See issue #242
            Task.Run(async () =>
            {
                try
                {
                    // Pre-load FFmpeg
                    await Library.LoadFFmpegAsync();
                }
                catch (Exception ex)
                {
                    var dispatcher = Current?.Dispatcher;
                    if (dispatcher != null)
                    {
                        await dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(MainWindow,
                                $"Unable to Load FFmpeg Libraries from path:\r\n    {Library.FFmpegDirectory}" +
                                $"\r\nMake sure the above folder contains FFmpeg shared binaries (dll files) for the " +
                                $"applicantion's architecture ({(Environment.Is64BitProcess ? "64-bit" : "32-bit")})" +
                                $"\r\nTIP: You can download builds from https://ffmpeg.org/download.html" +
                                $"\r\n{ex.GetType().Name}: {ex.Message}\r\n\r\nApplication will exit.",
                                "FFmpeg Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                            Current?.Shutdown();
                        }));
                    }
                }
            });

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.InnerException != null)
                MessageBox.Show(e.Exception.InnerException + Environment.NewLine, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(e.Exception + Environment.NewLine, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
