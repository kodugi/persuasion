using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public class UIImageView: MonoBehaviour
    {
        [SerializeField] private List<UIImageEntry> _UIImageList = new List<UIImageEntry>();
        [SerializeField] private List<ImageColorEntry> _ImageColorEntryList = new List<ImageColorEntry>();
        
        private Dictionary<GameObject, Sprite> _capturedOriginalSprites = new Dictionary<GameObject, Sprite>();
        private Dictionary<GameInfo.MapType, Color> _imageColorDict = new Dictionary<GameInfo.MapType, Color>();
        
        private void Start()
        {
            foreach (UIImageEntry entry in _UIImageList)
            {
                _capturedOriginalSprites[entry.go] = entry.go.GetComponent<Image>().sprite;
            }

            foreach (var entry in _ImageColorEntryList)
            {
                _imageColorDict[entry.mapType] = entry.color;
            }

            ResetGame();
        }

        public void ResetGame()
        {
            bool useDreamSprite = false;
            Color spriteColor = Color.white;
            
            switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
            {
                case GameInfo.MapType.Dream1:
                case GameInfo.MapType.Dream2:
                case GameInfo.MapType.Dream3:
                case GameInfo.MapType.Dream4:
                    useDreamSprite = true;
                    spriteColor = _imageColorDict.GetValueOrDefault(GameInfoHolder.GetCurrentGameInfo().GetMapType(),
                        Color.white);
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

        [Serializable]
        private struct ImageColorEntry
        {
            public GameInfo.MapType mapType;
            public Color color;
        }
    }
}