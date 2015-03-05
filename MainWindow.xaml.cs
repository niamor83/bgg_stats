using BGGStats.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Xml;
using System.Xml.Serialization;

namespace BGGStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Plays BGGPlays = new Plays();
        CalculateStats calcStats;

        public MainWindow()
        {
            InitializeComponent();

            //TODO : has to be configured automatically
            BGGPlays.CurrentPlayerUsername = "niamor";
            BGGPlays.CurrentPlayerNickname = "Romain";
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            BGGPlays.ResetPlays();

            //Call Webservice from BGG to retrieve plays
            WebClient serviceRequest = new WebClient();
            serviceRequest.Encoding = System.Text.Encoding.UTF8;
            XmlDocument xmlDoc = new XmlDocument();
            int counter = 1;

            //TODO Add String to resource file
            string response = serviceRequest.DownloadString(new Uri(String.Format("http://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}", BGGPlays.CurrentPlayerUsername, counter)));
            while (response.Contains("<play id"))
            {
                xmlDoc.LoadXml(response);

                BGGPlays.LoadPlays(xmlDoc);

                counter++;
                response = serviceRequest.DownloadString(new Uri(String.Format("http://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}", BGGPlays.CurrentPlayerUsername, counter)));
            }

            lblTotalPlays.Content = BGGPlays.TotalPlays;

            lstGames.ItemsSource = BGGPlays.AllPlays;

            lstGames.DisplayMemberPath = "Game";

            //Calculate all Stats
            //TODO : Static class or singleton 
            calcStats = new CalculateStats(BGGPlays);
            lstPlayers.ItemsSource = calcStats.Stats.Select( s => s.Player.Nickname);
            //lstPlayers.DisplayMemberPath = "Player.Nickname";
            Resources["Stats"] = calcStats.Stats;


        }

        //TAB Details
        private void lstGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Resources["Results"] = BGGPlays.AllPlays.Single(p => p.Id == ((Play)lstGames.SelectedItem).Id).Result.OrderBy(p => p.Rating);
        }


        //TAB Players
        private void lstPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstPlayerGames.ItemsSource = BGGPlays.AllPlays.Where(p => p.Result.Exists( n => n.Player.Nickname == (string)lstPlayers.SelectedItem));
        }

        private void lstPlayerGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstPlayerGames.SelectedItem != null)
            Resources["PlayerResults"] = BGGPlays.AllPlays.Single(p => p.Id == ((Play)lstPlayerGames.SelectedItem).Id).Result.OrderBy(p => p.Rating);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(((Stats)lstStatsPlayers.SelectedItem).Player != null)
                chartPositionRating.ItemsSource = calcStats.Stats.Single(s => s.Player.Nickname == (((Stats)lstStatsPlayers.SelectedItem)).Player.Nickname).PositionRating.OrderBy(s => s.Key);
        }

        private void lstPlayerGames_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }
    }
}
