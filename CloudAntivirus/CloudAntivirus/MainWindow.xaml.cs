using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using System.Security.Cryptography;//for md5
using System.Net;//for webclient class for url requests
using Newtonsoft.Json;//for JSON file parsing
using Newtonsoft.Json.Linq;//for json parsing
using System.Data.SqlServerCe;//sql compact edition
using System.Data;//for DataSet
using System.Collections.Specialized;//for NameValueCollections
using System.Collections;//for ArrayList
using System.Management;//for WQLEventQuery
using System.Diagnostics;//for Process
using System.Reflection;//for Assembly
using System.Windows.Forms;//for the notify icons
using System.Drawing;//for icon
using SystemMonitor;//for the system monitoring visualization
using System.Windows.Forms.Integration;//for interating form controls int WPF
using System.Media;//for alert sounds


namespace CloudAntivirus
{
    public partial class MainWindow : Window
    {
        public SqlCeConnection connection;
        public SqlCeCommand command;
        public SqlCeDataAdapter adapter;
        public DataSet data;
        public String sql;
        public ManagementEventWatcher eventWatcher;
        public NotifyIcon trayIcon;
        public System.Windows.Forms.ContextMenu trayMenu;
        public SystemData sd;//for the system monitor data
        public Window notifyWindow;//the notification window
        public Timer timer;//timer for notifications
        public int malwareCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.Width = 400;
            this.Height = 380;

            //system tray icon and menu setup
            trayMenu = new System.Windows.Forms.ContextMenu();//create a traymenu
            trayMenu.MenuItems.Add("Show Window", ShowWindow);//add exit item to the menu
            trayMenu.MenuItems.Add("Exit", OnExit);//add exit item to the menu
            trayIcon = new NotifyIcon();//create a tray icon
            trayIcon.Text = "Nephelus";//add text to tray icon
            trayIcon.Icon = (System.Drawing.Icon)Properties.Resources.logo;
            trayIcon.ContextMenu = trayMenu;// Add menu to tray icon and show it.
            trayIcon.Visible = true;
            trayIcon.DoubleClick += ShowWindow;
        
            //timer properties set up for the notification window
            timer = new Timer();
            timer.Tick += new EventHandler(closeNotifyWindow); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = 20000;// Timer will tick evert 10 seconds
            timer.Enabled = true;// Enable the timer
            timer.Stop();

