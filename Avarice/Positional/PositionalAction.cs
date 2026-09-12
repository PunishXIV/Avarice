namespace Avarice.Positional;
public class PositionalAction
{
    public int Id { get; set; }
    public string ActionName { get; set; }
    public string ActionPosition { get; set; }

    public Dictionary<int, PositionalParameters> Positionals { get; set; }
}
