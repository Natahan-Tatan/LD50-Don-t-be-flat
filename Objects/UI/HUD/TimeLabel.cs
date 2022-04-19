using System.Xml;
using Godot;
using System;

namespace Game
{
    public class TimeLabel : Label
    {
        [Export]
        public NodePath Game {get; set;} = null;
        private Game _game = null;

        public override void _Ready()
        {
            base._Ready();

            _game = GetNode<Game>(Game);
        }
        public override void _Process(float delta)
        {
            ulong time = _game.CurrentGameTime;

            Text = String.Format("Time: {0}:{1}.{2}", ((time/1000)/60).ToString("00"), ((time/1000)%60).ToString("00"), (time%1000).ToString("000").Substring(0,2));
        }
    }
}
