using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UnityBase
{
    public interface IUnityMarker
    {
        void Mark(Vector3 position);

        void Mark();

        void Unmark();

        void DestroyGameObject();

        Vector3 Position { get; set; }

        Vector3 Scale { get; set; }

        SpriteRenderer Renderer { get; }

        bool Visible { get; set; }

        void BeginMove(IEnumerable<MoveOrder> orders, float movementSpeed, bool withRotation = false, bool continueMovement = false);
    }

    public interface IUnityButton : IUnityMarker
    {
        Action ClickableAction { get; set; }

        Action OnMouseOverAction { get; set; }

        Action OnMouseExitAction { get; set; }
    }
}