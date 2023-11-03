using UnityEngine;
using UnityEngine.EventSystems;

namespace VIPE_SDK
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform targetPosition;
        public float offset = 2.0f;
        public float distance = 2.0f;
        public float xSpeed = 1.0f;
        public float ySpeed = 5.0f;
        public float yMinLimit = -90f;
        public float yMaxLimit = 90f;
        public float distanceMin = 2f;
        public float distanceMax = 10f;
        public float smoothTime = 2f;
        public float panSpeed = 1f;
        float rotationYAxis = 0.0f;
        float rotationXAxis = 0.0f;
        float velocityX = 0.0f;
        float velocityY = 0.0f;

        private Vector3 resetPosition;
        private Quaternion resetRotation;
        private Transform targetPositionAux;
        public LayerMask uiLayer;
        void Start()
        {
            //targetPosition.position.Set(targetPosition.position.x, targetPosition.position.y + 2, targetPosition.position.z);
            //resetPosition = transform.position;
            //resetRotation = transform.rotation;
            resetPosition = transform.position;
            resetRotation = transform.rotation;

            Vector3 angles = transform.eulerAngles;
            rotationYAxis = angles.y;
            rotationXAxis = angles.x;
        }

        public void ResetCamera()
        {
            targetPositionAux = targetPosition;
            targetPosition = null;
            Debug.Log("Reset Camera");
            Debug.Log("Current Position" + transform.position);
            Debug.Log("Current Rotation" + transform.rotation);
            Debug.Log("Reset Position: " + resetPosition);
            Debug.Log("Reset Rotation: " + resetRotation);
            distance = 1;
            xSpeed = 5;
            ySpeed = 5;
            yMinLimit = -89f;
            yMaxLimit = 89f;
            distanceMin = 1f;
            distanceMax = 10f;
            smoothTime = 2f;
            panSpeed = 6f;
            targetPositionAux.position = new Vector3(0, 1.5f, 0);
            targetPositionAux.rotation = Quaternion.identity;
            targetPosition = targetPositionAux;
            targetPosition.position = targetPositionAux.position;
            targetPosition.rotation = targetPositionAux.rotation;
            //targetPosition = targetPositionAux;
            //targetPosition.position.Set(targetPosition.position.x, targetPosition.position.y + offset, targetPosition.position.z);
            //resetPosition = transform.position;
            //resetRotation = transform.rotation;
        }

        void LateUpdate()
        {
            if (targetPosition && !IsPointerOverUI())
            {
                velocityX += xSpeed * Input.GetAxis("Mouse X") * 0.02f;
                velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;

                Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
                Vector3 negDistance = new Vector3(0, 0, -distance);
                Vector3 targetPos = new Vector3(targetPosition.position.x, targetPosition.position.y + offset, targetPosition.position.z);
                Vector3 position = toRotation * negDistance + targetPos;

                transform.position = position;

                rotationYAxis += velocityX;
                rotationXAxis -= velocityY;
                rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

                transform.rotation = toRotation;

                velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
                velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);

                //// Handle panning
                //if (Input.GetMouseButton(1))
                //{
                //    float panX = -panSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
                //    float panY = -panSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;

                //    transform.Translate(transform.right * panX, Space.World);
                //    transform.Translate(transform.up * panY, Space.World);

                //    targetPosition.Translate(transform.right * panX, Space.World);
                //    targetPosition.Translate(transform.up * panY, Space.World);

                //    distance = Vector3.Distance(transform.position, targetPosition.position);
                //}
            }

        }

        bool IsPointerOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}