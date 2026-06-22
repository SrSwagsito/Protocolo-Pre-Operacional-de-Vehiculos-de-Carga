using UnityEngine;

public interface ICleanable
{
    public void Clean();
    bool IsClean { get; }
}
