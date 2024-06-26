// Copyright (c) 2019-2024 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.VEML
{
    /// <summary>
    /// Class for VEML Helper Utilities.
    /// </summary>
    public class VEMLUtilities
    {
        /// <summary>
        /// Convert the schema instance from version 1.1 to the current schema (version 1.2).
        /// </summary>
        /// <param name="inputVEML">Input VEML instance.</param>
        /// <returns>Current schema version for the input VEML instance.</returns>
        public static Schema.V1_2.veml ConvertFromV1_1(Schema.V1_1.veml inputVEML)
        {
            Schema.V1_2.veml outputVEML = new Schema.V1_2.veml();
            if (inputVEML.metadata != null)
            {
                // Set up metadata.
                outputVEML.metadata = new Schema.V1_2.vemlMetadata();

                // Assign scripts.
                outputVEML.metadata.script = inputVEML.metadata.script;

                // Assign title.
                outputVEML.metadata.title = inputVEML.metadata.title;

                // Assign input events.
                List<Schema.V1_2.inputevent> outputVEMLInputEvents = new List<Schema.V1_2.inputevent>();
                if (inputVEML.metadata.inputevent != null)
                {
                    foreach (Schema.V1_1.inputevent inputEvent in inputVEML.metadata.inputevent)
                    {
                        Schema.V1_2.inputevent outputVEMLInputEvent = new Schema.V1_2.inputevent();
                        outputVEMLInputEvent.@event = inputEvent.@event;
                        outputVEMLInputEvent.input = inputEvent.input;
                        outputVEMLInputEvents.Add(outputVEMLInputEvent);
                    }
                    outputVEML.metadata.inputevent = outputVEMLInputEvents.ToArray();
                }

                // Assign synchronization services.
                List<Schema.V1_2.synchronizationservice> outputVEMLSynchronizationServices
                    = new List<Schema.V1_2.synchronizationservice>();
                if (inputVEML.metadata.synchronizationservice != null)
                {
                    foreach (Schema.V1_1.synchronizationservice synchronizationService
                        in inputVEML.metadata.synchronizationservice)
                    {
                        Schema.V1_2.synchronizationservice outputVEMLSynchronizationService
                            = new Schema.V1_2.synchronizationservice();
                        outputVEMLSynchronizationService.id = synchronizationService.id;
                        outputVEMLSynchronizationService.address = synchronizationService.address;
                        outputVEMLSynchronizationService.session = synchronizationService.session;
                        outputVEMLSynchronizationService.type = synchronizationService.type;
                    }
                    outputVEML.metadata.synchronizationservice = outputVEMLSynchronizationServices.ToArray();
                }
            }

            if (inputVEML.environment != null)
            {
                // Set up environment.
                outputVEML.environment = new Schema.V1_2.vemlEnvironment();

                // Assign background.
                if (inputVEML.environment.background != null)
                {
                    outputVEML.environment.background = new Schema.V1_2.background();
                    outputVEML.environment.background.Item = inputVEML.environment.background.Item;
                    switch (inputVEML.environment.background.ItemElementName)
                    {
                        case Schema.V1_1.ItemChoiceType.panorama:
                            outputVEML.environment.background.ItemElementName = Schema.V1_2.ItemChoiceType.panorama;
                            break;

                        case Schema.V1_1.ItemChoiceType.color:
                        default:
                            outputVEML.environment.background.ItemElementName = Schema.V1_2.ItemChoiceType.color;
                            break;
                    }
                }

                // Set up entities.
                List<Schema.V1_2.entity> outputVEMLEntities = new List<Schema.V1_2.entity>();
                if (inputVEML.environment.entity != null)
                {
                    foreach (Schema.V1_1.entity e in inputVEML.environment.entity)
                    {
                        Queue<KeyValuePair<Schema.V1_1.entity, Schema.V1_2.entity>> entityQueue
                            = new Queue<KeyValuePair<Schema.V1_1.entity, Schema.V1_2.entity>>();
                        entityQueue.Enqueue(new KeyValuePair<Schema.V1_1.entity, Schema.V1_2.entity>(e, null));
                        while (entityQueue.Count > 0)
                        {
                            KeyValuePair<Schema.V1_1.entity, Schema.V1_2.entity> item = entityQueue.Dequeue();

                            Schema.V1_2.entity outputVEMLEntity = ConvertEntityFromV1_1(item.Key);

                            if (item.Key.entity1 != null)
                            {
                                foreach (Schema.V1_1.entity child in item.Key.entity1)
                                {
                                    entityQueue.Enqueue(new KeyValuePair<Schema.V1_1.entity, Schema.V1_2.entity>
                                        (child, outputVEMLEntity));
                                }
                            }

                            if (item.Value != null)
                            {
                                item.Value.entity1 = AddToV1_2EntityArray(item.Value.entity1, outputVEMLEntity);
                            }

                            if (item.Value == null)
                            {
                                outputVEMLEntities.Add(outputVEMLEntity);
                            }
                        }
                    }

                    outputVEML.environment.entity = outputVEMLEntities.ToArray();
                }
            }

            return outputVEML;
        }

        /// <summary>
        /// Convert the schema instance from version 1.0 to the current schema (version 1.2).
        /// </summary>
        /// <param name="inputVEML">Input VEML instance.</param>
        /// <returns>Current schema version for the input VEML instance.</returns>
        public static Schema.V1_2.veml ConvertFromV1_0(Schema.V1_0.veml inputVEML)
        {
            Schema.V1_2.veml outputVEML = new Schema.V1_2.veml();
            if (inputVEML.metadata != null)
            {
                // Set up metadata.
                outputVEML.metadata = new Schema.V1_2.vemlMetadata();

                // Assign scripts.
                outputVEML.metadata.script = inputVEML.metadata.script;

                // Assign title.
                outputVEML.metadata.title = inputVEML.metadata.title;

                // Assign input events.
                List<Schema.V1_2.inputevent> outputVEMLInputEvents = new List<Schema.V1_2.inputevent>();
                if (inputVEML.metadata.inputevent != null)
                {
                    foreach (Schema.V1_0.inputevent inputEvent in inputVEML.metadata.inputevent)
                    {
                        Schema.V1_2.inputevent outputVEMLInputEvent = new Schema.V1_2.inputevent();
                        outputVEMLInputEvent.@event = inputEvent.@event;
                        outputVEMLInputEvent.input = inputEvent.input;
                        outputVEMLInputEvents.Add(outputVEMLInputEvent);
                    }
                    outputVEML.metadata.inputevent = outputVEMLInputEvents.ToArray();
                }

                // Assign synchronization services.
                List<Schema.V1_2.synchronizationservice> outputVEMLSynchronizationServices
                    = new List<Schema.V1_2.synchronizationservice>();
                if (inputVEML.metadata.synchronizationservice != null)
                {
                    foreach (Schema.V1_0.synchronizationservice synchronizationService
                        in inputVEML.metadata.synchronizationservice)
                    {
                        Schema.V1_2.synchronizationservice outputVEMLSynchronizationService
                            = new Schema.V1_2.synchronizationservice();
                        outputVEMLSynchronizationService.id = synchronizationService.id;
                        outputVEMLSynchronizationService.address = synchronizationService.address;
                        outputVEMLSynchronizationService.session = synchronizationService.session;
                        outputVEMLSynchronizationService.type = synchronizationService.type;
                    }
                    outputVEML.metadata.synchronizationservice = outputVEMLSynchronizationServices.ToArray();
                }
            }

            if (inputVEML.environment != null)
            {
                // Set up environment.
                outputVEML.environment = new Schema.V1_2.vemlEnvironment();

                // Assign background.
                if (inputVEML.environment.background != null)
                {
                    outputVEML.environment.background = new Schema.V1_2.background();
                    outputVEML.environment.background.Item = inputVEML.environment.background.Item;
                    switch (inputVEML.environment.background.ItemElementName)
                    {
                        case Schema.V1_0.ItemChoiceType.panorama:
                            outputVEML.environment.background.ItemElementName = Schema.V1_2.ItemChoiceType.panorama;
                            break;

                        case Schema.V1_0.ItemChoiceType.color:
                        default:
                            outputVEML.environment.background.ItemElementName = Schema.V1_2.ItemChoiceType.color;
                            break;
                    }
                }

                // Set up entities.
                List<Schema.V1_2.entity> outputVEMLEntities = new List<Schema.V1_2.entity>();
                if (inputVEML.environment.entity != null)
                {
                    foreach (Schema.V1_0.entity e in inputVEML.environment.entity)
                    {
                        Queue<KeyValuePair<Schema.V1_0.entity, Schema.V1_2.entity>> entityQueue
                            = new Queue<KeyValuePair<Schema.V1_0.entity, Schema.V1_2.entity>>();
                        entityQueue.Enqueue(new KeyValuePair<Schema.V1_0.entity, Schema.V1_2.entity>(e, null));
                        while (entityQueue.Count > 0)
                        {
                            KeyValuePair<Schema.V1_0.entity, Schema.V1_2.entity> item = entityQueue.Dequeue();

                            Schema.V1_2.entity outputVEMLEntity = ConvertEntityFromV1_0(item.Key);

                            if (item.Key.entity1 != null)
                            {
                                foreach (Schema.V1_0.entity child in item.Key.entity1)
                                {
                                    entityQueue.Enqueue(new KeyValuePair<Schema.V1_0.entity, Schema.V1_2.entity>
                                        (child, outputVEMLEntity));
                                }
                            }

                            if (item.Value != null)
                            {
                                item.Value.entity1 = AddToV1_2EntityArray(item.Value.entity1, outputVEMLEntity);
                            }

                            if (item.Value == null)
                            {
                                outputVEMLEntities.Add(outputVEMLEntity);
                            }
                        }
                    }

                    outputVEML.environment.entity = outputVEMLEntities.ToArray();
                }
            }

            return outputVEML;
        }

        /// <summary>
        /// Parse VEML-compliant CSV formatted layer masks.
        /// </summary>
        /// <param name="csvMasks">VEML-compliant CSV formatted layer masks.</param>
        /// <returns>Terrain Entity Layer Mask Collection representation of the VEML-compliant
        /// CSV formatted layer masks.</returns>
        public static Javascript.APIs.Entity.TerrainEntityLayerMaskCollection ParseCSVLayerMasks(string csvMasks)
        {
            Javascript.APIs.Entity.TerrainEntityLayerMaskCollection telmc
                = new Javascript.APIs.Entity.TerrainEntityLayerMaskCollection();

            string[] masks = csvMasks.Split("|");
            foreach (string mask in masks)
            {
                int numCols = 0;
                string[] rows = mask.Split(";");
                int numRows = rows.Length;
                foreach (string row in rows)
                {
                    string[] cols = row.Split(",");
                    int colLength = cols.Length;
                    if (colLength > numCols)
                    {
                        numCols = colLength;
                    }
                }

                float[][] heights = new float[numRows][];
                for (int i = 0; i < numRows; i++)
                {
                    string[] cols = rows[i].Split(",");
                    heights[i] = new float[numCols];
                    for (int j = 0; j < cols.Length; j++)
                    {
                        heights[i][j] = float.Parse(cols[j]);
                    }
                }

                telmc.AddLayerMask(new Javascript.APIs.Entity.TerrainEntityLayerMask(heights));
            }

            return telmc;
        }

        /// <summary>
        /// Parse VEML-compliant CSV formatted layer masks.
        /// </summary>
        /// <param name="csvMasks">VEML-compliant CSV formatted layer masks.</param>
        /// <returns>Dictionary of indices and layer mask array representation of the VEML-compliant
        /// CSV formatted layer masks.</returns>
        public static Dictionary<int, float[,]> ParseCSVLayerMasksToInternalFormat(string csvMasks)
        {
            Dictionary<int, float[,]> outputFormat = new Dictionary<int, float[,]>();

            string[] masks = csvMasks.Split("|");
            int idx = 0;
            foreach (string mask in masks)
            {
                int numCols = 0;
                string[] rows = mask.Split(";");
                int numRows = rows.Length;
                foreach (string row in rows)
                {
                    string[] cols = row.Split(",");
                    int colLength = cols.Length;
                    if (colLength > numCols)
                    {
                        numCols = colLength;
                    }
                }

                float[,] heights = new float[numRows, numCols];
                for (int i = 0; i < numRows; i++)
                {
                    string[] cols = rows[i].Split(",");
                    for (int j = 0; j < cols.Length; j++)
                    {
                        heights[i, j] = float.Parse(cols[j]);
                    }
                }

                outputFormat.Add(idx++, heights);
            }

            return outputFormat;
        }

        /// <summary>
        /// Get a VEML-compliant CSV formatted series of layer masks.
        /// </summary>
        /// <param name="maskCollection">Terrain Entity Layer Mask Collection to format.</param>
        /// <returns>A VEML-compliant CSV formatted series of layer masks.</returns>
        public static string ToCSVLayerMasks(Javascript.APIs.Entity.TerrainEntityLayerMaskCollection maskCollection)
        {
            string outputString = "";
            foreach (Javascript.APIs.Entity.TerrainEntityLayerMask mask in maskCollection.GetLayerMasks())
            {
                int x = mask.heights.GetLength(0);
                int y = mask.heights.GetLength(1);
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (j == 0)
                        {
                            outputString = outputString + mask.heights[i, j];
                        }
                        else
                        {
                            outputString = outputString + "," + mask.heights[i, j];
                        }
                    }

                    if (i < x - 1)
                    {
                        outputString = outputString + ";";
                    }
                }

                outputString = outputString + "|";
            }

            return outputString.Substring(0, outputString.Length - 1);
        }

        /// <summary>
        /// Parse VEML-compliant CSV formatted heights.
        /// </summary>
        /// <param name="csvHeights">VEML-compliant CSV formatted heights.</param>
        /// <returns>2D float array representation of the VEML-compliant CSV formatted heights.</returns>
        public static float[,] ParseCSVHeights(string csvHeights)
        {
            int numCols = 0;
            string[] rows = csvHeights.Split(';');
            int numRows = rows.Length;
            foreach (string row in rows)
            {
                string[] cols = row.Split(",");
                int colLength = cols.Length;
                if (colLength > numCols)
                {
                    numCols = colLength;
                }
            }

            float[,] heights = new float[numRows, numCols];
            for (int i = 0; i < numRows; i++)
            {
                string[] cols = rows[i].Split(",");
                for (int j = 0; j < cols.Length; j++)
                {
                    heights[i, j] = float.Parse(cols[j]);
                }
            }

            return heights;
        }

        /// <summary>
        /// Parse VEML-compliant CSV formatted heights.
        /// </summary>
        /// <param name="csvHeights">VEML-compliant CSV formatted heights.</param>
        /// <returns>Array of float array representation of the VEML-compliant CSV formatted heights.</returns>
        public static float[][] ParseCSVHeightsArrayOfArray(string csvHeights)
        {
            int numCols = 0;
            string[] rows = csvHeights.Split(';');
            int numRows = rows.Length;
            foreach (string row in rows)
            {
                string[] cols = row.Split(",");
                int colLength = cols.Length;
                if (colLength > numCols)
                {
                    numCols = colLength;
                }
            }

            float[][] heights = new float[numRows][];
            for (int i = 0; i < numRows; i++)
            {
                heights[i] = new float[numCols];
                string[] cols = rows[i].Split(",");
                for (int j = 0; j < cols.Length; j++)
                {
                    heights[i][j] = float.Parse(cols[j]);
                }
            }

            return heights;
        }

        /// <summary>
        /// Attempts to return a fully qualified URI.
        /// </summary>
        /// <param name="rawURI"></param>
        /// <param name="uriBase"></param>
        /// <returns>If URI is already fully qualified, returns
        /// raw string. Otherwise, will prepend raw string with URI base.</returns>
        public static string FullyQualifyURI(string rawURI, string uriBase)
        {
            string uriToTest = rawURI.Replace("\\", "/");
            if (uriToTest.Contains("file://") && !uriToTest.Contains("file:///"))
            {
                uriToTest = uriToTest.Replace("file://", "file:///");
            }

            if (uriToTest.Contains("file:/") && !uriToTest.Contains("file:///"))
            {
                uriToTest = uriToTest.Replace("file:/", "file:///");
            }

            if (Uri.IsWellFormedUriString(uriToTest, UriKind.Absolute))
            {
                return rawURI;
            }
            else
            {
                return uriBase + "/" + rawURI;
            }
        }

        /// <summary>
        /// Format a URI to be valid. Will replace single forward slashes in the protocol heading
        /// with double forward slashes and replace back slashes with forward slashes.
        /// </summary>
        /// <param name="unformattedURI">URI to format.</param>
        /// <returns>The formatted URI. Will replace single forward slashes in the protocol heading
        /// with double forward slashes and replace back slashes with forward slashes.</returns>
        public static string FormatURI(string unformattedURI)
        {
            string uri = unformattedURI;
            if (uri.Contains("http:/") && !uri.Contains("http://"))
            {
                uri = uri.Replace("http:/", "http://");
            }
            if (uri.Contains("http:\\") && !uri.Contains("http:\\\\"))
            {
                uri = uri.Replace("http:\\", "http://");
            }
            if (uri.Contains("https:/") && !uri.Contains("https://"))
            {
                uri = uri.Replace("https:/", "https://");
            }
            if (uri.Contains("https:\\") && !uri.Contains("https:\\\\"))
            {
                uri = uri.Replace("https:\\", "https://");
            }
            uri = uri.Replace("\\", "/");
            return uri;
        }

        /// <summary>
        /// Convert an entity from version 1.1 to the current schema (version 1.2).
        /// </summary>
        /// <param name="entity">Input entity instance.</param>
        /// <returns>Current schema version for the input entity instance.</returns>
        private static Schema.V1_2.entity ConvertEntityFromV1_1(Schema.V1_1.entity entity)
        {
            // Assign entity.
            Schema.V1_2.entity outputEntity;

            // Button Entity.
            if (entity is Schema.V1_1.buttonentity)
            {
                outputEntity = new Schema.V1_2.buttonentity();
                ((Schema.V1_2.buttonentity) outputEntity).onclickevent
                    = ((Schema.V1_1.buttonentity) entity).onclickevent;
            }

            // Canvas Entity.
            else if (entity is Schema.V1_1.canvasentity)
            {
                outputEntity = new Schema.V1_2.canvasentity();
            }

            // Character Entity.
            else if (entity is Schema.V1_1.characterentity)
            {
                outputEntity = new Schema.V1_2.characterentity();
            }

            // Container Entity.
            else if (entity is Schema.V1_1.containerentity)
            {
                outputEntity = new Schema.V1_2.containerentity();
            }

            // Input Entity.
            else if (entity is Schema.V1_1.inputentity)
            {
                outputEntity = new Schema.V1_2.inputentity();
            }

            // LightEntity.
            else if (entity is Schema.V1_1.lightentity)
            {
                outputEntity = new Schema.V1_2.lightentity();
            }

            // Mesh Entity.
            else if (entity is Schema.V1_1.meshentity)
            {
                outputEntity = new Schema.V1_2.meshentity();
                ((Schema.V1_2.meshentity) outputEntity).meshresource
                    = ((Schema.V1_1.meshentity) entity).meshresource;
                ((Schema.V1_2.meshentity) outputEntity).meshname
                    = ((Schema.V1_1.meshentity) entity).meshname;
            }

            // Terrain Entity.
            else if (entity is Schema.V1_1.terrainentity)
            {
                outputEntity = new Schema.V1_2.terrainentity();
                ((Schema.V1_2.terrainentity) outputEntity).height
                    = ((Schema.V1_1.terrainentity) entity).height;
                ((Schema.V1_2.terrainentity) outputEntity).heights
                    = ((Schema.V1_1.terrainentity) entity).heights;
                ((Schema.V1_2.terrainentity) outputEntity).layer = null;
                ((Schema.V1_2.terrainentity) outputEntity).layermasks = null;
                ((Schema.V1_2.terrainentity) outputEntity).length
                    = ((Schema.V1_1.terrainentity) entity).length;
                ((Schema.V1_2.terrainentity) outputEntity).width
                    = ((Schema.V1_1.terrainentity) entity).width;
            }

            // Text Entity.
            else if (entity is Schema.V1_1.textentity)
            {
                outputEntity = new Schema.V1_2.textentity();
                ((Schema.V1_2.textentity) outputEntity).text
                    = ((Schema.V1_1.textentity) entity).text;
                ((Schema.V1_2.textentity) outputEntity).fontsize
                    = ((Schema.V1_1.textentity) entity).fontsize;
            }

            // Voxel Entity.
            else if (entity is Schema.V1_1.voxelentity)
            {
                outputEntity = new Schema.V1_2.voxelentity();
            }

            // Default to Base Entity.
            else
            {
                outputEntity = new Schema.V1_2.entity();
            }

            // Assign id.
            outputEntity.id = entity.id;

            // Assign tag.
            outputEntity.tag = entity.tag;

            // Assign transform.
            if (entity.transform != null)
            {
                // Canvas Transform.
                if (entity.transform is Schema.V1_1.canvastransform)
                {
                    // Assign Canvas Transform.
                    outputEntity.transform = new Schema.V1_2.canvastransform();

                    // Assign Position Percent.
                    if (((Schema.V1_1.canvastransform) entity.transform).positionpercent != null)
                    {
                        ((Schema.V1_2.canvastransform) outputEntity.transform).positionpercent =
                            new Schema.V1_2.positionpercent();
                        ((Schema.V1_2.canvastransform) outputEntity.transform).positionpercent.x =
                            ((Schema.V1_1.canvastransform) entity.transform).positionpercent.x;
                        ((Schema.V1_2.canvastransform) outputEntity.transform).positionpercent.y =
                            ((Schema.V1_1.canvastransform) entity.transform).positionpercent.y;
                    }

                    // Assign Size Percent.
                    if (((Schema.V1_1.canvastransform) entity.transform).sizepercent != null)
                    {
                        ((Schema.V1_2.canvastransform) outputEntity.transform).sizepercent =
                            new Schema.V1_2.sizepercent();
                        ((Schema.V1_2.canvastransform) outputEntity.transform).sizepercent.x =
                            ((Schema.V1_1.canvastransform) entity.transform).sizepercent.x;
                        ((Schema.V1_2.canvastransform) outputEntity.transform).sizepercent.y =
                            ((Schema.V1_1.canvastransform) entity.transform).sizepercent.y;
                    }
                }

                // Scale Transform.
                else if (entity.transform is Schema.V1_1.scaletransform)
                {
                    // Assign Scale Transform.
                    outputEntity.transform = new Schema.V1_2.scaletransform();

                    // Assign Position.
                    if (((Schema.V1_1.scaletransform) entity.transform).position != null)
                    {
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position =
                            new Schema.V1_2.position();
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position.x =
                            ((Schema.V1_1.scaletransform) entity.transform).position.x;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position.y =
                            ((Schema.V1_1.scaletransform) entity.transform).position.y;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position.z =
                            ((Schema.V1_1.scaletransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_1.scaletransform) entity.transform).rotation != null)
                    {
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation =
                            new Schema.V1_2.rotation();
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.x;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.y;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.z;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_1.scaletransform) entity.transform).scale != null)
                    {
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale =
                            new Schema.V1_2.scale();
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale.x =
                            ((Schema.V1_1.scaletransform) entity.transform).scale.x;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale.y =
                            ((Schema.V1_1.scaletransform) entity.transform).scale.y;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale.z =
                            ((Schema.V1_1.scaletransform) entity.transform).scale.z;
                    }
                }

                // Size Transform.
                else if (entity.transform is Schema.V1_1.sizetransform)
                {
                    // Assign Size Transform.
                    outputEntity.transform = new Schema.V1_2.sizetransform();

                    // Assign Position.
                    if (((Schema.V1_1.sizetransform) entity.transform).position != null)
                    {
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position =
                            new Schema.V1_2.position();
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position.x =
                            ((Schema.V1_1.sizetransform) entity.transform).position.x;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position.y =
                            ((Schema.V1_1.sizetransform) entity.transform).position.y;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position.z =
                            ((Schema.V1_1.sizetransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_1.sizetransform) entity.transform).rotation != null)
                    {
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation =
                            new Schema.V1_2.rotation();
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.x;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.y;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.z;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_1.sizetransform) entity.transform).size != null)
                    {
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size =
                            new Schema.V1_2.size();
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size.x =
                            ((Schema.V1_1.sizetransform) entity.transform).size.x;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size.y =
                            ((Schema.V1_1.sizetransform) entity.transform).size.y;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size.z =
                            ((Schema.V1_1.sizetransform) entity.transform).size.z;
                    }
                }

                // Default to Base Transform.
                else
                {
                    outputEntity.transform = new Schema.V1_2.basetransform();
                }
            }

            // Assign On Load Event.
            outputEntity.onloadevent = entity.onloadevent;

            // Assign Synchronizer.
            outputEntity.synchronizer = entity.synchronizer;

            return outputEntity;
        }

        /// <summary>
        /// Convert an entity from version 1.0 to the current schema (version 1.2).
        /// </summary>
        /// <param name="entity">Input entity instance.</param>
        /// <returns>Current schema version for the input entity instance.</returns>
        private static Schema.V1_2.entity ConvertEntityFromV1_0(Schema.V1_0.entity entity)
        {
            // Assign entity.
            Schema.V1_2.entity outputEntity;

            // Button Entity.
            if (entity is Schema.V1_0.buttonentity)
            {
                outputEntity = new Schema.V1_2.buttonentity();
                ((Schema.V1_2.buttonentity) outputEntity).onclickevent
                    = ((Schema.V1_0.buttonentity) entity).onclickevent;
            }

            // Canvas Entity.
            else if (entity is Schema.V1_0.canvasentity)
            {
                outputEntity = new Schema.V1_2.canvasentity();
            }

            // Character Entity.
            else if (entity is Schema.V1_0.characterentity)
            {
                outputEntity = new Schema.V1_2.characterentity();
            }

            // Container Entity.
            else if (entity is Schema.V1_0.containerentity)
            {
                outputEntity = new Schema.V1_2.containerentity();
            }

            // Input Entity.
            else if (entity is Schema.V1_0.inputentity)
            {
                outputEntity = new Schema.V1_2.inputentity();
            }

            // LightEntity.
            else if (entity is Schema.V1_0.lightentity)
            {
                outputEntity = new Schema.V1_2.lightentity();
            }

            // Mesh Entity.
            else if (entity is Schema.V1_0.meshentity)
            {
                outputEntity = new Schema.V1_2.meshentity();
                ((Schema.V1_2.meshentity) outputEntity).meshresource
                    = ((Schema.V1_0.meshentity) entity).meshresource;
                ((Schema.V1_2.meshentity) outputEntity).meshname
                    = ((Schema.V1_0.meshentity) entity).meshname;
            }

            // Terrain Entity.
            else if (entity is Schema.V1_0.terrainentity)
            {
                outputEntity = new Schema.V1_2.terrainentity();
                ((Schema.V1_2.terrainentity) outputEntity).height
                    = ((Schema.V1_0.terrainentity) entity).height;
                ((Schema.V1_2.terrainentity) outputEntity).heights
                    = ((Schema.V1_0.terrainentity) entity).heights;
                ((Schema.V1_2.terrainentity) outputEntity).layer = null;
                ((Schema.V1_2.terrainentity) outputEntity).layermasks = null;
                ((Schema.V1_2.terrainentity) outputEntity).length
                    = ((Schema.V1_0.terrainentity) entity).length;
                ((Schema.V1_2.terrainentity) outputEntity).width
                    = ((Schema.V1_0.terrainentity) entity).width;
            }

            // Text Entity.
            else if (entity is Schema.V1_0.textentity)
            {
                outputEntity = new Schema.V1_2.textentity();
                ((Schema.V1_2.textentity) outputEntity).text
                    = ((Schema.V1_0.textentity) entity).text;
                ((Schema.V1_2.textentity) outputEntity).fontsize
                    = ((Schema.V1_0.textentity) entity).fontsize;
            }

            // Voxel Entity.
            else if (entity is Schema.V1_0.voxelentity)
            {
                outputEntity = new Schema.V1_2.voxelentity();
            }

            // Default to Base Entity.
            else
            {
                outputEntity = new Schema.V1_2.entity();
            }

            // Assign id.
            outputEntity.id = entity.id;

            // Assign tag.
            outputEntity.tag = entity.tag;

            // Assign transform.
            if (entity.transform != null)
            {
                // Canvas Transform.
                if (entity.transform is Schema.V1_0.canvastransform)
                {
                    // Assign Canvas Transform.
                    outputEntity.transform = new Schema.V1_2.canvastransform();

                    // Assign Position Percent.
                    if (((Schema.V1_0.canvastransform) entity.transform).positionpercent != null)
                    {
                        ((Schema.V1_2.canvastransform) outputEntity.transform).positionpercent =
                            new Schema.V1_2.positionpercent();
                        ((Schema.V1_2.canvastransform) outputEntity.transform).positionpercent.x =
                            ((Schema.V1_0.canvastransform) entity.transform).positionpercent.x;
                        ((Schema.V1_2.canvastransform) outputEntity.transform).positionpercent.y =
                            ((Schema.V1_0.canvastransform) entity.transform).positionpercent.y;
                    }

                    // Assign Size Percent.
                    if (((Schema.V1_0.canvastransform) entity.transform).sizepercent != null)
                    {
                        ((Schema.V1_2.canvastransform) outputEntity.transform).sizepercent =
                            new Schema.V1_2.sizepercent();
                        ((Schema.V1_2.canvastransform) outputEntity.transform).sizepercent.x =
                            ((Schema.V1_0.canvastransform) entity.transform).sizepercent.x;
                        ((Schema.V1_2.canvastransform) outputEntity.transform).sizepercent.y =
                            ((Schema.V1_0.canvastransform) entity.transform).sizepercent.y;
                    }
                }

                // Scale Transform.
                else if (entity.transform is Schema.V1_0.scaletransform)
                {
                    // Assign Scale Transform.
                    outputEntity.transform = new Schema.V1_2.scaletransform();

                    // Assign Position.
                    if (((Schema.V1_0.scaletransform) entity.transform).position != null)
                    {
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position =
                            new Schema.V1_2.position();
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position.x =
                            ((Schema.V1_0.scaletransform) entity.transform).position.x;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position.y =
                            ((Schema.V1_0.scaletransform) entity.transform).position.y;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).position.z =
                            ((Schema.V1_0.scaletransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_0.scaletransform) entity.transform).rotation != null)
                    {
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation =
                            new Schema.V1_2.rotation();
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.x;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.y;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.z;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_0.scaletransform) entity.transform).scale != null)
                    {
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale =
                            new Schema.V1_2.scale();
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale.x =
                            ((Schema.V1_0.scaletransform) entity.transform).scale.x;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale.y =
                            ((Schema.V1_0.scaletransform) entity.transform).scale.y;
                        ((Schema.V1_2.scaletransform) outputEntity.transform).scale.z =
                            ((Schema.V1_0.scaletransform) entity.transform).scale.z;
                    }
                }

                // Size Transform.
                else if (entity.transform is Schema.V1_0.sizetransform)
                {
                    // Assign Size Transform.
                    outputEntity.transform = new Schema.V1_2.sizetransform();

                    // Assign Position.
                    if (((Schema.V1_0.sizetransform) entity.transform).position != null)
                    {
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position =
                            new Schema.V1_2.position();
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position.x =
                            ((Schema.V1_0.sizetransform) entity.transform).position.x;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position.y =
                            ((Schema.V1_0.sizetransform) entity.transform).position.y;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).position.z =
                            ((Schema.V1_0.sizetransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_0.sizetransform) entity.transform).rotation != null)
                    {
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation =
                            new Schema.V1_2.rotation();
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.x;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.y;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.z;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_0.sizetransform) entity.transform).size != null)
                    {
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size =
                            new Schema.V1_2.size();
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size.x =
                            ((Schema.V1_0.sizetransform) entity.transform).size.x;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size.y =
                            ((Schema.V1_0.sizetransform) entity.transform).size.y;
                        ((Schema.V1_2.sizetransform) outputEntity.transform).size.z =
                            ((Schema.V1_0.sizetransform) entity.transform).size.z;
                    }
                }

                // Default to Base Transform.
                else
                {
                    outputEntity.transform = new Schema.V1_2.basetransform();
                }
            }

            // Assign On Load Event.
            outputEntity.onloadevent = entity.onloadevent;

            // Assign Synchronizer.
            outputEntity.synchronizer = entity.synchronizer;

            return outputEntity;
        }

        /// <summary>
        /// Add an entity to a Version 1.2 entity array.
        /// </summary>
        /// <param name="baseArray">Initial array.</param>
        /// <param name="entityToAdd">Entity to add to array.</param>
        /// <returns>The input array with the specified entity added.</returns>
        private static Schema.V1_2.entity[] AddToV1_2EntityArray(Schema.V1_2.entity[] baseArray,
            Schema.V1_2.entity entityToAdd)
        {
            List<Schema.V1_2.entity> entityList;
            if (baseArray == null)
            {
                entityList = new List<Schema.V1_2.entity>();
            }
            else
            {
                entityList = new List<Schema.V1_2.entity>(baseArray);
            }
            entityList.Add(entityToAdd);
            return entityList.ToArray();
        }
    }
}