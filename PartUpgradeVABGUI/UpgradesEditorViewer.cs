using KellermanSoftware.CompareNetObjects;
using NUnit;
using KSP.UI.Screens.Editor;
using System;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace PartUpgradeVABGUI
{
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public class pfiew : MonoBehaviour
  {

    PartListTooltipMasterController masterToolTip;

    public void Start()
    {
      //PartListTooltipController.
    }

    public void Update()
    {

      Part firstpart = null;

      if (EditorLogic.fetch.getSortedShipList().Count > 0)
      {
        firstpart = EditorLogic.fetch.getSortedShipList().First();
        ModuleEngines m = firstpart.Modules.GetModule<ModuleEngines>();
        if (m != null)
        {
          float ispASL = m.atmosphereCurve.Evaluate(1);
          float ispVAC = m.atmosphereCurve.Evaluate(0);
          float maxThrustVAC = m.maxThrust;
          float maxThrustASL = maxThrustVAC * (ispASL / ispVAC);
          float minThurst = m.minThrust;
          float propFlow = (maxThrustVAC / (ispVAC * 9.82f)) * 100;
          float flameout = m.ignitionThreshold;

          string moduleInfo =
            "<b>Max. Thrust (ASL): </b>" + maxThrustASL.ToString("0.###") + " kN\n" +
            "<b>Max. Thrust (Vac.): </b>" + maxThrustVAC.ToString("0.###") + " kN\n" +
            "<b>Min. Thrust: </b>" + minThurst.ToString("0.###") + " kN\n" +
            "<b>Engine Isp: </b>" + ispASL.ToString("F0") + " (ASL) - " + ispVAC.ToString("F0") + " (Vac.)\n" +
            "\n" +
            "<b><color=#99ff00ff>Propellants:</color></b>\n";

          foreach (Propellant p in m.propellants)
          {
            moduleInfo += "- <b>" + p.name + "</b>: " + (p.ratio * propFlow).ToString("0.###") + "/sec. Max.\n";
          }

          moduleInfo += "<b>Flameout under: </b>" + flameout.ToString("P");

        }
      }



      if (PartListTooltipMasterController.Instance != null)
      {
        masterToolTip = PartListTooltipMasterController.Instance;
        if (masterToolTip.currentTooltip != null)
        {
          PartListTooltip tooltip = masterToolTip.currentTooltip;

          var field = typeof(PartListTooltip).GetField("partRef", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
          Part partRef = (Part)field.GetValue(tooltip);



          //AvailablePart ap = partRef.partInfo;
          //UnityEngine.Object obj = UnityEngine.Object.Instantiate(ap.partPrefab);
          //if (obj)
          //{
          //  partRef = (Part)obj;
          //  partRef.gameObject.SetActive(true);
          //  partRef.gameObject.name = ap.name;
          //  partRef.partInfo = ap;
          //}

      if (firstpart != null )
          {
            if (firstpart.partName == partRef.partName)
            {

              // List<MemberComparison> changes = ReflectiveCompare(firstpart, partRef);

            }
          }


          PartModule statsUpgrade = partRef.Modules.GetModule<PartStatsUpgradeModule>();

          if (statsUpgrade != null)
          {
            // statsUpgrade.ApplyUpgrades(PartModule.StartState.PreLaunch);
            // statsUpgrade.FindUpgrades(true);
          }

          string basicInfo =
            "Total mass: " + (partRef.mass + partRef.resourceMass).ToString("G") + "t, Dry mass: " + partRef.mass.ToString("G") + "t\n" +
            "Tolerance: " + partRef.crashTolerance.ToString("G") + " m/s impact\n" +
            "Tolerance: " + partRef.gTolerance.ToString("G") + " Gees, " + partRef.maxPressure.ToString("G") + " kPA Pressure\n" +
            "Max. Temp. Int / Skin : " + partRef.maxTemp.ToString("G") + " / " + partRef.skinMaxTemp.ToString("G") + " K\n";

          if (partRef.CrewCapacity > 0) { basicInfo += "Crew capacity: " + partRef.CrewCapacity + "\n"; }

          tooltip.textInfoBasic.text = basicInfo;

          // Found :
          // PartListTooltipExtendedPartInfo
          // PartListTooltipExtendedResourceInfo

          int i = 0;
          foreach (Component c in tooltip.panelExtended.GetComponentsInChildren<PartListTooltipWidget>())
          {
            // Debug.Log("Found a widget of type " + c.name);
            if (c.name == "PartListTooltipExtendedPartInfo(Clone)")
            {
              PartListTooltipWidget widget = (PartListTooltipWidget)c;
              while(true)
              {
                if (partRef.Modules.Count < i)
                {
                  break;
                }
                else if (partRef.Modules.GetModule(i).GetInfo().Equals(""))
                {
                  i++;
                }
                else
                {



                  string moduleInfo;
                  switch (partRef.Modules.GetModule(i).GUIName)
                  {
                    case "Engines":
                      ModuleEngines m = (ModuleEngines) partRef.Modules.GetModule(i);

                      // Awaken(m);



                      m.OnStart(PartModule.StartState.Editor);

                      Debug.Log(m.GUIName + "FindUpgrades=" + m.FindUpgrades(false));

                      Debug.Log(m.GUIName + "ApplyUpgrades=" + m.ApplyUpgrades(PartModule.StartState.Editor));

                      //m.LoadUpgrades(m.upgrades.First());

                      //m.ApplyUpgradeNode(m.upgradesApplied, m.upgrades.First(), true);

                      // Debug.Log(m.GUIName + " ApplyUpgrades result :" + m.ApplyUpgrades(PartModule.StartState.None) + ", appliedUpgrades : " + m.upgradesApplied.Count());


                      float ispASL = m.atmosphereCurve.Evaluate(1);
                      float ispVAC = m.atmosphereCurve.Evaluate(0);
                      float maxThrustVAC = m.maxThrust;
                      float maxThrustASL = maxThrustVAC * (ispASL / ispVAC);
                      float minThurst = m.minThrust;
                      float propFlow = (maxThrustVAC / (ispVAC * 9.82f)) * 100;
                      float flameout = m.ignitionThreshold;

                      moduleInfo =
                        "<b>Max. Thrust (ASL): </b>" + maxThrustASL.ToString("0.###") + " kN\n" +
                        "<b>Max. Thrust (Vac.): </b>" + maxThrustVAC.ToString("0.###") + " kN\n" +
                        "<b>Min. Thrust: </b>" + minThurst.ToString("0.###") + " kN\n" +
                        "<b>Engine Isp: </b>" + ispASL.ToString("F0") + " (ASL) - " + ispVAC.ToString("F0") + " (Vac.)\n" +
                        "\n" +
                        "<b><color=#99ff00ff>Propellants:</color></b>\n";

                      foreach (Propellant p in m.propellants)
                      {
                        moduleInfo += "- <b>" + p.name + "</b>: " + (p.ratio * propFlow).ToString("0.###") + "/sec. Max.\n";
                      }

                      moduleInfo += "<b>Flameout under: </b>" + flameout.ToString("P");

                      //Debug.Log("NEW INFO : \n" + moduleInfo);
                      //Debug.Log("OLD INFO : \n" + m.GetInfo());


                      

                      break;

                    default:
                      moduleInfo = partRef.Modules.GetModule(i).GetInfo();
                      break;
                  }
                  widget.Setup(partRef.Modules.GetModule(i).GUIName, moduleInfo);
                  i++;
                  break;
                }
              }
            }

            //Debug.Log("Found a widget in tooltip.extInfoListContainer : " + c.name); // Faut trouver le parent, pour qu'on puisse réinjecter des PartListTooltipWidget neufs
            //try
            //{
            //  PartListTooltipWidget widget = (PartListTooltipWidget)c;
            //  Debug.Log("textName : " + widget.textName.text);
            //  Debug.Log("textInfo : " + widget.textInfo.text);
            //}
            //catch (Exception)
            //{
            //  Debug.Log("Cast to widget failed");
            //}

          }






          //foreach (Component c in tooltip.GetComponentsInChildren<PartListTooltipWidget>())
          //{
          //  //  Debug.Log("Found a widget in tooltip.extInfoListContainer : " + c.);
          //}

          //foreach (Component c in tooltip.extInfoListContainer.GetComponentsInChildren<PartListTooltipWidget>())
          //{
          //  Debug.Log("Found a widget in tooltip.extInfoListContainer : " + c.);
          //}


          //Debug.Log("PART IS : " + partRef.partInfo.title);

          // Debug.Log(tooltip.textInfoBasic.text);
          // textInfoBasic has the following info :
          // Mass: x.xt
          // Tolerance: x.x m/s Impact
          // Tolerance: x Gees, x.x kPA Pressure
          // Max. Temp. Int/Skin : xxxx/xxxx K
          // Crew capacity: x
          // (opt on decouplers) Crossfeed toggles in Editor and Flight.
          // (opt on decouplers) Default Off.
          // Then : 
          // - "No fuel crossfeed"
          // - landing gear : Fixed, Unpowered, Steerable, No brakes
          // - retractable landing gear : Retractable
          // - river wheel : Motorized
          // - spearator/decoupler : Ejection Force 
          // - Ressources

        }
      }
    }

    public static bool Awaken(PartModule module)
    {
      // thanks to Mu and Kine for help with this bit of Dark Magic. 
      // KINEMORTOBESTMORTOLOLOLOL
      if (module == null)
        return false;
      object[] paramList = new object[] { };
      MethodInfo awakeMethod = typeof(PartModule).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

      if (awakeMethod == null)
        return false;

      awakeMethod.Invoke(module, paramList);
      return true;
    }

    public struct MemberComparison
    {
      public readonly System.Reflection.MemberInfo Member; //Which member this Comparison compares
      public readonly object Value1, Value2;//The values of each object's respective member
      public readonly Exception Value1Exception, Value2Exception;
      public MemberComparison(System.Reflection.MemberInfo member, object value1, object value2, Exception value1Exception = null, Exception value2Exception = null)
      {
        Member = member;
        Value1 = value1;
        Value2 = value2;
        Value1Exception = value1Exception;
        Value2Exception = value2Exception;
      }

      public override string ToString()
      {
        if (Value1Exception != null && Value2Exception != null)
        {
          if (Value1Exception.GetType().Equals(Value2Exception.GetType()))
          {
            List<string> lt = new List<string>();
            foreach (MemberComparison m in ReflectiveCompare(Value1Exception, Value2Exception))
            {
              lt.Add("\n" + m.ToString());
            }
            return Member.Name + ": Exception in both, same exception type of type " + Value1Exception.GetType().Name + ", message in first exception: " + Value1Exception.Message + ", message in second exception: " + Value2Exception.Message + ", differences in type value: " + lt;
          }
          else if (!Value1Exception.GetType().Equals(Value2Exception.GetType()))
          {
            return Member.Name + ": Exception in both, different exception type: " + Value1Exception.GetType().Name + " : " + Value2Exception.GetType().Name + ", message in first exception: " + Value1Exception.Message + ", message in second exception: " + Value2Exception.Message;
          }
        }
        else if (Value1Exception != null && Value2Exception == null)
        {
          return Member.Name + ": " + Value2.ToString() + " Exception in first of type " + Value1Exception.GetType().Name + ", message is: " + Value1Exception.Message;
        }
        else if (Value1Exception == null && Value2Exception != null)
        {
          return Member.Name + ": " + Value1.ToString() + " Exception in second of type " + Value2Exception.GetType().Name + ", message is: " + Value2Exception.Message;
        }
        return Member.Name + ": " + Value1.ToString() + (Value1.Equals(Value2) ? " == " : " != ") + Value2.ToString();
      }
    }


    public static bool isCollection(object obj)
    {
      return obj.GetType().GetInterfaces()
  .Any(iface => (iface.GetType() == typeof(ICollection) || iface.GetType() == typeof(IEnumerable) || iface.GetType() == typeof(IList)) || (iface.IsGenericTypeDefinition && (iface.GetGenericTypeDefinition() == typeof(ICollection<>) || iface.GetGenericTypeDefinition() == typeof(IEnumerable<>) || iface.GetGenericTypeDefinition() == typeof(IList<>))));
    }

    //This method can be used to get a list of MemberComparison values that represent the fields and/or properties that differ between the two objects.
    public static List<MemberComparison> ReflectiveCompare<T>(T x, T y)
    {
      List<MemberComparison> list = new List<MemberComparison>();//The list to be returned

      var memb = typeof(T).GetMembers();
      foreach (System.Reflection.MemberInfo m in memb)
        //Only look at fields and properties.
        //This could be changed to include methods, but you'd have to get values to pass to the methods you want to compare
        if (m.MemberType == System.Reflection.MemberTypes.Field)
        {
          System.Reflection.FieldInfo field = (System.Reflection.FieldInfo)m;
          Exception excep1 = null;
          Exception excep2 = null;
          object xValue = null;
          object yValue = null;
          try
          {
            xValue = field.GetValue(x);
          }
          catch (Exception e)
          {
            excep1 = e;
          }
          try
          {
            yValue = field.GetValue(y);
          }
          catch (Exception e)
          {
            excep2 = e;
          }
          if ((excep1 != null && excep2 == null) || (excep1 == null && excep2 != null)) { list.Add(new MemberComparison(field, yValue, xValue, excep1, excep2)); }
          else if (excep1 != null && excep2 != null && !excep1.GetType().Equals(excep2.GetType())) { list.Add(new MemberComparison(field, yValue, xValue, excep1, excep2)); }
          else if (excep1 != null && excep2 != null && excep1.GetType().Equals(excep2.GetType()) && ReflectiveCompare(excep1, excep2).Count > 0) { list.Add(new MemberComparison(field, yValue, xValue, excep1, excep2)); }
          else if ((xValue == null && yValue == null)) { continue; }
          else if (xValue == null || yValue == null) list.Add(new MemberComparison(field, yValue, xValue));
          else if (!xValue.Equals(yValue) && ((!isCollection(xValue) && !isCollection(yValue)) || (isCollection(xValue) && !isCollection(yValue)) || (!isCollection(xValue) && isCollection(yValue)) || (isCollection(xValue) && isCollection(yValue) && ReflectiveCompare(xValue, yValue).Count > 0)))//Add a new comparison to the list if the value of the member defined on 'x' isn't equal to the value of the member defined on 'y'.
            list.Add(new MemberComparison(field, yValue, xValue));
        }
        else if (m.MemberType == System.Reflection.MemberTypes.Property)
        {
          var prop = (System.Reflection.PropertyInfo)m;
          if (prop.CanRead && !(prop.GetGetMethod() == null || prop.GetGetMethod().GetParameters() == null) && prop.GetGetMethod().GetParameters().Length == 0)
          {
            Exception excep1 = null;
            Exception excep2 = null;
            object xValue = null;
            object yValue = null;
            try
            {
              xValue = prop.GetValue(x, null);
            }
            catch (Exception e)
            {
              excep1 = e;
            }
            try
            {
              yValue = prop.GetValue(y, null);
            }
            catch (Exception e)
            {
              excep2 = e;
            }
            if ((excep1 != null && excep2 == null) || (excep1 == null && excep2 != null)) { list.Add(new MemberComparison(prop, yValue, xValue, excep1, excep2)); }
            else if (excep1 != null && excep2 != null && !excep1.GetType().Equals(excep2.GetType())) { list.Add(new MemberComparison(prop, yValue, xValue, excep1, excep2)); }
            else if (excep1 != null && excep2 != null && excep1.GetType().Equals(excep2.GetType()) && ReflectiveCompare(excep1, excep2).Count > 0) { list.Add(new MemberComparison(prop, yValue, xValue, excep1, excep2)); }
            else if ((xValue == null && yValue == null)) { continue; }
            else if (xValue == null || yValue == null) list.Add(new MemberComparison(prop, yValue, xValue));
            else if (!xValue.Equals(yValue) && ((!isCollection(xValue) && !isCollection(yValue)) || (isCollection(xValue) && !isCollection(yValue)) || (!isCollection(xValue) && isCollection(yValue)) || (isCollection(xValue) && isCollection(yValue) && ReflectiveCompare(xValue, yValue).Count > 0)))// || (isCollection(xValue) && isCollection(yValue)  && ((IEnumerable<T>)xValue).OrderBy(i => i).SequenceEqual(xValue.OrderBy(i => i))) )))
              list.Add(new MemberComparison(prop, xValue, yValue));
          }
          else//Ignore properties that aren't readable or are indexers
            continue;
        }
      return list;
    }
  }

  public static class ReflectionHelper
  {
    private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
    {
      PropertyInfo propInfo = null;
      do
      {
        propInfo = type.GetProperty(propertyName,
               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        type = type.BaseType;
      }
      while (propInfo == null && type != null);
      return propInfo;
    }

    public static object GetPropertyValue(this object obj, string propertyName)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      Type objType = obj.GetType();
      PropertyInfo propInfo = GetPropertyInfo(objType, propertyName);
      if (propInfo == null)
        throw new ArgumentOutOfRangeException("propertyName",
          string.Format("Couldn't find property {0} in type {1}", propertyName, objType.FullName));
      return propInfo.GetValue(obj, null);
    }

    public static void SetPropertyValue(this object obj, string propertyName, object val)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      Type objType = obj.GetType();
      PropertyInfo propInfo = GetPropertyInfo(objType, propertyName);
      if (propInfo == null)
        throw new ArgumentOutOfRangeException("propertyName",
          string.Format("Couldn't find property {0} in type {1}", propertyName, objType.FullName));
      propInfo.SetValue(obj, val, null);
    }
  }
}
