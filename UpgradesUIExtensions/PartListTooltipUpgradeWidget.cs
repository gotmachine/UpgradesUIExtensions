using KSP.UI.Screens.Editor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

namespace UpgradesUIExtensions
{
  public class PartUpgradeUI : MonoBehaviour
  {
    public string upgradeName;
    public bool isUnlocked;
    public bool isEnabled;

    public PartUpgradeUI(string name, bool unlocked, bool enabled)
    {
      this.upgradeName = name;
      this.isUnlocked = unlocked;
      this.isEnabled = enabled;
    }
  }
}
