using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Border.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
    public class InputReader : ScriptableObject
    {
        private GameInput _gameInput;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();

                _gameInput.Gameplay.SetCallbacks(this);
                _gameInput.Menus.SetCallbacks(this);
                _gameInput.Gameplay.Enable();
            }
        }

        private void OnDisable()
        {
            DisableAllInput();
        }

        public void DisableAllInput()
        {
            // _gameInput.##.Disable();
        }
    }
}
