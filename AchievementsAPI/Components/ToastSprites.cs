using System.Collections;
using System.Collections.Generic;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace AchievementsAPI;

/// <summary>
/// Generates and caches the procedural sprites used by the toast UI, since the
/// asset bundles cannot be edited.
/// </summary>
public static class ToastSprites
{
    private const int Size = 128;
    private const float CornerRadius = 48f;
    private const float CornerRadiusSmall = 14f;
    private const float CornerRadiusTiny = 6f;
    private const float SquircleRadius = 28f;
    private const float CardRadius = 26f;
    private const int CardScale = 4;

    private static Sprite? _circle;
    private static Sprite? _roundedRect;
    private static Sprite? _roundedRectSmall;
    private static Sprite? _roundedRectTiny;
    private static Sprite? _squircle;
    private static Sprite? _gear;
    private static Sprite? _card;
    private static Sprite? _cardRim;
    private static Sprite? _outlineBox;
    private static Sprite? _panelBox;
    // Plain Sprite (no `?`): the project has no nullable annotations context,
    // and a null here is a true null before first use anyway
    private static Sprite _wikiPanel;
    private static Sprite? _outlineBoxSmall;
    private static Sprite? _buttonBox;
    private static Sprite? _bubbleTail;
    private static Sprite? _panelBoxSmall;
    private static Sprite? _closeX;

    /// <summary>
    /// A 512x512 (ppu 400) anti-aliased filled circle. Rendered at 4x like the
    /// card sprites so rings/discs stay crisp; the UI size is unchanged.
    /// </summary>
    public static Sprite Circle => _circle ??= CreateCircle();

    /// <summary>
    /// A 128x128 anti-aliased rounded rectangle (corner radius 48) with a 9-slice
    /// border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite RoundedRect => _roundedRect ??= CreateRoundedRect(CornerRadius, "AchToastRoundedRect");

    /// <summary>
    /// A 128x128 anti-aliased rounded rectangle (corner radius 14) with a 9-slice
    /// border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite RoundedRectSmall => _roundedRectSmall ??= CreateRoundedRect(CornerRadiusSmall, "AchToastRoundedRectSmall");

    /// <summary>
    /// A 128x128 anti-aliased rounded rectangle (corner radius 6) with a 9-slice
    /// border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite RoundedRectTiny => _roundedRectTiny ??= CreateRoundedRect(CornerRadiusTiny, "AchToastRoundedRectTiny");

    /// <summary>
    /// A 128x128 anti-aliased squircle (superellipse corners, radius 28) with a
    /// 9-slice border, suitable for <see cref="UnityEngine.UI.Image.Type.Sliced"/>.
    /// </summary>
    public static Sprite Squircle => _squircle ??= CreateRoundedRect(SquircleRadius, "AchToastSquircle", 4f);

    /// <summary>
    /// A 128x128 anti-aliased gear with 8 teeth and an 18px center hole.
    /// </summary>
    public static Sprite Gear => _gear ??= CreateGear();

    /// <summary>
    /// A 128x128 framed card matching the game's achievement frame: rounded
    /// corners (radius 26), bevelled rim, dark fill, 9-slice border of 28.
    /// </summary>
    public static Sprite Card => _card ??= CreateCard(false, "AchToastCard");

    /// <summary>
    /// The rim/bevel bands of <see cref="Card"/> as a grayscale sprite (fill and
    /// outer edge transparent), so it can be tinted and layered over Card.
    /// </summary>
    public static Sprite CardRim => _cardRim ??= CreateCard(true, "AchToastCardRim");

