using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChickenWing : EnemyBasic
{
    public GameObject chickenProj;
    public GameObject projSpawn;
    protected Vector3 projSpawnPt;

    public bool isFleeing = false;

    public float throwCooldown = 3f;
    protected float threwAtTime;
    public bool thrownProj = false;
    protected int throwSide;

    public float scareRadius = 3f;

    //chicken audio
    public AudioSource ChickenRun;
    public AudioSource MolotovThrow;
    public float Counter;


    public override void Start()
    {
        base.Start();
        ChickenRun.clip = AudioDictionary.aDict.audioDict["chickenRun"];
        MolotovThrow.clip = AudioDictionary.aDict.audioDict["molotovToss"];
    }

    override public void Update()
    {
        projSpawnPt = projSpawn.transform.position;
        if (Time.time >= threwAtTime + throwCooldown) thrownProj = false;

        // Chicken uses InRoom in BaseUpdate
       /* if (inRoom == false)
        {
            transform.position = originalPos;
            return;
        }
       */
        base.Update();
        EnemyMovement();
        
    }
    override public void EnemyMovement()
    {

        /*if (jumping) return;
        if ((playerPos.x >= enemyPos.x - 0.25f) && (playerPos.x <= enemyPos.x + 0.25f) &&
            (playerPos.y > enemyPos.y) && !jumping && (Time.time > jumpTime))
        {
            jumping = true;
            jumpTime = Time.time + jumpDuration;
            Invoke("EnemyJump", 0.15f);
        }*/
        // add an "else if" below if want to fix jumping, otherwise below is just basic movement
        if ((playerPos.x >= enemyPos.x - scareRadius) && (playerPos.x <= scareRadius + enemyPos.x))
        {
            if (playerPos.y > enemyPos.y + detRadius/2 || playerPos.y < enemyPos.y - detRadius/2) return;

            isFleeing = true;

            if (!ChickenRun.isPlaying) ChickenRun.Play();

            if (playerPos.x >= enemyPos.x - scareRadius && playerPos.x <= enemyPos.x)
            {
                //Debug.Log("Player in from left");
                rigid.velocity = Vector2.right * speed;
                enemyPos = transform.position;
                Moving = 1;
            }
            if (playerPos.x <= enemyPos.x + scareRadius && playerPos.x >= enemyPos.x)
            {
                //Debug.Log("Player in from right");
                rigid.velocity = Vector2.left * speed;
                enemyPos = transform.position;
                Moving = -1;
            }
        }
        else
        {
            isFleeing = false;
            Moving = 0;
            //for chicken audio

            if ((playerPos.x <= enemyPos.x - scareRadius) || (playerPos.x >= detRadius + enemyPos.x))
            {
                rigid.velocity = new Vector2(0, 0);
                enemyPos = transform.position;
            }
            if ((playerPos.x >= enemyPos.x - detRadius) && (playerPos.x <= detRadius + enemyPos.x))
            {
                if (playerPos.y > enemyPos.y + detRadius/2 || playerPos.y < enemyPos.y - detRadius/2) return;

                if (playerPos.x >= enemyPos.x - detRadius && playerPos.x <= enemyPos.x)
                {
                    //Debug.Log("Player in from left");
                    throwSide = -1;
                    ThrowMolotov();
                    Idle = -1;
                }
                if (playerPos.x <= enemyPos.x + detRadius && playerPos.x >= enemyPos.x)
                {
                    //Debug.Log("Player in from right");
                    throwSide = 1;
                    Idle = 1;
                    ThrowMolotov();
                }
            }
        }   
    }

    public void ThrowMolotov()
    {
        if (thrownProj || isFleeing) return;
        MolotovThrow.Play();
        GameObject chickMolv = Instantiate<GameObject>(chickenProj);
        chickMolv.transform.position = projSpawnPt;
        float molvLength = chickMolv.GetComponent<ChickenMolotov>().arcLength;
        molvLength *= throwSide;
        chickMolv.GetComponent<ChickenMolotov>().arcLength = molvLength;

        thrownProj = true;
        threwAtTime = Time.time;
    }

}