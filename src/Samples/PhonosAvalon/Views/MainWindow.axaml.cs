using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.Linq;

namespace PhonosAvalon.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var pos = Settings.Instance.WindowPos;

        // Ignore 0,0 size due to Mac problem
        if (!string.IsNullOrEmpty(pos) && !pos.StartsWith("0,0,"))
        {
            try
            {
                var parts = pos.Split(',');
                var newState = (WindowState)Enum.Parse(typeof(WindowState), parts[4]);

                // If saved as maximized, restore it without a size (as that will be the same size)
                if (newState != WindowState.Maximized)
                {
                    var newPosition = new Avalonia.PixelPoint(int.Parse(parts[2]), int.Parse(parts[3]));

                    var screen = this.Screens.All.FirstOrDefault(s => s.WorkingArea.Contains(newPosition));

                    if (screen != null)
                    {
                        this.Width = int.Parse(parts[0]);
                        this.Height = int.Parse(parts[1]);
                        this.Position = newPosition;
                    }
                }
                this.WindowState = newState;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message} loading window state");
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Don't save things if Minimized as confusing when restored
        if (this.WindowState != WindowState.Minimized)
        {
            // persist as ints to avoid floating point decimal character locale madness
            string summary = $"{(int)this.Width},{(int)this.Height},{this.Position.X},{this.Position.Y},{this.WindowState}";
            Settings.Instance.WindowPos = summary;
        }

        base.OnClosing(e);
    }
}
