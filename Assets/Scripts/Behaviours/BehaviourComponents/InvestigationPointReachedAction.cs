using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Investigation Point Reached", story: "Reset last sound heard from zombie", category: "Action", id: "d4cce67ad6fe8c7d7b918c39e967f24c")]
public partial class InvestigationPointReachedAction : Action
{
    protected override Status OnStart()
    {
	    if (GameObject.TryGetComponent(out ISoundListener soundListener))
		    soundListener.OnSoundInvestigate();
	    
        return Status.Success;
    }
}

