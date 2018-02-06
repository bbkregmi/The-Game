using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGUI : MonoBehaviour {

    private static bool isOpen;
    private static string playerStats;
    private static BasePlayer player;
    private static int maxHealth;
    private static Texture2D tex;
    private static Rect healthOutline;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static void SetPlayer(BasePlayer newPlayer)
    {
        player = newPlayer;
        GenerateInfoString();
        isOpen = true;
        maxHealth = player.PlayerMaxHealth;
        tex = new Texture2D(50, 20);
    }

    private static void GenerateInfoString()
    {
        playerStats = player.PlayerName + "  " +
            player.PlayerClass.CharacterClassName + "\n" +
            "Health: " + player.PlayerHealth +
            "  Mana: " + player.PlayerMana + "\n" +
            "Atk: " + player.PlayerAttack +
            "  Def: " + player.PlayerDefense +
            "  Spd: " + player.PlayerSpeed + "\n" +
            "M.Atk: " + player.PlayerMagicAttack +
            "  M.Def: " + player.PlayerMagicDefense;
    }
    
    private void OnGUI()
    {
        if (isOpen)
        {
            GUI.Box(new Rect(Screen.width - 200, 0, 200, 100), playerStats);
            Rect rec = new Rect(Screen.width - 200, 70,
                (player.PlayerHealth / (float)maxHealth) * 100, 10);
            GUI.Label(new Rect(Screen.width - 90, 65, 90, 20),
                player.PlayerHealth + "/" + maxHealth);
            GUI.color = Color.red;
            GUI.DrawTexture(rec, tex);
            

        }
    }
}
