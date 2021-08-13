using System;
using _Scripts.Unit.Player;
using KinematicCharacterController;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace _Scripts.Unit.AI
{
    public class MovementAI : MonoBehaviour, ICharacterController
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
        
        private Vector3 _internalVelocityAdd;
        
        private PhotonView _photonView;

        [Space]
        
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _detectionRange;

        private NavMeshPath _path;
        private Vector3[] _corners;
        private int _currentCorner;
        
        private void Start()
        {
            _motor = GetComponent<KinematicCharacterMotor>();
            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();

            _motor.CharacterController = this;
            
            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(_ =>
            {
                var results = new Collider[1];
                var size = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, results, _layerMask);
                for (int i = 0; i < size; i++)
                {
                    var position = results[i].transform.position;
                    SetDestination(position.x, position.y, position.z);
                }
            }).AddTo(this);
        }

        [PunRPC]
        private void SetDestination(float x, float y, float z)
        {
            var destination = new Vector3(x, y, z);
            
            _currentCorner = 1;
            _path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, _path);
            _corners = _path.corners;
        }

        private void Update()
        {
            if (_corners.Length < 2) return;
                
            _moveInputVector = (_corners[_currentCorner] - transform.position).normalized;

            _moveInputVector.y = 0;
            if (Vector3.Distance(transform.position, _corners[_currentCorner]) < .1f && _corners.Length > _currentCorner) _currentCorner++;
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
                case UnitState.Default:
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
                            var addedVelocity = _moveInputVector * (_airAccelerationSpeed * deltaTime);

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

                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
            }
        }
        
        public void AddVelocity(Vector3 velocity)
        {
            switch (_unit.CurrentState)
            {
                case UnitState.Default:
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
                case UnitState.Default:
                {
                    if(_moveInputVector != Vector3.zero)
                        currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(_moveInputVector, Vector3.up), _rotationSpeed * Time.deltaTime);
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