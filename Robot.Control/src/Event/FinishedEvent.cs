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
        private string _script;

        public string Script
        {
            get { return _script; }
            set { _script = value; }
        }

        public override CommandEvent.Kind EventKind
        {
            get { return Kind.FINISHED; }
        }
    }
}
