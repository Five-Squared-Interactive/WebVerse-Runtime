// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a UUID.
    /// </summary>
    public class UUID
    {
        /// <summary>
        /// Internal Guid class.
        /// </summary>
        private Guid internalValue;

        /// <summary>
        /// Constructor for a UUID.
        /// </summary>
        /// <param name="input">Input string to use.</param>
        public UUID(string input = null)
        {
            if (input == null)
            {
                internalValue = Guid.Empty;
            }
            else
            {
                internalValue = Guid.Parse(input);
            }
        }

        /// <summary>
        /// Get a new UUID.
        /// </summary>
        /// <returns>A new UUID.</returns>
        public static UUID NewUUID()
        {
            return new UUID(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Parse a UUID from a string.
        /// </summary>
        /// <param name="input">Input string to use.</param>
        /// <returns>A UUID containing the provided value, or null.</returns>
        public static UUID Parse(string input)
        {
            return new UUID(Guid.Parse(input).ToString());
        }

        /// <summary>
        /// Convert UUID to string.
        /// </summary>
        /// <returns>String representation of the UUID, or null.</returns>
        public override string ToString()
        {
            if (internalValue == null)
            {
                return null;
            }
            else
            {
                return internalValue.ToString();
            }
        }
    }
}