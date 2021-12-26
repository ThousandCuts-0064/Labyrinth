using System;
using System.Collections.Generic;
using System.IO;

namespace Labyrinth
{
    class Program
    {
        static void Main()
        {
            List<Path> explorationList = new List<Path>();
            int[,] labyrinth;
            int n;

            using (StreamReader streamReader = File.OpenText("lab1.txt"))
            {
                n = int.Parse(streamReader.ReadLine());
                labyrinth = new int[n, n];
                int startX = int.Parse(streamReader.ReadLine());
                int startY = int.Parse(streamReader.ReadLine());
                int endX = int.Parse(streamReader.ReadLine());
                int endY = int.Parse(streamReader.ReadLine());
                Path.SetStartAndEndLocation(new Location(startY, startX), new Location(endY, endX));
                explorationList.Add(Path.Start);

                for (int i = 0; i < n; i++)
                {
                    string[] chars = streamReader.ReadLine().Split(' ');
                    for (int y = 0; y < chars.Length; y++)
                    {
                        labyrinth[i, y] = int.Parse(chars[y]);
                    }
                }
            }

            while (explorationList.Count > 0)
            {
                Path path = explorationList[explorationList.Count - 1];
                explorationList.RemoveAt(explorationList.Count - 1);

                if (ExplorePath(path, path.Location.Up())) break;
                if (ExplorePath(path, path.Location.Down())) break;
                if (ExplorePath(path, path.Location.Right())) break;
                if (ExplorePath(path, path.Location.Left())) break;

                explorationList.Sort((curr, next) => next.DistanceSum - curr.DistanceSum);
            }

            if (Path.End.Entrance == null)
            {
                Console.WriteLine("NO PATH");
            }
            else
            {
                Path trace = Path.End;
                List<Path> traceList = new List<Path>() { Path.End };
                while (trace.Entrance != null)
                {
                    trace = trace.Entrance;
                    traceList.Add(trace);
                }
                traceList.Reverse();

                foreach (var path in traceList)
                {
                    Console.Write($"({path.Location.Column}, {path.Location.Row}) -> ");
                }
                Console.Write("EXIT");
            }

            // Return true if the current path can connect to the end path.
            // Otherwise returns false and tries to add new path to the exploration list.
            bool ExplorePath(Path current, Location check)
            {
                if (check.Row < 0 ||
                    check.Row >= n ||
                    check.Column < 0 ||
                    check.Column >= n ||
                    labyrinth[check.Row, check.Column] == 1)
                    return false;

                if (Path.TryGetOnLocation(check, out Path existing))
                {
                    if (existing == Path.End)
                    {
                        Path.End.SetEntrance(current);
                        return true;
                    }

                    if (current.DistanceFromStart < existing.DistanceFromStart)
                        existing.SetEntrance(current);
                }
                else explorationList.Add(new Path(current, check));

                return false;
            }
        }
    }

    readonly struct Location
    {
        public int Row { get; }
        public int Column { get; }

        public Location(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public Location Up() => new Location(Row - 1, Column);
        public Location Down() => new Location(Row + 1, Column);
        public Location Right() => new Location(Row, Column + 1);
        public Location Left() => new Location(Row, Column - 1);
    }

    class Path
    {
        private static readonly Dictionary<Location, Path> locationToPath = new Dictionary<Location, Path>();
        public static Path Start { get; private set; }
        public static Path End { get; private set; }
        public Path Entrance { get; private set; }
        public Location Location { get; }
        public int DistanceFromEnd { get; }
        public int DistanceFromStart { get; private set; }
        public int DistanceSum { get; private set; }

        private Path(Location location, int distanceFromStart, int distanceFromEnd)
        {
            Location = location;
            locationToPath.Add(location, this);
            DistanceFromEnd = distanceFromEnd;
            DistanceFromStart = distanceFromStart;
        }

        public Path(Path entrance, Location location)
        {
            Location = location;
            locationToPath.Add(location, this);
            DistanceFromEnd = Math.Abs(End.Location.Column - location.Column) + Math.Abs(End.Location.Row - location.Row);
            SetEntrance(entrance);
        }

        public void SetEntrance(Path entrance)
        {
            Entrance = entrance;
            DistanceFromStart = entrance.DistanceFromStart + 1;
            DistanceSum = DistanceFromStart + DistanceFromEnd;
        }

        public static void SetStartAndEndLocation(Location start, Location end)
        {
            int distance = Math.Abs(end.Column - start.Column) + Math.Abs(end.Row - start.Row);
            Start = new Path(start, 0, distance);
            End = new Path(end, distance, 0);
        }

        public static bool TryGetOnLocation(Location location, out Path path) => locationToPath.TryGetValue(location, out path);
    }
}
