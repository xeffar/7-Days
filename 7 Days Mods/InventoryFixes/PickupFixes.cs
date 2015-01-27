using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InventoryFixes {
  public class PickupFixes {

    // NGuiInvGrid
    public static bool TransferItemStackToBagAndBeltNoUpdate(
        NGuiInvGrid _bag,
        NGuiInvGrid _belt,
        ref InventoryField _itemStack,
        HashSet<int> _modifiedBagSlots,
        HashSet<int> _modifiedBeltSlots) {
      if (_itemStack.md0002()) {
        return false;
      }
      bool flag = _belt.TransferItemStackNoUpdate(ref _itemStack, _modifiedBeltSlots, false);
      if (_itemStack.md0002()) {
        return flag;
      }
      flag = (_bag.TransferItemStackNoUpdate(ref _itemStack, _modifiedBagSlots, false) || flag);
      if (_itemStack.md0002()) {
        return flag;
      }
      flag = (_belt.TransferItemStackNoUpdate(ref _itemStack, _modifiedBeltSlots, true) || flag);
      if (_itemStack.md0002()) {
        return flag;
      }
      return _bag.TransferItemStackNoUpdate(ref _itemStack, _modifiedBagSlots, true) || flag;
    }

    // NGuiDragAndDropItem
    public static bool MoveItemToQuickTakeTarget(ref NGuiDragAndDropItem dragAndDropItem, int _itemCount) {
      if (dragAndDropItem.md000c() != null && dragAndDropItem.md000c().grid.UiGridQuickTake != null) {
        NGuiDragAndDropContainer nGuiDragAndDropContainer = dragAndDropItem.md000c();
        int stackNumber = ItemBase.list[dragAndDropItem.itemStack.itemValue.type].StackNumber;
        int num = (_itemCount <= stackNumber) ? _itemCount : stackNumber;
        int num2 = dragAndDropItem.itemStack.count - num;
        dragAndDropItem.itemStack.count = num;
        dragAndDropItem.md0006();
        NGuiInvGrid containerInv = NGUIWindowManager.Instance.GetWindow(EnumNGUIWindow.LootContainer).GetComponentInChildren<NGuiInvGrid>();
        if (NGUITools.GetActive(containerInv) && containerInv != nGuiDragAndDropContainer.grid) {
          containerInv.AddItemStack(ref dragAndDropItem.itemStack, true);
        } else {
          if (dragAndDropItem.itemStack.count > 0 && nGuiDragAndDropContainer.grid.UiGridQuickTake2 != null) {
            nGuiDragAndDropContainer.grid.UiGridQuickTake2.AddItemStack(ref dragAndDropItem.itemStack, false);
          }
          if (dragAndDropItem.itemStack.count > 0 && nGuiDragAndDropContainer.grid.UiGridQuickTake != null) {
            nGuiDragAndDropContainer.grid.UiGridQuickTake.AddItemStack(ref dragAndDropItem.itemStack, false);
          }
          if (dragAndDropItem.itemStack.count > 0) {
            if (dragAndDropItem.itemStack.count > 0 && nGuiDragAndDropContainer.grid.UiGridQuickTake2 != null) {
              nGuiDragAndDropContainer.grid.UiGridQuickTake2.AddItemStack(ref dragAndDropItem.itemStack, true);
            }
            if (dragAndDropItem.itemStack.count > 0 && nGuiDragAndDropContainer.grid.UiGridQuickTake != null) {
              nGuiDragAndDropContainer.grid.UiGridQuickTake.AddItemStack(ref dragAndDropItem.itemStack, true);
              num2 += dragAndDropItem.itemStack.count;
            }
          }
        }
        if (dragAndDropItem.itemStack.count == 0 && nGuiDragAndDropContainer != null ) {
          nGuiDragAndDropContainer.grid.OnItemRemoved(nGuiDragAndDropContainer.Index);
        }
        else {
          num2 = dragAndDropItem.itemStack.count;
        }
        if (num2 == 0) {
          UnityEngine.Object.Destroy(dragAndDropItem.transform.gameObject);
          return true;
        }
        InventoryField inventoryField = new InventoryField(dragAndDropItem.itemStack.itemValue, num2);
        NGuiInvGrid.SetItemInSlot(nGuiDragAndDropContainer.grid, nGuiDragAndDropContainer.gameObject, inventoryField);
        nGuiDragAndDropContainer.grid.OnItemAdded(nGuiDragAndDropContainer.Index, inventoryField);
      }
      return false;
    }
  }
}
