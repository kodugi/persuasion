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
            if (GameSettingsController.Instance == null)
            {
                Debug.LogError("GameSettingsController.Instance == null");
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
            
            _maxTurnsInputField.text = GameSettingsController.Instance.GetMaxTurns().ToString();
            _maxTurnsInputField.onEndEdit.AddListener(HandleMaxTurnsChanged);
            
            _targetNumberInputField.text = GameSettingsController.Instance.GetTargetNumber().ToString();
            _targetNumberInputField.onEndEdit.AddListener(HandleTargetNumberChanged);
            return true;
        }

        private void HandleMaxTurnsChanged(string maxTurnsString)
        {
            try
            {
                int maxTurns = int.Parse(maxTurnsString);
                GameSettingsController.Instance.SetMaxTurns(maxTurns);
            }
            catch(FormatException e)
            {
                Debug.LogWarning("input value ignored due to format error: " + e.Message);
                _maxTurnsInputField.text = GameSettingsController.Instance.GetMaxTurns().ToString();
            }
        }

        private void HandleTargetNumberChanged(string targetNumberString)
        {
            try
            {
                int targetNumber = int.Parse(targetNumberString);
                GameSettingsController.Instance.SetTargetNumber(targetNumber);
            }
            catch(FormatException e)
            {
                Debug.LogWarning("input value ignored due to format error: " + e.Message);
                _targetNumberInputField.text = GameSettingsController.Instance.GetTargetNumber().ToString();
            }
        }

        public void Refresh()
        {
            _maxTurnsInputField.text = GameSettingsController.Instance.GetMaxTurns().ToString();
            _targetNumberInputField.text = GameSettingsController.Instance.GetTargetNumber().ToString();
        }
    }
}