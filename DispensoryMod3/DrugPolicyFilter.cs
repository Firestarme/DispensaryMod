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
            return allowedPolicyUId.Contains(policy.uniqueId);
        }


        public void ExposeData()
        {
            throw new NotImplementedException();
        }

        public void SetAllow(DrugPolicy policy, bool allow)
        {
            if (allow != Allows(policy))
            {
                if (allow)
                {
                    allowedPolicyUId.Add(policy.uniqueId);
                }
                else
                {
                    allowedPolicyUId.Remove(policy.uniqueId);
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
                allowedPolicyUId.Add(policy.uniqueId);
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
            HashSet<int> AllPolicyIds = new HashSet<int>(GetAllPolicies().Select<DrugPolicy,int>((DrugPolicy x)=>x.uniqueId));

            foreach(int id in allowedPolicyUId)
            {
                if (!AllPolicyIds.Contains(id)) allowedPolicyUId.Remove(id);
            }
        }
    }
}
