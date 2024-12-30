using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoG
{
    public class QMindTester : IQMind
    {

        private Dictionary<(State, int), float> QTable;

        public void Initialize(WorldInfo worldInfo)
        {
            Debug.Log("QMindDummy: initialized");
            QTable = LoadQTable();
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            Debug.Log("QMindDummy: GetNextStep");
            return null;
        }
        
        private Dictionary<(State, int), float> LoadQTable()
        {
            return new 
        }
        
    }
}
