using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewTurnManager : MonoBehaviour {

    //List of units in the team that is making their turn.
    private static List<TacticsMove> teamTurn;

    //List of units who have yet to execute their turn
    private static List<TacticsMove> hasNotMoved;

    //Unit who is currently taking t's turn
    private static TacticsMove currentUnit;

	public static void StartTurn(List<TacticsMove> currentTeam)
    {
        teamTurn = currentTeam;

        hasNotMoved = new List<TacticsMove>();
        foreach (TacticsMove unit in teamTurn)
        {
            unit.hasMoved = false;
            unit.hasAttacked = false;
            hasNotMoved.Add(unit);
        }

        ExecuteTurn();
    }

    private static void ExecuteTurn()
    {
        currentUnit = hasNotMoved[0];
        currentUnit.turn = true;
    }

    public static void ExecuteTurn(TacticsMove unit)
    {
        if (currentUnit != null && 
            currentUnit.hasMoved && currentUnit.hasAttacked)
        {
            hasNotMoved.Remove(currentUnit);
        }

        currentUnit = unit;
        currentUnit.turn = true;
    }

    public static void EndTurn(TacticsMove unit)
    {
        hasNotMoved.Remove(unit);
        unit.turn = false;
        if (hasNotMoved.Count > 0)
        {
            ExecuteTurn();
        }
        else
        {
            EndTeamTurn();
        }
    }

    private static void EndTeamTurn()
    {
        BattleManager.SwitchTurn();
    }


}
