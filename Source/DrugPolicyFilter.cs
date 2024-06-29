using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DispensaryMod
{
    public class DrugPolicyFilter : IExposable
    {

        public event DrugPolicyFilterChangedHandler FilterChanged;

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

                var ev_args = new DrugPolicyFilterChangedArgs();
                ev_args.ChangedPolicyIds.Add(policy.id);
                FilterChanged?.Invoke(this, ev_args);
            }
        }

        public static IEnumerable<DrugPolicy> GetAllPolicies()
        {
            return Current.Game.drugPolicyDatabase.AllPolicies;
        }

        public void SetAllowAll()
        {
            allowedPolicyUId.Clear();
            var ev_args = new DrugPolicyFilterChangedArgs();

            foreach(DrugPolicy policy in GetAllPolicies())
            {
                allowedPolicyUId.Add(policy.id);
                ev_args.ChangedPolicyIds.Add(policy.id);
            }

            FilterChanged.Invoke(this, ev_args);
        }

        public void SetDisallowAll()
        {
            var ev_args = new DrugPolicyFilterChangedArgs();
            ev_args.ChangedPolicyIds.AddRange(allowedPolicyUId);

            allowedPolicyUId.Clear();
            FilterChanged?.Invoke(this, ev_args);
        }

        public void CopyAllowancesFrom(DrugPolicyFilter filter)
        {

            filter.VerifyPolicies();
            allowedPolicyUId.Clear();
            allowedPolicyUId.AddRange(filter.allowedPolicyUId);

            var ev_args = new DrugPolicyFilterChangedArgs();
            ev_args.ChangedPolicyIds.AddRange(filter.allowedPolicyUId);

            FilterChanged?.Invoke(this, ev_args);
        }

        public void VerifyPolicies()
        {
            HashSet<int> AllPolicyIds = new HashSet<int>(GetAllPolicies().Select<DrugPolicy,int>((DrugPolicy x)=>x.id));

            foreach(int id in allowedPolicyUId)
            {
                if (!AllPolicyIds.Contains(id)) allowedPolicyUId.Remove(id);
            }
        }

        public IEnumerable<int> GetAllowedPolicyIds()
        {
            return allowedPolicyUId;
        }

        public IEnumerable<int> GetRestrictedPolicyIds()
        {
            foreach (Policy policy in GetAllPolicies())
            {
                if( !this.allowedPolicyUId.Contains(policy.id)) yield return policy.id;
            }
        }

    }

    public delegate void DrugPolicyFilterChangedHandler(object sender, DrugPolicyFilterChangedArgs args);
    public class DrugPolicyFilterChangedArgs
    {
        public List<int> ChangedPolicyIds = new List<int>();
        public DrugPolicyFilterChangedArgs() { }
    }
}
