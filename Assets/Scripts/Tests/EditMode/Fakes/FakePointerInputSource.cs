using Game.Core.Inputs;
using UnityEngine;

namespace Game.Tests.EditMode.Fakes
{
    public sealed class FakePointerInputSource : IPointerInputSource
    {
        public bool WasPressedThisFrame { get; set; }
        public bool IsOverUI { get; set; }
        public Vector2 Position { get; set; }

        public void PressAt(Vector2 position)
        {
            WasPressedThisFrame = true;
            Position = position;
        }

        public void Release() => WasPressedThisFrame = false;
    }
}
