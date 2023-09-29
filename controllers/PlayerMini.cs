using Launcher.controllers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Launcher
{
    public class PlayerMini
    {
        #region Variables
        private DispatcherTimer player_timer;
        private bool isPlayerDragging;
        private MainWindow _mw;

        private string btn_play_svg = "M36.068 20.176l-29-20c-.307-.211-.705-.233-1.033-.062C5.706.287 5.5.627 5.5.999v40c0 .372.206.713.535.886.146.076.306.114.465.114.199 0 .397-.06.568-.177l29-20c.271-.187.432-.494.432-.823s-.162-.636-.432-.823z";
        private string btn_pause_svg = "M13.987 0c-2.762 0-5 2.239-5 5v35.975c0 2.763 2.238 5 5 5s5-2.238 5-5V5c0-2.762-2.237-5-5-5zM31.987 0c-2.762 0-5 2.239-5 5v35.975c0 2.762 2.238 5 5 5s5-2.238 5-5V5c0-2.761-2.238-5-5-5z";
        #endregion

        public PlayerMini(MainWindow mw)
        {
            _mw = mw;

            _mw.player.MediaOpened += player_MediaOpened;
            _mw.player.MediaEnded += player_MediaEnded;

            _mw.player_progress.MouseDown += player_progress_MouseDown;
            _mw.player_progress.MouseUp += player_progress_MouseUp;
            _mw.player_progress.MouseMove += player_progress_MouseMove;

            _mw.btn_player_control.Click += btn_player_control_Click;

            isPlayerDragging = false;
            player_timer = new DispatcherTimer();
            player_timer.Interval = TimeSpan.FromSeconds(1);
            player_timer.Tick += player_timerTick;
        }

        public void LoadSource(List<LauncherVideosInfo> list)
        {
            try
            {
                Random rand = new Random();
                int index = rand.Next(list.Count);
                string source_url = list[index].source_url;
                if (source_url.Contains("https"))
                    source_url = source_url.Replace("https", "http");

                _mw.player.Source = new Uri(source_url);
                _mw.player.Stop();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void player_timerTick(object sender, EventArgs e)
        {
            if (_mw.player.Source != null && _mw.player.NaturalDuration.HasTimeSpan && !isPlayerDragging)
            {
                _mw.player_progress.Minimum = 0;
                _mw.player_progress.Maximum = _mw.player.NaturalDuration.TimeSpan.TotalSeconds;
                _mw.player_progress.Value = _mw.player.Position.TotalSeconds;
            }
        }

        public void ResetPlayer()
        {
            player_timer.Stop();
            _mw.player.Source = null;
            _mw.player_progress.Value = 0;
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            _mw.player_progress.Maximum = _mw.player.NaturalDuration.TimeSpan.TotalSeconds;
            DrawPlayerControlButton("PLAY");
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            _mw.player.Stop();
            DrawPlayerControlButton("PLAY");
        }

        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            return (MediaState)stateField.GetValue(helperObject);
        }

        public bool IsPlaying()
        {
            return GetMediaState(_mw.player) == MediaState.Play;
        }

        public bool IsPaused()
        {
            return GetMediaState(_mw.player) == MediaState.Pause;
        }

        public bool IsStopped()
        {
            return GetMediaState(_mw.player) == MediaState.Stop;
        }

        private void DrawPlayerControlButton(string control)
        {
            ControlTemplate ct = _mw.btn_player_control.Template;
            System.Windows.Shapes.Path control_svg = (System.Windows.Shapes.Path)ct.FindName("player_control_svg", _mw.btn_player_control);
            switch (control)
            {
                case "PLAY":
                    control_svg.Data = Geometry.Parse(btn_play_svg);
                    break;
                case "PAUSE":
                    control_svg.Data = Geometry.Parse(btn_pause_svg);
                    break;
                default:
                    break;
            }
        }

        private void player_progress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double mousePosition = e.GetPosition(_mw.player_progress).X;
            setProgressBarPosition(mousePosition);

            isPlayerDragging = true;
        }

        private void player_progress_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double mousePosition = e.GetPosition(_mw.player_progress).X;
                setProgressBarPosition(mousePosition);
            }
        }

        public void Pause()
        {
            _mw.player.Pause();
            player_timer.Stop();
            DrawPlayerControlButton("PLAY");
        }

        public void Stop()
        {
            _mw.player.Stop();
            player_timer.Stop();
            DrawPlayerControlButton("PLAY");
        }

        public void Play()
        {
            _mw.player.Play();
            player_timer.Start();
            DrawPlayerControlButton("PAUSE");
        }

        private void setProgressBarPosition(double mousePosition)
        {
            if (_mw.player.Source != null)
            {
                double progressBarPosition = mousePosition / _mw.player_progress.ActualWidth * _mw.player_progress.Maximum;
                _mw.player_progress.Value = progressBarPosition;
            }
        }

        private void player_progress_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_mw.player.Source != null && _mw.player.NaturalDuration.HasTimeSpan)
            {
                double videoPositon = _mw.player.NaturalDuration.TimeSpan.Ticks * (_mw.player_progress.Value / _mw.player_progress.Maximum);
                _mw.player.Position = new TimeSpan((int)videoPositon);
            }

            isPlayerDragging = false;
        }

        private void btn_player_control_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying())
                Pause();
            else
                Play();
        }
    }
}
