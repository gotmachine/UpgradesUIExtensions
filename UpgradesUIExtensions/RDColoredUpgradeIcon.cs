/*
TODO :
- Show upgraded stats in partstatsupgrades tooltip widget
*/

using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UpgradesGUI
{
  [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
  public class RDColoredUpgradeIcon : MonoBehaviour
  {
    RDNode selectedNode;

    public void Update()
    {
      // Do nothing if there is no PartList
      if (RDController.Instance == null) { return; }
      if (RDController.Instance.partList == null) { return; }

      // In case the node is deselected
      if (RDController.Instance.node_selected == null) { selectedNode = null; }

      // Do nothing if the tooltip hasn't changed since last update
       if (selectedNode == RDController.Instance.node_selected) { return; }

      // Get the the selected node and partlist ui object
      selectedNode = RDController.Instance.node_selected;
      RDPartList partList = RDController.Instance.partList;

      var field = typeof(RDPartList).GetField("partListItems", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
      List<RDPartListItem> items = (List<RDPartListItem>)field.GetValue(partList);

      foreach (RDPartListItem item in items.Where(p => !p.isPart && p.upgrade != null))
      {
        item.gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(0.717f, 0.819f, 0.561f); // light green RGB(183,209,143)
      }
    }
  }
}
