using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    public class UnlockableToolsController
    {
        private static UnlockableToolsController _instance;
        public static UnlockableToolsController Instance => _instance ??= new UnlockableToolsController();
        private  readonly int FLOATING_TEXT_HASH = "UnlockableTool".GetHashCode();
        private const float FLOATING_TEXT_DELAY = 3.0f;
        private const string FLOATING_TEXT_MESSAGE = "Required!";

        Color floatingTextColor = Color.red;

        private  UnlockableTool[] registeredUnlockableTools;
        public  UnlockableTool[] RegisteredUnlockableTools => registeredUnlockableTools;

        private  float nextMessageTime;

        public async UniTask Initialise()
        {
            var data = await GameEntry.Loader.LoadMainAssetAsync<UnlockableToolsDatabase>("Assets/Game/Download/ProjectFiles/Data/Unlockable Tools Database.asset", GameEntry.Instance.gameObject);
            registeredUnlockableTools = data.UnlockableTools;
            foreach(UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                unlockableTool.Initialise();
            }
            
            await UniTask.NextFrame();
        }

        public  bool IsToolUnlocked(InteractionAnimationType toolType)
        {
            foreach (UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                if (unlockableTool.ToolType == toolType)
                    return unlockableTool.IsUnlocked;
            }

            return true;
        }

        public  UnlockableTool GetUnlockableTool(InteractionAnimationType toolType)
        {
            foreach (UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                if (unlockableTool.ToolType == toolType)
                {
                    return unlockableTool;
                }
            }

            return null;
        }

        public  void UnlockTool(InteractionAnimationType toolType)
        {
            foreach (UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                if (unlockableTool.ToolType == toolType)
                {
                    unlockableTool.Unlock();

                    return;
                }
            }
        }

        public FloatingTextBaseBehavior ShowMessage(InteractionAnimationType toolType, Vector3 position, Quaternion rotation)
        {
            if (nextMessageTime > Time.time)
                return null;

            UnlockableTool unlockableTool = GetUnlockableTool(toolType);

            if (unlockableTool == null)
                return null;

            return ShowMessage(unlockableTool, position, rotation);
        }

        public FloatingTextBaseBehavior ShowMessage(UnlockableTool unlockableTool, Vector3 position, Quaternion rotation)
        {
            if (nextMessageTime > Time.time)
                return null;

            nextMessageTime = Time.time + FLOATING_TEXT_DELAY;

            FloatingTextBaseBehavior floatingText = FloatingTextController.Instance.SpawnFloatingText(FLOATING_TEXT_HASH, FLOATING_TEXT_MESSAGE, position, rotation, 1.0f, floatingTextColor);
            
            UnlockableToolFloatingText unlockableToolFloatingText = (UnlockableToolFloatingText)floatingText;
            if(unlockableToolFloatingText != null)
            {
                unlockableToolFloatingText.Initialise(unlockableTool);
            }
            
            return floatingText;
        }
    }
}
