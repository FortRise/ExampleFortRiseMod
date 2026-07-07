using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

public class BarChart : Component
{
    public Vector2 Position;
    public int OffsetY;
    private int maxWidth;
    private float maxPercentage;
    private ChartData[] datas;
    private ProfileSettings settings;

    public BarChart(Vector2 position, int maxWidth, params ChartData[] data) 
        : base(false, true)
    {
        Position = position;
        this.maxWidth = maxWidth;
        datas = data;
        settings = ProfilesModule.Instance.GetSettings<ProfileSettings>()!;

        for (int i = 0; i < data.Length; i += 1)
        {
            maxPercentage = Math.Max(data[i].Value, maxPercentage);
        }

        if (settings.ProfileStatsChartType == "Incremental")
        {
            maxPercentage = (int)Math.Ceiling(maxPercentage / 5.0f) * 5;
        }

        OffsetY = (data.Length * 25) + 35;
    }

    public override void Render()
    {
        base.Render();
        Vector2 pos = Position;
        if (Entity is not null)
        {
            pos += Entity.Position;
        }

        int totalChartHeight = 70;

        int totalItems = datas.Length;

        float heightPerSlot = totalChartHeight / (totalItems + (totalItems - 1) * 0.2f);

        int barHeight = (int)heightPerSlot;
        int gap = (int)(barHeight * 0.2f);

        int offsetY = 0;

        for (int i = 0; i < datas.Length; i += 1)
        {
            var (color, name, value) = datas[i];

            var currentY = pos.Y + (i * (barHeight + gap));

            var labelY = currentY + barHeight / 2;

            Draw.OutlineTextJustify(TFGame.Font, name, new Vector2(pos.X + 40, labelY), color, Color.Black, new Vector2(1, 0f));
            // Draw.OutlineTextCentered(TFGame.Font, name, new Vector2(pos.X + 40, currentY + 5), color, Color.Black);

            int currentBarWidth = (int)(value / maxPercentage * maxWidth);
            Draw.HollowRect(new Rectangle((int)pos.X + 50 - 1, (int)currentY - 1, currentBarWidth + 2, barHeight + 2), Color.Black);
            Draw.Rect(new Rectangle((int)pos.X + 50, (int)currentY, currentBarWidth, barHeight), color);
            // offsetY += 20;
        }

        if (settings.ProfileStatsChartType == "Relative")
        {
            return;
        }

        offsetY += totalChartHeight;

        int totalLabels = 3;

        for (int i = 0; i <= totalLabels; i += 1)
        {
            float pct = (float)i / totalLabels;

            int labelX = (int)pos.X + 50 + (int)(pct * maxWidth);

            int axisValue = (int)(pct * maxPercentage);
            string text = axisValue.ToString();

            Vector2 textPosition = new Vector2(labelX, pos.Y + offsetY);

            Draw.OutlineTextCentered(TFGame.Font, text, textPosition, Color.White, Color.Black);
        }
    }

    public record struct ChartData(Color Color, string Name, int Value);
    
}