
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
    public class K3poRule
    {
        private Latch latch;
        private string scriptRoot;
        private Uri controlURL = new Uri("tcp://localhost:11642");
        private ScriptRunner _scriptRunner;
        private Task<ScriptPair> _task;
        private int _timeout = 10000;

        /**
         * Allocates a new K3poRule.
         */

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
        /// <param name="timeout"> milliseconds, default 10 second</param>
        /// <returns>an instance of K3poRule for convenience</returns>
        /// 
        public K3poRule setTimeout(int timeout)
        {
            this._timeout = timeout;
            return this;
        }

        /// <summary>
        ///Starts the connects in the k3po scripts.  The accepts are implicitly started just prior to the test logic in the
        /// Specification.
        /// </summary>
        /// <param name="scripts"></param>
        public void Prepare(string[] scripts)
        {
            latch = new Latch();
 
            StartK3po(scripts);
            // wait for script ready to start
            latch.AwaitPrepared();
            Console.WriteLine("K3po script Prepared");
        }


 /// <summary>
///Send Start command to k3po engine.  The accepts are implicitly started just prior to the test logic in the
 /// Specification.
 /// </summary>

        public void Start()
        {

            _scriptRunner.Start();
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
            latch.AwaitFinished();
            Console.WriteLine("K3po scirpt finished");
        }

        /// <summary>
        /// block call to await for the K3po threads to stop executing.
        /// </summary>
        /// <param name="timeout">millesecond to wait</param>
        /// <returns>ture if script finished, false if timeout</returns>
        public bool Finish(int timeout)
        {
            Assert.IsFalse(latch.IsInitState, "K3po is not ready for this test.");
            bool ret = latch.AwaitFinished(timeout);
            Console.WriteLine("K3po scirpt finished");
            return ret;
        }

        public void Abort()
        {
            Assert.IsFalse(latch.IsInitState, "K3po is not ready for this test.");
            _scriptRunner.Abort();
            latch.AwaitFinished();
            Console.WriteLine("K3po scirpt aborted");
        }

        public void Dispose()
        {
            _scriptRunner.Dispose();
            latch.AwaitDisposed();
        }

        public bool IsFinished
        {
            get { return latch.IsFinished; }
        }

        public bool HasException
        {
            get { return latch.HasException; }
        }

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
                    return _scriptRunner.Start();
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