using UnityEngine;

namespace X3D {
public class X3DAppearance : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // Appearance fields
    public GameObject material; // Material node
    public GameObject texture; // ImageTexture, MovieTexture, PixelTexture
    public GameObject textureTransform; // TextureTransform node
    public GameObject fillProperties; // FillProperties node
    public GameObject lineProperties; // LineProperties node
}
}
