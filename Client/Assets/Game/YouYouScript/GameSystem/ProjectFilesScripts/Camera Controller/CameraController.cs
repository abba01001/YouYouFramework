using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    [DefaultExecutionOrder(100)]
    public sealed class CameraController : ISceneSavingCallback
    {
        private static CameraController _instance;
        public static CameraController Instance => _instance ??= new CameraController();

        CameraType firstCamera = CameraType.Gameplay;

        VirtualCamera[] virtualCameras;
        List<CameraBlendSettings> blendSettings = new List<CameraBlendSettings>();

        CameraBlendData defaultBlendData;

        private Dictionary<CameraType, int> virtualCamerasLink = new Dictionary<CameraType, int>();
        private VirtualCamera activeCamera;
        public VirtualCamera ActiveVirtualCamera => activeCamera;

        private bool isBlending;
        public bool IsBlending => isBlending;

        private CameraBlendCase currentBlendCase;

        public async UniTask Initialise()
        {
            // Initialise cameras link
            virtualCameras = GameEntry.Instance.MainCamera.GetComponentsInChildren<VirtualCamera>(true);
            for (int i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameras[i].Init();

                virtualCamerasLink.Add(virtualCameras[i].CameraType, i);
            }

            VirtualCamera firstVirtualCamera = GetCamera(firstCamera);
            firstVirtualCamera.Activate();

            activeCamera = firstVirtualCamera;

            UpdateCamera();
            await PreviewCamera.Initialise();
            await UniTask.NextFrame();
            Init = true;
        }

        private void UpdateCamera()
        {
            UpdateCamera(activeCamera.CameraData);
        }

        private void UpdateCamera(CameraLocalData cameraData)
        {
            if (activeCamera.Target == null)
                return;

            GameEntry.Instance.MainCamera.fieldOfView = cameraData.FieldOfView;
            GameEntry.Instance.MainCamera.nearClipPlane = cameraData.NearClipPlane;
            GameEntry.Instance.MainCamera.farClipPlane = cameraData.FarClipPlane;

            GameEntry.Instance.MainCamera.transform.SetPositionAndRotation(cameraData.Position, cameraData.Rotation);
        }

        private bool Init = false;

        public void LateUpdate()
        {
            if (!Init) return;
            if (activeCamera == null)
                return;

            // Update camera position
            if (isBlending)
            {
                UpdateCamera(currentBlendCase.CameraData);

                return;
            }

            UpdateCamera();
        }

        public VirtualCamera GetCamera(CameraType cameraType)
        {
            return virtualCameras[virtualCamerasLink[cameraType]];
        }

        private CameraBlendData GetBlendData(CameraType firstCameraType, CameraType secondCameraType)
        {
            for (int i = 0; i < blendSettings.Count; i++)
            {
                if (blendSettings[i].FirstCameraType == firstCameraType &&
                    blendSettings[i].SecondCameraType == secondCameraType)
                {
                    return blendSettings[i].BlendData;
                }
            }

            return defaultBlendData;
        }

        public void EnableCamera(CameraType cameraTypeToEnable)
        {
            // if required camera is already active
            if (activeCamera != null && activeCamera.CameraType == cameraTypeToEnable)
                return;

            // if required camera doesn't exist
            VirtualCamera newCamera = GetCamera(cameraTypeToEnable);
            if (newCamera == null)
            {
                Debug.LogError($"Camera of type {cameraTypeToEnable} not found.");

                return;
            }

            // if there was no camera - instantly activate required
            if (activeCamera == null)
            {
                activeCamera = newCamera;
                activeCamera.Activate();

                UpdateCamera();

                return;
            }

            CameraType currentCameraType = activeCamera.CameraType;

            // Get blend data
            CameraBlendData blendData = GetBlendData(currentCameraType, cameraTypeToEnable);

            // if blend time is zero - disable current camera and activate required
            if (blendData.BlendTime <= 0)
            {
                activeCamera.Disable();

                activeCamera = newCamera;
                activeCamera.Activate();

                UpdateCamera();

                return;
            }

            isBlending = true;

            newCamera.Activate();

            // running blend
            currentBlendCase = new CameraBlendCase(activeCamera, newCamera, blendData, () =>
            {
                activeCamera.Disable();
                activeCamera = newCamera;
                activeCamera.Activate();

                isBlending = false;
            });
        }

        public void OverrideBlend(CameraType firstCameraType, CameraType secondCameraType, float newTime,
            Ease.Type easing)
        {
            if (blendSettings.FindIndex(s =>
                    s.FirstCameraType == firstCameraType && s.SecondCameraType == secondCameraType) != -1)
            {
                CameraBlendData blendData = GetBlendData(firstCameraType, secondCameraType);
                blendData.OverrideBlendTime(newTime);
            }
            else
            {
                CameraBlendSettings newBlend = new CameraBlendSettings(firstCameraType, secondCameraType,
                    new CameraBlendData(newTime, easing));
                blendSettings.Add(newBlend);
            }
        }

        public void OnSceneSaving()
        {
            VirtualCamera[] cachedVirtualCameras =
                GameEntry.Instance.MainCamera.transform.GetComponentsInChildren<VirtualCamera>(true);
            if (!cachedVirtualCameras.SafeSequenceEqual(virtualCameras))
            {
                virtualCameras = cachedVirtualCameras;

                RuntimeEditorUtils.SetDirty(GameEntry.Instance.MainCamera.transform);
            }
        }
    }

    public class CameraBlendCase
    {
        public readonly VirtualCamera FirstCamera;
        public readonly VirtualCamera SecondCamera;

        private CameraBlendData cameraBlendData;

        private Ease.IEasingFunction easingFunction;

        private TweenCase tweenCase;

        private CameraLocalData cameraData;
        public CameraLocalData CameraData => cameraData;

        public CameraBlendCase(VirtualCamera firstCamera, VirtualCamera secondCamera, CameraBlendData cameraBlendData,
            SimpleCallback completeCallback)
        {
            this.cameraBlendData = cameraBlendData;

            FirstCamera = firstCamera;
            SecondCamera = secondCamera;

            firstCamera.StartTransition();
            secondCamera.StartTransition();

            cameraData = new CameraLocalData(firstCamera.CameraData);
            easingFunction = Ease.GetFunction(cameraBlendData.BlendEaseType);

            tweenCase = Tween
                .DoFloat(0f, 1.0f, cameraBlendData.BlendTime,
                    (value) => { cameraData.Lerp(firstCamera.CameraData, secondCamera.CameraData, value); })
                .SetCustomEasing(easingFunction).OnComplete(() =>
                {
                    FirstCamera.StopTransition();
                    SecondCamera.StopTransition();

                    completeCallback?.Invoke();
                });
        }

        public void Clear()
        {
            tweenCase.KillActive();
        }
    }
}