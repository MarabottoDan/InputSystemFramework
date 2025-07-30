using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.LiveObjects;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.Player
{

    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        private PlayerInputAction _input; //DM
        private Vector2 _inputDirection; //DM

        private CharacterController _controller;
        private Animator _anim;
        [SerializeField]
        private float _speed = 5.0f;
        private bool _playerGrounded;
        [SerializeField]
        private Detonator _detonator;
        private bool _canMove = true;
        [SerializeField]
        private CinemachineVirtualCamera _followCam;
        [SerializeField]
        private GameObject _model;


        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete += ReleasePlayerControl;
            Laptop.onHackEnded += ReturnPlayerControl;
            Forklift.onDriveModeEntered += ReleasePlayerControl;
            Forklift.onDriveModeExited += ReturnPlayerControl;
            Forklift.onDriveModeEntered += HidePlayer;
            Drone.OnEnterFlightMode += ReleasePlayerControl;
            Drone.onExitFlightmode += ReturnPlayerControl;
        } 

        private void Start()
        {
            _input = new PlayerInputAction(); //DM
            _input.Player.Enable(); //DM
            _input.Player.Movement.performed += Movement_performed; //DM
            _input.Player.Movement.canceled += Movement_canceled; //DM

            _controller = GetComponent<CharacterController>();

            if (_controller == null)
                Debug.LogError("No Character Controller Present");

            _anim = GetComponentInChildren<Animator>();

            if (_anim == null)
                Debug.Log("Failed to connect the Animator");
        }

        private void Movement_canceled(InputAction.CallbackContext obj)//DM
        {
            _inputDirection = Vector2.zero;
            Debug.Log("Move canceled");
        }

        private void Movement_performed(InputAction.CallbackContext context)//DM
        {
            Debug.Log("Walking");
            _inputDirection = context.ReadValue<Vector2>();

            var move = context.ReadValue<Vector2>();

            transform.Translate(move * Time.deltaTime * 5);
        }

        private void Update()
        {
            if (_canMove == true);
            // CalcutateMovement();
            HandleMovement();//DM
           

        }

        private void HandleMovement()//DM
        {
            _playerGrounded = _controller.isGrounded;

            
            float h = _inputDirection.x; 
            float v = _inputDirection.y; 

           
            transform.Rotate(transform.up, h);

            
            var direction = transform.forward * v;
            var velocity = direction * _speed;

            
            _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));

            
            if (_playerGrounded)
                velocity.y = 0f;
            else
                velocity.y += -20f * Time.deltaTime;

            _controller.Move(velocity * Time.deltaTime);
        }

        /*private void CalcutateMovement()
        {
            _playerGrounded = _controller.isGrounded;
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            transform.Rotate(transform.up, h);

            var direction = transform.forward * v;
            var velocity = direction * _speed;


            _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));


            if (_playerGrounded)
                velocity.y = 0f;
            if (!_playerGrounded)
            {
                velocity.y += -20f * Time.deltaTime;
            }
            
            _controller.Move(velocity * Time.deltaTime);                      

        }*/

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            switch(zone.GetZoneID())
            {
                case 1: //place c4
                    _detonator.Show();
                    break;
                case 2: //Trigger Explosion
                    TriggerExplosive();
                    break;
            }
        }

        private void ReleasePlayerControl()
        {
            _canMove = false;
            _followCam.Priority = 9;
        }

        private void ReturnPlayerControl()
        {
            _model.SetActive(true);
            _canMove = true;
            _followCam.Priority = 10;
        }

        private void HidePlayer()
        {
            _model.SetActive(false);
        }
               
        private void TriggerExplosive()
        {
            _detonator.TriggerExplosion();
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete -= ReleasePlayerControl;
            Laptop.onHackEnded -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= ReleasePlayerControl;
            Forklift.onDriveModeExited -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= HidePlayer;
            Drone.OnEnterFlightMode -= ReleasePlayerControl;
            Drone.onExitFlightmode -= ReturnPlayerControl;
        }

    }
}

