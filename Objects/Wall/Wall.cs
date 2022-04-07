using System.Globalization;
using Godot;
using System;

namespace Game
{
    [Tool]
    public class Wall : Node2D
    {
        [Export]
        public Texture Tile {get; set; } = null;
        
        [Export(PropertyHint.Range, "1,50,1")]
        public int Width {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                Resize();
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
                Resize();
            }
        }
        private int _height = 1;

        public Vector2 Size {
            get{
                
                if(Tile == null)
                {
                    return Vector2.Zero;
                }

                return new Vector2(
                    Width * Tile.GetSize().x * Scale.x, 
                    Height *Tile.GetSize().y * Scale.y
                    );
            }
        }

        private CollisionShape2D _shape = null;

        public override void _Ready()
        {
            base._Ready();

            _shape = GetNode<CollisionShape2D>("CollisionShape2D");

            _shape.Shape = _shape.Shape.Duplicate() as RectangleShape2D;

            Resize();
        }

        public void Resize()
        {
            Update();

            if(_shape != null)
                _shape.Position = (_shape.Shape as RectangleShape2D).Extents = new Vector2(Size.x / Scale.x, Size.y / Scale.y) / 2;
        }

        public override void _Draw()
        {
            base._Draw();

            for(int i=0; i<Width; i++)
            {
                for(int j=0; j<Height; j++)
                {
                    DrawTexture(Tile, new Vector2(i*Tile.GetSize().x, j*Tile.GetSize().y));
                }
            }
        }

    }
}
