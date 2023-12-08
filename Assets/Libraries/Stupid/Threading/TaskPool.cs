using System;
using System.Linq;
using System.Threading.Tasks;

namespace Stupid {
    [Serializable]
    public class TaskPool {
        private const int DEFAULT_SIZE = 8;

        public TaskPool(int size = DEFAULT_SIZE) {
            tasks = new Task[size];
        }

        public bool Completed => tasks.All(t => t == null || t.IsCompleted);
        public int TaskHash => tasks.Where(t => t != null).Sum(t => t.Id);
        public int Size { get => tasks.Length; set => Resize(value); }

        private Task[] tasks;

        public void Resize(int size) {
            Array.Resize(ref tasks, size);
        }

        public void Clear() {
            tasks = new Task[tasks.Length];
        }

        public bool TryRun(Action action) {
            for (int t = 0; t < tasks.Length; t++) {
                if (tasks[t] != null && !tasks[t].IsCompleted) { continue; }

                tasks[t] = Task.Run(action);
                return true;
            }

            return false;
        }
    }
}