using Godot;
using System;

public class Camera : Camera2D
{
    public void _on_Player_Flattened()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("zoom");
    }
}
