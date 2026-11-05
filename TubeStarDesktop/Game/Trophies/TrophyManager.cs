using System;
using System.Collections.Generic;

namespace TubeStar
{
    public enum Trophy
    {
        Pupil = 1358,
        PropDepartment = 1363,
        Loser = 1366,
        PoopStar = 1359,
        AptPupil = 1361,
        RantMaster = 1364,
        WellHeeld = 1367,
        Bunsen = 1365,
        HookLineAndSinker = 1362,
        InternetFamous = 1360,
        DownTheTubes = 1922,
        Procrastinator = 1924,
        InItToWinIt = 1926,
        TopModel = 1923,
        GiveAndTake = 1925,
        RebelLeader = 1950,
        RobotMasters = 1949,
        Upgrade = 7610,
        King = 7611,
        CatInBin = 7612,
        OCD = 7613,
    }

    public class TrophyManager
    {
        // Troféus desligados: mude para true para reativar
        public const bool Enabled = false;

        private static List<Trophy> _achievedTrophies;

        static TrophyManager()
        {
            _achievedTrophies = new List<Trophy>();
        }

        public static bool HasTrophy(Trophy trophy)
        {
            return _achievedTrophies.Contains(trophy);
        }

        public static void UnlockTrophy(Trophy trophy)
        {
            if (!Enabled || HasTrophy(trophy))
                return;

            _achievedTrophies.Add(trophy);
        }
    }
}
