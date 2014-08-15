using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Kaazing.Robot.Control.Event
{
    public abstract class CommandEvent
    {
        public enum Kind
        {
            PREPARED,
            STARTED,
            ERROR,
            FINISHED
        }

        private string _name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public abstract Kind EventKind
        {
            get;
        }

    }
}
