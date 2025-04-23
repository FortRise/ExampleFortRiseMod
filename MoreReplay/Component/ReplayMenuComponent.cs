using Monocle;
using TowerFall;

namespace Teuria.MoreReplay;

public class ReplayMenuComponent : Component
{
    private bool replaySaved;
    private bool saving;

    public ReplayMenuComponent() : base(true, true)
    {
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
        Sounds.ui_click.Play(160f, 1f);
        Scene.Add<GifExporter>(new GifExporter(level.ReplayRecorder.Data, FinishSaveReplay));
    }

    private void FinishSaveReplay(bool success) 
    {
        if (!success) 
        {
            replaySaved = false;
        }
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
        ScreenEffects.Reset();
        (Scene as Level)!.Frozen = false;
        Entity.Visible = (Entity.Active = true);
    }
}