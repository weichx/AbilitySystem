using System.Collections.Generic;

public interface ISerializable {
    void OnSerialized(Dictionary<string, object> properties);
}