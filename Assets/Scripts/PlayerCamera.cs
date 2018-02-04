/*
 *  PlayerCamera.cs
 *  Author: Tomaz Saraiva (addcomponent.com)
 */
using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    // serializables

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private float _distance;

    [SerializeField]
    private float _speedMovement;

    [SerializeField]
    private float _speedRotation;

    [SerializeField]
    private float _wallOffset;



    // fields

    Quaternion _targetRotation;

    Vector3 _desiredPosition;
    Vector3 _adjustedPosition;
    
    Collider _collider;
    bool _inTrigger;

    RaycastHit _hit;



    // MonoBehaviour

    void Start()
    {
        if(_player == null)
        {
            Debug.LogError("You need to assign a Player Object!");
        }

        StartCoroutine(LinecastCoroutine());
    }

    void Update()
    {
        // direction vector from the camera to the player position
        Vector3 direction = _player.transform.position - transform.position;
        
        // create a rotation aligned with the previous direction vector
        _targetRotation = Quaternion.LookRotation(direction);

        // only apply the X axis of the rotation
        // this way even if the camera is falling behind when following the player 
        // it won't rotate sideways
        _targetRotation = Quaternion.Euler(_targetRotation.eulerAngles.x, 0, 0);

        // change the camera rotation in update
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _speedRotation);


        // target position
        _desiredPosition = new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z - _distance);

        // update the adjusted position
        _adjustedPosition = _desiredPosition;

        // if the camera collided with something
        if (_collider != null)
        {
            // change the camera position to collider closest point to the player
            _adjustedPosition.z = _collider.ClosestPoint(_player.transform.position).z + _wallOffset;

            // if the player is far enough from the camera just follow him
            if (_player.transform.position.z - _adjustedPosition.z > _distance)
            {
                _collider = null;
            }
        }

        // change the camera position
        transform.position = Vector3.Lerp(transform.position, _adjustedPosition, Time.deltaTime * _speedMovement);
    }

    private void OnTriggerEnter(Collider other)
    {
        _inTrigger = true;
        _collider = other;
    }

    private void OnTriggerExit(Collider other)
    {
        _inTrigger = false;
        _collider = null;
    }



    // PlayerCamera

    private IEnumerator LinecastCoroutine()
    {
        while(true)
        {
            bool occluded = Physics.Linecast(transform.position, _player.transform.position, out _hit);

            if (_collider == null && occluded) // if the camera it's not fixed and the player is occluded
            {
                _collider = _hit.transform.GetComponent<Collider>();
            }
            else if(!occluded && !_inTrigger) // else if not occluded nor in any trigger, release the camera
            {
                if(!Physics.Linecast(_desiredPosition, _player.transform.position, out _hit))
                {
                    _collider = null;
                }                
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}