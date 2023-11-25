using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace BugBusters_GRPC_Client
{
    public class A_star
    {

        List<List<Node>> Grid;
        int GridX
        {
            get
            {
                return Grid[0].Count;
            }
        }
        int GridY
        {
            get
            {
                return Grid.Count;
            }
        }

        public A_star(List<List<Node>> grid)
        {
            Grid = grid;
        }

        public Stack<Node> FindPath(Vector2 Start, Vector2 End)
        {
            Node start = new Node(new Vector2((int)(Start.X / Node.NODE_SIZE), (int)(Start.Y / Node.NODE_SIZE)), true);
            Node end = new Node(new Vector2((int)(End.X / Node.NODE_SIZE), (int)(End.Y / Node.NODE_SIZE)), true);

            Stack<Node> Path = new Stack<Node>();
            PriorityQueue<Node, float> OpenList = new PriorityQueue<Node, float>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            adjacencies = GetAdjacentNodes(current);
            foreach (var node in adjacencies) {
                if (node.Walkable) {
                    Console.WriteLine(node.Walkable);
                    Path.Push(node);
                    break;
                }
            }

            // add start node to Open List
            //OpenList.Enqueue(start, start.F);

            //while (OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            //{
            //    current = OpenList.Dequeue();
            //    ClosedList.Add(current);
            //    adjacencies = GetAdjacentNodes(current);

            //    foreach (Node n in adjacencies) //ez itt gebaszos
            //    {
            //        if (!ClosedList.Contains(n) && n.Walkable)
            //        {
            //            bool isFound = false;
            //            foreach (var oLNode in OpenList.UnorderedItems)
            //            {
            //                if (oLNode.Element == n)
            //                {
            //                    isFound = true;
            //                }
            //            }
            //            if (!isFound)
            //            {
            //                n.Parent = current;
            //                n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
            //                n.Cost = n.Weight + n.Parent.Cost;
            //                OpenList.Enqueue(n, n.F);
            //            }
            //        }
            //    }
            //}

            // construct path, if end was not closed return null
            //if (!ClosedList.Exists(x => x.Position == end.Position))
            //{
            //    return null;
            //}

            //// if all good, return path
            //Node temp = ClosedList[ClosedList.IndexOf(current)];
            //if (temp == null) return null;
            //do
            //{
            //    Path.Push(temp);
            //    temp = temp.Parent;
            //} while (temp != start && temp != null);
            return Path;
        }

        private List<Node> GetAdjacentNodes(Node n)
        {
            List<Node> temp = new List<Node>();

            int X = (int)n.Position.X;
            int Y = (int)n.Position.Y;

            if (X + 1 < GridX) {
                temp.Add(Grid[X + 1][Y]); //1
                if (Y - 1 >= 0 ) {
                    temp.Add(Grid[X + 1][Y - 1]); //2
                }
                if (Y + 1 < GridY) {
                    temp.Add(Grid[X + 1][Y + 1]); //3
                }               
            }
            if (X - 1 >= 0) {
                temp.Add(Grid[X - 1][Y]); //4
                if (Y - 1 >= 0) {
                    temp.Add(Grid[X - 1][Y - 1]); //5
                }
                if (Y + 1 < GridY) {
                    temp.Add(Grid[X - 1][Y + 1]); //6
                }
            }
            if (Y - 1 >= 0) {
                temp.Add(Grid[X][Y - 1]); //7
            }
            if (Y + 1 < GridY) {
                temp.Add(Grid[X][Y + 1]); //8
            }
            return temp;
        }
    }
}
