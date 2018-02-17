using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DVBViewerServerApiWrapper.Model;
using DVBViewerServerApiWrapper;
using System.Diagnostics;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für PageAufnahmen.xaml
    /// </summary>
    public partial class PageAufnahmen : Page, INotifyPropertyChanged
    {
        //Wird benötigt, damit die eigene Instanz zurückgegeben werden kann.
        private static PageAufnahmen pageAufnahmen;
        //Liste mit den Aufnahmen: Denke daran, dass die Aufnahmen im Items Property stecken
        private RecordingList recordings;
        //die ausgewählte Aufnahme, das ist immer die erste ausgewählte: Auch wenn mehrere ausgewählt sind
        private RecordingItem recordingItem;
        //Die Liste der Aufnahmen, wenn mehrere ausgewählt sind
        private List<RecordingItem> recordingItems;
        //Der ApiWrapper
        private DVBViewerServerApi serverApi;
        //Liste der DVBViewer Clients
        private DVBViewerClients clients;
        //Der ausgewählte DVBViewer Client
        private DVBViewerClient client;
        //Text für die Suche in der Beschreibung
        private string searchDesc;
        //Text für die Suche im Titel
        private string searchTitle;
        //Anzahl der Aufnahmen (aktuell in der Liste)
        private int anzahlAufnahmen;
        //Die Serien
        private List<RecordingSeries> series;
        //Die Serie, welche ausgewählt wurde
        private RecordingSeries serie;
        //Die Sender
        private List<RecordingChannel> channels;
        //Der ausgewählte Sender
        private RecordingChannel channel;
        //Wenn der EditMode on ist, ist dies True
        private bool editMode;
        //Serienname, entweder der Text des ausgwählten Items oder der Text der hinein geschrieben wurde
        private string seriesValue;
        //Nach welcher Spalte wurde sortiert
        private string sortingColumn = Properties.Resources.Recordingdate;
        //Wie war die Sortierrichtung
        private ListSortDirection sortingColumnDirection = ListSortDirection.Descending;

        //EventHandler, für die Aktualisierung der Oberfläche
        public event PropertyChangedEventHandler PropertyChanged;

        //Alle Public Properties für die Oberfläche

        /// <summary>
        /// Aufnahmeliste
        /// </summary>
        public RecordingList Recordings { get { return recordings; } set { recordings = value; Notify(); } }
        /// <summary>
        /// Ausgewähltes Aufnahme Item
        /// </summary>
        public RecordingItem RecordingItem { get { return recordingItem; } set { recordingItem = value; Notify(); } }
        /// <summary>
        /// Liste der DVBViewer Clients
        /// </summary>
        public DVBViewerClients Clients { get { return clients; } set { clients = value; Notify(); } }
        /// <summary>
        /// Der ausgewählte DVBViewer Client
        /// </summary>
        public DVBViewerClient Client { get { return client; } set { client = value; Notify(); } }
        /// <summary>
        /// Der Suchetext in der Beschreibung
        /// </summary>
        public string SearchDesc { get { return searchDesc; } set { searchDesc = value; Notify(); } }
        /// <summary>
        /// Der Suchtext im Title
        /// </summary>
        public string SearchTitle { get { return searchTitle; } set { searchTitle = value; Notify(); } }
        /// <summary>
        /// Anzahl der Aufnahmen
        /// </summary>
        public int AnzahlAufnahmen { get { return anzahlAufnahmen; } set { anzahlAufnahmen = value; Notify(); } }
        /// <summary>
        /// Pfad für die Bilddatei
        /// </summary>
        public string ImagePath
        {
            get
            {
                if (RecordingItem != null && Recordings != null && Recordings.ImageURL != null && RecordingItem.Image != null)
                {
                    return Recordings.ImageURL + RecordingItem.Image;
                }
                return null;
            }
        }
        /// <summary>
        /// Die Liste aller Serien
        /// </summary>
        public List<RecordingSeries> Series { get { return series; } set { series = value; Notify(); } }
        /// <summary>
        /// Die ausgewählte Serie
        /// </summary>
        public RecordingSeries Serie { get { return serie; } set { serie = value; Notify(); } }
        /// <summary>
        /// Eine Liste der Sender
        /// </summary>
        public List<RecordingChannel> Channels { get { return channels; } set { channels = value; Notify(); } }
        /// <summary>
        /// Der ausgewählte Sender
        /// </summary>
        public RecordingChannel Channel { get { return channel; } set { channel = value; Notify(); } }
        /// <summary>
        /// EditMode an oder aus
        /// </summary>
        public bool EditMode { get => editMode; set { editMode = value; Notify(); } }
        /// <summary>
        /// Die Serie in der EditComboBox, die entweder ausgewählt oder selbst hinein geschrieben wurde.
        /// </summary>
        public string SeriesValue { get => seriesValue; set { seriesValue = value; Notify(); } }
        /// <summary>
        /// Die Liste mit Aufnahmen, wenn mehrere ausgewählt sind.
        /// </summary>
        public List<RecordingItem> RecordingItems { get => recordingItems; set { recordingItems = value; Notify(); } }

        //public bool SmoothScrolling { get => !PageEinstellungen.GetInstance().SmoothScrolling ; set { PageEinstellungen.GetInstance().SmoothScrolling = !value; Notify(); } }


        /// <summary>
        /// Erzeugt eine Instanz dieser Seite
        /// </summary>
        public PageAufnahmen()
        {
            InitializeComponent();
            //Instanz speichern
            pageAufnahmen = this;
            //Datenkontext festlegen
            DataContext = this;
            //Server Api anzapfen
            serverApi = DVBViewerServerApi.GetCurrentInstance();

        }

        internal static PageAufnahmen GetInstance()
        {
            return pageAufnahmen;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Alle Aufnahmen holen
            //Aufnahmen sortieren Absteigend nach Datum
            Recordings = SortRecordings(await serverApi.RecordingsAsync);
            //oder so abrufen
            //Recordings = RecordingList.GetRecordings();

            //Clients abholen
            Clients = await serverApi.DVBViewerClientsAsync;
            //Wenn welche vorhanden sind, den ersten nehmen
            if (Clients.Items.Count > 0)
                Client = Clients.Items[0];

            //Serien abrufen
            Series = await RecordingSeries.GetSeriesAsync();
            //Sender abfrufen
            Channels = await RecordingChannel.GetChannelsAsync();
        }
        /// <summary>
        /// Ereignis auslösen damit die UI Oberfläche aktualisiert wird.
        /// </summary>
        /// <param name="argument"></param>
        public void Notify([CallerMemberName] string argument = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argument));
        }

        /// <summary>
        /// Die Aufnahmen sortieren. Das stimmt nicht immer 1:1 mit dem DataGrid überein!
        /// </summary>
        /// <param name="recordingList"></param>
        /// <returns></returns>
        private RecordingList SortRecordings(RecordingList recordingList)
        {
            if (!string.IsNullOrEmpty(sortingColumn) && (sortingColumnDirection == ListSortDirection.Ascending || sortingColumnDirection == ListSortDirection.Descending))
            {
                if (sortingColumn == Properties.Resources.Title)
                {
                    if (sortingColumnDirection == ListSortDirection.Ascending)
                        recordingList.Items = recordingList.Items.OrderBy(x => x.Title).ToList();
                    else
                        recordingList.Items = recordingList.Items.OrderByDescending(x => x.Title).ToList();
                    return recordingList;
                }
                else if (sortingColumn == Properties.Resources.Series)
                {
                    if (sortingColumnDirection == ListSortDirection.Ascending)
                        recordingList.Items = recordingList.Items.OrderBy(x => x.Series?.Name).ToList();
                    else
                        recordingList.Items = recordingList.Items.OrderByDescending(x => x.Series?.Name).ToList();
                    return recordingList;
                }
                else if (sortingColumn == Properties.Resources.Duration)
                {
                    if (sortingColumnDirection == ListSortDirection.Ascending)
                        recordingList.Items = recordingList.Items.OrderBy(x => x.Duration2).ToList();
                    else
                        recordingList.Items = recordingList.Items.OrderByDescending(x => x.Duration2).ToList();
                    return recordingList;
                }
                else if (sortingColumn == Properties.Resources.Recordingdate)
                {
                    if (sortingColumnDirection == ListSortDirection.Ascending)
                        recordingList.Items = recordingList.Items.OrderBy(x => x.RecDate).ToList();
                    else
                        recordingList.Items = recordingList.Items.OrderByDescending(x => x.RecDate).ToList();
                    return recordingList;
                }
                else if (sortingColumn == Properties.Resources.Channel)
                {
                    if (sortingColumnDirection == ListSortDirection.Ascending)
                        recordingList.Items = recordingList.Items.OrderBy(x => x.Channel?.Name).ToList();
                    else
                        recordingList.Items = recordingList.Items.OrderByDescending(x => x.Channel?.Name).ToList();
                    return recordingList;
                }
                else if (sortingColumn == Properties.Resources.Info)
                {
                    if (sortingColumnDirection == ListSortDirection.Ascending)
                        recordingList.Items = recordingList.Items.OrderBy(x => x.Info).ToList();
                    else
                        recordingList.Items = recordingList.Items.OrderByDescending(x => x.Info).ToList();
                    return recordingList;
                }
                else
                {
                }
            }
            return recordingList;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            //Die Aufnahme auf dem DVBViewer Client abspielen
            RecordingItem?.PlayAsync(Client);
        }

        private void BtnVideoplayer_Click(object sender, RoutedEventArgs e)
        {
            if (RecordingItem != null)
            {
                try
                {
                    //Eine m3u Datei von der Aufnahme erzeugen und dem System übergeben.
                    Process.Start(RecordingItem.CreateM3UFile());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnVideoplayerList_Click(object sender, RoutedEventArgs e)
        {
            if (Recordings != null)
            {
                //Eine m3u über alle Aufnahmen erzeugen und dem System übergeben.
                Process.Start(Recordings.CreateM3UFile());
            }
        }

        private async void BtnSearchTitle_Click(object sender, RoutedEventArgs e)
        {
            //Suche nach Titel
            if (!string.IsNullOrEmpty(SearchTitle))
            {
                Recordings = SortRecordings(await RecordingList.GetRecordingsAsync(searchTitle).ConfigureAwait(false));
            }
        }

        private async void BtnSearchDesc_Click(object sender, RoutedEventArgs e)
        {
            //Suche in der Beschreibung
            if (!string.IsNullOrEmpty(SearchDesc))
            {
                Recordings = SortRecordings(await RecordingList.GetRecordingsByDescAsync(searchDesc).ConfigureAwait(false));
            }
        }

        private async void BtnAlleAufnahmen_Click(object sender, RoutedEventArgs e)
        {
            //ALle Aufnahmen anzeigen
            if (Recordings != null)
            {
                Recordings = SortRecordings(await RecordingList.GetRecordingsAsync().ConfigureAwait(false));
                Series = await RecordingSeries.GetSeriesAsync().ConfigureAwait(false);
            }
        }

        private async void CbSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Aufnahmen einer Serie anzeigen
            if (recordings != null)
            {
                var recs = await serverApi.RecordingsAsync;
                Recordings.Items = (from f in recs.Items where f.Series?.Name.Equals(Serie?.Name) == true select f).ToList();
                Recordings = SortRecordings(Recordings);
                Notify("Recordings");
            }
        }

        private async void CbSender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Aufnahmen eines Senders anzeigen
            if (recordings != null)
            {
                var recs = await serverApi.RecordingsAsync;
                Recordings.Items = (from f in recs.Items where f.Channel?.Name?.Equals(channel?.Name) == true select f).ToList();
                Recordings = SortRecordings(Recordings);
                Notify("Recordings");
            }
        }

        private async void BtnMoveToSeries_Click(object sender, RoutedEventArgs e)
        {
            //verschiebt die markierten Aufnahmen zur ausgewählten Serie
            if (RecordingItems?.Count > 0 && EditMode)
            {
                //var result = MessageBox.Show($"Dies ändert die Serie von {RecordingItems.Count} Aufnahmen zur Serie \"{SeriesValue}\".\nMöchtest du diese Änderung durchführen?", "Sicherheitsabfrage", MessageBoxButton.YesNo, MessageBoxImage.Question);
                var result = MessageBox.Show($"{Properties.Resources.ChangesSeriesNextNumber} {RecordingItems.Count} {Properties.Resources.ChangesSeriesFromNumberToName} \"{SeriesValue}\".\n{Properties.Resources.YouWantThisUpdate}", Properties.Resources.CheckDialogHeader, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    //Für jede Aufnahme in der Liste
                    foreach (var item in RecordingItems)
                    {
                        //Wenn die Serie nicht existieren sollte
                        if (item.Series == null)
                        {
                            //Neue Serie
                            item.Series = new RecordingSeries { Name = SeriesValue };
                        }
                        else
                        {
                            //Alte überschreiben
                            item.Series.Name = SeriesValue;
                        }

                        //Update durchführen
                        var res = await item.UpdateAsync();

                        //Sollte bei einer Aufnahme etwas schieflaufen
                        if (res != System.Net.HttpStatusCode.OK)
                        {
                            //MessageBox.Show($"{item.Title} konnte nicht geändert werden.\nDer Rückgabewert des Servers war: {res.ToString()}", "Fehler beim ändern der Serie", MessageBoxButton.OK, MessageBoxImage.Error);
                            MessageBox.Show($"{item.Title} {Properties.Resources.ChangeASerieFromTheTitle}\n{Properties.Resources.MessageFromServer} {res.ToString()}", Properties.Resources.ErrorUpdateSeries, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    Series = await RecordingSeries.GetSeriesAsync().ConfigureAwait(false);
                    //Die Sortierung wird nicht übernommen
                    Recordings = SortRecordings(await serverApi.RecordingsAsync.ConfigureAwait(false));
                }
            }
        }

        private async void BtnDeleteRecording_Click(object sender, RoutedEventArgs e)
        {
            //Die ausgewählten Aufnahmen löschen
            if (RecordingItems?.Count > 0)
            {
                var result = MessageBox.Show($"{Properties.Resources.ThisDeletes} {RecordingItems.Count} {Properties.Resources.Recordings}.\n{Properties.Resources.DeleteRecordingsQuestion}", Properties.Resources.DeleteDialogHeader, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    string message = "";
                    foreach (var item in RecordingItems)
                    {
                        var res = await item.DeleteAsync();
                        if (res.ToString().Equals("423"))
                        {
                            message += $"\n\"{item.Title}\" {Properties.Resources.CouldNotDelete}. {Properties.Resources.ServerLocksTheRecording}";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        MessageBox.Show(message, Properties.Resources.DeleteDialogHeader, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    Series = await RecordingSeries.GetSeriesAsync().ConfigureAwait(false);
                    //Die Sortierung wird nicht übernommen
                    Recordings = SortRecordings(await serverApi.RecordingsAsync.ConfigureAwait(false));
                }
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine aktive Zelle das 2. mal angeklickt wird.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Im EditMode nicht abwählen
            if (!EditMode)
            {
                //Auswahl löschen
                dataGrid.SelectedIndex = -1;
            }
        }

        private void DataGrid_LayoutUpdated(object sender, EventArgs e)
        {
            //Anzahl der Aufnahmen übernehmen
            AnzahlAufnahmen = Recordings?.Items?.Count ?? 0;
            //Die Sortierung übernehmen
            //Recordings.Items = dataGrid.Items.Cast<RecordingItem>().ToList();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Die ausgewählten Zeilen aus der Liste
            RecordingItems = dataGrid.SelectedItems.OfType<RecordingItem>().ToList();
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            //Zu sortierende Spalte
            sortingColumn = e.Column.Header.ToString();
            if (!e.Column.SortDirection.HasValue)
            {
                //Sortierung absteigend
                sortingColumnDirection = ListSortDirection.Ascending;
            }
            else
            {
                //Sortierung umkehren
                sortingColumnDirection = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
            }
        }

        private void ChkEditMode_Checked(object sender, RoutedEventArgs e)
        {
            //Wenn der Editmode an ist, dann werden keine Details mehr angezeigt
            dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            //Und es können mehrere Zeilen ausgewählt werden
            dataGrid.SelectionMode = DataGridSelectionMode.Extended;
        }

        private void ChkEditMode_Unchecked(object sender, RoutedEventArgs e)
        {
            //Alle Items abwählen
            dataGrid.SelectedIndex = -1;
            //Details werden wieder angezeigt
            dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            //Es kann nur noch eine Zeile angezeigt werden
            dataGrid.SelectionMode = DataGridSelectionMode.Single;
        }

        private async void DataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EditMode && RecordingItem != null)
            {
                var editWindow = new WindowAufnahmeEdit(RecordingItem)
                {
                    Owner = Window.GetWindow(this)
                };
                bool? res = editWindow.ShowDialog();
                if (res == true)
                {
                    MessageBox.Show(Properties.Resources.UpdateSucceed);
                    Series = await RecordingSeries.GetSeriesAsync().ConfigureAwait(false);
                    //Die Sortierung wird nicht übernommen
                    Recordings = SortRecordings(await serverApi.RecordingsAsync.ConfigureAwait(false));
                }
                else if (res == false && !string.IsNullOrEmpty(editWindow.GetErrorCode()))
                {
                    MessageBox.Show($"{Properties.Resources.UpdateFailedWithCode} {editWindow.GetErrorCode()}");
                }
            }
        }
    }
}
