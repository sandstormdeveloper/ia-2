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

    public float PlayerDistance;
    
    public State(CellInfo AgentPos, CellInfo OtherPos)
    {
        //PlayerDistance = ;
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
               PlayerDistance == state.PlayerDistance;
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
        hash.Add(PlayerDistance);
        return hash.ToHashCode();
    }
}
