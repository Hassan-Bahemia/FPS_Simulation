using UnityEngine;

namespace Player
{
    public class PlayerCam : MonoBehaviour
    {
        [Header("Mouse Settings")]
        [SerializeField] private float m_sensX;
        [SerializeField] private float m_sensY;
        
        [Header("PlayerRefs")]
        [SerializeField] private Transform m_Orientation;
        [SerializeField] private float m_xRotation;
        [SerializeField] private float m_yRotation;
        
        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            //Get Mouse Input
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * m_sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * m_sensY;

            m_yRotation += mouseX;
            m_xRotation -= mouseY;
            m_xRotation = Mathf.Clamp(m_xRotation, -90f, 90f);
            
            //Rotate Cam and Orientation
            transform.rotation = Quaternion.Euler(m_xRotation, m_yRotation, 0);
            m_Orientation.rotation = Quaternion.Euler(0, m_yRotation, 0);
        }
    }
}
