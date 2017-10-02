using Akka.Actor;
using Akka.DistributedData;

namespace CrdtDemo
{
    public class Listener : ReceiveActor
    {
        public Listener()
        {
            var ddata = DistributedData.Get(Context.System);
        }
    }
}