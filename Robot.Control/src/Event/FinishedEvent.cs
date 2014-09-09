/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.Control.Event
{
    public class FinishedEvent : CommandEvent
    {
        private string _observedScript;
        private string _expectedScript;

        public string ObservedScript
        {
            get { return _observedScript; }
            set { _observedScript = value; }
        }

        public string ExpectedScript
        {
            get { return _expectedScript; }
            set { _expectedScript = value; }
        }

        public override CommandEvent.Kind EventKind
        {
            get { return Kind.FINISHED; }
        }
    }
}
