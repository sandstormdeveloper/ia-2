using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System;

namespace GrupoG
{
    public class QMindTester : IQMind
    {
        private Dictionary<(State, int), float> QTable; // Tabla Q
        private WorldInfo _worldInfo;
        string filePath = "Assets/Scripts/GrupoG/TablaQ.csv"; // Archivo .csv donde se guarda la tabla

        // Se inicializa la tabla Q y se cargan los valores guardados
        public void Initialize(WorldInfo worldInfo)
        {
            Debug.Log("QMindDummy: initialized");
            _worldInfo = worldInfo;
            QTable = new Dictionary<(State, int), float>();
            QTable = LoadQTable(filePath);
        }

        // Se ejecuta cada paso y busca la mejor acción según la tabla Q
        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            Debug.Log("QMindDummy: GetNextStep");
            State state = new State(currentPosition, otherPosition, _worldInfo);
            int action = GetBestAction(state);

            return _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
        }

        // Se escoge la mejor acción para el estado actual
        private int GetBestAction(State state)
        {
            float maxQValue = float.MinValue;
            int bestAction = 0;

            for (int action = 0; action < 4; action++)
            {
                float qValue = GetQValue(state, action);
                if (qValue > maxQValue)
                {
                    maxQValue = qValue;
                    bestAction = action;
                }
            }

            return bestAction;
        }

        // Se busca el valor en la tabla
        private float GetQValue(State state, int action)
        {
            return QTable.TryGetValue((state, action), out float value) ? value : 0f;
        }

        // Se carga la tabla
        public Dictionary<(State, int), float> LoadQTable(string filePath)
        {
            Dictionary<(State, int), float> qTable = new Dictionary<(State, int), float>();

            if (!File.Exists(filePath))
                return qTable;

            string[] lines = File.ReadAllLines(filePath);

            // Saltar la cabecera (línea 0)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                bool nWall = bool.Parse(values[0]);
                bool sWall = bool.Parse(values[1]);
                bool eWall = bool.Parse(values[2]);
                bool oWall = bool.Parse(values[3]);

                bool nPlayer = bool.Parse(values[4]);
                bool sPlayer = bool.Parse(values[5]);
                bool ePlayer = bool.Parse(values[6]);
                bool oPlayer = bool.Parse(values[7]);

                int playerDistance = int.Parse(values[8]);
                int action = ToIntValue(values[9]);
                float qValue = float.Parse(values[10]);

                State state = new State(null, null, null)
                {
                    NWall = nWall,
                    SWall = sWall,
                    EWall = eWall,
                    OWall = oWall,
                    NPlayer = nPlayer,
                    SPlayer = sPlayer,
                    EPlayer = ePlayer,
                    OPlayer = oPlayer,
                    playerDistance = playerDistance
                };

                qTable[(state, action)] = qValue;
            }

            return qTable;
        }

        int ToIntValue(string action)
        {
            switch (action)
            {
                case "Up":
                    return 0;

                case "Right":
                    return 1;

                case "Down":
                    return 2;

                case "Left":
                    return 3;

                default:
                    return -1;
            }
        }
    }
}