    /// <summary>
    /// Transparent fill with a 3px white ring inset ~3px, edged by thin black
    /// rings on both sides, like the game's settings boxes (radius 22).
    /// </summary>
    public static Sprite OutlineBox => _outlineBox ??= CreateOutlineBox(22f, new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0f), "AchMenuOutlineBox");

    /// <summary>
    /// <see cref="OutlineBox"/> with an opaque black fill inside the ring.
    /// </summary>
    public static Sprite PanelBox => _panelBox ??= CreateOutlineBox(22f, new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 255), "AchMenuPanelBox");

    /// <summary>
    /// A 976x631 framed background loaded from <c>AchievementsAPI.Resources.Menu.png</c>.
    /// It is 9-sliced with a 48px border so the corners and frame retain their
    /// proportions when stretched, for the menu window.
    /// </summary>
    public static Sprite WikiPanel => _wikiPanel ??= CreateWikiPanel();

    /// <summary>
    /// <see cref="OutlineBox"/> with corner radius 14, for pill tabs and buttons.
    /// </summary>
    public static Sprite OutlineBoxSmall => _outlineBoxSmall ??= CreateOutlineBox(14f, new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0f), "AchMenuOutlineBoxSmall", 20f);

    /// <summary>
    /// A solid #3D4040 rounded box with no outline, for buttons, pills and stat boxes.
    /// </summary>
    public static Sprite ButtonBox => _buttonBox ??= CreateSolidRoundedBox(14f, new Color32(61, 64, 64, 255), "AchMenuButtonBox");

    /// <summary>
    /// A solid speech-bubble tail in the <see cref="ButtonBox"/> color. The flat
    /// attachment edge is the right side of the texture; it tapers to a soft,
    /// centered tip on the left. Mirror via localScale.x to point right.
    /// </summary>
    public static Sprite BubbleTail => _bubbleTail ??= CreateBubbleTail();

    /// <summary>
    /// <see cref="PanelBox"/> with corner radius 14.
    /// </summary>
    public static Sprite PanelBoxSmall => _panelBoxSmall ??= CreateOutlineBox(14f, new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 255), "AchMenuPanelBoxSmall", 20f);

    /// <summary>
    /// White disc with a black ring and a bold black X, matching the game's
    /// close button.
    /// </summary>
    public static Sprite CloseX => _closeX ??= CreateCloseX();

    /// <summary>
    /// Builds every lazily created sprite, one per yielded frame, so the first
    /// toast or menu open does not pay the generation cost mid-game.
    /// </summary>
    internal static IEnumerator CoPrewarm()
    {
        _ = Circle; yield return null;
        _ = Card; yield return null;
        _ = CardRim; yield return null;
        _ = OutlineBox; yield return null;
        _ = PanelBox; yield return null;
        _ = OutlineBoxSmall; yield return null;
        _ = PanelBoxSmall; yield return null;
        _ = RoundedRect; yield return null;
        _ = RoundedRectSmall; yield return null;
        _ = RoundedRectTiny; yield return null;
        _ = BubbleTail; yield return null;
        _ = Squircle; yield return null;
        _ = Gear; yield return null;
        _ = CloseX; yield return null;
        _ = WikiPanel; yield return null;
        _ = MenuUi.AchievementIcon;
        _ = Assets.LoadSprite("ExampleIcon");
    }

    private static readonly Dictionary<int, Sprite> _trimmed = new();

    /// <summary>
    /// Returns a copy of <paramref name="sprite"/> cropped to its opaque bounds so icons
    /// with different transparent padding render at a consistent size. Falls back to the
    /// original sprite when the texture is not readable. Results are cached per sprite.
    /// </summary>
    public static Sprite TrimTransparent(Sprite sprite)
    {
        SpriteTools.PreserveSprite(sprite);
        if (_trimmed.TryGetValue(sprite.GetInstanceID(), out var cached)) return cached;

        var result = sprite;
        try
        {
            // The trimmed sprite shares the source texture; keep it alive
            // across Resources.UnloadUnusedAssets on both paths
            sprite.texture.DontUnload();
            var pixels = sprite.texture.GetPixels32();
            var texW = sprite.texture.width;
            var x0 = Mathf.RoundToInt(sprite.rect.x);
            var y0 = Mathf.RoundToInt(sprite.rect.y);
            var x1 = x0 + Mathf.RoundToInt(sprite.rect.width);
            var y1 = y0 + Mathf.RoundToInt(sprite.rect.height);

            var minX = x1; var minY = y1; var maxX = x0; var maxY = y0;
            var found = false;
            for (var y = y0; y < y1; y++)
            {
                for (var x = x0; x < x1; x++)
                {
                    if (pixels[y * texW + x].a <= 10) continue;
                    found = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (found && (minX > x0 || minY > y0 || maxX < x1 - 1 || maxY < y1 - 1))
            {
                var trimmed = Sprite.Create(sprite.texture,
                    new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1),
                    new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit);
                trimmed.name = $"{sprite.name}_trimmed";
                SpriteTools.PreserveSprite(trimmed);
                result = trimmed;
            }
        }
        catch (System.Exception)
        {
            // Texture not readable; keep the original sprite
        }

        _trimmed[sprite.GetInstanceID()] = result;
        return result;
    }

    private static Sprite CreateCircle()
    {
        // Rendered at CardScale like CreateCard; pixelsPerUnit scales with it,
        // so the visual size is unchanged
        var size = Size * CardScale;
        var s = (float) CardScale;
        var center = (size - 1) / 2f;
        var tex = CreateTexture((x, y) =>
        {
            var d = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
            return Mathf.Clamp01(size / 2f - d);
        }, "AchToastCircle", size, mipmaps: true);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f * s);
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateCard(bool rimOnly, string name)
    {
        // Rendered at 4x so the bevels stay crisp on screen; pixelsPerUnit and
        // the 9-slice border scale with it, so the visual size is unchanged.
        var size = Size * CardScale;
        var s = (float) CardScale;
        var radius = CardRadius * s;
        var tex = CreateTexture((x, y) =>
        {
            // Signed inset inside the rounded rect (negative = outside)
            var px = Mathf.Clamp(x, radius, size - radius);
            var py = Mathf.Clamp(y, radius, size - radius);
            var d = Mathf.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
            var inset = radius - d;
            var alpha = Mathf.Clamp01(inset / s);
            Color color;
            if (inset < 2f * s) color = rimOnly ? new Color(0f, 0f, 0f, 0f) : new Color32(0, 0, 0, 255);
            else if (inset < 3f * s) color = rimOnly ? Color.white : new Color32(146, 157, 165, 255);
            else if (inset < 12f * s) color = rimOnly
                ? Color.Lerp(new Color(0.55f, 0.55f, 0.55f), Color.white, y / (size - 1f))
                : Color.Lerp(new Color32(69, 74, 79, 255), new Color32(112, 114, 118, 255), y / (size - 1f));
            else if (inset < 13f * s) color = rimOnly ? Color.white : new Color32(160, 172, 181, 255);
            else if (inset < 16f * s) color = rimOnly ? new Color(0f, 0f, 0f, 0f) : new Color32(0, 0, 0, 255);
            else color = rimOnly ? new Color(0f, 0f, 0f, 0f) : new Color32(41, 40, 41, 255);
            color.a *= alpha;
            return color;
        }, name, size);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f * s,
            0, SpriteMeshType.FullRect, Vector4.one * 28f * s);
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateWikiPanel()
    {
        var tex = Assets.LoadTexture("Menu");
        tex.DontUnload();

        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(48f, 48f, 48f, 48f));
        sprite.name = "AchMenuWikiPanel";
        sprite.DontUnload();
        return sprite;
    }

    // White 3px stroke inset ~3px with 1.5px black rings on either side;
    // rendered at CardScale so the rings stay crisp. The interior band
    // (inset >= 7.5px) is filled with a vertical lerp from `bottom` at y 0 to
    // `top` at the texture top (y grows upward in the texture); transparent
    // colors give the outline-only variant.
    private static Sprite CreateOutlineBox(float radius, Color top, Color bottom, string name, float border = 32f)
    {
        var size = Size * CardScale;
        var s = (float) CardScale;
        var r = radius * s;
        var maxY = size - 1f;
        var tex = CreateTexture((x, y) =>
        {
            var px = Mathf.Clamp(x, r, size - r);
            var py = Mathf.Clamp(y, r, size - r);
            var d = Mathf.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
            var inset = r - d;
            var alpha = Mathf.Clamp01(inset / s);
            Color color;
            if (inset < 3f * s) color = new Color32(0, 0, 0, 255);
            else if (inset < 6f * s) color = Color.white;
            else if (inset < 7.5f * s) color = new Color32(0, 0, 0, 255);
            else color = Color.Lerp(bottom, top, y / maxY);
            color.a *= alpha;
            return color;
        }, name, size);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f * s,
            0, SpriteMeshType.FullRect, Vector4.one * border * s);
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateCloseX()
    {
        var tex = CreateTexture((x, y) =>
        {
            var dx = x - 63.5f;
            var dy = y - 63.5f;
            var d = Mathf.Sqrt(dx * dx + dy * dy);
            var alpha = Mathf.Clamp01(62f - d);
            var black = Mathf.Clamp01(d - 56f);
            // Two diagonal bars, halfwidth 7px, halflength 32px
            var u = Mathf.Abs(dx + dy) / 1.4142135f;
            var v = Mathf.Abs(dx - dy) / 1.4142135f;
            var bar = Mathf.Max(
                Mathf.Clamp01(7f - u) * Mathf.Clamp01(32f - v),
                Mathf.Clamp01(7f - v) * Mathf.Clamp01(32f - u));
            black = Mathf.Max(black, bar);
            var c = Color.Lerp(Color.white, Color.black, black);
            c.a = alpha;
            return c;
        }, "AchMenuCloseX", mipmaps: true);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateGear()
    {
        var tex = CreateTexture((x, y) =>
        {
            var dx = x - 63.5f;
            var dy = y - 63.5f;
            var d = Mathf.Sqrt(dx * dx + dy * dy);
            var angle = Mathf.Atan2(dy, dx);
            var tooth = 0.5f + 0.5f * Mathf.Cos(angle * 8f);
            var outer = Mathf.Lerp(44f, 56f, Mathf.Clamp01((tooth - 0.5f) * 6f));
            return Mathf.Clamp01(outer - d) * Mathf.Clamp01(d - 18f);
        }, "AchToastGear", mipmaps: true);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateRoundedRect(float radius, string name, float exponent = 2f)
    {
        var tex = CreateTexture((x, y) =>
        {
            // Distance to the inner rect inset by the corner radius, measured
            // with a superellipse norm (exponent 2 = circle, higher = squarer)
            var px = Mathf.Clamp(x, radius, Size - radius);
            var py = Mathf.Clamp(y, radius, Size - radius);
            var d = Mathf.Pow(Mathf.Pow(Mathf.Abs(x - px), exponent) + Mathf.Pow(Mathf.Abs(y - py), exponent), 1f / exponent);
            return Mathf.Clamp01(radius - d);
        }, name);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sprite.DontUnload();
        return sprite;
    }

    private static Sprite CreateSolidRoundedBox(float radius, Color color, string name)
    {
        var tex = CreateTexture((x, y) =>
        {
            var px = Mathf.Clamp(x, radius, Size - radius);
            var py = Mathf.Clamp(y, radius, Size - radius);
            var d = Mathf.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
            var result = color;
            result.a *= Mathf.Clamp01(radius - d);
            return result;
        }, name);
        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sprite.DontUnload();
        return sprite;
    }

    // Symmetric rounded speech-bubble tail. The width profile uses smoothstep
    // so the flat attachment edge (right side) is vertical and blends into the
    // bubble, while the tip on the left is softly rounded instead of sharp.
    // Mipmapped so the downscaled tail stays smooth.
    private static Sprite CreateBubbleTail()
    {
        const float baseHalf = Size * 0.26f;
        var tex = CreateTexture((x, y) =>
        {
            var t = x / (Size - 1f);
            var halfWidth = baseHalf * Mathf.SmoothStep(0f, 1f, t);
            var inside = halfWidth - Mathf.Abs(y - (Size - 1) / 2f);
            return new Color(61f / 255f, 64f / 255f, 64f / 255f, Mathf.Clamp01(inside + 0.5f));
        }, "AchMenuBubbleTail", mipmaps: true);

        tex.DontUnload();
        var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(1f, 0.5f), 100f);
        sprite.DontUnload();
        return sprite;
    }

    private static Texture2D CreateTexture(System.Func<int, int, float> alphaAt, string name, int size = Size, bool mipmaps = false)
        => CreateTexture((x, y) => new Color(1f, 1f, 1f, alphaAt(x, y)), name, size, mipmaps);

    private static Texture2D CreateTexture(System.Func<int, int, Color> colorAt, string name, int size = Size, bool mipmaps = false)
    {
        // Mipmaps are only used for non-sliced sprites; sliced sprites are
        // stretched anisotropically and trilinear minification bleeds the ring
        // into the side slices.
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, mipmaps)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = mipmaps ? FilterMode.Trilinear : FilterMode.Bilinear,
            name = name
        };

        var pixels = new Color32[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                pixels[y * size + x] = colorAt(x, y);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(mipmaps, true);
        return tex;
    }
}
