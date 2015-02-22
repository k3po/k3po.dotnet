using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaazing.K3po.NUnit
{
    public class ScriptPair
    {
        private string _observedScript;
        private string _expectedScript;

        public string ObservedScript
        {
            get { return _observedScript; }
            set { _observedScript = value; }
        }

        public string ExpectedScript
        {
            get { return _expectedScript; }
            set { _expectedScript = value; }
        }
    }
}
