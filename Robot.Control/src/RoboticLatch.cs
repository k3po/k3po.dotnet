/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kaazing.Robot.Control
{
    public class RoboticLatch
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

        public RoboticLatch()
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
            // TODO
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
                _prepared.Signal();
            }

            if (_startable.CurrentCount == 1)
            {
                _startable.Signal();
            }

            if (_finished.CurrentCount == 1)
            {
                _finished.Signal();
            }
        }

    }
}
