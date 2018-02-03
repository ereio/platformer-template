using UnityEngine;
using Assets.Scripts;
using System.Collections;
using System;

public class PlayerControl : MonoBehaviour {

    public struct weapon_offset
    {
       public float x;
       public float y;
    }

    #region Status

    /// <summary>
    /// This could easily be used for each entity
    /// </summary>
    static public class Status {
        // Use for status bools or triggers
        public static int health;
        public static bool dead;
        public static bool grounded;
        public static bool charging;
        public static bool stunned;
        public static bool forward;  // true is right facing
        public static bool running;  // set true if holding shift
        public static bool initalized;
        public static float run_force;
        public static float walk_force;
        public static float jump_force;
        public static weapon_offset weapon_offset;

        static public void Initalize()
        {
            initalized = false;
            health = 10;
            dead = false;
            running = false;
            charging = false;
            grounded = true;
            stunned = false;
            forward = true;
            run_force = 3f;
            walk_force = 1f;
            jump_force = 400f;
            weapon_offset.x = 0.3f;
            weapon_offset.y = 0.4f;
        }
    }

    #endregion

    public GameObject projectile;
    public GameObject charged_projectile;
    public GameObject alt_projectile;

    public LayerMask ground_layer;
    public Transform ground_check;
    public float ground_radius = 0.2f;
    private Animator anim;
    private Rigidbody2D body;
    private SpriteRenderer sprite;

    public float movement = 0f;
    public float force = 0f;
    public float stun_time = 0f;
    public float death_time = 0f;
    public float charge_time = 0f;
    public float complete_charge_time = 1.5f;
    
	void Start () {
        Status.Initalize();
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        // Uncomment for AnimationBlend
        //anim.SetFloat ("vSpeed", GetComponent<Rigidbody2D>().velocity.y);
    }

    void FixedUpdate()  {
        Status.grounded = Physics2D.OverlapCircle(ground_check.position, ground_radius, ground_layer);

        // Horizontal movement
        movement = !Status.stunned ? Input.GetAxis(Movement.Horizontal) : 0;
        force = !Status.running ? Status.walk_force : Status.run_force;
        body.velocity = new Vector2(movement * force, body.velocity.y);

        // Vertical movement
        if(Status.grounded && 0 < Input.GetAxis(Movement.Vertical))
            body.AddForce(new Vector2(0, Status.jump_force));

        if ((movement > 0 && !Status.forward) || movement < 0 && Status.forward)
            SetFlip();
    }
    
	void Update () {

        if (!Status.initalized)
        {
            Status.initalized = Status.stunned = true;
            stun_time = Time.time + 1.5f;
            //anim.SetBool("Spawning",true);
        }

        if (Status.health < 1 && !Status.dead)
        {
            Status.stunned = Status.dead = true;
            death_time = stun_time = Time.time + 2f;
        }

        if (Status.dead && death_time < Time.time)
        {
            // Actually handle respawn here
        }

        Status.stunned = Time.time < stun_time;
            //anim.SetBool("Spawning",false);

        GetInput();
	}

    #region Internals

    private void SetFlip()
    {
        Status.forward = !Status.forward;
        sprite.flipX = !Status.forward;

        //Vector3 scale = transform.localScale;
        //scale.x *= -1;
        //transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        switch (col.gameObject.layer)
        {
            case Layers.ENEMY:
                if (!Status.stunned) SetDamage(1);
                break;
            case Layers.RESPAWN:
                Status.health = 0;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {

    }

    #endregion

    #region Player Actions

    private void GetInput()
    {
        if (Status.stunned) return;

        // Fire button states
        if (Input.GetButtonDown(Action.Fire)) FireProjectile(projectile);
        else if (Input.GetButton(Action.Fire)) ChargeProjectile();
        else if (Input.GetButtonUp(Action.Fire)) FireChargedProjectile();


    }

    public void SetDamage(int damage)
    {
        // TODO - Run any anim or sounds of damage here
        Status.stunned = true;
        stun_time = Time.time + 0.5f;
        Status.health -= damage;
    }

    public void FireProjectile(GameObject projectile)
    {
        // Schedule Audio -
        // Set Animation -

        float proj_origin_y = transform.position.y + Status.weapon_offset.y;
        float proj_origin_x = Status.weapon_offset.x;

        if (!Status.forward) proj_origin_x *= -1;
        proj_origin_x += transform.position.x;

        Vector3 proj_origin = new Vector3(proj_origin_x, proj_origin_y, 0f);
        Instantiate(projectile, proj_origin, Quaternion.identity);
    }

    public void FireChargedProjectile()
    {
        Status.charging = false;
        if(complete_charge_time < charge_time)
        {
            // Schedule Audio - //schedulePlay(sound.mm_charged_burst);
            // Schedule Fire - //anim.SetBool("FireMode",true); //anim.SetTrigger("Fire");
            FireProjectile(charged_projectile);
        }
    }

    public void ChargeProjectile()
    {
        if (!Status.charging)
        {
            Status.charging = true;
            charge_time = 0f;
        }
        else
        {
            charge_time += Time.deltaTime;
        }
    }

    #endregion 

}
