using System.Collections;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[DisallowMultipleComponent()]
public class Invoke : MonoBehaviour
{
    public TextAsset SVGFile = null;
    [Tooltip("Use a faster rendering approach that takes notably more memory.")]
    public bool fastRenderer = false;

    [Space(15)]
    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    public FilterMode filterMode = FilterMode.Trilinear;
    [Range(0, 9)]
    public int anisoLevel = 9;

    private void Start()
    {
        //yield return new WaitForSeconds(0.1f);
        if (SVGFile != null)
        {
            var rend = GetComponent<Renderer>();
            
            var shader1 = Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse");
            rend.material.shader = shader1;


            ISVGDevice device;
            if (fastRenderer)
                device = new SVGDeviceFast();
            else
                device = new SVGDeviceSmall();
            var implement = new Implement(SVGFile, device);



            implement.StartProcess();


            var result = implement.GetTexture();

            // result.wrapMode = wrapMode;
            // result.filterMode = filterMode;
            // result.anisoLevel = anisoLevel;
            result.alphaIsTransparency=true;
            print(result.format);

            rend.material.SetTexture("_MainTex", result);





        }
    }
}
