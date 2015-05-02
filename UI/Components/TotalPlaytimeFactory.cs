using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: ComponentFactory(typeof(LiveSplit.UI.Components.TotalPlaytimeFactory))]

namespace LiveSplit.UI.Components
{
    public class TotalPlaytimeFactory : IComponentFactory
    {
        public ComponentCategory Category
        {
            get { return ComponentCategory.Information; }
        }

        public string ComponentName
        {
            get { return "Total Playtime"; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new TotalPlaytimeComponent(state);
        }

        public string Description
        {
            get { return "Shows the total playtime for running with these splits."; }
        }

        public string XMLURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/Components/update.LiveSplit.TotalPlaytime.xml"; }
#endif
        }

        public string UpdateURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/"; }
#endif
        }

        public Version Version
        {
            get { return Version.Parse("1.0.0"); }
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }
    }
}
