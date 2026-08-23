using Game.Core.Inputs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.Runtime.Inputs
{
    public sealed class PointerInputSource : IPointerInputSource
    {
        public bool WasPressedThisFrame =>
            Pointer.current != null && Pointer.current.press.wasPressedThisFrame;

        public bool IsOverUI =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        public Vector2 Position =>
            Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
    }
}
