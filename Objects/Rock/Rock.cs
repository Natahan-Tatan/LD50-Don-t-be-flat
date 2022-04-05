
using Godot;

namespace Game
{    public class Rock : KinematicBody2D
    {
        [Export(PropertyHint.Range,"10,500,10")]
        public int Speed = 100;
        [Export]
        public bool CanFall{
            get{
                return _canFall;
            }

            set{
                _canFall = value;

                if(_raycast != null)
                {
                    _raycast.Enabled = _canFall;
                }
            }
        }
        private bool _canFall = true;

        private Vector2 _extends = Vector2.Zero;
        private RayCast2D _raycast = null;
        public override void _Ready()
        {
            base._Ready();

            _raycast = GetNode<RayCast2D>("RayCast");
            _extends = (GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D).Extents;

            _raycast.CastTo = new Vector2(0, _extends.y + 5);

            //Init complex properties
            CanFall = CanFall;
        }

        public void DoMove(float delta)
        {
            Position += new Vector2(0, Speed * delta);
        }

        public override void _PhysicsProcess(float delta)
        {
            if(Engine.EditorHint)
                return;

            base._PhysicsProcess(delta);

            if(CanFall)
            {
                if(
                    _raycast.IsColliding() && _raycast.GetCollider() is Node node && node.IsInGroup("rock")
                )
                {
                    var collide = MoveAndCollide(new Vector2(0,Speed * delta));

                    if(collide != null && collide.Collider is Node2D collider && collider.IsInGroup("ground"))
                    {
                        if(collider is Rock)
                        {
                            Position = new Vector2(Position.x, collider.Position.y - (_extends.y * 2));
                        }
                        
                        this.AddToGroup("ground");
                        SetPhysicsProcess(false);
                    }

                }
                else
                {
                    
                    DoMove(delta);
                }
            }
        }
    }

}