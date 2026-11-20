using System;

/// <summary>The fixed number of places customers can stand at the counter.</summary>
public sealed class CounterSlots
{
	public const int DefaultCount = 3;
	private readonly bool[] _taken;

	public CounterSlots(int count = DefaultCount)
	{
		if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
		_taken = new bool[count];
	}

	public int Count => _taken.Length;
	public bool HasFree => Array.IndexOf(_taken, false) >= 0;

	/// <summary>Takes the lowest free slot; -1 if full.</summary>
	public int TryOccupy()
	{
		int i = Array.IndexOf(_taken, false);
		if (i >= 0) _taken[i] = true;
		return i;
	}

	/// <summary>Frees a slot. Out-of-range or already-free slots are ignored.</summary>
	public void Release(int slot)
	{
		if (slot >= 0 && slot < _taken.Length) _taken[slot] = false;
	}
}
