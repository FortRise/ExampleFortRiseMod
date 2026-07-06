using System.Collections.Generic;
using System.Text.Json.Serialization;
using TowerFall;

namespace Teuria.Profiles;


public class PlayerProfile
{
    public required string Name { get; set; }
    public List<ProfileArcher> SelectedArchers { get; set; } = [];

    public GamepadConfig GamepadConfig { get; set; } = GamepadConfig.GetDefault();
    public KeyboardConfig KeyboardConfig { get; set; } = KeyboardConfig.GetDefault();

    public bool FollowsDefaultKeyboardConfig { get; set; } = true;

    [JsonIgnore]
    public int FirstArcherIndex => SelectedArchers[0].CharacterIndex;
}