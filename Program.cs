/*
 * Usage: Cave routing app used to find the quickest route to the
 * final destination cave using the A* Search Algorithm 
 * Created by: Valeri Vladimirov 40399682
 * Last modified: 09.11.2020
 * 
 * How to use:
 * 1. Open bin/debug/netcoreapp3.1 in the cmd
 * 2. And type: caveroute + "the file you want to find a route in"
 * 3. Example: caveroute input1
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CavernRoutingApp
{
    class Program
    {
        public static List<Cave> caveList = new List<Cave>();

        static void Main(string[] args)
        {
            string file = ExtCheck(args[0]);
            string data = ProcessPath(Load(file));
            OutputToFile(file, data);
        }

        // Method to check if the file has the appropriate extension (.cav).
        public static String ExtCheck(string file)
        {
            if (!file.ToLower().EndsWith(".cav"))
                file = String.Concat(file, ".cav");
            return file;
        }

        // Method to check if the file exists
        public static Boolean FileCheck(string file)
        {
            if (!File.Exists(file))
                return false;
            return true;
        }

        // Method to read the data from the .cav file using a StreamReader
        public static List<int> Load(string file)
        {
            string numbersString;
            using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
            {
                numbersString = streamReader.ReadToEnd();
            }
            List<string> numbersList = new List<string>(numbersString.Split(','));
            return numbersList.ConvertAll(int.Parse);
        }

        // Method to find if there is a path
        // And if there is, to ensure that it is the quickest possible.
        public static string ProcessPath(List<int> numbers)
        {
            int caveNum = numbers[0];
            int coordsNum = caveNum * 2;

            // Add Caves with their coordinates to the caves list
            int caveId = 1;
            for (int i = 0; i < coordsNum; i++)
            {
                caveList.Add(new Cave { CaveId = caveId, Coords = new Tuple<int, int>(numbers[i + 1], numbers[i + 2]) });
                i++; caveId++;
            }

            // Create the matrix
            int connectionsStart = coordsNum + 1;
            int n = 1;
            int connectionsNum = caveNum * caveNum;
            for (int i = 0; i < connectionsNum; i++)
            {
                if (caveNum < n)
                    n = 1;
                if (caveList[n - 1].Connections != null)
                    caveList[n - 1].Connections.Add(numbers[connectionsStart + i]);
                else
                    caveList[n - 1].Connections = new List<int> { numbers[connectionsStart + i] };
                n++;
            }

            // Set the variables for the algorithm
            Cave currentCave = null;
            Cave lastCave = caveList.FirstOrDefault(cav => cav.CaveId == caveNum);
            List<Cave> openList = new List<Cave>();
            List<Cave> closedList = new List<Cave>();
            double g = 0;
            bool pathFound = false;

            // Add the first position and start the A* search algorithm
            openList.Add(caveList.FirstOrDefault(cav => cav.CaveId == 1));
            while (openList.Count > 0)
            {
                // Get the lowest F score Cave
                double lowest = openList.Min(cav => cav.F);
                currentCave = openList.First(cav => cav.F == lowest);

                // Add the currentCave to the closed list and remove it from the open list
                closedList.Add(currentCave);
                openList.Remove(currentCave);

                // If the last cave is added to the closed list that means a path has been found
                if (closedList.FirstOrDefault(cav => cav.CaveId == lastCave.CaveId) != null)
                {
                    pathFound = true;
                    break;
                }

                // Get the list of cave connections 
                List<int> connected = new List<int>();
                for (int i = 0; i < currentCave.Connections.Count; i++)
                {
                    if (currentCave.Connections[i] == 1)
                        connected.Add(i + 1);
                }
                List<Cave> caveConnections = GetCaveConnections(connected);

                // Get the adjancent Cave's and add to the openList if needed
                foreach (Cave cave in caveConnections)
                {
                    // Distance from the starting point
                    g = currentCave.G + EuclideanDistance(currentCave.Coords, cave.Coords);
                    // If the neighbour id is already on the closed list then ignore it
                    if (closedList.FirstOrDefault(c => c.CaveId == cave.CaveId) != null)
                        continue;

                    // If the adjancent cave id is not in the open list
                    if (openList.FirstOrDefault(cav => cav.CaveId == cave.CaveId) == null)
                    {
                        // Set score, parent and insert it
                        cave.H = EuclideanDistance(cave.Coords, lastCave.Coords);
                        cave.G = g;
                        cave.F = cave.G + cave.H;
                        cave.ParentCave = currentCave;
                        openList.Insert(0, cave);
                    }
                    // If it is in the open list
                    else
                    {
                        // Compare routes and if it is quicker change parent cave
                        if (g + cave.H < cave.F)
                        {
                            cave.G = g;
                            cave.F = cave.G + cave.H;
                            cave.ParentCave = currentCave;
                        }
                    }
                }
            }

            // If a path is found, go backwards to get it.
            string path = "";
            if (pathFound)
            {
                while (!(currentCave == null))
                {
                    path = currentCave.CaveId + " " + path;
                    currentCave = currentCave.ParentCave;
                }
                path = path.TrimEnd(' ');
            }
            // Path is 0, meaning there is no path.
            else
                path = "0";

            return path;
        }

        // Method to get the Cave for each ID 
        public static List<Cave> GetCaveConnections(List<int> caveConnectionList)
        {
            List<Cave> caveConnections = new List<Cave>();
            for (int i = 0; i < caveConnectionList.Count; i++)
                caveConnections.Add(caveList[caveConnectionList[i] - 1]);
            return caveConnections;
        }

        // Method to get the EuclideanDistance between two coordinates
        public static double EuclideanDistance(Tuple<int, int> a, Tuple<int, int> b)
        {
            return Math.Sqrt(Math.Pow((b.Item1 - a.Item1), 2) + Math.Pow((b.Item2 - a.Item2), 2));
        }

        // Method to write the path to a csn file.
        public static void OutputToFile(string file, string path)
        {
            string fileCsn = Path.GetFileNameWithoutExtension(file) + ".csn";
            File.WriteAllText(fileCsn, path);
        }
    }
}
