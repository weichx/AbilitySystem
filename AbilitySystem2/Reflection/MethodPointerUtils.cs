using System;
using System.Reflection;

public static class MethodPointerUtils {
    public static string CreateSignature(MethodInfo info) {
        if (info == null) throw new Exception("Method info is null");
        string type = info.DeclaringType.Name;
        string method = info.Name;
        string retnType = info.ReturnType.Name;
        ParameterInfo[] parameters = info.GetParameters();
        string[] argumentTypes = new string[parameters.Length];
        for (int i = 0; i < parameters.Length; i++) {
            argumentTypes[i] = parameters[i].ParameterType.Name;
        }
        return CreateSignature(type, method, retnType, argumentTypes);
    }

    public static string CreateSignature(string type, string method, string retnType, string[] parameters) {
        string rType = null;
        rType = FilterFloatTypeName(retnType);
        string signature = rType + " " + type + "." + method + "(";
        if (parameters != null) {
            for (int i = 0; i < parameters.Length; i++) {
                signature += FilterFloatTypeName(parameters[i]);
                if (i != parameters.Length - 1) {
                    signature += ", ";
                }
            }
        }
        signature += ")";
        return signature;
    }

    public static string FilterFloatTypeName(string parameterType) {
        if (parameterType == "Single") return "float";
        return parameterType;
    }
}