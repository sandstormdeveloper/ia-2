 using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GrupoG
{
    public class QMindTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; }
        public int CurrentStep { get; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; }
        public float ReturnAveraged { get; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private QMindTrainerParams _qMindTrainerParams;
        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;
        private bool terminal_state = false;
        private Dictionary<(State, int), float> QTable;


        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _qMindTrainerParams = qMindTrainerParams;
            _navigationAlgorithm.Initialize(_worldInfo);

            Debug.Log("QMindTrainerDummy: initialized");
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        //DESMARCAR SHOW SIMULATION PARA ENTRENARLO

        public void DoStep(bool train)
        { 
            if (terminal_state)
            {
                ReturnAveraged = (float)(ReturnAveraged*0.9 + Return*0.1);
                ResetEnvironment();
            }

            State state = new State(AgentPosition, OtherPosition);
            int action = selectAction(state);
            (CellInfo newAgentPosition, CellInfo newOtherPosition) = UpdateEnvironment(action);
            State nextState = new State(newAgentPosition, newOtherPosition);
            float reward = CalculateReward(newAgentPosition, newOtherPosition);
            
            if (train)
            {
                UpdateQTable(state, action, reward, nextState);
            }

            AgentPosition = newAgentPosition;
            OtherPosition = newOtherPosition;
            
            

            /*int action = Random.Range(0, 4);
            CellInfo newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            AgentPosition = newAgentPosition;
            OtherPosition = path[0];
            Debug.Log("QMindTrainerDummy: DoStep");
            */
        }

        //Función para determinar cuando el jugador ha pillado al agente (ha llegado a su misma casilla)
        private void terminalState(CellInfo agentPosition, CellInfo otherPosition)
        {
            terminal_state = agentPosition == otherPosition;
        }    


        //Función para seleccionar la acción que va a realizar el agente
        private int selectAction(State state)
        {
            if (Random.Range(0f, 1f) < _qMindTrainerParams.epsilon)
            {
                return Random.Range(0, 4);
            }

            return getBestAction(state);
        }

        private int getBestAction(State state)
        {
            float maxQValue = float.MinValue;
            int bestAction = 0;

            for (int action = 0; action < 4; action++)
            {
                float qValue = getQValue(state, action);
                if (qValue > maxQValue) 
                {
                   maxQValue = qValue;
                   bestAction = action;
                }
            }

            return bestAction;
        }

        private float getQValue(State state, int action)
        {
            var key = (state, action);
            return QTable.ContainsKey(key) ? QTable[key] : 0f;
        }
        private void UpdateQTable(State state, int action, float reward, State nextState)
        {
            var key = (state, action);

            if (QTable.ContainsKey(key))
            {
                float current_QValue = QTable[key];
                QTable[key] = (current_QValue + reward) / 2f;
            }
            else
            {
                QTable[key] = reward;
            }
        }

        private float CalculateReward(CellInfo AgentPosition, CellInfo OtherPosition)
        {
            if (AgentPosition == OtherPosition)
            {
                return -1f;
            }

            return -0.1f;
        }

        private void ResetEnvironment()
        {
            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
            Debug.Log("Reseting environment");
        }

        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            CellInfo newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));

            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPosition = path[0];

            return (newAgentPosition, newOtherPosition);
        }
    }
}
