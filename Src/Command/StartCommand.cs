/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.Control.Command
{
    public class StartCommand : BaseCommand
    {
        public override Kind CommandKind
        {
            get { return Kind.START; }
        }
    }
}
