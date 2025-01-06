using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
            return new Dictionary<(State, int), float>();
        }
        
    }
}
