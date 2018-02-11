using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using DMSApiWrapperDemo.Properties;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für PageEinstellungen.xaml
    /// </summary>
    public partial class PageEinstellungen : Page, INotifyPropertyChanged
    {
        /// <summary>
        /// Speichert die Instanz zwischen, damit wir auf sie von außen zurückgreifen können.
        /// </summary>
        private static PageEinstellungen pageEinstellungen;
        //Private Felder
        private string hostname;
        private int port;
        private string username;
        private string testText;
        private bool trustedDevice;
        private bool bypassLocalhost;
        private bool smoothScrolling;

        /// <summary>
        /// Hostname, IPAdresse des Servers
        /// </summary>
        public string Hostname { get { return hostname; } set { hostname = value; Notify(); } }
        /// <summary>
        /// Port des Servers
        /// </summary>
        public int Port { get { return port; } set { port = value; Notify(); } }
        /// <summary>
        /// Benutzername für den Server
        /// </summary>
        public string Username { get { return username; } set { username = value; Notify(); } }
        /// <summary>
        /// Der Text der angezeigt wird, wenn auf den Button Speichern geklickt wird.
        /// </summary>
        public string TestText { get { return testText; } set { testText = value; Notify(); } }
        /// <summary>
        /// The device is a trusted device in the DMS
        /// </summary>
        public bool TrustedDevice { get => trustedDevice; set { trustedDevice = value; Notify(); } }
        /// <summary>
        /// Avoids Playlists
        /// </summary>
        public bool BypassLocalhost { get => bypassLocalhost; set { bypassLocalhost = value; Notify(); } }
        /// <summary>
        /// Enables smooth scrolling in the media lists
        /// </summary>
        public bool SmoothScrolling { get => smoothScrolling; set { smoothScrolling = value; Notify(); } }

        /// <summary>
        /// Sicheres Passwort für den Server
        /// </summary>
        public SecureString Password;

        public event PropertyChangedEventHandler PropertyChanged;

        public PageEinstellungen()
        {
            InitializeComponent();
            pageEinstellungen = this;
            DataContext = this;
            LoadSettings();
        }

        /// <summary>
        /// Ereignis auslösen damit die UI Oberfläche aktualisiert wird.
        /// </summary>
        /// <param name="argument"></param>
        public void Notify([CallerMemberName] string argument = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argument));
        }

        internal static PageEinstellungen GetInstance()
        {
            return pageEinstellungen;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Hostname) || Port == 0)
            {
                TestText = Properties.Resources.SettingsHostNamePort;
                MessageBox.Show(Properties.Resources.SettingsHostNamePort, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!TrustedDevice && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(pwBox.Password)))
            {
                TestText = Properties.Resources.SettingsPWUSER;
                MessageBox.Show(Properties.Resources.SettingsPWUSER, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (pwBox.Password.Length > 0)
            {
                //Password für die aktuelle Session
                Password = new SecureString();
                foreach (char item in pwBox.Password)
                {
                    Password.AppendChar(item);
                }
                Password.MakeReadOnly();

                //Einstellungen speichern
                Settings.Default.Password = DVBViewerServerApiWrapper.Helper.Security.GenerateEnrcyptedPassword(pwBox.Password, out string entropy);
                Settings.Default.Entropy = entropy;
            }
            Settings.Default.Hostname = Hostname;
            Settings.Default.Port = Port;
            Settings.Default.UserName = Username;
            Settings.Default.TrustedDevice = TrustedDevice;
            Settings.Default.BypassLocalhost = BypassLocalhost;
            //Settings.Default.SmoothScrolling = SmoothScrolling;
            Settings.Default.Save();

            //Api aufbauen
            var dvbApi = new DVBViewerServerApiWrapper.DVBViewerServerApi
            {
                Hostname = hostname,
                User = username,
                Port = port,
                Password = Password,
                TrustedDevice = TrustedDevice,
                BypassLocalhost = BypassLocalhost,
            };

            //Bei Fehler Nachricht anzeigen
            try
            {
                TestText = (await dvbApi.ServerVersionAsync).Version;
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                    TestText = ex.Message;
                else
                    TestText = ex.InnerException.Message;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        /// <summary>
        /// Einstellungen laden
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (Settings.Default.Password.Length > 0)
                {
                    //Password in den SecureString bringen
                    pwBox.Password = DVBViewerServerApiWrapper.Helper.Security.GenerateUnEnrcyptedPassword(Settings.Default.Password, Settings.Default.Entropy);
                    Password = new SecureString();
                    foreach (char item in pwBox.Password)
                    {
                        Password.AppendChar(item);
                    }
                    Password.MakeReadOnly();
                }
                //Restliche Einstellungen
                Hostname = Settings.Default.Hostname;
                Port = Settings.Default.Port;
                Username = Settings.Default.UserName;
                BypassLocalhost = Settings.Default.BypassLocalhost;
                TrustedDevice = Settings.Default.TrustedDevice;
                //SmoothScrolling = Settings.Default.SmoothScrolling;

                //Kein Fehler, dann Text löschen
                TestText = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