            //file system watching setup
            DriveInfo[] drives = DriveInfo.GetDrives();//get all the drives in the system
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)//if the drive is ready
                {
                    WatchFileSystem(drive.Name, "*.exe"); //start file system watchers for exe
                }
            }

            //process monitoring setup
            MonitorProcesses();

            //database connection setup
            try //open up the local database connection
            {
                // Create a connection to the file datafile.sdf in the program folder
                string dbfile = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + "\\MyDatabase.sdf";
                connection = new SqlCeConnection(@"datasource=C:\Users\Aravindhan\Documents\Visual Studio 2012\Projects\CloudAntivirus\CloudAntivirus\MyDatabase.sdf");
                connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        //this method is called when exit item from traymenu is clicked
        public void OnExit(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            eventWatcher.Stop();//stop the eventWatcher
            connection.Close();//close the database connection
            System.Windows.Application.Current.Shutdown();//shutdown the application
        }

        //this method is called when show window item from traymenu is clicked
        public void ShowWindow(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }


        //when close button is clicked our window should only be minimized and application shud still run in background
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;//hence cancel the closing event
            this.Hide();//hide or minimize the window
        }

        //called when file system creation occurs
        //1. we need to submit the file to cloud for analysis
        //2. add the file to the database with file path, analysis as incomplete,task_id and other fields as null
        public void OnCreated(object sender, FileSystemEventArgs e)
        {
            String filePath = e.FullPath;//get the file path
            int taskId = 0;//the task id of the file
            Console.WriteLine("created " + filePath);//now a file is created
            //1. submit the file to the cuckoo sandbox using cukoo's api server
            taskId = SubmitFileForAnalysis(filePath);
            //2. add it to the local database with file path,task id  and analysis as incomplete
            if (taskId != 0)//if task id is 0 then the file has not been submitted os dont add it to the database
                AddFileToDatabase(filePath, taskId);
        }

        //called when file system deletion occurs
        //when a file is deleted we need to delete its entry from our local database
        public void OnDeleted(object sender, FileSystemEventArgs e)
        {
            String filePath = e.FullPath;//get the file path
            Console.WriteLine("deleted " + filePath);
            try
            {
                sql = "delete from static where path=@path";
                command = new SqlCeCommand(sql, connection);
                command.Parameters.AddWithValue("@path", filePath);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //called when file gets renamed
        //we have to corresponsingly rename the file in our local database
        public void OnRenamed(object sender, RenamedEventArgs e)
        {
            String filePath = e.FullPath;//get the file path
            String oldPath = e.OldFullPath;//get the old file path
            Console.WriteLine("renamed " + oldPath + "to" + filePath);
            try
            {
                sql = "update static set path=@filePath where path=@oldPath";
                command = new SqlCeCommand(sql, connection);
                command.Parameters.AddWithValue("@filePath", filePath);
                command.Parameters.AddWithValue("@oldPath", oldPath);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //this method is called when a new process is started
        //when a file is executed we need to do te following
        //1. check whether we have the file in our database
        //2. if not send it to network cloud for analysis and add it to database(in this case the virus is detected only during 2nd time execution)
        //3. if yes check whether we have its analysis report
        //4. if no analysis report then retrieve the report from the cloud and store it in database
        //5. check whether it is a virus or not(do nothing if it is clean)
        //6. if malware then we shud intimate the user it is a malware, provide him with options to delete the file, quarantine, kill the process
        public void ProcessStartEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            int processId = int.Parse(targetInstance.Properties["ProcessId"].Value.ToString());
            String filePath = "";
            try
            {
                Process p = Process.GetProcessById(processId);
                filePath = targetInstance.Properties["ExecutablePath"].Value.ToString();
                Console.WriteLine("executing " + filePath);
                //1.check whether the file is added to the database already
                SqlCeDataReader dataReader = null;
                sql = "select * from static where path=@path";
                command = new SqlCeCommand(sql, connection);
                command.Parameters.AddWithValue("@path", filePath);
                dataReader = command.ExecuteReader();
                if (dataReader.Read())//if the file is already present then check wehther we have its analysis data
                {
                    if (dataReader["analysis_status"].ToString().Equals("Complete"))//if we have it analysis report
                    {
                        for (int i = 3; i <= 55; i++)//check for each antivirus engine whether there r anyvirus claims
                        {
                            if (i == 38)//this is because we have some antivirus engine columns in the database in random order
                                i = 45;
                            if (!dataReader[i].ToString().Equals(""))//if there is a virus claim
                            {
                                p.Kill();//kill the process 
                                NotifyUser(dataReader);//notify the user about malware identification
                                break;//break the for loop for checking
                            }
                        }
                        //if there is no virus claim then it will not notify the user    
                    }
                    else//if we dont have it analysis report then get the report from the cloud and parse it and store in our database
                    {
                        WebClient client = new WebClient();
                        Action action = () =>
                        {
                            try
                            {
                                String response = client.DownloadString("http://aravindhan.cloudapp.net:8090/tasks/report/" + dataReader["task_id"].ToString());//get json file
                                ParseJsonAndStoreInDatabase(filePath, response);//parse the json file and store it in db
                                dataReader = command.ExecuteReader();//re execute the query
                                for (int i = 3; i <= 55; i++)//check for each antivirus engine whether there r anyvirus claims
                                {
                                    if (i == 38)//this is because we have some antivirus engine columns in the database in random order
                                        i = 45;
                                    if (!dataReader[i].ToString().Equals(""))//if there is a virus claim
                                    {
                                        p.Kill();//kill the process
                                        NotifyUser(dataReader);//notify the user about malware identification
                                        break;//break the for loop for checking
                                    }
                                }
                                //if there is no virus claim then it will not notify the user    
                            }
                            catch (Exception ex)//if some web exception occurs then the report cannot be retreieved..so just ignore it
                            {
                                Console.WriteLine(ex.Message);
                                return;
                            }
                        };
                        Dispatcher.BeginInvoke(action);
                    }
                }
                else//if the file is not present
                {
                    int taskId = SubmitFileForAnalysis(filePath);//submit the file for analysis
                    if (taskId != 0)//if the taskid is 0 the file has not been submmited..so dont add it to the database
                        AddFileToDatabase(filePath, taskId);//add the file to the database
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }


        }


        //returns the md5 hash value for the file
        public static String getMD5HashValue(String filePath)
        {
            MD5 md5 = MD5.Create();//create MD5 object
            FileInfo file = new FileInfo(filePath);//fileinfo object
            while (IsFileLocked(file))//checking whether the file is locked or available
            {
            }
            FileStream stream = File.OpenRead(filePath);
            byte[] hashBytes = md5.ComputeHash(stream);
            stream.Close();
            String md5HashValue = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return md5HashValue;
        }

        //checks whether a file is locked or not
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)//if an exception occurs the file is still being copied
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }


        //this method is called to notify the user that a malware has been identified
        public void NotifyUser(SqlCeDataReader dataReader)
        {
            SystemSounds.Asterisk.Play();//alert the user with sound
            //the event isn't being raised in the UI thread, and you need to marshal over to the UI thread 
            //before creating the window. This is probably as simple as changing your event handler code
            //the window wont be displayed if we dont change the event handler code as follows
            Action action = () =>
            {
                notifyWindow = new Window();//create a new window
                notifyWindow.ShowInTaskbar = false;//disable it in taskbar
                notifyWindow.Title = "A malware has been identified and killed";//set its title
                var uriSource = new Uri("logo.ico", UriKind.Relative);
                notifyWindow.Icon = new BitmapImage(uriSource);
                notifyWindow.ResizeMode = System.Windows.ResizeMode.NoResize;//set it not resizable
                notifyWindow.Width = 370;//set width if the window
                notifyWindow.Height = 400;//set height of the window
                notifyWindow.Left = Screen.PrimaryScreen.WorkingArea.Right - notifyWindow.Width;
                notifyWindow.Top = Screen.PrimaryScreen.WorkingArea.Bottom - notifyWindow.Height;
                notifyWindow.Topmost = true;

                //Create a Stack Panel and add it to our window
                StackPanel stackPanel = new StackPanel();
                stackPanel.Background = System.Windows.Media.Brushes.Orange;
                notifyWindow.Content = stackPanel;

                //now we should continue adding items to our stack panel one by one

                //fileDetails
                StackPanel filePanel = new StackPanel();//create a new file panel
                
                filePanel.Background = System.Windows.Media.Brushes.PeachPuff;
                Expander filePanelExpander = new Expander();//create and expander for it
                filePanelExpander.Expanded += new RoutedEventHandler(changeExpanderBackgroundColor);
                filePanelExpander.Collapsed +=new RoutedEventHandler(revertExpanderBackgroundColor);
                filePanelExpander.Content = filePanel;//set the content of the expander to our panel
                filePanelExpander.BorderThickness = new Thickness(2);
                filePanelExpander.BorderBrush = System.Windows.Media.Brushes.OrangeRed;
                //create and add expander label
                System.Windows.Controls.Label filedetailsLabel = new System.Windows.Controls.Label();
                filedetailsLabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                filedetailsLabel.Content = "File Details";
                filedetailsLabel.FontWeight = FontWeights.UltraBold;//bold face the label
                filePanelExpander.Header = filedetailsLabel;//set the label as the header of the expander
                filePanelExpander.IsExpanded = true;//expand the file details by default
                stackPanel.Children.Add(filePanelExpander);//add the expander to our stack panel
                System.Windows.Controls.Label filepathlabel = new System.Windows.Controls.Label();
                filepathlabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                filepathlabel.Content = "  File Path: " + dataReader["path"].ToString();
                filePanel.Children.Add(filepathlabel);
                System.Windows.Controls.Label filetypelabel = new System.Windows.Controls.Label();
                filetypelabel.Content = "  File Type: " + dataReader["file_type"].ToString();
                filetypelabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                filePanel.Children.Add(filetypelabel);
                System.Windows.Controls.Label sha1label = new System.Windows.Controls.Label();
                sha1label.Foreground = System.Windows.Media.Brushes.DarkGreen;
                sha1label.Content = "  SHA1:      " + dataReader["sha1"].ToString();
                filePanel.Children.Add(sha1label);
                System.Windows.Controls.Label md5label = new System.Windows.Controls.Label();
                md5label.Content = "  MD5:       " + dataReader["md5"].ToString();
                md5label.Foreground = System.Windows.Media.Brushes.DarkGreen;
                filePanel.Children.Add(md5label);
                System.Windows.Controls.Label crc32label = new System.Windows.Controls.Label();
                crc32label.Content = "  CRC32:    " + dataReader["crc32"].ToString();
                crc32label.Foreground = System.Windows.Media.Brushes.DarkGreen;
                filePanel.Children.Add(crc32label);

                //Signature based detection

                //get the database data ready in a datatable
                string sqlQuery = @"select * from static where path='" + dataReader["path"] + "'";
                SqlCeCommand cmd = new SqlCeCommand(sqlQuery, connection);
                SqlCeDataAdapter da = new SqlCeDataAdapter(cmd);
                DataTable table = new DataTable();
                da.Fill(table);
                DataTable flipTable = FlipDataTable(table);

                StackPanel sigPanel = new StackPanel();//create a new file panel
                Expander sigPanelExpander = new Expander();//create and expander for it
                sigPanelExpander.Expanded += new RoutedEventHandler(changeExpanderBackgroundColor);
                sigPanelExpander.Collapsed += new RoutedEventHandler(revertExpanderBackgroundColor);
                sigPanelExpander.Content = sigPanel;//set the content of the expander to our panel
                sigPanelExpander.BorderThickness = new Thickness(0);
                sigPanelExpander.BorderBrush = System.Windows.Media.Brushes.OrangeRed;
                //1..Creating the label and adding to the stackPanel
                System.Windows.Controls.Label staticLabel = new System.Windows.Controls.Label();
                staticLabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                staticLabel.Content = "Signature Based Analysis Detection Ratio: ("+malwareCount.ToString()+"/46)";
                staticLabel.FontWeight = FontWeights.UltraBold;//bold face the label
                sigPanelExpander.Header = staticLabel;
                stackPanel.Children.Add(sigPanelExpander);//add the expander to our stack panel
                //2..creating the data grid
                System.Windows.Controls.ListView listView = new System.Windows.Controls.ListView();//create a list view
                listView.Height = 200;//set the height of the listView
              
                GridView gridView = new GridView();//create a grid view
                Style style = new Style();//style for removing headers
                style.Setters.Add(new Setter(System.Windows.Controls.Control.VisibilityProperty, Visibility.Hidden));
                Style foregroundStyle = new Style();//style for removing headers
                foregroundStyle.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, System.Windows.Media.Brushes.Red));
                gridView.AllowsColumnReorder = false;
                listView.Foreground = System.Windows.Media.Brushes.DarkGreen;
                listView.Background = System.Windows.Media.Brushes.PeachPuff;
                ScrollViewer.SetVerticalScrollBarVisibility(listView, ScrollBarVisibility.Visible);
                ScrollViewer.SetHorizontalScrollBarVisibility(listView, ScrollBarVisibility.Hidden);
                sigPanel.Children.Add(listView);//add it to our panel
                //listView.SelectionMode = System.Windows.Controls.SelectionMode.Single; 
                listView.View = gridView;//set the liste view's property to the grid view

                GridViewColumn column1 = new GridViewColumn();
                gridView.Columns.Add(column1);
                column1.SetValue(GridViewColumn.HeaderContainerStyleProperty, style); 
                column1.Width = 30;
                DataTemplate template = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));
                factory.SetBinding(System.Windows.Controls.Image.SourceProperty, new System.Windows.Data.Binding("2"));
                template.VisualTree = factory;
                column1.CellTemplate = template;

                GridViewColumn column2 = new GridViewColumn();
                gridView.Columns.Add(column2);
                column2.SetValue(GridViewColumn.HeaderContainerStyleProperty, style); 
                column2.Width = 112;
                column2.DisplayMemberBinding = new System.Windows.Data.Binding("0");
              
                GridViewColumn column3 = new GridViewColumn();
                gridView.Columns.Add(column3);
                column3.Width = 200;
                column3.SetValue(GridViewColumn.HeaderContainerStyleProperty, style);
                column3.DisplayMemberBinding = new System.Windows.Data.Binding("1");

                System.Windows.Data.Binding bind = new System.Windows.Data.Binding();
                bind.Source = flipTable;
                listView.DataContext = flipTable;
                listView.SetBinding(System.Windows.Controls.ListView.ItemsSourceProperty, bind);                
              
                //Behavioural Analysis
                StackPanel behavPanel = new StackPanel();
                behavPanel.Background = System.Windows.Media.Brushes.PeachPuff;
                Expander behavPanelExpander = new Expander();
                behavPanelExpander.Expanded += new RoutedEventHandler(changeExpanderBackgroundColor);
                behavPanelExpander.Collapsed += new RoutedEventHandler(revertExpanderBackgroundColor);
                behavPanelExpander.Content = behavPanel;
                behavPanelExpander.BorderThickness = new Thickness(0);
                behavPanelExpander.BorderBrush = System.Windows.Media.Brushes.OrangeRed;
                stackPanel.Children.Add(behavPanelExpander);
                System.Windows.Controls.Label behavLabel = new System.Windows.Controls.Label();
                behavLabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                behavLabel.Content = "Behavioural Analysis";
                behavLabel.FontWeight = FontWeights.UltraBold;//bold face the label
                behavPanelExpander.Header = behavLabel;
                string[] signatures=System.Text.RegularExpressions.Regex.Split(dataReader["signatures"].ToString(),",");
                foreach (string signature in signatures)
                {
                    if(signature!="")
                    {
                    System.Windows.Controls.Label label = new System.Windows.Controls.Label();
                    label.Content = "  " + signature;
                    label.Foreground = System.Windows.Media.Brushes.DarkGreen;
                    behavPanel.Children.Add(label);
                    }
                }
                if (behavPanel.Children.Count == 0)//if there is no behavioural malicious patterns then it is clean
                {
                    System.Windows.Controls.Label label=new System.Windows.Controls.Label();
                    label.Foreground = System.Windows.Media.Brushes.DarkGreen;
                    label.Content = "  No malicious behaviour observed";
                    behavPanel.Children.Add(label);
                }

                //Heuristic Analysis
                StackPanel heurPanel = new StackPanel();
                heurPanel.Background = System.Windows.Media.Brushes.PeachPuff;
                Expander heurPanelExpander = new Expander();
                heurPanelExpander.Expanded += new RoutedEventHandler(changeExpanderBackgroundColor);
                heurPanelExpander.Collapsed += new RoutedEventHandler(revertExpanderBackgroundColor);
                heurPanelExpander.Content = heurPanel;
                heurPanelExpander.BorderThickness = new Thickness(0);
                heurPanelExpander.BorderBrush = System.Windows.Media.Brushes.OrangeRed;
                stackPanel.Children.Add(heurPanelExpander);
                System.Windows.Controls.Label heurLabel = new System.Windows.Controls.Label();
                heurLabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                heurLabel.Content = "Heuristic Analysis";
                heurLabel.FontWeight = FontWeights.UltraBold;//bold face the label
                heurPanelExpander.Header = heurLabel;
                System.Windows.Controls.Label hlabel = new System.Windows.Controls.Label();
                hlabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                hlabel.Content = "  This file is found to be "+dataReader["heuristics"].ToString();
                heurPanel.Children.Add(hlabel);

                //Network Analysis
                 StackPanel networkPanel = new StackPanel();
                 networkPanel.Background = System.Windows.Media.Brushes.PeachPuff;
                Expander networkPanelExpander = new Expander();
                networkPanelExpander.Expanded += new RoutedEventHandler(changeExpanderBackgroundColor);
                networkPanelExpander.Collapsed += new RoutedEventHandler(revertExpanderBackgroundColor); 
                networkPanelExpander.Content = networkPanel;
                networkPanelExpander.BorderThickness = new Thickness(0);
                networkPanelExpander.BorderBrush = System.Windows.Media.Brushes.OrangeRed;
                stackPanel.Children.Add(networkPanelExpander);
                System.Windows.Controls.Label networkLabel = new System.Windows.Controls.Label();
                networkLabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                networkLabel.Content = "Network Activity";
                networkLabel.FontWeight = FontWeights.UltraBold;//bold face the label
                networkPanelExpander.Header = networkLabel;
                string[] hosts = System.Text.RegularExpressions.Regex.Split(dataReader["network"].ToString(), ",");
                foreach (string host in hosts)
                {
                    if (host != "")
                    {
                        System.Windows.Controls.Label label = new System.Windows.Controls.Label();
                        label.Content = "  " + host;
                        label.Foreground = System.Windows.Media.Brushes.DarkGreen;
                        networkPanel.Children.Add(label);
                    }
                }
                if (networkPanel.Children.Count == 0)//if there is no behavioural malicious patterns then it is clean
                {
                    System.Windows.Controls.Label label = new System.Windows.Controls.Label();
                    label.Content = "  No network activity observed";
                    label.Foreground = System.Windows.Media.Brushes.DarkGreen;
                    networkPanel.Children.Add(label);
                }

                timer.Start();//start the timer
                notifyWindow.ShowDialog();//show the window
                notifyWindow.Closed += new EventHandler(stopTimer);//when the window is closed then stop the timer  
            };
            Dispatcher.BeginInvoke(action);
        }

        //parse the json file and store data in the database
        //1. parse the json data and append to the database
        //2.change the analysis_status to complete
        public void ParseJsonAndStoreInDatabase(String filePath, String response)
        {
            sql = @"update static set analysis_status='Complete'";//prepare thw sql query and set analysis to complete
            JToken token = JObject.Parse(response);
            String heuristics = token.SelectToken("heuristics").ToString();//get the heuristics data
            sql = sql + ",heuristics='" + heuristics + "'";//append data to sql query
            //get the signatures data
            String behaviour = "";
            JToken signatures = token.SelectToken("signatures");
            foreach (JToken signature in signatures)
            {
                behaviour = behaviour + signature.SelectToken("description").ToString() + ",";
            }
            sql = sql + ",signatures='" + behaviour + "'";//append signature data to sql query
            //get the virus total data and append it to sql query
            JToken virustotal = token.SelectToken("virustotal");
            foreach (JToken engines in virustotal)
            {
                foreach (JToken engine in engines)
                {
                    sql = sql + "," + ((JProperty)engine.Parent).Name.ToString().Replace("-", "_") + "='" + engine.SelectToken("result").ToString() + "'";
                }
            }

            
            //get the network data
            String network = "";
            try
            {
                JToken hosts = token.SelectToken("network").SelectToken("hosts");
                foreach (JToken host in hosts)
                {
                    network = network + host.ToString() + ",";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            sql = sql + ",network='" + network + "'";//append the network data to teh sql query
            
            //get the file details and append then to sql query 
            JToken filedetails = token.SelectToken("filedetails").SelectToken("file");
            String file_type = filedetails.SelectToken("type").ToString();
            sql = sql + ",file_type='" + file_type + "'";
            String sha1 = filedetails.SelectToken("sha1").ToString();
            sql = sql + ",sha1='" + sha1 + "'";
            String md5 = filedetails.SelectToken("md5").ToString();
            sql = sql + ",md5='" + md5 + "'";
            String crc32 = filedetails.SelectToken("crc32").ToString();
            sql = sql + ",crc32='" + crc32 + "'";
            sql = sql + " where path='" + filePath + "'";//append the file path data to it
            try
            {
                command = new SqlCeCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //this is the timer event handler which closes the notification window after 5 seconds
        public void closeNotifyWindow(object sender, EventArgs e)
        {
            notifyWindow.Close();
        }

        public void stopTimer(object sender, EventArgs e)
        {
            timer.Stop();
        }

        
        public void changeExpanderBackgroundColor(object sender, RoutedEventArgs args)
        {
            //collpase all expanders
            foreach(UIElement expander in ((StackPanel)notifyWindow.Content).Children)
            {
                if (((Expander)expander) != ((Expander)sender) && ((Expander)expander).IsExpanded)
                    ((Expander)expander).IsExpanded = false;
            }
            ((Expander)sender).Background = System.Windows.Media.Brushes.OrangeRed;
            ((Expander)sender).BorderThickness = new Thickness(2);

        }

        public void revertExpanderBackgroundColor(object sender, RoutedEventArgs args)
        {
            ((Expander)sender).Background=null;
            ((Expander)sender).BorderThickness = new Thickness(0);
        }

        //adds the file specified with file path into the database along with its task id and analysis as incomplete(default value)
        public void AddFileToDatabase(String filePath, int taskId)
        {
            try
            {
                sql = "insert into static(path,task_id) values(@path,@task_id)";
                command = new SqlCeCommand(sql, connection);
                command.Parameters.AddWithValue("@path", filePath);
                command.Parameters.AddWithValue("@task_id", taskId);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //this takes in file path as input and submit the file to cloud for analysis
        //returns the task id of the file
        //if some web exception occurs task id will be 0
        public int SubmitFileForAnalysis(String filePath)
        {
            WebClient client = new WebClient();
            int taskId = 0;//the task id for file submission
            try
            {
                //replace locahost with our server's ip address
                byte[] bret = client.UploadFile("http://aravindhan.cloudapp.net:8090/tasks/create/file", "POST", filePath);
                string response = System.Text.Encoding.ASCII.GetString(bret);
                //we need to parse the json response and determine its task_id
                JToken token = JObject.Parse(response);
                taskId = (int)token.SelectToken("task_id");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return taskId;
        }

        // this method will capture every process start
        public void MonitorProcesses()
        {
            //start monitoring for processes
            // create event query to be notified within 1 second of a change in a service
            EventArrivedEventHandler eventHandler = new EventArrivedEventHandler(ProcessStartEvent);
            eventWatcher = new ManagementEventWatcher();//creating a management event watcher
            WqlEventQuery eventQuery = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\"");
            eventWatcher.Query = eventQuery;
            eventWatcher.EventArrived += eventHandler;
            eventWatcher.Start();
        }


        //file system watcher for the specified file for specified  file types
        public void WatchFileSystem(String drive, String type)
        {
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();//creating an instance for filesystem watcher
            fileSystemWatcher.Path = drive;//setting the path for file system watching
            fileSystemWatcher.Filter = type;//setting what files to look for.. *.* indicates all types of files
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fileSystemWatcher.IncludeSubdirectories = true;//also check for the sub directories

            //adding event handlers for various file system events
            fileSystemWatcher.Created += new FileSystemEventHandler(OnCreated);
            fileSystemWatcher.Deleted += new FileSystemEventHandler(OnDeleted);
            fileSystemWatcher.Renamed += new RenamedEventHandler(OnRenamed);
            fileSystemWatcher.EnableRaisingEvents = true;//start the file system watching action
        }

        //this method converts an input image into a byte array
        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }


        //this will flip the dataset for displaying it in notification window
        //further we will remove the unwanted data other than signature based detection data
        public DataTable FlipDataTable(DataTable table)
        {
            DataTable flipTable = new DataTable();
            malwareCount = 0;
            for (int i = 0; i <= table.Rows.Count; i++)
            {
                flipTable.Columns.Add(Convert.ToString(i));
            }
            DataColumn column = new DataColumn("2", typeof(System.Byte[]));
            flipTable.Columns.Add(column);
            DataRow r;
            for (int k = 0; k < table.Columns.Count; k++)
            {
                String data=table.Columns[k].ToString();
                if(data=="path" || data=="analysis_status" || data=="task_id" || data=="network" || data=="signature" || data=="signatures" || data=="heuristics" || data=="file_type" || data=="sha1" || data=="md5" || data=="crc32")
                {
                    continue;
                }
                r = flipTable.NewRow();
                r[0] = table.Columns[k].ToString();
                for (int j = 1; j <= table.Rows.Count; j++)
                    r[j] = table.Rows[j - 1][k];
                System.Drawing.Image image;
                if ((string)r[1] == (""))
                    image = (System.Drawing.Image)Properties.Resources.clean.GetThumbnailImage(20, 20, null, IntPtr.Zero);
                else
                {
                    malwareCount++;
                    image = (System.Drawing.Image)Properties.Resources.malicious.GetThumbnailImage(20, 20, null, IntPtr.Zero);
                }
                r[2] = imageToByteArray(image);
               flipTable.Rows.Add(r);
            }
            return flipTable;
        }

    }
}