using NavigationDJIA.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoG
{
    public class State
    {
        public bool NWall; // Pared al norte
        public bool SWall; // Pared al sur
        public bool EWall; // Pared al este
        public bool OWall; // Pared al oeste

        public bool NPlayer; // Pared al norte
        public bool SPlayer; // Pared al sur
        public bool EPlayer; // Pared al este
        public bool OPlayer; // Pared al oeste

        public int playerDistance; // Distancia al jugador

        public State(CellInfo AgentPos, CellInfo OtherPos, WorldInfo worldInfo)
        {
            if (AgentPos != null && OtherPos != null && worldInfo != null)
            {
                NWall = !worldInfo.NextCell(AgentPos, Directions.Up).Walkable;
                SWall = !worldInfo.NextCell(AgentPos, Directions.Down).Walkable;
                EWall = !worldInfo.NextCell(AgentPos, Directions.Right).Walkable;
                OWall = !worldInfo.NextCell(AgentPos, Directions.Left).Walkable;

                NPlayer = AgentPos.y < OtherPos.y;
                SPlayer = AgentPos.y > OtherPos.y;
                EPlayer = AgentPos.x < OtherPos.x;
                OPlayer = AgentPos.x > OtherPos.x;

                playerDistance = BinDistance(AgentPos.Distance(OtherPos, CellInfo.DistanceType.Euclidean));
            }
        }

        // Se simplifica la distancia al jugador, para que sea más facil de guardar en la tabla
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
                   NPlayer == state.NPlayer &&
                   SPlayer == state.SPlayer &&
                   EPlayer == state.EPlayer &&
                   OPlayer == state.OPlayer &&
                   playerDistance == state.playerDistance;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(NWall);
            hash.Add(SWall);
            hash.Add(EWall);
            hash.Add(OWall);
            hash.Add(NPlayer);
            hash.Add(SPlayer);
            hash.Add(EPlayer);
            hash.Add(OPlayer);
            hash.Add(playerDistance);
            return hash.ToHashCode();
        }
    }
}
