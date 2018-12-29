using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEditor
{
    public class EventDispatcher
    {
        private struct InternalEventData
        {
            public ThreadedEvent Method;
            public object[] Args;
        }

        private object KEY = new object();

        private List<InternalEventData> pendingData = new List<InternalEventData>();

        public void DispatchEvents()
        {
            lock (KEY)
            {
                foreach (var item in pendingData)
                {
                    item.Method.Invoke(item.Args);
                }
                pendingData.Clear();
            }
        }

        public void AddNew(ThreadedEvent method, params object[] args)
        {
            var newItem = new InternalEventData() { Method = method, Args = args };
            lock (KEY)
            {
                pendingData.Add(newItem);
            }
        }
    }

    public delegate void ThreadedEvent(object[] args);
}
