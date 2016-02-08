
/*
 * Copyright (c) 2007-2016 Kaazing Corporation. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using NUnit.Framework;
using System.Reflection;

namespace Kaazing.K3po.NUnit
{
    /// <summary>
    /// A K3poRule specifies how a Test using k3po is executed.
    /// </summary>
    public class K3poRule
    {
        private Latch latch;
        private string scriptRoot;
        private Uri controlURL = new Uri("tcp://localhost:11642");
        private ScriptRunner _scriptRunner;
        private Task<ScriptPair> _task;
        private int _timeout = 0;

        /// <summary>
        /// Allocates a new K3poRule.
        /// </summary>
        public K3poRule()
        {
            latch = new Latch();
        }

        ///
        /// <summary>
        /// Sets the ClassPath root of where to look for scripts when resolving them.
        /// </summary>
        /// <param name="scriptRoot">scriptRoot is a directory/package name of where to resolve scripts from.</param>
        /// <returns>an instance of K3poRule for convenience</returns>
        ///
        public K3poRule setScriptRoot(string scriptRoot)
        {
            if (scriptRoot.EndsWith(@"\"))
            {
                // remove last \ if exist
                this.scriptRoot = scriptRoot.Substring(0, scriptRoot.Length - 1);
            }
            else
            {
                this.scriptRoot = scriptRoot;
            }
            return this;
        }

        ///
        /// <summary>
        /// Sets the URI on which to communicate to the k3po driver.
        /// </summary>
        /// <param name="controlURI">the URI on which to connect</param>
        /// <returns>an instance of K3poRule for convenience</returns>
        ///
        public K3poRule setControlURI(Uri controlURI)
        {
            this.controlURL = controlURI;
            return this;
        }

        ///
        /// <summary>
        /// Sets the Timeout on which to communicate to the k3po driver.
        /// </summary>
        /// <param name="timeout"> milliseconds, default 0 (no timeout)</param>
        /// <returns>an instance of K3poRule for convenience</returns>
        /// 
        public K3poRule setTimeout(int timeout)
        {
            this._timeout = timeout;
            return this;
        }

        /// <summary>
        /// Starts the connects in the k3po script.  The accepts are implicitly started just prior to the test logic in the
        /// Specification.
        /// </summary>
        /// <param name="script">the k3po script for the test</param>
        public void Prepare(string script)
        {
            Prepare(new string[] { script });
        }

        /// <summary>
        /// Starts the connects in the k3po scripts.  The accepts are implicitly started just prior to the test logic in the
        /// Specification.
        /// </summary>
        /// <param name="scripts">the list of scripts for the test</param>
        public void Prepare(string[] scripts)
        {
            latch = new Latch();
 
            StartK3po(scripts);
            // wait for script ready to start
            latch.AwaitPrepared();
            Console.WriteLine("K3po script Prepared");
        }

        /// <summary>
        /// Wait a Barrier to be notified
        /// </summary>
        /// <param name="barrierName">name of the barrier</param>
        public void AwaitBarrier(string barrierName)
        {
            Assert.IsTrue(latch.IsPrepared, "K3po is not ready for this test.");
            _scriptRunner.AwaitBarrier(barrierName);
        }

        /// <summary>
        /// Notify barrier to fire.
        /// </summary>
        /// <param name="barrierName">name of the barrier</param>
        public void NotifyBarrier(string barrierName)
        {
            Assert.IsTrue(latch.IsPrepared, "K3po is not ready for this test.");
            _scriptRunner.NotifyBarrier(barrierName);
        }

        /// <summary>
        /// Send Start command to k3po engine.  The accepts are implicitly started just prior to the test logic in the
        /// Specification.
        /// </summary>
        public void Start()
        {
            Assert.IsTrue(latch.IsPrepared, "K3po is not ready for this test.");
            //_scriptRunner.Start();
            // wait for script ready to start
            latch.AwaitStartable();
            Console.WriteLine("K3po script Started");
        }

        /**
         * block call to await for the K3po threads to stop executing.
         */
        public void Finish()
        {
            Assert.IsFalse(latch.IsInitState, "K3po is not ready for this test.");
            if (_timeout > 0)
            {
                Finish(_timeout);
            }
            else
            {
                latch.AwaitFinished();
                Console.WriteLine("K3po scirpt finished");
            }
        }

        /// <summary>
        /// Block call to await for the K3po threads to stop executing.
        /// this call will abort K3po test on timeout
        /// </summary>
        /// <param name="timeout">millesecond to wait</param>
        public void Finish(int timeout)
        {
            Assert.IsFalse(latch.IsInitState, "K3po is not ready for this test.");
            if (latch.AwaitFinished(timeout))
            {
                Console.WriteLine("K3po scirpt finished");
            }
            else
            {
                Console.WriteLine("K3po scirpt timeout, abort now");
                Abort();
            }
        }

        /// <summary>
        /// Sends Abort to K3po engine to abort current test
        /// K3po responses with finished event
        /// </summary>
        public void Abort()
        {
            Assert.IsFalse(latch.IsInitState, "K3po is not ready for this test.");
            _scriptRunner.Abort();
            latch.AwaitFinished();
            Console.WriteLine("K3po scirpt aborted");
        }

        /// <summary>
        /// Sends Dispose to K3po engine to closes all open connections
        /// </summary>
        public void Dispose()
        {
            _scriptRunner.Dispose();
            latch.AwaitDisposed();
        }

        /// <summary>
        /// the test is finished 
        /// </summary>
        public bool IsFinished
        {
            get { return latch.IsFinished; }
        }

        /// <summary>
        /// K3po has exception during the test
        /// </summary>
        public bool HasException
        {
            get { return latch.HasException; }
        }

        /// <summary>
        /// the Result for the test, this value is available when IsFinished is true
        /// </summary>
        public ScriptPair Result
        {
            get
            {
                return _task.Result;
            }
        }
        
        private void StartK3po(string[] scripts)
        {
            if (!string.IsNullOrEmpty(scriptRoot))
            {
                for (int i = 0; i < scripts.Length; i++)
                {
                    scripts[i] = string.Format("{0}/{1}", scriptRoot, scripts[i]);
                }
            }
            _scriptRunner = new ScriptRunner(controlURL, scripts, latch);
            try
            {
                _task = Task.Factory.StartNew<ScriptPair>(() =>
                {
                    return _scriptRunner.StartTest();
                }, TaskCreationOptions.LongRunning);
            }
            catch (AggregateException e)
            {
                _scriptRunner.Dispose();
                throw e.InnerException;
            }
        }
    }
}