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
        string filePath = "Assets/Scripts/Grupo G/TablaQ.csv";

        public void Initialize(WorldInfo worldInfo)
        {
            Debug.Log("QMindDummy: initialized");
            _worldInfo = worldInfo;
            QTable = new Dictionary<(State, int), float>();
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

            using (StreamReader reader = new StreamReader(filePath))
            {
                string header = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(' ');

                    if (parts.Length == 3)
                    {
                        string stateId = parts[0];
                        int action = int.Parse(parts[1]);
                        float qValue = float.Parse(parts[2]);

                        State state = ParseStateFromId(stateId);

                        QTable[(state, action)] = qValue;
                    }
                }

                return QTable;
            }
        }

        private State ParseStateFromId(string stateId)
        {
            bool NWall = stateId[0] == '1';
            bool SWall = stateId[1] == '1';
            bool EWall = stateId[2] == '1';
            bool OWall = stateId[3] == '1';

            bool playerAbove = stateId[4] == '1';
            bool playerRight = stateId[5] == '1';

            int playerDistance = int.Parse(stateId[6].ToString());

            return new State(null, null, null)
            {
                NWall = NWall,
                SWall = SWall,
                EWall = EWall,
                OWall = OWall,
                playerAbove = playerAbove,
                playerRight = playerRight,
                playerDistance = playerDistance
            };
        }
    }
}
