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
using System.Reflection;

namespace Kaazing.K3po.NUnit
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SpecificationAttribute : CategoryAttribute, ITestAction
    {
        private string[] _scripts;
        private ScriptRunner _scriptRunner;
        private Latch _latch = new Latch();

        public SpecificationAttribute(params string[] scripts)
        {
            _scripts = scripts;
            categoryName = "K3PO";
        }

        public void AfterTest(TestDetails testDetails)
        {
            // Test could fail due to assertion error or timeout or when preparing
            if (TestContext.CurrentContext.Result.Status == TestStatus.Failed)
            {
                // Abort the script execution
                _scriptRunner.Abort();
            }

            // wait until the script execution is completed
            _scriptRunner.Join();
            
            try
            {
                Assert.AreEqual(_scriptRunner.ScriptPair.ExpectedScript, _scriptRunner.ScriptPair.ObservedScript, "Robotic behavior did not match expected");
            }
            catch (AssertionException exception)
            {
                Console.WriteLine(exception.Message);
                throw exception;
            }
            
        }

        public void BeforeTest(TestDetails testDetails)
        {
            K3poTestFixtureAttribute fixtureAttribute = Attribute.GetCustomAttribute(testDetails.Method.DeclaringType, typeof(K3poTestFixtureAttribute)) as K3poTestFixtureAttribute;
            IList<string> scripts = new List<string>();
            foreach (string script in _scripts) {
                String scriptName = String.Empty;
                if (fixtureAttribute == null || String.IsNullOrEmpty(fixtureAttribute.ScriptRoot))
                {
                    scriptName = script;
                }
                else
                {
                    scriptName = String.Format("{0}/{1}", fixtureAttribute.ScriptRoot, script);
                }
                scripts.Add(scriptName);
            }


            _scriptRunner = new ScriptRunner(new Uri("tcp://localhost:11642"), scripts, _latch); 

            // Start the script execution
            _scriptRunner.Start();

            // wait until k3po server is ready to accept connections
            _latch.AwaitPrepared();
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
