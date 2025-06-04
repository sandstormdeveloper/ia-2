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
            QTable = LoadQTable(filePath);
            ResetEnvironment();
        }

        private void InitializeQTable()
        {
            QTable = new Dictionary<(State, int), float>();

            foreach (var state in GenerateAllPossibleStates())
            {
                for (int action = 0; action < 5; action++)
                {
                    QTable[(state, action)] = 0f;
                }
            }
        }

        // Algoritmo principal, se ejecuta cada paso
        public void DoStep(bool train)
        {
            if (AgentPosition == OtherPosition || !AgentPosition.Walkable || CurrentStep >= 1000) // Estado terminal, finaliza el episodio
            {
                ReturnAveraged = Mathf.Round((ReturnAveraged * 0.9f + Return * 0.1f) * 100) / 100;
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                saveRate += 1;

                if (saveRate % _qMindTrainerParams.episodesBetweenSaves == 0)
                {
                    SaveQTableToCsv(filePath);
                }

                _qMindTrainerParams.epsilon = Mathf.Max(0.01f, _qMindTrainerParams.epsilon * 0.9999f); // Se va reduciendo el epsilon

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
                return Random.Range(0, 5);
            }

            return GetBestAction(state);
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

        // Se actualiza la tabla 
        private void UpdateQTable(State state, int action, float reward, State nextState)
        {
            float currentQ = GetQValue(state, action);
            float maxNextQ = float.MinValue;

            for (int nextAction = 0; nextAction < 5; nextAction++)
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
                // Escribir encabezado con cada atributo del estado y las acciones
                writer.WriteLine("NWall SWall EWall OWall NPlayer SPlayer EPlayer OPlayer Dist Action0 Action1 Action2 Action3 Action4");

                HashSet<string> statesWritten = new HashSet<string>();

                foreach (var entry in QTable)
                {
                    State state = entry.Key.Item1;

                    // Serializar cada atributo del estado en columnas separadas
                    string stateString = $"{(state.NWall ? 1 : 0)} " +
                                         $"{(state.SWall ? 1 : 0)} " +
                                         $"{(state.EWall ? 1 : 0)} " +
                                         $"{(state.OWall ? 1 : 0)} " +
                                         $"{(state.NPlayer ? 1 : 0)} " +
                                         $"{(state.SPlayer ? 1 : 0)} " +
                                         $"{(state.EPlayer ? 1 : 0)} " +
                                         $"{(state.OPlayer ? 1 : 0)} " +
                                         $"{state.playerDistance}";

                    if (!statesWritten.Contains(stateString))
                    {
                        writer.Write(stateString); // Escribe el estado sin ID codificado

                        for (int action = 0; action < 5; action++)
                        {
                            writer.Write($" {GetQValue(state, action),8:F2}");
                        }

                        writer.WriteLine();
                        statesWritten.Add(stateString);
                    }
                }
            }
            Debug.Log($"QTable saved successfully to {filePath}");
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
                    for (int action = 0; action < 5; action++)
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

            if (action < 4)
            {
                newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            }
            
            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            CellInfo newOtherPosition = path.Length > 0 ? path[0] : OtherPosition;

            return (newAgentPosition, newOtherPosition);
        }
    }
}
