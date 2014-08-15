using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Kaazing.Robot.Control.Event
{
    public class FinishedEvent : CommandEvent
    {
        private string _observedScript;
        private string _expectedScript;

        [JsonProperty(PropertyName = "expected_script")]
        public string ExpectedScript
        {
            get { return _expectedScript; }
            set { _expectedScript = value; }
        }

        [JsonProperty(PropertyName = "observed_script")]
        public string ObservedScript
        {
            get { return _observedScript; }
            set { _observedScript = value; }
        }

        public override CommandEvent.Kind EventKind
        {
            get { return Kind.FINISHED; }
        }
    }
}
