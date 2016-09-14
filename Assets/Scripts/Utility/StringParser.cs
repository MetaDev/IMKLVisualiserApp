using UnityEngine;
using System.Linq;

namespace Utility
{

	public static class StringParser 
	{

		public static Vector2 parsePos(string pos){
			string[] posArr = pos.Split (' ');
			return parseVec2 (posArr);
		}
		private static Vector2 parseVec2(string[] posArr){
			return new Vector2 (parseFloat( posArr[0]), parseFloat (posArr [1]));
		}
		private static float parseFloat(string f){
			return float.Parse (f);
		}
			
		public static Vector2[] parsePosList(string posList){
			return posList.Split (' ').Batch (2).Select (posArr => parseVec2 (posArr.ToArray ())).ToArray ();

		}
	}
}

