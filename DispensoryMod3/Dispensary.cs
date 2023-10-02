using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace DispensaryMod
{
    public class Building_Dispensary : Building
    {

        public static int DispenseDuration = 200;

        private CompPowerTrader pcomp;
        private CompFlickable fcomp;

        public IntVec3 backCell { get { return base.Position + IntVec3.North.RotatedBy(base.Rotation); } }

        public Zone_Stockpile stockpile(Map map)
        {
            Zone zone = this.backCell.GetZone(map);
            if (zone.GetType() == typeof(Zone_Stockpile)) return (Zone_Stockpile)zone;
            return null;
        }

        public bool isDrugAvailable(Map map, ThingDef drug)
        {
            if (this.stockpile(map) != null)
            {
                foreach (Thing t in stockpile(map).AllContainedThings)
                {
                    if (t.def == drug) return true;
                }
            }
            return false;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.pcomp = base.GetComp<CompPowerTrader>();
            this.fcomp = base.GetComp<CompFlickable>();
        }

        public bool canDispenseDrugNow(Map map, Thing drug)
        {
            return this.pcomp.PowerOn & this.stockpile(map).AllContainedThings.Contains(drug) & fcomp.SwitchIsOn;
        }

        public Thing tryDispenseDrug(Map map, Thing drugStack)
        {
            if (this.stockpile(map) != null && this.stockpile(map).AllContainedThings.Contains(drugStack))
            {
                this.def.building.soundDispense.PlayOneShot(SoundInfo.InMap(new TargetInfo(this)));
                Thing drug;
                if (drugStack.stackCount == 1)
                {
                    drugStack.Destroy();
                }
                else
                {
                    drugStack.stackCount -= 1;
                }
                drug = ThingMaker.MakeThing(drugStack.def);
                return drug;
            }
            return null;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            //new PlaceWorker_Dispensary().DrawGhost(this.def, this.Position, this.Rotation, Color.green);

            Zone_Stockpile selStockpile = this.stockpile(this.Map);
            if (selStockpile != null) {GenDraw.DrawFieldEdges(selStockpile.Cells.ToList(), Color.green); }
            
        }
    }


    public class PlaceWorker_Dispensary : PlaceWorker
    {
        List<IntVec3> StockpileCells = new List<IntVec3>();

        private IntVec3 getBackVec(IntVec3 center, Rot4 rot)
        {
            return center + IntVec3.North.RotatedBy(rot);
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostColor,Thing thing = null)
        {
            GenDraw.DrawFieldEdges(StockpileCells, Color.green);
        }

        private List<IntVec3> GetStockpileCells(Map map, IntVec3 BackVec)
        {
            Zone_Stockpile sp = getStockpile(map, BackVec);
            if (sp == null) { return new List<IntVec3> { BackVec }; }
            else { return sp.cells; }
        }

        private Zone_Stockpile getStockpile(Map map, IntVec3 BackVec)
        {
            Zone zone = map.zoneManager.ZoneAt(BackVec);
            if (zone is Zone_Stockpile) return (Zone_Stockpile)zone;
            return null;
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null,Thing thing = null)
        {
            Zone_Stockpile Sp = getStockpile(map, getBackVec(loc, rot));

            if (Sp == null)
            {
                StockpileCells = GetStockpileCells(map, getBackVec(loc, rot));
                return "Must build with back against stockpile";
            }
            else
            {
                StockpileCells.Clear();
                return true;
            }
        }

    }

}