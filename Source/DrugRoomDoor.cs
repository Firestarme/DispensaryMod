using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace DispensaryMod
{
    [StaticConstructorOnStartup]
    class Building_DrugRoomDoor : Building_Door
    {
        //private static Texture2D ToggleIcon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone");
        private static Texture2D ToggleOpen = ContentFinder<Texture2D>.Get("UI/Commands/Halt");

        private CompPowerTrader cPower;
        private DrugDoorSettings drugDoorSettings;

        private bool _isPowered = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cPower = base.GetComp<CompPowerTrader>();
        }

        public Building_DrugRoomDoor() 
        {
            drugDoorSettings = new DrugDoorSettings();
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public DrugDoorSettings GetDrugPolicySettings()
        {
            return drugDoorSettings;
        }

        public override void ExposeData()
        {
            drugDoorSettings.Filter.FilterChanged -= DoorSettingsChangeHandler;
            base.ExposeData();
            Scribe_Deep.Look<DrugDoorSettings>(ref drugDoorSettings, "drugDoorSettings", new object[0]);
            drugDoorSettings.Filter.FilterChanged += DoorSettingsChangeHandler;
        }

        public override void Tick()
        {
            base.Tick();

            if (_isPowered != this.powerComp.PowerOn)
            {
                // When powered state changes, clear the reachability cache for all restricted pawns
                ClearPathCacheForRestrictedPawns();
                _isPowered = this.powerComp.PowerOn;
            }
        }

        public override bool PawnCanOpen(Pawn p)
        {
            if (allowOpen(p)) return base.PawnCanOpen(p);

            //Log.Message($"Pawn {p.Name} with policy {p.drugs.CurrentPolicy.label} blocked by drug door");
            return false;

        }

        private bool allowOpen(Pawn p)
        {
            if (!this.powerComp.PowerOn) return true;
            if (!p.IsColonist) return false;
            if (p.drugs.CurrentPolicy == null) return true;

            return drugDoorSettings.PolicyAllowed(p.drugs.CurrentPolicy);
        }

        private void onOpenDoorNow()
        {
            this.DoorOpen();
            ClearPathCacheForRestrictedPawns();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> list = new List<Gizmo>();

            Command_Action ca2 = new Command_Action();
            ca2.icon = ToggleOpen;
            ca2.activateSound = SoundDef.Named("click");
            ca2.defaultLabel = "Open Door Now";
            ca2.action = new Action(this.onOpenDoorNow);
            list.Add(ca2);


            IEnumerable<Gizmo> commands = base.GetGizmos();
            return (commands == null) ? list.AsEnumerable<Gizmo>() : list.AsEnumerable<Gizmo>().Concat(commands);
        }


        private void ClearPathCacheForPawnsWithPolicy(int DrugPolicyId)
        {
            foreach (Pawn pawn in this.Map.mapPawns.AllHumanlike)
            {
                if (!pawn.IsColonist) continue;
                if (pawn.drugs.CurrentPolicy == null) continue;
                if(pawn.drugs.CurrentPolicy.id != DrugPolicyId) continue;

                this.Map.reachability.ClearCacheFor(pawn);
               // Log.Message($"Clearing Cache for {pawn.Name} with policy {pawn.drugs.CurrentPolicy.label}");
            }
        }

        private void DoorSettingsChangeHandler(object sender, DrugPolicyFilterChangedArgs args)
        {
            foreach (int ChangedId in args.ChangedPolicyIds)
            {
                ClearPathCacheForPawnsWithPolicy(ChangedId);
                //Log.Message($"Door policy changed, Clearing Cache for policy with id {ChangedId}");
            }
        }

        private void ClearPathCacheForRestrictedPawns()
        {
            foreach (int policyID in this.drugDoorSettings.Filter.GetRestrictedPolicyIds())
            {
                ClearPathCacheForPawnsWithPolicy(policyID);
            }
        }

    }

}
