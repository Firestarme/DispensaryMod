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

        private static Texture2D ToggleIcon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone");
        private static Texture2D ToggleOpen = ContentFinder<Texture2D>.Get("UI/Commands/Halt");

        private CompPowerTrader cPower;

        private DrugDoorSettings drugDoorSettings;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cPower = base.GetComp<CompPowerTrader>();
        }

        public Building_DrugRoomDoor()
        {
            drugDoorSettings = new DrugDoorSettings(); 
        }

        private Room getIndoorRoom(IntVec3 Pos)
        {
            if (!Pos.UsesOutdoorTemperature(this.Map)) return Pos.GetRoom(this.Map); else return null;
        }

        public DrugDoorSettings GetDrugPolicySettings()
        {
            return drugDoorSettings;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<bool>(ref ToggleSetOpen, "ToggleSetOpen", true);
        }

        public override bool PawnCanOpen(Pawn p)
        {

            //Log.Message(string.Format("Pawn {0} is trying to access drug room", p.Name));

            if (!allowOpen(p))
            {
                return false;
            }
            return base.PawnCanOpen(p);
        }

        private bool allowOpen(Pawn p)
        {
            if (!this.powerComp.PowerOn) return true;
            if (!p.IsColonist) return false;
            
            return drugDoorSettings.PolicyAllowed(p.drugs.CurrentPolicy);
        }

        public override void DrawExtraSelectionOverlays()
        {

        }

        private void onOpenDoor()
        {
            this.DoorOpen();
            //ToggleSetOpen = !ToggleSetOpen;
        }

        //private void doToggleOpen()
        //{
        //    if (ToggleSetOpen)
        //    {
        //        this.DoorOpen();
        //    }
        //    else
        //    {
        //        this.DoorTryClose();
        //    }

        //}


        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> list = new List<Gizmo>();

            Command_Action ca2 = new Command_Action();
            ca2.icon = ToggleOpen;
            ca2.activateSound = SoundDef.Named("click");
            ca2.defaultLabel = "Toggle Open";
            ca2.action = new Action(this.onOpenDoor);
            list.Add(ca2);


            IEnumerable<Gizmo> commands = base.GetGizmos();
            return (commands == null) ? list.AsEnumerable<Gizmo>() : list.AsEnumerable<Gizmo>().Concat(commands);
        }

    }

}
