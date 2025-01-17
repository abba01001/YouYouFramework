using System;
using System.Collections;
using System.Collections.Generic;

public class BatchObjectCreator
{
    // 分批创建
    public static IEnumerator CreateObjectsInBatches<T>(
        int totalCount,
        Func<int, T> objectFactory,
        Action<T, int> initializeObject,
        int batchSize = 20,
        Action onComplete = null)
    {
        List<T> tempObjects = new List<T>();
        int currentIndex = 0;

        while (currentIndex < totalCount)
        {
            tempObjects.Clear();
            for (int i = 0; i < batchSize && currentIndex < totalCount; i++)
            {
                var obj = objectFactory(currentIndex);
                initializeObject(obj, currentIndex);
                tempObjects.Add(obj);
                currentIndex++;
            }

            yield return null;
        }

        onComplete?.Invoke();
    }
}