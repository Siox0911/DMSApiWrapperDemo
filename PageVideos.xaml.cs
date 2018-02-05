﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private int anzahlVideos;
        //Long zum Berechnen der Videogröße gesamt
        private long fileSizeVideos;
        //String zum Anzeigen der Videos gesamt
        private string groesseVideos;
        //Text der Titelsuche
        private string searchTitle;
        //Text der Pfadsuche
        private string searchPath;
        //Alle Videos
        private VideoFileList videos;
        //Das ausgewählte Video
        private VideoFileItem videoFileItem;
        //Der ApiWrapper
        private DVBViewerServerApi serverApi;
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
        public int AnzahlVideos { get => anzahlVideos; set { anzahlVideos = value; Notify(); } }
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
        public string GroesseVideos { get => groesseVideos; set { groesseVideos = value; Notify(); } }

        public PageVideos()
        {
            InitializeComponent();
            pageVideos = this;
            //Datenkontext festlegen
            DataContext = this;
            //Server Api anzapfen
            serverApi = DVBViewerServerApi.GetCurrentInstance();
            //Clients abholen
            Clients = serverApi.DVBViewerClients;
            //Wenn welche vorhanden sind, den ersten nehmen
            if (Clients.Items?.Count > 0)
                Client = Clients.Items[0];

            //Alle Videos abrufen
            Videos = serverApi.VideoFileList;
        }

        internal static PageVideos GetInstance()
        {
            return pageVideos;
        }
        /// <summary>
        /// Ereignis auslösen damit die UI Oberfläche aktualisiert wird.
        /// </summary>
        /// <param name="argument"></param>
        public void Notify([CallerMemberName] string argument = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argument));
        }


        private void BtnSearchPath_Click(object sender, RoutedEventArgs e)
        {
            //Im PFad suchen
            if (!string.IsNullOrEmpty(SearchPath))
                Videos = VideoFileList.GetVideoFileListByPath(SearchPath);
        }

        private void BtnAlleVideos_Click(object sender, RoutedEventArgs e)
        {
            //Alle Videos vom Server holen
            Videos = serverApi.VideoFileList;
        }

        private void BtnSearchTitle_Click(object sender, RoutedEventArgs e)
        {
            //Titelsuche
            if (string.IsNullOrEmpty(SearchTitle))
                Videos = VideoFileList.GetVideoFileList(SearchTitle);
        }

        private void BtnVideoplayer_Click(object sender, RoutedEventArgs e)
        {
            if (VideoFileItem != null)
            {
                //Eine m3u Datei von dem ausgewählten Video erzeugen und dem System übergeben.
                Process.Start(VideoFileItem.CreateM3UFile());
            }
        }

        private void BtnVideoplayerList_Click(object sender, RoutedEventArgs e)
        {
            if (Videos != null)
            {
                //Eine m3u Datei von den Videos aus der Liste erzeugen und dem System übergeben.
                Process.Start(Videos.CreateM3UFile());
            }
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
                AnzahlVideos = Videos.Items.Count;

                //Dateigröße gesamt
                fileSizeVideos = 0;
                foreach (var item in videos.Items)
                {
                    fileSizeVideos += item.FileSize;
                }
                GroesseVideos = FileSize.GetFileSize(fileSizeVideos).ToString();

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
