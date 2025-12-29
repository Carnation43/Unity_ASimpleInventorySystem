using System.Collections.Generic;
using UnityEngine;

namespace InfiniteScrollTest
{
    public class TestNormalController : MonoBehaviour
    {
        [SerializeField] private TestNormalGridView _view;
        [SerializeField] private int _dataCount = 5000;

        private void Start()
        {
            List<TestItemData> mockData = new List<TestItemData>();
            for (int i = 0; i < _dataCount; i++)
            {
                mockData.Add(new TestItemData
                {
                    ID = i,
                    Name = $"NormalItem_{i}",
                    PixelColor = Color.white
                });
            }

            float startTime = Time.realtimeSinceStartup;

            _view.Initialize(mockData);

            Debug.Log($"[Normal] Spawned {_dataCount} items consuming : {(Time.realtimeSinceStartup - startTime) * 1000} ms");
        }
    }
}