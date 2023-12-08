namespace Stupid {
#if GODOT
	using Godot;
	using System;

	public partial struct Vector3 {
		public static implicit operator Godot.Vector3(Vector3 from) => new Godot.Vector3(from.x, from.y, from.z);
		public static implicit operator Vector3(Godot.Vector3 from) => new Vector3(from.X, from.Y, from.Z);
	}

	public partial struct Vector3Int {
		public static implicit operator Godot.Vector3I(Vector3Int from) => new Godot.Vector3I(from.x, from.y, from.z);
		public static implicit operator Vector3Int(Godot.Vector3I from) => new Vector3Int(from.X, from.Y, from.Z);
	}

	public partial struct Vector2 {
		public static implicit operator Godot.Vector2(Vector2 from) => new Godot.Vector2(from.x, from.y);
		public static implicit operator Vector2(Godot.Vector2 from) => new Vector2(from.X, from.Y);
	}

	public partial struct Vector2Int {
		public static implicit operator Godot.Vector2I(Vector2Int from) => new Godot.Vector2I(from.x, from.y);
		public static implicit operator Vector2Int(Godot.Vector2I from) => new Vector2Int(from.X, from.Y);
	}

#endif
}
