using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using MoreLinq;

namespace Utility
{
    

    public static class StringParser
    {
        public static string EnumerableString<T>(IEnumerable<T> enumerable){
            return string.Join(", ", enumerable.Select(s=>s.ToString()).ToArray());
        }
       
        public static Vector2d parsePos(string pos)
        {
            string[] posArr = pos.Split(' ');
            return parseVec2(posArr);
        }
        private static Vector2d parseVec2(string[] posArr)
        {
            return new Vector2d(parseDouble(posArr[0]), parseDouble(posArr[1]));
        }
        private static double parseDouble(string f)
        {
            return double.Parse(f);
        }

        public static IEnumerable<Vector2d> parsePosList(string posList)
        {
            return posList.Split(' ').Batch(2).Select(posArr => parseVec2(posArr.ToArray()));

        }
    }
}

