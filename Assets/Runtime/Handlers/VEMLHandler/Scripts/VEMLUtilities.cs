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
        public static readonly string xmlHeadingTag = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

        public static readonly string VEML2_1FullTag = "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/2.1\"" +
            " xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/2.1 schema.xsd\"" +
            " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";

        public static readonly string VEML2_0FullTag = "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/2.0\"" +
            " xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/2.0 schema.xsd\"" +
            " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";

        public static readonly string VEML1_3FullTag = "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/1.3\"" +
            " xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/1.3" +
            " schema.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";

        public static readonly string VEML1_2FullTag = "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/1.2\"" +
            " xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/1.2 schema.xsd\"" +
            " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";

        public static readonly string VEML1_1FullTag = "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/1.1\"" +
            " xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/1.1 schema.xsd\"" +
            " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";

        public static readonly string VEML1_0FullTag = "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/1.0\"" +
            " xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/1.0 schema.xsd\"" +
            " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";

        public static string FullyNotateVEML2_1(string inputVEML)
        {
            return FullyNotateVEML(inputVEML, VEML2_1FullTag);
        }

        public static string FullyNotateVEML2_0(string inputVEML)
        {
            return FullyNotateVEML(inputVEML, VEML2_0FullTag);
        }

        public static string FullyNotateVEML1_3(string inputVEML)
        {
            return FullyNotateVEML(inputVEML, VEML1_3FullTag);
        }

        public static string FullyNotateVEML1_2(string inputVEML)
        {
            return FullyNotateVEML(inputVEML, VEML1_2FullTag);
        }

        public static string FullyNotateVEML1_1(string inputVEML)
        {
            return FullyNotateVEML(inputVEML, VEML1_1FullTag);
        }

        public static string FullyNotateVEML1_0(string inputVEML)
        {
            return FullyNotateVEML(inputVEML, VEML1_0FullTag);
        }

        /// <summary>
        /// Convert the schema instance from version 2.0 to the current schema (version 2.1).
        /// </summary>
        /// <param name="inputVEML">Input VEML instance.</param>
        /// <returns>Current schema version for the input VEML instance.</returns>
        public static Schema.V2_1.veml ConvertFromV2_0(Schema.V2_0.veml inputVEML)
        {
            Schema.V2_1.veml outputVEML = new Schema.V2_1.veml();
            if (inputVEML.metadata != null)
            {
                // Set up metadata.
                outputVEML.metadata = new Schema.V2_1.vemlMetadata();

                // Assign scripts.
                outputVEML.metadata.script = inputVEML.metadata.script;

                // Assign title.
                outputVEML.metadata.title = inputVEML.metadata.title;

                // Assign input events.
                List<Schema.V2_1.inputevent> outputVEMLInputEvents = new List<Schema.V2_1.inputevent>();
                if (inputVEML.metadata.inputevent != null)
                {
                    foreach (Schema.V2_0.inputevent inputEvent in inputVEML.metadata.inputevent)
                    {
                        Schema.V2_1.inputevent outputVEMLInputEvent = new Schema.V2_1.inputevent();
                        outputVEMLInputEvent.@event = inputEvent.@event;
                        outputVEMLInputEvent.input = inputEvent.input;
                        outputVEMLInputEvents.Add(outputVEMLInputEvent);
                    }
                    outputVEML.metadata.inputevent = outputVEMLInputEvents.ToArray();
                }

                // Assign synchronization services.
                List<Schema.V2_1.synchronizationservice> outputVEMLSynchronizationServices
                    = new List<Schema.V2_1.synchronizationservice>();
                if (inputVEML.metadata.synchronizationservice != null)
                {
                    foreach (Schema.V2_0.synchronizationservice synchronizationService
                        in inputVEML.metadata.synchronizationservice)
                    {
                        Schema.V2_1.synchronizationservice outputVEMLSynchronizationService
                            = new Schema.V2_1.synchronizationservice();
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
                outputVEML.environment = new Schema.V2_1.vemlEnvironment();

                // Assign background.
                if (inputVEML.environment.background != null)
                {
                    outputVEML.environment.background = new Schema.V2_1.background();
                    outputVEML.environment.background.Item = inputVEML.environment.background.Item;
                    switch (inputVEML.environment.background.ItemElementName)
                    {
                        case Schema.V2_0.ItemChoiceType.panorama:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.panorama;
                            break;

                        case Schema.V2_0.ItemChoiceType.color:
                        default:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.color;
                            break;
                    }
                }

                // Set up entities.
                List<Schema.V2_1.entity> outputVEMLEntities = new List<Schema.V2_1.entity>();
                if (inputVEML.environment.entity != null)
                {
                    foreach (Schema.V2_0.entity e in inputVEML.environment.entity)
                    {
                        Queue<KeyValuePair<Schema.V2_0.entity, Schema.V2_1.entity>> entityQueue
                            = new Queue<KeyValuePair<Schema.V2_0.entity, Schema.V2_1.entity>>();
                        entityQueue.Enqueue(new KeyValuePair<Schema.V2_0.entity, Schema.V2_1.entity>(e, null));
                        while (entityQueue.Count > 0)
                        {
                            KeyValuePair<Schema.V2_0.entity, Schema.V2_1.entity> item = entityQueue.Dequeue();

                            Schema.V2_1.entity outputVEMLEntity = ConvertEntityFromV2_0(item.Key);

                            if (item.Key.entity1 != null)
                            {
                                foreach (Schema.V2_0.entity child in item.Key.entity1)
                                {
                                    entityQueue.Enqueue(new KeyValuePair<Schema.V2_0.entity, Schema.V2_1.entity>
                                        (child, outputVEMLEntity));
                                }
                            }

                            if (item.Value != null)
                            {
                                item.Value.entity1 = AddToV2_1EntityArray(item.Value.entity1, outputVEMLEntity);
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
        /// Convert the schema instance from version 1.3 to the current schema (version 2.1).
        /// </summary>
        /// <param name="inputVEML">Input VEML instance.</param>
        /// <returns>Current schema version for the input VEML instance.</returns>
        public static Schema.V2_1.veml ConvertFromV1_3(Schema.V1_3.veml inputVEML)
        {
            Schema.V2_1.veml outputVEML = new Schema.V2_1.veml();
            if (inputVEML.metadata != null)
            {
                // Set up metadata.
                outputVEML.metadata = new Schema.V2_1.vemlMetadata();

                // Assign scripts.
                outputVEML.metadata.script = inputVEML.metadata.script;

                // Assign title.
                outputVEML.metadata.title = inputVEML.metadata.title;

                // Assign input events.
                List<Schema.V2_1.inputevent> outputVEMLInputEvents = new List<Schema.V2_1.inputevent>();
                if (inputVEML.metadata.inputevent != null)
                {
                    foreach (Schema.V1_3.inputevent inputEvent in inputVEML.metadata.inputevent)
                    {
                        Schema.V2_1.inputevent outputVEMLInputEvent = new Schema.V2_1.inputevent();
                        outputVEMLInputEvent.@event = inputEvent.@event;
                        outputVEMLInputEvent.input = inputEvent.input;
                        outputVEMLInputEvents.Add(outputVEMLInputEvent);
                    }
                    outputVEML.metadata.inputevent = outputVEMLInputEvents.ToArray();
                }

                // Assign synchronization services.
                List<Schema.V2_1.synchronizationservice> outputVEMLSynchronizationServices
                    = new List<Schema.V2_1.synchronizationservice>();
                if (inputVEML.metadata.synchronizationservice != null)
                {
                    foreach (Schema.V1_3.synchronizationservice synchronizationService
                        in inputVEML.metadata.synchronizationservice)
                    {
                        Schema.V2_1.synchronizationservice outputVEMLSynchronizationService
                            = new Schema.V2_1.synchronizationservice();
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
                outputVEML.environment = new Schema.V2_1.vemlEnvironment();

                // Assign background.
                if (inputVEML.environment.background != null)
                {
                    outputVEML.environment.background = new Schema.V2_1.background();
                    outputVEML.environment.background.Item = inputVEML.environment.background.Item;
                    switch (inputVEML.environment.background.ItemElementName)
                    {
                        case Schema.V1_3.ItemChoiceType.panorama:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.panorama;
                            break;

                        case Schema.V1_3.ItemChoiceType.color:
                        default:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.color;
                            break;
                    }
                }

                // Set up entities.
                List<Schema.V2_1.entity> outputVEMLEntities = new List<Schema.V2_1.entity>();
                if (inputVEML.environment.entity != null)
                {
                    foreach (Schema.V1_3.entity e in inputVEML.environment.entity)
                    {
                        Queue<KeyValuePair<Schema.V1_3.entity, Schema.V2_1.entity>> entityQueue
                            = new Queue<KeyValuePair<Schema.V1_3.entity, Schema.V2_1.entity>>();
                        entityQueue.Enqueue(new KeyValuePair<Schema.V1_3.entity, Schema.V2_1.entity>(e, null));
                        while (entityQueue.Count > 0)
                        {
                            KeyValuePair<Schema.V1_3.entity, Schema.V2_1.entity> item = entityQueue.Dequeue();

                            Schema.V2_1.entity outputVEMLEntity = ConvertEntityFromV1_3(item.Key);

                            if (item.Key.entity1 != null)
                            {
                                foreach (Schema.V1_3.entity child in item.Key.entity1)
                                {
                                    entityQueue.Enqueue(new KeyValuePair<Schema.V1_3.entity, Schema.V2_1.entity>
                                        (child, outputVEMLEntity));
                                }
                            }

                            if (item.Value != null)
                            {
                                item.Value.entity1 = AddToV2_1EntityArray(item.Value.entity1, outputVEMLEntity);
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
        /// Convert the schema instance from version 1.2 to the current schema (version 2.1).
        /// </summary>
        /// <param name="inputVEML">Input VEML instance.</param>
        /// <returns>Current schema version for the input VEML instance.</returns>
        public static Schema.V2_1.veml ConvertFromV1_2(Schema.V1_2.veml inputVEML)
        {
            Schema.V2_1.veml outputVEML = new Schema.V2_1.veml();
            if (inputVEML.metadata != null)
            {
                // Set up metadata.
                outputVEML.metadata = new Schema.V2_1.vemlMetadata();

                // Assign scripts.
                outputVEML.metadata.script = inputVEML.metadata.script;

                // Assign title.
                outputVEML.metadata.title = inputVEML.metadata.title;

                // Assign input events.
                List<Schema.V2_1.inputevent> outputVEMLInputEvents = new List<Schema.V2_1.inputevent>();
                if (inputVEML.metadata.inputevent != null)
                {
                    foreach (Schema.V1_2.inputevent inputEvent in inputVEML.metadata.inputevent)
                    {
                        Schema.V2_1.inputevent outputVEMLInputEvent = new Schema.V2_1.inputevent();
                        outputVEMLInputEvent.@event = inputEvent.@event;
                        outputVEMLInputEvent.input = inputEvent.input;
                        outputVEMLInputEvents.Add(outputVEMLInputEvent);
                    }
                    outputVEML.metadata.inputevent = outputVEMLInputEvents.ToArray();
                }

                // Assign synchronization services.
                List<Schema.V2_1.synchronizationservice> outputVEMLSynchronizationServices
                    = new List<Schema.V2_1.synchronizationservice>();
                if (inputVEML.metadata.synchronizationservice != null)
                {
                    foreach (Schema.V1_2.synchronizationservice synchronizationService
                        in inputVEML.metadata.synchronizationservice)
                    {
                        Schema.V2_1.synchronizationservice outputVEMLSynchronizationService
                            = new Schema.V2_1.synchronizationservice();
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
                outputVEML.environment = new Schema.V2_1.vemlEnvironment();

                // Assign background.
                if (inputVEML.environment.background != null)
                {
                    outputVEML.environment.background = new Schema.V2_1.background();
                    outputVEML.environment.background.Item = inputVEML.environment.background.Item;
                    switch (inputVEML.environment.background.ItemElementName)
                    {
                        case Schema.V1_2.ItemChoiceType.panorama:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.panorama;
                            break;

                        case Schema.V1_2.ItemChoiceType.color:
                        default:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.color;
                            break;
                    }
                }

                // Set up entities.
                List<Schema.V2_1.entity> outputVEMLEntities = new List<Schema.V2_1.entity>();
                if (inputVEML.environment.entity != null)
                {
                    foreach (Schema.V1_2.entity e in inputVEML.environment.entity)
                    {
                        Queue<KeyValuePair<Schema.V1_2.entity, Schema.V2_1.entity>> entityQueue
                            = new Queue<KeyValuePair<Schema.V1_2.entity, Schema.V2_1.entity>>();
                        entityQueue.Enqueue(new KeyValuePair<Schema.V1_2.entity, Schema.V2_1.entity>(e, null));
                        while (entityQueue.Count > 0)
                        {
                            KeyValuePair<Schema.V1_2.entity, Schema.V2_1.entity> item = entityQueue.Dequeue();

                            Schema.V2_1.entity outputVEMLEntity = ConvertEntityFromV1_2(item.Key);

                            if (item.Key.entity1 != null)
                            {
                                foreach (Schema.V1_2.entity child in item.Key.entity1)
                                {
                                    entityQueue.Enqueue(new KeyValuePair<Schema.V1_2.entity, Schema.V2_1.entity>
                                        (child, outputVEMLEntity));
                                }
                            }

                            if (item.Value != null)
                            {
                                item.Value.entity1 = AddToV2_1EntityArray(item.Value.entity1, outputVEMLEntity);
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
        /// Convert the schema instance from version 1.1 to the current schema (version 2.1).
        /// </summary>
        /// <param name="inputVEML">Input VEML instance.</param>
        /// <returns>Current schema version for the input VEML instance.</returns>
        public static Schema.V2_1.veml ConvertFromV1_1(Schema.V1_1.veml inputVEML)
        {
            Schema.V2_1.veml outputVEML = new Schema.V2_1.veml();
            if (inputVEML.metadata != null)
            {
                // Set up metadata.
                outputVEML.metadata = new Schema.V2_1.vemlMetadata();

                // Assign scripts.
                outputVEML.metadata.script = inputVEML.metadata.script;

                // Assign title.
                outputVEML.metadata.title = inputVEML.metadata.title;

                // Assign input events.
                List<Schema.V2_1.inputevent> outputVEMLInputEvents = new List<Schema.V2_1.inputevent>();
                if (inputVEML.metadata.inputevent != null)
                {
                    foreach (Schema.V1_1.inputevent inputEvent in inputVEML.metadata.inputevent)
                    {
                        Schema.V2_1.inputevent outputVEMLInputEvent = new Schema.V2_1.inputevent();
                        outputVEMLInputEvent.@event = inputEvent.@event;
                        outputVEMLInputEvent.input = inputEvent.input;
                        outputVEMLInputEvents.Add(outputVEMLInputEvent);
                    }
                    outputVEML.metadata.inputevent = outputVEMLInputEvents.ToArray();
                }

                // Assign synchronization services.
                List<Schema.V2_1.synchronizationservice> outputVEMLSynchronizationServices
                    = new List<Schema.V2_1.synchronizationservice>();
                if (inputVEML.metadata.synchronizationservice != null)
                {
                    foreach (Schema.V1_1.synchronizationservice synchronizationService
                        in inputVEML.metadata.synchronizationservice)
                    {
                        Schema.V2_1.synchronizationservice outputVEMLSynchronizationService
                            = new Schema.V2_1.synchronizationservice();
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
                outputVEML.environment = new Schema.V2_1.vemlEnvironment();

                // Assign background.
                if (inputVEML.environment.background != null)
                {
                    outputVEML.environment.background = new Schema.V2_1.background();
                    outputVEML.environment.background.Item = inputVEML.environment.background.Item;
                    switch (inputVEML.environment.background.ItemElementName)
                    {
                        case Schema.V1_1.ItemChoiceType.panorama:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.panorama;
                            break;

                        case Schema.V1_1.ItemChoiceType.color:
                        default:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.color;
                            break;
                    }
                }

                // Set up entities.
                List<Schema.V2_1.entity> outputVEMLEntities = new List<Schema.V2_1.entity>();
                if (inputVEML.environment.entity != null)
                {
                    foreach (Schema.V1_1.entity e in inputVEML.environment.entity)
                    {
                        Queue<KeyValuePair<Schema.V1_1.entity, Schema.V2_1.entity>> entityQueue
                            = new Queue<KeyValuePair<Schema.V1_1.entity, Schema.V2_1.entity>>();
                        entityQueue.Enqueue(new KeyValuePair<Schema.V1_1.entity, Schema.V2_1.entity>(e, null));
                        while (entityQueue.Count > 0)
                        {
                            KeyValuePair<Schema.V1_1.entity, Schema.V2_1.entity> item = entityQueue.Dequeue();

                            Schema.V2_1.entity outputVEMLEntity = ConvertEntityFromV1_1(item.Key);

                            if (item.Key.entity1 != null)
                            {
                                foreach (Schema.V1_1.entity child in item.Key.entity1)
                                {
                                    entityQueue.Enqueue(new KeyValuePair<Schema.V1_1.entity, Schema.V2_1.entity>
                                        (child, outputVEMLEntity));
                                }
                            }

                            if (item.Value != null)
                            {
                                item.Value.entity1 = AddToV2_1EntityArray(item.Value.entity1, outputVEMLEntity);
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
        /// Convert the schema instance from version 1.0 to the current schema (version 2.1).
        /// </summary>
        /// <param name="inputVEML">Input VEML instance.</param>
        /// <returns>Current schema version for the input VEML instance.</returns>
        public static Schema.V2_1.veml ConvertFromV1_0(Schema.V1_0.veml inputVEML)
        {
            Schema.V2_1.veml outputVEML = new Schema.V2_1.veml();
            if (inputVEML.metadata != null)
            {
                // Set up metadata.
                outputVEML.metadata = new Schema.V2_1.vemlMetadata();

                // Assign scripts.
                outputVEML.metadata.script = inputVEML.metadata.script;

                // Assign title.
                outputVEML.metadata.title = inputVEML.metadata.title;

                // Assign input events.
                List<Schema.V2_1.inputevent> outputVEMLInputEvents = new List<Schema.V2_1.inputevent>();
                if (inputVEML.metadata.inputevent != null)
                {
                    foreach (Schema.V1_0.inputevent inputEvent in inputVEML.metadata.inputevent)
                    {
                        Schema.V2_1.inputevent outputVEMLInputEvent = new Schema.V2_1.inputevent();
                        outputVEMLInputEvent.@event = inputEvent.@event;
                        outputVEMLInputEvent.input = inputEvent.input;
                        outputVEMLInputEvents.Add(outputVEMLInputEvent);
                    }
                    outputVEML.metadata.inputevent = outputVEMLInputEvents.ToArray();
                }

                // Assign synchronization services.
                List<Schema.V2_1.synchronizationservice> outputVEMLSynchronizationServices
                    = new List<Schema.V2_1.synchronizationservice>();
                if (inputVEML.metadata.synchronizationservice != null)
                {
                    foreach (Schema.V1_0.synchronizationservice synchronizationService
                        in inputVEML.metadata.synchronizationservice)
                    {
                        Schema.V2_1.synchronizationservice outputVEMLSynchronizationService
                            = new Schema.V2_1.synchronizationservice();
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
                outputVEML.environment = new Schema.V2_1.vemlEnvironment();

                // Assign background.
                if (inputVEML.environment.background != null)
                {
                    outputVEML.environment.background = new Schema.V2_1.background();
                    outputVEML.environment.background.Item = inputVEML.environment.background.Item;
                    switch (inputVEML.environment.background.ItemElementName)
                    {
                        case Schema.V1_0.ItemChoiceType.panorama:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.panorama;
                            break;

                        case Schema.V1_0.ItemChoiceType.color:
                        default:
                            outputVEML.environment.background.ItemElementName = Schema.V2_1.ItemChoiceType.color;
                            break;
                    }
                }

                // Set up entities.
                List<Schema.V2_1.entity> outputVEMLEntities = new List<Schema.V2_1.entity>();
                if (inputVEML.environment.entity != null)
                {
                    foreach (Schema.V1_0.entity e in inputVEML.environment.entity)
                    {
                        Queue<KeyValuePair<Schema.V1_0.entity, Schema.V2_1.entity>> entityQueue
                            = new Queue<KeyValuePair<Schema.V1_0.entity, Schema.V2_1.entity>>();
                        entityQueue.Enqueue(new KeyValuePair<Schema.V1_0.entity, Schema.V2_1.entity>(e, null));
                        while (entityQueue.Count > 0)
                        {
                            KeyValuePair<Schema.V1_0.entity, Schema.V2_1.entity> item = entityQueue.Dequeue();

                            Schema.V2_1.entity outputVEMLEntity = ConvertEntityFromV1_0(item.Key);

                            if (item.Key.entity1 != null)
                            {
                                foreach (Schema.V1_0.entity child in item.Key.entity1)
                                {
                                    entityQueue.Enqueue(new KeyValuePair<Schema.V1_0.entity, Schema.V2_1.entity>
                                        (child, outputVEMLEntity));
                                }
                            }

                            if (item.Value != null)
                            {
                                item.Value.entity1 = AddToV2_1EntityArray(item.Value.entity1, outputVEMLEntity);
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

        private static string FullyNotateVEML(string inputVEML, string fullVEMLTag)
        {
            if (!inputVEML.StartsWith(xmlHeadingTag))
            {
                inputVEML = xmlHeadingTag + "\n" + inputVEML;
            }

            if (inputVEML.Contains("<VEML>"))
            {
                inputVEML = inputVEML.Replace("<VEML>", "<veml>").Replace("</VEML>", "</veml>");
            }

            return inputVEML.Replace("<veml>", fullVEMLTag);
        }

        /// <summary>
        /// Convert an entity from version 1.3 to the current schema (version 2.1).
        /// </summary>
        /// <param name="entity">Input entity instance.</param>
        /// <returns>Current schema version for the input entity instance.</returns>
        private static Schema.V2_1.entity ConvertEntityFromV2_0(Schema.V2_0.entity entity)
        {
            // Assign entity.
            Schema.V2_1.entity outputEntity;

            // Arch Mesh Entity.
            if (entity is Schema.V2_0.archmeshentity)
            {
                outputEntity = new Schema.V2_1.archmeshentity();
                ((Schema.V2_1.archmeshentity) outputEntity).color
                    = ((Schema.V2_0.archmeshentity) entity).color;
            }

            // Audio Entity.
            else if (entity is Schema.V2_0.audioentity)
            {
                outputEntity = new Schema.V2_1.audioentity();
                ((Schema.V2_1.audioentity) outputEntity).audiofile
                    = ((Schema.V2_0.audioentity) entity).audiofile;
                ((Schema.V2_1.audioentity) outputEntity).autoplay
                    = ((Schema.V2_0.audioentity) entity).autoplay;
                ((Schema.V2_1.audioentity) outputEntity).loop
                    = ((Schema.V2_0.audioentity) entity).loop;
                ((Schema.V2_1.audioentity) outputEntity).priority
                    = ((Schema.V2_0.audioentity) entity).priority;
                ((Schema.V2_1.audioentity) outputEntity).volume
                    = ((Schema.V2_0.audioentity) entity).volume;
                ((Schema.V2_1.audioentity) outputEntity).pitch
                    = ((Schema.V2_0.audioentity) entity).pitch;
                ((Schema.V2_1.audioentity) outputEntity).stereopan
                    = ((Schema.V2_0.audioentity) entity).stereopan;
            }

            // Button Entity.
            else if (entity is Schema.V2_0.buttonentity)
            {
                outputEntity = new Schema.V2_1.buttonentity();
                ((Schema.V2_1.buttonentity) outputEntity).onclickevent
                    = ((Schema.V2_0.buttonentity) entity).onclickevent;
            }

            // Canvas Entity.
            else if (entity is Schema.V2_0.canvasentity)
            {
                outputEntity = new Schema.V2_1.canvasentity();
            }

            // Capsule Mesh Entity.
            if (entity is Schema.V2_0.capsulemeshentity)
            {
                outputEntity = new Schema.V2_1.capsulemeshentity();
                ((Schema.V2_1.capsulemeshentity) outputEntity).color
                    = ((Schema.V2_0.capsulemeshentity) entity).color;
            }

            // Character Entity.
            else if (entity is Schema.V2_0.characterentity)
            {
                outputEntity = new Schema.V2_1.characterentity();
                ((Schema.V2_1.characterentity) outputEntity).meshname
                    = ((Schema.V2_0.characterentity) entity).meshname;
                ((Schema.V2_1.characterentity) outputEntity).meshresource
                    = ((Schema.V2_0.characterentity) entity).meshresource;
                if (((Schema.V2_0.characterentity) entity).meshoffset != null)
                {
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset
                        = new Schema.V2_1.position();
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset.x
                        = ((Schema.V2_0.characterentity) entity).meshoffset.x;
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset.y
                        = ((Schema.V2_0.characterentity) entity).meshoffset.y;
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset.z
                        = ((Schema.V2_0.characterentity) entity).meshoffset.z;
                }
                if (((Schema.V2_0.characterentity) entity).meshrotation != null)
                {
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation
                        = new Schema.V2_1.rotation();
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.x
                        = ((Schema.V2_0.characterentity) entity).meshrotation.x;
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.y
                        = ((Schema.V2_0.characterentity) entity).meshrotation.y;
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.z
                        = ((Schema.V2_0.characterentity) entity).meshrotation.z;
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.w
                        = ((Schema.V2_0.characterentity) entity).meshrotation.w;
                }
                if (((Schema.V2_0.characterentity) entity).labeloffset != null)
                {
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset
                        = new Schema.V2_1.position();
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset.x
                        = ((Schema.V2_0.characterentity) entity).labeloffset.x;
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset.y
                        = ((Schema.V2_0.characterentity) entity).labeloffset.y;
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset.z
                        = ((Schema.V2_0.characterentity) entity).labeloffset.z;
                }
            }

            // Cone Mesh Entity.
            if (entity is Schema.V2_0.conemeshentity)
            {
                outputEntity = new Schema.V2_1.conemeshentity();
                ((Schema.V2_1.conemeshentity) outputEntity).color
                    = ((Schema.V2_0.conemeshentity) entity).color;
            }

            // Container Entity.
            else if (entity is Schema.V2_0.containerentity)
            {
                outputEntity = new Schema.V2_1.containerentity();
            }

            // Cube Mesh Entity.
            if (entity is Schema.V2_0.cubemeshentity)
            {
                outputEntity = new Schema.V2_1.cubemeshentity();
                ((Schema.V2_1.cubemeshentity) outputEntity).color
                    = ((Schema.V2_0.cubemeshentity) entity).color;
            }

            // Cylinder Mesh Entity.
            if (entity is Schema.V2_0.cylindermeshentity)
            {
                outputEntity = new Schema.V2_1.cylindermeshentity();
                ((Schema.V2_1.cylindermeshentity) outputEntity).color
                    = ((Schema.V2_0.cylindermeshentity) entity).color;
            }

            // HTML Entity.
            else if (entity is Schema.V2_0.htmlentity)
            {
                outputEntity = new Schema.V2_1.htmlentity();
                ((Schema.V2_1.htmlentity) outputEntity).onmessage
                    = ((Schema.V2_0.htmlentity) entity).onmessage;
                ((Schema.V2_1.htmlentity) outputEntity).url
                    = ((Schema.V2_0.htmlentity) entity).url;
            }

            // Image Entity.
            else if (entity is Schema.V2_0.imageentity)
            {
                outputEntity = new Schema.V2_1.imageentity();
                ((Schema.V2_1.imageentity) outputEntity).imagefile
                    = ((Schema.V2_0.imageentity) entity).imagefile;
            }

            // Input Entity.
            else if (entity is Schema.V2_0.inputentity)
            {
                outputEntity = new Schema.V2_1.inputentity();
            }

            // LightEntity.
            else if (entity is Schema.V2_0.lightentity)
            {
                outputEntity = new Schema.V2_1.lightentity();
            }

            // Mesh Entity.
            else if (entity is Schema.V2_0.meshentity)
            {
                outputEntity = new Schema.V2_1.meshentity();
                ((Schema.V2_1.meshentity) outputEntity).meshresource
                    = ((Schema.V2_0.meshentity) entity).meshresource;
                ((Schema.V2_1.meshentity) outputEntity).meshname
                    = ((Schema.V2_0.meshentity) entity).meshname;
            }

            // Plane Mesh Entity.
            if (entity is Schema.V2_0.planemeshentity)
            {
                outputEntity = new Schema.V2_1.planemeshentity();
                ((Schema.V2_1.planemeshentity) outputEntity).color
                    = ((Schema.V2_0.planemeshentity) entity).color;
            }

            // Prism Mesh Entity.
            if (entity is Schema.V2_0.prismmeshentity)
            {
                outputEntity = new Schema.V2_1.prismmeshentity();
                ((Schema.V2_1.prismmeshentity) outputEntity).color
                    = ((Schema.V2_0.prismmeshentity) entity).color;
            }

            // Rectangular Pyramid Mesh Entity.
            if (entity is Schema.V2_0.rectangularpyramidmeshentity)
            {
                outputEntity = new Schema.V2_1.rectangularpyramidmeshentity();
                ((Schema.V2_1.rectangularpyramidmeshentity) outputEntity).color
                    = ((Schema.V2_0.rectangularpyramidmeshentity) entity).color;
            }

            // Sphere Mesh Entity.
            if (entity is Schema.V2_0.spheremeshentity)
            {
                outputEntity = new Schema.V2_1.spheremeshentity();
                ((Schema.V2_1.spheremeshentity) outputEntity).color
                    = ((Schema.V2_0.spheremeshentity) entity).color;
            }

            // Terrain Entity.
            else if (entity is Schema.V2_0.terrainentity)
            {
                outputEntity = new Schema.V2_1.terrainentity();
                ((Schema.V2_1.terrainentity) outputEntity).height
                    = ((Schema.V2_0.terrainentity) entity).height;
                ((Schema.V2_1.terrainentity) outputEntity).heights
                    = ((Schema.V2_0.terrainentity) entity).heights;
                List<Schema.V2_1.terrainentitylayer> outputLayers
                    = new List<Schema.V2_1.terrainentitylayer>();
                if (((Schema.V2_0.terrainentity) entity).layer != null)
                {
                    foreach (Schema.V2_0.terrainentitylayer layer
                        in ((Schema.V2_0.terrainentity) entity).layer)
                    {
                        Schema.V2_1.terrainentitylayer outputLayer
                            = new Schema.V2_1.terrainentitylayer();
                        outputLayer.metallic = layer.metallic;
                        outputLayer.normaltexture = layer.normaltexture;
                        outputLayer.masktexture = layer.masktexture;
                        outputLayer.smoothness = layer.smoothness;
                        outputLayer.diffusetexture = layer.diffusetexture;
                        outputLayer.specular = layer.specular;
                        outputLayers.Add(outputLayer);
                    }
                }
                ((Schema.V2_1.terrainentity) outputEntity).layer
                    = outputLayers.ToArray();
                ((Schema.V2_1.terrainentity) outputEntity).layermasks
                    = ((Schema.V2_0.terrainentity) entity).layermasks;
                ((Schema.V2_1.terrainentity) outputEntity).length
                    = ((Schema.V2_0.terrainentity) entity).length;
                ((Schema.V2_1.terrainentity) outputEntity).width
                    = ((Schema.V2_0.terrainentity) entity).width;
                ((Schema.V2_1.terrainentity) outputEntity).type
                    = ((Schema.V2_0.terrainentity) entity).type;
            }

            // Tetrahedron Mesh Entity.
            if (entity is Schema.V2_0.tetrahedronmeshentity)
            {
                outputEntity = new Schema.V2_1.tetrahedronmeshentity();
                ((Schema.V2_1.tetrahedronmeshentity) outputEntity).color
                    = ((Schema.V2_0.tetrahedronmeshentity) entity).color;
            }

            // Text Entity.
            else if (entity is Schema.V2_0.textentity)
            {
                outputEntity = new Schema.V2_1.textentity();
                ((Schema.V2_1.textentity) outputEntity).text
                    = ((Schema.V2_0.textentity) entity).text;
                ((Schema.V2_1.textentity) outputEntity).fontsize
                    = ((Schema.V2_0.textentity) entity).fontsize;
            }

            // Torus Mesh Entity.
            if (entity is Schema.V2_0.torusmeshentity)
            {
                outputEntity = new Schema.V2_1.torusmeshentity();
                ((Schema.V2_1.torusmeshentity) outputEntity).color
                    = ((Schema.V2_0.torusmeshentity) entity).color;
            }

            // Voxel Entity.
            else if (entity is Schema.V2_0.voxelentity)
            {
                outputEntity = new Schema.V2_1.voxelentity();
            }

            // Default to Base Entity.
            else
            {
                outputEntity = new Schema.V2_1.entity();
            }

            // Assign id.
            outputEntity.id = entity.id;

            // Assign tag.
            outputEntity.tag = entity.tag;

            // Assign transform.
            if (entity.transform != null)
            {
                // Canvas Transform.
                if (entity.transform is Schema.V2_0.canvastransform)
                {
                    // Assign Canvas Transform.
                    outputEntity.transform = new Schema.V2_1.canvastransform();

                    // Assign Position Percent.
                    if (((Schema.V2_0.canvastransform) entity.transform).positionpercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent =
                            new Schema.V2_1.positionpercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.x =
                            ((Schema.V2_0.canvastransform) entity.transform).positionpercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.y =
                            ((Schema.V2_0.canvastransform) entity.transform).positionpercent.y;
                    }

                    // Assign Size Percent.
                    if (((Schema.V2_0.canvastransform) entity.transform).sizepercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent =
                            new Schema.V2_1.sizepercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.x =
                            ((Schema.V2_0.canvastransform) entity.transform).sizepercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.y =
                            ((Schema.V2_0.canvastransform) entity.transform).sizepercent.y;
                    }
                }

                // Scale Transform.
                else if (entity.transform is Schema.V2_0.scaletransform)
                {
                    // Assign Scale Transform.
                    outputEntity.transform = new Schema.V2_1.scaletransform();

                    // Assign Position.
                    if (((Schema.V2_0.scaletransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.x =
                            ((Schema.V2_0.scaletransform) entity.transform).position.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.y =
                            ((Schema.V2_0.scaletransform) entity.transform).position.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.z =
                            ((Schema.V2_0.scaletransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V2_0.scaletransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.x =
                            ((Schema.V2_0.scaletransform) entity.transform).rotation.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.y =
                            ((Schema.V2_0.scaletransform) entity.transform).rotation.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.z =
                            ((Schema.V2_0.scaletransform) entity.transform).rotation.z;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.w =
                            ((Schema.V2_0.scaletransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V2_0.scaletransform) entity.transform).scale != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale =
                            new Schema.V2_1.scale();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.x =
                            ((Schema.V2_0.scaletransform) entity.transform).scale.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.y =
                            ((Schema.V2_0.scaletransform) entity.transform).scale.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.z =
                            ((Schema.V2_0.scaletransform) entity.transform).scale.z;
                    }
                }

                // Size Transform.
                else if (entity.transform is Schema.V2_0.sizetransform)
                {
                    // Assign Size Transform.
                    outputEntity.transform = new Schema.V2_1.sizetransform();

                    // Assign Position.
                    if (((Schema.V2_0.sizetransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.x =
                            ((Schema.V2_0.sizetransform) entity.transform).position.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.y =
                            ((Schema.V2_0.sizetransform) entity.transform).position.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.z =
                            ((Schema.V2_0.sizetransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V2_0.sizetransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.x =
                            ((Schema.V2_0.sizetransform) entity.transform).rotation.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.y =
                            ((Schema.V2_0.sizetransform) entity.transform).rotation.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.z =
                            ((Schema.V2_0.sizetransform) entity.transform).rotation.z;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.w =
                            ((Schema.V2_0.sizetransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V2_0.sizetransform) entity.transform).size != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size =
                            new Schema.V2_1.size();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.x =
                            ((Schema.V2_0.sizetransform) entity.transform).size.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.y =
                            ((Schema.V2_0.sizetransform) entity.transform).size.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.z =
                            ((Schema.V2_0.sizetransform) entity.transform).size.z;
                    }
                }

                // Default to Base Transform.
                else
                {
                    outputEntity.transform = new Schema.V2_1.basetransform();
                }
            }

            // Assign On Load Event.
            outputEntity.onloadevent = entity.onloadevent;

            // Assign Synchronizer.
            outputEntity.synchronizer = entity.synchronizer;

            // Assign Placement Socket.
            List<Schema.V2_1.placementsocket> outputPlacementSockets = new List<Schema.V2_1.placementsocket>();
            if (entity.placementsocket != null)
            {
                foreach (Schema.V2_0.placementsocket placementSocket in entity.placementsocket)
                {
                    Schema.V2_1.placementsocket outputPlacementSocket = new Schema.V2_1.placementsocket();
                    outputPlacementSocket.position = new Schema.V2_1.position();
                    outputPlacementSocket.position.x = placementSocket.position.x;
                    outputPlacementSocket.position.y = placementSocket.position.y;
                    outputPlacementSocket.position.z = placementSocket.position.z;
                    outputPlacementSocket.rotation = new Schema.V2_1.rotation();
                    outputPlacementSocket.rotation.x = placementSocket.rotation.x;
                    outputPlacementSocket.rotation.y = placementSocket.rotation.y;
                    outputPlacementSocket.rotation.z = placementSocket.rotation.z;
                    outputPlacementSocket.rotation.w = placementSocket.rotation.w;
                    outputPlacementSocket.connectingoffset = new Schema.V2_1.position();
                    outputPlacementSocket.connectingoffset.x = placementSocket.connectingoffset.x;
                    outputPlacementSocket.connectingoffset.y = placementSocket.connectingoffset.y;
                    outputPlacementSocket.connectingoffset.z = placementSocket.connectingoffset.z;
                    outputPlacementSockets.Add(outputPlacementSocket);
                }
            }
            outputEntity.placementsocket = outputPlacementSockets.ToArray();

            return outputEntity;
        }

        /// <summary>
        /// Convert an entity from version 1.3 to the current schema (version 2.1).
        /// </summary>
        /// <param name="entity">Input entity instance.</param>
        /// <returns>Current schema version for the input entity instance.</returns>
        private static Schema.V2_1.entity ConvertEntityFromV1_3(Schema.V1_3.entity entity)
        {
            // Assign entity.
            Schema.V2_1.entity outputEntity;

            // Button Entity.
            if (entity is Schema.V1_3.buttonentity)
            {
                outputEntity = new Schema.V2_1.buttonentity();
                ((Schema.V2_1.buttonentity) outputEntity).onclickevent
                    = ((Schema.V1_3.buttonentity) entity).onclickevent;
            }

            // Canvas Entity.
            else if (entity is Schema.V1_3.canvasentity)
            {
                outputEntity = new Schema.V2_1.canvasentity();
            }

            // Character Entity.
            else if (entity is Schema.V1_3.characterentity)
            {
                outputEntity = new Schema.V2_1.characterentity();
                ((Schema.V2_1.characterentity) outputEntity).meshname
                    = ((Schema.V1_3.characterentity) entity).meshname;
                ((Schema.V2_1.characterentity) outputEntity).meshresource
                    = ((Schema.V1_3.characterentity) entity).meshresource;
                if (((Schema.V1_3.characterentity) entity).meshoffset != null)
                {
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset
                        = new Schema.V2_1.position();
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset.x
                        = ((Schema.V1_3.characterentity) entity).meshoffset.x;
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset.y
                        = ((Schema.V1_3.characterentity) entity).meshoffset.y;
                    ((Schema.V2_1.characterentity) outputEntity).meshoffset.z
                        = ((Schema.V1_3.characterentity) entity).meshoffset.z;
                }
                if (((Schema.V1_3.characterentity) entity).meshrotation != null)
                {
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation
                        = new Schema.V2_1.rotation();
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.x
                        = ((Schema.V1_3.characterentity) entity).meshrotation.x;
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.y
                        = ((Schema.V1_3.characterentity) entity).meshrotation.y;
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.z
                        = ((Schema.V1_3.characterentity) entity).meshrotation.z;
                    ((Schema.V2_1.characterentity) outputEntity).meshrotation.w
                        = ((Schema.V1_3.characterentity) entity).meshrotation.w;
                }
                if (((Schema.V1_3.characterentity) entity).labeloffset != null)
                {
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset
                        = new Schema.V2_1.position();
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset.x
                        = ((Schema.V1_3.characterentity) entity).labeloffset.x;
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset.y
                        = ((Schema.V1_3.characterentity) entity).labeloffset.y;
                    ((Schema.V2_1.characterentity) outputEntity).labeloffset.z
                        = ((Schema.V1_3.characterentity) entity).labeloffset.z;
                }
            }

            // Container Entity.
            else if (entity is Schema.V1_3.containerentity)
            {
                outputEntity = new Schema.V2_1.containerentity();
            }

            // HTML Entity.
            else if (entity is Schema.V1_3.htmlentity)
            {
                outputEntity = new Schema.V2_1.htmlentity();
                ((Schema.V2_1.htmlentity) outputEntity).onmessage
                    = ((Schema.V1_3.htmlentity) entity).onmessage;
                ((Schema.V2_1.htmlentity) outputEntity).url
                    = ((Schema.V1_3.htmlentity) entity).url;
            }

            // Input Entity.
            else if (entity is Schema.V1_3.inputentity)
            {
                outputEntity = new Schema.V2_1.inputentity();
            }

            // LightEntity.
            else if (entity is Schema.V1_3.lightentity)
            {
                outputEntity = new Schema.V2_1.lightentity();
            }

            // Mesh Entity.
            else if (entity is Schema.V1_3.meshentity)
            {
                outputEntity = new Schema.V2_1.meshentity();
                ((Schema.V2_1.meshentity) outputEntity).meshresource
                    = ((Schema.V1_3.meshentity) entity).meshresource;
                ((Schema.V2_1.meshentity) outputEntity).meshname
                    = ((Schema.V1_3.meshentity) entity).meshname;
            }

            // Terrain Entity.
            else if (entity is Schema.V1_3.terrainentity)
            {
                outputEntity = new Schema.V2_1.terrainentity();
                ((Schema.V2_1.terrainentity) outputEntity).height
                    = ((Schema.V1_3.terrainentity) entity).height;
                ((Schema.V2_1.terrainentity) outputEntity).heights
                    = ((Schema.V1_3.terrainentity) entity).heights;
                List<Schema.V2_1.terrainentitylayer> outputLayers
                    = new List<Schema.V2_1.terrainentitylayer>();
                if (((Schema.V1_3.terrainentity) entity).layer != null)
                {
                    foreach (Schema.V1_3.terrainentitylayer layer
                        in ((Schema.V1_3.terrainentity) entity).layer)
                    {
                        Schema.V2_1.terrainentitylayer outputLayer
                            = new Schema.V2_1.terrainentitylayer();
                        outputLayer.metallic = layer.metallic;
                        outputLayer.normaltexture = layer.normaltexture;
                        outputLayer.masktexture = layer.masktexture;
                        outputLayer.smoothness = layer.smoothness;
                        outputLayer.diffusetexture = layer.diffusetexture;
                        outputLayer.specular = layer.specular;
                        outputLayers.Add(outputLayer);
                    }
                }
                ((Schema.V2_1.terrainentity) outputEntity).layer
                    = outputLayers.ToArray();
                ((Schema.V2_1.terrainentity) outputEntity).layermasks
                    = ((Schema.V1_3.terrainentity) entity).layermasks;
                ((Schema.V2_1.terrainentity) outputEntity).length
                    = ((Schema.V1_3.terrainentity) entity).length;
                ((Schema.V2_1.terrainentity) outputEntity).width
                    = ((Schema.V1_3.terrainentity) entity).width;
                ((Schema.V2_1.terrainentity) outputEntity).type
                    = ((Schema.V1_3.terrainentity) entity).type;
            }

            // Text Entity.
            else if (entity is Schema.V1_3.textentity)
            {
                outputEntity = new Schema.V2_1.textentity();
                ((Schema.V2_1.textentity) outputEntity).text
                    = ((Schema.V1_3.textentity) entity).text;
                ((Schema.V2_1.textentity) outputEntity).fontsize
                    = ((Schema.V1_3.textentity) entity).fontsize;
            }

            // Voxel Entity.
            else if (entity is Schema.V1_3.voxelentity)
            {
                outputEntity = new Schema.V2_1.voxelentity();
            }

            // Default to Base Entity.
            else
            {
                outputEntity = new Schema.V2_1.entity();
            }

            // Assign id.
            outputEntity.id = entity.id;

            // Assign tag.
            outputEntity.tag = entity.tag;

            // Assign transform.
            if (entity.transform != null)
            {
                // Canvas Transform.
                if (entity.transform is Schema.V1_3.canvastransform)
                {
                    // Assign Canvas Transform.
                    outputEntity.transform = new Schema.V2_1.canvastransform();

                    // Assign Position Percent.
                    if (((Schema.V1_3.canvastransform) entity.transform).positionpercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent =
                            new Schema.V2_1.positionpercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.x =
                            ((Schema.V1_3.canvastransform) entity.transform).positionpercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.y =
                            ((Schema.V1_3.canvastransform) entity.transform).positionpercent.y;
                    }

                    // Assign Size Percent.
                    if (((Schema.V1_3.canvastransform) entity.transform).sizepercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent =
                            new Schema.V2_1.sizepercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.x =
                            ((Schema.V1_3.canvastransform) entity.transform).sizepercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.y =
                            ((Schema.V1_3.canvastransform) entity.transform).sizepercent.y;
                    }
                }

                // Scale Transform.
                else if (entity.transform is Schema.V1_3.scaletransform)
                {
                    // Assign Scale Transform.
                    outputEntity.transform = new Schema.V2_1.scaletransform();

                    // Assign Position.
                    if (((Schema.V1_3.scaletransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.x =
                            ((Schema.V1_3.scaletransform) entity.transform).position.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.y =
                            ((Schema.V1_3.scaletransform) entity.transform).position.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.z =
                            ((Schema.V1_3.scaletransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_3.scaletransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_3.scaletransform) entity.transform).rotation.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_3.scaletransform) entity.transform).rotation.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_3.scaletransform) entity.transform).rotation.z;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_3.scaletransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_3.scaletransform) entity.transform).scale != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale =
                            new Schema.V2_1.scale();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.x =
                            ((Schema.V1_3.scaletransform) entity.transform).scale.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.y =
                            ((Schema.V1_3.scaletransform) entity.transform).scale.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.z =
                            ((Schema.V1_3.scaletransform) entity.transform).scale.z;
                    }
                }

                // Size Transform.
                else if (entity.transform is Schema.V1_3.sizetransform)
                {
                    // Assign Size Transform.
                    outputEntity.transform = new Schema.V2_1.sizetransform();

                    // Assign Position.
                    if (((Schema.V1_3.sizetransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.x =
                            ((Schema.V1_3.sizetransform) entity.transform).position.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.y =
                            ((Schema.V1_3.sizetransform) entity.transform).position.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.z =
                            ((Schema.V1_3.sizetransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_3.sizetransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_3.sizetransform) entity.transform).rotation.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_3.sizetransform) entity.transform).rotation.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_3.sizetransform) entity.transform).rotation.z;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_3.sizetransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_3.sizetransform) entity.transform).size != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size =
                            new Schema.V2_1.size();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.x =
                            ((Schema.V1_3.sizetransform) entity.transform).size.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.y =
                            ((Schema.V1_3.sizetransform) entity.transform).size.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.z =
                            ((Schema.V1_3.sizetransform) entity.transform).size.z;
                    }
                }

                // Default to Base Transform.
                else
                {
                    outputEntity.transform = new Schema.V2_1.basetransform();
                }
            }

            // Assign On Load Event.
            outputEntity.onloadevent = entity.onloadevent;

            // Assign Synchronizer.
            outputEntity.synchronizer = entity.synchronizer;

            // Assign Placement Socket.
            List<Schema.V2_1.placementsocket> outputPlacementSockets = new List<Schema.V2_1.placementsocket>();
            if (entity.placementsocket != null)
            {
                foreach (Schema.V1_3.placementsocket placementSocket in entity.placementsocket)
                {
                    Schema.V2_1.placementsocket outputPlacementSocket = new Schema.V2_1.placementsocket();
                    outputPlacementSocket.position = new Schema.V2_1.position();
                    outputPlacementSocket.position.x = placementSocket.position.x;
                    outputPlacementSocket.position.y = placementSocket.position.y;
                    outputPlacementSocket.position.z = placementSocket.position.z;
                    outputPlacementSocket.rotation = new Schema.V2_1.rotation();
                    outputPlacementSocket.rotation.x = placementSocket.rotation.x;
                    outputPlacementSocket.rotation.y = placementSocket.rotation.y;
                    outputPlacementSocket.rotation.z = placementSocket.rotation.z;
                    outputPlacementSocket.rotation.w = placementSocket.rotation.w;
                    outputPlacementSocket.connectingoffset = new Schema.V2_1.position();
                    outputPlacementSocket.connectingoffset.x = placementSocket.connectingoffset.x;
                    outputPlacementSocket.connectingoffset.y = placementSocket.connectingoffset.y;
                    outputPlacementSocket.connectingoffset.z = placementSocket.connectingoffset.z;
                    outputPlacementSockets.Add(outputPlacementSocket);
                }
            }
            outputEntity.placementsocket = outputPlacementSockets.ToArray();

            return outputEntity;
        }

        /// <summary>
        /// Convert an entity from version 1.2 to the current schema (version 2.1).
        /// </summary>
        /// <param name="entity">Input entity instance.</param>
        /// <returns>Current schema version for the input entity instance.</returns>
        private static Schema.V2_1.entity ConvertEntityFromV1_2(Schema.V1_2.entity entity)
        {
            // Assign entity.
            Schema.V2_1.entity outputEntity;

            // Button Entity.
            if (entity is Schema.V1_2.buttonentity)
            {
                outputEntity = new Schema.V2_1.buttonentity();
                ((Schema.V2_1.buttonentity) outputEntity).onclickevent
                    = ((Schema.V1_2.buttonentity) entity).onclickevent;
            }

            // Canvas Entity.
            else if (entity is Schema.V1_2.canvasentity)
            {
                outputEntity = new Schema.V2_1.canvasentity();
            }

            // Character Entity.
            else if (entity is Schema.V1_2.characterentity)
            {
                outputEntity = new Schema.V2_1.characterentity();
            }

            // Container Entity.
            else if (entity is Schema.V1_2.containerentity)
            {
                outputEntity = new Schema.V2_1.containerentity();
            }

            // HTML Entity.
            else if (entity is Schema.V1_2.htmlentity)
            {
                outputEntity = new Schema.V2_1.htmlentity();
                ((Schema.V2_1.htmlentity) outputEntity).onmessage
                    = ((Schema.V1_2.htmlentity) entity).onmessage;
                ((Schema.V2_1.htmlentity) outputEntity).url
                    = ((Schema.V1_2.htmlentity) entity).url;
            }

            // Input Entity.
            else if (entity is Schema.V1_2.inputentity)
            {
                outputEntity = new Schema.V2_1.inputentity();
            }

            // LightEntity.
            else if (entity is Schema.V1_2.lightentity)
            {
                outputEntity = new Schema.V2_1.lightentity();
            }

            // Mesh Entity.
            else if (entity is Schema.V1_2.meshentity)
            {
                outputEntity = new Schema.V2_1.meshentity();
                ((Schema.V2_1.meshentity) outputEntity).meshresource
                    = ((Schema.V1_2.meshentity) entity).meshresource;
                ((Schema.V2_1.meshentity) outputEntity).meshname
                    = ((Schema.V1_2.meshentity) entity).meshname;
            }

            // Terrain Entity.
            else if (entity is Schema.V1_2.terrainentity)
            {
                outputEntity = new Schema.V2_1.terrainentity();
                ((Schema.V2_1.terrainentity) outputEntity).height
                    = ((Schema.V1_2.terrainentity) entity).height;
                ((Schema.V2_1.terrainentity) outputEntity).heights
                    = ((Schema.V1_2.terrainentity) entity).heights;
                List<Schema.V2_1.terrainentitylayer> outputLayers
                    = new List<Schema.V2_1.terrainentitylayer>();
                if (((Schema.V1_2.terrainentity) entity).layer != null)
                {
                    foreach (Schema.V1_2.terrainentitylayer layer
                        in ((Schema.V1_2.terrainentity) entity).layer)
                    {
                        Schema.V2_1.terrainentitylayer outputLayer
                            = new Schema.V2_1.terrainentitylayer();
                        outputLayer.metallic = layer.metallic;
                        outputLayer.normaltexture = layer.normaltexture;
                        outputLayer.masktexture = layer.masktexture;
                        outputLayer.smoothness = layer.smoothness;
                        outputLayer.diffusetexture = layer.diffusetexture;
                        outputLayer.specular = layer.specular;
                        outputLayers.Add(outputLayer);
                    }
                }
                ((Schema.V2_1.terrainentity) outputEntity).layer
                    = outputLayers.ToArray();
                ((Schema.V2_1.terrainentity) outputEntity).layermasks
                    = ((Schema.V1_2.terrainentity) entity).layermasks;
                ((Schema.V2_1.terrainentity) outputEntity).length
                    = ((Schema.V1_2.terrainentity) entity).length;
                ((Schema.V2_1.terrainentity) outputEntity).width
                    = ((Schema.V1_2.terrainentity) entity).width;
                ((Schema.V2_1.terrainentity) outputEntity).type
                    = ((Schema.V1_2.terrainentity) entity).type;
            }

            // Text Entity.
            else if (entity is Schema.V1_2.textentity)
            {
                outputEntity = new Schema.V2_1.textentity();
                ((Schema.V2_1.textentity) outputEntity).text
                    = ((Schema.V1_2.textentity) entity).text;
                ((Schema.V2_1.textentity) outputEntity).fontsize
                    = ((Schema.V1_2.textentity) entity).fontsize;
            }

            // Voxel Entity.
            else if (entity is Schema.V1_2.voxelentity)
            {
                outputEntity = new Schema.V2_1.voxelentity();
            }

            // Default to Base Entity.
            else
            {
                outputEntity = new Schema.V2_1.entity();
            }

            // Assign id.
            outputEntity.id = entity.id;

            // Assign tag.
            outputEntity.tag = entity.tag;

            // Assign transform.
            if (entity.transform != null)
            {
                // Canvas Transform.
                if (entity.transform is Schema.V1_2.canvastransform)
                {
                    // Assign Canvas Transform.
                    outputEntity.transform = new Schema.V2_1.canvastransform();

                    // Assign Position Percent.
                    if (((Schema.V1_2.canvastransform) entity.transform).positionpercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent =
                            new Schema.V2_1.positionpercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.x =
                            ((Schema.V1_2.canvastransform) entity.transform).positionpercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.y =
                            ((Schema.V1_2.canvastransform) entity.transform).positionpercent.y;
                    }

                    // Assign Size Percent.
                    if (((Schema.V1_2.canvastransform) entity.transform).sizepercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent =
                            new Schema.V2_1.sizepercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.x =
                            ((Schema.V1_2.canvastransform) entity.transform).sizepercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.y =
                            ((Schema.V1_2.canvastransform) entity.transform).sizepercent.y;
                    }
                }

                // Scale Transform.
                else if (entity.transform is Schema.V1_2.scaletransform)
                {
                    // Assign Scale Transform.
                    outputEntity.transform = new Schema.V2_1.scaletransform();

                    // Assign Position.
                    if (((Schema.V1_2.scaletransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.x =
                            ((Schema.V1_2.scaletransform) entity.transform).position.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.y =
                            ((Schema.V1_2.scaletransform) entity.transform).position.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.z =
                            ((Schema.V1_2.scaletransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_2.scaletransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_2.scaletransform) entity.transform).rotation.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_2.scaletransform) entity.transform).rotation.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_2.scaletransform) entity.transform).rotation.z;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_2.scaletransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_2.scaletransform) entity.transform).scale != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale =
                            new Schema.V2_1.scale();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.x =
                            ((Schema.V1_2.scaletransform) entity.transform).scale.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.y =
                            ((Schema.V1_2.scaletransform) entity.transform).scale.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.z =
                            ((Schema.V1_2.scaletransform) entity.transform).scale.z;
                    }
                }

                // Size Transform.
                else if (entity.transform is Schema.V1_2.sizetransform)
                {
                    // Assign Size Transform.
                    outputEntity.transform = new Schema.V2_1.sizetransform();

                    // Assign Position.
                    if (((Schema.V1_2.sizetransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.x =
                            ((Schema.V1_2.sizetransform) entity.transform).position.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.y =
                            ((Schema.V1_2.sizetransform) entity.transform).position.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.z =
                            ((Schema.V1_2.sizetransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_2.sizetransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_2.sizetransform) entity.transform).rotation.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_2.sizetransform) entity.transform).rotation.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_2.sizetransform) entity.transform).rotation.z;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_2.sizetransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_2.sizetransform) entity.transform).size != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size =
                            new Schema.V2_1.size();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.x =
                            ((Schema.V1_2.sizetransform) entity.transform).size.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.y =
                            ((Schema.V1_2.sizetransform) entity.transform).size.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.z =
                            ((Schema.V1_2.sizetransform) entity.transform).size.z;
                    }
                }

                // Default to Base Transform.
                else
                {
                    outputEntity.transform = new Schema.V2_1.basetransform();
                }
            }

            // Assign On Load Event.
            outputEntity.onloadevent = entity.onloadevent;

            // Assign Synchronizer.
            outputEntity.synchronizer = entity.synchronizer;

            // Assign Placement Socket.
            List<Schema.V2_1.placementsocket> outputPlacementSockets = new List<Schema.V2_1.placementsocket>();
            if (entity.placementsocket != null)
            {
                foreach (Schema.V1_2.placementsocket placementSocket in entity.placementsocket)
                {
                    Schema.V2_1.placementsocket outputPlacementSocket = new Schema.V2_1.placementsocket();
                    outputPlacementSocket.position = new Schema.V2_1.position();
                    outputPlacementSocket.position.x = placementSocket.position.x;
                    outputPlacementSocket.position.y = placementSocket.position.y;
                    outputPlacementSocket.position.z = placementSocket.position.z;
                    outputPlacementSocket.rotation = new Schema.V2_1.rotation();
                    outputPlacementSocket.rotation.x = placementSocket.rotation.x;
                    outputPlacementSocket.rotation.y = placementSocket.rotation.y;
                    outputPlacementSocket.rotation.z = placementSocket.rotation.z;
                    outputPlacementSocket.rotation.w = placementSocket.rotation.w;
                    outputPlacementSocket.connectingoffset = new Schema.V2_1.position();
                    outputPlacementSocket.connectingoffset.x = placementSocket.connectingoffset.x;
                    outputPlacementSocket.connectingoffset.y = placementSocket.connectingoffset.y;
                    outputPlacementSocket.connectingoffset.z = placementSocket.connectingoffset.z;
                    outputPlacementSockets.Add(outputPlacementSocket);
                }
            }
            outputEntity.placementsocket = outputPlacementSockets.ToArray();

            return outputEntity;
        }

        /// <summary>
        /// Convert an entity from version 1.1 to the current schema (version 2.1).
        /// </summary>
        /// <param name="entity">Input entity instance.</param>
        /// <returns>Current schema version for the input entity instance.</returns>
        private static Schema.V2_1.entity ConvertEntityFromV1_1(Schema.V1_1.entity entity)
        {
            // Assign entity.
            Schema.V2_1.entity outputEntity;

            // Button Entity.
            if (entity is Schema.V1_1.buttonentity)
            {
                outputEntity = new Schema.V2_1.buttonentity();
                ((Schema.V2_1.buttonentity) outputEntity).onclickevent
                    = ((Schema.V1_1.buttonentity) entity).onclickevent;
            }

            // Canvas Entity.
            else if (entity is Schema.V1_1.canvasentity)
            {
                outputEntity = new Schema.V2_1.canvasentity();
            }

            // Character Entity.
            else if (entity is Schema.V1_1.characterentity)
            {
                outputEntity = new Schema.V2_1.characterentity();
            }

            // Container Entity.
            else if (entity is Schema.V1_1.containerentity)
            {
                outputEntity = new Schema.V2_1.containerentity();
            }

            // Input Entity.
            else if (entity is Schema.V1_1.inputentity)
            {
                outputEntity = new Schema.V2_1.inputentity();
            }

            // LightEntity.
            else if (entity is Schema.V1_1.lightentity)
            {
                outputEntity = new Schema.V2_1.lightentity();
            }

            // Mesh Entity.
            else if (entity is Schema.V1_1.meshentity)
            {
                outputEntity = new Schema.V2_1.meshentity();
                ((Schema.V2_1.meshentity) outputEntity).meshresource
                    = ((Schema.V1_1.meshentity) entity).meshresource;
                ((Schema.V2_1.meshentity) outputEntity).meshname
                    = ((Schema.V1_1.meshentity) entity).meshname;
            }

            // Terrain Entity.
            else if (entity is Schema.V1_1.terrainentity)
            {
                outputEntity = new Schema.V2_1.terrainentity();
                ((Schema.V2_1.terrainentity) outputEntity).height
                    = ((Schema.V1_1.terrainentity) entity).height;
                ((Schema.V2_1.terrainentity) outputEntity).heights
                    = ((Schema.V1_1.terrainentity) entity).heights;
                ((Schema.V2_1.terrainentity) outputEntity).layer = null;
                ((Schema.V2_1.terrainentity) outputEntity).layermasks = null;
                ((Schema.V2_1.terrainentity) outputEntity).length
                    = ((Schema.V1_1.terrainentity) entity).length;
                ((Schema.V2_1.terrainentity) outputEntity).width
                    = ((Schema.V1_1.terrainentity) entity).width;
            }

            // Text Entity.
            else if (entity is Schema.V1_1.textentity)
            {
                outputEntity = new Schema.V2_1.textentity();
                ((Schema.V2_1.textentity) outputEntity).text
                    = ((Schema.V1_1.textentity) entity).text;
                ((Schema.V2_1.textentity) outputEntity).fontsize
                    = ((Schema.V1_1.textentity) entity).fontsize;
            }

            // Voxel Entity.
            else if (entity is Schema.V1_1.voxelentity)
            {
                outputEntity = new Schema.V2_1.voxelentity();
            }

            // Default to Base Entity.
            else
            {
                outputEntity = new Schema.V2_1.entity();
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
                    outputEntity.transform = new Schema.V2_1.canvastransform();

                    // Assign Position Percent.
                    if (((Schema.V1_1.canvastransform) entity.transform).positionpercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent =
                            new Schema.V2_1.positionpercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.x =
                            ((Schema.V1_1.canvastransform) entity.transform).positionpercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.y =
                            ((Schema.V1_1.canvastransform) entity.transform).positionpercent.y;
                    }

                    // Assign Size Percent.
                    if (((Schema.V1_1.canvastransform) entity.transform).sizepercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent =
                            new Schema.V2_1.sizepercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.x =
                            ((Schema.V1_1.canvastransform) entity.transform).sizepercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.y =
                            ((Schema.V1_1.canvastransform) entity.transform).sizepercent.y;
                    }
                }

                // Scale Transform.
                else if (entity.transform is Schema.V1_1.scaletransform)
                {
                    // Assign Scale Transform.
                    outputEntity.transform = new Schema.V2_1.scaletransform();

                    // Assign Position.
                    if (((Schema.V1_1.scaletransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.x =
                            ((Schema.V1_1.scaletransform) entity.transform).position.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.y =
                            ((Schema.V1_1.scaletransform) entity.transform).position.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.z =
                            ((Schema.V1_1.scaletransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_1.scaletransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.z;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_1.scaletransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_1.scaletransform) entity.transform).scale != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale =
                            new Schema.V2_1.scale();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.x =
                            ((Schema.V1_1.scaletransform) entity.transform).scale.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.y =
                            ((Schema.V1_1.scaletransform) entity.transform).scale.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.z =
                            ((Schema.V1_1.scaletransform) entity.transform).scale.z;
                    }
                }

                // Size Transform.
                else if (entity.transform is Schema.V1_1.sizetransform)
                {
                    // Assign Size Transform.
                    outputEntity.transform = new Schema.V2_1.sizetransform();

                    // Assign Position.
                    if (((Schema.V1_1.sizetransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.x =
                            ((Schema.V1_1.sizetransform) entity.transform).position.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.y =
                            ((Schema.V1_1.sizetransform) entity.transform).position.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.z =
                            ((Schema.V1_1.sizetransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_1.sizetransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.z;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_1.sizetransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_1.sizetransform) entity.transform).size != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size =
                            new Schema.V2_1.size();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.x =
                            ((Schema.V1_1.sizetransform) entity.transform).size.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.y =
                            ((Schema.V1_1.sizetransform) entity.transform).size.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.z =
                            ((Schema.V1_1.sizetransform) entity.transform).size.z;
                    }
                }

                // Default to Base Transform.
                else
                {
                    outputEntity.transform = new Schema.V2_1.basetransform();
                }
            }

            // Assign On Load Event.
            outputEntity.onloadevent = entity.onloadevent;

            // Assign Synchronizer.
            outputEntity.synchronizer = entity.synchronizer;

            // Assign Placement Socket.
            List<Schema.V2_1.placementsocket> outputPlacementSockets = new List<Schema.V2_1.placementsocket>();
            if (entity.placementsocket != null)
            {
                foreach (Schema.V1_1.placementsocket placementSocket in entity.placementsocket)
                {
                    Schema.V2_1.placementsocket outputPlacementSocket = new Schema.V2_1.placementsocket();
                    outputPlacementSocket.position = new Schema.V2_1.position();
                    outputPlacementSocket.position.x = placementSocket.position.x;
                    outputPlacementSocket.position.y = placementSocket.position.y;
                    outputPlacementSocket.position.z = placementSocket.position.z;
                    outputPlacementSocket.rotation = new Schema.V2_1.rotation();
                    outputPlacementSocket.rotation.x = placementSocket.rotation.x;
                    outputPlacementSocket.rotation.y = placementSocket.rotation.y;
                    outputPlacementSocket.rotation.z = placementSocket.rotation.z;
                    outputPlacementSocket.rotation.w = placementSocket.rotation.w;
                    outputPlacementSocket.connectingoffset = new Schema.V2_1.position();
                    outputPlacementSocket.connectingoffset.x = placementSocket.connectingoffset.x;
                    outputPlacementSocket.connectingoffset.y = placementSocket.connectingoffset.y;
                    outputPlacementSocket.connectingoffset.z = placementSocket.connectingoffset.z;
                    outputPlacementSockets.Add(outputPlacementSocket);
                }
            }
            outputEntity.placementsocket = outputPlacementSockets.ToArray();

            return outputEntity;
        }

        /// <summary>
        /// Convert an entity from version 1.0 to the current schema (version 2.1).
        /// </summary>
        /// <param name="entity">Input entity instance.</param>
        /// <returns>Current schema version for the input entity instance.</returns>
        private static Schema.V2_1.entity ConvertEntityFromV1_0(Schema.V1_0.entity entity)
        {
            // Assign entity.
            Schema.V2_1.entity outputEntity;

            // Button Entity.
            if (entity is Schema.V1_0.buttonentity)
            {
                outputEntity = new Schema.V2_1.buttonentity();
                ((Schema.V2_1.buttonentity) outputEntity).onclickevent
                    = ((Schema.V1_0.buttonentity) entity).onclickevent;
            }

            // Canvas Entity.
            else if (entity is Schema.V1_0.canvasentity)
            {
                outputEntity = new Schema.V2_1.canvasentity();
            }

            // Character Entity.
            else if (entity is Schema.V1_0.characterentity)
            {
                outputEntity = new Schema.V2_1.characterentity();
            }

            // Container Entity.
            else if (entity is Schema.V1_0.containerentity)
            {
                outputEntity = new Schema.V2_1.containerentity();
            }

            // Input Entity.
            else if (entity is Schema.V1_0.inputentity)
            {
                outputEntity = new Schema.V2_1.inputentity();
            }

            // LightEntity.
            else if (entity is Schema.V1_0.lightentity)
            {
                outputEntity = new Schema.V2_1.lightentity();
            }

            // Mesh Entity.
            else if (entity is Schema.V1_0.meshentity)
            {
                outputEntity = new Schema.V2_1.meshentity();
                ((Schema.V2_1.meshentity) outputEntity).meshresource
                    = ((Schema.V1_0.meshentity) entity).meshresource;
                ((Schema.V2_1.meshentity) outputEntity).meshname
                    = ((Schema.V1_0.meshentity) entity).meshname;
            }

            // Terrain Entity.
            else if (entity is Schema.V1_0.terrainentity)
            {
                outputEntity = new Schema.V2_1.terrainentity();
                ((Schema.V2_1.terrainentity) outputEntity).height
                    = ((Schema.V1_0.terrainentity) entity).height;
                ((Schema.V2_1.terrainentity) outputEntity).heights
                    = ((Schema.V1_0.terrainentity) entity).heights;
                ((Schema.V2_1.terrainentity) outputEntity).layer = null;
                ((Schema.V2_1.terrainentity) outputEntity).layermasks = null;
                ((Schema.V2_1.terrainentity) outputEntity).length
                    = ((Schema.V1_0.terrainentity) entity).length;
                ((Schema.V2_1.terrainentity) outputEntity).width
                    = ((Schema.V1_0.terrainentity) entity).width;
            }

            // Text Entity.
            else if (entity is Schema.V1_0.textentity)
            {
                outputEntity = new Schema.V2_1.textentity();
                ((Schema.V2_1.textentity) outputEntity).text
                    = ((Schema.V1_0.textentity) entity).text;
                ((Schema.V2_1.textentity) outputEntity).fontsize
                    = ((Schema.V1_0.textentity) entity).fontsize;
            }

            // Voxel Entity.
            else if (entity is Schema.V1_0.voxelentity)
            {
                outputEntity = new Schema.V2_1.voxelentity();
            }

            // Default to Base Entity.
            else
            {
                outputEntity = new Schema.V2_1.entity();
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
                    outputEntity.transform = new Schema.V2_1.canvastransform();

                    // Assign Position Percent.
                    if (((Schema.V1_0.canvastransform) entity.transform).positionpercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent =
                            new Schema.V2_1.positionpercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.x =
                            ((Schema.V1_0.canvastransform) entity.transform).positionpercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).positionpercent.y =
                            ((Schema.V1_0.canvastransform) entity.transform).positionpercent.y;
                    }

                    // Assign Size Percent.
                    if (((Schema.V1_0.canvastransform) entity.transform).sizepercent != null)
                    {
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent =
                            new Schema.V2_1.sizepercent();
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.x =
                            ((Schema.V1_0.canvastransform) entity.transform).sizepercent.x;
                        ((Schema.V2_1.canvastransform) outputEntity.transform).sizepercent.y =
                            ((Schema.V1_0.canvastransform) entity.transform).sizepercent.y;
                    }
                }

                // Scale Transform.
                else if (entity.transform is Schema.V1_0.scaletransform)
                {
                    // Assign Scale Transform.
                    outputEntity.transform = new Schema.V2_1.scaletransform();

                    // Assign Position.
                    if (((Schema.V1_0.scaletransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.x =
                            ((Schema.V1_0.scaletransform) entity.transform).position.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.y =
                            ((Schema.V1_0.scaletransform) entity.transform).position.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).position.z =
                            ((Schema.V1_0.scaletransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_0.scaletransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.z;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_0.scaletransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_0.scaletransform) entity.transform).scale != null)
                    {
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale =
                            new Schema.V2_1.scale();
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.x =
                            ((Schema.V1_0.scaletransform) entity.transform).scale.x;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.y =
                            ((Schema.V1_0.scaletransform) entity.transform).scale.y;
                        ((Schema.V2_1.scaletransform) outputEntity.transform).scale.z =
                            ((Schema.V1_0.scaletransform) entity.transform).scale.z;
                    }
                }

                // Size Transform.
                else if (entity.transform is Schema.V1_0.sizetransform)
                {
                    // Assign Size Transform.
                    outputEntity.transform = new Schema.V2_1.sizetransform();

                    // Assign Position.
                    if (((Schema.V1_0.sizetransform) entity.transform).position != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position =
                            new Schema.V2_1.position();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.x =
                            ((Schema.V1_0.sizetransform) entity.transform).position.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.y =
                            ((Schema.V1_0.sizetransform) entity.transform).position.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).position.z =
                            ((Schema.V1_0.sizetransform) entity.transform).position.z;
                    }

                    // Assign Rotation.
                    if (((Schema.V1_0.sizetransform) entity.transform).rotation != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation =
                            new Schema.V2_1.rotation();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.x =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.y =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.z =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.z;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).rotation.w =
                            ((Schema.V1_0.sizetransform) entity.transform).rotation.w;
                    }

                    // Assign Scale.
                    if (((Schema.V1_0.sizetransform) entity.transform).size != null)
                    {
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size =
                            new Schema.V2_1.size();
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.x =
                            ((Schema.V1_0.sizetransform) entity.transform).size.x;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.y =
                            ((Schema.V1_0.sizetransform) entity.transform).size.y;
                        ((Schema.V2_1.sizetransform) outputEntity.transform).size.z =
                            ((Schema.V1_0.sizetransform) entity.transform).size.z;
                    }
                }

                // Default to Base Transform.
                else
                {
                    outputEntity.transform = new Schema.V2_1.basetransform();
                }
            }

            // Assign On Load Event.
            outputEntity.onloadevent = entity.onloadevent;

            // Assign Synchronizer.
            outputEntity.synchronizer = entity.synchronizer;

            return outputEntity;
        }

        /// <summary>
        /// Add an entity to a Version 2.1 entity array.
        /// </summary>
        /// <param name="baseArray">Initial array.</param>
        /// <param name="entityToAdd">Entity to add to array.</param>
        /// <returns>The input array with the specified entity added.</returns>
        private static Schema.V2_1.entity[] AddToV2_1EntityArray(Schema.V2_1.entity[] baseArray,
            Schema.V2_1.entity entityToAdd)
        {
            List<Schema.V2_1.entity> entityList;
            if (baseArray == null)
            {
                entityList = new List<Schema.V2_1.entity>();
            }
            else
            {
                entityList = new List<Schema.V2_1.entity>(baseArray);
            }
            entityList.Add(entityToAdd);
            return entityList.ToArray();
        }
    }
}