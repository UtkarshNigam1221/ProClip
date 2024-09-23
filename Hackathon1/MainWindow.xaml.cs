using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;

namespace Hackathon1
{
    public partial class MainWindow : Window
    {
        private List<AppDataItem> _clipboardHistory;
        private readonly DispatcherTimer _clipboardMonitorTimer;
        private readonly HashSet<string> imageFileExtensions = new HashSet<string>
            {
                ".jpg", ".jpeg",
                ".png",
                ".gif",
                ".bmp",
                ".tiff", ".tif",
                ".ico",
                ".webp",
                ".svg"
            };
        private Dictionary<string, AppDataItem> _dict = new Dictionary<string, AppDataItem>();

        private readonly TrayIconManager _trayIconManager;

        public MainWindow()
        {
            InitializeComponent();
            // Set up clipboard monitor
            _clipboardMonitorTimer = new DispatcherTimer();
            _clipboardMonitorTimer.Interval = TimeSpan.FromSeconds(1); // Check every 1 second
            _clipboardMonitorTimer.Tick += ClipboardMonitorTimer_Tick;
            _clipboardMonitorTimer.Start();
            _trayIconManager = new TrayIconManager(this);
            LoadData();
            // Set the clipboard history list as the ItemsSource for the ListBox
            ClipboardHistoryListBox.ItemsSource = _clipboardHistory;

            // Enable filtering using CollectionView
            ICollectionView view = CollectionViewSource.GetDefaultView(ClipboardHistoryListBox.ItemsSource);
            view.Filter = ClipboardHistoryFilter;

        }

        private void LoadData()
        {
            string filePath = "appData.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new AppDataItemConverter());
                _clipboardHistory = JsonConvert.DeserializeObject<List<AppDataItem>>(json, settings);
            }
            else
            {
                _clipboardHistory = new List<AppDataItem>(); ; // Initialize if no data exists
            }
            initialiseClipBoardHistoryListBox();
        }

        private void initialiseClipBoardHistoryListBox()
        {
            foreach (AppDataItem s in _clipboardHistory)
            {
                string key="";
                if (s is ImageData imageData)
                {
                    key = "Image: " + imageData.Location;
                    _dict.Add(key, imageData);
                }
                else if (s is StringData stringData) {
                    key = "Text: " + stringData.Value;
                    _dict.Add(key, stringData);
                }

            }
        }

        private bool ClipboardHistoryFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                return true;
            if (item is string clipboardText)
            {
                return clipboardText.IndexOf(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else if (item is AppDataItem appDataItem)
            {
                string displayText = "";
                if (appDataItem is StringData stringData)
                {
                    displayText = stringData.Value;
                }
                else if (appDataItem is ImageData imageData)
                {
                    displayText = imageData.Location;
                }
                return displayText.IndexOf(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Refresh the CollectionView each time the search text changes
            CollectionViewSource.GetDefaultView(ClipboardHistoryListBox.ItemsSource).Refresh();
        }

        public List<AppDataItem> getClipboardHistory() { return _clipboardHistory; }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide(); // Hide the window when minimized
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the close event
            this.Hide();    // Hide the window instead
        }

        protected override void OnClosed(EventArgs e)
        {
            _trayIconManager.Dispose(); // Clean up the tray icon when closing
            base.OnClosed(e);
        }

        private bool SearchClipboardHistory(string text)
        {
            foreach (AppDataItem item in _clipboardHistory)
            {
                if(item.GetValue()==text)
                    return true;
            }
            return false;
        }

        // Event triggered every time the timer ticks (every 1 second)
        private void ClipboardMonitorTimer_Tick(object sender, EventArgs e)
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                string clipboardText = System.Windows.Clipboard.GetText();
                if (!SearchClipboardHistory(clipboardText))
                {
                    _clipboardHistory.Insert(0, new StringData(clipboardText)); // Add to history
                    string key = "Text: " + clipboardText;
                    _dict.Add(key, new StringData(clipboardText));
                    CollectionViewSource.GetDefaultView(ClipboardHistoryListBox.ItemsSource).Refresh();

                }
            }
            else if (System.Windows.Clipboard.ContainsFileDropList())
            {
                StringCollection fileDropList = Clipboard.GetFileDropList();

                if (fileDropList.Count > 0)
                {
                    // The latest copied file will typically be the first in the list
                    string latestFilePath = fileDropList[0];

                    if (imageFileExtensions.Contains(System.IO.Path.GetExtension(latestFilePath)) && !SearchClipboardHistory(latestFilePath))
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(latestFilePath));
                        string key = "Image: " + latestFilePath;
                        _clipboardHistory.Insert(0, new ImageData(latestFilePath,bitmap)); // Add to history
                        _dict.Add(key,new ImageData(latestFilePath, bitmap));
                        CollectionViewSource.GetDefaultView(ClipboardHistoryListBox.ItemsSource).Refresh();
                    }
                }
            }
        }

        // Event triggered when the user selects an item in the clipboard history
        private void ClipboardHistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClipboardHistoryListBox.SelectedIndex >= 0)
            {
                AppDataItem selectedItem = _dict[ClipboardHistoryListBox.SelectedItem.ToString()];

                // Display the preview based on the clipboard content type
                if (selectedItem is StringData stringData)
                {
                    TextBlock textPreview = new TextBlock
                    {
                        Text = stringData.GetValue(),
                        TextWrapping = TextWrapping.Wrap
                    };
                    PreviewControl.Content = textPreview; // Show text preview
                }
                else if (selectedItem is ImageData imageData)
                {
                    System.Windows.Controls.Image imagePreview = new System.Windows.Controls.Image
                    {
                        Source = (BitmapSource)imageData.Value,
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    PreviewControl.Content = imagePreview; // Show image preview
                }
            }
        }
        private void SearchTextBox_TextChanged(object sender, EventArgs e) {
            // Refresh the CollectionView each time the search text changes
            CollectionViewSource.GetDefaultView(ClipboardHistoryListBox.ItemsSource).Refresh();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // create a new settings window to save settings and openDialog to open the window from here
        }
    }
}
