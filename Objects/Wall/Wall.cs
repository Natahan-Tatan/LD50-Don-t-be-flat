using System.Globalization;
using Godot;
using System;

namespace Game
{
    [Tool]
    public class Wall : Node2D
    {
        [Export]
        public PackedScene BlockScene {get; set; } = null;
        
        [Export(PropertyHint.Range, "1,50,1")]
        public int Width {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                _resize();
            }
        }
        private int _width = 1;

        [Export(PropertyHint.Range, "1,100,1")]
        public int Height {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                _resize();
            }
        }

        public Vector2 Size {
            get{

                Block block = GetChildOrNull<Block>(0);

                if(block == null)
                {
                    return Vector2.Zero;
                }

                return new Vector2(Width* block.Size.x, Height * block.Size.y);
            }
        }
        
        private int _height = 1;

        public override void _Ready()
        {
            base._Ready();
            _resize();
        }
        
        private void _resize()
        {
            if(BlockScene != null && GetChildCount() != _width * _height)
            {
                //TODO: improve algo
                foreach(Node child in GetChildren())
                {
                    RemoveChild(child);
                    child.QueueFree();
                }

                for(int i=0; i<_width; i++)
                {
                    for(int j=0; j<_height; j++)
                    {
                        Block block = BlockScene.Instance<Block>();

                        block.Position = new Vector2(i * block.Size.x, -j * block.Size.y);

                        AddChild(block);
                    }
                }

            }
        }

    }
}
