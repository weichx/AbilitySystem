public interface IReader {
    void ReadDefault();
    void ReadDefaultExcept(string[] exceptions);
    void ReadField(string fieldId);
}