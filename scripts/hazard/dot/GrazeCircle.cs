using Godot;

public partial class GrazeCircle : Sprite2D
{
    private Tween _grazeTween;
    private AudioStreamPlayer2D _grazeSnd;
    private bool _isGrazed = false;
    private const float EXIT_DELAY = 0.05f;
    private const float FADE_TIME = 0.25f;
    private static readonly Color Transparent = new(1, 1, 1, 0); // 透明
    private static readonly Color Opaque = new(1, 1, 1, 1);      // 不透明

    public override void _Ready()
    {
        // 获取子节点 Area2D
        var grazeArea = GetNode<Area2D>("Area2D");
        // 连接信号
        grazeArea.BodyEntered += OnGrazeCircleEntered;
        grazeArea.BodyExited  += OnGrazeCircleExited;
        _grazeSnd = GetNode<AudioStreamPlayer2D>("GrazeSnd");
		// 不透明设置为 0
		Modulate = Transparent;
    }

    private void OnGrazeCircleEntered(Node2D body)
    {
        if (body is PlayerVelocity)
        {
            _isGrazed = true;
            _grazeSnd.Play();  // 播放音效
            PlayGhostEffect();
        }
    }

    private void OnGrazeCircleExited(Node2D body)
    {
        if (body is PlayerVelocity)
        {
           _isGrazed = false;
           // 等待 0.05 秒后判断
           GetTree().CreateTimer(EXIT_DELAY).Timeout += () =>
           {
               if (!_isGrazed) ExitGhostEffect();
           }; 
        }
    }

    private void PlayGhostEffect()
    {
        // 停止之前的动画（如有）
        _grazeTween?.Kill();

        // 立即设置透明度为 0
        Modulate = Opaque;
    }

    private void ExitGhostEffect()
    {
        _grazeTween?.Kill();
        _grazeTween = CreateTween();
        _grazeTween.TweenProperty(this, "modulate", Transparent, FADE_TIME);
    }
}