using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility : MonoBehaviour, IAbility
{
    protected bool onCooldown;
    [SerializeField] protected float cooldown;
    private float cooldownTimer;

    protected virtual void Start()
    {
        onCooldown = false;
    }

    protected virtual void Update()
    {
        HandleCooldown();
    }

    public virtual void TriggerAbility()
    {
        onCooldown = true;
    }

    private void HandleCooldown()
    {
        if (!onCooldown) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            onCooldown = false;
        }
    }

    public bool Ready()
    {
        return !onCooldown;
    }


}
