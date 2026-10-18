using UnityEngine;

namespace AchievementsAPI;

// Rewired (controller) + keyboard menu input, polled once per frame by the
// achievements menu. Action ids are the game's own menu map.
internal static class MenuInput
{
    private const int MenuVertical = 10;
    private const int MenuHorizontal = 9;
    private const int MenuConfirm = 11;
    private const int MenuCancel = 12;
    private const int MenuUp = 25;
    private const int MenuDown = 26;
    private const int MenuLeft = 28;
    private const int MenuRight = 27;
    private const int MenuLB = 52;
    private const int MenuRB = 53;
    private const int MenuLT = 35;
    private const int MenuRT = 34;

    private const float Threshold = 0.5f;
    private const float RepeatDelay = 0.4f;
    private const float RepeatRate = 0.12f;

    internal static bool Up { get; private set; }
    internal static bool Down { get; private set; }
    internal static bool Left { get; private set; }
    internal static bool Right { get; private set; }
    internal static bool Confirm { get; private set; }
    internal static bool Cancel { get; private set; }
    internal static bool PrevCategory { get; private set; }
    internal static bool NextCategory { get; private set; }
    internal static bool PrevFilter { get; private set; }
    internal static bool NextFilter { get; private set; }

    internal static bool AnyDirection => Up || Down || Left || Right;

    private static Rewired.Player _player;
    private static bool _rewiredFailed;

    private static Rewired.Player GetPlayer()
    {
        if (_rewiredFailed) return null;
        if (_player == null)
        {
            try { _player = Rewired.ReInput.players.GetPlayer(0); }
            catch { _rewiredFailed = true; }
        }
        return _player;
    }

    private static bool MouseDriven(Rewired.Player player)
    {
        try
        {
            var last = player.controllers.GetLastActiveController();
            return last != null && last.type == Rewired.ControllerType.Mouse;
        }
        catch
        {
            return false;
        }
    }

    private static readonly float[] _heldSince = { -1f, -1f, -1f, -1f };
    private static readonly float[] _lastFire = { 0f, 0f, 0f, 0f };

    private static bool Direction(int i, bool held)
    {
        var now = Time.unscaledTime;
        if (!held)
        {
            _heldSince[i] = -1f;
            return false;
        }
        if (_heldSince[i] < 0f)
        {
            _heldSince[i] = now;
            _lastFire[i] = now;
            return true;
        }
        if (now - _heldSince[i] >= RepeatDelay && now - _lastFire[i] >= RepeatRate)
        {
            _lastFire[i] = now;
            return true;
        }
        return false;
    }

    internal static void Poll()
    {
        var vert = 0f;
        var horz = 0f;
        var buttonUp = false;
        var buttonDown = false;
        var buttonLeft = false;
        var buttonRight = false;
        var confirm = false;
        var cancel = false;
        var prevCategory = false;
        var nextCategory = false;
        var prevFilter = false;
        var nextFilter = false;

        var player = GetPlayer();
        if (player != null && !MouseDriven(player))
        {
            vert = player.GetAxis(MenuVertical);
            horz = player.GetAxis(MenuHorizontal);
            buttonUp = player.GetButtonDown(MenuUp);
            buttonDown = player.GetButtonDown(MenuDown);
            buttonLeft = player.GetButtonDown(MenuLeft);
            buttonRight = player.GetButtonDown(MenuRight);
            confirm = player.GetButtonDown(MenuConfirm);
            cancel = player.GetButtonDown(MenuCancel);
            prevCategory = player.GetButtonDown(MenuLB);
            nextCategory = player.GetButtonDown(MenuRB);
            prevFilter = player.GetButtonDown(MenuLT);
            nextFilter = player.GetButtonDown(MenuRT);
        }

        Up = Direction(0, vert > Threshold || Input.GetKey(KeyCode.UpArrow)) || buttonUp;
        Down = Direction(1, vert < -Threshold || Input.GetKey(KeyCode.DownArrow)) || buttonDown;
        Left = Direction(2, horz < -Threshold || Input.GetKey(KeyCode.LeftArrow)) || buttonLeft;
        Right = Direction(3, horz > Threshold || Input.GetKey(KeyCode.RightArrow)) || buttonRight;

        Confirm = confirm || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space);
        Cancel = cancel || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(3);
        PrevCategory = prevCategory || Input.GetKeyDown(KeyCode.Q);
        NextCategory = nextCategory || Input.GetKeyDown(KeyCode.E);

        var tab = Input.GetKeyDown(KeyCode.Tab);
        var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        NextFilter = nextFilter || (tab && !shift);
        PrevFilter = prevFilter || (tab && shift);
    }
}
