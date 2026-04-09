#pragma warning disable CS0414

using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


namespace Watermelon
{
    public class MissionsController : MonoBehaviour
    {
        private static MissionsController _instance;
        public static MissionsController Instance => _instance ??= new MissionsController();

        public bool AutoCompleteMissions { get; } = true;

        private Mission[] missions;

        private Mission activeMission;
        public Mission ActiveMission => activeMission;

        private TweenCase completeTweenCase;

        public event SimpleCallback OnNextMissionStarted;
        public event SimpleCallback OnMissionFinished;

        private bool isInitialised => missions != null;
        private bool fistTimeLoadingMission = false;

        public async UniTask Initialise(Mission[] missions)
        {
            if (missions == null)
                return;

            // if missions are disabled using Actions menu - works only in the editor
            if (MissionsActionMenu.AreMissionsDisabled())
                return;

            this.missions = missions;
            
            FormGame.Instance.MissionUIPanel.Initialise();


            for (int i = 0; i < missions.Length; i++)
            {
                missions[i].Initialise();
            }
        }

        private void Activate(int index)
        {
            if (activeMission != null)
                activeMission.Deactivate();

            activeMission = missions[index];
            activeMission.Activate();

            FormGame.Instance.MissionUIPanel.ActivateMission(activeMission);

#if UNITY_EDITOR
            if (fistTimeLoadingMission)
            {
                WorldController.Instance.UpdateWorldSave(activeMission.gameObject.name);
                SavePresets.CreateSave(WorldController.Instance.CurrentWorld.Scene.Name + " " + activeMission.gameObject.name, "Missions", activeMission.ID);
            }
#endif
        }

        public bool DoesNextMissionExist()
        {
            int activationMissionIndex = 0;

            for (int i = 0; i < missions.Length; i++)
            {
                if (missions[i].MissionStage == Mission.Stage.Collected)
                    activationMissionIndex = i + 1;
            }

            if (missions.IsInRange(activationMissionIndex))
            {
                if (activeMission != missions[activationMissionIndex])
                {
                    return true;
                }
            }

            return false;
        }

        public void CompleteMission()
        {
            completeTweenCase.KillActive();

            if (activeMission != null)
            {
                if (activeMission.MissionStage == Mission.Stage.Finished)
                {
                    activeMission.ApplyReward(1.0f);

                    FormGame.Instance.MissionUIPanel.OnRewardClaimed();

                    fistTimeLoadingMission = true;

                    Tween.DelayedCall(3f, () =>
                    {
                        ActivateNextMission();
                    });
                }
            }
        }

        public void AutoCompleteMission(float duration)
        {
            completeTweenCase.KillActive();

            // autocomplete is available only for mission with resources
            if (activeMission != null && activeMission.RewardType == MissionRewardType.Resources)
            {
                completeTweenCase = Tween.DelayedCall(duration, () =>
                {
                    CompleteMission();
                });
            }
        }

        public int GetLastCompletedMissionIndex()
        {
            int activationMissionIndex = 0;

            for (int i = 0; i < missions.Length; i++)
            {
                if (missions[i].MissionStage == Mission.Stage.Collected)
                    activationMissionIndex = i + 1;
            }

            return activationMissionIndex;
        }

        public Mission GetMissionById(string id)
        {
            if (missions == null)
                return null;

            return missions.Find(m => m.ID.Equals(id));
        }

        public void ActivateNextMission()
        {
            if (!isInitialised)
                return;

            int activationMissionIndex = 0;

            for (int i = 0; i < missions.Length; i++)
            {
                if (missions[i].MissionStage == Mission.Stage.Collected)
                    activationMissionIndex = i + 1;
            }

            if (missions.IsInRange(activationMissionIndex))
            {
                if (activeMission != missions[activationMissionIndex])
                {
                    if (activationMissionIndex == 0)
                    {
                        fistTimeLoadingMission = true;
                    }

                    Activate(activationMissionIndex);

                    OnNextMissionStarted?.Invoke();
                }
            }
            // if last mission is completed
            else
            {
                FormGame.Instance.MissionUIPanel.gameObject.SetActive(false);
            }
        }

        public void MissionFinished()
        {
            OnMissionFinished?.Invoke();

            Tween.DelayedCall(0.5f, async () =>
            {
                if (activeMission.RewardType == MissionRewardType.Tool)
                {
                    FormUnlockBuilding form = await GameEntry.UI.OpenUIForm<FormUnlockBuilding>();
                    form.Show(activeMission.ToolsReward.RewardInfo);
                }
                else if (activeMission.RewardType == MissionRewardType.Generic)
                {
                    FormUnlockBuilding form = await GameEntry.UI.OpenUIForm<FormUnlockBuilding>();
                    form.Show(activeMission.GenericReward.RewardInfo);
                }
            });
        }

        public void Unload()
        {
            if (!isInitialised) return;

            FormGame.Instance.MissionUIPanel.Unload();

            activeMission?.Deactivate();

            if(!missions.IsNullOrEmpty())
            {
                for (int i = 0; i < missions.Length; i++)
                {
                    missions[i].Unload();
                }
                missions = null;
            }
        }
    }

    [System.Serializable]
    public abstract class MissionSave : ISaveObject
    {
        [SerializeField] Mission.Stage missionStage;
        public Mission.Stage MissionStage => missionStage;

        [SerializeField] string missionID;
        public string MissionID => missionID;

        [System.NonSerialized]
        private Mission mission;

        public virtual void Flush()
        {
            missionStage = mission.MissionStage;
        }

        public void LinkMission(Mission mission)
        {
            this.mission = mission;
            missionID = mission.ID;
        }
    }
}
