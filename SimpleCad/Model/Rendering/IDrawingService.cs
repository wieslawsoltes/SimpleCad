namespace SimpleCad.Model;

public interface IDrawingService
{
    void Add(DxfEntity dxfEntity);
    void Remove(DxfEntity dxfEntity);
}
