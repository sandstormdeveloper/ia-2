using JetBrains.Annotations;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace GrupoG
{
    public class QMindTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; private set; }
        public float ReturnAveraged { get; private set; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private QMindTrainerParams _qMindTrainerParams;
        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;
        private Dictionary<(State, int), float> QTable;
        private bool terminal_state = false;
        float total_reward = 0;
        private CellInfo lastValidPosition = null;
        string filePath = "TablaQ.csv";
        int saveRate = 0;

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            saveRate = qMindTrainerParams.episodesBetweenSaves;
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
            _qMindTrainerParams = qMindTrainerParams;
            QTable = new Dictionary<(State, int), float>();
            ResetEnvironment();
        }

        //DESMARCAR SHOW SIMULATION PARA ENTRENARLO

        public void DoStep(bool train)
        {
            saveRate -= 1;

            if (terminal_state)
            {
                ReturnAveraged = Mathf.Round((ReturnAveraged * 0.9f + Return * 0.1f) * 100) / 100;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                ResetEnvironment();
            }

            State currentState = new State(AgentPosition, OtherPosition, _worldInfo);
            int action = SelectAction(currentState);
            (CellInfo newAgentPosition, CellInfo newOtherPosition) = UpdateEnvironment(action);
            State nextState = new State(newAgentPosition, newOtherPosition, _worldInfo);
            float reward = CalculateReward(newAgentPosition, newOtherPosition);
            total_reward += reward;
            Return = Mathf.Round(total_reward * 10) / 10;
            
            if (train)
            {
                UpdateQTable(currentState, action, reward, nextState);

                if (saveRate <= 0)
                {
                    SaveQTableToCsv(filePath);
                    saveRate = _qMindTrainerParams.episodesBetweenSaves;
                }
            }

            AgentPosition = newAgentPosition;
            OtherPosition = newOtherPosition;
            CurrentStep++;

            terminal_state = IsTerminalState();

            /*int action = Random.Range(0, 4);
            CellInfo newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            AgentPosition = newAgentPosition;
            OtherPosition = path[0];
            Debug.Log("QMindTrainerDummy: DoStep");
            */
        }

        private bool IsTerminalState()
        {
            if(AgentPosition == OtherPosition)
            {
                return true;
            }

            if(AgentPosition.Type == CellInfo.CellType.Limit)
            {
                return true;
            }

            return false;
        }

        //Función para seleccionar la acción que va a realizar el agente
        private int SelectAction(State state)
        {
            if (Random.Range(0f, 1f) < _qMindTrainerParams.epsilon)
            {
                return Random.Range(0, 4);
            }

            return GetBestAction(state);
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

        private void UpdateQTable(State state, int action, float reward, State nextState)
        {
            float currentQ = GetQValue(state, action);
            float maxNextQ = float.MinValue;

            for (int nextAction = 0; nextAction < 4; nextAction++)
            {
                float nextQ = GetQValue(nextState, nextAction);
                maxNextQ = MathF.Max(maxNextQ, nextQ);
            }

            float updatedQ = currentQ + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * maxNextQ - currentQ);
            QTable[(state, action)] = updatedQ;
        }

        public void SaveQTableToCsv(string filePath)
        {
            
        }

        private float CalculateReward(CellInfo AgentPosition, CellInfo OtherPosition)
        {
            if (AgentPosition == OtherPosition)
            {
                Debug.Log("Agent was caught");
                return -20f;
            }

            if (AgentPosition.Type == CellInfo.CellType.Limit)
            {
                Debug.Log("Agent went out of bounds");
                return -10f;
            }

            if (AgentPosition.Type == CellInfo.CellType.Wall)
            {
                Debug.Log("Agent went inside a wall");
                return -5f;
            }

            return 0.1f;
        }

        private void ResetEnvironment()
        {
            terminal_state = false;
            AgentPosition = _worldInfo.RandomCell();
            lastValidPosition = AgentPosition;
            OtherPosition = _worldInfo.RandomCell();
            CurrentEpisode++;
            CurrentStep = 0;
            Return = 0f;
            total_reward = 0f;
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
            Debug.Log("Reseting environment");
        }

        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            CellInfo newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));

            if(newAgentPosition.Walkable)
            {
                lastValidPosition = newAgentPosition;
            }

            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, lastValidPosition, 1);

            if (path != null)
            {
                if (path.Length > 0)
                {
                    CellInfo newOtherPosition = path[0];
                    return (newAgentPosition, newOtherPosition);
                }
            }

            return (newAgentPosition, OtherPosition);
        }
    }
}
