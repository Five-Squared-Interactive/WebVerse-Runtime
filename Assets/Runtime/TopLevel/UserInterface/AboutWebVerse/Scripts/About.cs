// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using TMPro;
using UnityEngine;

namespace FiveSQD.WebVerse.Interface.About
{
    /// <summary>
    /// Class for about page.
    /// </summary>
    public class About : MonoBehaviour
    {
        /// <summary>
        /// The version text.
        /// </summary>
        public TMP_Text versionText;

        /// <summary>
        /// URL for documentation.
        /// </summary>
        private static readonly string documentationURL
            = "https://github.com/Five-Squared-Interactive/WebVerse/wiki";

        /// <summary>
        /// URL for reporting an issue.
        /// </summary>
        private static readonly string reportAnIssueURL
            = "https://github.com/Five-Squared-Interactive/WebVerse/issues";

        /// <summary>
        /// URL for terms and conditions.
        /// </summary>
        private static readonly string termsAndConditionsURL
            = "https://github.com/Five-Squared-Interactive/WebVerse/blob/main/TermsAndConditions.md";

        /// <summary>
        /// URL for license.
        /// </summary>
        private static readonly string licenseURL
            = "https://github.com/Five-Squared-Interactive/WebVerse/blob/main/LICENSE";

        /// <summary>
        /// URL for open source project.
        /// </summary>
        private static readonly string openSourceProjectURL
            = "https://github.com/Five-Squared-Interactive/WebVerse";

        /// <summary>
        /// Initialize the about page.
        /// </summary>
        public void Initialize()
        {
            versionText.text = "WebVerse Version: " + WebVerseRuntime.versionString + ": \"" + WebVerseRuntime.codenameString + "\"";
        }

        /// <summary>
        /// Terminate the about page.
        /// </summary>
        public void Terminate()
        {

        }

        /// <summary>
        /// Return from the about page.
        /// </summary>
        public void Return()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Open documentation.
        /// </summary>
        public void OpenDocumentation()
        {
            Application.OpenURL(documentationURL);
        }

        /// <summary>
        /// Open report an issue.
        /// </summary>
        public void OpenReportAnIssue()
        {
            Application.OpenURL(reportAnIssueURL);
        }

        /// <summary>
        /// Open terms and conditions.
        /// </summary>
        public void OpenTermsAndConditions()
        {
            Application.OpenURL(termsAndConditionsURL);
        }

        /// <summary>
        /// Open license.
        /// </summary>
        public void OpenLicense()
        {
            Application.OpenURL(licenseURL);
        }

        /// <summary>
        /// Open open source project.
        /// </summary>
        public void OpenOpenSourceProject()
        {
            Application.OpenURL(openSourceProjectURL);
        }
    }
}