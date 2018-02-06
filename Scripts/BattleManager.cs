using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {

    private static List<TacticsMove> players;
    private static List<TacticsMove> enemies;
    private static Queue<List<TacticsMove>> turnQueue;
    private static List<TacticsMove> currentTeam;

	public static void StartBattle(List<GameObject> playerList,
                        List<GameObject> enemyList)
    {
        players = new List<TacticsMove>();
        enemies = new List<TacticsMove>();

        foreach (GameObject player in playerList)
        {
            players.Add(player.GetComponent<TacticsMove>());
        }

        foreach (GameObject enemy in enemyList)
        {
            enemies.Add(enemy.GetComponent<TacticsMove>());
        }

        turnQueue = new Queue<List<TacticsMove>>();
        turnQueue.Enqueue(players);
        turnQueue.Enqueue(enemies);

        BeginTurn();

    }

    private static void BeginTurn()
    {
        currentTeam = turnQueue.Dequeue();
        NewTurnManager.StartTurn(currentTeam);
    }

    public static void KillUnit(TacticsMove unit)
    {
        //Get tag and remove from list
        if (unit.tag.Equals(Tag.PLAYER))
        {
            players.Remove(unit);
            if (players.Count == 0)
            {
                EnemyVictory();
            }
        }
        else if (unit.tag.Equals(Tag.NPC))
        {
            enemies.Remove(unit);
            PlayerVictory();
        }

    }

    public static void SwitchTurn()
    {
        turnQueue.Enqueue(currentTeam);
        BeginTurn();
    }

    private static void EnemyVictory()
    {
        Application.Quit();
    }

    private static void PlayerVictory()
    {
        Application.Quit();
    }
}
