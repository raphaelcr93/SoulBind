using System.Collections.Generic;
using ScriptableObjectArchitecture;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //We are considering we will only have one Manager per game.
    public static PlayerManager Instance { get; private set; }

    public static string playerToken;
    private Player _currentPlayer;
    public TurnPlayerActionControl currentPlayerActionControls { get; private set; }

    private Unit selectedUnit;


    /********************/
    /****Player Utils****/
    /********************/
    #region Player Utils

    void Player_Initialize(string token, string name, List<Card> deck)
    {
        Player newPlayer = new Player(token, name, 0f, deck);
        Player_SetCurrent(newPlayer);
        ServerConnector.Instance.serverMethods.Turn_Player_Add(_currentPlayer);
        Player_CheckPlayerInfo();
        Debug.Log("New player: " + _currentPlayer.playerToken);
        Debug.Log("Player Changed: " + _currentPlayer.playerToken);
    }

    void Player_SetCurrent(Player player) {
        _currentPlayer = player;
        playerToken = player.playerToken;
    }

    void Player_CheckPlayerInfo()
    {
        PlayerEventRaiser.Instance?.PlayerManager_OnCheckPlayerInfo?.Raise(_currentPlayer);
    }

    public int Player_GetResourceValue(ResourceUtils.ResourceTypes type) {
       return _currentPlayer.GetResourceValue(type);
    }

    public void Player_RemoveResourceValue(ResourceUtils.ResourceTypes type, int value) {
        _currentPlayer.RemoveResourcePoints(type, value);
    }
    
    #endregion

    /********************/
    /****Turn Methods****/
    /********************/
    #region Player's Turn Methods

    void Player_SelectedUnitActionsCalculate()
    {
        if (selectedUnit != null)
        {
            selectedUnit.ActionTiles_Calculate();
        }
    }

    void Player_Raise_ActionEvent(TurnActionInstance actionInstance, bool isReaction)
    {
        if (isReaction)
        {
            PlayerEventRaiser.Instance?.PlayerManager_OnPlayerReactionPerformed?.Raise(actionInstance);
        }
        else
        {
            PlayerEventRaiser.Instance?.PlayerManager_OnPlayerActionPerformed?.Raise(actionInstance);
        }
    }

    bool Player_OnAction_CouldPerform(TurnUtils.ActionType type)
    {
        bool canPerform = true;

        canPerform = canPerform && currentPlayerActionControls != null;

        canPerform = canPerform && currentPlayerActionControls.canPerformAction;

        canPerform = canPerform && selectedUnit != null;

        canPerform = canPerform && selectedUnit.Unit_IsFromPlayer(_currentPlayer.playerToken);

        canPerform = canPerform && currentPlayerActionControls.possibleActions.Exists(x => (TurnUtils.ActionType)x.type == type);

        return canPerform;
    }

    bool Player_OnAction_CanPerform(TurnUtils.ActionType type, GridPosition gridPosition)
    {
        bool canPerform = Player_OnAction_CouldPerform(type);

        switch (type)
        {
            case TurnUtils.ActionType.Move:
                canPerform = canPerform && (selectedUnit != null ? selectedUnit.ActionsTiles_Movement_Contains(gridPosition) : false);
                break;

            case TurnUtils.ActionType.Attack:
                canPerform = canPerform && (selectedUnit != null ? selectedUnit.ActionsTiles_Attack_Contains(gridPosition) : false);
                break;
        }

        return canPerform;
    }

    bool Player_OnReaction_CanPerform()
    {
        bool canPerform = true;
        if (currentPlayerActionControls == null)
            canPerform = false;

        if (!currentPlayerActionControls.canPerformReaction)
            canPerform = false;
        return canPerform;
    }

    //Acknowledges (When player receive an action change)

    void Player_PerformAcknowledges()
    {
        foreach (TurnActionAckInstance actAckInstance in currentPlayerActionControls.actionsToAcknowledge)
        {
            Player_PerformActionAcknowledge(actAckInstance);
        }
    }

    void Player_PerformActionAcknowledge(TurnActionAckInstance actToAck)
    {
        TurnActionExtrasAcknowledge ackExtras = new TurnActionExtrasAcknowledge(actToAck.actionToAckIndex);
        TurnControlAction ackAction = new TurnControlAction(TurnUtils.ControlActionType.Control_WaitingAcknowledge);
        TurnActionInstance actInstance = new TurnActionInstance(ackAction, _currentPlayer, ackExtras);
        PlayerEventRaiser.Instance?.PlayerManager_OnPlayerAcknowledgePerformed?.Raise(actInstance);
    }

    //Actions and Reactions (When the player select an action to do):

    void Player_OnAction_Wrapper(ITurnAction act)
    {
        TurnUtils.ActionType actType = (TurnUtils.ActionType)act.type;

        if (Player_OnReaction_CanPerform())
        {
            Player_OnReaction_Switch(act);
        }
        else if (Player_OnAction_CouldPerform(actType))
        {
            Player_OnAction_Switch(act);
        }

    }

    void Player_OnReaction_Switch(ITurnAction act)
    {
        TurnUtils.ActionType actType = (TurnUtils.ActionType)act.type;
        switch (actType)
        {
            case TurnUtils.ActionType.Pass:
                Player_OnAction_Pass(act, true);
                break;
        }

    }

    void Player_OnAction_Switch(ITurnAction act)
    {
        Player_SelectedUnitActionsCalculate();
        TurnUtils.ActionType actType = (TurnUtils.ActionType)act.type;
        switch (actType)
        {
            case TurnUtils.ActionType.Pass:
                Player_OnAction_Pass(act, false);
                break;
            case TurnUtils.ActionType.Attack:
                Player_OnAction_Attack(act);
                break;
            case TurnUtils.ActionType.Move:
                Player_OnAction_Move(act);
                break;
        }
    }

    void Player_OnAction_Attack(ITurnAction act)
    {
        selectedUnit.OnSelectedToAttack();
    }

    void Player_OnAction_Move(ITurnAction act)
    {
        selectedUnit.OnSelectedToMove();
    }

    void Player_OnAction_Pass(ITurnAction act, bool isReaction)
    {
        if (selectedUnit != null && selectedUnit.Unit_IsFromPlayer(_currentPlayer.playerToken))
            selectedUnit.OnIdle();

        TurnActionInstance actionInstance = new TurnActionInstance(act, _currentPlayer, null);
        Player_Raise_ActionEvent(actionInstance, isReaction);
    }

    //On click (When the player clicks the grid after selecting the action)
    void Player_OnGridTile_MouseUp_Wrapper(GridPosition position)
    {
        if(selectedUnit == null)
            return;

        //Selected unit if from player
        if (!selectedUnit.Unit_IsFromPlayer(_currentPlayer.playerToken))
            return;

        TurnUtils.ActionType? actionType = null;

        switch (selectedUnit.unitStatus)
        {
            case Unit.UnitStatus.SelectedToMove:
                actionType = TurnUtils.ActionType.Move;
                break;
            case Unit.UnitStatus.SelectedToAttack:
                actionType = TurnUtils.ActionType.Attack;
                break;
        }

        if (actionType != null)
        {
            Player_OnGridTile_MouseUp_Switch(position, (TurnUtils.ActionType)actionType);
        }
    }

    void Player_OnGridTile_MouseUp_Switch(GridPosition position, TurnUtils.ActionType actType)
    {
        if (Player_OnAction_CanPerform(actType, position))
        {
            TurnAction act = currentPlayerActionControls.possibleActions.Find(x => (TurnUtils.ActionType)x.type == actType);
            if (act != null)
            {
                switch (actType)
                {
                    case TurnUtils.ActionType.Attack:
                        Player_OnGridTile_MouseUp_Attack(position);
                        break;
                    case TurnUtils.ActionType.Move:
                        Player_OnGridTile_MouseUp_Move(position);
                        break;
                }

                TurnActionExtrasGridAction extras = new TurnActionExtrasGridAction(position);
                UnitServer unitServer = GridManager.Instance.GetUnitServerAtPosition(selectedUnit.Unit_GetPosition());
                TurnActionInstance actionInstance = new TurnActionInstance(act, unitServer, extras);
                Player_Raise_ActionEvent(actionInstance, false);
            }
        }
    }

    void Player_OnGridTile_MouseUp_Attack(GridPosition position)
    {
        GridManager.Instance.FixAttackArrowEndPosition(position);
        selectedUnit?.ActionsTiles_HideForTurn(position);
    }

    void Player_OnGridTile_MouseUp_Move(GridPosition position)
    {
        selectedUnit?.ActionsTiles_HideForTurn(position);
    }

    //Resolves (After all reactions are done):
    void Player_PerformResolve()
    {
        foreach (TurnActionInstance act in currentPlayerActionControls.actionsToResolve)
        {
            switch (act.action.type)
            {
                case TurnUtils.ActionType.Pass:
                    Player_PerformResolve_Pass(act);
                    break;
                case TurnUtils.ActionType.Move:
                    Player_PerformResolve_Move(act);
                    break;
                case TurnUtils.ActionType.Attack:
                    Player_PerformResolve_Attack(act);
                    break;
            }
        }

        TurnControlAction ackAction = new TurnControlAction(TurnUtils.ControlActionType.Control_WaitingResolve);
        TurnActionInstance actInstance = new TurnActionInstance(ackAction, _currentPlayer, null);
        PlayerEventRaiser.Instance?.PlayerManager_OnPlayerResolvePerformed?.Raise(actInstance);
    }

    void Player_PerformResolve_Pass(TurnActionInstance act)
    {
        Debug.Log(_currentPlayer.playerToken + " performed resolve for action Pass from player " + act.actor.playerToken);
    }

    void Player_PerformResolve_Move(TurnActionInstance act)
    {
        TurnActionExtrasGridAction extras = act.extras as TurnActionExtrasGridAction;
        UnitServer actor = act.actor as UnitServer;
        GridManager.Instance.GetUnitUIAtPosition(actor.unitPosition).IfSelectedToMove_MoveToPosition(extras.gridPosition);
        Debug.Log(_currentPlayer.playerToken + " performed resolve for action Move from player " + act.actor.playerToken);
    }

    void Player_PerformResolve_Attack(TurnActionInstance act)
    {
        TurnActionExtrasGridAction extras = act.extras as TurnActionExtrasGridAction;
        UnitServer actor = act.actor as UnitServer;
        GridManager.Instance.GetUnitUIAtPosition(actor.unitPosition).IfSelectedToAttack_Attack(extras.gridPosition);
        Debug.Log(_currentPlayer.playerToken + " performed resolve for action Attack from player " + act.actor.playerToken);
    }

    #endregion

    /********************/
    /****Deck Methods****/
    /********************/
    #region Deck Methods



    #endregion

    /**********************/
    /****Event Handlers****/
    /**********************/
    #region Event Handlers

    //TurnManager Events
    public void Handle_TurnManager_PlayerControlsUpdated(TurnPlayerActionControl controls)
    {
        currentPlayerActionControls = controls;
        if (currentPlayerActionControls.canPerformAcknowledge)
            Player_PerformAcknowledges();

        if (currentPlayerActionControls.canPerformResolveTurn)
            Player_PerformResolve();
    }

    public void Handle_TurnManager_PlayerInfoUpdated(ITurnActor player)
    {
        string oldToken = _currentPlayer.playerToken;
        Player_SetCurrent(player as Player);
        if (oldToken != _currentPlayer.playerToken)
        {
            Debug.Log("Player Changed: " + player.playerToken);
            Camera camera = Camera.main;
            Quaternion cameraRotation = camera.transform.rotation;
            cameraRotation.z = _currentPlayer.cameraZRotation;
            camera.transform.rotation = cameraRotation;
            Player_CheckPlayerInfo();
        }
    }

    public void Handle_TurnManager_OnTurnStarted(ITurnActor turnActor)
    {
        if (turnActor is UnitServer)
        {
            UnitServer unitServer = GridUtils.GetServerUnitFromITurnActor(turnActor);
            Unit unit = GridManager.Instance.GetUnitUIAtPosition(unitServer.unitPosition);
            selectedUnit = unit;
        }
        else
        {
            selectedUnit = null;
        }

        Player_CheckPlayerInfo();
    }

    public void Handle_TurnManager_OnTurnEnded(ITurnActor turnActor)
    {
        if (selectedUnit != null)
        {
            selectedUnit = null;
        }

        Player_CheckPlayerInfo();
    }

    //PlayerHudView Events
    public void Handle_PlayerHudView_OnButtonClicked(ITurnAction act)
    {
        Player_OnAction_Wrapper(act);
    }

    //GridTileUI Events
    public void Handle_GridTileUI_OnMouseUp(GridPosition position)
    {
        Player_OnGridTile_MouseUp_Wrapper(position);
    }

    public void Handle_GridTileUI_OnMouseEnter(GridPosition position)
    {
        //In case the player is moving the Unit we call the action on the Unit to draw the path arrow
        if (selectedUnit != null && selectedUnit.unitStatus == Unit.UnitStatus.SelectedToMove && Player_OnAction_CanPerform(TurnUtils.ActionType.Move, position))
        {
            selectedUnit.OnGridTile_OnMouseEnter(position);
        }
    }

    #endregion

    /*****************************/
    /****MonoBehaviour Methods****/
    /*****************************/
    #region MonoBehaviour Methods

    //Temp Singleton for testing, remove later
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            //Debug.Log($"Setting Instance to {this}");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //TO DO: Get deck from somewhere else
        List<Card> deck = new List<Card>();
        for (var i = 0; i < 10; ++i)
        {
            Card card = new Card("Card " + i, "");
            deck.Add(card);
        }
        Player_Initialize("Player1_Token", "Player 1", deck);
    }

    #endregion MonoBehaviour Methods
}
