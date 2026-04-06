// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Handlers.Voice;
using FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0;

/// <summary>
/// Unit tests for VoiceConfig, VoiceErrorCode, and VoiceException.
/// </summary>
public class VoiceConfigTests
{
    #region Story 1.1: Configuration Validation Tests

    [Test]
    public void ValidConfig_WithRequiredFields_PassesValidation()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = "wss://voice.example.com/v1"
        };

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => config.Validate());
    }

    [Test]
    public void ValidConfig_WithDefaults_AppliesCorrectDefaults()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = "wss://voice.example.com/v1"
        };

        // Assert defaults
        Assert.AreEqual("websocket", config.Provider);
        Assert.AreEqual(1f, config.MinDistance);
        Assert.AreEqual(25f, config.MaxDistance);
        Assert.AreEqual("logarithmic", config.Rolloff);
    }

    [Test]
    public void InvalidConfig_MissingEndpoint_ThrowsVoiceException()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Provider = "websocket"
            // Endpoint not set
        };

        // Act & Assert
        var ex = Assert.Throws<VoiceException>(() => config.Validate());
        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
        Assert.That(ex.Message, Does.Contain("endpoint"));
    }

    [Test]
    public void InvalidConfig_EmptyEndpoint_ThrowsVoiceException()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = ""
        };

        // Act & Assert
        var ex = Assert.Throws<VoiceException>(() => config.Validate());
        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void InvalidConfig_MinGreaterThanMax_ThrowsVoiceException()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = "wss://voice.example.com/v1",
            MinDistance = 50f,
            MaxDistance = 25f
        };

        // Act & Assert
        var ex = Assert.Throws<VoiceException>(() => config.Validate());
        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
        Assert.That(ex.Message, Does.Contain("MinDistance").IgnoreCase);
    }

    [Test]
    public void InvalidConfig_NegativeMinDistance_ThrowsVoiceException()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = "wss://voice.example.com/v1",
            MinDistance = -1f
        };

        // Act & Assert
        var ex = Assert.Throws<VoiceException>(() => config.Validate());
        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void VoiceErrorCode_HasExpectedValues()
    {
        // Assert all expected error codes exist
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceErrorCode), VoiceErrorCode.VOICE_INVALID_CONFIG));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceErrorCode), VoiceErrorCode.VOICE_CONNECTION_FAILED));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceErrorCode), VoiceErrorCode.VOICE_CONNECTION_LOST));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceErrorCode), VoiceErrorCode.VOICE_PERMISSION_DENIED));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceErrorCode), VoiceErrorCode.VOICE_ENCODING_ERROR));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceErrorCode), VoiceErrorCode.VOICE_DECODING_ERROR));
    }

    [Test]
    public void VoiceException_StoresErrorCode()
    {
        // Arrange & Act
        var ex = new VoiceException(VoiceErrorCode.VOICE_CONNECTION_FAILED, "Connection failed");

        // Assert
        Assert.AreEqual(VoiceErrorCode.VOICE_CONNECTION_FAILED, ex.ErrorCode);
        Assert.AreEqual("Connection failed", ex.Message);
    }

    #endregion

    #region Story 1.2: VEML Parsing Tests

    [Test]
    public void ParseVoiceConfig_ValidVEML_ReturnsConfig()
    {
        // Arrange
        var vemlVoice = new voice
        {
            provider = "websocket",
            endpoint = "wss://voice.example.com/v1"
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNotNull(config);
        Assert.AreEqual("websocket", config.Provider);
        Assert.AreEqual("wss://voice.example.com/v1", config.Endpoint);
    }

    [Test]
    public void ParseVoiceConfig_WithSpatialParams_IncludesCustomSettings()
    {
        // Arrange
        var vemlVoice = new voice
        {
            provider = "websocket",
            endpoint = "wss://voice.example.com/v1",
            mindistance = 2f,
            mindistanceSpecified = true,
            maxdistance = 50f,
            maxdistanceSpecified = true,
            rolloff = "linear"
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNotNull(config);
        Assert.AreEqual(2f, config.MinDistance);
        Assert.AreEqual(50f, config.MaxDistance);
        Assert.AreEqual("linear", config.Rolloff);
    }

    [Test]
    public void ParseVoiceConfig_MissingElement_ReturnsNull()
    {
        // Arrange
        voice vemlVoice = null;

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNull(config);
    }

    [Test]
    public void ParseVoiceConfig_WithDefaultsOnly_AppliesDefaults()
    {
        // Arrange - voice with only endpoint, no distance or rolloff specified
        var vemlVoice = new voice
        {
            endpoint = "wss://voice.example.com/v1"
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert - should have defaults
        Assert.IsNotNull(config);
        Assert.AreEqual("websocket", config.Provider);
        Assert.AreEqual(1f, config.MinDistance);
        Assert.AreEqual(25f, config.MaxDistance);
        Assert.AreEqual("logarithmic", config.Rolloff);
    }

    [Test]
    public void ParseVoiceConfig_NullProvider_DefaultsToWebsocket()
    {
        // Arrange
        var vemlVoice = new voice
        {
            provider = null,
            endpoint = "wss://voice.example.com/v1"
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.AreEqual("websocket", config.Provider);
    }

    #endregion

    #region VoiceBinding Tests

    [Test]
    public void VoiceBinding_Matches_WildcardPattern_ReturnsTrue()
    {
        // Arrange
        var binding = new VoiceBinding
        {
            UserPattern = "*",
            ToEntityTag = "avatar"
        };

        // Act & Assert
        Assert.IsTrue(binding.Matches("user123"));
        Assert.IsTrue(binding.Matches("anyone"));
        Assert.IsTrue(binding.Matches(""));
    }

    [Test]
    public void VoiceBinding_Matches_NullPattern_ReturnsTrue()
    {
        // Arrange
        var binding = new VoiceBinding
        {
            UserPattern = null,
            ToEntityTag = "avatar"
        };

        // Act & Assert
        Assert.IsTrue(binding.Matches("user123"));
    }

    [Test]
    public void VoiceBinding_Matches_SpecificPattern_OnlyMatchesExact()
    {
        // Arrange
        var binding = new VoiceBinding
        {
            UserPattern = "user123",
            ToEntityTag = "avatar"
        };

        // Act & Assert
        Assert.IsTrue(binding.Matches("user123"));
        Assert.IsFalse(binding.Matches("user456"));
        Assert.IsFalse(binding.Matches("user12"));
    }

    [Test]
    public void VoiceBinding_DefaultOffset_IsZero()
    {
        // Arrange
        var binding = new VoiceBinding();

        // Assert
        Assert.AreEqual(UnityEngine.Vector3.zero, binding.Offset);
    }

    [Test]
    public void ParseVoiceConfig_WithBindings_ParsesBindings()
    {
        // Arrange
        var vemlVoice = new voice
        {
            endpoint = "wss://voice.example.com/v1",
            bindings = new voicebinding[]
            {
                new voicebinding
                {
                    userpattern = "*",
                    toentitytag = "avatar",
                    offset = "0,1.7,0"
                }
            }
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNotNull(config);
        Assert.AreEqual(1, config.Bindings.Count);
        Assert.AreEqual("*", config.Bindings[0].UserPattern);
        Assert.AreEqual("avatar", config.Bindings[0].ToEntityTag);
        Assert.AreEqual(0f, config.Bindings[0].Offset.x, 0.001f);
        Assert.AreEqual(1.7f, config.Bindings[0].Offset.y, 0.001f);
        Assert.AreEqual(0f, config.Bindings[0].Offset.z, 0.001f);
    }

    [Test]
    public void ParseVoiceConfig_WithMultipleBindings_ParsesAll()
    {
        // Arrange
        var vemlVoice = new voice
        {
            endpoint = "wss://voice.example.com/v1",
            bindings = new voicebinding[]
            {
                new voicebinding
                {
                    userpattern = "host",
                    toentitytag = "host-avatar",
                    offset = "0,1.8,0"
                },
                new voicebinding
                {
                    userpattern = "*",
                    toentitytag = "guest-avatar",
                    offset = "0,1.6,0"
                }
            }
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNotNull(config);
        Assert.AreEqual(2, config.Bindings.Count);
        Assert.AreEqual("host", config.Bindings[0].UserPattern);
        Assert.AreEqual("*", config.Bindings[1].UserPattern);
    }

    [Test]
    public void ParseVoiceConfig_NoBindings_ReturnsEmptyList()
    {
        // Arrange
        var vemlVoice = new voice
        {
            endpoint = "wss://voice.example.com/v1",
            bindings = null
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNotNull(config);
        Assert.IsNotNull(config.Bindings);
        Assert.AreEqual(0, config.Bindings.Count);
    }

    [Test]
    public void ParseVoiceConfig_InvalidOffset_DefaultsToZero()
    {
        // Arrange
        var vemlVoice = new voice
        {
            endpoint = "wss://voice.example.com/v1",
            bindings = new voicebinding[]
            {
                new voicebinding
                {
                    userpattern = "*",
                    toentitytag = "avatar",
                    offset = "invalid"
                }
            }
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNotNull(config);
        Assert.AreEqual(1, config.Bindings.Count);
        Assert.AreEqual(UnityEngine.Vector3.zero, config.Bindings[0].Offset);
    }

    [Test]
    public void ParseVoiceConfig_EmptyOffset_DefaultsToZero()
    {
        // Arrange
        var vemlVoice = new voice
        {
            endpoint = "wss://voice.example.com/v1",
            bindings = new voicebinding[]
            {
                new voicebinding
                {
                    userpattern = "*",
                    toentitytag = "avatar",
                    offset = ""
                }
            }
        };

        // Act
        var config = VoiceConfig.FromVEML(vemlVoice);

        // Assert
        Assert.IsNotNull(config);
        Assert.AreEqual(UnityEngine.Vector3.zero, config.Bindings[0].Offset);
    }

    #endregion
}
