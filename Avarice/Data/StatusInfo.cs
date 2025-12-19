namespace Avarice.Data
{
    /// <summary>
    /// Simple struct to hold status information since Dalamud.Game.ClientState.Statuses.Status is now internal.
    /// </summary>
    internal struct StatusInfo
    {
        public uint StatusId { get; set; }
        public ushort Param { get; set; }
        public float RemainingTime { get; set; }
        public ulong SourceId { get; set; }
    }
}
