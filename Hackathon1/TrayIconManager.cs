
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms; // For NotifyIcon
using System.Drawing;      // For Icon
using ContextMenu = System.Windows.Forms.ContextMenu; // To avoid name conflicts
using Newtonsoft.Json;

namespace Hackathon1
{
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private MainWindow _mainWindow;

        public TrayIconManager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            // Create NotifyIcon
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "proclip_Hl6_icon.ico")), // Replace with the path to your icon
                Text = "ProClip",
                Visible = true
            };

            // Create a context menu for the tray icon
            var contextMenu = new ContextMenu(new[]
            {
                new MenuItem("ProClip", (s, e) => ShowMainWindow()),
                new MenuItem("Exit", (s, e) => ExitApplication())
            });
            _notifyIcon.ContextMenu = contextMenu;

            // Handle double-click event
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private void ExitApplication()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            string json = JsonConvert.SerializeObject(_mainWindow.getClipboardHistory(), Formatting.Indented);
            File.WriteAllText("appData.json", json);
            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }
    }
}
