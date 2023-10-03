using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace DispensaryMod
{
    internal class DrugDoorSettings : IExposable
    {
        public DrugPolicyFilter filter;

        public DrugDoorSettings()
        {
            filter = new DrugPolicyFilter();
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref filter, "filter",new object[0]);
        }

        public void CopyFrom(DrugDoorSettings other)
        {
            filter.CopyAllowancesFrom(other.filter);
        }

        public bool PolicyAllowed(DrugPolicy policy)
        {
            return filter.Allows(policy);
        }
    }
}
