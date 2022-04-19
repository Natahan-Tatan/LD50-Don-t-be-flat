using System.Collections.ObjectModel;
using Godot;
using System;

namespace Game
{
    public class Player : KinematicBody2D
    {
        [Signal]
        public delegate void Flattened();
        [Signal]
        public delegate void PortalReached();

        [Export]
        public bool HasControl {
            get{
                return _hasControl;
            }
            set{
                SetPhysicsProcess(value);
                _hasControl = value;
            }
        }
        private bool _hasControl = true;

        [Export(PropertyHint.Range,"0,1000,1")]
        public int Gravity {get; set;} = 100;
        [Export(PropertyHint.Range,"0,1000,1")]
        public int Speed {get; set;} = 100;
        [Export(PropertyHint.Range,"0,5000,1")]
        public int Jump {get; set;} = 100;

        private int _currentVerticalForce = 0;
        private bool _canJump = true;
        private AnimatedSprite _sprite = null;

        private RectangleShape2D _shape = null;
        private Godot.Collections.Array<RayCast2D> _raycasts = new Godot.Collections.Array<RayCast2D>();
        private RayCast2D _rayDownLeft = null;
        private RayCast2D _rayDownRight = null;
        private Node2D _lastTopCollider = null;
        private Timer _restoreTimer = null;
        private bool _isRestoring = false;

        private Node2D _droplet = null;

        public override void _Ready()
        {
            GD.Randomize();
            _sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            _restoreTimer = GetNode<Timer>("RestoreTimer");

            _shape = (GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D);

            _droplet = GetNode<Node2D>("Droplet");

            _raycasts.Add(GetNode<RayCast2D>("RayCastLeft"));
            _raycasts.Add(GetNode<RayCast2D>("RayCastMiddle"));
            _raycasts.Add(GetNode<RayCast2D>("RayCastRight"));

            foreach(RayCast2D ray in _raycasts)
            {
                ray.CastTo = new Vector2(0, -_shape.Extents.y - 2);
            }

            _rayDownLeft = GetNode<RayCast2D>("RayCastDownLeft");
            _rayDownRight = GetNode<RayCast2D>("RayCastDownRight");
            _rayDownLeft.CastTo = _rayDownRight.CastTo = new Vector2(0, _shape.Extents.y + 2);
        }
        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if(_canJump && Input.IsActionJustPressed("jump"))
            {
                _canJump = false;
                _currentVerticalForce = -Mathf.RoundToInt(Jump);
            }
            else
            {
                if(IsOnCeiling() && _currentVerticalForce < 0)
                {
                    _currentVerticalForce = 0;
                }
                
                _currentVerticalForce += Mathf.RoundToInt(Gravity);
            }
            
            var vel = new Vector2(0, _currentVerticalForce);

            if(Input.IsActionPressed("ui_right"))
            {
                vel += new Vector2(Speed, 0);
            }
            else if(Input.IsActionPressed("ui_left"))
            {
                vel += new Vector2(-Speed, 0);
            }
            
            MoveAndSlide(vel * Scale.y, Vector2.Up);

            if(Scale.y <= 0.05f)
            {
                EmitSignal("Flattened");
            }
            else
            {
                if(IsOnFloor())
                {
                    _currentVerticalForce = 0;
                    _canJump = true;    

                    if(vel.x != 0)   
                    {
                        PlayAnim("walk");
                    }
                    else 
                    {
                        PlayAnim("idle");
                    }
                }
                else
                {
                    if(vel.y > 0)
                    {
                        PlayAnim("fall");
                    }
                    else
                    {
                        PlayAnim("jump");
                    }
                }

                DoSquish();
            }

        }

        private void DoSquish()
        {
            if((_rayDownLeft.IsColliding() || _rayDownRight.IsColliding()) && Scale.y > 0.05f)
            {
                bool isColliding = false;

                foreach(RayCast2D ray in _raycasts)
                {
                    if(ray.IsColliding())
                    {
                        _lastTopCollider = ray.GetCollider() as Node2D;
                        isColliding = true;

                        Scale -= new Vector2(0, 0.1f);

                        _droplet.Visible = true;
                        _droplet.Scale = new Vector2((1.0f/Scale.x) * 2, (1.0f/Scale.y) * 2);

                        _isRestoring = false;
                        _restoreTimer.Start();

                        break;
                    }
                }

                if(isColliding)
                {
                    foreach(RayCast2D ray in _raycasts)
                    {
                        ray.CastTo = new Vector2(0, -_shape.Extents.y - 2);
                    }
                }
            }

            if(_isRestoring)
            {
                Scale += new Vector2(0, 0.01f);
                _droplet.Scale = new Vector2((1.0f/Scale.x) * 2, (1.0f/Scale.y) * 2);
                
                if(Scale.y > 1)
                {
                    Scale = new Vector2(1, 1);
                    _isRestoring = false;

                    _droplet.Scale = new Vector2(2,2);
                    _droplet.Visible = false;
                }
            }
        }

        public void _on_Player_Flattened()
        {
            _droplet.Scale = new Vector2(2,2);
            _droplet.Visible = false;
            if(_lastTopCollider is Rock rock)
            {
                Vector2 globaPos = GlobalPosition;
                GetParent().RemoveChild(this);
                rock.AddChild(this);
                Position = new Vector2(rock.ToLocal(globaPos).x, (rock.GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D).Extents.y - 5);
            }
            
            SetPhysicsProcess(false);
            GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
            _sprite.Play("flat");
            Scale = new Vector2(1,1);
            _sprite.ZIndex = 10;
        }

        public void _on_RestoreTimer_timeout()
        {
            _isRestoring = true;
        }

        public void PlayAnim(string anim)
        {
            if(Scale.y < 1)
            {
                if(_sprite.Animation != "flattening")
                {
                    _sprite.Animation = "flattening";
                }
            }
            else if(_sprite.Animation != anim)
            {
                _sprite.Animation = anim;
            }
        }

        public void _on_Portal_body_entered(Node body)
        {
            if(body == this)
            {
                AnimationPlayer anim = GetNode<AnimationPlayer>("SpawnAnim/AnimationPlayer");

                anim.Connect("animation_finished",this,nameof(_on_despawn_animation_finished),null, (uint)ConnectFlags.Oneshot);

                anim.Play("despawn");
            }
        }

        public void _on_despawn_animation_finished(String name)
        {
            if(name == "despawn")
            {
                EmitSignal(nameof(PortalReached));
            }
        }

    }
    //end class
}