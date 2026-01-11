// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.Materials
{
    /// <summary>
    /// Class for the material manager.
    /// </summary>
    public class MaterialManager : BaseManager
    {
        /// <summary>
        /// Entity highlight material.
        /// </summary>
        public static Material HighlightMaterial
        {
            get
            {
                return instance.highlightMaterial;
            }
        }

        /// <summary>
        /// Entity preview material.
        /// </summary>
        public static Material PreviewMaterial
        {
            get
            {
                return instance.previewMaterial;
            }
        }

        /// <summary>
        /// Entity highlight material.
        /// </summary>
        private Material highlightMaterial;

        /// <summary>
        /// Entity preview material.
        /// </summary>
        private Material previewMaterial;

        /// <summary>
        /// Instance of the material manager.
        /// </summary>
        private static MaterialManager instance;

        /// <summary>
        /// Initialize the material manager.
        /// </summary>
        /// <param name="highlightMaterial">Entity highlight material.</param>
        public void Initialize(Material highlightMaterial, Material previewMaterial)
        {
            instance = this;
            this.highlightMaterial = highlightMaterial;
            this.previewMaterial = previewMaterial;
        }
    }
}