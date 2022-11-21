using System;
using Manager;
using UnityEngine;
using Utils;

namespace Board.Tools
{
    /// <inheritdoc />
    public class Marker : WritingTool
    {
        public  Color      color = Color.blue;
        private AppManager _appManager;
        private Board      _board;
        private Color[]    _colors;

        /// <summary>
        ///     Position on the board touched last
        /// </summary>
        private Vector2 _lastTouchPos;

        private Renderer _renderer;

        private Transform _tipTransform;
        private Vector2   _touchPos;
        private Color     boardColor;

        private bool Erasing;

        // Start is called before the first frame update
        private void Start()
        {
            _appManager = GetComponent<AppManager>();
            _board      = _appManager.BoardTransform.GetComponentInChildren<Board>();
            _renderer   = _appManager.BoardTransform.GetComponentInChildren<Renderer>();

            TouchedLast = false;
            Erasing     = false;
            boardColor  = Board.Instance.texture.GetPixel(0, 0);
        }

        public void TryDraw(RaycastHit touch)
        {
            Tools.Instance.Modified = Draw(touch) || Tools.Instance.Modified;
        }

        public void StopDraw()
        {
            TouchedLast = false;
        }

        /// <summary>
        ///     We check if we are in the boundaries of the board
        /// </summary>
        /// <param name="x"> The x coordinate of the point where the marker touches the board </param>
        /// <param name="y"> The y coordinate of the point where the marker touches the board </param>
        /// <returns>
        ///     <see langword="true" /> if we are in the boundaries of the board
        ///     <see langword="false" /> otherwise
        /// </returns>
        private bool InBound(int x, int y)
        {
            return x >= 0 && x <= _board.textureSize.x && y >= 0 && y <= _board.textureSize.y;
        }

        /// <summary>
        ///     Check if we are touching the board
        ///     if we are not, return <see langword="false" />
        ///     otherwise draw while touching (interpolation used to avoid drawing only dots) and return <see langword="true" />
        ///     we send the modifications at each frame
        /// </summary>
        private bool Draw(RaycastHit touch)
        {
            // We check if we are touching the board with the marker
            if (touch.transform.CompareTag("Board"))
            {
                _board    ??= touch.transform.GetComponent<Board>();
                _touchPos =   new Vector2(touch.textureCoord.x, touch.textureCoord.y);

                var x = (int) (_touchPos.x * _board.textureSize.x - penSize / 2);
                var y = (int) (_touchPos.y * _board.textureSize.y - penSize / 2);

                // If we are touching the board and in its boundaries, then we draw
                if (!InBound(x, y))
                    return false;

                PrintVar.print(10, $"Inbound: {touch.textureCoord}");

                if (TouchedLast)
                {
                    if (Vector2.Distance(new Vector2(x, y), _lastTouchPos) < 0.01f)
                        return false;

                    try
                    {
                        ModifyTexture(x,               y,       _lastTouchPos.x,
                                      _lastTouchPos.y, _colors, penSize);
                    } catch (ArgumentException)
                    {
                        TouchedLast = false;
                    } finally
                    {
                        SendModification(x, y);

                        if (!TouchedLast)
                            _board = null;
                    }
                }
                else
                    _colors = GenerateShape();

                _lastTouchPos = new Vector2(x, y);
                TouchedLast   = true;

                return true;
            }

            _board      = null;
            TouchedLast = false;

            return false;
        }

        public void Eraser(bool active = true)
        {
            if (active == Erasing) return;

            penSize = active ? penSize * 5 : penSize / 5;
            color   = active ? boardColor : Color.blue;

            Erasing = active;
        }

        /// <summary>
        ///     Generates a color array
        /// </summary>
        /// <returns> color array </returns>
        private Color[] GenerateShape()
        {
            return Tools.GenerateSquare(color, penSize);

            // TODO generate shape depending on selected one
        }

        /// <summary>
        ///     Sends a modification to the other players
        /// </summary>
        /// <param name="x"> x coordinate of the starting point of the modification </param>
        /// <param name="y"> y coordinate of the starting point of the modification </param>
        private void SendModification(int x, int y)
        {
            new Modification(x, y, _lastTouchPos.x,
                             _lastTouchPos.y, color,
                             penSize)
                .Send(EventCode.Marker);
        }

        /// <summary>
        ///     Applies a modification between two points on the texture of the board
        /// </summary>
        /// <param name="x"> x coordinate of the starting point </param>
        /// <param name="y"> y coordinate of the starting point </param>
        /// <param name="destX"> x coordinate of the ending point </param>
        /// <param name="destY"> y coordinate of the ending point </param>
        /// <param name="colors"> color array to apply at each step between the two points </param>
        /// <param name="size"> size of the array </param>
        private static void ModifyTexture(int   x,     int     y,      float destX,
                                          float destY, Color[] colors, float size)
        {
            var castSize = (int) size;

            Board.Instance.texture.SetPixels(x, y, castSize,
                                             castSize, colors);

            Board.Instance.texture.SetPixels((int) destX, (int) destY, castSize,
                                             castSize, colors);

            // Interpolation
            for (var f = 0.01f; f < 1.00f; f += Tools.Instance.coverage)
            {
                var lerpX = (int) Mathf.Lerp(destX, x, f);
                var lerpY = (int) Mathf.Lerp(destY, y, f);

                Board.Instance.texture.SetPixels(lerpX, lerpY, castSize,
                                                 castSize, colors);
            }
        }

        /// <summary>
        ///     Applies a modification
        /// </summary>
        /// <param name="modification"> Modification to apply </param>
        private static void ModifyTexture(Modification modification)
        {
            Color[] colors = Tools.GenerateSquare(modification.Color, modification.PenSize);

            ModifyTexture(modification.X,     modification.Y, modification.DestX,
                          modification.DestY, colors,
                          modification.PenSize);
        }

        /// <summary>
        ///     Modifies the board's texture and notifies the GPU to update its rendering
        /// </summary>
        /// <param name="modification"> modification to apply </param>
        public static void AddModification(Modification modification)
        {
            ModifyTexture(modification);
            Tools.Instance.Modified = true;
        }

        // TODO add other shapes ?
    }
}
