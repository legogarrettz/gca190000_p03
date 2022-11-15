using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HamJoyGames
{
    public class Comp_CharacterController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _runSpeed = 6f;
        [SerializeField] private float _sprintSpeed = 8f;

        [Header("Sharpness")]
        [SerializeField] private float _rotationSharpness = 10f;
        [SerializeField] private float _moveSharpness;

        private Animator _animator;
        private Comp_PlayerInputs _inputs;
        private Comp_CameraContoller _cameraController;

        private bool _strafing;
        private bool _sprinting;
        private float _strafeParameter;
        private Vector3 _strafeParametersXZ;

        private float _targetSpeed;
        private Quaternion _targetRotation;

        private float _newSpeed;
        private Vector3 _newVelocity;
        private Quaternion _newRotation;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _inputs = GetComponent<Comp_PlayerInputs>();
            _cameraController = GetComponent<Comp_CameraContoller>();

            _animator.applyRootMotion = false;
        }

        private void Update()
        {
            Vector3 _moveInputVector = new Vector3(_inputs.MoveAxisRightRaw, 0, _inputs.MoveAxisForwardRaw);
            Vector3 _cameraPlanarDirection = _cameraController.CameraPlanarDirection;
            Quaternion _cameraPLanarRotation = Quaternion.LookRotation(_cameraPlanarDirection);

            Vector3 _moveInputVectorOriented = _cameraPLanarRotation * _moveInputVector.normalized;

            if (_strafing)
            {
                _sprinting = _inputs.Sprint.PressedDown() && (_moveInputVector != Vector3.zero);
                _strafing = _inputs.Aim.Pressed() && !_sprinting;
            }
            else
            {
                _sprinting = _inputs.Sprint.Pressed() && (_moveInputVector != Vector3.zero);
                _strafing = _inputs.Aim.PressedDown() && !_sprinting;
            }


            //Move Speed
            if(_sprinting) { _targetSpeed = _moveInputVector != Vector3.zero ? _sprintSpeed : 0; }
            else if (_strafing) { _targetSpeed = _moveInputVector != Vector3.zero ? _walkSpeed : 0; }
            else                         { _targetSpeed = _moveInputVector != Vector3.zero ? _runSpeed : 0; }
            _newSpeed = Mathf.Lerp(_newSpeed, _targetSpeed, Time.deltaTime * _moveSharpness);

            //Velocity
            _newVelocity = _moveInputVectorOriented * _targetSpeed;
            transform.Translate(_newVelocity * Time.deltaTime, Space.World);


            //Rotaion
            if (_strafing)
            {
                _targetRotation = Quaternion.LookRotation(_cameraPlanarDirection);
                _newRotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
                transform.rotation = _newRotation;
            }
            else if (_targetSpeed !=0)
            {
                _targetRotation = Quaternion.LookRotation(_moveInputVectorOriented);
                _newRotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
                transform.rotation = _newRotation;
            }


            //Animations
            if (_strafing)
            {
                _strafeParameter = Mathf.Clamp01(_strafeParameter + Time.deltaTime * 4);
                _strafeParametersXZ = Vector3.Lerp(_strafeParametersXZ, _moveInputVector * _newSpeed, _moveSharpness * Time.deltaTime);
            }
            else
            {
                _strafeParameter = Mathf.Clamp01(_strafeParameter - Time.deltaTime * 4);
                _strafeParametersXZ = Vector3.Lerp(_strafeParametersXZ, Vector3.forward * _newSpeed, _moveSharpness * Time.deltaTime);
            }
            _animator.SetFloat("Strafing", _strafeParameter);
            _animator.SetFloat("StrafingX", Mathf.Round(_strafeParametersXZ.x * 100f) / 100f);
            _animator.SetFloat("StrafingZ", Mathf.Round(_strafeParametersXZ.z * 100f) / 100f);



        }



    }
}