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
        private DrugPolicyFilter _filter;
        public DrugPolicyFilter Filter => _filter;

        public DrugDoorSettings()
        {
            _filter = new DrugPolicyFilter();
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref _filter, "filter",new object[0]);
        }

        public void CopyFrom(DrugDoorSettings other)
        {
            _filter.CopyAllowancesFrom(other._filter);
        }

        public bool PolicyAllowed(DrugPolicy policy)
        {
            return _filter.Allows(policy);
        }
    }
}
