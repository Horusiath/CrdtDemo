using System;
using Akka.Actor;
using Akka.Configuration;

namespace Lighthouse
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                  loglevel = INFO
                  log-config-on-start = on
                  actor.provider = cluster
                  remote.dot-netty.tcp {
                      hostname = localhost
                      port = 5000
                  }
                  cluster {
                      seed-nodes = [ ""akka.tcp://crdt-demo@localhost:5000/"" ]
                      # auto-down-unreachable-after = 10s
                      roles = [ ""lighthouse"" ]
                  }
                }");

            using (var system = ActorSystem.Create("crdt-demo", config))
            {
                Console.ReadLine();
            }
        }
    }
}
