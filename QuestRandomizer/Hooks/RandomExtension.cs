using System;

namespace Teuria.QuestRandomizer;

internal static class RandomExtension 
{
    // Monocle's Random range is pretty bad
    public static int InclusiveRange(this Random random, int min, int max)
    {
        return random.Next(min, max + 1);
    }
}

