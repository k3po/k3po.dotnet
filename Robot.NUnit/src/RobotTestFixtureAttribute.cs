using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaazing.Robot.NUnit
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RobotTestFixtureAttribute : Attribute
    {
        private string _scriptRoot;

        public string ScriptRoot
        {
            get { return _scriptRoot; }
            set { _scriptRoot = value; }
        }

    }
}
