namespace Teuria.ScoreCounter;

public class ScoreData
{
    public int TotalScore 
    {
        get 
        {
            int num = ScoreBonuses;
            for (int i = 0; i < 4; i += 1)
            {
                num += Scores[i];
            }

            return num;
        }
    }

    // Gameplay
    public int[] Scores;
    public int ScoreBonuses;

    // UI
    public string[] ScoreText;
    public float[] ScoreScale;
    public int[] ScoreLast;

    public ScoreData()
    {
        Scores = new int[4];
        ScoreText = new string[4];
        ScoreScale = new float[4];
        ScoreLast = new int[4];
        ScoreBonuses = 0;
    }
}
