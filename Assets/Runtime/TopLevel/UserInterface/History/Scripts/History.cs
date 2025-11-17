// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FiveSQD.WebVerse.Interface.History
{
    /// <summary>
    /// Class for history menu.
    /// </summary>
    public class History : MonoBehaviour
    {
        /// <summary>
        /// The native history script.
        /// </summary>
        public NativeHistory nativeHistory;

        /// <summary>
        /// Prefab for a history button.
        /// </summary>
        public GameObject historyButtonPrefab;

        /// <summary>
        /// Container for history buttons.
        /// </summary>
        public GameObject historyButtonContainer;

        /// <summary>
        /// The history buttons.
        /// </summary>
        private List<GameObject> historyButtons;

        /// <summary>
        /// Initialize history menu.
        /// </summary>
        public void Initialize()
        {
            if (historyButtons != null)
            {
                foreach (GameObject button in historyButtons)
                {
                    DestroyImmediate(button);
                }
            }

            historyButtons = new List<GameObject>();

            SetUpHistoryButtons();
        }

        /// <summary>
        /// Terminate history menu.
        /// </summary>
        public void Terminate()
        {

        }

        /// <summary>
        /// Return from history menu.
        /// </summary>
        public void Return()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Set up history buttons.
        /// </summary>
        private void SetUpHistoryButtons()
        {
            Tuple<DateTime, string, string>[] history = nativeHistory.GetAllItemsFromHistory();
            foreach (Tuple<DateTime, string, string> historyItem in history)
            {
                GameObject newHistoryButton = Instantiate(historyButtonPrefab);
                newHistoryButton.transform.SetParent(historyButtonContainer.transform);
                newHistoryButton.transform.localPosition = new Vector3(newHistoryButton.transform.localPosition.x,
                    newHistoryButton.transform.localPosition.y, 0);
                newHistoryButton.transform.localRotation = Quaternion.identity;
                newHistoryButton.transform.localScale = Vector3.one;
                GameObject timestampGO = newHistoryButton.transform.Find("Button").Find("Timestamp").gameObject;
                GameObject siteNameGO = newHistoryButton.transform.Find("Button").Find("SiteName").gameObject;
                GameObject siteURLGO = newHistoryButton.transform.Find("Button").Find("SiteURL").gameObject;
                TMP_Text timestampText = timestampGO.GetComponent<TMP_Text>();
                TMP_Text siteNameText = siteNameGO.GetComponent <TMP_Text>();
                TMP_Text siteURLText = siteURLGO.GetComponent<TMP_Text>();
                timestampText.text = historyItem.Item1.ToLocalTime().ToString();
                siteNameText.text = historyItem.Item2;
                siteURLText.text = historyItem.Item3;
                Button btn = newHistoryButton.GetComponentInChildren<Button>();
                btn.onClick.AddListener(() =>
                {
                    WebVerseRuntime.Instance.LoadURL(historyItem.Item3);
                });
                historyButtons.Add(newHistoryButton);
            }
        }
    }
}