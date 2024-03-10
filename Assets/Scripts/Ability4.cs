using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability4 : BaseAbility
{
    [SerializeField] private float abilityDuration;
    public bool isComplete { get; private set; }

    protected override void Start()
    {
        base.Start();
        isComplete = false;
    }

    protected override void Update()
    {
        base.Update();
    }

    [ContextMenu("useAbility1")]
    public override void TriggerAbility()
    {
        base.TriggerAbility(); // Starts the cooldown timer and sets the ability on cooldown
        isComplete = false;
        StartCoroutine(AbilityDurationTimer());
    }

    private IEnumerator AbilityDurationTimer()
    {
        yield return new WaitForSeconds(abilityDuration);
        isComplete = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

}
