/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.K3po.Control
{
    public abstract class Command
    {
        public enum Kind
        {
            PREPARE,
            START,
            ABORT
        };

        public abstract Kind CommandKind { get; }

    }
}
