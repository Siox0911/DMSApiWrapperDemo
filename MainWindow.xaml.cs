using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DVBViewerServerApiWrapper;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Wird true wenn das Hamburgermenu eingeklappt ist. (Default)
        /// </summary>
        private bool hamburgerMenuCollapsed = true;
        private bool error;
        /// <summary>
        /// Die Api
        /// </summary>
        private DVBViewerServerApi dvbApi;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: Es gibt noch kein Item für die Anwendung
            //Icon = Properties.Resources.Werkbuch464x64.ToImageSource();
            var settings = PageEinstellungen.GetInstance() ?? new PageEinstellungen();

            //Api instanziieren
            dvbApi = new DVBViewerServerApi
            {
                Hostname = settings.Hostname,
                Port = settings.Port,
                User = settings.Username,
                Password = settings.Password
            };

            //Wenn etwas bei der Instanziierung schief geht, dann gehe direkt zur Einstellungsseite
            try
            {
                var version = await dvbApi.ServerVersionAsync;
                Title = $"DMSApiWrapperDemo {Assembly.GetExecutingAssembly().GetName().Version}";
                Title = $"{Title} - {Properties.Resources.ConnectedWith} {version.Version}";
                error = false;
            }
            catch (Exception)
            {
                error = true;
                frameContent.Navigate(settings);
            }
        }

        /// <summary>
        /// Navigation linker Bereich
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HamburgerMenuMouseDown(object sender, MouseButtonEventArgs e)
        {
            var stackpanel = (StackPanel)sender;
            //TODO: Die Pages werden hier geladen
            try
            {
                if (error)
                {
                    Window_Loaded(sender, e);
                }

                switch (stackpanel.Name)
                {
                    case ("btnHamburger"):
                        OpenCloseHamburgerMenu();
                        break;
                    case ("btnAufnahmen"):
                        if (!error)
                        {
                            //Aufnahmen nur instanziieren, wenn noch nicht getan
                            var pa = PageAufnahmen.GetInstance() ?? new PageAufnahmen();
                            frameContent.Navigate(pa);
                        }
                        //HamburgerMenu schließen
                        if (!hamburgerMenuCollapsed) OpenCloseHamburgerMenu();
                        break;
                    case ("btnVideos"):
                        if (!error)
                        {
                            //Videos nur instanziieren, wenn noch nicht getan
                            var pv = PageVideos.GetInstance() ?? new PageVideos();
                            frameContent.Navigate(pv);
                        }
                        //HamburgerMenu schließen
                        if (!hamburgerMenuCollapsed) OpenCloseHamburgerMenu();
                        break;
                    case ("btnClient"):
                        if (!error)
                        {
                            //HamburgerMenu schließen
                            if (!hamburgerMenuCollapsed) OpenCloseHamburgerMenu();
                        }
                        break;
                    case ("btnTasks"):
                        if (!error)
                        {
                            //HamburgerMenu schließen
                            if (!hamburgerMenuCollapsed) OpenCloseHamburgerMenu();
                        }
                        break;
                    case ("btnSrvStatus"):
                        if (!error)
                        {
                            //HamburgerMenu schließen
                            if (!hamburgerMenuCollapsed) OpenCloseHamburgerMenu();
                        }
                        break;
                    case ("btnSettings"):
                        var settings = PageEinstellungen.GetInstance() ?? new PageEinstellungen();
                        frameContent.Navigate(settings);
                        //HamburgerMenu schließen
                        if (!hamburgerMenuCollapsed) OpenCloseHamburgerMenu();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    MessageBox.Show(ex.InnerException.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Alternative zum MouseDown im Stackpanel... Hier wird die Anweisung einfach auf das MouseDown Ereignis umgeleitet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HamburgerMenuKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Hier wird die Anweisung einfach auf das MouseDown Ereignis umgeleitet
                HamburgerMenuMouseDown(sender, new MouseButtonEventArgs(null, 0, MouseButton.Left));
            }
        }


        /// <summary>
        /// Öffnet und schließt das HamburgerMenu
        /// </summary>
        private void OpenCloseHamburgerMenu()
        {
            //Da es keine UWP Anwendung ist, helfen wir uns hier drüber.
            if (hamburgerMenuCollapsed)
            {
                stackMenuLeft.Width = 250;
                hamburgerMenuCollapsed = false;
            }
            else
            {
                stackMenuLeft.Width = 54;
                hamburgerMenuCollapsed = true;
            }
        }
    }
}
