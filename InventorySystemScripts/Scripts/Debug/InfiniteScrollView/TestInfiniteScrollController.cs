using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InfiniteScrollTest
{
    public class TestInfiniteScrollController : MonoBehaviour
    {
        [SerializeField] private TestInfiniteGridView _view;
        [SerializeField] private int _dataCount = 1000;

        private void Start()
        {
            GenerateAndShowData();
        }

        private void GenerateAndShowData()
        {
            List<TestItemData> mockData = new List<TestItemData>();

            for (int i = 0; i < _dataCount; i++)
            {
                float hue = (float)i / 100f;

                Color color = Color.HSVToRGB(hue % 1f, 0.8f, 0.8f);

                mockData.Add(new TestItemData
                {
                    ID = i,
                    Name = $"InfiniteItem_{i}",
                    PixelColor = color
                });
            }
            float startTime = Time.realtimeSinceStartup;

            _view.Initialize(mockData);

            Debug.Log($"Spawned {mockData.Count} slots");

            Debug.Log($"[Infinite] Spawned {_dataCount} items consuming : {(Time.realtimeSinceStartup - startTime) * 1000} ms");
        }
    }
}