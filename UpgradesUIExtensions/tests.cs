/*
How to do it :


- apply selected upgrades only :

List<string> upgradesToApply
foreach (partmodule pm in part)

  foreach (confignode cn in pm.upgrades)
    if (isAvailable && isSelected)
      upgradesToApply.add(cn.name__)
  // pm.ApplyUpgradeNode (List< string > appliedUps, ConfigNode node, bool doLoad)
  pm.ApplyUpgradeNode (upgradesToApply, ConfigNode node, bool doLoad)




 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens.Editor;

namespace UpgradesUIExtensions
{
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public class PartUpgradesManager : MonoBehaviour
  {

  }

  public class PartListTooltipUpgradeWidget : PartListTooltipWidget
  {

  }

  public class PartUpgradesSelector
  {
    Part partPrefab;
    PartStatsUpgradeModule statsModule; // reference to the part PartStatsUpgradeModule is there is an upgrade for this PARTUPGRADE (null otherwise)
    List<PartModule> upgradedModules; // reference to the part modules that have this PARTUPGRADE, if any

    string upgradeName; // name of the PARTUPGRADE config node
    bool isAvailable; // is the upgrade researched and bought
    bool isSelected; // selected upgrades will be applied to parts added in the editor



    public PartUpgradesSelector(Part partPrefab)
    {
      this.partPrefab = partPrefab;
    }

    public static void ApplySelectedUpgrades(Part part)
    {
      List<string> upgradesToApply = new List<string>();
      foreach (PartModule pm in part.Modules)
      {
        foreach (ConfigNode cn in pm.upgrades)
        {
          // if (isAvailable && isSelected)
          if (cn.GetValue("name__") == "LVT-GasGen-precProp")
          {
            upgradesToApply.Add(cn.GetValue("name__"));
          }
        }

        // pm.ApplyUpgradeNode (List< string > appliedUps, ConfigNode node, bool doLoad)
        // Applies the upgrades to a confignode. Will either copy the upgrades back to the node or (if doLoad) calls load/onload on it.
        pm.ApplyUpgradeNode(upgradesToApply, ConfigNode.CreateConfigFromObject(pm), true);

      }
    }
  }
}
