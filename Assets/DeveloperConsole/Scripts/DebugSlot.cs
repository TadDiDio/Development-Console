using TMPro;
using System;
using UnityEngine;

namespace DeveloperConsole
{
    public class DebugSlot : MonoBehaviour
    {
        private Func<string> getValue = () => "";
        private TMP_Text display;
        private void Awake()
        {
            display = GetComponent<TMP_Text>();
        }
        private void Update()
        {
            display.SetText(getValue());
        }

        public void SetEvaluation(Func<string> eval)
        {
            getValue = eval;
        }
    }
}