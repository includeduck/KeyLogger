using System.Windows;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace KeyMapper.Desktop;

public partial class MainWindow : Window
{
    private readonly Forms.NotifyIcon _trayIcon;
    private bool _isExitRequested;

    public MainWindow()
    {
        InitializeComponent();

        _trayIcon = new Forms.NotifyIcon
        {
            Icon = Drawing.SystemIcons.Application,
            Text = "KeyMapper Desktop - idle",
            Visible = true,
            ContextMenuStrip = BuildTrayMenu()
        };

        _trayIcon.DoubleClick += (_, _) => ShowFromTray();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExitRequested)
        {
            e.Cancel = true;
            Hide();
            WindowState = WindowState.Minimized;
            _trayIcon.ShowBalloonTip(1500, "KeyMapper Desktop", "Still running in the notification area.", Forms.ToolTipIcon.Info);
            return;
        }

        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        base.OnClosing(e);
    }

    private Forms.ContextMenuStrip BuildTrayMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open KeyMapper", null, (_, _) => ShowFromTray());
        menu.Items.Add("Pause collection", null, (_, _) => SetCollectorPaused());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());

        return menu;
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void SetCollectorPaused()
    {
        StatusText.Text = "Paused - collector controls are ready for Phase 2";
        _trayIcon.Text = "KeyMapper Desktop - paused";
    }

    private void ExitApplication()
    {
        _isExitRequested = true;
        Close();
    }
}
