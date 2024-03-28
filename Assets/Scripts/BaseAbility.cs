using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility : MonoBehaviour, IAbility
{
    [SerializeField] protected bool onCooldown;
    [SerializeField] protected bool abilityEnabled=false;
    [SerializeField] protected float cooldown;
    private float cooldownTimer;
    protected EntityType entity;
    protected EnemyController enemyController;

    protected virtual void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        entity = enemyController.GetEntityType();
    }

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
        cooldownTimer = cooldown;
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

    public bool Available()
    {
        return !onCooldown;
    }

    public bool isEnabled()
    {
        return abilityEnabled;
    }

    public void EnableAbility()
    {
        abilityEnabled = true;
    }

    public void DisableAbility()
    {
        abilityEnabled = false;
    }

    public float GetCooldown()
    {
        return cooldown;
    }


}
