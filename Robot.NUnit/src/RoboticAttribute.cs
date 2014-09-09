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
using System.Reflection;

namespace Kaazing.Robot.NUnit
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RoboticAttribute : CategoryAttribute, ITestAction
    {
        private string _script;
        private ScriptRunner _scriptRunner;

        public RoboticAttribute()
        {
            categoryName = "Robotic";
        }

        /// <summary>
        /// Script file name excluding extension.
        /// The script file shold be of .rpt extension.
        /// </summary>
        public string Script
        {
            get 
            { 
                return _script; 
            }

            set 
            {
                _script = Path.ChangeExtension(value, null); ; 
            }
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
            RobotTestFixtureAttribute fixtureAttribute = Attribute.GetCustomAttribute(testDetails.Method.DeclaringType, typeof(RobotTestFixtureAttribute)) as RobotTestFixtureAttribute;
            String scriptName = String.Empty;
            if (fixtureAttribute == null || String.IsNullOrEmpty(fixtureAttribute.ScriptRoot))
            {
                string baseDirectory = Path.GetFullPath("Scripts");
                scriptName = Path.Combine(baseDirectory, _script);
                scriptName = scriptName.Replace("\\", "/");
            }
            else
            {
                scriptName = String.Format("{0}/{1}", fixtureAttribute.ScriptRoot, _script);
            }

            _scriptRunner = new ScriptRunner(scriptName);

            // Start the script execution
            _scriptRunner.Start();
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
