using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public Transform player;
    [SerializeField] private BossPhase currentPhase = BossPhase.Follow; // Starting phase is Follow
    public float playerDistance;
    public bool PhaseDebug;

    [Header("Phase Permissions")]
    public bool canFollow;
    public bool canGuard;
    public bool canIdle;
    public bool canAttack;
    public bool canDefend;
    public bool canRetreat;
    public bool canApproach;

    [Header("Phase Manager")]
    public float intervalPhaseChanger = 1f;
    public bool isFollowPhase = false;
    public bool isGuardPhase = false;
    public bool isIdlePhase = true;
    public bool isAttackPhase = false;
    public bool isDefendPhase = false;
    public bool isRetreatPhase = false;
    public bool isApproachPhase = false;

    [Header("Follow Phase Settings")]
    public float moveSpeed = 5f;
    public float updateInterval = 3f; // Time interval between boss movement updates
    public float minChaseDistance = 1f; // Minimum distance to start or stop chasing the player
    public float yOffset = 0.5f; // You can adjust this value as needed
    public float minfollowDuration = 5f;
    public float maxfollowDuration = 10f;

    [Header("Guard Phase Settings")]
    public float rotationSpeed = 50f; // La velocità di rotazione
    public float minguardPhaseDuration = 5f;
    public float maxguardPhaseDuration = 10f;
    private int guardDirection;
    private Vector3 latestPlayerPosition;
    private Vector3 rotationCenter; // Punto di riferimento per la rotazione

    [Header("Idle Phase Settings")]
    public float minIdleDuration = 2f;
    public float maxIdleDuration = 5f;

    [Header("Attack Phase Settings")]
    public float minAttackDuration = 2f;
    public float maxAttackDuration = 5f;
    public float maxAttackDistance = 2f;

    [Header("Defend Phase Settings")]
    public float minDefendDuration = 2f;
    public float maxDefendDuration = 5f;

    [Header("Retreat Phase Settings")]
    public float retreatDistance = 3f;
    public float minRetreatDuration = 5f;
    public float maxRetreatDuration = 10f;

    [Header("Approach Phase Settings")]
    public float moveApproachSpeed = 3f;
    public float minApproachDuration = 5f;
    public float maxApproachDuration = 10f;

    [Header("Damage Manager")]
    public bool isDamaged = false;
    public float damageDuration = 3f; // Durata del danneggiamento in secondi
    [SerializeField] private float damageResetDelay = 5f;
    private float damageTimer = 0f;
    private float damageResetTimer = 0f;

    private float timer;
    private float phaseTimer;
    private float guardTimer;
    private float attackTimer;
    private float defendTimer;
    private float idleTimer;
    private float followTimer;
    private float retretatTimer;
    private float approachTimer;

    private int chosedDirection = 0;

    private enum BossPhase
    {
        Attack,
        Guard,
        Defend,
        Follow,
        Idle,
        Retreat,
        Approach
    }

    private void Start()
    {
        latestPlayerPosition = player.position;
        isGuardPhase = false;
        isApproachPhase = false; // Rimuovere una delle inizializzazioni
        isFollowPhase = false; // Impostare correttamente la fase di Follow
        isIdlePhase = true;
        isAttackPhase = false;
        isDefendPhase = false;
        isRetreatPhase = false;
    }

    private void Update()
    {
        playerDistance = Vector3.Distance(transform.position, player.position);

        // Switch tra le diverse fasi del boss
        switch (currentPhase)
        {
            case BossPhase.Attack:
                AttackPhase();
                break;
            case BossPhase.Guard:
                GuardPhase();
                break;
            case BossPhase.Defend:
                DefendPhase();
                break;
            case BossPhase.Follow: // Nuova fase Follow
                FollowPhase();
                break;
            case BossPhase.Idle: // Nuova fase Idle
                IdlePhase();
                break;
            case BossPhase.Retreat:
                RetreatPhase();
                break;
            case BossPhase.Approach:
                ApproachPhase();
                break;
            default:
                Debug.LogError("Unhandled boss phase.");
                break;
        }

        //Timer per calcolare l'ultima posizione
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            latestPlayerPosition = player.position;
            timer = updateInterval;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            // Attiva il danneggiamento
            IsGettingDamaged();
        }

        // Aggiorna il timer del danneggiamento se il boss è danneggiato
        if (isDamaged)
        {
            // Decrementa il timer
            damageTimer -= Time.deltaTime;

            // Se il timer arriva a zero, disattiva il danneggiamento
            if (damageTimer <= 0f)
            {
                damageTimer = damageDuration;
                isDamaged = false;
            }
        }

        if (!PhaseDebug)
        {
            if (isDamaged && !isAttackPhase && !isDefendPhase && !isRetreatPhase && !isFollowPhase)
            {
                
                    // Reset the timers for other phases
                    idleTimer = Random.Range(minIdleDuration, maxIdleDuration);
                    guardTimer = Random.Range(minguardPhaseDuration, maxguardPhaseDuration);
                    approachTimer = Random.Range(minApproachDuration, maxApproachDuration);

                    // Choose a random phase to switch to
                    int chosePhase = Random.Range(0, 3);

                    // Stop the current phase and switch to one of the available phases
                    if (chosePhase == 0)
                    {
                        Debug.Log("Follow");
                        SwitchPhase(BossPhase.Follow); // Switch to the follow phase
                    }
                    else if (chosePhase == 1)
                    {
                        Debug.Log("Defend");
                        SwitchPhase(BossPhase.Defend); // Switch to the defend phase
                    }
                    else if (chosePhase == 2)
                    {
                        Debug.Log("Retreat");
                        SwitchPhase(BossPhase.Retreat); // Switch to the retreat phase
                    }

                    // Reset the damage flag
                    isDamaged = false;
            }


            if (!isApproachPhase && !isAttackPhase && !isDefendPhase && !isFollowPhase && !isGuardPhase && !isIdlePhase && !isRetreatPhase)
            {
                //Timer per il cambio di fase
                phaseTimer -= Time.deltaTime;
            }

            if (phaseTimer <= 0f)
            {
                if (playerDistance > 15f)
                {
                    SwitchPhase(BossPhase.Approach);
                }
                else if (playerDistance <= 15f && playerDistance >= 10f)
                {
                    // Maggiormente in Guard
                    int phase2Chosed = Random.Range(0, 3);
                    if (phase2Chosed == 0)
                    {
                        SwitchPhase(BossPhase.Idle);
                    }
                    else if (phase2Chosed == 1)
                    {
                        SwitchPhase(BossPhase.Guard);
                    }
                    else if (phase2Chosed == 2)
                    {
                        int phaseChance = Random.Range(0, 2); // Aggiungi una probabilità per la ritirata
                        if (phaseChance == 0)
                        {
                            SwitchPhase(BossPhase.Follow);
                        }
                        else if (phaseChance == 1)
                        {
                            SwitchPhase(BossPhase.Approach);
                        }
                        else if (phaseChance == 2)
                        {
                            SwitchPhase(BossPhase.Idle);
                        }
                    }
                }
                else if (playerDistance < 10f && playerDistance > maxAttackDistance)
                {
                    int phaseChance = Random.Range(0, 3); // Aggiungi una probabilità per la ritirata
                    if (phaseChance == 0)
                    {
                        SwitchPhase(BossPhase.Follow);
                    }
                    else if (phaseChance == 1)
                    {
                        SwitchPhase(BossPhase.Approach);
                    }
                    else if (phaseChance == 2)
                    {
                        SwitchPhase(BossPhase.Guard);
                    }
                }
                else if (playerDistance <= maxAttackDistance)
                {
                        int retreatChance = Random.Range(0, 3); // Aggiungi una probabilità per la ritirata
                        if (retreatChance == 0)
                        {
                            SwitchPhase(BossPhase.Retreat);
                        }
                        else if (retreatChance == 1)
                        {
                            SwitchPhase(BossPhase.Attack);
                        }
                        else if (retreatChance == 2)
                        {
                            SwitchPhase(BossPhase.Defend);
                        }
                }

                phaseTimer = intervalPhaseChanger;
            }

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SwitchPhase(BossPhase.Follow);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                SwitchPhase(BossPhase.Idle);
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                SwitchPhase(BossPhase.Guard);
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                SwitchPhase(BossPhase.Attack);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                SwitchPhase(BossPhase.Defend);
            }

            if (Input.GetKeyDown(KeyCode.O)) // Aggiunta della chiave per la fase Retreat
            {
                SwitchPhase(BossPhase.Retreat);
            }

            if (Input.GetKeyDown(KeyCode.E)) // Aggiunta della chiave per la fase Retreat
            {
                SwitchPhase(BossPhase.Approach);
            }
        }
    }



    private void SwitchPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;

        switch (currentPhase)
        {
            case BossPhase.Attack:
                attackTimer = Random.Range(minAttackDuration, maxAttackDuration);
                isGuardPhase = false;
                isIdlePhase = false;
                isApproachPhase = false;
                isDefendPhase = false;
                isFollowPhase = false;
                isRetreatPhase = false;
                isAttackPhase = true;
                break;
            case BossPhase.Guard:
                guardTimer = Random.Range(minguardPhaseDuration, maxguardPhaseDuration);
                isAttackPhase = false;
                isIdlePhase = false;
                isApproachPhase = false;
                isDefendPhase = false;
                isFollowPhase = false;
                isRetreatPhase = false;
                isGuardPhase = true;
                break;
            case BossPhase.Defend:
                defendTimer = Random.Range(minDefendDuration, maxDefendDuration);
                isGuardPhase = false;
                isAttackPhase = false;
                isIdlePhase = false;
                isApproachPhase = false;
                isFollowPhase = false;
                isRetreatPhase = false;
                isDefendPhase = true;
                break;
            case BossPhase.Follow:
                followTimer = Random.Range(minfollowDuration, maxfollowDuration);
                isGuardPhase = false;
                isAttackPhase = false;
                isIdlePhase = false;
                isApproachPhase = false;
                isDefendPhase = false;
                isRetreatPhase = false;
                isFollowPhase = true;
                break;
            case BossPhase.Idle:
                idleTimer = Random.Range(minIdleDuration, maxIdleDuration);
                isGuardPhase = false;
                isAttackPhase = false;
                isApproachPhase = false;
                isDefendPhase = false;
                isFollowPhase = false;
                isRetreatPhase = false;
                isIdlePhase = true;
                break;
            case BossPhase.Retreat:
                retretatTimer = Random.Range(minRetreatDuration, maxRetreatDuration);
                isGuardPhase = false;
                isAttackPhase = false;
                isIdlePhase = false;
                isApproachPhase = false;
                isDefendPhase = false;
                isFollowPhase = false;
                isRetreatPhase = true;
                break;
            case BossPhase.Approach:
                approachTimer = Random.Range(minApproachDuration, maxApproachDuration);
                isGuardPhase = false;
                isAttackPhase = false;
                isIdlePhase = false;
                isDefendPhase = false;
                isFollowPhase = false;
                isRetreatPhase = false;
                isApproachPhase = true;
                break;
            default:
                Debug.LogError("Unhandled boss phase.");
                break;
        }
    }



    private void GuardPhase()
    {
        if (isGuardPhase)
        {
            if (chosedDirection == 0)
            {
                int directionChosing = Random.Range(0, 2);
                guardDirection = directionChosing == 0 ? -1 : 1;
                chosedDirection++;
            }

            guardTimer -= Time.deltaTime;

            if (guardTimer <= 0f)
            {
                isGuardPhase = false;
                chosedDirection = 0;
                guardTimer = Random.Range(minguardPhaseDuration, maxguardPhaseDuration);
            }
            else
            {
                // Calcola l'angolo di rotazione per ogni frame (basato sulla velocità costante)
                float rotationAngle = guardDirection * rotationSpeed * Time.deltaTime;

                // Aggiorna il centro di rotazione alla posizione attuale del giocatore
                rotationCenter = player.position;

                // Ruota il boss attorno al giocatore
                transform.RotateAround(rotationCenter, Vector3.up, rotationAngle);

                // Orienta l'oggetto per guardare sempre verso il centro
                transform.LookAt(rotationCenter);
            }
        }
    }




    private void AttackPhase()
    {
        if (isAttackPhase)
        {
            if (Vector3.Distance(transform.position, player.position) <= maxAttackDistance)
            {
                attackTimer -= Time.deltaTime;
                transform.LookAt(player.position);
                if (attackTimer <= 0f)
                {
                    isAttackPhase = false;
                    attackTimer = Random.Range(minAttackDuration, maxAttackDuration);
                }
            }
            else
            {
                attackTimer = Random.Range(minAttackDuration, maxAttackDuration);
                isAttackPhase = false;
            }
        }
    }

    private void DefendPhase()
    {
        if (isDefendPhase)
        {
            defendTimer -= Time.deltaTime;
            transform.LookAt(player.position);

            if (defendTimer <= 0f)
            {
                isDefendPhase = false;
                defendTimer = Random.Range(minDefendDuration, maxDefendDuration);
            }
        }
    }

    private void IdlePhase()
    {
        if (isIdlePhase)
        {
            idleTimer -= Time.deltaTime;
            transform.LookAt(player.position);

            if (idleTimer <= 0f)
            {
                isIdlePhase = false;
                idleTimer = Random.Range(minIdleDuration, maxIdleDuration);
            }
        }
    }

    private void FollowPhase()
    {
        if (isFollowPhase)
        {
            followTimer -= Time.deltaTime;

            if (followTimer <= 0f)
            {
                isFollowPhase = false;
                followTimer = Random.Range(minfollowDuration, maxfollowDuration);
            }
            else
            {
                // Check if player variable is assigned correctly
                if (player == null)
                {
                    Debug.LogError("Player variable is not assigned correctly.");
                    return;
                }
                transform.LookAt(latestPlayerPosition);

                // Calculate direction towards the player
                Vector3 direction = latestPlayerPosition - transform.position;
                float distanceToPlayer = direction.magnitude; // Distance to player

                // Check if the boss is within the minimum chase distance
                if (distanceToPlayer <= minChaseDistance)
                {
                    // Boss is already within minimum chase distance, do nothing
                    return;
                }

                direction.Normalize();

                // Cast a ray downwards to detect the ground
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {
                    // Check if the ray hits the ground
                    if (hit.collider.CompareTag("Ground"))
                    {
                        // Calculate new boss position on the ground
                        Vector3 newPosition = hit.point;

                        // Adjust the Y position slightly above the ground to avoid clipping
                        newPosition.y += yOffset;

                        // Move boss towards the player only on the X and Z axes
                        Vector3 targetPosition = newPosition + direction * moveSpeed * Time.deltaTime;
                        targetPosition.y = newPosition.y; // Maintain the Y position

                        // Update boss position
                        transform.position = targetPosition;
                    }
                    else
                    {
                        // If the ray does not hit the ground, do nothing (this can happen if there's no ground below)
                        Debug.LogWarning("No ground detected beneath the boss.");
                    }
                }
                else
                {
                    // If raycast fails, do nothing
                    Debug.LogWarning("Raycast failed to detect ground.");
                }
            }
        }
    }

    private void RetreatPhase()
    {
        if (isRetreatPhase)
        {
            retretatTimer -= Time.deltaTime;

            if (retretatTimer <= 0f)
            {
                isRetreatPhase = false;
                retretatTimer = Random.Range(minRetreatDuration, maxRetreatDuration);
            }
            else
            {
                transform.LookAt(player.position);
                // Calcola la direzione in cui il boss deve indietreggiare
                Vector3 retreatDirection = transform.position - player.position;
                retreatDirection.y = 0; // Assicura che l'indietreggiamento sia solo lungo l'asse X e Z
                retreatDirection.Normalize();

                // Calcola la posizione di destinazione per l'indietreggiamento
                Vector3 retreatPosition = transform.position + retreatDirection * retreatDistance * Time.deltaTime;

                // Muovi il boss verso la posizione di destinazione
                transform.position = retreatPosition;
            }
        }
    }

    private void ApproachPhase()
    {
        if (isApproachPhase)
        {
            approachTimer -= Time.deltaTime;

            if (approachTimer <= 0f)
            {
                isApproachPhase = false;
                approachTimer = Random.Range(minApproachDuration, maxApproachDuration);
            }
            else
            {
                // Check if player variable is assigned correctly
                if (player == null)
                {
                    Debug.LogError("Player variable is not assigned correctly.");
                    return;
                }
                transform.LookAt(player.position);

                // Calculate direction towards the player
                Vector3 direction = player.position - transform.position;
                float distanceToPlayer = direction.magnitude; // Distance to player

                // Check if the boss is within the minimum chase distance
                if (distanceToPlayer <= minChaseDistance)
                {
                    // Boss is already within minimum chase distance, do nothing
                    isApproachPhase = false;
                    return;
                }


                direction.Normalize();

                // Cast a ray downwards to detect the ground
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {
                    // Check if the ray hits the ground
                    if (hit.collider.CompareTag("Ground"))
                    {
                        // Calculate new boss position on the ground
                        Vector3 newPosition = hit.point;

                        // Adjust the Y position slightly above the ground to avoid clipping
                        newPosition.y += yOffset;

                        // Move boss towards the player only on the X and Z axes
                        Vector3 targetPosition = newPosition + direction * moveApproachSpeed * Time.deltaTime;
                        targetPosition.y = newPosition.y; // Maintain the Y position

                        // Update boss position
                        transform.position = targetPosition;
                    }
                    else
                    {
                        // If the ray does not hit the ground, do nothing (this can happen if there's no ground below)
                        Debug.LogWarning("No ground detected beneath the boss.");
                    }
                }
                else
                {
                    // If raycast fails, do nothing
                    Debug.LogWarning("Raycast failed to detect ground.");
                }
            }
        }
    }

    private void IsGettingDamaged()
    {
        isDamaged = true;
        damageResetTimer = damageResetDelay; // Set the damage reset timer to the delay value
    }

    private void OnDrawGizmos()
    {
        // Disegna un wireframe sferico intorno al boss per indicare il range di attacco
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxAttackDistance);

        // Disegna una freccia dalla posizione del boss nella direzione in cui sta guardando
        DrawArrow(transform.position, transform.forward * 2f, Color.green);

        // Disegna un cerchio per rappresentare il diametro della rotazione durante la fase di guardia
        if (currentPhase == BossPhase.Guard)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rotationCenter, Vector3.Distance(transform.position, rotationCenter));
        }
    }


    // Funzione per disegnare una freccia
    private void DrawArrow(Vector3 startPos, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(startPos, direction);

        // Disegna le punte della freccia
        Vector3 arrowHead = startPos + direction;
        float arrowHeadSize = 0.2f;
        float arrowHeadAngle = 20f;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(arrowHead, right * arrowHeadSize);
        Gizmos.DrawRay(arrowHead, left * arrowHeadSize);
    }
}
