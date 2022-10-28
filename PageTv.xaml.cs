using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DVBViewerServerApiWrapper;
using DVBViewerServerApiWrapper.Model;

namespace DMSApiWrapperDemo
{
    /// <summary>
    /// Interaktionslogik für PageTv.xaml
    /// </summary>
    public partial class PageTv : Page, INotifyPropertyChanged
    {
        private static PageTv pageTv;
        private DVBViewerServerApi serverApi;

        private ChannelList channelList;
        private ChannelItem selectedChannelItem;
        private string channelLogo;
        private string channelURL;
        private Visibility mediaPlayerVisibility;

        public event PropertyChangedEventHandler PropertyChanged;

        public ChannelList ChannelList { get => channelList; set { channelList = value; Notify(); } }

        public ChannelItem SelectedChannelItem { get => selectedChannelItem; set { selectedChannelItem = value; Notify(); } }

        public string ChannelLogo { get => channelLogo; set { channelLogo = value; Notify(); } }

        public string ChannelURL { get => channelURL; set { channelURL = value; Notify(); } }

        public Visibility MediaPlayerVisibility { get => mediaPlayerVisibility; set { mediaPlayerVisibility = value; Notify(); } }

        public PageTv()
        {
            InitializeComponent();
            pageTv = this;
            DataContext = this;
        }

        public static PageTv GetInstance()
        {
            return pageTv;
        }

        /// <summary>
        /// Ereignis auslösen damit die UI Oberfläche aktualisiert wird.
        /// </summary>
        /// <param name="argument"></param>
        public void Notify([CallerMemberName] string argument = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(argument));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            serverApi = DVBViewerServerApi.GetCurrentInstance();
            //ChannelList = await ChannelList.GetChannelListAsync("ard", false).ConfigureAwait(false);
            ChannelList = await ChannelList.GetChannelListAsync().ConfigureAwait(false);
        }

        private async void TvSender_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue.GetType().Equals(typeof(ChannelItem)))
            {
                SelectedChannelItem = (e.NewValue as ChannelItem);
                if (SelectedChannelItem != null)
                {
                    ChannelLogo = await SelectedChannelItem.ChannelLogoURLAsync.ConfigureAwait(false);
                }

                ChannelURL = selectedChannelItem.GetUPnPUriString();
                //ChannelURL = selectedChannelItem.GetRTSPUriString(); //Dont work

                var resultBetween = mediaPlayer.Open(new Uri(ChannelURL));

                ChannelURL = "Attempts to open...";
                MediaPlayerVisibility = Visibility.Collapsed;
                //TODO: Hier könnte man noch die VideoZeile in der Größe runternehmen, die Schrift vergrößern
                //Oder man könnte in Vollbild switchen
                //Oder man blendet das ChannelLogo groß ein
                //Oder man 

                var result = await resultBetween;
                if (!result)
                {
                    ChannelURL = $"Can't load Channel {selectedChannelItem.Name}, maybe used...";
                }
                else
                {
                    MediaPlayerVisibility = Visibility.Visible;
                    ChannelURL = selectedChannelItem.GetUPnPUriString();
                }

                //Process.Start(selectedChannelItem.CreateM3UFile());
            }
        }
    }
}
