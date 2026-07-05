using System.Collections.Generic;
using FortRise;

namespace Teuria.ScoreCounter;

public sealed class ScoreCounterSaveData : ModuleSaveData
{
    public Dictionary<string, ScoreQuestTower> QuestTowers { get; set; } = [];


    public sealed class ScoreQuestTower
    {
        public int SoloScore { get; set; }
        public int TeamScore { get; set; }
    }
}
