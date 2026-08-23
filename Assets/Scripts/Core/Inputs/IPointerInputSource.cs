using UnityEngine;

namespace Game.Core.Inputs
{
    public interface IPointerInputSource
    {
        bool WasPressedThisFrame { get; }
        bool IsOverUI { get; }
        Vector2 Position { get; }
    }
}
