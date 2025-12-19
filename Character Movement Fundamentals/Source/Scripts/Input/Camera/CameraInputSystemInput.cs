using UnityEngine;
using UnityEngine.InputSystem;

namespace CMF
{
    // Herda da mesma classe 'CameraInput'
    public class CameraInputSystemInput : CameraInput
    {
        [Header("Input Action Asset:")]
        [SerializeField]
        private InputActionAsset inputActions;

        [Header("Settings:")]
        // Copiamos as mesmas variáveis públicas do script antigo
        public bool invertHorizontalInput = false;
        public bool invertVerticalInput = false;
        public float mouseInputMultiplier = 0.01f;

        private InputAction lookAction;
        private const string actionMapName = "Player"; // Baseado no seu screenshot

        void Awake()
        {
            var playerActionMap = inputActions.FindActionMap(actionMapName);
            lookAction = playerActionMap.FindAction("Look");
        }

        void OnEnable()
        {
            inputActions.FindActionMap(actionMapName).Enable();
        }

        void OnDisable()
        {
            inputActions.FindActionMap(actionMapName).Disable();
        }

        // Implementação do "contrato" (agora com a lógica antiga)
        public override float GetHorizontalCameraInput()
        {
            // 1. Ler o valor X do Input System
            float _input = lookAction.ReadValue<Vector2>().x;

            // 2. Aplicar a correção de 'deltaTime' (igual ao script antigo)
            if (Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input /= Time.deltaTime;
                _input *= Time.timeScale;
            }
            else
                _input = 0f;

            // 3. Aplicar sensibilidade (igual ao script antigo)
            _input *= mouseInputMultiplier;

            // 4. Aplicar inversão (igual ao script antigo)
            if (invertHorizontalInput)
                _input *= -1f;

            return _input;
        }

        public override float GetVerticalCameraInput()
        {
            // 1. Ler o valor Y do Input System
            float _input = lookAction.ReadValue<Vector2>().y;

            // 2. Aplicar a correção de 'deltaTime' (igual ao script antigo)
            if (Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input /= Time.deltaTime;
                _input *= Time.timeScale;
            }
            else
                _input = 0f;

            // 3. Aplicar sensibilidade (igual ao script antigo)
            _input *= mouseInputMultiplier;

            // 4. Aplicar a inversão padrão (o '-' do script antigo)
            //    (O script antigo tinha '-Input.GetAxisRaw', 
            //     então replicamos isso)
            _input *= -1f;

            // 5. Aplicar a inversão opcional (igual ao script antigo)
            if (invertVerticalInput)
                _input *= -1f;

            return _input;
        }
    }
}