using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.DistributedData;
using static Akka.DistributedData.Dsl;

namespace CrdtDemo
{
    public class App
    {
        private readonly ActorSystem system;
        private readonly Cluster cluster;
        private readonly DistributedData ddata;
        private readonly IReadConsistency readConsistency;
        private readonly IWriteConsistency writeConsistency;
        private readonly TimeSpan timeout;

        public App(ActorSystem system)
        {
            this.system = system;
            this.cluster = Cluster.Get(system);
            this.ddata = DistributedData.Get(system);
            this.timeout = TimeSpan.FromSeconds(10);
            this.readConsistency = new ReadMajority(timeout);
            this.writeConsistency = new WriteMajority(timeout);
        }

        public async Task RunAsync()
        {
            while (true)
            {
                try
                {
                    var line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var expr = CommandParser.ParseLine(line);
                    Task.Run(() =>
                    {
                        switch (expr)
                        {
                            case SetExpr _: return ExecuteSetExpr((SetExpr) expr);
                            case QueryExpr _: return ExecuteQueryExpr((QueryExpr) expr);
                            default: return Task.CompletedTask;
                        }
                    }, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task ExecuteQueryExpr(QueryExpr expr)
        {
            var key = new ORSetKey<string>(expr.Key);

            try
            {
                var reply = await ddata.GetAsync(key, readConsistency);
                Console.WriteLine($"GET '{key}': {reply?.ToString() ?? "null" }");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't receive GET for {key}: {e}");
            }
        }

        private async Task ExecuteSetExpr(SetExpr expr)
        {
            var key = new ORSetKey<string>(expr.Key);

            try
            {
                var set = await ddata.GetAsync(key) ?? ORSet<string>.Empty;
                switch (expr.Symbol)
                {
                    case Symbol.ADD: set = set.Add(cluster, expr.Value); break;
                    case Symbol.REM: set = set.Remove(cluster, expr.Value); break;
                    default: throw new NotSupportedException($"Operation {expr.Symbol} not supported on ORSet");
                }

                await ddata.UpdateAsync(key, set, writeConsistency);
                var value = await ddata.GetAsync(key);
                Console.WriteLine($"Updated '{key}': {value}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't perform UPDATE for {key}: {e}");
            }
        }
    }
}