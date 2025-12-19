using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Necessário para o novo Input System
using CMF;

// Este controlador fornece funcionalidade básica de 'clicar para mover' usando o novo Input System;
public class ClickToMoveController : Controller
{
    // Velocidade de movimento do controlador;
    public float movementSpeed = 10f;
    // Gravidade para baixo;
    public float gravity = 30f;

    float currentVerticalSpeed = 0f;
    bool isGrounded = false;

    // Posição atual para onde mover;
    Vector3 currentTargetPosition;
    // Se a distância entre o controlador e a posição alvo for menor que isso, o alvo foi alcançado;
    float reachTargetThreshold = 0.001f;

    // Se o usuário pode segurar o botão do mouse para mover continuamente o controlador;
    public bool holdMouseButtonToMove = false;

    // --- NOVA FUNCIONALIDADE: Escolha do botão do mouse ---
    public enum MouseButtonSelection
    {
        Left,
        Right
    }

    [Header("Input Settings")]
    [Tooltip("Escolha qual botão do mouse será usado para mover o personagem.")]
    public MouseButtonSelection moveButton = MouseButtonSelection.Left;
    // -----------------------------------------------------

    public enum MouseDetectionType
    {
        AbstractPlane,
        Raycast
    }
    public MouseDetectionType mouseDetectionType = MouseDetectionType.AbstractPlane;

    // Layermask usada quando 'Raycast' é selecionado;
    public LayerMask raycastLayerMask = ~0;

    // Variáveis de Timeout;
    public float timeOutTime = 1f;
    float currentTimeOutTime = 1f;
    public float timeOutDistanceThreshold = 0.05f;
    Vector3 lastPosition;

    // Referência à câmera do jogador;
    public Camera playerCamera;

    // Se o controlador tem um alvo válido atualmente;
    bool hasTarget = false;

    Vector3 lastVelocity = Vector3.zero;
    Vector3 lastMovementVelocity = Vector3.zero;

    // Plano abstrato usado quando 'AbstractPlane' é selecionado;
    Plane groundPlane;

    // Referência aos componentes anexados;
    Mover mover;
    Transform tr;

    void Start()
    {
        mover = GetComponent<Mover>();
        tr = transform;

        if (playerCamera == null)
            Debug.LogWarning("Nenhuma câmera foi atribuída a este controlador!", this);

        lastPosition = tr.position;
        currentTargetPosition = transform.position;
        groundPlane = new Plane(tr.up, tr.position);
    }

    void Update()
    {
        HandleMouseInput();
    }

    void FixedUpdate()
    {
        mover.CheckForGround();
        isGrounded = mover.IsGrounded();
        HandleTimeOut();

        Vector3 _velocity = Vector3.zero;

        _velocity = CalculateMovementVelocity();
        lastMovementVelocity = _velocity;

        HandleGravity();
        _velocity += tr.up * currentVerticalSpeed;

        mover.SetExtendSensorRange(isGrounded);
        mover.SetVelocity(_velocity);

        lastVelocity = _velocity;
    }

    Vector3 CalculateMovementVelocity()
    {
        if (!hasTarget)
            return Vector3.zero;

        Vector3 _toTarget = currentTargetPosition - tr.position;
        _toTarget = VectorMath.RemoveDotVector(_toTarget, tr.up);

        float _distanceToTarget = _toTarget.magnitude;

        if (_distanceToTarget <= reachTargetThreshold)
        {
            hasTarget = false;
            return Vector3.zero;
        }

        Vector3 _velocity = _toTarget.normalized * movementSpeed;

        if (movementSpeed * Time.fixedDeltaTime > _distanceToTarget)
        {
            _velocity = _toTarget.normalized * _distanceToTarget;
            hasTarget = false;
        }

        return _velocity;
    }

    void HandleGravity()
    {
        if (!isGrounded)
            currentVerticalSpeed -= gravity * Time.deltaTime;
        else
        {
            if (currentVerticalSpeed < 0f)
            {
                if (OnLand != null)
                    OnLand(tr.up * currentVerticalSpeed);
            }

            currentVerticalSpeed = 0f;
        }
    }

    void HandleMouseInput()
    {
        if (playerCamera == null)
            return;

        // Verifica input usando os novos métodos baseados no Input System
        if (!holdMouseButtonToMove && WasMouseButtonJustPressed() || holdMouseButtonToMove && IsMouseButtonPressed())
        {
            Ray _mouseRay = playerCamera.ScreenPointToRay(GetMousePosition());

            if (mouseDetectionType == MouseDetectionType.AbstractPlane)
            {
                groundPlane.SetNormalAndPosition(tr.up, tr.position);
                float _enter = 0f;

                if (groundPlane.Raycast(_mouseRay, out _enter))
                {
                    currentTargetPosition = _mouseRay.GetPoint(_enter);
                    hasTarget = true;
                }
                else
                    hasTarget = false;
            }
            else if (mouseDetectionType == MouseDetectionType.Raycast)
            {
                RaycastHit _hit;

                if (Physics.Raycast(_mouseRay, out _hit, 100f, raycastLayerMask, QueryTriggerInteraction.Ignore))
                {
                    currentTargetPosition = _hit.point;
                    hasTarget = true;
                }
                else
                    hasTarget = false;
            }
        }
    }

    void HandleTimeOut()
    {
        if (!hasTarget)
        {
            currentTimeOutTime = 0f;
            return;
        }

        if (Vector3.Distance(tr.position, lastPosition) > timeOutDistanceThreshold)
        {
            currentTimeOutTime = 0f;
            lastPosition = tr.position;
        }
        else
        {
            currentTimeOutTime += Time.deltaTime;

            if (currentTimeOutTime >= timeOutTime)
            {
                hasTarget = false;
            }
        }
    }

    // --- MÉTODOS ATUALIZADOS PARA O UNITY INPUT SYSTEM ---

    // Obtém a posição atual do mouse na tela;
    protected Vector2 GetMousePosition()
    {
        if (Mouse.current == null) return Vector2.zero;
        return Mouse.current.position.ReadValue();
    }

    // Verifica se o botão do mouse está pressionado (Hold);
    protected bool IsMouseButtonPressed()
    {
        if (Mouse.current == null) return false;

        if (moveButton == MouseButtonSelection.Left)
            return Mouse.current.leftButton.isPressed;
        else
            return Mouse.current.rightButton.isPressed;
    }

    // Verifica se o botão do mouse foi pressionado neste frame (Click);
    protected bool WasMouseButtonJustPressed()
    {
        if (Mouse.current == null) return false;

        if (moveButton == MouseButtonSelection.Left)
            return Mouse.current.leftButton.wasPressedThisFrame;
        else
            return Mouse.current.rightButton.wasPressedThisFrame;
    }

    // -----------------------------------------------------

    public override bool IsGrounded()
    {
        return isGrounded;
    }

    public override Vector3 GetMovementVelocity()
    {
        return lastMovementVelocity;
    }

    public override Vector3 GetVelocity()
    {
        return lastVelocity;
    }
}