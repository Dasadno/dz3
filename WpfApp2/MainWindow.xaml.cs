using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_running) return;
            _running = true;
            _producer = new Thread(Producer);
            _consumer = new Thread(Consumer);
            _producer.Start();
            _consumer.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _running = false;
            lock (_lock) Monitor.PulseAll(_lock);
        }

        private void Producer()
        {
            int i = 0;
            while (_running)
            {
                lock (_lock)
                {
                    _queue.Enqueue($"Item-{i++}");
                    Log($"Produced: Item-{i - 1}");
                    Monitor.Pulse(_lock);
                }
                Thread.Sleep(500);
            }
        }

        private void Consumer()
        {
            while (_running)
            {
                string item = null;
                lock (_lock)
                {
                    while (_queue.Count == 0 && _running)
                        Monitor.Wait(_lock);

                    if (!_running) return;
                    item = _queue.Dequeue();
                }

                Log($"Consumed: {item}");
                Thread.Sleep(1000);
            }
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() => LogBox.Items.Add($"{DateTime.Now:HH:mm:ss} {message}"));
        }
    }
}