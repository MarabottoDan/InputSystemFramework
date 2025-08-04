using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem; //DM

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private float _thrustInput; //DM applied with any Shift Key
        private Vector2 _tiltInput;//DM
        private float _ascendDescendInput;//DM
        private float _rotateInput;//DM
        private PlayerInputAction _input;//DM
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void OnEnable()
        {
            if (_input == null)//DM
            {
                _input = new PlayerInputAction();
            }
            _input.Drone.Enable();
            _input.Drone.ExitMode.performed += ExitMode_performed;
            _input.Drone.Rotate.performed += Rotate_performed;
            _input.Drone.Rotate.canceled += Rotate_canceled;
            _input.Drone.AscendDescend.performed += AscendDescend_performed;
            _input.Drone.AscendDescend.canceled += AscendDescend_canceled;
            _input.Drone.Tilt.performed += Tilt_performed;
            _input.Drone.Tilt.canceled += Tilt_canceled;
            _input.Drone.Thrust.performed += Thrust_performed;
            _input.Drone.Thrust.canceled += Thrust_canceled;

            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void Thrust_performed(InputAction.CallbackContext obj)
        {
            _thrustInput = obj.ReadValue<float>();
        }

        private void Thrust_canceled(InputAction.CallbackContext obj)
        {
            _thrustInput = 0f;
        }

        private void Tilt_performed(InputAction.CallbackContext obj)
        {
            _tiltInput = obj.ReadValue<Vector2>();
        }

        private void Tilt_canceled(InputAction.CallbackContext obj)
        {
            _tiltInput = Vector2.zero;
        }

        private void AscendDescend_performed(InputAction.CallbackContext obj)
        {
            _ascendDescendInput = obj.ReadValue<float>();
        }

        private void AscendDescend_canceled(InputAction.CallbackContext obj)
        {
            _ascendDescendInput = 0f;
        }
        private void Rotate_performed(InputAction.CallbackContext obj)
        {
            _rotateInput = obj.ReadValue<float>();
        }

        private void Rotate_canceled(InputAction.CallbackContext obj)
        {
            _rotateInput = 0f;
        }

        private void ExitMode_performed(InputAction.CallbackContext obj)//DM
        {
            if (_inFlightMode)
            {
                _inFlightMode = false;
                onExitFlightmode?.Invoke();
                ExitFlightMode();
            }
        }

        

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();

               // if (Input.GetKeyDown(KeyCode.Q))//DM changed from Escape key to Q, because when I pressed Esc, game would stop playing.
               //{
               //   _inFlightMode = false;
               //  onExitFlightmode?.Invoke();
               // ExitFlightMode();
               //}
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {

            if(_rotateInput != 0)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y -= _rotateInput * (_speed / 3);
                transform.localRotation = Quaternion.Euler(tempRot);
            }

            if (_thrustInput > 0)//DM
            {
                _rigidbody.AddForce(transform.forward * _thrustInput * _speed, ForceMode.Acceleration);
            }

            /* if (Input.GetKey(KeyCode.LeftArrow))
             {
                 var tempRot = transform.localRotation.eulerAngles;
                 tempRot.y -= _speed / 3;
                 transform.localRotation = Quaternion.Euler(tempRot);
             }
             if (Input.GetKey(KeyCode.RightArrow))
             {
                 var tempRot = transform.localRotation.eulerAngles;
                 tempRot.y += _speed / 3;
                 transform.localRotation = Quaternion.Euler(tempRot);
             }*/
        }

        private void CalculateMovementFixedUpdate()
        {

            if (_ascendDescendInput != 0)
            {
                _rigidbody.AddForce(transform.up * _ascendDescendInput * _speed, ForceMode.Acceleration);
            }


            /* if (Input.GetKey(KeyCode.Space))
             {
                 _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
             }
             if (Input.GetKey(KeyCode.V))
             {
                 _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
             }*/
        }

        private void CalculateTilt()
        {

            if (_tiltInput.x < 0) // Left
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 30);
            else if (_tiltInput.x > 0) // Right
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (_tiltInput.y > 0) // Forward
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (_tiltInput.y < 0) // Back
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);

            /* if (Input.GetKey(KeyCode.A)) 
                 transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
             else if (Input.GetKey(KeyCode.D))
                 transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
             else if (Input.GetKey(KeyCode.W))
                 transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
             else if (Input.GetKey(KeyCode.S))
                 transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
             else 
                 transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);*/
        }

        private void OnDisable()
        {
            _input.Drone.ExitMode.performed -= ExitMode_performed;//DM
            _input.Drone.Disable();//DM
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
            _input.Drone.AscendDescend.performed -= AscendDescend_performed;//DM
            _input.Drone.AscendDescend.canceled -= AscendDescend_canceled;//DM
            _input.Drone.Tilt.performed -= Tilt_performed;
            _input.Drone.Tilt.canceled -= Tilt_canceled;
            _input.Drone.Thrust.performed -= Thrust_performed;
            _input.Drone.Thrust.canceled -= Thrust_canceled;


        }
    }
}
