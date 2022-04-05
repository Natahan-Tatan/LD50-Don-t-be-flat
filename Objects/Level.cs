using Godot;
using System;

namespace Game
{
    [Tool]
    public class Level : Node2D
    {
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

                    Vector2 ceilPos = GetNode<Wall>("Ceil").Position;

                    GetNode<Wall>("Ceil").Position = new Vector2(0, -Mathf.Round(GetNode<Wall>("LeftWall").Size.y * ((float)(leftBlockHeight - BaseHeight - 1) / (float)leftBlockHeight)));
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
                _freeSlots.Shuffle();

                Rock newRock = Rock.Instance<Rock>();
                newRock.Position = new Vector2(_freeSlots[0] * _blockSize + (_blockSize/2), _ceil.Position.y - + (_blockSize/2));

                AddChild(newRock);

                _slots[_freeSlots[0]]--;

                if(_slots[_freeSlots[0]] <= 0)
                {
                    _freeSlots.RemoveAt(0);
                }

                _timer.Start();
            }
            
        }
    }
}
