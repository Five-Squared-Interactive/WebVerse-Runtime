// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a light entity.
    /// </summary>
    public class LightEntity : BaseEntity
    {
        /// <summary>
        /// Light Properties class.
        /// </summary>
        public class LightProperties
        {
            /// <summary>
            /// Color of the light.
            /// </summary>
            public Color32 color;

            /// <summary>
            /// Temperature of the light.
            /// </summary>
            public int temperature;

            /// <summary>
            /// Intensity of the light.
            /// </summary>
            public float intensity;

            /// <summary>
            /// Range of the light.
            /// </summary>
            public float range;

            /// <summary>
            /// Inner spot angle for the light.
            /// </summary>
            public float innerSpotAngle;

            /// <summary>
            /// Outer spot angle for the light.
            /// </summary>
            public float outerSpotAngle;
        }

        /// <summary>
        /// Enumeration for a light type:
        /// Directional: Light is emitted in a given direction.
        /// Spot: A spot light is cast in a given direction.
        /// Point: Light is cast in all directions from a given point.
        /// </summary>
        public enum LightType { Directional, Spot, Point }

        /// <summary>
        /// The light object for the entity.
        /// </summary>
        private Light lightObject;

        /// <summary>
        /// The current light type.
        /// </summary>
        private LightType lightType;

        /// <summary>
        /// The current light color.
        /// </summary>
        private Color32 lightColor;

        /// <summary>
        /// The current light temperature.
        /// </summary>
        private int lightTemperature;

        /// <summary>
        /// The current light intensity.
        /// </summary>
        private float lightIntensity;

        /// <summary>
        /// The current light range.
        /// </summary>
        private float lightRange;

        /// <summary>
        /// The current inner spot angle for the light.
        /// </summary>
        private float lightInnerSpotAngle;

        /// <summary>
        /// The current outer spot angle for the light.
        /// </summary>
        private float lightOuterSpotAngle;

        /// <summary>
        /// Get the motion state for this entity.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public override EntityMotion? GetMotion()
        {
            return new EntityMotion
            {
                angularVelocity = Vector3.zero,
                stationary = true,
                velocity = Vector3.zero
            };
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            return new EntityPhysicalProperties
            {
                angularDrag = 0,
                centerOfMass = Vector3.zero,
                drag = 0,
                gravitational = false,
                mass = 0
            };
        }

        /// <summary>
        /// Get the size of the entity.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Get the light type for the light entity.
        /// </summary>
        /// <returns>The current light type of the light entity.</returns>
        public LightType GetLightType()
        {
            return lightType;
        }

        /// <summary>
        /// Set the interaction state for the entity.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetInteractionState(InteractionState stateToSet)
        {
            switch (stateToSet)
            {
                case InteractionState.Physical:
                    LogSystem.LogWarning("[LightEntity->SetInteractionState] Physical not valid for light.");
                    return false;

                case InteractionState.Placing:
                    LogSystem.LogWarning("[LightEntity->SetInteractionState] Placing not valid for light.");
                    return false;

                case InteractionState.Static:
                    MakeStatic();
                    return true;

                case InteractionState.Hidden:
                    MakeHidden();
                    return true;

                default:
                    LogSystem.LogWarning("LightEntity->SetInteractionState] Interaction state invalid.");
                    return false;
            }
        }

        /// <summary>
        /// Set the motion state for this entity.
        /// </summary>
        /// <param name="motionToSet">Motion state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetMotion(EntityMotion? motionToSet)
        {
            LogSystem.LogWarning("[LightEntity->SetMotion] Motion not settable for light.");

            return false;
        }

        /// <summary>
        /// Set the physical properties of the entity.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPhysicalProperties(EntityPhysicalProperties? epp)
        {
            LogSystem.LogWarning("[LightEntity->SetPhysicalProperties] Physical properties not settable for light.");

            return false;
        }

        /// <summary>
        /// Set the size of the entity.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetSize(Vector3 size, bool synchronize = true)
        {
            LogSystem.LogWarning("[LightEntity->SetSize] Size not settable for light.");

            return false;
        }

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetVisibility(bool visible, bool synchronize = true)
        {
            // Use base functionality.
            return base.SetVisibility(visible, synchronize);
        }

        /// <summary>
        /// Set the light type for the light entity.
        /// </summary>
        /// <param name="type">Light type to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightType(LightType type)
        {
            switch (type)
            {
                case LightType.Directional:
                    lightObject.type = UnityEngine.LightType.Directional;
                    lightObject.useColorTemperature = true;
                    lightObject.color = lightColor;
                    lightObject.colorTemperature = lightTemperature;
//#if !UNITY_WEBGL
//                    lightObject.lightmapBakeType = LightmapBakeType.Realtime;
//#endif
                    lightObject.intensity = lightIntensity;
                    lightObject.shadows = LightShadows.Soft;
                    break;

                case LightType.Point:
                    lightObject.type = UnityEngine.LightType.Point;
                    lightObject.range = lightRange;
                    lightObject.innerSpotAngle = lightInnerSpotAngle;
                    lightObject.spotAngle = lightOuterSpotAngle;
                    lightObject.useColorTemperature = true;
                    lightObject.color = lightColor;
                    lightObject.colorTemperature = lightTemperature;
                    lightObject.intensity = lightIntensity;
                    break;

                case LightType.Spot:
                    lightObject.type = UnityEngine.LightType.Spot;
                    lightObject.range = lightRange;
                    lightObject.useColorTemperature = true;
                    lightObject.intensity = lightIntensity;
                    break;

                default:
                    LogSystem.LogWarning("[LightEntity->SetLightType] Unknown light type.");
                    break;
            }

            lightType = type;

            return true;
        }

        /// <summary>
        /// Set the properties for the light.
        /// </summary>
        /// <param name="color">Color to apply to the light.</param>
        /// <param name="temperature">Temperature to apply to the light.</param>
        /// <param name="intensity">Intensity to apply to the light.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightProperties(Color32 color, int temperature, float intensity)
        {
            lightColor = color;
            lightTemperature = temperature;
            lightIntensity = intensity;
            SetLightType(lightType);

            return true;
        }

        /// <summary>
        /// Set the properties for the light.
        /// </summary>
        /// <param name="range">Range to apply to the light.</param>
        /// <param name="innerSpotAngle">Inner spot angle to apply to the light.</param>
        /// <param name="outerSpotAngle">Outer spot angle to apply to the light.</param>
        /// <param name="color">Color to apply to the light.</param>
        /// <param name="temperature">Temperature to apply to the light.</param>
        /// <param name="intensity">Intensity to apply to the light.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightProperties(float range, float innerSpotAngle, float outerSpotAngle,
            Color32 color, int temperature, float intensity)
        {
            lightRange = range;
            lightInnerSpotAngle = innerSpotAngle;
            lightOuterSpotAngle = outerSpotAngle;
            lightColor = color;
            lightTemperature = temperature;
            lightIntensity = intensity;
            SetLightType(lightType);

            return true;
        }

        /// <summary>
        /// Set the properties for the light.
        /// </summary>
        /// <param name="range">Range to apply to the light.</param>
        /// <param name="intensity">Intensity to apply to the light.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightProperties(float range, float intensity)
        {
            lightRange = range;
            lightIntensity = intensity;
            SetLightType(lightType);

            return true;
        }

        /// <summary>
        /// Get the properties for the light.
        /// </summary>
        /// <returns>The light properties of the light.</returns>
        public LightProperties GetLightProperties()
        {
            return new LightProperties()
            {
                color = lightColor,
                temperature = lightTemperature,
                intensity = lightIntensity,
                range = lightRange,
                innerSpotAngle = lightInnerSpotAngle,
                outerSpotAngle = lightOuterSpotAngle
            };
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(System.Guid idToSet)
        {
            base.Initialize(idToSet);

            lightObject = gameObject.AddComponent<Light>();
            gameObject.transform.localScale = Vector3.one;
            lightColor = Color.white;
            lightTemperature = 5000;
            lightIntensity = 1;
            SetLightType(lightType);

            MakeHidden();
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public override void TearDown()
        {
            base.TearDown();
        }

        /// <summary>
        /// Make the entity hidden.
        /// </summary>
        private void MakeHidden()
        {
            switch (interactionState)
            {
                case InteractionState.Physical:
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Hidden:
                default:
                    break;
            }

            gameObject.SetActive(false);
            interactionState = InteractionState.Hidden;
        }

        /// <summary>
        /// Make the entity static.
        /// </summary>
        private void MakeStatic()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                default:
                    break;
            }

            gameObject.SetActive(true);
            interactionState = InteractionState.Static;
        }
    }
}