using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    public Animator bossAnimator;
    public BossMovement bossMovement;
    float velocityX = 0.0f;
    float velocityZ = 0.0f;
    public float acceleration;
    public float deceleration;

    [SerializeField] private float distanceNormalizationFactor = 10.0f; // Moltiplicatore regolabile dall'Inspector
    [SerializeField] private float minVelocityX = 0.5f; // Velocità X minima regolabile
    [SerializeField] private float maxVelocityX = 6.0f;

    void Start()
    {
        bossMovement = GetComponent<BossMovement>();
    }

    void Update()
    {
        // Aggiorna velocityZ basandoti sulla distanza dal giocatore e sulla fase corrente
        UpdateMovementParameters();

        // Applica i valori calcolati agli animator parameters
        bossAnimator.SetFloat("VelocityX", velocityX);
        bossAnimator.SetFloat("VelocityZ", velocityZ);
    }

    void UpdateMovementParameters()
    {
        if (bossMovement == null) return;

        float targetVelocityX = 0.0f;
        float targetVelocityZ = 0.0f;
        float currentAcceleration = acceleration; // Usa l'accelerazione standard di default
        float playerDistance = Vector3.Distance(transform.position, bossMovement.player.position); // Distanza dal giocatore

        // Modifica qui usando il moltiplicatore regolabile
        float distanceBasedVelocityX = Mathf.Clamp(playerDistance / distanceNormalizationFactor, minVelocityX, maxVelocityX); // Calcola velocityX basato sulla distanza, assumendo 10 come fattore di normalizzazione

        // Determina i valori target di velocityX e velocityZ basati sulla fase corrente e sulla posizione del giocatore
        switch (bossMovement.currentPhase)
        {
            case BossMovement.BossPhase.Run:
                targetVelocityZ = 6.0f;
                currentAcceleration = acceleration * 2; // Raddoppia l'accelerazione per la fase Follow
                bossAnimator.SetBool("IsRunningAttack", false);
                break;
            case BossMovement.BossPhase.Approach:
                targetVelocityZ = 1.0f;
                bossAnimator.SetBool("IsRunningAttack", false);
                break;
            case BossMovement.BossPhase.Retreat:
                targetVelocityZ = -1.0f;
                bossAnimator.SetBool("IsRunningAttack", false);
                break;
            case BossMovement.BossPhase.Guard:
                // Usa distanceBasedVelocityX per la fase Guard, aggiusta il segno in base alla direzione di guardia
                targetVelocityX = bossMovement.isGuardingLeft ? distanceBasedVelocityX : (bossMovement.isGuardingRight ? -distanceBasedVelocityX : 0.0f);
                currentAcceleration = acceleration * 2; // Raddoppia l'accelerazione per la fase Guard
                bossAnimator.SetBool("IsRunningAttack", false);
                break;
            case BossMovement.BossPhase.Follow:
                bossAnimator.SetBool("IsRunningAttack", true);
                break;
            default:
                bossAnimator.SetBool("IsRunningAttack", false);
                break;
        }

        // Aggiusta gradualmente velocityX e velocityZ verso i loro target con l'accelerazione appropriata
        AdjustVelocityTowardsTarget(ref velocityX, targetVelocityX, currentAcceleration);
        AdjustVelocityTowardsTarget(ref velocityZ, targetVelocityZ, currentAcceleration);
    }


    void AdjustVelocityTowardsTarget(ref float velocity, float targetVelocity, float currentAcceleration)
    {
        if (velocity < targetVelocity)
        {
            velocity += Time.deltaTime * currentAcceleration;
            if (velocity > targetVelocity) // Evita di superare il target
                velocity = targetVelocity;
        }
        else if (velocity > targetVelocity)
        {
            velocity -= Time.deltaTime * currentAcceleration;
            if (velocity < targetVelocity) // Evita di andare sotto il target
                velocity = targetVelocity;
        }
    }
}
