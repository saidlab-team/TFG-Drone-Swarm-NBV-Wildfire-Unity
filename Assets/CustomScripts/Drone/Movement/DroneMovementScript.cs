using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovementScript : MonoBehaviour
{
    Rigidbody drone;
    private float upForce;

    private InputSystem_Actions m_actions;
    private InputSystem_Actions.DroneActions m_drone_actions;
    // Variables para el movimiento hacia arriba/abajo
    [HideInInspector] public float ascendInput = 0f;
    [HideInInspector] public float descendInput = 0f;

    // Variables para el movimiento hacia adelante
    private float movementForwardSpeed = 150.0f;
    private float tiltAmountForward = 0f;
    private float tiltVelocityForward;
    private Vector2 moveInput = Vector2.zero;

    public Vector2 moveCameraInput = Vector2.zero;

    // Variables para la rotacion
    private float rotateLeftInput = 0f;
    private float rotateRightInput = 0f;
    private float wantedYRotation;
    public float currentYRotation;
    private float rotateAmountByKeys = 2.5f;
    private float rotationYVelocity;

    // Variables para limitar la Velocidad del Dron
    private Vector3 VelocityToSmoothDampToZero;

    // Variables para la inclinacion cuando te mueves en horizontal
    private float tiltAmountSideways = 0f;
    private float tiltVelocitySideways;


    public float hoverHeight = 0f;        // altura objetivo en metros
    public float hoverStability = 2f;     // qué tan rápido corrige la altura

    [HideInInspector] public float changeCameraInput = 0f;

    public bool autoMode;
    private float autoModeInput = 0f;
    [HideInInspector] public Vector3 targetPosition;
    private Vector3 lastTargetPosition;

    [HideInInspector] public bool hasArrived;
    public bool MainDrone;
    private float maxAltitude = 40f; 

    void Awake()
    {
        m_actions = new InputSystem_Actions();
        m_drone_actions = m_actions.Drone;

        drone = GetComponent<Rigidbody>();
        
    }
    void OnEnable()
    {
        // Activar acciones
        if (MainDrone)
        {
            m_actions.Enable(); 
        }
        else
        {
            m_actions.Disable(); 
        }

        // Suscribirse a eventos de Up / Down
        m_drone_actions.Ascend.performed += ctx => ascendInput = 1f;
        m_drone_actions.Ascend.canceled += ctx => ascendInput = 0f;

        m_drone_actions.Descend.performed += ctx => descendInput = 1f;
        m_drone_actions.Descend.canceled += ctx => descendInput = 0f;

        // Suscribirse a eventos de Move
        m_drone_actions.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>().normalized;
        m_drone_actions.Move.canceled += ctx => moveInput = Vector2.zero;

        // Suscribirse a eventos de Rotate Left/Right
        m_drone_actions.Rotate_Left.performed += ctx => rotateLeftInput = 1f;
        m_drone_actions.Rotate_Left.canceled += ctx => rotateLeftInput = 0f;

        m_drone_actions.Rotate_Right.performed += ctx => rotateRightInput = 1f;
        m_drone_actions.Rotate_Right.canceled += ctx => rotateRightInput = 0f;

        // Suscribirse a eventos de cambio de camara FPV TPV
        m_drone_actions.Change_Camera.performed += ctx => changeCameraInput = 1f;
        m_drone_actions.Change_Camera.canceled += ctx => changeCameraInput = 0f;

        m_drone_actions.Change_Mode.performed += ctx => autoModeInput = 1f;
        m_drone_actions.Change_Mode.canceled += ctx => autoModeInput = 0f;

        // Suscribirse a eventos de Move Camera
        m_drone_actions.Move_Camera.performed += ctx => moveCameraInput = ctx.ReadValue<Vector2>();
        m_drone_actions.Move_Camera.canceled += ctx => moveCameraInput = Vector2.zero;

        
    }
    float targetX;
    void FixedUpdate()
    {
        if (!MainDrone)
        {
            AutoPilot();
        }
        ApplyMovement();
    }
    void Update()
    {
        
        if (!MainDrone)
        {
            //Debug.Log($"has arrived UPDATE {hasArrived}");
            
        }
        
    }
    
    

    void AutoPilot()
    {

        if (Vector3.Distance(drone.transform.position, targetPosition) > 3f )
        {
            hasArrived = false;
        }

        if (lastTargetPosition == targetPosition) return;
        //Debug.Log($"Has arrved AUTOPILOT {hasArrived}");
        if (hasArrived) return;

        // Darle los inputs dependiendo de la posicion actual y objetivo
        if (Vector3.Distance(drone.transform.position, targetPosition) < 3f ) // Le damos 3 unidades de margen para que no necesite estar en la posicion exacta
        {
            // limpiar inputs
            ascendInput=0f;
            descendInput=0f;
            rotateLeftInput = 0f;
            rotateRightInput = 0f;
            moveInput = Vector2.zero;

            float brakingSpeed = 5f; // Velocidad de frenado
            drone.linearVelocity = Vector3.Lerp(
                drone.linearVelocity, 
                Vector3.zero, 
                Time.deltaTime * brakingSpeed
            );
        
            if (drone.linearVelocity.magnitude < 0.5f)
            {
                drone.linearVelocity = Vector3.zero; // Parar completamente
                lastTargetPosition = targetPosition;
                hasArrived = true;
            }
            
            return; // Ya ha llegado a la poscion indicada
        }
        // Continua en caso de que le falte llegar 
        // Ajustar Arriba Abajo
        // Limite de 10m por encima del suelo ¿?
        if (targetPosition.y > maxAltitude) targetPosition.y = maxAltitude;

        if (drone.transform.position.y > targetPosition.y + 2f)
        {
            // Debe descender
            descendInput = 1f;
            ascendInput = 0f;
        }
        else if (drone.transform.position.y < targetPosition.y - 2f)
        {
            //Debe ascender 
            ascendInput = 1f;
            descendInput = 0f;
            //Debug.Log($" A {drone.transform.position.y} -- {targetPosition.y - 2f}");
        }
        else
        {
            // no hace nada 
            ascendInput = 0f;
            descendInput = 0f;
            //Debug.Log($" N {drone.transform.position.y} -- {targetPosition.y - 2f}");
        }

        // Ajustar Adelante atras
        // Ajustar Dcha Izq
        // Ajustar Rotacion

        Vector3 directionToPoint = targetPosition - drone.transform.position ;
        directionToPoint.y = 0f;
        // coordenada y no nos interesa. solo X(x) Z(y)
        //if (directionToPoint.magnitude > 5f)

        // Rotacion
        float targetAngle = Mathf.Atan2(directionToPoint.x, directionToPoint.z) * Mathf.Rad2Deg;
        float angleDifference = Mathf.DeltaAngle(currentYRotation, targetAngle);
        
        
        Vector3 horizontalDirection = targetPosition - drone.transform.position;
        horizontalDirection.y = 0f; 
        float horizontalDistance = horizontalDirection.magnitude;

        rotateLeftInput = 0f;
        rotateRightInput = 0f;
        if (horizontalDistance > 3f)
        {
            wantedYRotation = targetAngle;  
        }


        
        if (Mathf.Abs(angleDifference) < 45f){
            horizontalDirection.Normalize();
            Vector3 localDirection = drone.transform.InverseTransformDirection(horizontalDirection);
            
            float speedMultiplier = 1f;
            
            if (horizontalDistance < 15f)
            {
                // Reducir velocidad gradualmente
                speedMultiplier = Mathf.Clamp01(horizontalDistance / 15f);
                speedMultiplier = Mathf.Max(speedMultiplier, 0.15f); // Mínimo 15%
            }
            
            if (horizontalDistance < 10f)
            {
                // Cerca: solo adelante (sin laterales)
                moveInput = new Vector2(0f, localDirection.z) * speedMultiplier;
            }
            else
            {
                // Lejos: movimiento completo
                moveInput = new Vector2(localDirection.x, localDirection.z) * speedMultiplier;
            }
        }
        else
        {
            moveInput = Vector2.zero;
        }

    }

    void OnDisable()
    {
        m_actions.Disable();
    }

    void ApplyMovement()
    {
        MovementUp();
        MovementFowardSideways();
        Rotation();
        ClampingSpeedValues();
        CorrectRotation();
        //ChangeMode();
    }


    void MovementUp()
    {
        
        if (ascendInput > 0f)
        {
            upForce = 20f;

        }
        else if (descendInput > 0f)
        {
            upForce = -20f;
        }
        else if ((ascendInput <= 0f) && (descendInput <= 0f) /*&& (Mathf.Abs(moveInput.y) < 0.2f) && (Mathf.Abs(moveInput.x) < 0.2f)*/)
        {

            upForce = 9.81f;
        }

        Vector3 velocity = drone.linearVelocity;
        velocity.y = Mathf.Clamp(velocity.y, - 5f, 5f);
        drone.linearVelocity = velocity;
        

        hoverHeight = transform.position.y;
        drone.AddRelativeForce(Vector3.up * upForce);
    }

    void MovementFowardSideways()
    {

        if (MainDrone)
        {
            // 1. Obtener la dirección de la cámara (la mirada del usuario en VR)
            // Usamos Camera.main o la referencia a tu cámara de VR
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            // 2. Proyectar en el plano horizontal (y = 0) para evitar que el dron 
            // intente hundirse en el suelo si miras hacia abajo.
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 3. Calcular los vectores de movimiento relativos a la mirada
            Vector3 moveDirection = (cameraForward * moveInput.y) + (cameraRight * moveInput.x);

            // 4. Aplicar la fuerza de movimiento
            // Usamos AddForce para que el movimiento sea físico y fluido
            float moveSpeed = 5f; // Ajusta según necesites
            drone.AddForce(moveDirection * moveSpeed, ForceMode.Acceleration);

            // 5. Mantener tu lógica de inclinación (Visual)
            tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 20 * moveInput.y, ref tiltVelocityForward, 0.1f);
            tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, -20 * moveInput.x, ref tiltVelocitySideways, 0.1f);

            // 6. Hover para mantener la altitud (Tu código original)
            float desiredHeight = hoverHeight;
            float heightError = desiredHeight - transform.position.y;
            float verticalCorrection = heightError * hoverStability;
            drone.AddForce(Vector3.up * verticalCorrection, ForceMode.Acceleration);

            // 7. Rotación del Chasis
            // Importante: Si quieres que el dron rote para mirar hacia donde va, 
            // deberías actualizar 'currentYRotation' con la rotación Y de la cámara.
            currentYRotation = Camera.main.transform.eulerAngles.y;

            drone.rotation = Quaternion.Euler(
                new Vector3(tiltAmountForward, currentYRotation, tiltAmountSideways)
            );
        }else
        {
            Vector3 forward = transform.forward * moveInput.y;

            Vector3 right = transform.right * moveInput.x;



            tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 20 * moveInput.y, ref tiltVelocityForward, 0.1f);

            tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, -20 * moveInput.x, ref tiltVelocitySideways, 0.1f);



            // Hover para mantener la altitud

            float desiredHeight = hoverHeight;

            float heightError = desiredHeight - transform.position.y;

            float verticalCorrection = heightError * hoverStability;



            drone.AddForce(Vector3.up * verticalCorrection, ForceMode.Acceleration);



            drone.rotation = Quaternion.Euler(

                new Vector3(tiltAmountForward, currentYRotation, tiltAmountSideways)

            );
        }
        
    }
    /*
    void MovementFowardSideways()

    {

        Vector3 forward = transform.forward * moveInput.y;

        Vector3 right = transform.right * moveInput.x;



        tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward, 20 * moveInput.y, ref tiltVelocityForward, 0.1f);

        tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways, -20 * moveInput.x, ref tiltVelocitySideways, 0.1f);



        // Hover para mantener la altitud

        float desiredHeight = hoverHeight;

        float heightError = desiredHeight - transform.position.y;

        float verticalCorrection = heightError * hoverStability;



        drone.AddForce(Vector3.up * verticalCorrection, ForceMode.Acceleration);



        drone.rotation = Quaternion.Euler(

            new Vector3(tiltAmountForward, currentYRotation, tiltAmountSideways)

        );

    }
    */


    void Rotation()
    {
        if (rotateLeftInput > 0f)
        {
            wantedYRotation -= rotateAmountByKeys;
        }
        if (rotateRightInput > 0f)
        {
            wantedYRotation += rotateAmountByKeys;
        }
        currentYRotation = Mathf.SmoothDamp(currentYRotation, wantedYRotation, ref rotationYVelocity, 0.25f);
    }
    void ClampingSpeedValues()
    {
        
        if (Mathf.Abs(moveInput.y) > 0.2f && Mathf.Abs(moveInput.x) > 0.2f)
        {
            drone.linearVelocity  = Vector3.ClampMagnitude(drone.linearVelocity , Mathf.Lerp(drone.linearVelocity .magnitude, 10.0f, Time.deltaTime * 5f));
        }
        else if (Mathf.Abs(moveInput.y) > 0.2f && Mathf.Abs(moveInput.x) < 0.2f)
        {
            drone.linearVelocity  = Vector3.ClampMagnitude(drone.linearVelocity , Mathf.Lerp(drone.linearVelocity .magnitude, 10.0f, Time.deltaTime * 5f));
        }
        else if (Mathf.Abs(moveInput.y) < 0.2f && Mathf.Abs(moveInput.x) > 0.2f)
        {
            drone.linearVelocity  = Vector3.ClampMagnitude(drone.linearVelocity , Mathf.Lerp(drone.linearVelocity .magnitude, 5.0f, Time.deltaTime * 5f));
        }
        else if (Mathf.Abs(moveInput.y) < 0.2f && Mathf.Abs(moveInput.x) < 0.2f )
        {
            drone.linearVelocity  = Vector3.SmoothDamp(drone.linearVelocity , Vector3.zero , ref VelocityToSmoothDampToZero, 0.95f);
        }
        else
        {
        drone.linearVelocity = Vector3.SmoothDamp(
            drone.linearVelocity, 
            Vector3.zero, 
            ref VelocityToSmoothDampToZero, 
            0.95f // Cambiar de 0.95f a 0.2f
        );
    }
    }
    void CorrectRotation()
    {
        if (ascendInput == 0f && descendInput == 0f && rotateRightInput == 0f && rotateLeftInput == 0f && moveInput.x == 0f && moveInput.y == 0f) 
        {
            drone.rotation = Quaternion.Euler(
                new Vector3(0f, currentYRotation, 0f)
            );
        }
    }
    void ChangeMode()
    {
        if (autoModeInput > 0f)
        {
            MainDrone = false;
        }
        else
        {
            MainDrone = true;
        }
    }


}
