
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
using NUnit.Framework;
using System.IO;
using System.Reflection;
using NUnit.Framework.Interfaces;

namespace Kaazing.K3po.NUnit
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SpecificationAttribute : Attribute, ITestAction
    {
        private string[] _scripts;

        public SpecificationAttribute(string script)
        {
            _scripts = new string[] { script };
            //categoryName = "K3PO";
        }

        public SpecificationAttribute(params string[] scripts)
        {
            _scripts = scripts;
            //categoryName = "K3PO";
        }

        public string[] Scripts
        {
            get { return _scripts; }
        }

        public void BeforeTest(ITest test)
        {
            Console.WriteLine("BeforeTest");
            K3poRule k3poRule = GetK3poRule(test);
            k3poRule.Prepare(_scripts);
        }

        public void AfterTest(ITest test)
        {
            Console.WriteLine("AfterTest");
            K3poRule k3poRule = GetK3poRule(test);
            try
            {
                if (k3poRule.IsFinished)
                {
                    if (!k3poRule.HasException)
                    {
                        Assert.AreEqual(k3poRule.Result.ExpectedScript, k3poRule.Result.ObservedScript, "Script not Match");
                    }
                }
                else
                {
                    // timeout, abort k3po
                    k3poRule.Abort();
                    try
                    {
                        Assert.AreEqual(k3poRule.Result.ExpectedScript, k3poRule.Result.ObservedScript, "Test Timeout!!!");
                    }
                    catch (Exception assertException)
                    {
                        throw new TimeoutException("Test timeout!!!", assertException);
                    }
                    throw new TimeoutException("Test timeout!!!");
                }
            }
            finally
            {
                //k3poRule.Dispose();
            }
        }

        private K3poRule GetK3poRule(ITest test)
        {
            // get K3poRule instance
            K3poTestFixtureAttribute fixtureAttribute = Attribute.GetCustomAttribute(test.GetType(), typeof(K3poTestFixtureAttribute)) as K3poTestFixtureAttribute;
            string K3poPropertyName = (fixtureAttribute == null || String.IsNullOrEmpty(fixtureAttribute.K3poRulePropertyName)) ? "k3po" : fixtureAttribute.K3poRulePropertyName;

            FieldInfo k3poField = test.Fixture.GetType().GetField(K3poPropertyName);

            if(k3poField != null)
            {
                return (K3poRule)k3poField.GetValue(test.Fixture);
            }
            return null;
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
