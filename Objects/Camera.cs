using Godot;
using System;

public class Camera : Camera2D
{
    [Export]
    public NodePath NodeToFollow = null;
    private Node2D _follow = null;

    public override void _Ready()
    {
        base._Ready();

        _follow = GetNode<Node2D>(NodeToFollow);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Position = _follow.GlobalPosition;
    }
    public void _on_Player_Flattened()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("zoom");
    }
}
