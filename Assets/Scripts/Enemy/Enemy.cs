using UnityEngine;
using System.Collections;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum EnemyType {Melee, Ranged, Leap, Env}; //Enemy Classes
    public EnemyType Class; //Inspector visualization of classes
    public enum State {Idle, Chasing, Attacking};
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;

    Material skinMaterial;
    Color originalColor;

    public float attackDistanceThreshold = 0.5f;
    public  float timeBetweenAttacks = 1f;
    float nextAttackTime;
    float myCollsionRadius;
    float targetCollisionRadius;
    bool hasTarget;

    public float dmg = 4f;

    //Ranged enemy
    GunController gunController;

    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        if (Class == EnemyType.Ranged)
        {
            gunController = GetComponent<GunController>();
        }

        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            
            myCollsionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (hasTarget)
        {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    public override void TakeHit(float dmg, RaycastHit hit)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);

        if(dmg >= health)
        {
            AudioManager.instance.PlaySound("EnemyDeath", transform.position);
        }
        base.TakeHit(dmg, hit);
    }

    void Update()
    {
        if (hasTarget) { 
            if (Time.time > nextAttackTime)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollsionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.PlaySound("EnemyAttack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;
        //Leap Code
        if (Class == EnemyType.Leap) { 
            Vector3 originalPosition = transform.position;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            Vector3 attackPosition = target.position - dirToTarget * (myCollsionRadius);

            float attackSpeed = 3; //Leap speed
            float percent = 0f;

            skinMaterial.color = Color.white;

            bool hasAppliedDamage = false;

            while (percent <= 1)
            {
                if (percent >= 0.5f && !hasAppliedDamage)
                {
                    hasAppliedDamage = true;
                    targetEntity.TakeDamage(dmg);
                }
                percent += Time.deltaTime * attackSpeed;
                float interpolation = (Mathf.Pow(percent, 2) + percent) * 4;
                transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

                yield return null;
            }
        //End leap Code
    }

        if(Class == EnemyType.Ranged)
        {
            transform.LookAt(target.transform.position);
        //    gunController.Shoot();
        }
        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while (hasTarget)
        {
            if (currentState == State.Chasing) {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollsionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    public void SetCharacteristcs(float moveSpeed, float dmg, float health)
    {
        pathfinder.speed = moveSpeed;
        /* SCALED DAMAGE SYS
        if (hasTarget)
        {
            pathfinder.dmg = Mathf.Ceil(targetEntity.startHealth / dmg); // Kill player in X hits
        }
        */

    }
}
