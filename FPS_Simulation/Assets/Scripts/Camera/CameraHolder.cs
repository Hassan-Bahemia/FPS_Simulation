using System;
using UnityEngine;

namespace Camera
{
    public class CameraHolder : MonoBehaviour
    {
        [SerializeField] private Transform cameraPosition;

        private void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}
