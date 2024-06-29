using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DispensaryMod
{
    public class DrugPolicyFilter : IExposable
    {

        [Unsaved(false)]
        private HashSet<int> allowedPolicyUId = new HashSet<int>();

        public bool Allows(DrugPolicy policy)
        {
            return allowedPolicyUId.Contains(policy.id);
        }


        public void ExposeData()
        {
            Scribe_Collections.Look(ref allowedPolicyUId,"allowedPolicyUId");
        }

        public void SetAllow(DrugPolicy policy, bool allow)
        {
            if (allow != Allows(policy))
            {
                if (allow)
                {
                    allowedPolicyUId.Add(policy.id);
                }
                else
                {
                    allowedPolicyUId.Remove(policy.id);
                }
            }
        }

        private IEnumerable<DrugPolicy> GetAllPolicies()
        {
            return Current.Game.drugPolicyDatabase.AllPolicies;
        }

        public void SetAllowAll()
        {
            allowedPolicyUId.Clear();

            foreach(DrugPolicy policy in GetAllPolicies())
            {
                allowedPolicyUId.Add(policy.id);
            }
        }

        public void SetDisallowAll()
        {
            allowedPolicyUId.Clear();
        }

        public void CopyAllowancesFrom(DrugPolicyFilter filter)
        {
            filter.VerifyPolicies();
            allowedPolicyUId.Clear();
            allowedPolicyUId.AddRange(filter.allowedPolicyUId);
        }

        public void VerifyPolicies()
        {
            HashSet<int> AllPolicyIds = new HashSet<int>(GetAllPolicies().Select<DrugPolicy,int>((DrugPolicy x)=>x.id));

            foreach(int id in allowedPolicyUId)
            {
                if (!AllPolicyIds.Contains(id)) allowedPolicyUId.Remove(id);
            }
        }
    }
}
