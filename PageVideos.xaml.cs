using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DVBViewerServerApiWrapper;
using DVBViewerServerApiWrapper.Helper;
using DVBViewerServerApiWrapper.Model;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für PageVideos.xaml
    /// </summary>
    public partial class PageVideos : Page, INotifyPropertyChanged
    {
        //Clients, Client, AnzahlVideos, SearchTitle, SearchPath, Videos, VideoFileItem
        private DVBViewerClients clients;
        //Der ausgewählte DVBViewer Client
        private DVBViewerClient client;
        //Anzahl der Videos
        private int numberOfVideos;
        //Long zum Berechnen der Videogröße gesamt
        private long fileSizeVideos;
        //String zum Anzeigen der Videos gesamt
        private string sizeOfVideos;
        //Text der Titelsuche
        private string searchTitle;
        //Text der Pfadsuche
        private string searchPath;
        //Alle Videos
        private VideoFileList videos;
        //Das ausgewählte Video
        private VideoFileItem videoFileItem;
        //Der ApiWrapper
        private readonly DVBViewerServerApi serverApi;
        //Diese Seite selbst
        private static PageVideos pageVideos;

        //Eventhandler
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Die DVBViewer Clients
        /// </summary>
        public DVBViewerClients Clients
        {
            get => clients; set
            {
                clients = value;
                Notify();
            }
        }

        /// <summary>
        /// Der ausgewählte DVBViewer Client
        /// </summary>
        public DVBViewerClient Client { get => client; set { client = value; Notify(); } }

        /// <summary>
        /// Die Anzahl der Videos
        /// </summary>
        public int NumberOfVideos { get => numberOfVideos; set { numberOfVideos = value; Notify(); } }

        /// <summary>
        /// Der Text der Titelsuche
        /// </summary>
        public string SearchTitle { get => searchTitle; set { searchTitle = value; Notify(); } }

        /// <summary>
        /// Der Text der Pfadsuche
        /// </summary>
        public string SearchPath { get => searchPath; set { searchPath = value; Notify(); } }

        /// <summary>
        /// Die Videoliste, egal ob komplett oder gefiltert
        /// </summary>
        public VideoFileList Videos { get => videos; set { videos = value; Notify(); } }

        /// <summary>
        /// Das ausgewählte Video
        /// </summary>
        public VideoFileItem VideoFileItem { get => videoFileItem; set { videoFileItem = value; Notify(); } }

        /// <summary>
        /// Die Gesamtgröße der Videoliste
        /// </summary>
        public string SizeOfVideos { get => sizeOfVideos; set { sizeOfVideos = value; Notify(); } }

        public PageVideos()
        {
            InitializeComponent();
            pageVideos = this;
            //Datenkontext festlegen
            DataContext = this;
            //Server Api anzapfen
            serverApi = DVBViewerServerApi.GetCurrentInstance();
        }

        internal static PageVideos GetInstance()
        {
            return pageVideos;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Clients abholen
            Clients = await serverApi.DVBViewerClientsAsync;
            //Wenn welche vorhanden sind, den ersten nehmen
            if (Clients.Items?.Count > 0)
                Client = Clients.Items[0];

            //Alle Videos abrufen
            Videos = await serverApi.VideoFileListAsync;
        }

        /// <summary>
        /// Ereignis auslösen damit die UI Oberfläche aktualisiert wird.
        /// </summary>
        /// <param name="argument"></param>
        public void Notify([CallerMemberName] string argument = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argument));
        }

        private async void BtnSearchPath_Click(object sender, RoutedEventArgs e)
        {
            //Im PFad suchen
            if (!string.IsNullOrEmpty(SearchPath))
            {
                Videos = await VideoFileList.GetVideoFileListByPathAsync(SearchPath).ConfigureAwait(false);
            }
        }

        private async void BtnAlleVideos_Click(object sender, RoutedEventArgs e)
        {
            //Alle Videos vom Server holen
            Videos = await serverApi.VideoFileListAsync;
        }

        private async void BtnSearchTitle_Click(object sender, RoutedEventArgs e)
        {
            //Titelsuche
            if (!string.IsNullOrEmpty(SearchTitle))
            {
                Videos = await VideoFileList.GetVideoFileListAsync(SearchTitle).ConfigureAwait(false);
            }
        }

        private ICommand btnVideoPlayerClick;
        public ICommand BtnVideoPlayerClick
        {
            get
            {
                if (btnVideoPlayerClick == null)
                {
                    btnVideoPlayerClick = new RelayCommand(BtnVideoplayer_Click);
                }
                return btnVideoPlayerClick;
            }
        }

        private void BtnVideoplayer_Click()
        {
            BtnVideoplayer_Click(null, null);
        }

        private void BtnVideoplayer_Click(object sender, RoutedEventArgs e)
        {
            if (VideoFileItem != null)
            {
                //Eine m3u Datei von dem ausgewählten Video erzeugen und dem System übergeben.
                try
                {
                    Process.Start(VideoFileItem.CreateM3UFile());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private ICommand btnVideoPlayerListClick;
        public ICommand BtnVideoPlayerListClick
        {
            get
            {
                if (btnVideoPlayerListClick == null)
                {
                    btnVideoPlayerListClick = new RelayCommand(BtnVideoplayerList_Click);
                }
                return btnVideoPlayerListClick;
            }
        }

        private void BtnVideoplayerList_Click()
        {
            BtnVideoplayerList_Click(null, null);
        }

        private void BtnVideoplayerList_Click(object sender, RoutedEventArgs e)
        {
            if (Videos != null)
            {
                //Eine m3u Datei von den Videos aus der Liste erzeugen und dem System übergeben.
                Process.Start(Videos.CreateM3UFile());
            }
        }

        private ICommand btnPlayClick;
        public ICommand BtnPlayClick
        {
            get
            {
                if (btnPlayClick == null)
                {
                    btnPlayClick = new RelayCommand(BtnPlay_Click);
                }
                return btnPlayClick;
            }
        }

        private void BtnPlay_Click()
        {
            BtnPlay_Click(null, null);
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            //Video auf einem DVBViewer Client abspielen
            if (VideoFileItem != null && Client != null)
                VideoFileItem.Play(Client);
        }

        private void DataGrid_LayoutUpdated(object sender, EventArgs e)
        {
            if (Videos != null)
            {
                //Videoanzahl
                NumberOfVideos = Videos.Items.Count;

                //Dateigröße gesamt
                fileSizeVideos = 0;
                foreach (var item in videos.Items)
                {
                    fileSizeVideos += item.FileSize;
                }
                SizeOfVideos = FileSize.GetFileSize(fileSizeVideos).ToString();

                //Die Sortierung übernehmen
                Videos.Items = dataGrid.Items.Cast<VideoFileItem>().ToList();
            }
        }

        private void DataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Nicht genutzt
        }
    }
}
