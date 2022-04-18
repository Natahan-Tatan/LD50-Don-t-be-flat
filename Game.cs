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
        private Random Random = new Random();


        public override void _Ready()
        {
            base._Ready();

            //Get current level... (used in editor)
            _currentLevel = GetNodeOrNull<Level>("Level"); 

            //...but remove it to make a new
           GenerateLevel();
            
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
                        GD.Print(signal["name"], " ", connection["method"]," ", connection["target"].GetType());

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

                
                //PROCESSING: use difficuty to configure level


                AddChild(_currentLevel);

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
            GetTree().ChangeScene(MainMenu);
        }

        public void _on_RetryButton_pressed()
        {
            GenerateLevel();
        }
    }
}
