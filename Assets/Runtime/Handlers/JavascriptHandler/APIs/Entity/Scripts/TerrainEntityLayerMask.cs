// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a terrain entity layer mask.
    /// </summary>
    public class TerrainEntityLayerMask
    {
        /// <summary>
        /// Heights for the layer mask.
        /// </summary>
        public float[,] heights;

        /// <summary>
        /// Terrain entity layer mask.
        /// </summary>
        /// <param name="heights">Heights for the layer mask.</param>
        public TerrainEntityLayerMask(float[][] heights)
        {
            if (heights == null || heights[0] == null)
            {
                Logging.LogWarning("[TerrainEntityLayerMask] Invalid heights array.");
                return;
            }

            float[,] list = new float[heights.Length, heights[0].Length];
            for (int i = 0; i < heights.Length; i++)
            {
                for (int j = 0; j < heights[0].Length; j++)
                {
                    list[i, j] = heights[i][j];
                }
            }
            this.heights = list;
        }

        /// <summary>
        /// Terrain entity layer mask.
        /// </summary>
        /// <param name="length">Length of the layer mask.</param>
        /// <param name="width">Width of the layer mask.</param>
        public TerrainEntityLayerMask(int length, int width)
        {
            heights = new float[length, width];
        }

        /// <summary>
        /// Terrain entity layer mask.
        /// </summary>
        /// <param name="heights">Heights for the layer mask.</param>
        public TerrainEntityLayerMask(float[,] heights)
        {
            if (heights == null)
            {
                Logging.LogWarning("[TerrainEntityLayerMask] Invalid heights array.");
                return;
            }

            this.heights = heights;
        }

        /// <summary>
        /// Set the height at a given coordinate.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="height">Height.</param>
        public void SetHeight(int x, int y, float height)
        {
            if (heights.GetLength(0) > x && heights.GetLength(1) > y && x > -1 && y > -1)
            {
                heights[x, y] = height;
            }
            else
            {
                Logging.LogWarning("[TerrainEntityLayerMask->SetHeight] Invalid coordinate.");
            }
        }
    }
}