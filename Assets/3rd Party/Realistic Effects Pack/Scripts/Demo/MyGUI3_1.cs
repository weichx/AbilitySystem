/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using System;
using UnityEngine;
using System.Collections;

public class MyGUI3_1 : MonoBehaviour
{
  public enum GuiStat { Ball, BallRotate, BallRotatex4, Bottom, Middle, MiddleWithoutRobot, Top, TopTarget }

  public int CurrentPrefabNomber = 0; 
  public float UpdateInterval = 0.5F;
  public Light DirLight;
  public GameObject Target;
  public GameObject TopPosition;
  public GameObject MiddlePosition; 
  public Vector3 defaultRobotPos;
  public GameObject BottomPosition;  
  public GameObject Plane1;
  public GameObject Plane2;
  public Material[] PlaneMaterials;
  public GuiStat[] GuiStats;
  public GameObject[] Prefabs;
 
  private float oldLightIntensity;
  private Color oldAmbientColor;
  private GameObject currentGo;
  private bool isDay, isHomingMove, isDefaultPlaneTexture;
  private int current;
  private Animator anim;
  private float prefabSpeed = 4;
  private EffectSettings effectSettings;
  private bool isReadyEffect;
  private Quaternion defaultRobotRotation;

  private float accum = 0; // FPS accumulated over the interval
  private int frames = 0; // Frames drawn over the interval
  private float timeleft; // Left time for current interval
  private float fps;

  private GUIStyle guiStyleHeader = new GUIStyle();

  void Start()
  {
    oldAmbientColor = RenderSettings.ambientLight;
    oldLightIntensity = DirLight.intensity;

    anim = Target.GetComponent<Animator>();
    guiStyleHeader.fontSize = 14;
    guiStyleHeader.normal.textColor = new Color(1,1,1);
    var prefabSett = Prefabs[current].GetComponent<EffectSettings>();
    if (prefabSett != null) prefabSpeed = prefabSett.MoveSpeed;
    current = CurrentPrefabNomber;
    InstanceCurrent(GuiStats[CurrentPrefabNomber]);
  }

  private void InstanceEffect(Vector3 pos)
  {
    currentGo = Instantiate(Prefabs[current], pos, Prefabs[current].transform.rotation) as GameObject;
    effectSettings = currentGo.GetComponent<EffectSettings>();
    effectSettings.Target = GetTargetObject(GuiStats[current]);
    if (isHomingMove) effectSettings.IsHomingMove = isHomingMove;
    prefabSpeed = effectSettings.MoveSpeed;
    effectSettings.EffectDeactivated+=effectSettings_EffectDeactivated;
    if (GuiStats[current]==GuiStat.Middle) {
      currentGo.transform.parent = GetTargetObject(GuiStat.Middle).transform;
      currentGo.transform.position = GetInstancePosition(GuiStat.Middle);
    }
    else currentGo.transform.parent = transform;
    effectSettings.CollisionEnter += (n, e) => { if(e.Hit.transform!=null) Debug.Log(e.Hit.transform.name); };
  }


  GameObject GetTargetObject(GuiStat stat)
  {
    switch (stat)
    {
      case GuiStat.Ball:
        {
          return Target;
        }
      case GuiStat.BallRotate:
        {
          return Target;
        }
      case GuiStat.Bottom:
        {
          return BottomPosition;
        }
      case GuiStat.Top:
        {
          return TopPosition;
        }
      case GuiStat.TopTarget:
        {
          return BottomPosition;
        }
      case GuiStat.Middle:
        {
          MiddlePosition.transform.localPosition = defaultRobotPos;
          MiddlePosition.transform.localRotation = Quaternion.Euler(0, 180, 0);
          return MiddlePosition;
        }
      case GuiStat.MiddleWithoutRobot:
        {
          return MiddlePosition.transform.parent.gameObject;
        }
    }
    return gameObject;
  }

  private void effectSettings_EffectDeactivated(object sender, EventArgs e)
  {
    if (GuiStats[current]!=GuiStat.Middle) 
    currentGo.transform.position = GetInstancePosition(GuiStats[current]);
    isReadyEffect = true;
  }

