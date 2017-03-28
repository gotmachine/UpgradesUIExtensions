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
  class PartListTooltipUpgradeWidget : PartListTooltipWidget
  {
    Toggle toggle;
    public void Start()
    {
      // Add "toggle" component
      toggle = this.gameObject.AddComponent<Toggle>();
      toggle.onValueChanged.AddListener(onToggle);
    }

    private void onToggle(bool isOn)
    {
      if (isOn)
      {
        this.gameObject.GetComponent<CanvasRenderer>().SetColor(Color.blue);
      }
      else
      {
        this.gameObject.GetComponent<CanvasRenderer>().SetColor(Color.red);
      }
    }
  }
}
