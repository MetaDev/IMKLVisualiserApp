using UnityEngine;

public class Implement {
  private TextAsset _SVGFile;
  private Texture2D _texture;
  private readonly SVGGraphics _graphics;
  private SVGDocument _svgDocument;

  public Implement(TextAsset svgFile, ISVGDevice device) {
    _SVGFile = svgFile;
    _graphics = new SVGGraphics(device);
  }

  private void CreateEmptySVGDocument() {
    _svgDocument = new SVGDocument(_SVGFile.text, _graphics);
  }

  public void StartProcess() {
    
    CreateEmptySVGDocument();
    
    
    SVGSVGElement _rootSVGElement = _svgDocument.RootElement;
    
    
    _graphics.SetColor(Color.white);
    
    
    _rootSVGElement.Render();
    
    
    _texture = _graphics.Render();
    
  }

  public void NewSVGFile(TextAsset svgFile) {
    _SVGFile = svgFile;
  }

  public Texture2D GetTexture() {
    if(_texture == null)
      return new Texture2D(0, 0, TextureFormat.ARGB32, false);
    return _texture;
  }
}
