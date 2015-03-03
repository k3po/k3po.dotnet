using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.K3po.NUnit
{
    public class K3poTestFixtureAttribute : TestFixtureAttribute
    {
        private string _scriptRoot;

        public string ScriptRoot
        {
            get { return _scriptRoot; }
            set { _scriptRoot = value; }
        }
    }
}
