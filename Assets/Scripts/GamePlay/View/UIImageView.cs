using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public class UIImageView: MonoBehaviour
    {
        [SerializeField]
        private List<UIImageEntry> _UIImageList = new List<UIImageEntry>();
        
        private Dictionary<GameObject, Sprite> _capturedOriginalSprites = new Dictionary<GameObject, Sprite>();
        
        private void Start()
        {
            foreach (UIImageEntry entry in _UIImageList)
            {
                _capturedOriginalSprites[entry.go] = entry.go.GetComponent<Image>().sprite;
            }

            bool useDreamSprite = false;
            Color spriteColor = Color.white;
            
            switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
            {
                case GameInfo.MapType.Dream1:
                    useDreamSprite = true;
                    break;
                case GameInfo.MapType.Dream2:
                    useDreamSprite = true;
                    spriteColor *= 0.8f;
                    break;
                case GameInfo.MapType.Dream3:
                    useDreamSprite = true;
                    spriteColor.r += 0.4f;
                    spriteColor *= 0.6f;
                    break;
                case GameInfo.MapType.Dream4:
                    useDreamSprite = true;
                    spriteColor.r += 0.8f;
                    spriteColor *= 0.4f;
                    break;
            }
            
            foreach (UIImageEntry entry in _UIImageList)
            {
                Image image = entry.go.GetComponent<Image>();
                image.sprite = useDreamSprite ? entry.dreamSprite : _capturedOriginalSprites[entry.go];
                image.color = spriteColor;
            }
        }

        [Serializable]
        private struct UIImageEntry
        {
            public GameObject go;
            public Sprite dreamSprite;
        }
    }
}