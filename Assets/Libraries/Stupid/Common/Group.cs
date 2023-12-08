using System;
using System.Collections.Generic;

namespace Stupid {
    public class Group<T> : IDisposable where T : new() {
        public List<Group<T>> Instances => instances;

        private List<Group<T>> instances = new List<Group<T>>();

        public Group() {
            instances.Add(this);
        }

        public void Dispose() {
            instances.Remove(this);
        }
    }
}
