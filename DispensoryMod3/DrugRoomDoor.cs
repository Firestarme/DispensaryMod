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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cPower = base.GetComp<CompPowerTrader>();
        }

        public Building_DrugRoomDoor() { }

        public override void PostMake()
        {
            base.PostMake();
            drugDoorSettings = new DrugDoorSettings();

        }

        public DrugDoorSettings GetDrugPolicySettings()
        {
            return drugDoorSettings;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<DrugDoorSettings>(ref drugDoorSettings, "drugDoorSettings", new object[0]);
        }

        public override bool PawnCanOpen(Pawn p)
        {
            if (allowOpen(p)) return base.PawnCanOpen(p);


            //Not super happy with this solution, as this likley incurs a performace hit to blocked pawns
            //Prehaps investigate clearing the cache when pathfinding fails?
            this.Map.reachability.ClearCacheFor(p);


            Log.Message($"Pawn {p.Name} with policy {p.drugs.CurrentPolicy.label} blocked by drug door");
            return false;

        }

        private bool allowOpen(Pawn p)
        {
            if (!this.powerComp.PowerOn) return true;
            if (!p.IsColonist) return true;
            if (p.drugs.CurrentPolicy == null) return true;

            return drugDoorSettings.PolicyAllowed(p.drugs.CurrentPolicy);
        }

        private void onOpenDoorNow()
        {
            this.DoorOpen();
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

    }

}
