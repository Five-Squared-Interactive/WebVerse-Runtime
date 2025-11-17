using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a Terrain Entity Modification.
    /// </summary>
    public class TerrainEntityModification
    {
        /// <summary>
        /// Enumeration for a Terrain Entity Operation.
        /// Unset: no operation.
        /// Dig: dig operation.
        /// Build: build operation.
        /// </summary>
        public enum TerrainEntityOperation
        {
            Unset = 0,
            Dig = 1,
            Build = 2
        }

        /// <summary>
        /// Operation to be performed to the terrain.
        /// </summary>
        public TerrainEntityOperation operation;

        /// <summary>
        /// Position of the modification.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Brush type to be used for the terrain modification.
        /// </summary>
        public TerrainEntityBrushType brushType;

        /// <summary>
        /// Layer on which modification is to be made to the terrain.
        /// </summary>
        public int layer;

        /// <summary>
        /// Size of the modification.
        /// </summary>
        public float size;

        /// <summary>
        /// Constructor for a Terrain Entity Operation.
        /// </summary>
        /// <param name="operation">Operation to be performed on the terrain.</param>
        /// <param name="position">Position of the operation.</param>
        /// <param name="brushType">Brush type to be used for the terrain modification.</param>
        /// <param name="layer">Layer on which modification is to be made to the terrain.</param>
        /// <param name="size">Size of the hole, in meters.</param>
        public TerrainEntityModification(TerrainEntityOperation operation,
            Vector3 position, TerrainEntityBrushType brushType, int layer, float size)
        {
            this.operation = operation;
            this.position = position;
            this.brushType = brushType;
            this.layer = layer;
            this.size = size;
        }
    }
}