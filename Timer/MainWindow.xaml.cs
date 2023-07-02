using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Timer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex DigitRegex();
        private static readonly Regex _regex = DigitRegex();
        private TimeSpan endTime;
        private System.Timers.Timer timer;
        private double currentValue = 0;
        public event PropertyChangedEventHandler? PropertyChanged;

        public double Value
        {
            get => currentValue;
            set
            {
                this.currentValue = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Events

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Value++;
            double totalSeconds = endTime.TotalSeconds;
            if (Value >= totalSeconds)
            {
                timer.Stop();
                Value = 0;
                MessageBox.Show("Hết giờ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (timer == null || Value == 0)
            {
                if (int.TryParse(txtHrs.Text, out var hrs) &&
                int.TryParse(txtMin.Text, out var min) &&
                int.TryParse(txtSec.Text, out var sec))
                {
                    endTime = new TimeSpan(hrs, min, sec);
                    double totalSeconds = endTime.TotalSeconds;
                    timer = new(1000);

                    timer.Elapsed += Timer_Elapsed;
                    progressBar.Maximum = totalSeconds;

                    ToggleReadonly();
                    timer.Start();
                    UpdateTimeLabel((int)totalSeconds);
                }
                else
                    MessageBox.Show("Thời gian nhập không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                timer.Start();
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                Value = 0;
                if (txtHrs.IsReadOnly)
                    ToggleReadonly();
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void ToggleReadonly()
        {
            txtHrs.IsReadOnly = !txtHrs.IsReadOnly;
            txtMin.IsReadOnly = !txtMin.IsReadOnly;
            txtSec.IsReadOnly = !txtSec.IsReadOnly;
        }

        private void UpdateTimeLabel(int remainingTime)
        {
            TimeSpan time = TimeSpan.FromSeconds(remainingTime);
            string text = time.ToString(@"hh\:mm\:ss");
            Dispatcher.Invoke(() =>
            {
                lblRemaining.Text = text;
            });
        }

        #region INotifyPropertyChanged

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            double totalSeconds = endTime.TotalSeconds;
            if (Value > 0)
                UpdateTimeLabel((int)(totalSeconds - Value));
            else UpdateTimeLabel(0);
        }
        #endregion

    }
}
