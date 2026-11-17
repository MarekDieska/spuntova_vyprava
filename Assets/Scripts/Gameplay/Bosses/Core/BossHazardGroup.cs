using System;
using System.Collections;
using UnityEngine;

public class BossHazardGroup : MonoBehaviour
{
    [Serializable]
    public class HazardStage
    {
        public BossHazard[] hazards;
    }

    [Header("References")]
    [SerializeField] private BossBase boss;
    [SerializeField] private HazardStage[] stages;

    [Header("Stage Transition")]
    [SerializeField] private float stageTransitionDelay = 1.25f;

    private int currentStageIndex = 0;
    private int spentInCurrentStage = 0;
    private int activeHazardsInCurrentStage = 0;
    private bool isTransitioning = false;

    private void Awake()
    {
        for (int s = 0; s < stages.Length; s++)
        {
            BossHazard[] stageHazards = stages[s].hazards;
            for (int h = 0; h < stageHazards.Length; h++)
            {
                if (stageHazards[h] != null)
                {
                    stageHazards[h].OnBossHit += HandleHazardUsed;
                }
            }
        }
    }

    private void Start()
    {
        ActivateStage(0);
    }

    private void OnDestroy()
    {
        for (int s = 0; s < stages.Length; s++)
        {
            BossHazard[] stageHazards = stages[s].hazards;
            for (int h = 0; h < stageHazards.Length; h++)
            {
                if (stageHazards[h] != null)
                {
                    stageHazards[h].OnBossHit -= HandleHazardUsed;
                }
            }
        }
    }

    private void HandleHazardUsed()
    {
        if (isTransitioning || (boss != null && boss.IsDead)) return;

        spentInCurrentStage++;

        if (spentInCurrentStage >= activeHazardsInCurrentStage)
        {
            int nextStage = currentStageIndex + 1;
            if (nextStage < stages.Length)
            {
                StartCoroutine(TransitionToStageRoutine(nextStage));
            }
        }
    }

    private IEnumerator TransitionToStageRoutine(int nextStageIndex)
    {
        isTransitioning = true;

        yield return new WaitForSeconds(stageTransitionDelay);

        if (boss != null && boss.IsDead)
        {
            isTransitioning = false;
            yield break;
        }

        ActivateStage(nextStageIndex);
        isTransitioning = false;
    }

    private void ActivateStage(int stageIndex)
    {
        currentStageIndex = stageIndex;
        spentInCurrentStage = 0;
        activeHazardsInCurrentStage = 0;

        for (int s = 0; s < stages.Length; s++)
        {
            bool shouldBeActive = (s <= stageIndex);
            BossHazard[] stageHazards = stages[s].hazards;

            for (int h = 0; h < stageHazards.Length; h++)
            {
                BossHazard hazard = stageHazards[h];

                hazard.gameObject.SetActive(shouldBeActive);
                if (shouldBeActive)
                {
                    hazard.Reform();
                    activeHazardsInCurrentStage++;
                }
            }
        }
    }

    public void ResetGroup()
    {
        StopAllCoroutines();
        isTransitioning = false;
        ActivateStage(0);
    }
}