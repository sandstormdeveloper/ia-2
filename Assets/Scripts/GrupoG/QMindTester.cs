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
            using (StreamReader reader = new StreamReader(filePath))
            {
                string header = reader.ReadLine(); // Leer la primera línea con los nombres de columnas

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(' ');

                    // Extraer valores del estado desde la línea
                    State state = new State(null, null, null)
                    {
                        NWall = parts[0] == "1",
                        SWall = parts[1] == "1",
                        EWall = parts[2] == "1",
                        OWall = parts[3] == "1",
                        NPlayer = parts[4] == "1",
                        SPlayer = parts[5] == "1",
                        EPlayer = parts[6] == "1",
                        OPlayer = parts[7] == "1",
                        playerDistance = int.Parse(parts[8])
                    };

                    // Leer valores Q de las acciones
                    for (int action = 0; action < 4; action++)
                    {
                        if (float.TryParse(parts[action + 9], out float qValue))
                        {
                            QTable[(state, action)] = qValue;
                        }
                    }
                }
            }
            return QTable;
        }
    }
}
