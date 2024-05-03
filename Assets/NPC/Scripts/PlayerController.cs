using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Camera _camera;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Vector2 _velocity;
    private Vector2 _smoothDeltaPosition;


    private void Start()
    {
        _camera = Camera.main;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                _navMeshAgent.SetDestination(hit.point);
            }
        }

        SynchronizeAnimatorAndNavMeshAgent();
    }

    private void SynchronizeAnimatorAndNavMeshAgent()
    {
        Vector3 worldDeltaPosition = _navMeshAgent.nextPosition - transform.position;
        worldDeltaPosition.y = 0f;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
        _smoothDeltaPosition = Vector2.Lerp(_smoothDeltaPosition, deltaPosition, smooth);

        _velocity = _smoothDeltaPosition / Time.deltaTime;
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            _velocity = Vector2.Lerp(Vector2.zero, _velocity, _navMeshAgent.remainingDistance / _navMeshAgent.stoppingDistance);
        }

        bool shouldMove = _velocity.magnitude > 0.5f
            && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance;

        _animator.SetFloat("Speed", _velocity.magnitude);
        _animator.SetBool("IsMove", shouldMove);

        float deltaMagnitude = worldDeltaPosition.magnitude;
        if (deltaMagnitude > _navMeshAgent.radius / 2f)
        {
            transform.position = Vector3.Lerp(_animator.rootPosition, _navMeshAgent.nextPosition, smooth);
        }
    }

    private void OnAnimatorMove()
    {
        Vector3 rootPosition = _animator.rootPosition;
        rootPosition.y = _navMeshAgent.nextPosition.y;
        transform.position = rootPosition;
        _navMeshAgent.nextPosition = rootPosition;
    }
}
