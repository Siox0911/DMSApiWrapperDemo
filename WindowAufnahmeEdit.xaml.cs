using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DVBViewerServerApiWrapper.Model;
using DVBViewerServerApiWrapper;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für WindowAufnahmeEdit.xaml
    /// </summary>
    public partial class WindowAufnahmeEdit : Window, INotifyPropertyChanged
    {
        private static WindowAufnahmeEdit aufnahmeEdit;
        private DVBViewerServerApi serverApi;
        //Die zu bearbeitende Aufnahme
        private RecordingItem recordingItem;
        //Titel der Aufnahme
        private string title;
        //Info der Aufnahme (Untertitel)
        private string info;
        //Kanal
        private string channel;
        //Die ausgewählte oder eingebene Serie
        private string seriesValue;
        //Alle Serien
        private List<RecordingSeries> series;
        //Die Beschreibung
        private string description;

        /// <summary>
        /// Die Serien
        /// </summary>
        public List<RecordingSeries> Series { get => series; set { series = value; Notify(); } }
        /// <summary>
        /// Der Titel der Aufnahme
        /// </summary>
        public string Titel { get => title; set { title = value; Notify(); } }
        /// <summary>
        /// Der Untertitel (Info) der Aufnahme
        /// </summary>
        public string Info { get => info; set { info = value; Notify(); } }
        /// <summary>
        /// Der Kanal der Aufnahme
        /// </summary>
        public string Channel { get => channel; set { channel = value; Notify(); } }
        /// <summary>
        /// Die ausgewählte Serie oder der eingegebene Text
        /// </summary>
        public string SeriesValue { get => seriesValue; set { seriesValue = value; Notify(); } }
        /// <summary>
        /// Die Langbeschreibung der Aufnahme
        /// </summary>
        public string Description { get => description; set { description = value; Notify(); } }

        public WindowAufnahmeEdit(RecordingItem recordingItem)
        {
            InitializeComponent();
            DataContext = this;
            this.recordingItem = recordingItem;
            aufnahmeEdit = this;
        }

        /// <summary>
        /// Event für die UI Aktualisierung
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal static WindowAufnahmeEdit GetInstance()
        {
            return aufnahmeEdit;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            serverApi = DVBViewerServerApi.GetCurrentInstance();
            Series = RecordingSeries.GetSeries();
            SeriesValue = recordingItem.Series.Name;
            Titel = recordingItem.Title;
            Info = recordingItem.Info ?? "";
            Channel = recordingItem.Channel.Name;
            Description = recordingItem.Description;
        }

        /// <summary>
        /// Ereignis auslösen damit die UI Oberfläche aktualisiert wird.
        /// </summary>
        /// <param name="argument"></param>
        public void Notify([CallerMemberName] string argument = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argument));
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            //Die geänderten Werte der Aufnahme zuordnen
            recordingItem.Title = Titel;
            recordingItem.Series.Name = SeriesValue;
            recordingItem.Channel.Name = Channel;
            recordingItem.Info = Info;
            recordingItem.Description = Description;
            //Update dem Server übermitteln
            var res = await recordingItem.Update().ConfigureAwait(false);
            //Update auswerten
            if(res == System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("Die Änderung war erfolgreich");
            }
            else
            {
                MessageBox.Show($"Änderung fehlgeschlagen, der Server antwortete mit der Meldung: {res}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
