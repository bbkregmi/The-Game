using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove
{

    ClassInfo classinfo;
    TacticsMove enemy;

    // Use this for initialization
    void Start()
    {
        Init();
        classinfo = gameObject.GetComponent<ClassInfo>();
        if (classinfo == null)
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        //It's not the player's turn
        if (!turn)
        {
            return;
        }

        //Player is currently moving
        else if (moving)
        {
            Move();
        }

        //Player is currently attacking
        else if (attacking)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
            {
                attacking = false;
                AttackEnemy(enemy);
            }
        }

        //Player is choosing targets to attack
        else if (choosingAttackTargets)
        {
            FindAttackableTiles(attackRange);
            CheckMouse();
        }

        //Player has already moved, so we show attack targets on default
        else if (hasMoved && !hasAttacked)
        {
            FindAttackableTiles(attackRange);
            CheckMouse();
        }

        //Player is not moving and the player has not moved
        else if (!moving && !hasMoved)
        {
            FindSelectableTiles(move);
            CheckMouse();
        }

        else if (hasMoved && hasAttacked)
        {
            CheckMouse();
        }
    }


    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            //Mouse was clicked on an object with a collider
            if (Physics.Raycast(ray, out hit))
            {

                //A tile was clicked on
                if (hit.collider.tag == "Tile")
                {
                    Tile clickedTile = hit.collider.GetComponent<Tile>();
                    if (choosingAttackTargets)
                    {
                        if (clickedTile.selectable)
                        {
                            TacticsMove attackTarget = clickedTile.GetUnitOnTile();
                            if (attackTarget != null)
                            {
                                AttackEnemy(attackTarget);
                                hasAttacked = true;
                            }
                        }
                    }

                    //Tile was clicked on to move
                    else if (!choosingAttackTargets)
                    {
                        //Display GUI info on the current moving unit
                        classinfo.OnMouseDown();

                        //If the tile is a possible location to move to
                        if (clickedTile.selectable)
                        {
                            //Target to move to
                            MoveToTile(clickedTile);
                        }
                    }
                }

                //Player clicked on another unit of the same team
                else if (hit.collider.tag == "Player")
                {
                    TacticsMove player = hit.collider.GetComponent<TacticsMove>();
                    if (!player.hasMoved || !player.hasAttacked)
                    {
                        RemoveSelectableTiles();
                        RemoveAttackableTiles();
                        this.turn = false;
                        NewTurnManager.ExecuteTurn(player);
                    }
                }

                else if (hit.collider.tag == "NPC" && !hasAttacked)
                {
                    TacticsMove attackTarget = hit.collider.GetComponent<TacticsMove>();

                    //If our attack target exists and is within attack range
                    if (attackTarget != null && attackTarget.GetCurrentTile().attackable)
                    {
                        choosingAttackTargets = false;
                        enemy = attackTarget;
                        PlayAttackAnimation();
                    }
                    else
                    {
                        Debug.Log("This is the target tile " + attackTarget.GetCurrentTile());
                    }
                }
            }
        }
    }

    private void PlayAttackAnimation()
    {
        anim.Play("Attack1");
        attacking = true;
        RemoveAttackableTiles();
    }



    void OnGUI()
    {
        //Waiting for player to make decision
        if (turn)
        {
            //Unit is choosing attack target
            if (choosingAttackTargets)
            {
                GUI.Box(new Rect(Screen.width - 100, 130, 100, 100), "Action");

                //Player clicks cancel when choosing attack targets
                if (GUI.Button(new Rect(Screen.width - 100, 150, 100, 20), "Cancel"))
                {
                    OnAttackCancelClicked();
                }
            }
            else if (moving || attacking)
            {
                //Don't show anything
            }

            //Unit has not moved nor attacked yet
            else if (!hasMoved && !hasAttacked)
            {
                GUI.Box(new Rect(Screen.width - 100, 130, 100, 100), "Action");

                //Player clicks attack
                if (GUI.Button(new Rect(Screen.width - 100, 150, 100, 20), "Attack"))
                {
                    OnAttackClicked();
                }

                //Player clicks wait
                if (GUI.Button(new Rect(Screen.width - 100, 170, 100, 20), "Wait"))
                {
                    OnWaitClicked();
                }

            }

            //Unit has attacked but not moved
            else if (!hasMoved && hasAttacked)
            {
                GUI.Box(new Rect(Screen.width - 100, 130, 100, 100), "Action");

                //Player clicks wait after attacking
                if (GUI.Button(new Rect(Screen.width - 100, 170, 100, 20), "Wait"))
                {
                    OnWaitClicked();
                }
            }

            //Unit has moved but not attacked
            else if (hasMoved && !hasAttacked)
            {
                GUI.Box(new Rect(Screen.width - 100, 130, 100, 100), "Action");

                //Player clicks attack after moving
                if (GUI.Button(new Rect(Screen.width - 100, 150, 100, 20), "Attack"))
                {
                    OnAttackClicked();
                }

                //Player clicks wait after moving
                if (GUI.Button(new Rect(Screen.width - 100, 170, 100, 20), "Wait"))
                {
                    OnWaitClicked();
                }
            }

            //Unit has already moved and attacked
            else if (hasMoved && hasAttacked)
            {
                GUI.Box(new Rect(Screen.width - 100, 130, 100, 100), "Action");

                //Player clicks wait after moving
                if (GUI.Button(new Rect(Screen.width - 100, 150, 100, 20), "Wait"))
                {
                    OnWaitClicked();
                }
            }
        }
    }

    private void OnAttackClicked()
    {
        choosingAttackTargets = true;
    }

    private void OnWaitClicked()
    {
        NewTurnManager.EndTurn(this);
    }

    private void OnAttackCancelClicked()
    {
        choosingAttackTargets = false;
    }
}
