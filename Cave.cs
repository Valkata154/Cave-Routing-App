/*
 * Usage: Cave Class used to create a Cave object.
 * Created by: Valeri Vladimirov 40399682
 * Last modified: 29.10.2020
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CavernRoutingApp
{
    class Cave
    {
        public int CaveId { get; set; }
        public Cave ParentCave { get; set; }
        public double F { get; set; }
        public double G { get; set; }
        public double H { get; set; }
        public List<int> Connections { get; set; }
        public Tuple<int, int> Coords { get; set; }
    }
}
