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
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UpgradesUIExtensions
{
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public class PatchEditorTooltip : MonoBehaviour
  {
    List<UpgradePrefab> upgradePrefabs = new List<UpgradePrefab>();
    PartListTooltip tooltip = null;
    PartListTooltipWidget toggleWidget = null;
    bool updateTooltip = false;
    bool toggleWidgets = false;

    public void OnDestroy()
    {
      PartUpgradeHandler.AllEnabled = true;
    }

    public void Start()
    {
      // Another messy fix to attempt to replicate the exact state of the game
      // when OnLoad() is normally called.
      GameScenes currentScene = HighLogic.LoadedScene;
      HighLogic.LoadedScene = GameScenes.LOADING;

      foreach (AvailablePart ap in PartLoader.LoadedPartsList)
      {
        // Special parts like EVAkerbal or flag aren't needed :
        if (ap.partUrl == "" || ap.partConfig == null) continue;
        
        // Part newPart = (Part)obj;
        Part upgradedPart = Instantiate(ap.partPrefab);
        if (upgradedPart != null)
        {
          upgradedPart.gameObject.name = ap.name;
          upgradedPart.partInfo = ap;
            
          // Temporally enable the part to be able to call ApplyUpgrades on all modules
          // so upgrades nodes are checked and applyied to the part/module properties.
          // We try to call modules OnLoad to replicate the exact state of part prefabs.
          upgradedPart.gameObject.SetActive(true);
          LoadModulesUpgrades(upgradedPart, ap);

          // Disable the gameobject so the part isn't rendered and can't be interacted with
          upgradedPart.gameObject.SetActive(false);

          // Add the part to our list
          upgradePrefabs.Add(new UpgradePrefab(upgradedPart));
        }
      }
      HighLogic.LoadedScene = currentScene;

      // We now want to control ourself if the upgrades are available :
      PartUpgradeHandler.AllEnabled = false;
    }

    public void Update()
    {
      // Do nothing if there is no PartListTooltip on screen
      if (PartListTooltipMasterController.Instance.currentTooltip == null) { return; }

      // Do nothing if the tooltip hasn't changed since last update and we have nothing to update in the tooltip
      if (tooltip == PartListTooltipMasterController.Instance.currentTooltip && !updateTooltip && !toggleWidgets) { return; }

      // Get the upgrade prefab for this tooltip :
      UpgradePrefab upgradePrefab = GetTooltipInstance();
      // ReferenceEquals is a workaround because unity does a silly overload of the == operator
      if (ReferenceEquals(upgradePrefab, null)) { return; }

      // Toggle widget visibility in response to user click on toggle widget, and do nothing else
      if (toggleWidgets)
      {
        toggleWidgets = false;
        ToggleWidgetsVisibility(upgradePrefab);
        return;
      }

      // Update everything needed in response to the player enabling/disabling an upgrade
      if (updateTooltip)
      {
        updateTooltip = false;
        // Update the upgrade prefab in response to player toggle
        UpgradeWidgetComponent[] upgrades = tooltip.panelExtended.gameObject.GetComponentsInChildren<UpgradeWidgetComponent>();
        UpgradeWidgetComponent toggledWidget = upgrades.First(p => p.isUpdated);
        upgradePrefab.UpdateUpgradesState(toggledWidget);

        // Destroy upgrade widgets
        for (int k = 0; k < upgrades.Count(); k++)
        {
          Destroy(upgrades[k].gameObject);
        }
      }
      // If this is a new tooltip, we destroy the toggle widget if it exists
      else if (toggleWidget != null)
      {
        Destroy(toggleWidget.gameObject);
        toggleWidget = null;
      }

      // Update the PartUpgradeHandler with enabled upgrades for this part and ApplyUpgrades() on all modules
      ApplyUpgrades(upgradePrefab);

      // Rebuilding the tooltip cost string :
      tooltip.textCost.text = GetPartCost(upgradePrefab.part);

      // Update the part basic info with the new text :
      tooltip.textInfoBasic.text = GetPartInfo(upgradePrefab.part);

      // Update the module widgets text :
      UpdateModuleWidgetInfo(upgradePrefab);

      // Create the widget list toggle widget :
      if (toggleWidget == null && upgradePrefab.upgrades.Count > 0)
      {
        toggleWidget = CreateListToggleWidget(tooltip.panelExtended.gameObject.GetChild("Content"), upgradePrefab);
      }

      if (toggleWidget != null)
      {
        // Instantiate and setup the upgrade widgets
        CreateUpgradeWidgets(upgradePrefab, tooltip.panelExtended.gameObject.GetChild("Content"));

        // Toggle widgets visibility
        ToggleWidgetsVisibility(upgradePrefab);
      }
    }

    private string GetPartInfo(Part part)
    {
      // Rebuilding the global part stats string :
      string basicInfo = "<color=#acfffc>";

      // Basic part stats in blue (note : added dry mass)
      float dryMass = part.mass + part.GetModuleMass(part.mass, ModifierStagingSituation.CURRENT);
      basicInfo += (part.GetResourceMass() < Single.Epsilon) ?
        "<b>Mass: </b>" + dryMass.ToString("0.###") + "t\n" :
        "<b>Mass: </b>" + (dryMass + part.GetResourceMass()).ToString("0.###") + "t (<b>Dry mass: </b>" + dryMass.ToString("0.###") + "t)\n";
      basicInfo +=
        "<b>Tolerance: </b>" + part.crashTolerance.ToString("G") + " m/s impact\n" +
        "<b>Tolerance: </b>" + part.gTolerance.ToString("G") + " G, " + part.maxPressure.ToString("G") + " kPA Pressure\n" +
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
      return basicInfo;
    }

    private string GetPartCost(Part part)
    {
      string cost = "<b>Cost: </b><sprite=2 tint=1>  <b>" +
        (part.partInfo.cost + part.GetModuleCosts(part.partInfo.cost)).ToString("N2", CultureInfo.InvariantCulture) + // x,xxx.xx
        " </b>";
      return cost;
    }

    private void UpdateModuleWidgetInfo(UpgradePrefab upgradePrefab)
    {
      // Update every module widget :
      int i = 0;
      List<PartModule> modules = upgradePrefab.part.Modules.GetModules<PartModule>().OrderBy(p => p.GUIName).ToList();
      foreach (PartListTooltipWidget widget in tooltip.panelExtended.GetComponentsInChildren<PartListTooltipWidget>(true))
      {
        // Resource widgets are named "PartListTooltipExtendedResourceInfo(Clone)"
        // Module widgets are named "PartListTooltipExtendedPartInfo(Clone)"
        if (widget.name == "PartListTooltipExtendedPartInfo(Clone)" && modules.Count >= i)
        {
          while (true)
          {
            string widgetTitle;
            string widgetText;

            // Get the upgrades-updated module text info from our special prefab
            try
            {
              widgetTitle = modules[i].GUIName;
              widgetText = modules[i].GetInfo();
            }
            catch (Exception)
            {
              widgetTitle = widget.textName.text;
              widgetText = widget.textInfo.text;
              Debug.LogWarning("[UpgradesUIextensions] Could not retrieve module text for module " + modules[i].GUIName);
            }

            // Stock doesn't create a widget for modules that return an empty GetInfo(), but seems to be
            // checking against an already parsed string where control characters are removed
            if (RemoveControlCharacters(widgetText).Equals(""))
            {
              i++;
            }
            // The module has a widget text, we update it with some extra info on the applied upgrades
            else
            {
              // Special formatting for PartStatsUpgradeModule
              if (modules[i] is PartStatsUpgradeModule)
              {
                widgetTitle = "Part stats upgrade";
                widgetText = "";

                if (!(modules[i].upgradesApplied.Count() > 0))
                {
                  widgetText += "No stats modifications in current upgrades";
                }
                else
                {
                  if (modules[i].showUpgradesInModuleInfo)
                  {
                    widgetText += "<b>Current upgrades:\n</b>";
                    foreach (string upgrade in modules[i].upgradesApplied)
                    {
                      widgetText += "<b>" + PartUpgradeManager.Handler.GetUpgrade(upgrade).title + "</b>\n";
                    }

                    widgetText += "\n<color=#99ff00ff><b>Modified stats :</b></color>\n";
                  }

                  PartStatsUpgradeModule psum = (PartStatsUpgradeModule)modules[i];
                  if (psum.GetModuleCost(upgradePrefab.part.partInfo.cost, ModifierStagingSituation.CURRENT) > float.Epsilon || psum.GetModuleCost(upgradePrefab.part.partInfo.cost, ModifierStagingSituation.CURRENT) < -float.Epsilon)
                  {
                    widgetText += "<b>Cost modifier : </b>" + psum.GetModuleCost(upgradePrefab.part.partInfo.cost, ModifierStagingSituation.CURRENT).ToString("+ 0;- #") + " <sprite=2 tint=1>\n";
                  }
                  if (psum.GetModuleMass(upgradePrefab.part.mass, ModifierStagingSituation.CURRENT) > float.Epsilon || psum.GetModuleMass(upgradePrefab.part.mass, ModifierStagingSituation.CURRENT) < -float.Epsilon)
                  {
                    widgetText += "<b>Mass modifier : </b>" + psum.GetModuleMass(upgradePrefab.part.mass, ModifierStagingSituation.CURRENT).ToString("+ 0.###;- #.###") + " t\n";
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
              // All other modules : append upgrade text if showUpgradesInModuleInfo is set to true in the module cfg
              else
              {
                if (modules[i].showUpgradesInModuleInfo)
                {
                  if (modules[i].upgradesApplied.Count() > 0)
                  {
                    widgetText += "\n<color=#99ff00ff><b>Upgrades :</b></color>\n";

                    foreach (string upgrade in modules[i].upgradesApplied)
                    {
                      widgetText += "<b>- " + PartUpgradeManager.Handler.GetUpgrade(upgrade).title + ":</b>\n";
                      ConfigNode cn = modules[i].upgrades.Find(p => p.GetValue("name__") == upgrade);
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

    private void LoadModulesUpgrades(Part p, AvailablePart ap)
    {
      int i = 0;
      ConfigNode[] moduleNodes = ap.partConfig.GetNodes("MODULE");
      foreach (PartModule pm in p.Modules)
      {
        pm.OnAwake();
        if (moduleNodes != null)
        {
          if (moduleNodes.Count() > i)
          {
            if (moduleNodes[i].GetValue("name") == pm.moduleName)
            {
              pm.OnLoad(moduleNodes[i]);
            }
          }
        }
        pm.ApplyUpgrades(PartModule.StartState.Editor);
        i++;
      }
    }

    private void ApplyUpgrades(UpgradePrefab prefab)
    {
      // Get the upgraded state of 
      Dictionary<PartModule, bool> moduleUpgraded = new Dictionary<PartModule, bool>();
      foreach (PartModule pm in prefab.part.Modules)
      {
        if (pm.upgradesApplied.Count > 0)
        {
          moduleUpgraded.Add(pm, true);
        }
        else
        {
          moduleUpgraded.Add(pm, false);
        }
      }

      // Enable upgrades in the handler to reflect the state of user selection for this part
      foreach (PartUpgrade pu in prefab.upgrades)
      {
        if (pu.upgradeState == PartUpgrade.UpgradeState.Disabled)
        {
          PartUpgradeManager.Handler.SetEnabled(pu.upgradeName, false);

        }
        else
        {
          PartUpgradeManager.Handler.SetEnabled(pu.upgradeName, true);
        }
      }

      // Apply upgrades on modules
      // Due to ApplyUpgrades() not doing anything when all upgrades are disabled
      // I need to do OnLoad from the config node to revert the module fields to their
      // default value in this case
      int i = 0;
      ConfigNode[] moduleNodes = prefab.part.partInfo.partConfig.GetNodes("MODULE");
      foreach (PartModule pm in prefab.part.Modules)
      {
        pm.ApplyUpgrades(PartModule.StartState.Editor);
        bool wasUpgraded = false;
        moduleUpgraded.TryGetValue(pm, out wasUpgraded);
        
        if (wasUpgraded && pm.upgradesApplied.Count == 0)
        {
          pm.OnAwake();
          if (moduleNodes != null)
          {
            if (moduleNodes.Count() > i)
            {
              if (moduleNodes[i].GetValue("name") == pm.moduleName)
              {
                pm.OnLoad(moduleNodes[i]);
              }
            }
          }
          // In case of a PartStatsUpgradeModule, things get ugly because I've got no public function
          // that reset the module mass/cost modifiers, so I manually set them to 0. For part fields 
          // modifiers I'm forced to parse the upgrade node to reset the corresponding part fields to 
          // their config value.
          // TODO : test this with a more complex multi-node stats module
          if (pm is PartStatsUpgradeModule)
          {
            PartStatsUpgradeModule psum = (PartStatsUpgradeModule)pm;
            psum.costOffset = 0;
            psum.massOffset = 0;
            foreach (ConfigNode.Value v in psum.upgradeNode.values)
            {
              if (v.name != "mass" && v.name != "cost" && v.name != "massAdd " && v.name != "costAdd")
              {
                try
                {
                  FieldInfo partField = prefab.part.GetType().GetField(v.name);
                  partField.SetValue(prefab.part, Convert.ChangeType(prefab.part.partInfo.partConfig.GetValue(v.name), partField.FieldType));
                }
                catch (Exception)
                {
                  Debug.LogError("[UpgradesUIextensions] Could not revert part field \"" + v.name + "\" to initial value");
                }
                
              }
            }
          }
          pm.ApplyUpgrades(PartModule.StartState.Editor);
        }
        i++;
      }
    }

    private UpgradePrefab GetTooltipInstance()
    {
      // Get the PartListTooltip
      tooltip = PartListTooltipMasterController.Instance.currentTooltip;

      // Find the currently shown part in the list of updated parts instances
      var field = typeof(PartListTooltip).GetField("partInfo", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
      AvailablePart partInfo = (AvailablePart)field.GetValue(tooltip);
      UpgradePrefab upgradePrefab = upgradePrefabs.Find(p => p.part.partInfo == partInfo);
      if (ReferenceEquals(upgradePrefab, null)) // Workaround because unity does a silly overload of the == operator
      {
        Debug.LogWarning("[UpgradesUIextensions] Part upgrade stats for \"" + partInfo.title + "\" not found, using default stats.");
      }
      return upgradePrefab;
    }

    private PartListTooltipWidget CreateListToggleWidget(GameObject container, UpgradePrefab prefab)
    {
      // Create new widget
      PartListTooltipWidget newWidget = Instantiate(tooltip.extInfoRscWidgePrefab);
      newWidget.gameObject.name = "Widget list toggle";
      // Add widget to container and set it as the first one
      newWidget.transform.SetParent(container.transform, false);
      newWidget.transform.SetAsFirstSibling();
      // Create a toggle
      Toggle toggle = newWidget.gameObject.AddComponent<Toggle>();
      toggle.isOn = true;
      toggle.enabled = true;
      toggle.onValueChanged.AddListener(onListToggle);
      // Initialize widget text then destroy info text object (we only need the title)
      newWidget.Setup("null", "null");
      Destroy(newWidget.gameObject.GetChild("ModuleInfoText"));
      newWidget.gameObject.SetActive(true);
      return newWidget;
    }

    private void onListToggle(bool isOn)
    {
      toggleWidgets = true;
    }

    private void ToggleWidgetsVisibility(UpgradePrefab prefab)
    {
      PartListTooltipWidget[] widgets = tooltip.panelExtended.GetComponentsInChildren<PartListTooltipWidget>(true);
      bool isModuleView = toggleWidget.GetComponent<Toggle>().isOn;
      foreach (PartListTooltipWidget widget in widgets)
      {
        if (widget.name == "PartListTooltipExtendedPartInfo(Clone)" || widget.name == "PartListTooltipExtendedResourceInfo(Clone)")
        {
          widget.gameObject.SetActive(isModuleView);
        }
        if (widget.name == "Upgrade tooltip widget")
        {
          widget.gameObject.SetActive(!isModuleView);
        }
        if (widget.name == "Widget list toggle")
        {
          if (isModuleView)
          {
            int disabledUpgrades = prefab.upgrades.Count(p => p.upgradeState == PartUpgrade.UpgradeState.Disabled);
            if (disabledUpgrades > 0)
            {
              widget.GetComponent<Image>().color = Color255(222, 193, 88); // pale yellow
              widget.textName.text = "Select upgrades (" + disabledUpgrades + " disabled)";
            }
            else
            {
              widget.GetComponent<Image>().color = Color255(149, 223, 102); // pale green
              widget.textName.text = "Select upgrades (all enabled)";
            }
          }
          else
          {
            widget.GetComponent<Image>().color = Color.white;
            widget.textName.text = "Show modules & resources";
          }
        }
      }
    }

    private void CreateUpgradeWidgets(UpgradePrefab prefab, GameObject container)
    {
      foreach (PartUpgrade pu in prefab.upgrades)
      {
        // Don't create widgets for untracked upgrades
        if (pu.isUntracked) { continue; }
        // Create new widget
        PartListTooltipWidget newWidget = Instantiate(tooltip.extInfoModuleWidgetPrefab);
        newWidget.gameObject.name = "Upgrade tooltip widget";
        // Add widget to list
        newWidget.transform.SetParent(container.transform, false);
        // Add "toggle" component
        if ((pu.upgradeState == PartUpgrade.UpgradeState.Enabled || pu.upgradeState == PartUpgrade.UpgradeState.Disabled))
        {
          Toggle toggle = newWidget.gameObject.AddComponent<Toggle>();
          toggle.isOn = (pu.upgradeState == PartUpgrade.UpgradeState.Enabled) ? true : false;
          toggle.enabled = true;
          toggle.onValueChanged.AddListener(onUpgradeToggle);
        }
        // Add the upgrade handler
        UpgradeWidgetComponent handler = newWidget.gameObject.AddComponent<UpgradeWidgetComponent>();
        handler.partName = prefab.part.partInfo.name;
        handler.upgrade = pu.upgradeName;
        handler.upgradeState = pu.upgradeState;
        handler.isUpdated = false;
        // Create text
        newWidget.Setup(pu.GetTitle(), pu.GetInfo(prefab));
        ToggleColors(handler, newWidget.GetComponent<Image>());
        // Everything is good
        newWidget.gameObject.SetActive(true);
      }
    }

    private void onUpgradeToggle(bool isOn)
    {
      
      UpgradeWidgetComponent upgradeComponent = EventSystem.current.currentSelectedGameObject.GetComponent<UpgradeWidgetComponent>();

      if (isOn)
      {
        upgradeComponent.upgradeState = PartUpgrade.UpgradeState.Enabled;
      }
      else
      {
        upgradeComponent.upgradeState = PartUpgrade.UpgradeState.Disabled;
      }
      upgradeComponent.isUpdated = true;
      updateTooltip = true;
    }

    private void ToggleColors(UpgradeWidgetComponent handler, Image imageComponent)
    {
      switch (handler.upgradeState)
      {
        case PartUpgrade.UpgradeState.Unresearched:
          imageComponent.color = Color255(89, 139, 122); // blue
          break;
        case PartUpgrade.UpgradeState.Disabled:
          imageComponent.color = Color255(222, 193, 88); // yellow
          break;
        case PartUpgrade.UpgradeState.Overriden:
          imageComponent.color = Color255(177, 115, 87); // orange
          break;
        case PartUpgrade.UpgradeState.Enabled:
          imageComponent.color = Color255(149, 223, 102); // green
          break;
      }
    }

    public string RemoveControlCharacters(string input)
    {
      return
          input.Where(character => !char.IsControl(character))
          .Aggregate(new StringBuilder(), (builder, character) => builder.Append(character))
          .ToString();
    }

    public static Color Color255(int r, int g, int b, int a = 255)
    {
      return new Color(r / 255.0F, g / 255.0F, b / 255.0F, a / 255.0F);
    }

  }
}
