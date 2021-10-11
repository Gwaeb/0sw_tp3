using Godot;
using System;

public class player : KinematicBody2D
{
    Vector2 UP = new Vector2(0, -1);
    const int GRAVITY = 20;
    const int MAXFALLSPEED = 200;
    const int MAXSPEED = 100;
    const int JUMPFORCE = 300;
    const int ACCEL = 10;

    enum State
    {
        MOVE,
        ATTACK
    }

    State state = State.MOVE;


    Vector2 vZero = new Vector2();
    Vector2 motion = new Vector2();

    Sprite currentSprite;
    AnimationPlayer animPlayer;
    // Called when the node enters the scene tree for the first time.
    AnimationTree animationTree;
    AnimationNodeStateMachinePlayback animationState;

    bool facing_right = true;
    public override void _Ready()
    {
        currentSprite = GetNode<Sprite>("Sprite");
        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationTree = GetNode<AnimationTree>("AnimationTree");
        animationTree.Active = true;
        animationState = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case State.MOVE:
                move_state();
                break;
            case State.ATTACK:
                attack_state();
                break;
        }



        if (facing_right)
        {
            currentSprite.FlipH = false;
        }
        else
        {
            currentSprite.FlipH = true;
        }
    }

    private void move_state()
    {
        motion.y += GRAVITY;

        if (motion.y > MAXFALLSPEED)
        {
            motion.y = MAXFALLSPEED;
        }

        motion.x = motion.Clamped(MAXSPEED).x;

        animationTree.Set("parameters/Idle/blend_position", motion);
        animationTree.Set("parameters/Run/blend_position", motion);
        animationTree.Set("parameters/Attack/blend_position", motion);

        if (Input.IsActionPressed("ui_left"))
        {
            motion.x -= ACCEL;
            facing_right = false;
            animationState.Travel("Run");
            //animPlayer.Play("Run");
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            motion.x += ACCEL;
            facing_right = true;
            animationState.Travel("Run");
            //animPlayer.Play("Run");
        }
        else
        {
            motion = motion.LinearInterpolate(Vector2.Zero, 0.2f);
            animationState.Travel("Idle");
            //animPlayer.Play("Idle");
        }

        if (IsOnFloor())
            // On ne regarde qu'un seul fois et non le maintient de la touche
            if (Input.IsActionJustPressed("ui_jump"))
            {
                motion.y = -JUMPFORCE;
                GD.Print($"motion.y = {motion.y}");
                Console.WriteLine($"motion.y = {motion.y}");
            }

        if (!IsOnFloor())
        {
            if (motion.y < 0)
            {
                animPlayer.Play("jump");
            }
            else if (motion.y > 0)
            {
                animPlayer.Play("fall");
            }
        }

        motion = MoveAndSlide(motion, UP);

        if (Input.IsActionPressed("ui_attack"))
        {
            state = State.ATTACK;
        }
    }

    private void attack_state()
    {
        motion = Vector2.Zero;
        animationState.Travel("Attack");

    }

    public void attack_animation_finished()
    {
        state = State.MOVE;
    }
}
