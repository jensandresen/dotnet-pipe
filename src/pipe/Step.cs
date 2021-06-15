using System;
using System.Collections.Generic;
using System.Linq;

namespace pipe
{
    public class Step : IEquatable<Step>
    {
        private readonly LinkedList<string> _actions;
        private readonly LinkedList<string> _preStepNames;
        
        public Step(string name) : this(name, Enumerable.Empty<string>())
        {
            
        }

        public Step(string name, IEnumerable<string> actions) : this(name, actions, Enumerable.Empty<string>())
        {
            
        }
        
        public Step(string name, IEnumerable<string> actions, IEnumerable<string> preStepNames)
        {
            Name = name;
            _actions = new LinkedList<string>(actions);
            _preStepNames = new LinkedList<string>(preStepNames);
        }
        
        public string Name { get; }
        
        public IEnumerable<string> Actions => _actions;
        public void AddAction(string action)
        {
            _actions.AddLast(action);
        }

        public IEnumerable<string> PreStepNames => _preStepNames;
        public void AddPreStepName(string preStepName)
        {
            _preStepNames.AddLast(preStepName);
        }

        public override string ToString()
        {
            var temp = _actions.Select((value, index) => $"{index + 1}:\"{value}\"");
            return $"{Name}={string.Join(", ", temp)}";
        }

        #region equality 
        
        public bool Equals(Step other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _actions.SequenceEqual(other._actions) && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Step) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_actions, Name);
        }

        public static bool operator ==(Step left, Step right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Step left, Step right)
        {
            return !Equals(left, right);
        }
        
        #endregion
    }
}