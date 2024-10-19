using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class PopupQueueManager
    {
        internal UIPool UIPool;

        // 用于存储弹窗及其优先级的字典
        private Dictionary<string, int> popupPriorities = new Dictionary<string, int>();
        private Queue<string> popupQueue = new Queue<string>();
        private UIFormBase currentPopup;

        public event System.Action OnQueueComplete;
        private MonoBehaviour coroutineRunner;
        private bool isCoroutineRunning;

        internal PopupQueueManager(MonoBehaviour runner)
        {
            coroutineRunner = runner;
        }

        // 添加弹窗到队列
        public void AddPopupToQueue(string popupName)
        {
            if (!popupPriorities.TryGetValue(popupName, out int priority))
            {
                priority = GetPopupPriority(popupName); // 获取弹窗优先级
                popupPriorities[popupName] = priority; // 存储优先级
            }

            popupQueue.Enqueue(popupName);

            // 开始处理队列
            if (!isCoroutineRunning)
            {
                coroutineRunner.StartCoroutine(ProcessQueueWithDelay());
            }
        }

        // 清除指定弹窗
        public void RemovePopupFromQueue(string popupName)
        {
            // 如果当前正在显示该弹窗，关闭它
            if (currentPopup != null && currentPopup.Name == popupName)
            {
                currentPopup.Close(); // 假设 UIFormBase 类有一个 Close 方法
                currentPopup = null; // 清空当前弹窗
            }

            // 从队列中移除指定弹窗
            if (popupQueue.Contains(popupName))
            {
                var tempQueue = new Queue<string>(popupQueue);
                popupQueue.Clear();

                // 重新入队，跳过指定弹窗
                while (tempQueue.Count > 0)
                {
                    var nextPopup = tempQueue.Dequeue();
                    if (nextPopup != popupName)
                    {
                        popupQueue.Enqueue(nextPopup);
                    }
                }
            }
        }

        // 等待一定时间后开始处理队列
        private IEnumerator ProcessQueueWithDelay()
        {
            yield return new WaitForSeconds(0.1f); // 等待0.1秒（或你需要的时间）

            // 根据优先级排序弹窗
            var sortedPopups = new List<string>(popupQueue);
            sortedPopups.Sort((x, y) => popupPriorities[x].CompareTo(popupPriorities[y]));

            // 清空队列以便后续处理
            popupQueue.Clear();

            // 开始显示弹窗
            foreach (var popup in sortedPopups)
            {
                ShowNextPopup(popup);
                yield return null; // 等待每个弹窗的显示
            }

            // 队列处理完毕的回调
            OnQueueComplete?.Invoke();
            isCoroutineRunning = false; // 重置标志位

            // 清理优先级字典
            popupPriorities.Clear(); 
        }

        // 显示下一个弹窗
        private void ShowNextPopup(string popupName)
        {
            currentPopup = GameEntry.UI.OpenUIForm<UIFormBase>(popupName);
            currentPopup.OnClose += OnPopupClosed;
        }

        private void OnPopupClosed()
        {
            if (currentPopup != null)
            {
                currentPopup.OnClose -= OnPopupClosed;
                currentPopup = null;

                // 检查是否有新的弹窗需要显示
                if (popupQueue.Count > 0)
                {
                    ShowNextPopup(popupQueue.Dequeue());
                }
            }
        }

        // 清空队列
        public void ClearQueue()
        {
            popupQueue.Clear();
            currentPopup = null; // Reset the currentPopup to maintain consistency
            isCoroutineRunning = false;
            popupPriorities.Clear(); // 清空优先级字典
        }

        // 示例：获取弹窗优先级
        private int GetPopupPriority(string popupName)
        {
            // 在这里根据弹窗名称返回对应的优先级
            // 这里暂时返回一个示例优先级
            return Random.Range(0, 10); // 随机优先级，实际应替换为真实逻辑
        }
    }
}
