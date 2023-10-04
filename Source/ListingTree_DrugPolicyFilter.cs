using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DispensaryMod
{
    public class ListingTree_DrugPolicyFilter : Listing_Tree
    {

        private DrugPolicyFilter filter;
        private Rect visibleRect;

        public ListingTree_DrugPolicyFilter(DrugPolicyFilter filter)
        {
            this.filter = filter;
        }

        public void ListCatagoryPolicies(Rect visibleRect)
        {
            this.visibleRect = visibleRect;

            foreach (DrugPolicy policy in Current.Game.drugPolicyDatabase.AllPolicies)
            {
                DoPolicy(policy);
            }
        }

        private bool CurrentRowVisibleOnScreen()
        {
            Rect other = new Rect(0f, curY, base.ColumnWidth, lineHeight);
            return visibleRect.Overlaps(other);
        }

        private void DoPolicy(DrugPolicy policy)
        {
            Color? color = null;

            if (CurrentRowVisibleOnScreen())
            {
                LabelLeft(policy.label, policy.label, 0, 0, color);
                bool checkOn = filter.Allows(policy);
                bool flag = checkOn;
                Widgets.Checkbox(new Vector2(LabelWidth, curY), ref checkOn, lineHeight, disabled: false, paintable: true);
                if (checkOn != flag)
                {
                    filter.SetAllow(policy, checkOn);
                }
            }

            EndLine();
        }
    }
}
