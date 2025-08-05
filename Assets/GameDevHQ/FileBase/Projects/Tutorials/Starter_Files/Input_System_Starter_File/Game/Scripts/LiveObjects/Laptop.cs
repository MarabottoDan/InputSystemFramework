using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.InputSystem;


namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        private PlayerInputAction _inputactions;
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        private bool _hacked = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        private void OnEnable()
        {
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;

            _inputactions = new PlayerInputAction();
            _inputactions.Player.Enable();

            


            _inputactions.Player.NextCamera.performed += NextCamera_performed;
            _inputactions.Player.ExitHack.performed += ExitHack_performed;
        }

        private void NextCamera_performed(InputAction.CallbackContext obj)
        {
            SwitchToNextCamera();
        }

        private void ExitHack_performed(InputAction.CallbackContext obj)
        {
            
            ExitHacking();
        }

        private void Update()
        {
           if (_hacked == true)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    var previous = _activeCamera;
                    _activeCamera++;


                    if (_activeCamera >= _cameras.Length)
                        _activeCamera = 0;


                    _cameras[_activeCamera].Priority = 11;
                    _cameras[previous].Priority = 9;
                }

                if (Input.GetKeyDown(KeyCode.Q)) //DM changed from Escape key to G and button South Gamepad because when I pressed Esc, game would stop playing.
                {
                    _hacked = false;
                    onHackEnded?.Invoke();
                    ResetCameras();
                }
            } 
        }


        private void SwitchToNextCamera()
        {
            Debug.Log("SwitchToNextCamera() called");
            if (!_hacked)
            {
                Debug.Log("Aborted: not hacked.");
                return;
            }

            if (!_hacked) return;

            var previous = _activeCamera;
            _activeCamera++;

            if (_activeCamera >= _cameras.Length)
                _activeCamera = 0;

            _cameras[_activeCamera].Priority = 11;
            _cameras[previous].Priority = 9;
        }

        private void ExitHacking()
        {
           
           // if (!_hacked) return;

             _hacked = false;
             onHackEnded?.Invoke();
             ResetCameras();
        }

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
        }

        private void InteractableZone_onHoldStarted(int zoneID)
        {
            if (zoneID == 3 && _hacked == false) //Hacking terminal
            {
                _progressBar.gameObject.SetActive(true);
                StartCoroutine(HackingRoutine());
                onHackComplete?.Invoke();
            }
        }

        private void InteractableZone_onHoldEnded(int zoneID)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_hacked == true)
                    return;

                StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();
            }
        }

        
        IEnumerator HackingRoutine()
        {
            while (_progressBar.value < 1)
            {
                _progressBar.value += Time.deltaTime / _hackTime;
                yield return new WaitForEndOfFrame();
            }

            //successfully hacked
            _hacked = true;
            _interactableZone.CompleteTask(3);

            //hide progress bar
            _progressBar.gameObject.SetActive(false);

            //enable Vcam1
            _cameras[0].Priority = 11;
        }
        
        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;

            if (_inputactions != null)
            {
                _inputactions.Player.NextCamera.performed -= NextCamera_performed;
                _inputactions.Player.ExitHack.performed -= ExitHack_performed;
                _inputactions.Player.Disable();
            }
        }
    }

}

