/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.K3po.Control
{
    public class ErrorEvent : ControlEvent
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

        public override ControlEvent.Kind EventKind
        {
            get { return Kind.ERROR; }
        }
    }
}
