// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0
{
    /// <summary>
    /// Volumetric-fill authoring attributes for the VEML &lt;water&gt; element, added as a
    /// partial so the generated schema class is not edited. All attributes are optional;
    /// a &lt;water&gt; element without them behaves exactly as before (back-compat).
    ///
    /// Example:
    ///   &lt;water tag="lagoon" preset="tropical-shallows" fit-to="basin" water-level="3" buoyant="true"&gt;
    ///     &lt;transform&gt;...&lt;/transform&gt;
    ///   &lt;/water&gt;
    /// </summary>
    public partial class water
    {
        /// <summary>Named appearance preset (calm-lake, tropical-shallows, murky-pond, stormy-ocean).</summary>
        [System.Xml.Serialization.XmlAttributeAttribute("preset")]
        public string preset { get; set; }

        /// <summary>Tag of the mesh/entity whose world bounds the water fills.</summary>
        [System.Xml.Serialization.XmlAttributeAttribute("fit-to")]
        public string fitto { get; set; }

        /// <summary>World-space Y up to which the volume fills.</summary>
        [System.Xml.Serialization.XmlAttributeAttribute("water-level")]
        public float waterlevel { get; set; }

        /// <summary>Whether water-level was present in the document.</summary>
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public bool waterlevelSpecified { get; set; }

        /// <summary>Whether bodies in this water should float.</summary>
        [System.Xml.Serialization.XmlAttributeAttribute("buoyant")]
        public bool buoyant { get; set; }

        /// <summary>Whether buoyant was present in the document.</summary>
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public bool buoyantSpecified { get; set; }
    }
}
