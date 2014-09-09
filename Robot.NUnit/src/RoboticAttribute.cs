/**
 * Copyright (c) 2007-2013, Kaazing Corporation. All rights reserved.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Core;
using System.IO;
using Kaazing.Robot.Control;

namespace Kaazing.Robot.NUnit
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RoboticAttribute : CategoryAttribute, ITestAction
    {
        private string _script;
        private RoboticLatch _latch;
        private ScriptRunner _scriptRunner;

        public RoboticAttribute()
        {
            categoryName = "Robotic";
            _latch = new RoboticLatch();
        }

        /// <summary>
        /// Script file name excluding extension.
        /// The script file shold be of .rpt extension.
        /// </summary>
        public string Script
        {
            get { return _script; }
            set { _script = value; }
        }

        public void AfterTest(TestDetails testDetails)
        {
            if (TestContext.CurrentContext.Result.Status == TestStatus.Failed)
            {
                _scriptRunner.Abort();
            }
            _scriptRunner.Join();
            try
            {
                Assert.AreEqual(_scriptRunner.ExpectedScript, _scriptRunner.ObservedScript, "Robotic behavior did not match expected");
            }
            catch (AssertionException exception)
            {
                Console.WriteLine(exception.Message);
                throw exception;
            }
            
        }

        public void BeforeTest(TestDetails testDetails)
        {
            // Read the script file content
            string expectedScript = File.ReadAllText("Scripts\\" + _script + ".rpt");
            _scriptRunner = new ScriptRunner(_script, expectedScript, _latch);

            // Start the script execution
            _scriptRunner.Start();

            // Wait until all binds ready for incoming connections from actual test
            _latch.AwaitStartable();
        }

        public ActionTargets Targets
        {
            get 
            { 
                return ActionTargets.Test; 
            }
        }
    }
}
