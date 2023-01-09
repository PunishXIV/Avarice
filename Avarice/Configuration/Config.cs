using Dalamud.Configuration;

namespace Avarice.Configuration;

[Serializable]
internal class Config : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public uint ActionEffect1Opcode = 0x398;
    public List<Profile> Profiles = new();
    public Dictionary<uint, string> JobProfiles = new();
}
