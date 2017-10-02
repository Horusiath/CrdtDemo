using System;
using System.Linq;
using Sprache;

namespace CrdtDemo
{
    public interface IExpr
    {
        Symbol Symbol { get; }
        string Key { get; }
    }

    public enum Symbol
    {
        ADD,
        REM,
        GET
    }

    public sealed class SetExpr : IExpr
    {
        public Symbol Symbol { get; }
        public string Key { get; }
        public string Value { get; }

        public SetExpr(Symbol symbol, string key, string value)
        {
            Symbol = symbol;
            Key = key;
            Value = value;
        }
    }

    public sealed class QueryExpr : IExpr
    {
        public Symbol Symbol { get; }
        public string Key { get; }

        public QueryExpr(Symbol symbol, string key)
        {
            Symbol = symbol;
            Key = key;
        }
    }

    public static class CommandParser
    {

        private static readonly Parser<IExpr> _expr;

        static CommandParser()
        {
            var add = Parse.String("ADD").Text();
            var rem = Parse.String("REM").Text();
            var get = Parse.String("GET").Text();
            var setSym = new[] { add, rem }
                .Aggregate(Parse.Or);

            var str =
            (from open in Parse.Char('\'')
                from content in Parse.CharExcept('\'').Many().Text()
                from close in Parse.Char('\'')
                select content).Token();
            
            var setCmd =
            (from symbol in setSym
                from key in str
                from value in str
                select (IExpr)new SetExpr(
                    symbol: (Symbol)Enum.Parse(typeof(Symbol), symbol),
                    key: key,
                    value: value));
            
            var query =
            (from symbol in get
                from key in str
                select (IExpr)new QueryExpr(
                    symbol: (Symbol)Enum.Parse(typeof(Symbol), symbol),
                    key: key));

            _expr = Parse.Or(setCmd, query);
        }

        public static IExpr ParseLine(string text)
        {
            return _expr.Parse(text);
        }
    }
}