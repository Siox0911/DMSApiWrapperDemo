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
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(argument));
            }
        }

        internal static PageEinstellungen GetInstance()
        {
            return pageEinstellungen;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Hostname) || Port == 0 || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(pwBox.Password))
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
                Properties.Settings.Default.Password = DVBViewerServerApiWrapper.Helper.Security.GenerateEnrcyptedPassword(pwBox.Password, out string entropy);
                Properties.Settings.Default.Entropy = entropy;
                Properties.Settings.Default.Hostname = Hostname;
                Properties.Settings.Default.Port = Port;
                Properties.Settings.Default.UserName = Username;
                Properties.Settings.Default.Save();

                //Api aufbauen
                var dvbApi = new DVBViewerServerApiWrapper.DVBViewerServerApi
                {
                    Hostname = hostname,
                    User = username,
                    Port = port,
                    Password = Password
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
                if (Properties.Settings.Default.Password.Length > 0)
                {
                    //Password in den SecureString bringen
                    pwBox.Password = DVBViewerServerApiWrapper.Helper.Security.GenerateUnEnrcyptedPassword(Properties.Settings.Default.Password, Properties.Settings.Default.Entropy);
                    Password = new SecureString();
                    foreach (char item in pwBox.Password)
                    {
                        Password.AppendChar(item);
                    }
                    Password.MakeReadOnly();
                }
                //Restliche Einstellungen
                Hostname = Properties.Settings.Default.Hostname;
                Port = Properties.Settings.Default.Port;
                Username = Properties.Settings.Default.UserName;

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
