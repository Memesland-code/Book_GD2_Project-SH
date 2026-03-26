using UnityEngine;

public static class SoundSystem
{
	public static void EmitSound(Vector3 position, float radius, GameObject source)
	{
		Collider[] colliders = Physics.OverlapSphere(position, radius);

		foreach (Collider col in colliders)
		{
			if (col.TryGetComponent(out ISoundListener listener))
			{
				listener.OnSoundHeard(position, source);
			}
		}
	}
}
