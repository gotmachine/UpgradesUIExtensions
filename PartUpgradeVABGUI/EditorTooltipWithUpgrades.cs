/*
FUTURE PLANS :
Be able to select wich upgarde we want on the part in editor partlist UI :
maybe by extending PartListTooltipWidget with a special "upgradewidget" for each active and available update
this widget can be clicked on to be disabled, wich update the part prefab, the widget list and tooltip part stats and widget list with the previous upgrade.
then we need to hook onpartadded to alter the created part according to the tooltip state
---> or maybe, at the top of the widget list, do a module/updates to toggle the widget list mode
in update mode, show a widget for every available update update, that can be clicked on to enable/disable it
*/

using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens.Editor;
using System.Globalization;
using System.Text;

namespace UpgradesUIExtensions
{
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public class PatchEditorTooltip : MonoBehaviour
  {
    List<Part> upgradedParts = new List<Part>();
    PartListTooltip tooltip = null;

    public void Start()
    {
      // Create an hidden and disabled instance for every loaded part with their stats correctly updated according to the available updates
      // These instances will be used to retrieve the updated tooltip text and widgets
      foreach (AvailablePart ap in PartLoader.LoadedPartsList)
      {
        // Part newPart = (Part)obj;
        Part upgradedPart = Instantiate(ap.partPrefab);
        upgradedPart.gameObject.name = ap.name;
        upgradedPart.partInfo = ap;

        // Temporally enable the part to be able to call ApplyUpgrades on all modules
        // so upgrades nodes are checked and applyied to the part/module properties :
        upgradedPart.gameObject.SetActive(true);
        foreach (PartModule pm in upgradedPart.Modules)
        {
          pm.ApplyUpgrades(PartModule.StartState.Editor);
          // Not really needed since the whole part will be disabled, but probably safer :
          pm.enabled = false;
          pm.isEnabled = false;
        }

        // Again, not needed in practice but better safe than sorry :
        upgradedPart.enabled = false;

        // Disable the gameobject so the part isn't rendered, can't be interacted with and
        // no code run from updates / fixedupdates
        upgradedPart.gameObject.SetActive(false);

        // Add the part to our list
        upgradedParts.Add(upgradedPart);
      }
    }

    public void Update()
    {
      // Do nothing if there is no PartListTooltip on screen
      if (PartListTooltipMasterController.Instance.currentTooltip == null) { return; }

      // Do nothing if the tooltip hasn't changed since last update
      if (tooltip == PartListTooltipMasterController.Instance.currentTooltip) { return; }

      // Get the PartListTooltip
      tooltip = PartListTooltipMasterController.Instance.currentTooltip;

      // Find the currently shown part in the list of updated parts instances
      var field = typeof(PartListTooltip).GetField("partInfo", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
      AvailablePart partInfo = (AvailablePart)field.GetValue(tooltip);
      Part part = upgradedParts.Find(p => p.partInfo == partInfo);
      if (part == null)
      {
        Debug.Log("Part upgrade stats for \"" + partInfo.title + "\" not found, using default stats.");
        return;
      }

      // Rebuilding the tooltip cost string :
      tooltip.textCost.text = "<b>Cost: </b><sprite=2 tint=1>  <b>" +
        (part.partInfo.cost + part.GetModuleCosts(part.partInfo.cost)).ToString("N2", CultureInfo.InvariantCulture) + // x,xxx.xx
        " </b>";

      // Rebuilding the global part stats string :
      string basicInfo = "<color=#acfffc>";

      // Basic part stats in blue (note : added dry mass)
      float dryMass = part.mass + part.GetModuleMass(part.mass, ModifierStagingSituation.CURRENT);
      basicInfo += (part.GetResourceMass() < Single.Epsilon) ?
        "<b>Mass: </b>" + dryMass.ToString("0.###") + "t\n" :
        "<b>Mass: </b>" + (dryMass + part.GetResourceMass()).ToString("0.###") + "t (<b>Dry mass: </b>" + dryMass.ToString("0.###") + "t)\n";
      basicInfo +=
        "<b>Tolerance: </b>" + part.crashTolerance.ToString("G") + " m/s impact\n" +
        "<b>Tolerance: </b>" + part.gTolerance.ToString("G") + " Gees, " + part.maxPressure.ToString("G") + " kPA Pressure\n" +
        "<b>Max. Temp. Int / Skin: </b>" + part.maxTemp.ToString("G") + " / " + part.skinMaxTemp.ToString("G") + " K\n";

      if (part.CrewCapacity > 0) { basicInfo += "<b>Crew capacity: </b>" + part.CrewCapacity + "\n"; }
      basicInfo += "</color>";

      // Crossfeed info in red
      ModuleToggleCrossfeed mtc = part.Modules.GetModule<ModuleToggleCrossfeed>();
      if (mtc != null)
      {
        basicInfo += "<color=#f3a413>";
        if (mtc.toggleEditor && mtc.toggleFlight) { basicInfo += "Crossfeed toggles in Editor and Flight\n"; }
        else if (mtc.toggleEditor) { basicInfo += "Crossfeed toggles in Editor\n"; }
        else if (mtc.toggleFlight) { basicInfo += "Crossfeed toggles in Flight\n"; }
        basicInfo += mtc.crossfeedStatus ? "Default On\n" : "Default Off\n";
        basicInfo += "</color>";
      }
      else if (!part.fuelCrossFeed)
      {
        basicInfo += "<color=#f3a413>No fuel crossfeed</color>\n";
      }

      // Module/resource info is in light green
      basicInfo += "\n<color=#b4d455>";

      // Info from modules (note : revamped engine test) :
      string moduleInfo = "";
      if (part.Modules.GetModule<MultiModeEngine>() != null)
      {
        moduleInfo += "<b>Multi-mode engine:</b>\n";
      }
      foreach (PartModule pm in part.Modules)
      {
        if (pm is ModuleEngines)
        {
          ModuleEngines me = (ModuleEngines)pm;
          moduleInfo += "<b>";
          moduleInfo += (me.engineType != EngineType.Generic) ? me.engineType.ToString() + " engine" : "Engine";
          moduleInfo += (part.Modules.GetModule<MultiModeEngine>() != null) ? " (" + me.engineID + "):</b>\n" : ":</b>\n";

          if (me.engineType == EngineType.Turbine)
          {
            moduleInfo += "<b>Stationary Thrust:</b> " + me.maxThrust.ToString("F1") + "kN\n";
          }
          else
          {
            float ispVAC = me.atmosphereCurve.Evaluate(0.0f);
            float ispASL = me.atmosphereCurve.Evaluate(1.0f);
            float thrustASL = me.maxThrust * (ispASL / ispVAC);
            moduleInfo +=
              "<b>Thrust:</b> " + me.maxThrust.ToString("F1") + " kN, <b>ISP:</b> " + ispVAC.ToString("F0") + " (Vac.)\n" +
              "<b>Thrust:</b> " + thrustASL.ToString("F1") + " kN, <b>ISP:</b> " + ispASL.ToString("F0") + " (ASL)\n";
          }
        }
        else
        {
          IModuleInfo info = pm as IModuleInfo;
          if (info != null)
          {
            moduleInfo += info.GetPrimaryField();
          }
        }
      }

      if (moduleInfo != "")
      {
        basicInfo += moduleInfo + "\n";
      }

      // Resource list in green (note : GetInfo() doesn't have the same format as stock)
      foreach (PartResource pr in part.Resources)
      {
        basicInfo += "<b>" + pr.info.title + ": </b>" + pr.maxAmount.ToString("F1") + "\n";
      }

      basicInfo += "</color>";

      // Update the tooltip with the new text :
      tooltip.textInfoBasic.text = basicInfo;

      // Update every module widget :
      int i = 0;
      foreach (PartListTooltipWidget widget in tooltip.panelExtended.GetComponentsInChildren<PartListTooltipWidget>())
      {
        // Resource widgets are named "PartListTooltipExtendedResourceInfo(Clone)"
        // Module widgets are named "PartListTooltipExtendedPartInfo(Clone)"
        if (widget.name == "PartListTooltipExtendedPartInfo(Clone)" && part.Modules.Count >= i)
        {
          while (true)
          {
            // Stock doesn't create a widget for modules that return an empty GetInfo(), but seems to be
            // checking against an already parsed string where control characters are removed
            if (RemoveControlCharacters(part.Modules.GetModule(i).GetInfo()).Equals(""))
            {
              i++;
            }
            // The module has a widget text, we update it with some extra info on the applied upgrades
            // Note : could have used GetUpgradeInfo(), but added a bit of extra formatting to make things nicer
            else
            {
              string widgetTitle;
              string widgetText;

              if (part.Modules.GetModule(i) is PartStatsUpgradeModule)
              {
                widgetTitle = "Part upgrade";
                widgetText = "";

                if (!(part.Modules.GetModule(i).upgradesApplied.Count() > 0))
                {
                  widgetText += "No upgrade researched yet";
                }
                else
                {
                  if (part.Modules.GetModule(i).showUpgradesInModuleInfo)
                  {
                    widgetText += "<b>Current upgrades:\n</b>";
                    foreach (string upgrade in part.Modules.GetModule(i).upgradesApplied)
                    {
                      widgetText += "<b>" + PartUpgradeManager.Handler.GetUpgrade(upgrade).title + "</b>\n";
                    }

                    widgetText += "\n<color=#99ff00ff><b>Modified stats :</b></color>\n";
                  }

                  PartStatsUpgradeModule psum = (PartStatsUpgradeModule)part.Modules.GetModule(i);
                  if (psum.GetModuleCost(part.partInfo.cost, ModifierStagingSituation.CURRENT) > float.Epsilon || psum.GetModuleCost(part.partInfo.cost, ModifierStagingSituation.CURRENT) < -float.Epsilon)
                  {
                    widgetText += "<b>Cost modifier : </b>" + psum.GetModuleCost(part.partInfo.cost, ModifierStagingSituation.CURRENT).ToString("+ 0;- #") + " <sprite=2 tint=1>\n";
                  }
                  if (psum.GetModuleMass(part.mass, ModifierStagingSituation.CURRENT) > float.Epsilon || psum.GetModuleMass(part.mass, ModifierStagingSituation.CURRENT) < -float.Epsilon)
                  {
                    widgetText += "<b>Mass modifier : </b>" + psum.GetModuleMass(part.mass, ModifierStagingSituation.CURRENT).ToString("+ 0.###;- #.###") + " t\n";
                  }
                  foreach (ConfigNode.Value value in psum.upgradeNode.values)
                  {
                    if (value.name != "cost" && value.name != "costAdd" && value.name != "mass" && value.name != "massAdd")
                    {
                      widgetText += "<b>New " + value.name + ": </b>" + value.value + " \n";
                    }
                  }
                }
              }
              else
              {
                widgetTitle = part.Modules.GetModule(i).GUIName;
                widgetText = part.Modules.GetModule(i).GetInfo();

                if (part.Modules.GetModule(i).showUpgradesInModuleInfo)
                {
                  if (part.Modules.GetModule(i).upgradesApplied.Count() > 0)
                  {
                    widgetText += "\n<color=#99ff00ff><b>Upgrades :</b></color>\n";

                    foreach (string upgrade in part.Modules.GetModule(i).upgradesApplied)
                    {
                      widgetText += "<b>- " + PartUpgradeManager.Handler.GetUpgrade(upgrade).title + ":</b>\n";
                      ConfigNode cn = part.Modules.GetModule(i).upgrades.Find(p => p.GetValue("name__") == upgrade);
                      widgetText += cn.GetValue("description__") + "\n";
                    }
                  }
                  // This is the "Upgrades available at <techtree nodes>..." text, seems totally bugged in 1.2.2 :
                  // it usually show the already applied upgrades but sometimes also the "outdated" upgrade
                  // widgetText += "\n" + part.Modules.GetModule(i).PrintUpgrades();
                }
              }

              widget.Setup(widgetTitle, widgetText);

              i++;
              break;
            }
          }
        }
      }
    }

    public string RemoveControlCharacters(string input)
    {
      return
          input.Where(character => !char.IsControl(character))
          .Aggregate(new StringBuilder(), (builder, character) => builder.Append(character))
          .ToString();
    }

  }
}
