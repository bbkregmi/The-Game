using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove : MonoBehaviour
{

    public bool turn = false;
    public bool hasMoved = false;
    public bool hasAttacked = false;
    public bool choosingAttackTargets = false;
    public bool moving = false;
    public bool attacking = false;
    public Animator anim;


    List<Tile> selectableTiles = new List<Tile>();
    List<Tile> attackableTiles = new List<Tile>();
    GameObject[] tiles;

    Stack<Tile> path = new Stack<Tile>();
    public Tile currentTile;

    public int move = 5;
    public int attackRange = 1;
    public float moveSpeed = 2;
    public float jumpHeight = 2;
    public float jumpVelocity = 4.5f;

    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();

    public float halfHeight = 0;

    public bool fallingDown = false;
    public bool jumpingUp = false;
    public bool movingEdge = false;
    Vector3 jumpTarget;

    public Tile actualTargetTile;

    public void Init()
    {
        tiles = GameObject.FindGameObjectsWithTag(Tag.TILE);

        halfHeight = GetComponent<Collider>().bounds.min.y;
        anim = GetComponent<Animator>();
    }

    public Tile GetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
        return currentTile;

    }

    public List<Tile> GetSelectableTiles()
    {
        return selectableTiles;
    }

    public Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile tile = null;

        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 10))
        {
            tile = hit.collider.GetComponent<Tile>();
        }

        return tile;
    }

    public void ComputeAdjacencyList(float jumpHeight, Tile target)
    {
        foreach (GameObject tile in tiles)
        {
            //For maps that change within battle
            //tiles = GameObject.FindGameObjectsWithTag("Tile");

            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target);
        }
    }

    public void FindAttackableTiles(int distance)
    {
        ComputeAdjacencyList(jumpHeight, null);
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        process.Enqueue(currentTile);
        currentTile.visited = true;
        //currentTile.parent = null

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            TacticsMove attackable = t.GetUnitOnTile();

            //If no units on target tile or NPC is on target tile, it's attackable
            if (attackable == null || attackable.tag.Equals(Tag.NPC))
            {
                attackableTiles.Add(t);
                t.attackable = true;
            }

            if (t.distance < distance)
            {
                foreach (Tile tile in t.adjacencyList)
                {
                    if (!tile.visited)
                    {
                        tile.parent = t;
                        tile.visited = true;
                        tile.distance = 1 + t.distance;

                        process.Enqueue(tile);
                    }
                }
            }
        }
    }

    public void FindSelectableTiles(int distance)
    {
        ComputeAdjacencyList(jumpHeight, null);
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        process.Enqueue(currentTile);
        currentTile.visited = true;
        //currentTile.parent = null

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            selectableTiles.Add(t);
            t.selectable = true;

            if (t.distance < distance)
            {
                foreach (Tile tile in t.adjacencyList)
                {
                    if (!tile.visited && ((tile.GetUnitOnTile() == null) ||
                        tile.GetUnitOnTile().tag.Equals(this.tag)))
                    {
                        tile.parent = t;
                        tile.visited = true;
                        tile.distance = 1 + t.distance;

                        process.Enqueue(tile);
                    }
                }
            }
        }
    }

    public void MoveToTile(Tile tile)
    {
        path.Clear();

        tile.target = true;
        moving = true;
        anim.SetBool("Moving", true);

        Tile next = tile;

        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }

    }

    public void Move()
    {
        if (path.Count > 0)
        {
            Tile t = path.Peek();
            Vector3 target = t.transform.position;

            //Calculate the unit's position on top of target tile
            target.y = 0.1f + t.GetComponent<Collider>().bounds.extents.y;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {

                bool jump = transform.position.y != target.y;
                if (jump)
                {

                    Jump(target);
                }
                else
                {
                    CalculateHeading(target);
                    SetHorizontalVelocity();
                    fallingDown = false;
                    jumpingUp = false;
                    movingEdge = false;
                }

                transform.forward = heading;
                transform.position += velocity * Time.deltaTime * 2;
            }
            else
            {
                //Target tile is reached
                transform.position = target;
                path.Pop();
            }
        }
        else
        {
            //Remove Selectable Tiles
            RemoveSelectableTiles();
            moving = false;
            hasMoved = true;
            anim.SetBool("Moving", false);
        }
    }

    protected void RemoveSelectableTiles()
    {
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }
        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    protected void RemoveAttackableTiles()
    {
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }
        foreach (Tile tile in attackableTiles)
        {
            tile.Reset();
        }

        attackableTiles.Clear();
    }

    void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * moveSpeed;
    }

    void Jump(Vector3 target)
    {
        if (fallingDown)
        {
            FallDownward(target);
        }
        else if (jumpingUp)
        {
            JumpUpward(target);
        }
        else if (movingEdge)
        {
            MoveToEdge();
        }
        else
        {
            PrepareJump(target);
        }
    }

    void PrepareJump(Vector3 target)
    {
        float targetY = target.y;

        target.y = transform.position.y;

        CalculateHeading(target);

        //Fall down after moving to edge
        if (transform.position.y > targetY)
        {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = true;

            jumpTarget = transform.position + (target - transform.position) / 2.0f;
        }
        //Jump up
        else
        {
            fallingDown = false;
            jumpingUp = true;
            movingEdge = false;

            velocity = heading * moveSpeed / 3.0f;

            float difference = targetY - transform.position.y;
            velocity.y = jumpVelocity * (0.5f + difference / 2.0f);
        }
    }

    void FallDownward(Vector3 target)
    {
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y <= target.y)
        {
            fallingDown = false;

            Vector3 p = transform.position;
            p.y = target.y;
            transform.position = p;

            velocity = new Vector3();
        }
    }

    void JumpUpward(Vector3 target)
    {
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y > target.y)
        {
            jumpingUp = false;
            fallingDown = true;
        }
    }

    void MoveToEdge()
    {
        if (Vector3.Distance(transform.position, jumpTarget) >= 0.05f)
        {
            SetHorizontalVelocity();
        }
        else
        {
            movingEdge = false;
            fallingDown = true;

            velocity /= 3.0f;
            velocity.y = 1.5f;
        }
    }


    public void AttackEnemy(TacticsMove attackedEnemy)
    {

        BasePlayer enemy = attackedEnemy.GetComponent<ClassInfo>().player;
        BasePlayer hero = this.GetComponent<ClassInfo>().player;
        int damageDone =Mathf.Max(1, hero.PlayerAttack - enemy.PlayerDefense);
        enemy.PlayerHealth = Mathf.Max(0,enemy.PlayerHealth - damageDone);

        if (enemy.PlayerHealth <= 0)
        {
            BattleManager.KillUnit(attackedEnemy);
            Destroy(attackedEnemy.gameObject);
        }

        hasAttacked = true;
    }
}
