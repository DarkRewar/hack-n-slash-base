using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Components & GameObjects")]
    public Camera Camera;

    public Animator PlayerAnimator;

    [Header("Stats")]
    public int Lifepoints = 100;
    public int MaxLifepoints = 100;

    public int LifeRegen = 10;
    public float LifeRegenCooldown;

    public int Mana = 100;
    public int MaxMana = 100;

    public int ManaRegen = 10;
    public float ManaRegenCooldown;

    public int Damages = 50;

    public float AttackSpeed = 1;

    public float AttackRange = 2;

    [Header("Spells")]
    public Spell ASpell;
    public Spell ZSpell;
    public Spell ESpell;
    public Spell RSpell;

    private NavMeshAgent _agent;

    private EnemyBehaviour _enemyTarget;

    private float _lifeCooldown = 0;
    private float _manaCooldown = 0;
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

        if (ASpell) ASpell = Instantiate(ASpell);
        if (ZSpell) ZSpell = Instantiate(ZSpell);
        if (ESpell) ESpell = Instantiate(ESpell);
        if (RSpell) RSpell = Instantiate(RSpell);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpells();

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

        if (_manaCooldown >= ManaRegenCooldown)
        {
            _manaCooldown = 0f;
            Mana = Mathf.Clamp(
                ManaRegen + Mana,
                0,
                MaxMana
            );
        }
        else
        {
            _manaCooldown += Time.deltaTime;
        }

        if(_attackCooldown > 0)
        {
            _attackCooldown -= Time.deltaTime;
        }

        var checkMagnitude = _agent.desiredVelocity.magnitude < 0.06f;
        PlayerAnimator.SetBool("IsRunning", !checkMagnitude);
        if (checkMagnitude && Vector3.Distance(_agent.destination, transform.position) < 0.06f)
        {
            _agent.isStopped = true;
        }
        else
        {
            Vector3 position = transform.position + _agent.desiredVelocity;
            Quaternion newRotation = Quaternion.LookRotation(position - transform.position);
            //PlayerAnimator.transform.rotation = Quaternion.Slerp(PlayerAnimator.transform.rotation, newRotation, 0.1f);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
        }

        //PlayerAnimator.SetBool("IsRunning", !Mathf.Approximately(_agent.desiredVelocity.magnitude, 0));

        ////Debug.Log($"Desired = {_agent.desiredVelocity} | Magnitude = {_agent.desiredVelocity.magnitude} | {Mathf.Approximately(_agent.desiredVelocity.magnitude, 0)}");
        //if(!Mathf.Approximately(_agent.desiredVelocity.magnitude, 0))
        //{
        //    Vector3 position = transform.position + _agent.desiredVelocity;
        //    Quaternion newRotation = Quaternion.LookRotation(position - transform.position);
        //    //PlayerAnimator.transform.rotation = Quaternion.Slerp(PlayerAnimator.transform.rotation, newRotation, 0.1f);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
        //}
    }

    private void LateUpdate()
    {
        _sliderLife.value = Lifepoints;
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
        if (angle < -0.8f)
        {
            multiplier = 2;
        }

        _enemyTarget.TakeDamages(Damages * multiplier, this);

        _isAttacking = false;
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

    internal void UpdateSpells()
    {
        if (ASpell) ASpell.UpdateCooldown();
        if (ZSpell) ZSpell.UpdateCooldown();
        if (ESpell) ESpell.UpdateCooldown();
        if (RSpell) RSpell.UpdateCooldown();

        if (Input.GetKeyDown(KeyCode.A) && ASpell && ASpell.CanCast && Mana >= ASpell.Cost)
        {
            // A Spell
            if(ASpell is InstantSpell)
            {
                InstantSpell spell = ASpell as InstantSpell;

                Vector2 mousePos = Input.mousePosition;
                Ray ray = Camera.ScreenPointToRay(mousePos);

                Vector3 worldPoint = Vector3.zero;

                var plane = new Plane(Vector3.up, transform.position.y + 0.5f);

                if(plane.Raycast(ray, out float point))
                {
                    worldPoint = ray.GetPoint(point);
                }

                spell.Cast(this, worldPoint);
                Mana -= (int)ASpell.Cost;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Z) && ZSpell && ZSpell.CanCast)
        {
            // Z Spell
        }
        else if (Input.GetKeyDown(KeyCode.E) && ESpell && ESpell.CanCast)
        {
            // E Spell
        }
        else if (Input.GetKeyDown(KeyCode.R) && RSpell && RSpell.CanCast)
        {
            // R Spell
        }
    }
}
