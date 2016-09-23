using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using MoreLinq;

namespace Utility
{
     public class Pos
        {
            public Pos(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
            public double x;
            public double y;
            public static Pos operator +(Pos p1, Pos p2)
            {
                return new Pos(p1.x + p2.x, p1.y + p2.y);
            }
            public static Pos operator -(Pos p1, Pos p2)
            {
                return new Pos(p1.x - p2.x, p1.y - p2.y);
            }
            public static Pos operator /(Pos p1, double d)
            {
                return new Pos(p1.x / d, p1.y / 2);
            }
            public static implicit operator Vector2(Pos p)
            {
                return new Vector2((float)p.x, (float)p.y);
            }
            public override string ToString(){
                return this.x+ " " +this.y;
            }
        }

    public static class StringParser
    {
       
        public static Pos parsePos(string pos)
        {
            string[] posArr = pos.Split(' ');
            return parseVec2(posArr);
        }
        private static Pos parseVec2(string[] posArr)
        {
            return new Pos(parseFloat(posArr[0]), parseFloat(posArr[1]));
        }
        private static double parseFloat(string f)
        {
            return double.Parse(f);
        }

        public static IEnumerable<Pos> parsePosList(string posList)
        {
            return posList.Split(' ').Batch(2).Select(posArr => parseVec2(posArr.ToArray()));

        }
    }
}

