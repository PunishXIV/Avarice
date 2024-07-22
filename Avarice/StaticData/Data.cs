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
            //Dragoon
            {ActionID.FangandClaw, EnemyPositional.Flank},
            {ActionID.WheelingThrust, EnemyPositional.Rear},
            {ActionID.ChaosThrust, EnemyPositional.Rear },
            {ActionID.ChaoticSpring, EnemyPositional.Rear },
            //Monk
            {ActionID.Demolish, EnemyPositional.Rear },
            {ActionID.SnapPunch, EnemyPositional.Flank },
            //Ninja
            {ActionID.TrickAttack, EnemyPositional.Rear },
            {ActionID.AeolianEdge,EnemyPositional.Rear },
            {ActionID.ArmorCrush, EnemyPositional.Flank },
            //Reaper
            {ActionID.Gibbet, EnemyPositional.Flank},
            {ActionID.Gallows, EnemyPositional.Rear },
            {ActionID.ExecutionersGibbet, EnemyPositional.Flank},
            {ActionID.ExecutionersGallows, EnemyPositional.Rear },
            //Samurai
            {ActionID.Gekko, EnemyPositional.Rear},
            {ActionID.Kasha, EnemyPositional.Flank },
            //Viper
            {ActionID.FlankstingStrike, EnemyPositional.Flank },
            {ActionID.FlanksbaneFang, EnemyPositional.Flank },
            {ActionID.HindstingStrike, EnemyPositional.Rear },
            {ActionID.HindsbaneFang, EnemyPositional.Rear },
            {ActionID.HuntersCoil, EnemyPositional.Flank },
            {ActionID.SwiftskinsCoil, EnemyPositional.Rear },
        };
    }
}
