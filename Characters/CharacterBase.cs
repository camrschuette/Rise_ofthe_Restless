using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour 
{
	public const float JUMP_HIGHT = 2.5f; // Hight players can jump
	public float health = 100.0f;
	public float maxHealth = 100.0f;
	public bool dead = false;

	public float moveSpeed = 10.0f;
	public float rotationSpeed = 250.0f;
	public float visibility = 1.0f;
	public float moveMulti = 1.0f;

	protected CharacterController cc = null;
	public Vector3 forces = Vector3.zero;			// used to apply outside forces on the character (since the character controller won't allow us to use a rigidbody)
	private float forceFriction = 5.0f;
	private float maxForce = 10.0f;

	protected float damageInvulnTime = 0.5f; 		// after taking damage, the character is invulnerable for this many seconds
	protected float currentDamageCooldown = 0.0f;	// the character has this many seconds before they can take damage again

	public float attackMultiplier = 1.0f;
	
	public GameObject characterMesh;
	public Animator anim = null;


	public virtual void Awake()
	{
		anim = GetComponent<Animator>();
		cc = GetComponent<CharacterController>();
	}

	protected virtual void Start()
	{
		if(anim == null)
			anim = GetComponent<Animator>();
		if(cc == null)
			cc = GetComponent<CharacterController>();
	}

	protected virtual void FixedUpdate()
	{
		cc.Move(forces * Time.deltaTime * moveMulti);

		float y = forces.y;
		forces = Vector3.Lerp(forces, Vector3.zero, forceFriction * Time.deltaTime);
		forces = new Vector3(forces.x, y, forces.z);
		Vector3.ClampMagnitude(forces, maxForce);

		if (currentDamageCooldown > 0.0f)
		{
			currentDamageCooldown -= Time.deltaTime;
		}
		
		if (transform.position.y > JUMP_HIGHT) {
			transform.Translate(0, JUMP_HIGHT - transform.position.y, 0);
		}
	}

	public void addForce(Vector3 force)
	{
		forces += force;
	}

	public virtual void kill()
	{
		health = 0.0f;
		dead = true;
	}

	public virtual void respawn()
	{
		health = maxHealth;
	}

	public virtual void takeDamage(float amount, Transform enemy=null)
	{
		health -= amount;

		// Flash red to indicate damage taken
		if (characterMesh != null)
		{
			StopCoroutine ("flashRed"); // stop the character from flashing if they are flashing already
			StartCoroutine("flashRed");
		}

		if (health <= 0)
		{
			health = 0.0f;
			kill();
		}
		else
		{
			currentDamageCooldown = damageInvulnTime;
		}
	}

    public virtual void ifCriticalHit(float dmgMultiplier, int randomChance)
    {
        // The higher the int that is passed in randomChance, the lower the crit possibility is
        int i = 0;
        int r = Random.Range(0, randomChance);
        if (i == r)
        {
            attackMultiplier = dmgMultiplier;
        }
    }

    public virtual void resetAttackMultiplier()
    {
        attackMultiplier = 1.0f;
    }

	public IEnumerator Wait(float sec)
	{
		yield return new WaitForSeconds(sec);
	}

	public IEnumerator flashRed()
	{
		float start = Time.time;
		float end = start + damageInvulnTime;
		Material[] modelMats = characterMesh.renderer.materials;

		while (Time.time <= end)
		{
			float gbValue = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - start) / (end - Time.time));
			foreach (Material m in modelMats)
			{
				m.color = new Color(1.0f, gbValue, gbValue);
			}

			yield return null;
		}
	}
}
