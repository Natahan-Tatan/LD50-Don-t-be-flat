using System.Collections.Generic;
using Godot;
using System;

namespace Game
{
    [Tool]
    public class Level : Node2D
    {
        [Signal]
        public delegate void Finished();
        [Signal]
        public delegate void GameOver();
        public int BaseHeight = 6;

        private bool _ready = false;
        private int _blockSize = 0;
        private Wall _ceil = null;
        private Timer _timer = null;

        private Godot.Collections.Array<int> _slots = new Godot.Collections.Array<int>();
        private Godot.Collections.Array<int> _freeSlots = new Godot.Collections.Array<int>();
        private Random _rand = new Random();

        [Export]
        public PackedScene Rock = null;

        [Export(PropertyHint.Range,"1,10,1")]
        public int ExitHeight 
        {
            get
            {
                return _exitHeight;
            }
            set
            {
                _exitHeight = value;
                Height = Height;
            }
        }
        private int _exitHeight = 4;


        [Export(PropertyHint.Range,"2,100,1")]
        public int Height{
            get{
                return _height;
            }

            set{
                if(_ready)
                {
                    int leftBlockHeight = BaseHeight + value + BaseHeight + _exitHeight;
                    GetNode<Wall>("LeftWall").Height = leftBlockHeight;
                    GetNode<Wall>("RightWall").Height = BaseHeight + value;

                    GetNode<Wall>("LeftWall").Position = new Vector2(GetNode<Wall>("LeftWall").Position.x, -GetNode<Wall>("LeftWall").Size.y);

                    GetNode<Wall>("Ceil").Position = new Vector2(GetNode<Wall>("Ceil").Position.x, -GetNode<Wall>("LeftWall").Size.y);

                    GetNode<Wall>("RightWall").Position = new Vector2(GetNode<Wall>("RightWall").Position.x, -GetNode<Wall>("RightWall").Size.y);
                }

                _freeSlots.Clear();
                for(int i=0; i<_slots.Count; i++)
                {
                    _slots[i] = value;
                    _freeSlots.Add(i);
                }

                _height = value;
            }
        }
        private int _height = 20;


        [Export(PropertyHint.Range,"1,100,1")]
        public int Width{
            get{
                return _width;
            }

            set{
                if(_ready)
                {
                    GetNode<Wall>("Ceil").Width = value + GetNode<Wall>("RightWall").Width;
                    GetNode<Wall>("Ground").Width = value;

                    Vector2 rightPos = GetNode<Wall>("RightWall").Position;
                    GetNode<Wall>("RightWall").Position = new Vector2(GetNode<Wall>("Ground").Size.x,rightPos.y);
                }

                _freeSlots.Clear();
                _slots.Resize(value);
                for(int i=0; i<_slots.Count; i++)
                {
                    _slots[i] = Height;
                    _freeSlots.Add(i);
                }


                _width = value;
            }
        }
        private int _width = 20;

        [Export(PropertyHint.Range,"0.2,5,0.1")]
        public float FallingDelayMin {get; set;} = 5;
        [Export(PropertyHint.Range,"0.2,5,0.1")]
        public float FallingDelayMax {get; set;} = 5;

        [Export(PropertyHint.ExpRange,"1,500,0.9")]
        public int RockSpeedMin {get; set;} = 100;
        [Export(PropertyHint.ExpRange,"1,500,0.9")]
        public int RockSpeedMax {get; set;} = 200;

        [Export(PropertyHint.Range,"1,30,0.9")]
        public int MinCountRock {get; set;} = 1;
        [Export(PropertyHint.Range,"1,50,0.9")]
        public int MaxCountInRock {get; set;} = 3;

        public override void _Ready()
        {
            base._Ready();

            _ready = true;
            ExitHeight = ExitHeight;
            Width = Width;

            _ceil = GetNode<Wall>("Ceil");
            _blockSize = Mathf.RoundToInt(_ceil.Size.y / _ceil.Height);

            _timer = GetNode<Timer>("FallTimer");
            
        }

        public void _on_FallTimer_timeout()
        {
            if(_freeSlots.Count > 0 && Rock != null)
            {
                int speed = Mathf.RoundToInt((float)GD.RandRange(RockSpeedMin, RockSpeedMax));
                int countRocks = Mathf.RoundToInt((float)GD.RandRange(MinCountRock, MaxCountInRock));
                int currentSlot = _freeSlots[Mathf.RoundToInt((float)GD.RandRange(0, _freeSlots.Count-1))];

                Dictionary<int, int> countAppearInSlot = new Dictionary<int, int>();
                
                for(int i=0; i<countRocks;i++)
                {
                    if(i != 0)
                    {
                        int candidateSlot = currentSlot + (GD.Randi()%2 == 0 ? -1:1);

                        if(_freeSlots.Contains(candidateSlot))
                        { 
                            currentSlot = candidateSlot;
                        }
                    }

                    if(countAppearInSlot.ContainsKey(currentSlot))
                    {
                        countAppearInSlot[currentSlot]++;
                    }
                    else
                    {
                        countAppearInSlot.Add(currentSlot, 1);
                    }

                    Rock newRock = Rock.Instance<Rock>();
                    newRock.Speed = speed;

                    int offset = countAppearInSlot[currentSlot] - 1;

                    newRock.Position = new Vector2(currentSlot * _blockSize + (_blockSize/2), _ceil.Position.y + (_blockSize * (_ceil.Height - 1 - offset)));

                    AddChild(newRock);

                    _slots[currentSlot]--;

                    if(_slots[currentSlot] <= 0)
                    {
                        _freeSlots.Remove(currentSlot);

                        if(_freeSlots.Count > 0)
                        {
                            currentSlot = _freeSlots[Mathf.RoundToInt((float)GD.RandRange(0, _freeSlots.Count-1))];
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                float time = (float)GD.RandRange(FallingDelayMin, FallingDelayMax);
                _timer.Start(time);
            }
        }

        public void _on_Player_PortalReached()
        {
            GD.Print("Emit finisihed");
            EmitSignal(nameof(Finished));
        }

        public void _on_Player_Flattened()
        {
            GD.Print("Emit game over");
            EmitSignal(nameof(GameOver));
        }
    }
}
