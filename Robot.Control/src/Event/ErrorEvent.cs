/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.Control.Event
{
    public class ErrorEvent : CommandEvent
    {
        private string _summary;
        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Summary
        {
            get { return _summary; }
            set { _summary = value; }
        }

        public override CommandEvent.Kind EventKind
        {
            get { return Kind.ERROR; }
        }
    }
}
