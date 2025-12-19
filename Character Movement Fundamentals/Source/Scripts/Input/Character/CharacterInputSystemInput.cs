using UnityEngine;
using UnityEngine.InputSystem; // 1. Importar o novo Input System

namespace CMF
{
    // 2. Herdamos da mesma classe abstrata 'CharacterInput'
    public class CharacterInputSystemInput : CharacterInput
    {
        // 3. Crie um campo para arrastar seu Asset de Input Actions
        [SerializeField]
        private InputActionAsset inputActions;

        // Nossas actions (baseado na sua imagem)
        private InputAction moveAction;
        private InputAction jumpAction;

        // Nome do seu Action Map (baseado na sua imagem)
        private const string actionMapName = "Player";

        void Awake()
        {
            // Encontrar as actions dentro do Asset
            var playerActionMap = inputActions.FindActionMap(actionMapName);

            moveAction = playerActionMap.FindAction("Move");
            jumpAction = playerActionMap.FindAction("Jump");
        }

        void OnEnable()
        {
            // Ligar o Action Map
            inputActions.FindActionMap(actionMapName).Enable();
        }

        void OnDisable()
        {
            // Desligar o Action Map
            inputActions.FindActionMap(actionMapName).Disable();
        }

        // 4. Implementar os métodos obrigatórios do "contrato"

        public override float GetHorizontalMovementInput()
        {
            // Lemos o valor Vector2 da action "Move" e retornamos o X
            return moveAction.ReadValue<Vector2>().x;
        }

        public override float GetVerticalMovementInput()
        {
            // Lemos o valor Vector2 da action "Move" e retornamos o Y
            return moveAction.ReadValue<Vector2>().y;
        }

        public override bool IsJumpKeyPressed()
        {
            // Verificamos se o botão de "Jump" está pressionado
            // IsPressed() é o equivalente a GetKey()
            return jumpAction.IsPressed();
        }
    }
}