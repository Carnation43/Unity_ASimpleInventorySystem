using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteScrollTest
{
    public class TestSlotUI : MonoBehaviour, ISlotUI<TestItemData>
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TMP_Text _idText;

        public TestItemData _data;

        public TestItemData IData => _data;

        public void Initialize(TestItemData data)
        {
            _data = data;

            if (data != null)
            {
                gameObject.SetActive(true);

                if (_backgroundImage != null)
                {
                    _backgroundImage.color = data.PixelColor;
                }

                if (_idText != null)
                {
                    _idText.text = $"#{data.ID}\n{data.Name}";
                }
            }
            else
            {
                SetEmpty();
            }
        }

        public void SetEmpty()
        {
            gameObject.SetActive(false);
            _data = null;
        }

        public void Select()
        {
            if (_backgroundImage != null)
            {
                Color c = _backgroundImage.color;
                c.a = 1.0f;
                _backgroundImage.transform.localScale = Vector3.one * 1.1f;
            }
        }

        public void Deselect()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.transform.localScale = Vector3.one;
            }
        }
    }
}
