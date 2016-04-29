using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;

public static class ScriptableObjectCompiler {

    public static ScriptableObject Compile(string code) {
        Dictionary<string, string> provOptions = new Dictionary<string, string>();

        provOptions.Add("CompilerVersion", "v2.0");
        CSharpCodeProvider provider = new CSharpCodeProvider(provOptions);
        CompilerParameters compilerParams = new CompilerParameters();
        compilerParams.GenerateExecutable = false;
        compilerParams.GenerateInMemory = true;
        compilerParams.ReferencedAssemblies.Add(typeof(Ability).Assembly.Location);
        compilerParams.ReferencedAssemblies.Add(typeof(Vector3).Assembly.Location);
        CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, code);
        if (results.Errors.Count > 0) {
            Debug.Log("Number of Errors: " + results.Errors.Count);
            foreach (CompilerError err in results.Errors) {
                Debug.Log(err.ErrorText);
            }
        }
        Type t = results.CompiledAssembly.GetType("Generated");
        return ScriptableObject.CreateInstance(t);
    }
}