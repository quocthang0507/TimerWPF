﻿using System;
using System.ComponentModel;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
        private TimeSpan _endTime;
        private System.Timers.Timer _timer;
        private double _currentValue = 0;
        private int _lastTimeToAlert = 0;
        private string _ringtone = "defaultRingtone";
        private SoundPlayer _player = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public double Value
        {
            get => _currentValue;
            set
            {
                this._currentValue = value;
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
            double totalSeconds = _endTime.TotalSeconds;
            if (Value >= totalSeconds)
            {
                _timer.Stop();
                Value = 0;
                PlayRingtone();
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
            if (_timer == null || Value == 0)
            {
                if (int.TryParse(txtHrs.Text, out var hrs) &&
                int.TryParse(txtMin.Text, out var min) &&
                int.TryParse(txtSec.Text, out var sec))
                {
                    _endTime = new TimeSpan(hrs, min, sec);
                    double totalSeconds = _endTime.TotalSeconds;
                    _timer = new(1000);

                    _timer.Elapsed += Timer_Elapsed;
                    progressBar.Maximum = totalSeconds;

                    ToggleReadonly();
                    _timer.Start();
                    UpdateTimeLabel((int)totalSeconds);
                }
                else
                    MessageBox.Show("Thời gian nhập không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                _timer.Start();
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                Value = 0;
                if (txtHrs.IsReadOnly)
                    ToggleReadonly();
                _player.Stop();
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = FindResource("contextMenu") as ContextMenu;
            contextMenu.PlacementTarget = sender as Button;
            contextMenu.IsOpen = true;
        }

        private void MenuItemRingtone_Click(object sender, RoutedEventArgs e)
        {
            MenuItem? selectedMenu = sender as MenuItem;
            ContextMenu contextMenu = FindResource("contextMenu") as ContextMenu;
            foreach (var item in contextMenu.Items)
            {
                MenuItem? menuItem = item as MenuItem;
                if (item is MenuItem && menuItem.Name != selectedMenu.Name)
                    switch (menuItem.Name)
                    {
                        case "defaultRingtone":
                        case "ringtone1":
                        case "ringtone2":
                        case "ringtone3":
                            menuItem.IsChecked = false;
                            break;
                    }
            }
            selectedMenu.IsChecked = true;
            _ringtone = selectedMenu.Name;
        }

        private void MenuItemAlert_Click(object sender, RoutedEventArgs e)
        {
            MenuItem? selectedMenu = sender as MenuItem;
            ContextMenu contextMenu = FindResource("contextMenu") as ContextMenu;
            foreach (var item in contextMenu.Items)
            {
                MenuItem? menuItem = item as MenuItem;
                if (item is MenuItem && menuItem.Name != selectedMenu.Name)
                    switch (menuItem.Name)
                    {
                        case "zeroSecond":
                        case "threeSeconds":
                        case "fiveSeconds":
                        case "tenSeconds":
                            menuItem.IsChecked = false;
                            break;
                    }
            }
            selectedMenu.IsChecked = true;
            switch (selectedMenu.Name)
            {
                case "zeroSecond":
                    _lastTimeToAlert = 0;
                    break;
                case "threeSeconds":
                    _lastTimeToAlert = 3;
                    break;
                case "fiveSeconds":
                    _lastTimeToAlert = 5;
                    break;
                case "tenSeconds":
                    _lastTimeToAlert = 10;
                    break;
            }
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
            if (remainingTime > 0 && remainingTime <= _lastTimeToAlert)
                PlayAlert();
        }

        private void PlayAlert()
        {
            _player = new(Properties.Resources.beep);
            _player.Load();
            _player.Play();
        }

        private void PlayRingtone()
        {
            switch (_ringtone)
            {
                case "defaultRingtone":
                    MessageBox.Show("Hết giờ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                case "ringtone1":
                    _player = new(Properties.Resources.siren_alert);
                    break;
                case "ringtone2":
                    _player = new(Properties.Resources.attention);
                    break;
                case "ringtone3":
                    _player = new(Properties.Resources.success);
                    break;
            }
            _player.Load();
            _player.Play();
        }

        #region INotifyPropertyChanged

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            double totalSeconds = _endTime.TotalSeconds;
            if (Value > 0)
                UpdateTimeLabel((int)(totalSeconds - Value));
            else UpdateTimeLabel(0);
        }
        #endregion

    }
}
