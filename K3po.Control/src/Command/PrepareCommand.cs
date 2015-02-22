/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.K3po.Control
{
    public class PrepareCommand : Command
    {

        private readonly List<string> _names;

        public PrepareCommand()
        {
            _names = new List<string>();
        }

        public IList<string> Names {
            get {
                return _names;
            }
            set {
                _names.Clear();
                _names.AddRange(value);
            }
        }

        public override Kind CommandKind
        {
            get { return Kind.PREPARE; }
        }
    }
}
