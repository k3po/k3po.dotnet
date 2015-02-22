/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.K3po.Control
{
    public abstract class ControlEvent
    {
        public enum Kind
        {
            PREPARED, STARTED, FINISHED, ERROR
        }

        public abstract Kind EventKind
        {
            get;
        }

    }
}
