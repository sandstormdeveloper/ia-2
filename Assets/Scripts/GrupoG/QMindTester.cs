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

            if (action < 4)
            {
                return _worldInfo.NextCell(currentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            }

            return currentPosition;
        }

        // Se escoge la mejor acción para el estado actual
        private int GetBestAction(State state)
        {
            float maxQValue = float.MinValue;
            int bestAction = 0;

            for (int action = 0; action < 5; action++)
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
                string header = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(' ');

                    string stateId = parts[0].Trim();
                    State state = ParseStateFromId(stateId);

                    for (int action = 0; action < 5; action++)
                    {
                        if (float.TryParse(parts[action + 1], out float qValue))
                        {
                            QTable[(state, action)] = qValue;
                        }
                    }
                }

                return QTable;
            }
        }

        // Se pasa del ID guardado en el .csv a un estado de la clase State
        private State ParseStateFromId(string stateId)
        {
            bool NWall = stateId[0] == '1';
            bool SWall = stateId[1] == '1';
            bool EWall = stateId[2] == '1';
            bool OWall = stateId[3] == '1';

            bool NPlayer = stateId[4] == '1';
            bool SPlayer = stateId[5] == '1';
            bool EPlayer = stateId[6] == '1';
            bool OPlayer = stateId[7] == '1';

            int playerDistance = int.Parse(stateId[8].ToString());

            return new State(null, null, null)
            {
                NWall = NWall,
                SWall = SWall,
                EWall = EWall,
                OWall = OWall,
                NPlayer = NPlayer,
                SPlayer = SPlayer,
                EPlayer = EPlayer,
                OPlayer = OPlayer,
                playerDistance = playerDistance
            };
        }
    }
}
