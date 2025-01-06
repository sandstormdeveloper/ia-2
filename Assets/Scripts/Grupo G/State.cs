using NavigationDJIA.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public bool NWall;
    public bool SWall;
    public bool EWall;
    public bool OWall;

    public bool playerAbove;
    public bool playerRight;

    public int playerDistance;
    
    public State(CellInfo AgentPos, CellInfo OtherPos, WorldInfo worldInfo)
    {
        NWall = !worldInfo.NextCell(AgentPos, Directions.Up).Walkable;
        SWall = !worldInfo.NextCell(AgentPos, Directions.Down).Walkable;
        EWall = !worldInfo.NextCell(AgentPos, Directions.Right).Walkable;
        OWall = !worldInfo.NextCell(AgentPos, Directions.Left).Walkable;

        playerAbove = OtherPos.y > AgentPos.y;
        playerRight = OtherPos.x > AgentPos.x;
        playerDistance = BinDistance(AgentPos.Distance(OtherPos, CellInfo.DistanceType.Euclidean));
    }

    private int BinDistance(float distance)
    {
        if (distance < 3f) return 0;  // Cerca
        else if (distance < 6f) return 1;  // Normal
        return 2;  // Lejos
    }

    public override bool Equals(object obj)
    {
        return obj is State state &&
               NWall == state.NWall &&
               SWall == state.SWall &&
               EWall == state.EWall &&
               OWall == state.OWall &&
               playerAbove == state.playerAbove &&
               playerRight == state.playerRight &&
               playerDistance == state.playerDistance;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(NWall);
        hash.Add(SWall);
        hash.Add(EWall);
        hash.Add(OWall);
        hash.Add(playerAbove);
        hash.Add(playerRight);
        hash.Add(playerDistance);
        return hash.ToHashCode();
    }
}
