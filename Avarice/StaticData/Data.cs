namespace Avarice.StaticData
{
    internal class Data
    {
        public static readonly SortedList<ActionID, EnemyPositional> ActionPositional = new()
        {
            //Monk
            {ActionID.Demolish, EnemyPositional.Rear },
            {ActionID.SnapPunch, EnemyPositional.Flank },
            {ActionID.PouncingCoeurl, EnemyPositional.Flank },

            //Dragoon
            {ActionID.FangandClaw, EnemyPositional.Flank},
            {ActionID.WheelingThrust, EnemyPositional.Rear},
            {ActionID.ChaosThrust, EnemyPositional.Rear },
            {ActionID.ChaoticSpring, EnemyPositional.Rear },

            //Ninja
            {ActionID.TrickAttack, EnemyPositional.Rear },
            {ActionID.AeolianEdge,EnemyPositional.Rear },
            {ActionID.ArmorCrush, EnemyPositional.Flank },

            //Samurai
            {ActionID.Gekko, EnemyPositional.Rear},
            {ActionID.Kasha, EnemyPositional.Flank },

            //Reaper
            {ActionID.Gibbet, EnemyPositional.Flank},
            {ActionID.Gallows, EnemyPositional.Rear },
            {ActionID.ExecGibbet, EnemyPositional.Flank},
            {ActionID.ExecGallows, EnemyPositional.Rear },

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