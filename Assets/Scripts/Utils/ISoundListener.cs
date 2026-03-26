using UnityEngine;

public interface ISoundListener
{
	void OnSoundHeard(Vector3 soundPosition, GameObject source);
}
