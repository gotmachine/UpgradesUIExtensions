using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UpgradesUIExtensions
{
  public class UpgradePrefab : MonoBehaviour
  {

    public Part part;
    public List<PartUpgrade> upgrades;

    // If all upgrades were disabled by the player and
    // and the part has a PartStatsUpgradeModule,
    // we need to reload the part from its confignode
    public bool needConfigReload = false;

    public UpgradePrefab(Part part)
    {
      this.part = part;

      upgrades = new List<PartUpgrade>();
      foreach (PartModule pm in part.Modules)
      {
        for (int i = 0; i < pm.upgrades.Count; i++)
        {
          PartUpgrade partUpgrade;
          // if the PartUpgrade doesn't exist, we need to create it
          if (upgrades.Where(p => p.upgradeName == pm.upgrades[i].GetValue("name__")).Count() == 0)
          {
            partUpgrade = new PartUpgrade(pm.upgrades[i].GetValue("name__"), part.partInfo.name);
            upgrades.Add(partUpgrade);
          }
          else
          {
            partUpgrade = upgrades.Find(p => p.upgradeName == pm.upgrades[i].GetValue("name__"));
          }

          // Add the ModuleUpgrade to the PartUpgrade
          PartUpgrade.ModuleUpgrade mu = new PartUpgrade.ModuleUpgrade();
          mu.moduleName = pm.moduleName;
          mu.moduleTitle = pm.GUIName;
          mu.description = pm.upgrades[i].GetValue("description__");
          mu.statsNode = pm.upgrades[i].GetNode("PartStats");

          partUpgrade.AddModule(mu);

          // We check the fields "ExclusiveWith__" (standard modules) or "IsAdditiveUpgrade__" (PartStatsUpgradeModule)
          // to find wich upgrades this upgrade override.
          if (i > 0)
          {
            if (pm is PartStatsUpgradeModule)
            {
              bool isAdditive = false; // TODO : check if this is the default value
              pm.upgrades[i].TryGetValue("IsAdditiveUpgrade__",ref isAdditive);
              // If this UPGRADE node has IsAdditiveUpgrade__ = false, this mean that it will override
              // all previous UPGRADE nodes. This mean that those upgrades can't be selected as long as this one is,
              // so we iterate trough the previous nodes and them to the override list
              if (!isAdditive)
              {
                for (int j = i-1; j >= 0; j--)
                {
                  partUpgrade.AddOverride(pm.upgrades[j].GetValue("name__"));
                }
              }
            }
            else
            {
              for (int j = i - 1; j >= 0; j--)
              {
                if (pm.upgrades[i].GetValue("ExclusiveWith__") == pm.upgrades[j].GetValue("ExclusiveWith__"))
                {
                  partUpgrade.AddOverride(pm.upgrades[j].GetValue("name__"));
                }
              }
            }
          }
        }
      }
      // Finally, we determine wich upgrade can be toggled by the player
      ParseUpgradesState();
    }

    // Update the toggleable status of every upgrade
    public void ParseUpgradesState()
    {
      foreach (PartUpgrade pu in upgrades)
      {
          if (!pu.isUnlocked)
          {
            pu.upgradeState = PartUpgrade.UpgradeState.Unresearched;
          }
          else if (upgrades.Exists(p => p.isOverriding(pu.upgradeName)))
          {
            pu.upgradeState = PartUpgrade.UpgradeState.Overriden;
          }
          else
          {
            pu.upgradeState = PartUpgrade.UpgradeState.Enabled;
          }
      }
    }

    // Update the state of every upgrades in the prefab in responseto player toggling
    // Update the enabled state of relevant Upgrades in the UpgradeHandler
    public void UpdateUpgradesState(UpgradeWidgetComponent toggledUpdate)
    {
      PartUpgrade pu = upgrades.Find(p => p.upgradeName == toggledUpdate.upgrade);
      pu.upgradeState = toggledUpdate.upgradeState;

      // We reenable overriden upgrades that may have been disabled :
      if (toggledUpdate.upgradeState == PartUpgrade.UpgradeState.Enabled)
      {
        pu.UpdateOverridenUpgrades(this);
      }
      // In case the player has disabled an upgrade, we need to enable the last overriden upgrade, if any
      else if (toggledUpdate.upgradeState == PartUpgrade.UpgradeState.Disabled)
      {
        pu.EnableLastOverridenUpgrade(this);
      }
    }
  }

  public class PartUpgrade : MonoBehaviour
  {
    // NOTE: I'm not sure what will happen if there is several modules of the same name in the part
    // Maybe we need to store the module index ?
    public class ModuleUpgrade
    {
      public string moduleName;
      public string moduleTitle;
      public string description;
      public ConfigNode statsNode;
    }

    public enum UpgradeState
    {
      Overriden,
      Enabled,
      Disabled,
      Unresearched
    };

    public string partName;
    public string upgradeName;
    public UpgradeState upgradeState;
    public bool isUnlocked;
    public bool isUntracked;

    // If all upgrades were disabled by the player, 
    // we need to reload the module from its confignode
    public bool needConfigReload = false; 

    private List<ModuleUpgrade> moduleUpgrades;
    private List<string> overridenUpgrades;
    private string techTitle;
    private string upgradeTitle;

    public PartUpgrade(string name, string partName)
    {
      this.partName = partName;
      upgradeName = name;
      isUnlocked = PartUpgradeManager.Handler.IsUnlocked(upgradeName);
      string techRequired = PartUpgradeManager.Handler.GetUpgrade(upgradeName).techRequired;
      if (techRequired != null)
      {
        techTitle = ResearchAndDevelopment.GetTechnologyTitle(techRequired);
        isUntracked = false;
      }
      else
      {
        techTitle = "Undefined";
        isUntracked = true;
      }
      upgradeTitle = PartUpgradeManager.Handler.GetUpgrade(upgradeName).title;
      moduleUpgrades = new List<ModuleUpgrade>();
      overridenUpgrades = new List<string>();
    }

    public static PartUpgrade GetUpgradeInList(List<UpgradePrefab> prefabList, string part, string upgrade)
    {
      return prefabList.Find(p => p.part.partInfo.name == part).upgrades.Find(u => u.upgradeName == upgrade);
    }

    public void UpdateOverridenUpgrades(UpgradePrefab prefab)
    {
      foreach (PartUpgrade pu in prefab.upgrades)
      {
        if (overridenUpgrades.Contains(pu.upgradeName))
        {
          pu.upgradeState = UpgradeState.Overriden;
        }
      }
    }

    public void EnableLastOverridenUpgrade(UpgradePrefab prefab)
    {
      foreach (string upgrade in overridenUpgrades)
      {
        if (prefab.upgrades.Exists(p => p.upgradeName == upgrade))
        {
          PartUpgrade pu = prefab.upgrades.Find(p => p.upgradeName == upgrade);
          if (pu.upgradeState == UpgradeState.Overriden)
          {
            pu.upgradeState = UpgradeState.Enabled;
            break;
          }
        }
      }
    }

    public void AddModule(ModuleUpgrade module)
    {
      moduleUpgrades.Add(module);
    }

    public void AddOverride(string upgradeName)
    {
      if (!overridenUpgrades.Contains(upgradeName))
      {
        overridenUpgrades.Add(upgradeName);
      }
    }

    public bool isOverriding(string upgradeName)
    {
      return overridenUpgrades.Contains(upgradeName);
    }

    public string GetTitle()
    {
      return upgradeTitle;
    }

    // Build the string to display in the widget
    public string GetInfo(UpgradePrefab infoPrefab = null)
    {
      string info = "";
      switch (upgradeState)
      {
        case UpgradeState.Overriden:
          if (ReferenceEquals(infoPrefab,null))
          {
            info += "Overriden\n";
            break;
          }
          PartUpgrade pu = infoPrefab.upgrades.Find(p => p.isOverriding(upgradeName) && p.upgradeState == UpgradeState.Enabled);
          if (!ReferenceEquals(pu, null))
          {
            info += "Overriden by upgrade:\n" + pu.upgradeTitle;
            break;
          }
          break;
        case UpgradeState.Enabled:
          info += "Upgrade is enabled\n";
          break;
        case UpgradeState.Disabled:
          info += "Upgrade is disabled\n";
          break;
        case UpgradeState.Unresearched:
          info += "Will be available at node:\n" + techTitle;
          break;
      }
      info += "\n";

      if (moduleUpgrades.Exists(p => p.moduleName == "PartStatsUpgradeModule"))
      {
        info += "<color=#99ff00ff><b>Part stats modifiers:</b></color>\n";
        ConfigNode cn = moduleUpgrades.Find(p => p.moduleName == "PartStatsUpgradeModule").statsNode;
        float mass = 0f;       
        if (cn.TryGetValue("mass", ref mass)){info += "Mass: " + mass.ToString("+ 0.###;- #.###") + " t\n"; }
        if (cn.TryGetValue("massAdd", ref mass)) { info += "Mass: " + mass.ToString("+ 0.###;- #.###") + " t\n"; }
        float cost = 0f;
        if (cn.TryGetValue("cost", ref cost)) { info += "Cost: " + cost.ToString("+ 0;- #") + " <sprite=2 tint=1>\n"; }
        if (cn.TryGetValue("costAdd", ref cost)) { info += "Cost: " + cost.ToString("+ 0;- #") + " <sprite=2 tint=1>\n"; }
        foreach (ConfigNode.Value v in cn.values)
        {
          if (v.name != "mass" && v.name != "cost" && v.name != "massAdd " && v.name != "costAdd")
          {
            info += v.name + ": " + v.value + "\n";
          }
        }
      }
      foreach (ModuleUpgrade mu in moduleUpgrades)
      {
        if (mu.moduleName != "PartStatsUpgradeModule")
        {
          info += "<color=#99ff00ff><b>" + mu.moduleTitle + ":</b></color>\n" + mu.description + "\n";
        }
      }
      return info;
    }
  }

  public class UpgradeWidgetComponent : MonoBehaviour
  {
    public string partName;
    public string upgrade;
    public PartUpgrade.UpgradeState upgradeState;
    public bool isUpdated;
  }
}
