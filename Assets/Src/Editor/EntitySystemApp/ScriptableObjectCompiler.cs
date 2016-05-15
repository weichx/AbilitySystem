using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;
using System.Reflection;
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
        public bool started;
        private Process process;

        public CompileJob(int jobId) {
            this.jobId = jobId;
            path = @"./" + FileUtil.GetUniqueTempPathInProject();
            status = CompileJobStatus.NotStarted;
        }

        public void Start(string code, string[] assemblies, bool sync) {
            if (status != CompileJobStatus.NotStarted) {
                Debug.LogError("Compile job already running, Start can only be called once");
                return;
            }

            status = CompileJobStatus.Running;

            try {
                File.WriteAllText(@"./" + path + ".cs", code);
                process = new Process();
#if UNITY_EDITOR_OSX
                process.StartInfo.FileName = @"/usr/local/bin/mcs";
#elif UNITY_EDITOR_WIN
                process.StartInfo.FileName = @"C:\PROGRA~2\Unity\Editor\Data\Mono\bin\gmcs.bat";
                //need to escape due to spaces in windows paths
                for (int i = 0; i < assemblies.Length; i++) {
                    assemblies[i] = "\"" + assemblies[i] + "\"";
                }
#endif
                process.StartInfo.Arguments = BuildArguments(assemblies);
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                started = process.Start();
                if (sync) {
                    process.WaitForExit();
                    Update();
                }
            }
            catch (Exception e) {
                status = CompileJobStatus.Failed;
                Debug.LogError(e);
                process = null; ;
            }
        }

        public CompileJobStatus Update() {
            if (process != null && started && process.HasExited) {
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(output)) {
                    Debug.Log(output);
                }
                if (!string.IsNullOrEmpty(errors)) {
                    Debug.Log(errors);
                }
                if (process.ExitCode == 0) {
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
            if (status == CompileJobStatus.Running) {
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
            builder.Append(@"-out:" + path + ".dll");
            for (int i = 0; i < assemblies.Length; i++) {
                builder.Append(" -r:");
                builder.Append(assemblies[i]);
            }
            return builder.ToString();
        }

    }

    public static int QueueCompileJob(string code, string[] assemblyPaths, bool sync = false) {
        CompileJob job = new CompileJob(NextJobId);
        compileJobs.Add(job);
        job.Start(code, assemblyPaths, sync);
        return job.jobId;
    }

    public static Type CreateScriptableType(string code, string[] assemblyPaths, string typeName = "GeneratedScriptable") {
        CompileJob job = new CompileJob(NextJobId);
        compileJobs.Add(job);
        job.Start(code, assemblyPaths, true);
        CompileJobStatus result;
        string dllPath;
        if (TryGetJobResult(job.jobId, out result, out dllPath)) {
            if (result == CompileJobStatus.Succeeded) {
                return Assembly.LoadFrom(dllPath).GetType(typeName);
            }
        }
        return null;
    }

    public static void UpdateCompileJobs() {
        for (int i = 0; i < compileJobs.Count; i++) {
            compileJobs[i].Update();
        }
    }

    public static bool TryGetJobResult(int jobId, out CompileJobStatus result, out string outputFilePath) {
        CompileJob job = compileJobs.Find((j) => {
            return j.jobId == jobId;
        });
        if (job == null) {
            result = CompileJobStatus.Invalid;
            outputFilePath = null;
            return false;
        }
        result = job.status;
        outputFilePath = job.path + ".dll";
        return true;
    }

    public static bool RemoveJob(int jobId) {
        for (int i = 0; i < compileJobs.Count; i++) {
            if (compileJobs[i].jobId == jobId) {
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
