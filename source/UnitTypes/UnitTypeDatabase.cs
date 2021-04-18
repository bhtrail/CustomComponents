﻿using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    public class UnitTypeDatabase
    {
        private static UnitTypeDatabase _instance;

        List<IUnitType> types = new List<IUnitType>();
        Dictionary<string, string[]> known = new Dictionary<string, string[]>();


        public static UnitTypeDatabase Instance
        {
            get
            {
                if(_instance ==null)
                    _instance = new UnitTypeDatabase();
                return _instance;
            }
        }

        private UnitTypeDatabase()
        {


        }

        public void RegisterUnitType(IUnitType unitType)
        {
            if (unitType == null)
            {
                Control.LogError("Null unit type, skipping");
            }

            var old = types.FirstOrDefault(i => i.Name == unitType.Name);
            if (old != null)
            {
                Control.LogDebug(DType.UnitType, $"-- removed old one");
                types.Remove(old);
            }

            types.Add(unitType);
            Control.Log("UnitType " + unitType.Name + " Registred");
        }

        public string[] this[MechDef mech] => GetUnitTypes(mech);

        public string[] GetUnitTypes(MechDef mech)
        {
            string[] result;

            if (known.TryGetValue(mech.ChassisID, out result))
                return result;

            result = CheckCustom(mech);

            var tags = new List<string>();

            foreach (var unitType in types)
            {
                if (unitType.IsThisType(mech))
                    tags.Add(unitType.Name);
            }

            result = tags.ToArray();
            known[mech.Name] = result;
            return result;
        }

        //Method to patch with other custom type
        public string[] CheckCustom(MechDef mech)
        {
            return mech.Is<UnitTypeCustom>(out var ut) ? ut.Types : null;
        }

        public void ShowRegistredTypes()
        {
            if (!Control.Settings.DebugInfo.HasFlag(DType.UnitType))
                return;
            foreach (var unitType in types)
            {
                Control.LogDebug(DType.UnitType, unitType.ToString());
            }
        }
    }

}