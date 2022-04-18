using Godot;
using System;

public class PauseMenu : Control
{
    public override void _Ready()
    {
        Visible = false;
    }

    public void _on_PauseButton_pressed()
    {
        Visible = false;
    }
}
