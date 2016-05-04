using BGGStats.Model;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public SeriesCollection Series { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Series = new SeriesCollection();
            DataContext = this;
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
            cboChartRange.IsEnabled = false;
            cboYear.IsEnabled = false;

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
            cboChartRange.IsEnabled = true;
            cboYear.IsEnabled = true;

            cboYear.ItemsSource = BGGPlays.Years;
            cboYear.SelectedIndex = 0;

            cboChartRange.ItemsSource = Enum.GetValues(typeof(Plays.DateRange));

            if (cboYear.SelectedItem == Plays.ALL_YEARS)
                cboChartRange.SelectedItem = Plays.DateRange.Year;
            else
                cboChartRange.SelectedItem = Plays.DateRange.Month;

            UpdateDataByYear();

            //lblTotalPlays.Content = BGGPlays.TotalPlays;
            //lstGames.ItemsSource = BGGPlays.AllPlaysByYear;
            //dgLocations.ItemsSource = BGGPlays.LocationCounts.OrderBy(l => l.Key);
            //lblDistinctLocations.Content = BGGPlays.LocationCounts.Count;
            //dgGames.ItemsSource = BGGPlays.GameCounts.OrderBy(l => l.Key);
            //lblDistinctGames.Content = BGGPlays.GameCounts.Count;
            //lblHIndex.Content = BGGPlays.GetHIndex();

            ////Calculate all Stats
            ////TODO : Static class or singleton 
            //calcStats = new CalculateStats(BGGPlays);
            //lstPlayers.ItemsSource = calcStats.Stats;
            //Resources["Stats"] = calcStats.Stats;
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
            lstPlayers.ItemsSource = calcStats.Stats;
            Resources["Stats"] = calcStats.Stats;

            //Charts
            //chartAllGames.ItemsSource = BGGPlays.GetGamesByDateRange(Plays.DateRange.Year);
        }

        private void UpdateMonthsYearsChart()
        {
            if (cboChartRange.SelectedItem != null & cboYear.SelectedItem != null)
            {
                var selectedYear = (string)cboYear.SelectedItem;
                var selectChart = ((Plays.DateRange)cboChartRange.SelectedItem);
                var playedYears = BGGPlays.GetGamesByDateRange(selectChart, selectedYear);
                Series.Clear();
                ChartValues<double> values = new ChartValues<double>();

                chByYear.AxisX.Clear();


                //For Months
                if (selectChart == Plays.DateRange.Month)
                {
                    chByYear.AxisX.Add(new Axis { Title = "Mois", Labels = new List<string>() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" } });

                    foreach (var playedYear in playedYears)
                    {
                        values.Add(playedYear.Value);
                    }
                    Series.Add(new BarSeries { Title = selectedYear, Values = values });

                }
                //For Years
                else
                {
                    chByYear.AxisX.Add(new Axis { Title = "Année" });

                    foreach (var playedYear in playedYears)
                    {
                        Series.Add(new BarSeries { Title = playedYear.Key, Values = new ChartValues<double> { playedYear.Value } });
                    }
                }
            }
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

            if (cboYear.SelectedItem.ToString() != Plays.ALL_YEARS)
                cboChartRange.SelectedItem = Plays.DateRange.Month;

            UpdateDataByYear();
            UpdateMonthsYearsChart();
        }

        private void cboChartRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMonthsYearsChart();
        }

        private void HyperLinkBehavior(RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }

        //TAB All Games
        private void lstGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstGames.SelectedItem != null)
                Resources["Results"] = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)lstGames.SelectedItem).Id).Result.OrderBy(p => p.Rating);
            
            txtCommentAllGAmes.Text = (lstGames.SelectedItem == null) ? String.Empty : txtCommentAllGAmes.Text = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)lstGames.SelectedItem).Id).Comments;                            
        }

        //TAB Players
        private void lstPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPlayers.SelectedItem != null)
            {
                string currentPlayer = ((Stats)lstPlayers.SelectedItem).Player.Nickname;
                BGGPlays.AllPlaysByYear.Select(p => {p.CurrentUser = currentPlayer; return p;}).ToList();
                lstPlayerGames.ItemsSource = BGGPlays.AllPlaysByYear.Where(p => p.Result.Exists(n => n.Player.Nickname == currentPlayer));
            }                
        }

        private void lstPlayerGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstPlayerGames.SelectedItem != null)
                Resources["PlayerResults"] = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)lstPlayerGames.SelectedItem).Id).Result.OrderBy(p => p.Rating);

            txtCommentByPlayer.Text = (lstPlayerGames.SelectedItem == null) ? String.Empty : txtCommentByPlayer.Text = BGGPlays.AllPlaysByYear.Single(p => p.Id == ((Play)lstPlayerGames.SelectedItem).Id).Comments;                            
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
