using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Демонстрация модели «производитель — потребитель» на основе Monitor.Wait/Pulse.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly object _lock = new object();
        private readonly Queue<string> _queue = new Queue<string>();
        private volatile bool _running = false;
        private Thread _producer, _consumer;

        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => Stop();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_running) return;

            _running = true;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            _producer = new Thread(Producer) { IsBackground = true, Name = "Producer" };
            _consumer = new Thread(Consumer) { IsBackground = true, Name = "Consumer" };
            _producer.Start();
            _consumer.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void Stop()
        {
            if (!_running) return;

            _running = false;
            // Будим потребителя, ожидающего в Monitor.Wait, чтобы он завершился.
            lock (_lock)
            {
                Monitor.PulseAll(_lock);
            }
        }

        private void Producer()
        {
            int i = 0;
            while (_running)
            {
                string item = $"Item-{i++}";

                lock (_lock)
                {
                    _queue.Enqueue(item);
                    Monitor.Pulse(_lock);
                }

                // Логируем вне блокировки и асинхронно, иначе при синхронном
                // Dispatcher.Invoke внутри lock возможен дедлок с UI-потоком.
                Log($"Produced: {item}");
                Thread.Sleep(500);
            }
        }

        private void Consumer()
        {
            while (_running)
            {
                string item;
                lock (_lock)
                {
                    while (_queue.Count == 0 && _running)
                    {
                        Monitor.Wait(_lock);
                    }

                    if (!_running) return;
                    item = _queue.Dequeue();
                }

                Log($"Consumed: {item}");
                Thread.Sleep(1000);
            }
        }

        private void Log(string message)
        {
            // BeginInvoke — асинхронно, не блокирует рабочий поток.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LogBox.Items.Add($"{DateTime.Now:HH:mm:ss} {message}");
                LogBox.ScrollIntoView(LogBox.Items[LogBox.Items.Count - 1]);
            }));
        }
    }
}
