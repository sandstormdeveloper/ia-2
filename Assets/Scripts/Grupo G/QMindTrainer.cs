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
using UnityEditorInternal;
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
        float total_reward = 0;
        string filePath = "Assets/Scripts/Grupo G/TablaQ.csv";
        int saveRate = 0;

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
            _qMindTrainerParams = qMindTrainerParams;

            if (PlayerPrefs.HasKey("e"))
            {
                _qMindTrainerParams.epsilon = PlayerPrefs.GetFloat("e");
            }

            QTable = new Dictionary<(State, int), float>();
            QTable = LoadQTable(filePath);
            ResetEnvironment();
        }

        //DESMARCAR SHOW SIMULATION PARA ENTRENARLO

        public void DoStep(bool train)
        {
            if (AgentPosition == OtherPosition || !AgentPosition.Walkable || CurrentStep >= 1000)
            {
                ReturnAveraged = Mathf.Round((ReturnAveraged * 0.9f + Return * 0.1f) * 100) / 100;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                saveRate += 1;

                if (saveRate % _qMindTrainerParams.episodesBetweenSaves == 0)
                {
                    SaveQTableToCsv(filePath);
                }

                _qMindTrainerParams.epsilon = Mathf.Max(0.01f, _qMindTrainerParams.epsilon * 0.999f);

                PlayerPrefs.SetFloat("e", _qMindTrainerParams.epsilon);
                PlayerPrefs.Save();

                ResetEnvironment();
                return;
            }

            State currentState = new State(AgentPosition, OtherPosition, _worldInfo);
            int action = SelectAction(currentState);
            (CellInfo newAgentPosition, CellInfo newOtherPosition) = UpdateEnvironment(action);
            State nextState = new State(newAgentPosition, newOtherPosition, _worldInfo);
            float reward = CalculateReward(AgentPosition, OtherPosition, newAgentPosition, newOtherPosition);
            total_reward += reward;
            Return = Mathf.Round(total_reward * 10) / 10;
            
            if (train)
            {
                UpdateQTable(currentState, action, reward, nextState);
            }

            AgentPosition = newAgentPosition;
            OtherPosition = newOtherPosition;
            CurrentStep++;

            /*int action = Random.Range(0, 4);
            CellInfo newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            AgentPosition = newAgentPosition;
            OtherPosition = path[0];
            Debug.Log("QMindTrainerDummy: DoStep");
            */
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
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("State Action QValue");
                foreach (var entry in QTable)
                {
                    string stateId = entry.Key.Item1.StateId();
                    int action = entry.Key.Item2;
                    float qValue = entry.Value;
                    writer.WriteLine($"{stateId} {action} {qValue}");
                }
            }
            Debug.Log($"QTable saved successfully to {filePath}");
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

        private float CalculateReward(CellInfo AgentPosition, CellInfo OtherPosition, CellInfo newAgentPosition, CellInfo newOtherPosition)
        {
            float reward = 0f;

            if (newAgentPosition == newOtherPosition)
            {
                Debug.Log("Agent was caught");
                reward -= 100f;
            }

            if (newAgentPosition.Type == CellInfo.CellType.Limit)
            {
                Debug.Log("Agent went out of bounds");
                reward -= 50f;
            }

            if (newAgentPosition.Type == CellInfo.CellType.Wall)
            {
                Debug.Log("Agent went into a wall");
                reward -= 10f;
            }

            if (newAgentPosition.Distance(newOtherPosition, CellInfo.DistanceType.Euclidean) < AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Euclidean))
            {
                reward -= 1f;
            }

            if (newAgentPosition.Distance(newOtherPosition, CellInfo.DistanceType.Euclidean) >= AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Euclidean))
            {
                reward += 1f;
            }

            return reward;
        }

        private void ResetEnvironment()
        {
            AgentPosition = _worldInfo.RandomCell();
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
            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPosition = path.Length > 0 ? path[0] : OtherPosition;

            return (newAgentPosition, newOtherPosition);
        }
    }
}
