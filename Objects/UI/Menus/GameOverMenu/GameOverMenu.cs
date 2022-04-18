using Godot;
using System;

namespace Game
{
    public class GameOverMenu : Control
    {
        public void _on_Level_GameOver()
        {
            GD.Print("Make menu visible");
            Visible = true;
        }

    }
}
