using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ResetGraphValues")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ResetGraphValues", message: "Restart Graph", category: "Events", id: "6be9eb38813b5ca2e1a9ea2a71eb41d7")]
public sealed partial class ResetGraphValues : EventChannel { }

