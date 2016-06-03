namespace DevConsole{
	public class Command:CommandBase{
		
		public delegate void ConsoleMethod();
		
		public Command (string name, ConsoleMethod method):base(name, method){}
		public Command (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public Command (string name, ConsoleMethod method, HelpMethod helpMethod):base(name, method, helpMethod){}
		public Command (ConsoleMethod method):base(method){}
		public Command (ConsoleMethod method, string helpText):base(method, helpText){}
		public Command (ConsoleMethod method, HelpMethod helpMethod):base(method, helpMethod){}

		protected override object[] ParseArguments (string args){
			return new object[]{};
		}
	}
	
	public class ParamsCommand<T0>:CommandBase{
		
		public delegate void ConsoleMethod(params T0[] arg0);
		
		public ParamsCommand (string name, ConsoleMethod method):base(name, method){}
		public ParamsCommand (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public ParamsCommand (string name, ConsoleMethod method, HelpMethod helpMethod):base(name, method, helpMethod){}
		public ParamsCommand (ConsoleMethod method):base(method){}
		public ParamsCommand (ConsoleMethod method, string helpText):base(method, helpText){}
		public ParamsCommand (ConsoleMethod method, HelpMethod helpMethod):base(method, helpMethod){}

		protected override object[] ParseArguments (string message){
			try{
				string[] args = message.Split(' ');
				T0[] parameters = new T0[args.Length];
				for (int i = 0; i < parameters.Length; i++)
					parameters[i] = GetValueType<T0>(args[i]);
				return new object[]{parameters};
			}catch(System.Exception e){
				throw e;
			}
		}
	}

	public class Command<T0>:CommandBase {

		public delegate void ConsoleMethod(T0 arg0);
		
		public Command (string name, ConsoleMethod method):base(name, method){}
		public Command (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public Command (string name, ConsoleMethod method, HelpMethod helpMethod):base(name, method, helpMethod){}
		public Command (ConsoleMethod method):base(method){}
		public Command (ConsoleMethod method, string helpText):base(method, helpText){}
		public Command (ConsoleMethod method, HelpMethod helpMethod):base(method, helpMethod){}

		protected override object[] ParseArguments (string args){
			try{
				return new object[]{GetValueType<T0>(args)};
			}catch(System.Exception e){
				throw e;
			}
		}
	}

	public class Command<T0,T1>:CommandBase{
		
		public delegate void ConsoleMethod(T0 arg0, T1 arg1);
		
		public Command (string name, ConsoleMethod method):base(name, method){}
		public Command (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public Command (string name, ConsoleMethod method, HelpMethod helpMethod):base(name, method, helpMethod){}
		public Command (ConsoleMethod method):base(method){}
		public Command (ConsoleMethod method, string helpText):base(method, helpText){}
		public Command (ConsoleMethod method, HelpMethod helpMethod):base(method, helpMethod){}

		protected override object[] ParseArguments (string message){
			try{
				string[] args = message.Split(' ');
				return new object[]{GetValueType<T0>(args[0]), GetValueType<T1>(args[1])};
			}catch(System.Exception e){
				throw e;
			}
		}
	}

	public class Command<T0,T1,T2>:CommandBase{
		
		public delegate void ConsoleMethod(T0 arg0, T1 arg1, T2 arg2);
		
		public Command (string name, ConsoleMethod method):base(name, method){}
		public Command (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public Command (string name, ConsoleMethod method, HelpMethod helpMethod):base(name, method, helpMethod){}
		public Command (ConsoleMethod method):base(method){}
		public Command (ConsoleMethod method, string helpText):base(method, helpText){}
		public Command (ConsoleMethod method, HelpMethod helpMethod):base(method, helpMethod){}

		protected override object[] ParseArguments (string message){
			try{
				string[] args = message.Split(' ');
				return new object[]{GetValueType<T0>(args[0]), GetValueType<T1>(args[1]), GetValueType<T2>(args[2])};
			}catch(System.Exception e){
				throw e;
			}
		}
	}
}