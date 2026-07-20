using Monocle;
using TowerFall;

namespace Teuria.MoreReplay;

public class ReplayMenuComponent : Component
{
    private bool replaySaved;
    private bool saving;
    private readonly MenuButtonGuide confirmGuide;
    private readonly MenuButtonGuide replayGuide;
    private readonly MenuButtonGuide saveReplayGuide;

    private readonly bool shouldBefrozen;
    private readonly bool resetEffects;

    public ReplayMenuComponent(
        bool shouldBefrozen = false, bool resetEffects = true) : base(true, true)
    {
        this.shouldBefrozen = shouldBefrozen;
        this.resetEffects = resetEffects;

        confirmGuide = new MenuButtonGuide(
                0, MenuButtonGuide.ButtonModes.Confirm, "CONTINUE");

        replayGuide = new MenuButtonGuide(
                1, MenuButtonGuide.ButtonModes.Alt, "REPLAY");

        saveReplayGuide = new MenuButtonGuide(
                2, MenuButtonGuide.ButtonModes.SaveReplay, "SAVE REPLAY");
    }

    public override void Added()
    {
        base.Added();

        Alarm.Set(Entity, 180, () => {
            Entity.Add(confirmGuide);
            Entity.Add(replayGuide);
            Entity.Add(saveReplayGuide);

            if (SaveData.Instance.Options.ReplayMode == Options.ReplayModes.Off)
            {
                replayGuide.Clear();
                saveReplayGuide.Clear();
            }
        });
    }

    public override void Update()
    {
        base.Update();
        if (SaveData.Instance.Options.ReplayMode != Options.ReplayModes.Off && !saving)
        {
            if (MenuInput.Alt)
            {
                GotoReplay();
                return;
            }
            if (MenuInput.SaveReplay && !replaySaved)
            {
                SaveReplay();
                return;
            }
        }
    }

    private void GotoReplay()
    {
        var level = (Scene as Level)!;
        if (level.ReplayRecorder != null)
        {
            Entity.Visible = Entity.Active = false;
            level.ReplayViewer.Watch(level.ReplayRecorder, ReplayViewer.ReplayType.Rewind, ReplayFinish);
        }
    }

    private void SaveReplay()
    {
        var level = (Scene as Level)!;
        saving = true;
        var pauseMenu = level.Layers[4].GetFirst<PauseMenu>();
        if (pauseMenu == null)
        {
            saving = false;
            return;
        }
        pauseMenu.Visible = false;
        pauseMenu.Active = false;
        replaySaved = true;

        confirmGuide.Visible = replayGuide.Visible = saveReplayGuide.Visible = false;
        Sounds.ui_click.Play(160f, 1f);
        Scene.Add(new GifExporter(level.ReplayRecorder.Data, FinishSaveReplay));
    }

    private void FinishSaveReplay(bool success) 
    {
        if (!success) 
        {
            replaySaved = false;
        }
        else 
        {
            saveReplayGuide.Clear();
        }


        confirmGuide.Visible = replayGuide.Visible = saveReplayGuide.Visible = true;
        saving = false;

        var level = (Scene as Level)!;
        Alarm.Set(Entity, 20, () => {
            var pauseMenu = level.Layers[4].GetFirst<PauseMenu>();
            pauseMenu.Active = true;
            pauseMenu.Visible = true;
        });
    }

    private void ReplayFinish()
    {
        if (resetEffects)
        {
            ScreenEffects.Reset();
        }
        (Scene as Level)!.Frozen = shouldBefrozen;
        Entity.Visible = Entity.Active = true;
    }
}
