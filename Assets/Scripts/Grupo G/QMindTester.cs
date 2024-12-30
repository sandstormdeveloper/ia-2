using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrupoG
{
    public class QMindTester : IQMind
    {

        private Dictionary<(int, int), float> QTable;

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
        
        public Dictionary<(int, int), float> LoadQTable()
        {
            return null; 
        }
        
    }
}
