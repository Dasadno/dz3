using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int IterationsPerThread = 50000;

        private readonly object _lock = new object();
        private int _counter = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RaceButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsEnabled(false);
            output.Text = "Выполняется гонка данных...";

            int result = await Task.Run(() => RunIncrement(useLock: false));

            output.Text =
                $"Гонка данных: {result} (ожидалось {2 * IterationsPerThread})";
            SetButtonsEnabled(true);
        }

        private async void SafeButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsEnabled(false);
            output.Text = "Выполняется безопасное добавление...";

            int result = await Task.Run(() => RunIncrement(useLock: true));

            output.Text =
                $"Безопасное добавление: {result} (ожидалось {2 * IterationsPerThread})";
            SetButtonsEnabled(true);
        }

        private void MonitorButton_Click(object sender, RoutedEventArgs e)
        {
            bool acquired = false;
            try
            {
                acquired = Monitor.TryEnter(_lock, TimeSpan.FromSeconds(1));
                output.Text = acquired
                    ? "Блокировка успешно захвачена"
                    : "Таймаут — блокировка не захвачена";
            }
            finally
            {
                if (acquired)
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        /// <summary>
        /// Запускает два потока, каждый из которых увеличивает общий счётчик.
        /// Без блокировки получается гонка данных, с блокировкой — корректный результат.
        /// </summary>
        private int RunIncrement(bool useLock)
        {
            _counter = 0;

            Action increment = () =>
            {
                if (useLock)
                {
                    lock (_lock)
                    {
                        for (int i = 0; i < IterationsPerThread; i++)
                        {
                            _counter++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < IterationsPerThread; i++)
                    {
                        _counter++;
                    }
                }
            };

            var thread1 = new Thread(new ThreadStart(increment));
            var thread2 = new Thread(new ThreadStart(increment));

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            return _counter;
        }

        private void SetButtonsEnabled(bool enabled)
        {
            raceButton.IsEnabled = enabled;
            safeButton.IsEnabled = enabled;
            monitorButton.IsEnabled = enabled;
        }
    }
}
