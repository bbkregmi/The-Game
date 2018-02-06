using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class finds all the units on the map, and places them
 * on separate lists based on their tag.
 */
public class Initialize : MonoBehaviour {

    List<GameObject> playerList;
    List<GameObject> enemyList;

	// Use this for initialization
	void Start () {
        playerList = new List<GameObject>(
            GameObject.FindGameObjectsWithTag(Tag.PLAYER)
            );

        enemyList = new List<GameObject>(
            GameObject.FindGameObjectsWithTag(Tag.NPC)
            );

        //Pass the list to BattleManager to begin battle
        BattleManager.StartBattle(playerList, enemyList);
	}

}
