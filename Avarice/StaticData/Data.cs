using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avarice.StaticData
{
    internal class Data
    {
        public static readonly SortedList<ActionID, EnemyPositional> ActionPositional = new()
        {
            {ActionID.FangandClaw, EnemyPositional.Flank},
            {ActionID.WheelingThrust, EnemyPositional.Rear},
            {ActionID.ChaosThrust, EnemyPositional.Rear },
            {ActionID.ChaoticSpring, EnemyPositional.Rear },
            {ActionID.Demolish, EnemyPositional.Rear },
            {ActionID.SnapPunch, EnemyPositional.Flank },
            {ActionID.TrickAttack, EnemyPositional.Rear },
            {ActionID.AeolianEdge,EnemyPositional.Rear },
            {ActionID.ArmorCrush, EnemyPositional.Flank },
            {ActionID.Gibbet, EnemyPositional.Flank},
            {ActionID.Gallows, EnemyPositional.Rear },
            {ActionID.Gekko, EnemyPositional.Rear},
            {ActionID.Kasha, EnemyPositional.Flank },
        };
    }
}
