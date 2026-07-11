using System;
using SingletonUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditor
{
    public class GameSettingsView: SelfInitializingMonoBehaviourSingleton<GameSettingsView>
    {
        [SerializeField] private TMP_InputField _maxTurnsInputField;
        [SerializeField] private TMP_InputField _targetNumberInputField;
        protected override bool InitializeCore()
        {
            if (GameInfoController.Instance == null)
            {
                Debug.LogError("GameInfoController.Instance == null");
                return false;
            }

            if (_maxTurnsInputField == null)
            {
                Debug.Log("_maxTurnsInputField == null");
                return false;
            }

            if (_targetNumberInputField == null)
            {
                Debug.Log("_targetNumberInputField == null");
                return false;
            }
            
            _maxTurnsInputField.text = "";
            _maxTurnsInputField.onEndEdit.AddListener(HandleMaxTurnsChanged);
            
            _targetNumberInputField.text = "";
            _targetNumberInputField.onEndEdit.AddListener(HandleTargetNumberChanged);
            return true;
        }

        private void HandleMaxTurnsChanged(string maxTurnsString)
        {
            try
            {
                int maxTurns = int.Parse(maxTurnsString);
                GameInfoController.Instance.SetMaxTurns(maxTurns);
            }
            catch(FormatException e)
            {
                Debug.LogWarning("input value ignored due to format error");
                _maxTurnsInputField.text = "";
            }
        }

        private void HandleTargetNumberChanged(string targetNumberString)
        {
            try
            {
                int targetNumber = int.Parse(targetNumberString);
                GameInfoController.Instance.SetTargetNumber(targetNumber);
            }
            catch(FormatException e)
            {
                Debug.LogWarning("input value ignored due to format error");
                _targetNumberInputField.text = "";
            }
        }
    }
}