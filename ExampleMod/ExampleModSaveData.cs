using FortRise;
using TeuJson;

namespace ExampleMod;

public class ExampleModSaveData : ModuleSaveData
{
    private int gems;

    public ExampleModSaveData() : base(new JsonSaveDataFormat())
    {
    }

    public override void Load(SaveDataFormat formatter)
    {
        var jsonObj = formatter.CastTo<JsonSaveDataFormat>().GetJsonObject();
        gems = jsonObj.GetJsonValueOrNull("gems") ?? 0;
    }

    public override ClosedFormat Save(FortModule fort)
    {
        var jsonObject = new JsonObject {
            ["gems"] = gems
        };
        return Formatter.Close(jsonObject);
    }
}