  private void OnGUI()
  {
    if (GUI.Button(new Rect(10, 15, 105, 30), "Previous Effect")) {
      ChangeCurrent(-1);
    }
    if (GUI.Button(new Rect(130, 15, 105, 30), "Next Effect"))
    {
      ChangeCurrent(+1);
    }
    if(Prefabs[current]!=null)GUI.Label(new Rect(300, 15, 100, 20), "Prefab name is \"" + Prefabs[current].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
    if (GUI.Button(new Rect(10, 60, 225, 30), "Day/Night")) {
      DirLight.intensity = !isDay ? 0.00f : oldLightIntensity;
      RenderSettings.ambientLight = !isDay ? new Color(0.1f, 0.1f, 0.1f) : oldAmbientColor;
      isDay = !isDay;
    }
    if (GUI.Button(new Rect(10, 105, 225, 30), "Change environment")) {
      if (isDefaultPlaneTexture) {
        Plane1.GetComponent<Renderer>().material = PlaneMaterials[0];
        Plane2.GetComponent<Renderer>().material = PlaneMaterials[0];
      }
      else {
        Plane1.GetComponent<Renderer>().material = PlaneMaterials[1];
        Plane2.GetComponent<Renderer>().material = PlaneMaterials[2];
      }
      isDefaultPlaneTexture = !isDefaultPlaneTexture;
    }
    if (current <= 40) {
      GUI.Label(new Rect(10, 152, 225, 30), "Ball Speed " + (int) prefabSpeed + "m", guiStyleHeader);
      prefabSpeed = GUI.HorizontalSlider(new Rect(115, 155, 120, 30), prefabSpeed, 1.0F, 30.0F);
      isHomingMove = GUI.Toggle(new Rect(10, 190, 150, 30), isHomingMove, " Is Homing Move");
      effectSettings.MoveSpeed = prefabSpeed;
    }

    //GUI.Label(new Rect(1, 1, 30, 30), "" + (int)fps + "   " + Screen.dpi, guiStyleHeader);
    
  }

  void Update()
  {
    anim.enabled = isHomingMove;
    //effectSettings.IsHomingMove = isHomingMove;

    timeleft -= Time.deltaTime;
    accum += Time.timeScale / Time.deltaTime;
    ++frames;

    if (timeleft <= 0.0)
    {
      fps = accum / frames;
      timeleft = UpdateInterval;
      accum = 0.0F;
      frames = 0;
    }
    if (isReadyEffect) {
      isReadyEffect = false;
      currentGo.SetActive(true);
    }
    if (GuiStats[current]==GuiStat.BallRotate) {
      currentGo.transform.localRotation = Quaternion.Euler(0, Mathf.PingPong(Time.time*5, 60)-50, 0);
    }
    if (GuiStats[current] == GuiStat.BallRotatex4)
    {
      currentGo.transform.localRotation = Quaternion.Euler(0, Mathf.PingPong(Time.time * 30, 100) - 70, 0);
    }
  }

  private void InstanceCurrent(GuiStat stat)
  {
    switch (stat) {
    case GuiStat.Ball: {
      MiddlePosition.SetActive(false);
      InstanceEffect(transform.position);
      break;
    }
    case GuiStat.BallRotate:
      {
        MiddlePosition.SetActive(false);
        InstanceEffect(transform.position);
        break;
      }
    case GuiStat.BallRotatex4:
      {
        MiddlePosition.SetActive(false);
        InstanceEffect(transform.position);
        break;
      }
    case GuiStat.Bottom: {
      MiddlePosition.SetActive(false);
      InstanceEffect(BottomPosition.transform.position);
      break;
    }
    case GuiStat.Top: {
      MiddlePosition.SetActive(false);
      InstanceEffect(TopPosition.transform.position);
      break;
    }
    case GuiStat.TopTarget:
      {
        MiddlePosition.SetActive(false);
        InstanceEffect(TopPosition.transform.position);
        break;
      }
    case GuiStat.Middle: {
      MiddlePosition.SetActive(true);
      InstanceEffect(MiddlePosition.transform.parent.transform.position);
      break;
    }
    case GuiStat.MiddleWithoutRobot: {
      MiddlePosition.SetActive(false);
      InstanceEffect(MiddlePosition.transform.position);
      break;
    }
    }
  }

  private Vector3 GetInstancePosition(GuiStat stat)
  {
    switch (stat) {
    case GuiStat.Ball: {
      return transform.position;
    }
    case GuiStat.BallRotate:
      {
        return transform.position;
      }
    case GuiStat.BallRotatex4:
      {
        return transform.position;
      }
    case GuiStat.Bottom: {
      return BottomPosition.transform.position;
    }
    case GuiStat.Top: {
      return TopPosition.transform.position;
    }
    case GuiStat.TopTarget:
      {
        return TopPosition.transform.position;
      }
    case GuiStat.MiddleWithoutRobot:
      {
        return MiddlePosition.transform.parent.transform.position;
    }
    case GuiStat.Middle: {
      return MiddlePosition.transform.parent.transform.position;
    }
    }
    return transform.position;
  }

  void ChangeCurrent(int delta)
  {
    Destroy(currentGo);
    CancelInvoke("InstanceDefaulBall");
    current += delta;
    if (current> Prefabs.Length - 1)
      current = 0;
    else if (current < 0)
      current = Prefabs.Length - 1;
      
    if(effectSettings!=null) effectSettings.EffectDeactivated -= effectSettings_EffectDeactivated;
    MiddlePosition.SetActive(GuiStats[current]==GuiStat.Middle);
    //if (GuiStats[current] == GuiStat.Middle) Invoke("InstanceDefaulBall", 2);
    InstanceEffect(GetInstancePosition(GuiStats[current]));
  }
}
