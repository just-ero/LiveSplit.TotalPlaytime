using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class TotalPlaytimeComponent : IComponent
    {
        protected ITimeFormatter TimeFormatter { get; set; }
        protected InfoTimeComponent InternalComponent { get; set; }
        protected TotalPlaytimeSettings Settings { get; set; }

        protected TimerPhase LastPhase { get; set; }
        protected int LastAttemptCount { get; set; }
        protected IRun LastRun { get; set; }

        public string ComponentName
        {
            get { return "Total Playtime"; }
        }

        public IDictionary<string, Action> ContextMenuControls
        {
            get { return null; }
        }

        public float HorizontalWidth
        {
            get { return InternalComponent.HorizontalWidth; }
        }

        public float VerticalHeight
        {
            get { return InternalComponent.VerticalHeight; }
        }

        public float MinimumHeight
        {
            get { return InternalComponent.MinimumHeight; }
        }

        public float MinimumWidth
        {
            get { return InternalComponent.MinimumWidth; }
        }

        public float PaddingBottom
        {
            get { return InternalComponent.PaddingBottom; }
        }

        public float PaddingLeft
        {
            get { return InternalComponent.PaddingLeft; }
        }

        public float PaddingRight
        {
            get { return InternalComponent.PaddingRight; }
        }

        public float PaddingTop
        {
            get { return InternalComponent.PaddingTop; }
        }

        public TotalPlaytimeComponent(LiveSplitState state)
        {
            TimeFormatter = new RegularTimeFormatter(TimeAccuracy.Seconds);
            InternalComponent = new InfoTimeComponent("Total Playtime", TimeSpan.Zero, TimeFormatter);
            Settings = new TotalPlaytimeSettings()
            {
                CurrentState = state
            };
        }

        private void DrawBackground(Graphics g, LiveSplitState state, float width, float height)
        {
            if (Settings.BackgroundColor.ToArgb() != Color.Transparent.ToArgb()
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.ToArgb() != Color.Transparent.ToArgb())
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            DrawBackground(g, state, HorizontalWidth, height);

            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = Settings.OverrideTimeColor ? Settings.TimeColor : state.LayoutSettings.TextColor;

            InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            DrawBackground(g, state, width, VerticalHeight);

            InternalComponent.DisplayTwoRows = Settings.Display2Rows;

            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = Settings.OverrideTimeColor ? Settings.TimeColor : state.LayoutSettings.TextColor;

            InternalComponent.DrawVertical(g, state, width, clipRegion);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public TimeSpan CalculateTotalPlaytime(LiveSplitState state)
        {
            var totalPlaytime = TimeSpan.Zero;

            foreach (var attempt in state.Run.AttemptHistory)
            {
                var duration = attempt.Duration;

                if (duration.HasValue)
                {
                    //Either >= 1.6.0 or a finished run
                    totalPlaytime += duration.Value;
                }
                else
                {
                    //Must be < 1.6.0 and a reset
                    //Calculate the sum of the segments for that run
                    
                    foreach (var segment in state.Run)
                    {
                        Time segmentHistoryElement;
                        if (segment.SegmentHistory.TryGetValue(attempt.Index, out segmentHistoryElement) && segmentHistoryElement.RealTime.HasValue)
                            totalPlaytime += segmentHistoryElement.RealTime.Value;
                    }
                }
            }

            if (state.CurrentPhase == TimerPhase.Ended)
            {
                totalPlaytime += state.AttemptEnded - state.AttemptStarted;
            }
            else if (state.CurrentPhase != TimerPhase.NotRunning)
            {
                totalPlaytime += TimeStamp.CurrentDateTime - state.AttemptStarted;
            }

            return totalPlaytime;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (LastAttemptCount != state.Run.AttemptHistory.Count 
                || LastPhase != state.CurrentPhase
                || LastRun != state.Run
                || state.CurrentPhase == TimerPhase.Running 
                || state.CurrentPhase == TimerPhase.Paused)
            {
                InternalComponent.TimeValue = CalculateTotalPlaytime(state);

                LastAttemptCount = state.Run.AttemptHistory.Count;
                LastPhase = state.CurrentPhase;
                LastRun = state.Run;
            }

            InternalComponent.Update(invalidator, state, width, height, mode);
        }

        public void Dispose()
        {
            InternalComponent.Dispose();
        }

        public int GetSettingsHashCode()
        {
            return Settings.GetSettingsHashCode();
        }
    }
}
