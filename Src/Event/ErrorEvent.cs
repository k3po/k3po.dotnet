using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Kaazing.Robot.Control.Event
{
    public class ErrorEvent : CommandEvent
    {
        private string _summary;
        private string _description;

        [JsonProperty(PropertyName = "description")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [JsonProperty(PropertyName = "summary")]
        public string Summary
        {
            get { return _summary; }
            set { _summary = value; }
        }

        public override CommandEvent.Kind EventKind
        {
            get { return Kind.ERROR; }
        }
    }
}
