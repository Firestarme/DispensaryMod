using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace DispensaryMod
{
    [StaticConstructorOnStartup]
    class Building_DrugRoomDoor : Building_Door
    {

        private static Texture2D ToggleIcon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone");
        private static Texture2D ToggleOpen = ContentFinder<Texture2D>.Get("UI/Commands/Halt");

        private RoomSide drugRoomSide = RoomSide.NotSet;

        private CompPowerTrader cPower;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cPower = base.GetComp<CompPowerTrader>();
        }

        private Room getIndoorRoom(IntVec3 Pos)
        {
            if (!Pos.UsesOutdoorTemperature(this.Map)) return Pos.GetRoom(this.Map); else return null;
        }

        private Room getDrugRoom()
        {
            if (drugRoomSide == RoomSide.NotSet) SetRoomSide();
            if (drugRoomSide == RoomSide.NorthSide) return getIndoorRoom(getNCell());
            else if (drugRoomSide == RoomSide.SouthSide) return getIndoorRoom(getSCell());
            else return null;
        }

        private IntVec3 getNCell() { return (base.Position + IntVec3.North.RotatedBy(base.Rotation)); }
        private IntVec3 getSCell() { return (base.Position + IntVec3.South.RotatedBy(base.Rotation)); }

        private void SetRoomSide()
        {
            IntVec3 NCell = getNCell();
            IntVec3 SCell = getSCell();
            if (!NCell.UsesOutdoorTemperature(this.Map)) drugRoomSide = RoomSide.SouthSide;
            if (!SCell.UsesOutdoorTemperature(this.Map)) drugRoomSide = RoomSide.NorthSide;
            if (!NCell.UsesOutdoorTemperature(this.Map) & !SCell.UsesOutdoorTemperature(this.Map))
            {
                drugRoomSide = RoomSide.SouthSide;
            }
        }

        private void ToggleActiveRoom()
        {
            if (drugRoomSide == RoomSide.NorthSide) drugRoomSide = RoomSide.SouthSide;
            else drugRoomSide = RoomSide.NorthSide;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<Room>(ref DrugRoom, "DrugRoom", null);
            Scribe_Values.Look<RoomSide>(ref drugRoomSide, "RoomToggle", RoomSide.NotSet);
            //Scribe_Values.Look<bool>(ref ToggleSetOpen, "ToggleSetOpen", true);
        }

        public override bool PawnCanOpen(Pawn p)
        {

            Log.Message(string.Format("Pawn {0} is trying to access drug room", p.Name));


            if (!allowOpen(p))
            {
                return false;
            }
            return base.PawnCanOpen(p);
        }

        //private IEnumerable<JobDef> getAllowedJobs()
        //{
        //    yield return JobDefOf.HaulToCell;
        //    yield return JobDefOf.HaulToContainer;
        //    yield return JobDefOf.TendPatient;
        //    yield return JobDefOf.Clean;
        //    yield return JobDefOf.DoBill;
        //    yield return JobDefOf.Clean;
        //    yield return JobDefOf.Deconstruct;
        //    yield return JobDefOf.BeatFire;
        //    yield return JobDefOf.FixBrokenDownBuilding;
        //    yield return JobDefOf.Repair;
        //    yield return JobDefOf.Rescue;
        //    yield return JobDefOf.Uninstall;
        //}

        private IEnumerable<JobDef> getBlockedJobs()
        {
            yield return JobDefOf.Ingest;
            yield return JobDefOf.GotoWander;
            yield return JobDefOf.GotoAndBeSociallyActive;
        }

        private bool allowOpen(Pawn p)
        {
            Room drugRoom = getDrugRoom();

            if (drugRoom != null)
            {
                if (p.GetRoom() == drugRoom) return true;
                if (p.IsColonist)
                {
                    if (p.CurJob == null) {
                        Log.Message(String.Format("Pawn {0} has no job while accesing drug room", p.Name));
                        //p.needs.drugsDesire.CurLevelPercentage
                        //p.jobs.curDriver.

                        return true; }
                    if (!cPower.PowerOn) return true;
                    Log.Message(String.Format("Pawn {0} is trying to access drug room with job {1}", p.Name, p.CurJobDef.defName));
                    if (getBlockedJobs().Contains<JobDef>(p.CurJob.def)) return false;
                    return true;
                }
                return false;
            }
            return true;
        }

        public override void DrawExtraSelectionOverlays()
        {
            Room drugRoom = getDrugRoom();

            base.DrawExtraSelectionOverlays();
            if (drugRoom != null) GenDraw.DrawFieldEdges(drugRoom.Cells.ToList(), Color.green);
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

            Command_Action ca1 = new Command_Action();
            ca1.icon = ToggleIcon;
            ca1.activateSound = SoundDef.Named("Click");
            ca1.defaultLabel = "Toggle DrugRoom Side";
            ca1.action = new Action(this.ToggleActiveRoom);
            list.Add(ca1);

            Command_Action ca2 = new Command_Action();
            ca2.icon = ToggleOpen;
            ca2.activateSound = SoundDef.Named("click");
            ca2.defaultLabel = "Toggle Open";
            ca2.action = new Action(this.onOpenDoor);
            list.Add(ca2);


            IEnumerable<Gizmo> commands = base.GetGizmos();
            return (commands == null) ? list.AsEnumerable<Gizmo>() : list.AsEnumerable<Gizmo>().Concat(commands);
        }

        private enum RoomSide
        {
            NotSet = 0,
            NorthSide=1,
            SouthSide=2
        }

    }

}
