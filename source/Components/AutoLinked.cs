﻿using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using System.Linq;
using Localize;

namespace CustomComponents
{
    public class Link
    {
        public ChassisLocations Location;
        public string ComponentDefId;
        public ComponentType? ComponentDefType = null;
    }

    [CustomComponent("Linked")]
    public class AutoLinked : SimpleCustomComponent, IOnItemGrabbed, IOnInstalled, IAdjustValidateDrop, IClearInventory
    {

        public Link[] Links { get; set; }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, ChassisLocations location)
        {
            if (Links != null && Links.Length > 0)
            {
                RemoveLinked(item, mechLab);
            }
        }

        public void RemoveLinked(IMechLabDraggableItem item, MechLabPanel mechLab)
        {
            foreach (var r_link in Links)
            {
                Control.LogDebug(DType.ComponentInstall, $"{r_link.ComponentDefId} from {r_link.Location}");
                DefaultHelper.RemoveMechLab(r_link.ComponentDefId, r_link.ComponentDefType ?? Def.ComponentType, r_link.Location);
            }
        }

        public static void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            var linked = mechDef.Inventory
                .Select(i => i.GetComponent<AutoLinked>())
                .Where(i => i != null && i.Links != null)
                .SelectMany(i => i.Links, (a,b) => new { custom = a, link = b}).ToList();

            if (linked.Count > 0)
            {
                var inv = mechDef.Inventory.ToList();

                foreach (var item in linked)
                {
                    var found = inv.FirstOrDefault(i =>
                        i.ComponentDefID == item.link.ComponentDefId && i.MountedLocation == item.link.Location);

                    if (found == null)
                        errors[MechValidationType.InvalidInventorySlots].Add(new Text(
                            Control.Settings.Message.Linked_Validate,
                            mechDef.Description.UIName, item.custom.Def.Description.Name,
                            item.custom.Def.Description.UIName,
                            item.link.Location));
                    else
                        inv.Remove(found);
                }
            }
        }

        public static bool CanBeFielded(MechDef mechDef)
        {
            var linked = mechDef.Inventory
                .Select(i => i.GetComponent<AutoLinked>())
                .Where(i => i != null && i.Links != null)
                .SelectMany(i => i.Links, (a, b) => new { custom = a, link = b }).ToList();

            if (linked.Count > 0)
            {
                var inv = mechDef.Inventory.ToList();

                foreach (var item in linked)
                {
                    var found = inv.FirstOrDefault(i =>
                        i.ComponentDefID == item.link.ComponentDefId && i.MountedLocation == item.link.Location);

                    if (found == null)
                        return false;

                }
            }

            return true;
        }


        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.LogDebug(DType.ComponentInstall, $"- AutoLinked");

            if (order.PreviousLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.LogDebug(DType.ComponentInstall, $"-- removing {link.ComponentDefId} from {link.Location}");
                    DefaultHelper.RemoveInventory(link.ComponentDefId, mech, link.Location, link.ComponentDefType ?? Def.ComponentType);
                }

            if (order.DesiredLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.LogDebug(DType.ComponentInstall, $"-- adding {link.ComponentDefId} to {link.Location}");
                    DefaultHelper.AddInventory(link.ComponentDefId, mech, link.Location, link.ComponentDefType ?? Def.ComponentType, state);
                }

        }

        public void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state, MechComponentRef source)
        {
            foreach (var l in Links)
            {
                result.RemoveAll(item => item.ComponentDefID == l.ComponentDefId && item.MountedLocation == l.Location);
            }
        }

        public bool ValidateDropOnAdd(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            Control.LogDebug(DType.ComponentInstall, "--- AutoLinked Add");

            if (Links == null || Links.Length == 0)
                return false;

            var result = false;
            foreach (var link in Links)
            {
                Control.LogDebug(DType.ComponentInstall, $"---- {link.ComponentDefId} to {link.Location}");
                var slot = DefaultHelper.CreateSlot(link.ComponentDefId, link.ComponentDefType ?? Def.ComponentType);

                if (slot != null)
                {
                    Control.LogDebug(DType.ComponentInstall, $"----- added");
                    changes.Enqueue(new AddDefaultChange(link.Location, slot));
                    result = true;
                }
                else
                    Control.LogDebug(DType.ComponentInstall, $"----- not found");
            }

            return result;
        }

        public bool ValidateDropOnRemove(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            Control.LogDebug(DType.ComponentInstall, "--- AutoLinked Remove");

            if (Links == null || Links.Length == 0)
                return false;

            var result = false;

            var to_remove = new List<SlotInvItem>();
            foreach (var link in Links)
            {
                Control.LogDebug(DType.ComponentInstall, $"---- {link.ComponentDefId} from {link.Location}");

                var remove = inventory.FirstOrDefault(i => i.item.ComponentDefID == link.ComponentDefId && i.location == link.Location && !to_remove.Contains(i));
                if (remove != null)
                {
                    Control.LogDebug(DType.ComponentInstall, $"----- removed");
                    to_remove.Add(remove);
                    changes.Enqueue(new RemoveChange(link.Location, remove.slot));
                    result = true;

                }
                else
                    Control.LogDebug(DType.ComponentInstall, $"----- not found");
            }
            return result;
        }

     
    }
}