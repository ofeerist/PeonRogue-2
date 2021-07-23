using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.PhotonEvent
{
    public enum Event
    {
        Damage = 1,
        RollDamage = 2,
        SlamDamage = 4,
        DashDamage = 5,
        AttackEffect = 6,
        RollEffect = 7,
        GameStart = 8
    }
}
