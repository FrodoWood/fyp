using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability2 : BaseAbility
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    [ContextMenu("useAbility1")]
    public override void TriggerAbility()
    {
        base.TriggerAbility(); // Starts the cooldown timer and sets the ability on cooldown

    }
}
