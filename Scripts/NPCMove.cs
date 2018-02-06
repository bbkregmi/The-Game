using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMove : TacticsMove
{

    GameObject target;

    // Use this for initialization
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (!turn)
        {
            return;
        }
        else if (attacking)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
            {
                attacking = false;
                AttackEnemy(target.GetComponent<TacticsMove>());
                NewTurnManager.EndTurn(this);
            }
        }
        else if (hasMoved && !hasAttacked)
        {
            FindNearestTarget();
            if (Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                anim.Play("Attack1");
                attacking = true;
            }
            else
            {
                NewTurnManager.EndTurn(this);
            }
        }

        else if (!moving)
        {
            FindNearestTarget();
            CalculatePath();
            FindSelectableTiles(move);
            actualTargetTile.target = true;
        }
        else
        {
            anim.SetBool("Moving", true);
            Move();
        }
    }

    /**
    private TacticsMove GetLowestHealth(List<TacticsMove> playerList)
    {
        TacticsMove player = playerList[0];

        foreach (TacticsMove tm in playerList)
        {
            BasePlayer bp = tm.GetComponent<ClassInfo>().player;
            if (bp.PlayerHealth < player.GetComponent<ClassInfo>().player.PlayerHealth)
            {
                player = tm;
            }
        }

        return player;
    }
    */

    void CalculatePath()
    {
        Tile targetTile = GetTargetTile(target);

        FindPath(targetTile);
    }

    //Get closest target player
    void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(Tag.PLAYER);

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d < distance)
            {
                distance = d;
                nearest = obj;
            }
        }
        target = nearest;
    }

    protected Tile FindLowestF(List<Tile> list)
    {
        Tile lowest = list[0];

        foreach (Tile t in list)
        {
            if (t.fCost < lowest.fCost)
            {
                lowest = t;
            }
        }
        list.Remove(lowest);
        return lowest;
    }

    protected Tile FindEndTile(Tile t)
    {
        Stack<Tile> tempPath = new Stack<Tile>();

        Tile next = t.parent;

        while (next != null)
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= move)
        {
            return t.parent;
        }

        Tile endTile = null;

        for (int i = 0; i <= move; i++)
        {
            endTile = tempPath.Pop();
        }
        return endTile;
    }

    protected void FindPath(Tile target)
    {
        ComputeAdjacencyList(jumpHeight, target);
        Tile currentTile = GetCurrentTile();

        //A* Algorithm
        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        openList.Add(currentTile);
        //currentTile.parent = null
        currentTile.hCost = Vector3.Distance(currentTile.transform.position,
            target.transform.position);
        currentTile.fCost = currentTile.hCost;

        while (openList.Count > 0)
        {
            Tile t = FindLowestF(openList);
            closedList.Add(t);

            if (t == target)
            {
                actualTargetTile = FindEndTile(t);
                MoveToTile(actualTargetTile);
                return;
            }

            foreach (Tile tile in t.adjacencyList)
            {
                
                if (closedList.Contains(tile))
                {
                    //Do Nothing, already processed
                }

                else if (t.GetUnitOnTile() != null && !t.GetUnitOnTile().Equals(this))
                {
                    Debug.Log(t.GetUnitOnTile());
                }

                else if (openList.Contains(tile))
                {
                    float tempG = t.gCost +
                        Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.gCost)
                    {
                        tile.parent = t;
                        tile.gCost = tempG;
                        tile.fCost = tile.gCost + tile.hCost;
                    }
                }
                else
                {
                    tile.parent = t;
                    tile.gCost = t.gCost +
                        Vector3.Distance(tile.transform.position, t.transform.position);
                    tile.hCost = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.fCost = tile.gCost + tile.hCost;

                    openList.Add(tile);
                }
                
            }
        }
    }
}
