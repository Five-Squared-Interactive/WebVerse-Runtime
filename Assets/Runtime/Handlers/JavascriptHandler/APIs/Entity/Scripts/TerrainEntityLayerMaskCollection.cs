// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a terrain entity layer mask collection.
    /// </summary>
    public class TerrainEntityLayerMaskCollection
    {
        private List<TerrainEntityLayerMask> layerMaskCollection;

        /// <summary>
        /// Terrain Entity Layer Mask Collection.
        /// </summary>
        public TerrainEntityLayerMaskCollection()
        {
            layerMaskCollection = new List<TerrainEntityLayerMask>();
        }

        /// <summary>
        /// Add a layer mask.
        /// </summary>
        /// <param name="mask">Mask to add.</param>
        public void AddLayerMask(TerrainEntityLayerMask mask)
        {
            layerMaskCollection.Add(mask);
        }

        /// <summary>
        /// Get layer masks.
        /// </summary>
        /// <returns>Array of layer masks.</returns>
        public TerrainEntityLayerMask[] GetLayerMasks()
        {
            return layerMaskCollection.ToArray();
        }

        /// <summary>
        /// Get layer masks as an array of 2d float arrays.
        /// </summary>
        /// <returns>Layer masks as an array of 2d float arrays.</returns>
        public float[][,] ToFloatArrays()
        {
            List<float[,]> floats = new List<float[,]>();
            for (int i = 0; i < layerMaskCollection.Count; i++)
            {
                floats.Add(layerMaskCollection[i].heights);
            }
            return floats.ToArray();
        }
    }
}