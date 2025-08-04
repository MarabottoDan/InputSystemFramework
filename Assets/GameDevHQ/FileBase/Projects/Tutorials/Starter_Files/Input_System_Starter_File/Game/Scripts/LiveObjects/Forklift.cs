using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        private PlayerInputAction _input;//DM
        private Vector2 _moveInput;//DM
        private bool _isLiftingUp;//DM
        private bool _isLiftingDown;//DM

        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        private void OnEnable()
        {
            if (_input == null)
            {
                _input = new PlayerInputAction();
            }

            _input.Forklift.Enable();
            _input.Forklift.Move.performed += Move_performed;
            _input.Forklift.Move.canceled += Move_canceled;

            _input.Forklift.LiftUp.performed += LiftUp_performed;
            _input.Forklift.LiftUp.canceled += LiftUp_canceled;
            _input.Forklift.LiftDown.performed += LiftDown_performed;
            _input.Forklift.LiftDown.canceled += LiftDown_canceled;

            _input.Forklift.ExitDriveMode.performed += ExitDriveMode_performed;

            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void ExitDriveMode_performed(InputAction.CallbackContext obj)
        {
            if (_inDriveMode)
            {
                ExitDriveMode();
            }
        }

        private void LiftDown_performed(InputAction.CallbackContext obj)
        {
            _isLiftingDown = true;
        }

        private void LiftDown_canceled(InputAction.CallbackContext obj)
        {
            _isLiftingDown = false;
        }

        private void LiftUp_performed(InputAction.CallbackContext obj)
        {
            _isLiftingUp = true;
        }

        private void LiftUp_canceled(InputAction.CallbackContext obj)
        {
            _isLiftingUp = false;

        }

        private void Move_performed(InputAction.CallbackContext obj)
        {
            _moveInput = obj.ReadValue<Vector2>();
        }

        private void Move_canceled(InputAction.CallbackContext obj)
        {
            _moveInput = Vector2.zero;
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
               /* if (Input.GetKeyDown(KeyCode.Q))//DM changed from Escape key to Q, because when I pressed Esc, game would stop playing.
                    ExitDriveMode();*/
            }

        }

        private void CalcutateMovement()
        {
            //float h = Input.GetAxisRaw("Horizontal");
            //float v = Input.GetAxisRaw("Vertical");
            float h = _moveInput.x;//DM
            float v = _moveInput.y;//DM
            var direction = new Vector3(0, 0, v);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(v) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += h * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            if (_isLiftingUp)//DM
                LiftUpRoutine();
            else if (_isLiftingDown)
                LiftDownRoutine();

            /* if (Input.GetKey(KeyCode.R))
                 LiftUpRoutine();
             else if (Input.GetKey(KeyCode.T))
                 LiftDownRoutine();*/
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;

            if (_input != null)//DM
            {
                _input.Forklift.Move.performed -= Move_performed;
                _input.Forklift.Move.canceled -= Move_canceled;
                _input.Forklift.LiftUp.performed -= LiftUp_performed;
                _input.Forklift.LiftUp.canceled -= LiftUp_canceled;
                _input.Forklift.LiftDown.performed -= LiftDown_performed;
                _input.Forklift.LiftDown.canceled -= LiftDown_canceled;
                _input.Forklift.ExitDriveMode.performed -= ExitDriveMode_performed;
                _input.Forklift.Disable();
            }
        }

    }
}