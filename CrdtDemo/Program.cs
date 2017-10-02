using System;
using Akka.Actor;
using Akka.Configuration;

namespace CrdtDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                  actor.provider = cluster
                  #log-config-on-start = on
                  loglevel = WARNING
                  suppress-json-serializer-warning = on
                  remote {
                      # log-received-messages = on
                      # log-sent-messages = on
                      dot-netty.tcp {
                      hostname = localhost
                      port = 0
                    }
                  }
                  cluster {
                    seed-nodes = [ ""akka.tcp://crdt-demo@localhost:5000/"" ]
                    #auto-down-unreachable-after = 10s
                    roles = [""crdt""]
                    distributed-data {
                        role = ""crdt""
                    }
                  }
                }")
                .WithFallback(Akka.DistributedData.DistributedData.DefaultConfig());
            using (var system = ActorSystem.Create("crdt-demo", config))
            {
                Console.WriteLine("===Commands===\n\n* ADD '{key}' '{value}' - upserts value to an ORSet\n* REM '{key}' '{value}' - removes value from an ORSet\n* GET '{key}' - gets an ORSet under provided key\n\n==============\n");

                var app = new App(system);

                app.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                Console.ReadLine();
            }
        }
    }
}
