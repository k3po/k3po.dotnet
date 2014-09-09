/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.Control.Command
{
    public abstract class BaseCommand
    {
        public enum Kind
        {
            PREPARE,
            START,
            ABORT
        };

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public abstract Kind CommandKind { get; }

    }
}
