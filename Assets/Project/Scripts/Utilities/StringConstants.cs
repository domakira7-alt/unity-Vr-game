using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Utilities
{
    public static class StringConstants
    {
        #region TIMER_ID
        public static readonly string GAMEPLAY_TIMER_ID = "GameplayTimer";
        #endregion   

        #region TEXT_DATA
        public static readonly string TEXT_STRING_YOU_WON = "MEH\nYou have assembled the plane efficiently.";
        public static readonly string TEXT_STRING_YOU_LOST = "You didn't finish in time.\nTry again to complete the puzzle.";
        public static readonly string TEXT_STRING_EXCEEDED_WRONG_ATTEMPTS = "Attempt limit reached!\nTry again to complete the puzzle. ";
        public static readonly string TEXT_STRING_DEFAULT_LOADING = "Loading...0%";
        public static readonly string TEXT_STRING_CONTINUE = "Continue";
        public static readonly string TEXT_STRING_RETRY = "Retry";

        public static readonly string TEXT_STRING_WRONG_ATTEMPT_TITLE = "Wrong Attempt";
        public static readonly string TEXT_STRING_WRONG_ATTEMPT_DESCRIPTION = "You have placed the Puzzle piece in the wrong Slot!";
        public const string TEXT_STRING_WRONG_ATTEMPT_DESCRIPTION_PILL = "You misplaced it, the pill blurred your judgment.";

        public const string TEXT_STRING_YOU_WON_SLOW_PILL = "You felt a slight slowdown.\nThat’s how drug use often begins small changes that seem harmless.\nBut every start has a consequence.";
        public const string TEXT_STRING_YOU_WON_FAST_PILL = "Your reactions slowed, and the puzzle took longer.\nDrugs weaken focus, even when you think you're still in control.";
        public const string TEXT_STRING_YOU_WON_DRUNK_PILL = "Did you notice the confusion building?\nDrugs don’t just change your mood they disrupt how your brain processes information.";
        #endregion  

    }
}

