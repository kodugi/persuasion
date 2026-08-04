using System;

namespace GamePlay
{
    public enum CellKind
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

    public class CellUtils
    {
        public static Type CellKindToType(CellKind cellKind)
        {
            switch (cellKind)
            {
                case CellKind.Empty:
                    return typeof(EmptyCell);
                case CellKind.Black:
                    return typeof(BlackCell);
                case CellKind.WeakBlack:
                    return typeof(WeakBlackCell);
                case CellKind.Concept:
                    return typeof(ConceptCell);
                case CellKind.Lie:
                    return typeof(LieCell);
                case CellKind.Threat:
                    return typeof(ThreatCell);
                case CellKind.Disdain:
                    return typeof(DisdainCell);
                case CellKind.Religious:
                    return typeof(ReligiousCell);
                default:
                    return null;
            }
        }

        public static string CellKindToName(CellKind cellKind)
        {
            switch (cellKind)
            {
                case CellKind.Empty:
                    return "빈 생각";
                case CellKind.Black:
                    return "의심";
                case CellKind.WeakBlack:
                    return "약한 생각";
                case CellKind.Concept:
                    return "무해함";
                case CellKind.Lie:
                    return "거짓말";
                case CellKind.Threat:
                    return "협박";
                case CellKind.Disdain:
                    return "업신여김";
                case CellKind.Religious:
                    return "종교적 공포 조성";
                default:
                    return null;
            }
        }
    }
}