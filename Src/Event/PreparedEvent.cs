using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.Control.Event
{
    public class PreparedEvent : CommandEvent
    {
        public override CommandEvent.Kind EventKind
        {
            get { return Kind.PREPARED; }
        }
    }
}
