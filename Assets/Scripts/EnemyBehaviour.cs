using UnityEngine;
using UnityEngine.AI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyBehaviour : MonoBehaviour
{
    public int Lifepoints = 70;
    public int MaxLifepoints = 70;

    public int Damages = 20;

    public float AttackSpeed = 1;

    public float AttackRange = 2;

    public float DetectionRange = 5;
    public float DetectionAngle = 70;

    public float CallEnemyRange = 70;

    protected PlayerBehaviour _player;

    protected float _attackCooldown = 0;

    protected PlayerBehaviour _target;

    protected NavMeshAgent _agent;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!_player) _player = GameManager.Instance.Player;

        if (_player)
        {
            var dist = Vector3.Distance(_player.transform.position, transform.position);

            if (dist < AttackRange && _target)
            {
                _agent.isStopped = true;
                if (_player && _attackCooldown <= 0)
                {
                    Attack();
                }
            }
            else if (_target)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_target.transform.position);
            }

            if (_target)
            {
                Quaternion newRotation = Quaternion.LookRotation(_player.transform.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
            }
        }

        //if (_target)
        //{
        //    Vector3 position = transform.position + _agent.desiredVelocity;
        //    Quaternion newRotation = Quaternion.LookRotation(position - transform.position);
        //    //PlayerAnimator.transform.rotation = Quaternion.Slerp(PlayerAnimator.transform.rotation, newRotation, 0.1f);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
        //}

        if (_attackCooldown > 0)
        {
            _attackCooldown -= Time.deltaTime;
        }

        if(Lifepoints <= 0)
            Destroy(gameObject);
    }

    protected virtual void LateUpdate()
    {
        if (!_player) return;

        float dist = Vector3.Distance(_player.transform.position, transform.position);
        float angle = Vector3.SignedAngle(_player.transform.position - transform.position, transform.forward, Vector3.up);
        float limits = DetectionAngle / 2;

        if (dist < DetectionRange && -limits <= angle && angle <= limits)
        {
            _target = _player;
        }
    }

    protected virtual void OnDestroy()
    {
        GameManager.KillEnemy(this);
    }

    protected virtual void Attack()
    {
        _attackCooldown = 1f / AttackSpeed;
        _player.TakeDamages(Damages);
    }

    internal void TakeDamages(int damages, PlayerBehaviour player)
    {
        _target = player;

        Lifepoints = Mathf.Clamp(Lifepoints - damages, 0, MaxLifepoints);
        DamageDisplayer.ShowDamage(
            damages,
            transform.position + Vector3.up * 2,
            new Color(211f/255, 84f/255, 0)
        );

        if(Lifepoints > 0)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, CallEnemyRange, ~LayerMask.NameToLayer("Enemy"));
            if (cols.Length > 0)
            {
                foreach (Collider col in cols)
                {
                    if (col.gameObject.TryGetComponent(out EnemyBehaviour enemy))
                    {
                        enemy.SetTarget(player);
                    }
                }
            }
        }
    }

    internal void SetTarget(PlayerBehaviour player)
    {
        _target = player;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Handles.color = new Color(1, 1, 0, 0.3f);
        Handles.DrawSolidDisc(transform.position, transform.up, CallEnemyRange);

        Handles.color = new Color(1, 0, 0, 0.4f);
        Handles.DrawSolidArc(
            transform.position, 
            transform.up, 
            Quaternion.Euler(0, -DetectionAngle/2f, 0) * transform.forward, 
            DetectionAngle, 
            DetectionRange 
        );
    }

#endif
}
