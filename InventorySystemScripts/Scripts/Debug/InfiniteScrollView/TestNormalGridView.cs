using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InfiniteScrollTest
{
    public class TestNormalGridView : MonoBehaviour
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private TestSlotUI _slotPrefab;

        private List<GameObject> _spawnedObj = new List<GameObject>();

        public void Initialize(List<TestItemData> dataList)
        {
            foreach (var obj in _spawnedObj)
            {
                Destroy(obj);
            }
            _spawnedObj.Clear();

            foreach (var data in dataList)
            {
                TestSlotUI newSlot = Instantiate(_slotPrefab, _content);
                newSlot.Initialize(data);
                newSlot.gameObject.SetActive(true);

                _spawnedObj.Add(newSlot.gameObject);
            }
        }
    }
}