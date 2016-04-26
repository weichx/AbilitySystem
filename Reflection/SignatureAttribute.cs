using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SignatureAttribute : Attribute {

    public string signatureString;
    public Type[] parameterTypes;
    public Type retnType; //todo can probably always set this to typeof(float)

    public SignatureAttribute(params Type[] signature) {
        signatureString = "";
        if (signature != null && signature.Length > 0) {
            retnType = signature[signature.Length - 1];
            signatureString += MethodPointerUtils.FilterFloatTypeName(retnType.Name) + " Fn(";
            parameterTypes = new Type[signature.Length - 1];
            for (int i = 0; i < parameterTypes.Length; i++) {
                parameterTypes[i] = signature[i];
                signatureString += MethodPointerUtils.FilterFloatTypeName(signature[i].Name);
                if (i != parameterTypes.Length - 1) {
                    signatureString += ", ";
                }
            }
            signatureString += ")";
        }
    }

    public override string ToString() {
        return signatureString;
    }

}
