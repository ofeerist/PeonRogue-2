using System;
using KinematicCharacterController;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace _Scripts.Unit.AI
{
    public class MovementAI : MonoBehaviour, ICharacterController
    {
        [HideInInspector] public Vector3 ToTarget;
        
        private KinematicCharacterMotor _motor;
        private Unit _unit;
        
        private Vector3 _moveInputVector = Vector3.zero;
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

        [Space]
        
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private bool _chase;
        [SerializeField] private float _detectionRange;
        [SerializeField] private float _minDetectionRange;
        
        [SerializeField] private bool _retreat;
        [SerializeField] private float _retreatDistance;

        public delegate void MovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint);
        public event MovementHit ByMovementHit;
        
        private NavMeshPath _path;
        private readonly Vector3[] _corners = new Vector3[5];
        private int _currentCorner;
        private static readonly int Speed = Animator.StringToHash("Speed");

        private readonly Collider[] _results = new Collider[1];

        public void SetData(float speed, bool chase, float detectionRange, float minDetectionRange, bool retreat,
            float retreatDistance)
        {
            _maxStableMoveSpeed = speed;
            _chase = chase;
            _detectionRange = detectionRange;
            _minDetectionRange = minDetectionRange;
            _retreat = retreat;
            _retreatDistance = retreatDistance;
        }
        
        private void Start()
        {
            _motor = GetComponent<KinematicCharacterMotor>();
            _unit = GetComponent<Unit>();

            _motor.CharacterController = this;
            _path = new NavMeshPath();

            if (!PhotonNetwork.IsMasterClient)
            {
                _motor.enabled = false;
                return;
            }

            Observable.EveryUpdate().Subscribe(x =>
            {
                _unit.Animator.SetFloat(Speed, _motor.Velocity.magnitude);

                if (_corners[_currentCorner] == null)
                    return;

                _moveInputVector = (_corners[_currentCorner] - transform.position).normalized;

                _moveInputVector.y = 0;
                if (Vector3.Distance(transform.position, _corners[_currentCorner]) < .1f &&
                    _corners.Length > _currentCorner) _currentCorner++;
            }).AddTo(this);
            
            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(_ =>
            {
                if (_chase)
                {
                    var size = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, _results, _layerMask);
                    
                    for (int i = 0; i < size; i++)
                    {
                        var position = _results[i].transform.position;
                        var distance = Vector3.Distance(transform.position, position);

                        if (distance < _retreatDistance && _retreat) continue;
                        if (distance < _minDetectionRange) continue;
                        
                        if (distance < .5f) position = transform.position;

                        _currentCorner = 1;
                        if (NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, _path))
                            _path.GetCornersNonAlloc(_corners);
                    }
                }

                if (_retreat)
                {
                    var size = Physics.OverlapSphereNonAlloc(transform.position, _retreatDistance, _results, _layerMask);
                    for (int i = 0; i < size; i++)
                    {
                        var selfp = transform.position;
                        var targetp = _results[i].transform.position;
                        var position = selfp + (selfp - targetp).normalized * _retreatDistance;
                        
                        if (!NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, _path) || _motor.Velocity.magnitude < 0.1f)
                            position = RandomNavmeshLocation(1);
                        
                        _currentCorner = 1;
                        if(NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, _path))
                            _path.GetCornersNonAlloc(_corners);
                    }
                }
            }).AddTo(this);
        }

        private Vector3 RandomNavmeshLocation(float radius)
        {
            var randomDirection = (Random.insideUnitSphere * radius) + transform.position;
            var finalPosition = Vector3.zero;

            if (NavMesh.SamplePosition(randomDirection, out var hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
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
                case UnitState.Attack:
                {
                    currentVelocity = Vector3.zero;

                    currentVelocity += _gravity * deltaTime;
                    currentVelocity *= (1f / (1f + (_drag * deltaTime)));
                    
                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
                case UnitState.InStan:
                {
                    currentVelocity += _gravity * deltaTime;
                    currentVelocity *= (1f / (1f + (_drag * deltaTime)));
                    
                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
                case UnitState.Dash:
                {
                    currentVelocity += _gravity * deltaTime;
                    currentVelocity *= (1f / (1f + (_drag * deltaTime)));
                    
                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity = GetReorientedInput(ref currentVelocity, _internalVelocityAdd.normalized) * _internalVelocityAdd.magnitude;
                        
                        _internalVelocityAdd = Vector3.zero;
                    }
                    else
                    {
                        currentVelocity = Vector3.zero;
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
                case UnitState.Dash:
                case UnitState.InStan:
                case UnitState.Attack:
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
                case UnitState.Attack:
                {
                    var dir = (ToTarget - transform.position).normalized;
                    dir.y = 0;
                    
                    currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(dir, Vector3.up), _rotationSpeed * Time.deltaTime);
                    
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
            if (coll.gameObject.layer == 11) return false;
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {

        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            ByMovementHit?.Invoke(hitCollider, hitNormal, hitPoint);
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