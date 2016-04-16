using System.Collections.Generic;

public interface IDeserializable {
    void OnDeserialized(Dictionary<string, object> table);
}