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

        private Dictionary<(State, int), float> QTable;
        private WorldInfo _worldInfo;
        string filePath = "TablaQ.csv";

        public void Initialize(WorldInfo worldInfo)
        {
            Debug.Log("QMindDummy: initialized");
            _worldInfo = worldInfo;
            QTable = LoadQTable(filePath);
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            Debug.Log("QMindDummy: GetNextStep");
            State state = new State(currentPosition, otherPosition, _worldInfo);
            int action = GetBestAction(state);
            return _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
        }

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

        private float GetQValue(State state, int action)
        {
            return QTable.TryGetValue((state, action), out float value) ? value : 0f;
        }

        public Dictionary<(State, int), float> LoadQTable(string filePath)
        {
           
            using (StreamReader reader = new StreamReader(filePath)) {
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length != 3)
                        {
                            Debug.LogWarning($"Formato incorrecto en la linea: {line}");
                            continue;
                        }

                        try
                        {
                            string stateString = parts[0];
                            State state = ParseState(stateString);

                            int action = int.Parse(parts[1]);

                            float qValue = float.Parse(parts[2]);

                            QTable[(state, action)] = qValue;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error al procesar la linea: {line}. {ex.Message}");
                        }
                    }
                }
                Debug.Log("Tabla q cargada correctamente.");
                return QTable;
            }
        }

        private State ParseState(string stateString)
        {
            string[] pos = stateString.Split(';');
            if(pos.Length != 2)
            {
                throw new FormatException($"Formato de estado invalido: {stateString}");
            }

            CellInfo agentPosition = ParseCellInfo(pos[0]);  
            CellInfo otherPosition = ParseCellInfo(pos[1]);

            return new State(agentPosition, otherPosition, _worldInfo);
        }

        private CellInfo ParseCellInfo(string cellString)
        {
            string[] coord = cellString.Split(';');
            if (coord.Length != 3)
            {
                throw new FormatException($"Formato de CellInfo invalido: {cellString}"); 
            }

            float x = float.Parse(coord[0]);
            float y = float.Parse(coord[1]);
            float z = float.Parse(coord[2]);

            return new CellInfo(new Vector3(x, y, z));
        }
    }
}
