using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
{
    private readonly object _lock = new object();
    private int _counter = 0;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void RaceButton_Click(object sender, RoutedEventArgs e)
    {
        _counter = 0;
        var thread1 = new Thread(() =>
        {
            for (int i = 0; i < 50000; i++)
            {
                _counter++;
            }
        });
        var thread2 = new Thread(() =>
        {
            for (int i = 0; i < 50000; i++)
            {
                _counter++;
            }
        });

        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();

        Dispatcher.Invoke(() => output.Text = _counter.ToString());
    }

    private void SafeButton_Click(object sender, RoutedEventArgs e)
    {
        _counter = 0;
        var thread1 = new Thread(() =>
        {
            lock (_lock)
            {
                for (int i = 0; i < 50000; i++)
                {
                    _counter++;
                }
            }
        });
        var thread2 = new Thread(() =>
        {
            lock (_lock)
            {
                for (int i = 0; i < 50000; i++)
                {
                    _counter++;
                }
            }
        });

        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();

        Dispatcher.Invoke(() => output.Text = _counter.ToString());
    }

    private void MonitorButton_Click(object sender, RoutedEventArgs e)
    {
        bool acquired = false;

        try
        {
            acquired = Monitor.TryEnter(_lock, TimeSpan.FromSeconds(1));

            if (acquired)
            {
                Dispatcher.Invoke(() =>
                {
                    output.Text = "Блокировка захвачена\n";
                    output.Text += "Блокировка успешно захвачена";
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    output.Text = "Таймаут! Блокировка не захвачена\n";
                    output.Text += "Таймаут - блокировка не захвачена";
                });
            }
        }
        finally
        {
            if (acquired)
            {
                Monitor.Exit(_lock);
            }
        }
    }
}
}