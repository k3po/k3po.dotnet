/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.Control
{
    public class RobotControlFactory
    {
        public RobotControl NewClient(Uri controlUri)
        {
            string scheme = controlUri.Scheme;

            switch (scheme)
            {
                case "tcp":
                    return new TcpRobotControl(controlUri.Host, controlUri.Port);
                default:
                    throw new NotSupportedException("Scheme: '" + scheme + "' is not supported");
            }
        }
    }
}
