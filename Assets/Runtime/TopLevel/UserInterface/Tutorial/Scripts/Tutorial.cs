// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FiveSQD.WebVerse.Interface.Tutorial
{
    /// <summary>
    /// Class for the tutorial.
    /// </summary>
    public class Tutorial : MonoBehaviour
    {
        /// <summary>
        /// Number of tutorial pages.
        /// </summary>
        public uint numPages;

        /// <summary>
        /// Back button.
        /// </summary>
        public Button backButton;

        /// <summary>
        /// Forward button.
        /// </summary>
        public Button forwardButton;

        /// <summary>
        /// Done button.
        /// </summary>
        public Button doneButton;

        /// <summary>
        /// Tutorial arrows.
        /// </summary>
        public GameObject[] arrows;

        /// <summary>
        /// Tutorial prompts.
        /// </summary>
        public GameObject[] prompts;

        /// <summary>
        /// Main prompt.
        /// </summary>
        public GameObject mainPrompt;

        /// <summary>
        /// Page text.
        /// </summary>
        public TMP_Text pageText;

        /// <summary>
        /// Current page.
        /// </summary>
        private uint currentPage;

        /// <summary>
        /// Action to perform on terminate.
        /// </summary>
        private Action onTerminateAction;

        /// <summary>
        /// Initialize tutorial.
        /// </summary>
        /// <param name="onTerminate">Action to perform on terminate.</param>
        public void Initialize(Action onTerminate)
        {
            SetPage(1);

            if (arrows == null || arrows.Length < 1 || prompts == null || prompts.Length < 1)
            {
                Logging.LogError("[Tutorial->Initialize] Inavlid tutorial settings.");
            }

            onTerminateAction = onTerminate;
        }

        /// <summary>
        /// Terminate tutorial.
        /// </summary>
        public void Terminate()
        {
            if (onTerminateAction != null)
            {
                onTerminateAction.Invoke();
            }

            DisableAllArrows();
            DisableAllPages();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Go back.
        /// </summary>
        public void GoBack()
        {
            if (currentPage > 1)
            {
                SetPage(currentPage - 1);
            }
        }

        /// <summary>
        /// Go forward.
        /// </summary>
        public void GoForward()
        {
            if (currentPage < numPages)
            {
                SetPage(currentPage + 1);
            }
        }

        /// <summary>
        /// Set page number.
        /// </summary>
        /// <param name="pageNum"></param>
        private void SetPage(uint pageNum)
        {
            if (pageNum == 0 || pageNum > numPages)
            {
                Logging.LogError("[Tutorial->SetPage] Invalid page number.");
                return;
            }

            currentPage = pageNum;

            DisableAllArrows();
            DisableAllPages();

            if (pageNum == 1)
            {
                SetNavButtonsFirstPage();
                mainPrompt.SetActive(true);
            }
            else
            {
                if (pageNum == numPages)
                {
                    SetNavButtonsLastPage();
                }
                else
                {
                    SetNavButtonsMiddlePage();
                }
                arrows[pageNum - 2].SetActive(true);
                prompts[pageNum - 2].SetActive(true);
            }

            pageText.text = pageNum + "/" + numPages;
        }

        /// <summary>
        /// Set navigation buttons for first page.
        /// </summary>
        private void SetNavButtonsFirstPage()
        {
            backButton.enabled = false;
            forwardButton.gameObject.SetActive(true);
            forwardButton.enabled = true;
            doneButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Set navigation buttons for middle page.
        /// </summary>
        private void SetNavButtonsMiddlePage()
        {
            backButton.enabled = true;
            forwardButton.gameObject.SetActive(true);
            forwardButton.enabled = true;
            doneButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Set navigation buttons for last page.
        /// </summary>
        private void SetNavButtonsLastPage()
        {
            backButton.enabled = true;
            forwardButton.gameObject.SetActive(false);
            doneButton.gameObject.SetActive(true);
            doneButton.enabled = true;
        }

        /// <summary>
        /// Disable all tutorial arrows.
        /// </summary>
        private void DisableAllArrows()
        {
            foreach (GameObject arrow in arrows)
            {
                arrow.SetActive(false);
            }
        }

        /// <summary>
        /// Disable all tutorial pages.
        /// </summary>
        private void DisableAllPages()
        {
            mainPrompt.SetActive(false);
            foreach (GameObject prompt in prompts)
            {
                prompt.SetActive(false);
            }
        }
    }
}