using System;
using System.Collections.Generic;

namespace TubeStar
{
    public static class Settings
    {
        public static VideoCategory? LastCategory { get; set; }
        public static Channel LastChannel { get; set; }
        public static string PlayerName { get; set; }

        public static bool ListView { get; set; }    }
}