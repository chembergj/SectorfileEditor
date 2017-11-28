using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace SectorfileEditor.Control
{
    [Target("LogWindow")]
    public class LogWindowTarget: TargetWithLayout
    {
        public event Action<LogEventInfo> LogReceived;

        protected override void Write(LogEventInfo logEvent)
        {
            if (LogReceived != null)
                LogReceived(logEvent);
        }
    }
}
