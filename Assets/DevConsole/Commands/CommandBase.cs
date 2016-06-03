using System;
using System.Reflection;

namespace DevConsole{
	[System.Serializable]
	public abstract class CommandBase {

		//=======================
		//VARS
		//=======================
		public string name{
			get; private set;
		}
		public string helpText{
			get; private set;
		}
		public delegate void HelpMethod();
		HelpMethod helpMethod;
		Delegate method;
		
		//==============================
		//CONSTRUCTORS
		//==============================
		public CommandBase (string name, Delegate method){
			this.name = name;
			this.method = method;
		}
		public CommandBase (string name, Delegate method, string helpText):this(name, method){
			this.helpText = helpText;
		}
		public CommandBase (string name, Delegate method, HelpMethod helpMethod):this(name, method){
			this.helpMethod = helpMethod;
		}
		public CommandBase(Delegate method):this(method.Method.DeclaringType.Name+"."+method.Method.Name, method){}
		public CommandBase(Delegate method, string helpText):this(method.Method.DeclaringType.Name+"."+method.Method.Name, method, helpText){}
		public CommandBase(Delegate method, HelpMethod helpMethod):this(method.Method.DeclaringType.Name+"."+method.Method.Name, method, helpMethod){}
		//=======================
		//EXECUTE
		//=======================
		public void Execute(string args){
			try{
				method.Method.Invoke(method.Target,ParseArguments(args));
			}catch(Exception e){
				Console.LogError(e.InnerException.Message+(Console.verbose?"\n"+e.InnerException.StackTrace:string.Empty));
			}
		}
		protected abstract object[] ParseArguments (string args);
		public virtual void ShowHelp(){
			if (helpMethod != null)
				helpMethod();
			else
				Console.LogInfo("Command Info: " + (helpText == null?"There's no help for this command":helpText));
		}
		//=======================
		//HELPERS
		//=======================
		protected T GetValueType<T>(string arg){
			try{
				T returnValue;
				if (typeof(bool) == typeof(T)){
					bool result;
					if (StringToBool(arg,out result))
						returnValue = (T)(object)result;
					else
						throw new System.Exception();
				}
				else
					returnValue = (T)System.Convert.ChangeType(arg,typeof(T));
				return returnValue;
			}catch{
				throw new ArgumentException("The entered value is not a valid "+typeof(T)+" value");
			}
		}
		protected bool StringToBool(string value, out bool result){
			bool bResult = result= false;
			int iResult = 0;
			
			if (bool.TryParse(value, out bResult))
				result = bResult;
			else if(int.TryParse(value, out iResult)){
				if (iResult == 1 || iResult == 0)
					result = iResult==1?true:false;
				else
					return false;
			}
			else{
				string s = value.ToLower().Trim();
				if (s.Equals("yes") || s.Equals("y"))
					result = true;
				else if (s.Equals("no") || s.Equals("n"))
					result = false;
				else
					return false;
			}
			return true;
		}
	}
}