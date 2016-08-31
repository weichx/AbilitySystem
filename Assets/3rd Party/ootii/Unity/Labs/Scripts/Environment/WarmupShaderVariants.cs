using UnityEngine;

public class WarmupShaderVariants : MonoBehaviour
{

    public ShaderVariantCollection ShaderVariants;

    void Awake()
    {
        // Do this here because the array in GraphicsSettings doesn't seem to be getting used when running inside the editor
        ShaderVariants.WarmUp();
    }
}
