// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for an RGBA color.
    /// </summary>
    public class Color
    {
        /// <summary>
        /// Red component, from 0-255.
        /// </summary>
        public float r;

        /// <summary>
        /// Green component, from 0-255.
        /// </summary>
        public float g;

        /// <summary>
        /// Blue component, from 0-255.
        /// </summary>
        public float b;

        /// <summary>
        /// Alpha component, from 0-255
        /// </summary>
        public float a;

        /// <summary>
        /// Returns the color black (RGBA: 0, 0, 0, 1).
        /// </summary>
        public static Color black
        {
            get
            {
                return new Color(0, 0, 0, 1);
            }
        }

        /// <summary>
        /// Returns the color blue (RGBA: 0, 0, 1, 1).
        /// </summary>
        public static Color blue
        {
            get
            {
                return new Color(0, 0, 1, 1);
            }
        }

        /// <summary>
        /// Returns the color clear (RGBA: 0, 0, 0, 0).
        /// </summary>
        public static Color clear
        {
            get
            {
                return new Color(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Returns the color cyan (RGBA: 0, 1, 1, 1).
        /// </summary>
        public static Color cyan
        {
            get
            {
                return new Color(0, 1, 1, 1);
            }
        }

        /// <summary>
        /// Returns the color gray (RGBA: 0.5, 0.5, 0.5, 1).
        /// </summary>
        public static Color gray
        {
            get
            {
                return new Color((float) 0.5, (float) 0.5, (float) 0.5, 1);
            }
        }

        /// <summary>
        /// Returns the color green (RGBA: 0, 1, 0, 1).
        /// </summary>
        public static Color green
        {
            get
            {
                return new Color(0, 1, 0, 1);
            }
        }

        /// <summary>
        /// Returns the color grey (RGBA: 0.5, 0.5, 0.5, 1).
        /// </summary>
        public static Color grey
        {
            get
            {
                return gray;
            }
        }

        /// <summary>
        /// Returns the color magenta (RGBA: 1, 0, 1, 1).
        /// </summary>
        public static Color magenta
        {
            get
            {
                return new Color(1, 0, 1, 1);
            }
        }

        /// <summary>
        /// Returns the color red (RGBA: 1, 0, 0, 1).
        /// </summary>
        public static Color red
        {
            get
            {
                return new Color(1, 0, 0, 1);
            }
        }

        /// <summary>
        /// Returns the color white (RGBA: 1, 1, 1, 1).
        /// </summary>
        public static Color white
        {
            get
            {
                return new Color(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// Returns the color yellow (RGBA: 1, 0.92, 0.016, 1).
        /// </summary>
        public static Color yellow
        {
            get
            {
                return new Color(1, (float) 0.92, (float) 0.016, 1);
            }
        }

        /// <summary>
        /// Constructor for a Color.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <param name="a">Alpha component.</param>
        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}