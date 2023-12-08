namespace Stupid.Bindings {
#if GODOT
	using Stupid;
	using Godot;
	using System;

	// Add to project settings autoload list to use.
	public partial class GodotCoroutinerBinder : Node {
		private Action onUpdate;
		private float time;

		public override void _Ready() {
			base._Ready();

			Coroutiner.Bind(() => time,
				() => Engine.GetFramesDrawn(),
				onUpdate);
		}

		public override void _Process(double delta) {
			base._Process(delta);

			time += (float)delta;
			onUpdate?.Invoke();
		}
	}
#endif
}
