using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JBlam.NetflixScrape.Core
{
    class BufferedEventSource<TEventArgs>
    {
        // buffers events if no handlers are attached
        
        readonly Queue<(object, TEventArgs)> buffer = new Queue<(object, TEventArgs)>();
        public void SendOrBuffer(object sender, TEventArgs args)
        {
            var evt = Inner;
            if (evt == null)
            {
                buffer.Enqueue((sender, args));
            }
            else
            {
                evt(sender, args);
            }
        }
        public event EventHandler<TEventArgs> Event
        {
            add
            {
                Inner += value;
                while (buffer.Any())
                {
                    var (sender, args) = buffer.Dequeue();
                    value(sender, args);
                }
            }
            remove { Inner -= value; }
        }
        event EventHandler<TEventArgs> Inner;
    }
}
