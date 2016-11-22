using System;
using System.Reflection;

public interface IWriter {
    void WriteDefault();
    void WriteDefaultExcept(string[] exceptions);
    void WriteField(FieldInfo fInfo);
    void WriteField(string fieldId, object value);
    void WriteField(string fieldId, Type type, object value);
}