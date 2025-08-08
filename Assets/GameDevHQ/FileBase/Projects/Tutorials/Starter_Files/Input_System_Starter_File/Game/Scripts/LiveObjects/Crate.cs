using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private PlayerInputAction _input;
        private Coroutine _punchRoutine;
        private bool _inZone = false;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;

            _input = new PlayerInputAction();
            _input.Player.Enable();

            _input.Player.PunchHold.performed += PunchHold_performed;
            _input.Player.PunchTap.performed += PunchTap_performed;         
            _input.Player.PunchHold.canceled += PunchHold_canceled;
        }

        private void PunchTap_performed(InputAction.CallbackContext obj)
        {
            Debug.Log($"[TAP] Action: {obj.action.name}, Phase: {obj.phase}, Time: {obj.time}, Started: {obj.started}");

            if (_isReadyToBreak && _brakeOff.Count > 0 && _inZone)
            {
                BreakPart();
                StartCoroutine(PunchDelay());
            }
        }

        private void PunchHold_performed(InputAction.CallbackContext obj)
        {
            Debug.Log($"[HOLD] Action: {obj.action.name}, Phase: {obj.phase}, Time: {obj.time}, Started: {obj.started}");

            

            if (_isReadyToBreak && _brakeOff.Count > 0 && _inZone)
            {
                BreakPart();
                BreakPart();
                BreakPart();
                BreakPart();
                StartCoroutine(PunchDelay());
            }
            
        }

        private void PunchHold_canceled(InputAction.CallbackContext obj)
        {
            Debug.Log($"HOLD CANCELED at {Time.time}");
            // Stop breaking when released
            if (_punchRoutine != null)
            {
                StopCoroutine(_punchRoutine);
                _punchRoutine = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                _inZone = true;
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {

            if (_isReadyToBreak == false && _brakeOff.Count > 0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    _inZone = true;
                    
                }
                else if (_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
        }

        public void BreakPart()
        {
            int rng = Random.Range(0, _brakeOff.Count); 
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;

            if (_input != null)
            {
                _input.Player.PunchTap.performed -= PunchTap_performed;
                _input.Player.PunchHold.performed -= PunchHold_performed;
                _input.Player.PunchHold.canceled -= PunchHold_canceled;
                _input.Player.Disable();
            }
        }
    }
}
