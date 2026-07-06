using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

internal sealed class UIFollowDefaultKeyboardConfigAlert : Entity
{
    private OptionsButton sourceButton;
    private string[] alerts;

    public UIFollowDefaultKeyboardConfigAlert(OptionsButton sourceButton)
    {
        Depth = -10000;
        this.sourceButton = sourceButton;
        this.sourceButton.Selected = false;
        alerts = [
            "CANNOT MODIFY KEYBOARD CONFIG WHILE",
            "FOLLOW DEFAULT KEYBOARD CONFIG IS ENABLED"
        ];
        Add(new Coroutine(Sequence()));
    }

    public override void Render()
    {
        MenuPanel.DrawPanel(X - 100f, Y - 40f, 200f, 40 + (alerts.Length * 20));
        float offset = -6f;

        for (int i = 0; i < alerts.Length; i += 1)
        {
            Draw.TextCentered(TFGame.Font, alerts[i], Position + new Vector2(0f, offset), Color.White);
            offset += 12;
        }
    }

    private IEnumerator Sequence()
    {
        var wassup = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        wassup.OnUpdate = (t) =>
        {
            Position = Vector2.Lerp(new Vector2(-160f, 120f), new Vector2(160f, 120f), t.Eased);
        };
        Add(wassup);

        yield return wassup.Wait();
        yield return 10;

        while (!MenuInput.ConfirmOrStart)
        {
            yield return 0;
        }
        var outtaHere = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        outtaHere.OnUpdate = (t) =>
        {
            Position = Vector2.Lerp(new Vector2(160f, 120f), new Vector2(480f, 120f), t.Eased);
        };

        Add(outtaHere);
        yield return outtaHere.Wait();

        RemoveSelf();
        sourceButton.Selected = true;
    }
}