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

        public bool playerAbove; // El jugador esta encima
        public bool playerRight; // El jugador esta a la derecha

        public int playerDistance; // Distancia al jugador

        public State(CellInfo AgentPos, CellInfo OtherPos, WorldInfo worldInfo)
        {
            if (AgentPos != null && OtherPos != null && worldInfo != null)
            {
                NWall = !worldInfo.NextCell(AgentPos, Directions.Up).Walkable;
                SWall = !worldInfo.NextCell(AgentPos, Directions.Down).Walkable;
                EWall = !worldInfo.NextCell(AgentPos, Directions.Right).Walkable;
                OWall = !worldInfo.NextCell(AgentPos, Directions.Left).Walkable;

                playerAbove = OtherPos.y > AgentPos.y;
                playerRight = OtherPos.x > AgentPos.x;
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

        // Genera un ID único para cada estado
        public string StateId()
        {
            string id = "";

            id += NWall ? 1 : 0;
            id += SWall ? 1 : 0;
            id += EWall ? 1 : 0;
            id += OWall ? 1 : 0;

            id += playerAbove ? 1 : 0;
            id += playerRight ? 1 : 0;

            id += playerDistance;

            return id;
        }
    }
}
