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

        private int _currentLevelNumber = 1;

        //Time
        public ulong CurrentGameTime{
            get{
                if(IsGameOver)
                {
                    return _gameDuration;
                }
                else
                {
                    return OS.GetTicksMsec() - _timeStart - _durationPause;
                }
            }
        }

        public bool IsGameOver {get; set;} = false;
        private ulong _timeStart = 0;
        private ulong _timePause = 0;
        private ulong _durationPause = 0;

        private ulong _gameDuration = 0;


        public override void _Ready()
        {
            GD.Randomize();
            base._Ready();

            //Get current level... (used in editor)
            _currentLevel = GetNodeOrNull<Level>("Level"); 

            //...but remove it to make a new
           GenerateLevel();

           _timeStart = OS.GetTicksMsec();
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if(Input.IsActionJustPressed("Pause") && !IsGameOver)
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

            if(GetTree().Paused)
            {
                _timePause = OS.GetTicksMsec();
            }
            else
            {
                _durationPause += OS.GetTicksMsec() - _timePause;
            }
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

                _currentLevel.Width = (int)Math.Round(GD.RandRange(5, 8));
                _currentLevel.Height =(int)Math.Round(GD.RandRange(5, 10));

                // _currentLevel.Width = (int)Math.Round(GD.RandRange(15,15));//TEST
                // _currentLevel.Height =(int)Math.Round(GD.RandRange(20,20));//TEST

                _currentLevel.ExitHeight = (int)GD.RandRange(1,3);
                _currentLevel.ExitHeight += 5 - Mathf.Min(Difficulty, 5);

                _currentLevel.FallingDelayMin = (float)GD.RandRange(0.3, 4);
                _currentLevel.FallingDelayMax = (float)GD.RandRange(_currentLevel.FallingDelayMin + 0.1, 5);

                _currentLevel.RockSpeedMin = (int)Math.Round(GD.RandRange(50, 100));
                _currentLevel.RockSpeedMax = (int)Math.Round(GD.RandRange(_currentLevel.RockSpeedMin, 500 + (Mathf.Min(Difficulty, 20)) * 10));

                _currentLevel.MinCountRock = (int)Math.Round(GD.RandRange(1, Mathf.CeilToInt(_currentLevel.Width/5)));
                _currentLevel.MaxCountInRock = (int)Math.Round(GD.RandRange(_currentLevel.MinCountRock,  Mathf.CeilToInt(_currentLevel.Width / 2)));

                _currentLevel.MinCountRock += Mathf.Min(Mathf.FloorToInt(Difficulty/10), 4);
                _currentLevel.MaxCountInRock += Mathf.Min(Mathf.FloorToInt(Difficulty/10), 4);

                // //TEST

                // _currentLevel.FallingDelayMin = (float)GD.RandRange(0.5, 0.5);
                // _currentLevel.FallingDelayMax = (float)GD.RandRange(1,1);

                AddChild(_currentLevel);
                MoveChild(_currentLevel, 0);

            }
        }


        public void _on_Level_Finished()
        {
            _currentLevelNumber++;
            Difficulty = Mathf.Max(_currentLevelNumber/2,1);

            GD.Print("Difficulty: ", Difficulty);
            GetNode<Label>("UI/HUD/VBoxContainer/LevelLabel").Text = "Level: " + _currentLevelNumber + " ";
            GenerateLevel();
        }

        public void _on_Level_GameOver()
        {
            _gameDuration = CurrentGameTime;
            IsGameOver = true;
            GD.Print("Level game over");
        }

        public void _on_QuitButton_pressed()
        {
            GetTree().Paused = false;
            GetTree().ChangeScene(MainMenu);
        }

        public void _on_RetryButton_pressed()
        {
            _timePause = _durationPause = _gameDuration = 0;
            IsGameOver = false;
            _timeStart = OS.GetTicksMsec();

            GenerateLevel();
        }
    }
}
