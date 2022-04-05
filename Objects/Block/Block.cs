using Godot;
using System;

namespace Game
{
    [Tool]
    public class Block : StaticBody2D
    {
        public Vector2 Size
        {
            get
            {
                return (GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D).Extents * 2;
            }
        }
    }
}
