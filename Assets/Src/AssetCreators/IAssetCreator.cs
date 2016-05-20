public interface IAssetCreator<T> {
    T Create();
    T CreateForEditor();
}