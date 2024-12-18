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
            /*
            if(terminal_state)
            {
                state = RandomState();
            }

            state = GetStateGraph(AgentPosition, OtherPosition);
            action = selectAction(state, available_actions);
            next_state, reward = Random.Range(0, 4);
            updateQTable(State, Action, next_state, reward);

            State state = new State(AgentPosition, OtherPosition);
            int action = selectAction(state);
            (CellInfo newAgentPosition, CellInfo newOtherPosition) = UpdateEnvironment(action);
            State nextState = new State(newAgentPosition, newOtherPosition);
            float reward = CalculateReward(newAgentPositionm, newOtherPosition);
            UpdateQTable(state, action)
            */

            int action = Random.Range(0, 4);
            CellInfo newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            AgentPosition = newAgentPosition;
            OtherPosition = path[0];
            Debug.Log("QMindTrainerDummy: DoStep");
        }

        private int selectAction(State state)
        {
            if (Random.Range(0f, 1f) < _qMindTrainerParams.epsilon)
            {

            }

        }
    }
}
