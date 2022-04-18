using Godot;
using System;

namespace Game
{
    public class Game : Node
    {
        [Export(PropertyHint.File,"*.tscn")]
        public String MainMenu {get; set;}
        [Export]
        public int Difficulty {get; set;} = 1;
        [Export]
        public PackedScene LevelScene {get; set;} = null;

        private Level _currentLevel = null;


        public override void _Ready()
        {
            GD.Randomize();
            base._Ready();

            //Get current level... (used in editor)
            _currentLevel = GetNodeOrNull<Level>("Level"); 

            //...but remove it to make a new
           GenerateLevel();
            
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if(Input.IsActionJustPressed("Pause"))
            {
                ChangePause();
            }
        }

        public void _on_PauseButton_pressed()
        {
            ChangePause();
        }

        protected void ChangePause()
        {
            bool currentInPause = GetNode<Control>("UI/PauseMenu").Visible;

            GetNode<Control>("UI/PauseMenu").Visible = !currentInPause;

            GetTree().Paused = !currentInPause;
        }

        protected void GenerateLevel()
        {
            GetNode<Control>("UI/GameOverMenu").Visible = false;

            if(_currentLevel != null && LevelScene != null)
            {
                var newLevel = LevelScene.Instance<Level>();

                //Connect signals froms old to new level
                foreach(Godot.Collections.Dictionary signal in _currentLevel.GetSignalList())
                {                    
                    foreach(Godot.Collections.Dictionary connection in _currentLevel.GetSignalConnectionList(signal["name"] as string))
                    {
                        newLevel.Connect(signal["name"] as string, connection["target"] as Godot.Object, connection["method"] as string);
                    }
                }

                //Check if the old level is a child of the Game (process all cases)
                if(_currentLevel.Owner == this)
                {
                    RemoveChild(_currentLevel);
                }

                //Free old level and assign new  as current
                _currentLevel.QueueFree();
                _currentLevel = newLevel;

                
                //TODO: use difficuty to configure level

                _currentLevel.Width = (int)Math.Round(GD.RandRange(5, 20));
                _currentLevel.Height =(int)Math.Round(GD.RandRange(5, 30));

                _currentLevel.ExitHeight = (int)GD.RandRange(1,3);

                _currentLevel.FallingDelayMin = (float)GD.RandRange(0.1, 5);
                _currentLevel.FallingDelayMax = (float)GD.RandRange(_currentLevel.FallingDelayMin + 0.1, 10);

                _currentLevel.RockSpeedMin = (int)Math.Round(GD.RandRange(50, 100));
                _currentLevel.RockSpeedMax = (int)Math.Round(GD.RandRange(_currentLevel.RockSpeedMin, 500));

                AddChild(_currentLevel);
                MoveChild(_currentLevel, 0);

            }
        }


        public void _on_Level_Finished()
        {
            
        }

        public void _on_Level_GameOver()
        {
            GD.Print("Level game over");
        }

        public void _on_QuitButton_pressed()
        {
            GetTree().Paused = false;
            GetTree().ChangeScene(MainMenu);
        }

        public void _on_RetryButton_pressed()
        {
            GenerateLevel();
        }
    }
}
