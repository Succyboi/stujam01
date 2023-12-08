namespace Stupid {
#if UNITY_EDITOR || UNITY_STANDALONE
	public partial struct Vector3 {
		public static implicit operator UnityEngine.Vector3(Vector3 from) => new UnityEngine.Vector3(from.x, from.y, from.z);
		public static implicit operator Vector3(UnityEngine.Vector3 from) => new Vector3(from.x, from.y, from.z);
	}

	public partial struct Vector3Int {
		public static implicit operator UnityEngine.Vector3Int(Vector3Int from) => new UnityEngine.Vector3Int(from.x, from.y, from.z);
		public static implicit operator Vector3Int(UnityEngine.Vector3Int from) => new Vector3Int(from.x, from.y, from.z);
	}

	public partial struct Vector2 {
		public static implicit operator UnityEngine.Vector2(Vector2 from) => new UnityEngine.Vector2(from.x, from.y);
		public static implicit operator Vector2(UnityEngine.Vector2 from) => new Vector2(from.x, from.y);
	}

	public partial struct Vector2Int {
		public static implicit operator UnityEngine.Vector3Int(Vector2Int from) => new UnityEngine.Vector3Int(from.x, from.y);
		public static implicit operator Vector2Int(UnityEngine.Vector3Int from) => new Vector2Int(from.x, from.y);
	}

#endif
}
