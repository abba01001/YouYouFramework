using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// This class provides 3 solutions to help navigation. 
    /// Arrow pointing down at something, arrow showing direction to something and arrows line showing direction to something.
    /// </summary>
    public class NavigationHelper : MonoBehaviour
    {
        private static NavigationHelper _instance;
        public static NavigationHelper Instance => _instance ??= new NavigationHelper();
        
        GameObject positionPointerPrefab;
        readonly DirecitonPointersController directionPointersController = new DirecitonPointersController();

        private Pool positionPointerPool;

        public async UniTask Initialise()
        {
            GameObject obj1 = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/ProjectFiles/Game/Prefabs/Navigation Helpers/Arrow Direction Pointer.prefab");
            obj1.gameObject.MSetActive(false);
            directionPointersController.arrowDirectionPointerPrefab = obj1.gameObject;
            
            GameObject obj2 = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/ProjectFiles/Game/Prefabs/Navigation Helpers/Arrow Line Direction Pointer.prefab");
            obj2.gameObject.MSetActive(false);
            directionPointersController.arrowsLineDirectionPointerPrefab = obj2.gameObject;
            
            GameObject obj3 = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/ProjectFiles/Game/Prefabs/Navigation Helpers/Position Pointer Arrow.prefab");
            obj3.gameObject.MSetActive(false);
            positionPointerPrefab = obj3.gameObject;

            directionPointersController.Initialise();

            positionPointerPool = new Pool(positionPointerPrefab, positionPointerPrefab.name);
            await UniTask.NextFrame();
        }

        private void OnDestroy()
        {
            positionPointerPool?.Destroy();

            directionPointersController.Unload();
        }

        private void LateUpdate()
        {
            directionPointersController.LateUpdate();
        }

        // returns ready to use arrow pointing down at specified position
        public PositionPointerCase CreatePositionPointer(Vector3 position)
        {
            GameObject arrowObject = positionPointerPool.GetPooledObject();
            arrowObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            arrowObject.transform.localScale = Vector3.one;

            return new PositionPointerCase(arrowObject);
        }

        // returns ready to use arrow showing direciton to specified position
        public  ArrowPointerCase CreateGuidingArrow(Vector3 position)
        {
            return DirecitonPointersController.RegisterArrowPointer(PlayerBehavior.InstanceTransform, position);
        }

        // returns ready to use arrow line showing direciton to specified position
        public  ArrowLinePointerCase CreateGuidingLine(Vector3 position)
        {
            return DirecitonPointersController.RegisterArrowLinePointer(PlayerBehavior.InstanceTransform, position);
        }

        // unloads all 3 types of provided helpers
        public void Unload()
        {
            positionPointerPool.Clear();

            directionPointersController.Unload();
        }
    }
}