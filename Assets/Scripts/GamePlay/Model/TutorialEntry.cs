using System;
using UnityEngine;

namespace GamePlay
{
    [CreateAssetMenu(fileName = "TutorialEntry", menuName = "GamePlay/Tutorial Entry")]
    public class TutorialEntry : ScriptableObject
    {
        [SerializeField] private TutorialEntryKind _entryKind = TutorialEntryKind.Cell;
        [SerializeField, Min(0)] private int _cellX;
        [SerializeField, Min(0)] private int _cellY;
        [SerializeField] private TutorialCellType _cellType = TutorialCellType.Concept;
        [Tooltip("Scene object path used by GameObject.Find, for example Canvas/RightPanel/ButtonsPanel/EndTurnButton.")]
        [SerializeField] private string _gameObjectPath;

        public Vector2Int CellCoord
        {
            get
            {
                return _entryKind == TutorialEntryKind.Cell ? new Vector2Int(_cellX, _cellY) : null;
            }
        }

        public Type CellType
        {
            get
            {
                return _entryKind == TutorialEntryKind.Cell ? GetCellType(_cellType) : null;
            }
        }

        public GameObject GameObjectToMark
        {
            get
            {
                if (_entryKind != TutorialEntryKind.GameObject)
                {
                    return null;
                }

                return string.IsNullOrEmpty(_gameObjectPath) ? null : GameObject.Find(_gameObjectPath);
            }
        }

        public static TutorialEntry CreateCellEntry(Vector2Int cellCoord, Type cellType)
        {
            TutorialEntry entry = CreateInstance<TutorialEntry>();
            entry._entryKind = TutorialEntryKind.Cell;
            entry._cellX = cellCoord == null ? 0 : cellCoord.X;
            entry._cellY = cellCoord == null ? 0 : cellCoord.Y;
            entry._cellType = GetTutorialCellType(cellType);
            return entry;
        }

        public static TutorialEntry CreateGameObjectEntry(GameObject gameObjectToMark)
        {
            TutorialEntry entry = CreateInstance<TutorialEntry>();
            entry._entryKind = TutorialEntryKind.GameObject;
            entry._gameObjectPath = GetGameObjectPath(gameObjectToMark);
            return entry;
        }

        public static TutorialEntry CreateGameObjectEntry(string gameObjectPath)
        {
            TutorialEntry entry = CreateInstance<TutorialEntry>();
            entry._entryKind = TutorialEntryKind.GameObject;
            entry._gameObjectPath = gameObjectPath;
            return entry;
        }

        private static Type GetCellType(TutorialCellType cellType)
        {
            switch (cellType)
            {
                case TutorialCellType.Black:
                    return typeof(BlackCell);
                case TutorialCellType.WeakBlack:
                    return typeof(WeakBlackCell);
                case TutorialCellType.Concept:
                    return typeof(ConceptCell);
                case TutorialCellType.Lie:
                    return typeof(LieCell);
                case TutorialCellType.Threat:
                    return typeof(ThreatCell);
                case TutorialCellType.Disdain:
                    return typeof(DisdainCell);
                case TutorialCellType.Religious:
                    return typeof(ReligiousCell);
                case TutorialCellType.Empty:
                default:
                    return typeof(EmptyCell);
            }
        }

        private static TutorialCellType GetTutorialCellType(Type cellType)
        {
            if (cellType == typeof(BlackCell))
            {
                return TutorialCellType.Black;
            }

            if (cellType == typeof(WeakBlackCell))
            {
                return TutorialCellType.WeakBlack;
            }

            if (cellType == typeof(LieCell))
            {
                return TutorialCellType.Lie;
            }

            if (cellType == typeof(ThreatCell))
            {
                return TutorialCellType.Threat;
            }

            if (cellType == typeof(DisdainCell))
            {
                return TutorialCellType.Disdain;
            }

            if (cellType == typeof(ReligiousCell))
            {
                return TutorialCellType.Religious;
            }

            if (cellType == typeof(ConceptCell))
            {
                return TutorialCellType.Concept;
            }

            return TutorialCellType.Empty;
        }

        private static string GetGameObjectPath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            string path = gameObject.name;
            Transform current = gameObject.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        public enum TutorialEntryKind
        {
            Cell,
            GameObject
        }

        public enum TutorialCellType
        {
            Empty,
            Black,
            WeakBlack,
            Concept,
            Lie,
            Threat,
            Disdain,
            Religious
        }
    }
}
