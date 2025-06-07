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
        private Dictionary<(State, int), float> QTable; // Tabla Q

        float total_reward = 0; // Recompensa total de este episodio
        string filePath = "Assets/Scripts/GrupoG/TablaQ.csv"; // Archivo .csv donde se guarda la tabla
        int saveRate = 0; // Para calcular la frecuencia de guardado

        // Se carga la tabla Q desde el archivo y se inicializa el mundo para el primer episodio
        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            Application.runInBackground = true;

            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
            _qMindTrainerParams = qMindTrainerParams;

            QTable = new Dictionary<(State, int), float>();
            InitializeQTable();
            var loadedQTable = LoadQTable(filePath);
            foreach (var entry in loadedQTable)
            {
                QTable[entry.Key] = entry.Value; // Sobrescribe si existe
            }
            ResetEnvironment();
        }

        private void InitializeQTable()
        {
            QTable = new Dictionary<(State, int), float>();

            foreach (var state in GenerateAllPossibleStates())
            {
                for (int action = 0; action < 4; action++)
                {
                    QTable[(state, action)] = 0f;
                }
            }
        }

        // Algoritmo principal, se ejecuta cada paso
        public void DoStep(bool train)
        {
            if (AgentPosition == OtherPosition || !AgentPosition.Walkable || (CurrentStep >= _qMindTrainerParams.maxSteps && _qMindTrainerParams.maxSteps != -1)) // Estado terminal, finaliza el episodio
            {
                ReturnAveraged = Mathf.Round((ReturnAveraged * 0.9f + Return * 0.1f) * 100) / 100;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                saveRate += 1;

                if (saveRate % _qMindTrainerParams.episodesBetweenSaves == 0)
                {
                    SaveQTableToCsv(filePath);
                }


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
            
            if (train) // Si se esta entrenando, se actualiza la tabla
            {
                UpdateQTable(currentState, action, reward, nextState);
            }

            AgentPosition = newAgentPosition;
            OtherPosition = newOtherPosition;
            CurrentStep++;
        }

        //Función para seleccionar la acción que va a realizar el agente
        private int SelectAction(State state)
        {
            if (Random.Range(0f, 1f) < _qMindTrainerParams.epsilon) // El epsilon es la exploración, cuando es más alto, es más probable que la accion sea aleatoria
            {
                return Random.Range(0, 4);
            }

            return GetBestAction(state);
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

        // Se actualiza la tabla 
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

        private List<State> GenerateAllPossibleStates()
        {
            List<State> states = new List<State>();

            for (int nWall = 0; nWall <= 1; nWall++)
            {
                for (int sWall = 0; sWall <= 1; sWall++)
                {
                    for (int eWall = 0; eWall <= 1; eWall++)
                    {
                        for (int oWall = 0; oWall <= 1; oWall++)
                        {
                            for (int nPlayer = 0; nPlayer <= 1; nPlayer++)
                            {
                                for (int sPlayer = 0; sPlayer <= 1; sPlayer++)
                                {
                                    for (int ePlayer = 0; ePlayer <= 1; ePlayer++)
                                    {
                                        for (int oPlayer = 0; oPlayer <= 1; oPlayer++)
                                        {
                                            for (int playerDistance = 0; playerDistance <= 2; playerDistance++)
                                            {
                                                states.Add(new State(null, null, null)
                                                {
                                                    NWall = nWall == 1,
                                                    SWall = sWall == 1,
                                                    EWall = eWall == 1,
                                                    OWall = oWall == 1,
                                                    NPlayer = nPlayer == 1,
                                                    SPlayer = sPlayer == 1,
                                                    EPlayer = ePlayer == 1,
                                                    OPlayer = oPlayer == 1,
                                                    playerDistance = playerDistance
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return states;
        }

        // Se guarda la tabla en el archivo .csv externo
        public void SaveQTableToCsv(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Escribir cabecera separada por espacios
                writer.WriteLine("NWall SWall EWall OWall NPlayer SPlayer EPlayer OPlayer PlayerDistance Action QValue");

                // Escribir cada entrada de la tabla Q
                foreach (var entry in QTable)
                {
                    State s = entry.Key.Item1;
                    int action = entry.Key.Item2;
                    float qValue = entry.Value;

                    string line = $"{s.NWall} {s.SWall} {s.EWall} {s.OWall} " +
                                  $"{s.NPlayer} {s.SPlayer} {s.EPlayer} {s.OPlayer} " +
                                  $"{s.playerDistance} {_worldInfo.AllowedMovements.FromIntValue(action)} {qValue}";

                    writer.WriteLine(line);
                }
            }
        }

        // Se carga la tabla
        public Dictionary<(State, int), float> LoadQTable(string filePath)
        {
            Dictionary<(State, int), float> qTable = new Dictionary<(State, int), float>();

            if (!File.Exists(filePath))
                return qTable;

            string[] lines = File.ReadAllLines(filePath);

            // Saltar la cabecera (línea 0)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                bool nWall = bool.Parse(values[0]);
                bool sWall = bool.Parse(values[1]);
                bool eWall = bool.Parse(values[2]);
                bool oWall = bool.Parse(values[3]);

                bool nPlayer = bool.Parse(values[4]);
                bool sPlayer = bool.Parse(values[5]);
                bool ePlayer = bool.Parse(values[6]);
                bool oPlayer = bool.Parse(values[7]);

                int playerDistance = int.Parse(values[8]);
                int action = ToIntValue(values[9]);
                float qValue = float.Parse(values[10]);

                State state = new State(null, null, null)
                {
                    NWall = nWall,
                    SWall = sWall,
                    EWall = eWall,
                    OWall = oWall,
                    NPlayer = nPlayer,
                    SPlayer = sPlayer,
                    EPlayer = ePlayer,
                    OPlayer = oPlayer,
                    playerDistance = playerDistance
                };

                qTable[(state, action)] = qValue;
            }

            return qTable;
        }

        int ToIntValue(string action)
        {
            switch(action)
            {
                case "Up":
                    return 0;

                case "Right":
                    return 1;

                case "Down":
                    return 2;

                case "Left":
                    return 3;

                default:
                    return -1;
            }
        }

        // Se calcula la recompensa otorgada, según la acción tomada
        private float CalculateReward(CellInfo AgentPosition, CellInfo OtherPosition, CellInfo newAgentPosition, CellInfo newOtherPosition)
        {
            float distance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Euclidean);
            float newDistance = newAgentPosition.Distance(newOtherPosition, CellInfo.DistanceType.Euclidean);

            if(newAgentPosition == newOtherPosition || !newAgentPosition.Walkable) // Si se alcanza un estado terminal
            {
                return -100f;
            }

            if (newDistance < distance) // Si se acerca al jugador
            {
                return -10f;
            } 
            else if (newDistance == distance) 
            {
                return 0f;
            }
            else 
            {
                return 50f;
            }
        }

        // Se reinicia el mapa antes de comenzar el siguiente episodio
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

        // Se actualizan las posiciones del agente (tabla Q) y del jugador (A*)
        private (CellInfo, CellInfo) UpdateEnvironment(int action)
        {
            CellInfo newAgentPosition = AgentPosition;

            newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));

            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPosition = path.Length > 0 ? path[0] : OtherPosition;

            return (newAgentPosition, newOtherPosition);
        }
    }
}
