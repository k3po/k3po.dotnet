
/*
 * Copyright (c) 2007-2014 Kaazing Corporation. All rights reserved.
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kaazing.K3po.NUnit
{
    public class Latch
    {
        enum State
        {
            INIT,
            PREPARED,
            STARTABLE,
            FINISHED
        }

        private volatile State _state;
        private volatile Exception _exception;

        private CountdownEvent _prepared;
        private CountdownEvent _startable;
        private CountdownEvent _finished;

        public Latch()
        {
            _state = State.INIT;

            _prepared = new CountdownEvent(1);
            _startable = new CountdownEvent(1);
            _finished = new CountdownEvent(1);
        }

        public void NotifyPrepared()
        {
            switch (_state)
            {
                case State.INIT:
                    _state = State.PREPARED;
                    _prepared.Signal();
                    break;
                default:
                    throw new InvalidOperationException(_state.ToString());
            }
        }

        public void AwaitPrepared()
        {
            _prepared.Wait();
            if (_exception != null)
            {
                throw _exception;
            }
        }

        public bool IsPrepared
        {
            get
            {
                return _prepared.CurrentCount == 0;
            }
        }

        public void NotifyStartable()
        {
            switch (_state)
            {
                case State.PREPARED:
                    _state = State.STARTABLE;
                    _startable.Signal();
                    break;
                default:
                    throw new InvalidOperationException(_state.ToString());
            }
        }

        public void AwaitStartable()
        {
            _startable.Wait();
            if (_exception != null)
            {
                throw _exception;
            }
        }

        public bool IsStartable
        {
            get
            {
                return _startable.CurrentCount == 0;
            }
        }

        public void NotifyFinished()
        {
            switch (_state)
            {
                case State.INIT:
                    NotifyPrepared();
                    break;
                // We could abort before started.
                case State.PREPARED:
                case State.STARTABLE:
                    _state = State.FINISHED;
                    _finished.Signal();
                    break;
                default:
                    throw new InvalidOperationException(_state.ToString());
            }
        }

        public void NotifyAbort()
        {
            switch (_state)
            {
                case State.INIT:
                    NotifyPrepared();
                    break;
                case State.PREPARED:
                    NotifyStartable();
                    break;
            }
        }

        public void AwaitFinished()
        {
            _finished.Wait();
            if (_exception != null)
            {
                throw _exception;
            }
        }

        public bool IsFinished
        {
            get
            {
                return _finished.CurrentCount == 0L;
            }
        }

        public bool HasException
        {
            get
            {
                return _exception != null;
            }
        }

        public void NotifyException(Exception exception)
        {
            this._exception = exception;
            if (_prepared.CurrentCount == 1)
            {
                NotifyPrepared();
            }

            if (_startable.CurrentCount == 1)
            {
                NotifyStartable();
            }

            if (_finished.CurrentCount == 1)
            {
                NotifyFinished();
            }
        }

    }
}
