namespace HiveLib.SearchAlgorithms
{
    using HiveContracts;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class BreadthFirstSearch
    {
        public static bool CheckSingleHive(Dictionary<int,ITile> board, ITile start)
        {
            var frontier = new Queue<ITile>();
            frontier.Enqueue(start);

            var reached = new HashSet<Hex>();
            reached.Add(start.Location);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                Console.WriteLine("Visiting {0}", current);

                for(int i = 0; i < 6; i++)
                {
                    var neighbor = current.Location.Neighbor(i);
                    var hash = neighbor.GetHashCode();
                    if (board.ContainsKey(hash) && !reached.Contains(neighbor))
                    {
                        frontier.Enqueue(board[hash]);
                        reached.Add(neighbor);
                    }
                }
            }
            if (board.Count == reached.Count)
            {
                return true;
            }
            return false;
            //do we check if the reached equals the board ?
        }
    }
}
