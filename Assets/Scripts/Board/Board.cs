using System.Linq;
using UnityEngine;

namespace Board
{
    public class Board : MonoBehaviour
    {
        public Texture2D texture;
        public Vector2   textureSize = new(2048, 2048);

        [SerializeField] [Range(0.01f, 1)] private float coverage = 0.01f;

        [SerializeField] private float gpuRefreshRate = 0.1f;
        private                  bool  _modified;
        public static            Board Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var r = GetComponent<Renderer>();

            texture                = new Texture2D((int) textureSize.x, (int) textureSize.y);
            r.material.mainTexture = texture;

            InvokeRepeating(nameof(UpdateGPU), 0.5f, gpuRefreshRate);
        }

        /// <summary>
        ///     Updates the GPU rendering of the board's rendering every <see cref="gpuRefreshRate" />
        ///     The update is done only if the value of <see cref="_modified" /> has been changed
        /// </summary>
        private void UpdateGPU()
        {
            if (!_modified) return;

            texture.Apply();
            _modified = false;
        }

        /// <summary>
        ///     Modifies the board's texture and notifies the GPU to update its rendering
        /// </summary>
        /// <param name="modification"> modification to apply </param>
        public void AddModification(Modification modification)
        {
            Debug.Log($"Adding modification {modification}");
            ModifyTexture(modification);

            _modified = true;
        }

        /// <summary>
        ///     Applies a modification
        /// </summary>
        /// <param name="modification"> Modification to apply </param>
        private void ModifyTexture(Modification modification)
        {
            Color[] colors = GenerateSquare(modification.Color, (int) modification.PenSize);

            ModifyTexture(modification.X,     modification.Y, modification.DestX,
                          modification.DestY, colors,
                          modification.PenSize);
        }

        /// <summary>
        ///     Applies a modification between two points on the texture of the board
        /// </summary>
        /// <param name="x"> x coordinate of the starting point </param>
        /// <param name="y"> y coordinate of the starting point </param>
        /// <param name="destX"> x coordinate of the ending point </param>
        /// <param name="destY"> y coordinate of the ending point </param>
        /// <param name="colors"> color array to apply at each step between the two points </param>
        /// <param name="penSize"> size of the array </param>
        private void ModifyTexture(int   x,     int     y,      float destX,
                                   float destY, Color[] colors, float penSize)
        {
            var size = (int) penSize;

            texture.SetPixels(x, y, size,
                              size, colors);

            // Interpolation
            for (var f = 0.01f; f < 1.00f; f += coverage)
            {
                var lerpX = (int) Mathf.Lerp(destX, x, f);
                var lerpY = (int) Mathf.Lerp(destY, y, f);

                texture.SetPixels(lerpX, lerpY, size,
                                  size, colors);
            }
        }

        /// <summary>
        ///     Generates an array of colors
        /// </summary>
        /// <param name="color"> color used to populate the array </param>
        /// <param name="penSize"> size of the array </param>
        /// <returns> the array created </returns>
        private static Color[] GenerateSquare(Color color, int penSize)
        {
            return Enumerable.Repeat(color, penSize * penSize).ToArray();
        }
    }
}
