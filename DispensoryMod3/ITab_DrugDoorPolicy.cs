using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static UnityEngine.Random;

namespace DispensaryMod
{
    internal class ITab_DrugDoorPolicy : ITab
    {

        private static readonly Vector2 WinSize = new Vector2(300f, 480f);

        private float TopAreaHeight = 35f;

        public override bool IsVisible
        {
            get
            {
                if (base.SelObject != null)
                {
                    if (base.SelObject is Thing thing && thing.Faction != null && thing.Faction != Faction.OfPlayer)
                    {
                        return false;
                    }
                }
                else
                {
                    if (base.AllSelObjects.Count <= 1)
                    {
                        return false;
                    }
                    foreach (object allSelObject in base.AllSelObjects)
                    {
                        if (allSelObject is Thing thing2 && thing2.Faction != null && thing2.Faction != Faction.OfPlayer)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public Building_DrugRoomDoor SettingsObj
        {
            get
            {
                return (Building_DrugRoomDoor) base.SelObject;
            }
        }



        private static float viewHeight;

        private Vector2 scrollPosition;

        public ITab_DrugDoorPolicy()
        {
            size = WinSize;
            labelKey = "Policy";
            tutorTag = "Policy";

        }


        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            Widgets.BeginGroup(rect);

            if (SettingsObj == null) return;
            if (SettingsObj.GetDrugPolicySettings() == null) return;

            DrugDoorSettings settings = SettingsObj.GetDrugPolicySettings();

            Rect rect3 = new Rect(0f, TopAreaHeight, rect.width, rect.height - TopAreaHeight);
           
            DoDrugPolicyFilterConfigWindow(rect3, settings.filter);

            Widgets.EndGroup();
        }


        private void DoDrugPolicyFilterConfigWindow(Rect rect, DrugPolicyFilter filter)
        {
            Widgets.DrawMenuSection(rect);
            Text.Font = GameFont.Tiny;
            float num = rect.width - 2f;
            Rect rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);

            if (Widgets.ButtonText(rect2, "ClearAll".Translate()))
            {
                filter.SetDisallowAll();
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 1f, rect2.y, rect.xMax - 1f - (rect2.xMax + 1f), 24f), "AllowAll".Translate()))
            {
                filter.SetAllowAll();
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            }

            Text.Font = GameFont.Small;
            rect.yMin = rect2.yMax;
            int num2 = 1;
            Rect rect3 = new Rect(rect.x + 1f, rect.y + 1f + (float)num2, rect.width - 2f, 24f);

            rect.yMin = rect3.yMax;

            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);
            Rect visibleRect = new Rect(0f, 0f, rect.width, rect.height);
            visibleRect.position += scrollPosition;
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float y = 2f;

            float num3 = y;
            Rect rect4 = new Rect(0f, y, viewRect.width, 9999f);
            visibleRect.position -= rect4.position;
            ListingTree_DrugPolicyFilter listing_DrugPolicyFilter = new ListingTree_DrugPolicyFilter(filter);
            listing_DrugPolicyFilter.Begin(rect4);
            listing_DrugPolicyFilter.ListCatagoryPolicies(visibleRect);
            listing_DrugPolicyFilter.End();

            if (Event.current.type == EventType.Layout)
            {
                viewHeight = num3 + listing_DrugPolicyFilter.CurHeight + 90f;
            }
            Widgets.EndScrollView();
        }
    }
}
