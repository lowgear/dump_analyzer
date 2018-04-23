using System;
using System.Collections.Generic;
using System.Linq;

namespace DmpAnalyze.Utils
{
    public static class Graphs
    {
        public static IEnumerable<IEnumerable<T>> FindCycles<T>(IEnumerable<T> nodes,
            Func<T, IEnumerable<T>> getNeigbours, IEqualityComparer<T> comparer)
        {
            var notUsedNodes = new HashSet<T>(nodes, comparer);
            var stack = new Stack<Tuple<T, IEnumerator<T>>>();
            var stackSet = new HashSet<T>(comparer);
            while (notUsedNodes.Count > 0)
            {
                var root = notUsedNodes.First();
                notUsedNodes.Remove(root);
                AddNodeToStack(root, getNeigbours, notUsedNodes, stack, stackSet);

                while (stack.Count > 0)
                {
                    var enumerator = stack.Peek().Item2;
                    if (enumerator.MoveNext())
                    {
                        var neighbour = enumerator.Current;
                        if (stackSet.Contains(neighbour))
                            yield return ExtractCycle(neighbour, stack, comparer);

                        AddNodeToStack(neighbour, getNeigbours, notUsedNodes, stack, stackSet);
                    }
                    else
                        PopFromStack(stackSet, stack);
                }
            }
        }

        private static void PopFromStack<T>(HashSet<T> stackSet, Stack<Tuple<T, IEnumerator<T>>> stack)
        {
            stackSet.Remove(stack.Peek().Item1);
            stack.Pop();
        }

        private static IEnumerable<T> ExtractCycle<T>(T neighbour, Stack<Tuple<T, IEnumerator<T>>> stack, IEqualityComparer<T> comparer)
        {
            return stack
                .Select(t => t.Item1)
                .TakeWhile(n => !comparer.Equals(n, neighbour))
                .Concat(new[] {neighbour});
        }

        private static void AddNodeToStack<T>(T node, Func<T, IEnumerable<T>> getNeigbours, ICollection<T> notUsedNodes, Stack<Tuple<T, IEnumerator<T>>> stack, ISet<T> stackSet)
        {
            if (!notUsedNodes.Remove(node)) return;
            stack.Push(Tuple.Create(node, getNeigbours(node).GetEnumerator()));
            stackSet.Add(node);
        }
    }
}