using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class ScriptableObjectCompiler {
	public enum CompileJobStatus {
		Running, Succeeded, Failed, NotStarted, Invalid, Disposed
	}

	private static List<CompileJob> compileJobs = new List<CompileJob>();

	private class CompileJob {

		public readonly int jobId;
		public readonly string path;
		public CompileJobStatus status;

		private Process process;

		public CompileJob(int jobId) {
			this.jobId = jobId;
			path = @"./" + FileUtil.GetUniqueTempPathInProject();
			status = CompileJobStatus.NotStarted;
		}

		public void Start(string code, string[] assemblies) {
			if(status != CompileJobStatus.NotStarted) {
				Debug.LogError("Compile job already running, Start can only be called once");
				return;
			}

			status = CompileJobStatus.Running;

			try {
				File.WriteAllText(@"./" + path + ".cs", code);
				process = new Process();
				//todo change this path for windows
				process.StartInfo.FileName = @"/usr/local/bin/mcs";
				process.StartInfo.Arguments = BuildArguments(assemblies);
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.UseShellExecute = false;
				process.Start();
				process.WaitForExit();
			} catch(Exception e) {
				process = null;
				status = CompileJobStatus.Failed;
				Debug.LogError(e.Message);
			}
		}

		public CompileJobStatus Update() {
			if(process != null && process.HasExited) {
				string output = process.StandardOutput.ReadToEnd();
				string errors = process.StandardError.ReadToEnd();

				if(!string.IsNullOrEmpty(output)) {
					Debug.Log(output);
				}
				if(!string.IsNullOrEmpty(errors)) {
					Debug.Log(errors);
				}
				if(process.ExitCode == 0) {
					status = CompileJobStatus.Succeeded;
				}
				else {
					status = CompileJobStatus.Failed;
				}
				process.Close();
				process.Dispose();
				process = null;
			}
			return status;
		}

		public void CleanUp() {
			if(status == CompileJobStatus.Running) {
				process.Close();
				process.Dispose();
				process = null;
			}
			status = CompileJobStatus.Disposed;
		}
			
		private string BuildArguments(string[] assemblies) {
			StringBuilder builder = new StringBuilder();
			builder.Append(path + ".cs ");
			builder.Append(@"-target:library ");
			builder.Append(@"-out:" +  path + ".dll");
			for(int i = 0; i < assemblies.Length; i++) {
				builder.Append(" -r:");
				builder.Append(assemblies[i]);
			}
			return builder.ToString();
		}

	}

	public static int QueueCompileJob(string code, string[] assemblyPaths) {
		CompileJob job = new CompileJob(NextJobId);
		compileJobs.Add(job);
		job.Start(code, assemblyPaths);
		return job.jobId;
	}

	public static void UpdateCompileJobs() {
		for(int i = 0; i < compileJobs.Count; i++) {
			compileJobs[i].Update();
		}
	}

	public static bool TryGetJobResult(int jobId, out CompileJobStatus result, out string outputFilePath) {
		CompileJob job = compileJobs.Find((j) => { 
			return j.jobId == jobId;
		});
		if(job == null) {
			result = CompileJobStatus.Invalid;
			outputFilePath = null;
			return false;
		}
		result = job.status;
		outputFilePath = job.path + ".dll";
		return true;
	}

	public static bool RemoveJob(int jobId) {
		for(int i = 0; i < compileJobs.Count; i++) {
			if(compileJobs[i].jobId == jobId) {
				compileJobs[i].CleanUp();
				return true;
			}
		}
		return false;
	}

	private static int JobId = 0;
	private static int NextJobId {
		get { return JobId++; }
	}

}
	