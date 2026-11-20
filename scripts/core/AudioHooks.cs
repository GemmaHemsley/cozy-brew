using Godot;

/// <summary>
/// Autoload (registered in project.godot) that the rest of the game calls for sound. There is no audio yet, so
/// every method is a deliberate no-op: call sites are already in place, and adding real audio later means
/// filling in this one class (load streams, play them on AudioStreamPlayer nodes) without touching callers.
/// </summary>
public partial class AudioHooks : Node
{
	public static AudioHooks Instance { get; private set; }

	public override void _EnterTree() => Instance = this;

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	public void PlaySfx(SfxId id)
	{
		// No audio yet.
	}

	public void PlayMusic(MusicId id)
	{
		// No audio yet.
	}

	public void StopMusic()
	{
		// No audio yet.
	}
}
