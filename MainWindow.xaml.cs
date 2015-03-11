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
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            //disable Control                     
            pbLoading.Value = 0;
            pbLoading.Visibility = Visibility.Visible;
            tabControl.IsEnabled = false;
            btnImport.IsEnabled = false;
            txtNickname.IsEnabled = false;
            txtUsername.IsEnabled = false;

            //TODO : Use configuration file to keep last user  
            BGGPlays.CurrentPlayerUsername = txtUsername.Text;
            BGGPlays.CurrentPlayerNickname = txtNickname.Text;
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);            

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Enable controls
            //TODO : To be adapted in case of error (currently this code is never reached)
            pbLoading.Visibility = Visibility.Hidden;
            tabControl.IsEnabled = true;
            btnImport.IsEnabled = true;
            txtNickname.IsEnabled = true;
            txtUsername.IsEnabled = true;

            cboYear.ItemsSource = BGGPlays.Years;
            cboYear.SelectedIndex = 0;

            lblTotalPlays.Content = BGGPlays.TotalPlays;
            lstGames.ItemsSource = BGGPlays.AllPlaysByYear;
            dgLocations.ItemsSource = BGGPlays.LocationCounts.OrderBy(l => l.Key);
            lblDistinctLocations.Content = BGGPlays.LocationCounts.Count;
            dgGames.ItemsSource = BGGPlays.GameCounts.OrderBy(l => l.Key);
            lblDistinctGames.Content = BGGPlays.GameCounts.Count;
            lblHIndex.Content = BGGPlays.GetHIndex();

            //Calculate all Stats
            //TODO : Static class or singleton 
            calcStats = new CalculateStats(BGGPlays);
            lstPlayers.ItemsSource = calcStats.Stats.Select(s => s.Player.Nickname);
            Resources["Stats"] = calcStats.Stats;
        }

        private void UpdateDataByYear()
        {
            //TODO : Awful, should be done by using "NotifyPropertiesChanged"
            lblTotalPlays.Content = BGGPlays.TotalPlays;
            lstGames.ItemsSource = BGGPlays.AllPlaysByYear;
            dgLocations.ItemsSource = BGGPlays.LocationCounts.OrderBy(l => l.Key);
            lblDistinctLocations.Content = BGGPlays.LocationCounts.Count;
            dgGames.ItemsSource = BGGPlays.GameCounts.OrderBy(l => l.Key);
            lblDistinctGames.Content = BGGPlays.GameCounts.Count;
            lblHIndex.Content = BGGPlays.GetHIndex();

            //Calculate all Stats
            //TODO : Static class or singleton 
            calcStats = new CalculateStats(BGGPlays);
            lstPlayers.ItemsSource = calcStats.Stats.Select(s => s.Player.Nickname);
            Resources["Stats"] = calcStats.Stats;

            //Charts
            chartAllGames.ItemsSource = BGGPlays.GetGamesByDateRange(Plays.DateRange.YEAR);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbLoading.Value = e.ProgressPercentage;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BGGPlays.ResetPlays();

            //Call Webservice from BGG to retrieve plays
            WebClient serviceRequest = new WebClient();
            serviceRequest.Encoding = System.Text.Encoding.UTF8;
            XmlDocument xmlDoc = new XmlDocument();
            int counter = 1;
            int totalPlaysXml = 0;

            //TODO Add String to resource file
            string response = serviceRequest.DownloadString(new Uri(String.Format("http://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}", BGGPlays.CurrentPlayerUsername, counter)));
            while (response.Contains("<play id"))
            {               
                xmlDoc.LoadXml(response);
                //Update Progress Bar
                if(totalPlaysXml == 0)
                        totalPlaysXml = Int32.Parse(xmlDoc.SelectSingleNode("plays").Attributes["total"].InnerText);
                
                (sender as BackgroundWorker).ReportProgress(Math.Min(Convert.ToInt32(counter*100.0/totalPlaysXml*100.0),100));

                BGGPlays.LoadPlays(xmlDoc);

                counter++;
                response = serviceRequest.DownloadString(new Uri(String.Format("http://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}", BGGPlays.CurrentPlayerUsername, counter)));
            }
            //TODO : TO be refactored, shouldn't be there. It should be calculated automatically
            BGGPlays.PopulateYears();
        }

        private void cboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BGGPlays.FilterByYear(cboYear.SelectedItem.ToString());
            UpdateDataByYear();
        } 

        private void HyperLinkBehavior(RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }

        //TAB Details
        private void lstGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstGames.SelectedItem != null)
            Resources["Results"] = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)lstGames.SelectedItem).Id).Result.OrderBy(p => p.Rating);
        }


        //TAB Players
        private void lstPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPlayers.SelectedItem != null)
                lstPlayerGames.ItemsSource = BGGPlays.AllPlaysByYear.Where(p => p.Result.Exists( n => n.Player.Nickname == (string)lstPlayers.SelectedItem));
        }

        private void lstPlayerGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstPlayerGames.SelectedItem != null)
                Resources["PlayerResults"] = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)lstPlayerGames.SelectedItem).Id).Result.OrderBy(p => p.Rating);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstStatsPlayers.SelectedItem != null && ((Stats)lstStatsPlayers.SelectedItem).Player != null)
                chartPositionRating.ItemsSource = calcStats.Stats.Single(s => s.Player.Nickname == (((Stats)lstStatsPlayers.SelectedItem)).Player.Nickname).PositionRating.OrderBy(s => s.Key);
        }

        private void lstPlayerGames_Click(object sender, RoutedEventArgs e)
        {
            HyperLinkBehavior(e);
        }

        //TAB Locations
        //TODO : Refactor, duplicate useless code
        //TODO : Update variables names...
        private void dgLocationGames_Click(object sender, RoutedEventArgs e)
        {
            HyperLinkBehavior(e);
        }              

        private void dgLocationGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLocationGames.SelectedItem != null)
                dgLocationSelectedGame.ItemsSource = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)dgLocationGames.SelectedItem).Id).Result.OrderBy(p => p.Rating);
        }

        private void dgLocations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLocations.SelectedItem != null)
                dgLocationGames.ItemsSource = BGGPlays.AllPlaysByYear.Where(p => p.Location.Equals(((KeyValuePair<string, int>)dgLocations.SelectedItem).Key, StringComparison.CurrentCultureIgnoreCase));
        }


        //TAB Games
        //TODO : Refactor, duplicate useless code
        //TODO : Update variables names...
        private void dgGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgGames.SelectedItem != null)
                dgByGamesGame.ItemsSource = BGGPlays.AllPlaysByYear.Where(p => p.Game.Equals(((KeyValuePair<string, int>)dgGames.SelectedItem).Key, StringComparison.CurrentCultureIgnoreCase));
        }

        private void dgByGamesGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgByGamesGame.SelectedItem != null)
                dgByGameSelectedGame.ItemsSource = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)dgByGamesGame.SelectedItem).Id).Result.OrderBy(p => p.Rating);
        }


        private void dgByGamesGame_Click(object sender, RoutedEventArgs e)
        {
            HyperLinkBehavior(e);
        }        
    }
}
