using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {

        private CultureInfo cultureOverride = new CultureInfo("en");

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
