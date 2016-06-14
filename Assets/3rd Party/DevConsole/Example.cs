using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DevConsole;

public class Example:MonoBehaviour {

	void Start(){
		Console.AddCommand(new Command<string>("TIME_TIMESCALE", TimeScale));
		Console.AddCommand(new Command<string>("TIME_SHOWTIME", ShowTime));
		Console.AddCommand(new Command<string>("PHYSICS_GRAVITY_X", XGravity));
		Console.AddCommand(new Command<string>("PHYSICS_GRAVITY_Y", YGravity));
		Console.AddCommand(new Command<string>("PHYSICS_GRAVITY_Z", ZGravity));
		Console.AddCommand(new Command<string>("EXAMPLE_HELP",ExampleCommand, ExampleCommandHelp));
	}
    static void ExampleCommand(string args){
		Console.Log("Type EXAMPLE_HELP? to use this command");
	}
	static void ExampleCommandHelp(){
		string unColoredText = "The help for this command is shown through a custom method";
		while (unColoredText != string.Empty){
			string coloredText = string.Empty;
			int i = 0;
			for (i = 0; i < unColoredText.Length; i++){
				if (unColoredText[i] == ' ')
					break;
				coloredText+=unColoredText[i];
			}
			Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			Console.Log(coloredText, color);
			unColoredText = unColoredText.Substring(Mathf.Min(unColoredText.Length,i+1));
		};
	}
	static void TimeScale(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Time.timeScale = fValue;
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
	static void ShowTime(string args){
		Console.Log(Time.time.ToString());
	}

	static void XGravity(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Physics.gravity = new Vector3(fValue,Physics.gravity.y,  Physics.gravity.z);
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
	static void YGravity(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Physics.gravity = new Vector3(Physics.gravity.x, fValue, Physics.gravity.z);
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
	static void ZGravity(string sValue){
		float fValue;
		if (float.TryParse(sValue, out fValue)){
			Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, fValue);
			Console.Log("Change successful", Color.green);
		}
		else
			Console.LogError("The entered value is not a valid float value");
	}
}