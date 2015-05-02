using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class TotalPlaytimeComponent : IComponent
    {
        protected ITimeFormatter TimeFormatter { get; set; }
        protected InfoTimeComponent InternalComponent { get; set; }
        protected TimerPhase LastPhase { get; set; }
        protected int LastAttemptCount { get; set; }

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
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;

            InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.ForeColor = state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = state.LayoutSettings.TextColor;

            InternalComponent.DrawVertical(g, state, width, clipRegion);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("element");
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public void SetSettings(XmlNode settings)
        {
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
                        var segmentHistoryElement = segment.SegmentHistory.FirstOrDefault(x => x.Index == attempt.Index);
                        if (segmentHistoryElement != null && segmentHistoryElement.Time.RealTime.HasValue)
                            totalPlaytime += segmentHistoryElement.Time.RealTime.Value;
                    }
                }
            }

            if (state.CurrentPhase == TimerPhase.Ended)
            {
                totalPlaytime += state.AttemptEnded - state.AttemptStarted;
            }
            else if (state.CurrentPhase != TimerPhase.NotRunning)
            {
                totalPlaytime += TripleDateTime.Now - state.AttemptStarted;
            }

            return totalPlaytime;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (LastAttemptCount != state.Run.AttemptHistory.Count 
                || LastPhase != state.CurrentPhase
                || state.CurrentPhase == TimerPhase.Running 
                || state.CurrentPhase == TimerPhase.Paused)
            {
                InternalComponent.TimeValue = CalculateTotalPlaytime(state);

                LastAttemptCount = state.Run.AttemptHistory.Count;
                LastPhase = state.CurrentPhase;
            }

            InternalComponent.Update(invalidator, state, width, height, mode);
        }

        public void Dispose()
        {
            InternalComponent.Dispose();
        }
    }
}
