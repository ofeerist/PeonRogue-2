
using System;
using System.Numerics;
using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace _Scripts.Unit.Player
{
    public class Movement : MonoBehaviour, ICharacterController
    {
        private KinematicCharacterMotor _motor;
        private Unit _unit;
        
        private Vector3 _moveInputVector;
        private bool _toDash;
        private Vector3 _dashDirection;
        
        [SerializeField] private Vector3 _gravity;

        [Space]
        
        [SerializeField] private float _maxStableMoveSpeed;
        [SerializeField] private float _stableMovementSharpness;
        
        [Space]
        
        [SerializeField] private float _airAccelerationSpeed;
        [SerializeField] private float _maxAirMoveSpeed;
        
        [Space]
        
        [SerializeField] private float _drag;
        
        [Space]
        
        [SerializeField] private float _rotationSpeed;

        [Space]
        
        [SerializeField] private float _dashSpeed;
        [SerializeField] private float _dashLenght;
        private float _dashTimer;
        
        private Vector3 _internalVelocityAdd;
        
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Dash = Animator.StringToHash("Dash");


        private void Start()
        {
            _motor = GetComponent<KinematicCharacterMotor>();
            _unit = GetComponent<Unit>();
            
            _motor.CharacterController = this;
        }

        private void Update()
        {
            _moveInputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            
            _unit.Animator.SetBool(Walk, _motor.Velocity.magnitude > 0.01f && _unit.CurrentState == PlayerState.Default);

            _toDash = Input.GetKey(KeyCode.Space);
        }

        private Vector3 GetReorientedInput(ref Vector3 currentVelocity, Vector3 input)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;

            var effectiveGroundNormal = _motor.GroundingStatus.GroundNormal;
            if (currentVelocityMagnitude > 0f && _motor.GroundingStatus.SnappingPrevented)
            {
                var groundPointToCharacter = _motor.TransientPosition - _motor.GroundingStatus.GroundPoint;
                effectiveGroundNormal = Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f ? _motor.GroundingStatus.OuterGroundNormal : _motor.GroundingStatus.InnerGroundNormal;
            }
                        
            currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
                        
            var inputRight = Vector3.Cross(input, _motor.CharacterUp);
            var reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * input.magnitude;
            return reorientedInput;
        }
        
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // code from example
            switch (_unit.CurrentState)
            {
                case PlayerState.Default:
                {
                    if (_motor.GroundingStatus.IsStableOnGround)
                    {
                        var targetMovementVelocity = GetReorientedInput(ref currentVelocity, _moveInputVector) * _maxStableMoveSpeed;
                        
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_stableMovementSharpness * deltaTime));
                    }
                    else
                    {
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            var addedVelocity = _moveInputVector * _airAccelerationSpeed * deltaTime;

                            var currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, _motor.CharacterUp);
                            
                            if (currentVelocityOnInputsPlane.magnitude < _maxAirMoveSpeed)
                            {
                                var newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, _maxAirMoveSpeed);
                                addedVelocity = newTotal - currentVelocityOnInputsPlane;
                            }
                            else
                            {
                                if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                                {
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                                }
                            }
                            
                            if (_motor.GroundingStatus.FoundAnyGround)
                            {
                                if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                                {
                                    var perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal), _motor.CharacterUp).normalized;
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                                }
                            }
                            
                            currentVelocity += addedVelocity;
                        }
                        
                        currentVelocity += _gravity * deltaTime;
                        
                        currentVelocity *= (1f / (1f + (_drag * deltaTime)));
                    }
                    
                    // Dash
                    if (_toDash)
                    {
                        _unit.CurrentState = PlayerState.Dash;
                        _unit.Animator.SetTrigger(Dash);

                        _dashDirection = _moveInputVector;
                        if (_moveInputVector == Vector3.zero)
                        {
                            var ray = _unit.Camera.ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out var hit))
                            {
                                var toPoint = (hit.point - transform.position).normalized;
                                toPoint.y = 0;
                                _dashDirection = toPoint;
                            }
                        }

                        _dashTimer = Time.time + _dashLenght;
                    }

                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
                case PlayerState.Dash:
                {
                    if (_dashTimer <= Time.time) _unit.CurrentState = PlayerState.Default;
                    
                    var targetMovementVelocity = GetReorientedInput(ref currentVelocity, _dashDirection) * _dashSpeed;
                    
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_stableMovementSharpness * deltaTime));
                    
                    break;
                }
            }
        }
        
        public void AddVelocity(Vector3 velocity)
        {
            switch (_unit.CurrentState)
            {
                case PlayerState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
            }
        }
        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (_unit.CurrentState)
            {
                case PlayerState.Default:
                {
                    if(_moveInputVector != Vector3.zero)
                        currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(_moveInputVector, Vector3.up), _rotationSpeed * Time.deltaTime);
                    break;
                }
                case PlayerState.Dash:
                {
                    currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(_dashDirection, Vector3.up), _rotationSpeed * Time.deltaTime);
                    break;
                }
            }
            
        }
        
        public void BeforeCharacterUpdate(float deltaTime)
        {

        }

        public void PostGroundingUpdate(float deltaTime)
        {

        }

        public void AfterCharacterUpdate(float deltaTime)
        {

        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {

        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {

        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {

        }
    }
}