using System;
using UnityEngine;

namespace DevConsole{
	public class Vector2Command:CommandBase{
		
		public delegate void ConsoleMethod(Vector2 vector);
		
		public Vector2Command (string name, ConsoleMethod method):base(name, method){}
		public Vector2Command (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public Vector2Command (string name, ConsoleMethod method, HelpMethod helpMethod):base(name, method, helpMethod){}
		public Vector2Command (ConsoleMethod method):base(method){}
		public Vector2Command (ConsoleMethod method, string helpText):base(method, helpText){}
		public Vector2Command (ConsoleMethod method, HelpMethod helpMethod):base(method, helpMethod){}

		protected override object[] ParseArguments (string message){
			try{
				string[] args = message.Split(' ');
				Vector2 vector = new Vector2(float.Parse(args[0]), float.Parse(args[1]));
				return new object[]{vector};
			}catch{
				throw new ArgumentException("The entered value is not a valid Vector2 value");
			}
		}
	}
	public class Vector3Command:CommandBase{
		
		public delegate void ConsoleMethod(Vector3 vector);
		
		public Vector3Command (string name, ConsoleMethod method):base(name, method){}
		public Vector3Command (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public Vector3Command (string name, ConsoleMethod method, HelpMethod helpMethod):base(name, method, helpMethod){}
		public Vector3Command (ConsoleMethod method):base(method){}
		public Vector3Command (ConsoleMethod method, string helpText):base(method, helpText){}
		public Vector3Command (ConsoleMethod method, HelpMethod helpMethod):base(method, helpMethod){}
		
		protected override object[] ParseArguments (string message){
			try{
				string[] args = message.Split(' ');
				Vector3 vector = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
				return new object[]{vector};
			}catch{
				throw new ArgumentException("The entered value is not a valid Vector3 value");
			}
		}
	}
}