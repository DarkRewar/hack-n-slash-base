using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerBehaviour : MonoBehaviour
{
    public Camera Camera;

    public Animator PlayerAnimator;

    public int Lifepoints = 100;
    public int MaxLifepoints = 100;

    public int LifeRegen = 10;
    public float LifeRegenCooldown;

    public int Damages = 50;

    public float AttackSpeed = 1;

    public float AttackRange = 2;

    private NavMeshAgent _agent;

    private EnemyBehaviour _enemyTarget;

    private float _lifeCooldown = 0;
    private float _attackCooldown = 0;

    [SerializeField]
    private Slider _sliderLife;

    private bool _isAttacking = false;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _sliderLife.maxValue = MaxLifepoints;

        GameManager.Instance?.SetPlayer(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Vector2 mousePos = Input.mousePosition;
            Ray ray = Camera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 30))
            {
                if (hit.collider.gameObject.CompareTag("Ground"))
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(hit.point);
                    _enemyTarget = null;
                }
                else if(hit.collider.gameObject.CompareTag("Enemy"))
                {
                    _enemyTarget = hit.collider.GetComponent<EnemyBehaviour>();
                }
            }
        }

        if (_enemyTarget)
        {
            var dist = Vector3.Distance(_enemyTarget.transform.position, transform.position);
            if (dist > AttackRange)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_enemyTarget.transform.position);
            }
            else if (_attackCooldown <= 0 && !_isAttacking)
            {
                _agent.isStopped = true;
                StartCoroutine(Attack());

                Quaternion newRotation = Quaternion.LookRotation(_enemyTarget.transform.position - transform.position);
                transform.rotation = newRotation;
            }
        }

        if (_lifeCooldown >= LifeRegenCooldown)
        {
            _lifeCooldown = 0f;
            Lifepoints = Mathf.Clamp(
                LifeRegen + Lifepoints,
                0,
                MaxLifepoints
            );
        }
        else
        {
            _lifeCooldown += Time.deltaTime;
        }

        if(_attackCooldown > 0)
        {
            _attackCooldown -= Time.deltaTime;
        }

        PlayerAnimator.SetBool("IsRunning", !Mathf.Approximately(_agent.desiredVelocity.magnitude, 0));

        if(!Mathf.Approximately(_agent.desiredVelocity.magnitude, 0))
        {
            Vector3 position = transform.position + _agent.desiredVelocity;
            Quaternion newRotation = Quaternion.LookRotation(position - transform.position);
            //PlayerAnimator.transform.rotation = Quaternion.Slerp(PlayerAnimator.transform.rotation, newRotation, 0.1f);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
        }
    }

    protected IEnumerator Attack()
    {
        _isAttacking = true;
        PlayerAnimator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.6f);

        _attackCooldown = 1f / AttackSpeed;

        /**
         * Gestion du coup critique dans le dos.
         * Multiplicateur * 2 si derrière le dos
         * dans un angle de 70°
         */
        int multiplier = 1;
        float angle = Vector3.Dot(
            (transform.position - _enemyTarget.transform.position).normalized,
            _enemyTarget.transform.forward
        );
        if(angle < -0.8f)
        {
            multiplier = 2;
        }

        _enemyTarget.TakeDamages(Damages * multiplier, this);

        _isAttacking = false;
    }

    private void LateUpdate()
    {
        _sliderLife.value = Lifepoints;
    }

    internal void TakeDamages(int damages)
    {
        _lifeCooldown = 0;
        Lifepoints = Mathf.Clamp(Lifepoints - damages, 0, MaxLifepoints);

        DamageDisplayer.ShowDamage(
            damages,
            transform.position + Vector3.up * 2,
            new Color(142f/255, 68f/255, 173f/255)
        );
    }
}
