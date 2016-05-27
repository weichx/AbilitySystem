public interface IReader {
    void ReadDefault();
    void ReadDefaultExcept(string[] exceptions);
    object GetFieldValue(string fieldId);
    T GetFieldValue<T>(string fieldId);
    object GetFieldValueAtIndex(int index);
    T GetFieldValueAtIndex<T>(int index);
}