using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Data
{
    public class DataAPIHelper : MonoBehaviour
    {
        /// <summary>
        /// Instance of the Entity API Helper.
        /// </summary>
        private static DataAPIHelper instance;

        /// <summary>
        /// Queue of javascript code to be executed.
        /// </summary>
        private Queue<Tuple<string, object[]>> javascriptQueue;

        public void Initialize()
        {
            instance = this;
            javascriptQueue = new Queue<Tuple<string, object[]>>();
        }

        public static void QueueJavascript(string functionName, object[] parameters)
        {
            instance.javascriptQueue.Enqueue(new Tuple<string, object[]>(functionName, parameters));
        }

        void Update()
        {
            if (javascriptQueue.Count > 0)
            {
                Tuple<string, object[]> javascriptToExecute = javascriptQueue.Dequeue();
                Runtime.WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                    javascriptToExecute.Item1, javascriptToExecute.Item2);
            }
        }
    }
}