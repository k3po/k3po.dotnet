/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.Control.Event
{
    public abstract class CommandEvent
    {
        public enum Kind
        {
            PREPARED, STARTED, FINISHED, ERROR
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public abstract Kind EventKind
        {
            get;
        }

    }
}
