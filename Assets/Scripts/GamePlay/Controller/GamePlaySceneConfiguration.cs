using System;
using System.Collections.Generic;
using MapEditor.Model;
using UnityEngine;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public enum PlayableBlockType
    {
        Basic,
        Lie,
        Threat,
        Religious
    }

    [Serializable]
    internal sealed class TutorialEntryGroup
    {
        public TutorialState State;
        public List<TutorialEntry> Entries = new List<TutorialEntry>();
    }

    [Serializable]
    internal sealed class StageEntry
    {
        public string MapName;
        public List<GameInfo> GameInfoList = new List<GameInfo>();
    }

    /// <summary>
    /// Converts inspector data into the runtime configuration used by GamePlayScene.
    /// </summary>
    internal sealed class GamePlaySceneConfiguration
    {
        private readonly List<GameInfo> _fallbackGameInfos;
        private readonly List<TutorialEntryGroup> _tutorialGroups;
        private readonly Dictionary<string, List<GameInfo>> _gameInfosByStage;

        public GamePlaySceneConfiguration(
            List<GameInfo> fallbackGameInfos,
            List<TutorialEntryGroup> tutorialGroups,
            List<StageEntry> stages)
        {
            _fallbackGameInfos = fallbackGameInfos;
            _tutorialGroups = tutorialGroups;
            _gameInfosByStage = BuildStageLookup(stages);
        }

        public bool SelectGameInfoForCurrentScene()
        {
            ChiefManager chiefManager = ChiefManager.Instance;
            string stageId = chiefManager == null ? null : chiefManager.per_Scene_ID;

            if (chiefManager != null)
            {
                if (TrySelectStage(stageId))
                {
                    return true;
                }

                Debug.LogWarning("No gameplay stage is configured for scene id: " + stageId);
                return TrySelectFallbackOrExisting();
            }

            // Map editor play-tests should take precedence when GamePlayScene is opened directly.
            GameInfo editorGameInfo = EditorInfoHolder.GetGameInfo();
            if (editorGameInfo != null)
            {
                GameInfoHolder.SetGameInfo(editorGameInfo);
                return true;
            }

            return TrySelectFallbackOrExisting();
        }

        public Dictionary<TutorialState, List<TutorialEntry>> CreateTutorialLookup()
        {
            if (_tutorialGroups == null || _tutorialGroups.Count == 0)
            {
                return CreateFallbackTutorialLookup();
            }

            Dictionary<TutorialState, List<TutorialEntry>> result =
                new Dictionary<TutorialState, List<TutorialEntry>>();

            foreach (TutorialEntryGroup group in _tutorialGroups)
            {
                if (group == null)
                {
                    continue;
                }

                if (!result.TryGetValue(group.State, out List<TutorialEntry> entries))
                {
                    entries = new List<TutorialEntry>();
                    result.Add(group.State, entries);
                }

                if (group.Entries == null)
                {
                    continue;
                }

                foreach (TutorialEntry entry in group.Entries)
                {
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }

            if (!result.ContainsKey(TutorialState.None))
            {
                result.Add(TutorialState.None, new List<TutorialEntry>());
            }

            return result;
        }

        public static List<IBlock> CreateBlocks(List<PlayableBlockType> blockTypes)
        {
            List<IBlock> blocks = new List<IBlock>();
            if (blockTypes != null)
            {
                foreach (PlayableBlockType blockType in blockTypes)
                {
                    IBlock block = CreateBlock(blockType);
                    if (block != null)
                    {
                        blocks.Add(block);
                    }
                }
            }

            if (blocks.Count == 0)
            {
                blocks.Add(new BasicBlock());
            }

            return blocks;
        }

        private bool TrySelectStage(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId) ||
                !_gameInfosByStage.TryGetValue(stageId, out List<GameInfo> gameInfos) ||
                !HasGameInfo(gameInfos))
            {
                return false;
            }

            GameInfoHolder.SetGameInfoList(gameInfos);
            return true;
        }

        private bool TrySelectFallbackOrExisting()
        {
            if (HasGameInfo(_fallbackGameInfos))
            {
                Debug.LogWarning("Using the fallback GameInfo list configured on GameManager.");
                GameInfoHolder.SetGameInfoList(_fallbackGameInfos);
                return true;
            }

            if (GameInfoHolder.TryGetCurrentGameInfo(out _))
            {
                return true;
            }

            Debug.LogError("GamePlayScene cannot start because no GameInfo is available.");
            return false;
        }

        private static Dictionary<string, List<GameInfo>> BuildStageLookup(List<StageEntry> stages)
        {
            Dictionary<string, List<GameInfo>> result =
                new Dictionary<string, List<GameInfo>>(StringComparer.Ordinal);

            if (stages == null)
            {
                return result;
            }

            foreach (StageEntry stage in stages)
            {
                if (stage == null || string.IsNullOrWhiteSpace(stage.MapName))
                {
                    Debug.LogWarning("Ignoring a gameplay stage with an empty map name.");
                    continue;
                }

                if (result.ContainsKey(stage.MapName))
                {
                    Debug.LogWarning("Duplicate gameplay stage configuration: " + stage.MapName);
                }

                result[stage.MapName] = stage.GameInfoList;
            }

            return result;
        }

        private static bool HasGameInfo(List<GameInfo> gameInfos)
        {
            if (gameInfos == null || gameInfos.Count == 0)
            {
                return false;
            }

            foreach (GameInfo gameInfo in gameInfos)
            {
                if (gameInfo != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static IBlock CreateBlock(PlayableBlockType blockType)
        {
            switch (blockType)
            {
                case PlayableBlockType.Basic:
                    return new BasicBlock();
                case PlayableBlockType.Lie:
                    return new LieBlock();
                case PlayableBlockType.Threat:
                    return new ThreatBlock();
                case PlayableBlockType.Religious:
                    return new ReligiousBlock();
                default:
                    Debug.LogError("Unsupported gameplay block type: " + blockType);
                    return null;
            }
        }

        private static Dictionary<TutorialState, List<TutorialEntry>> CreateFallbackTutorialLookup()
        {
            return new Dictionary<TutorialState, List<TutorialEntry>>
            {
                {
                    TutorialState.PlaceFirstCell,
                    new List<TutorialEntry>
                    {
                        TutorialEntry.CreateCellEntry(new Vector2Int(2, 3), typeof(ConceptCell))
                    }
                },
                {
                    TutorialState.PlaceSecondCell,
                    new List<TutorialEntry>
                    {
                        TutorialEntry.CreateCellEntry(new Vector2Int(4, 3), typeof(ConceptCell))
                    }
                },
                {
                    TutorialState.ExplainEndTurn,
                    new List<TutorialEntry>
                    {
                        TutorialEntry.CreateGameObjectEntry("Canvas/RightPanel/ButtonsPanel/EndTurnButton")
                    }
                },
                { TutorialState.None, new List<TutorialEntry>() }
            };
        }
    }
}
