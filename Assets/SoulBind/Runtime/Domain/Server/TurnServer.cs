using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnServer
{
    //All possible players
    private List<ITurnPlayer> _turnPlayers;

    //All actors that will have a turn
    private List<ITurnActor> _turnActors;
    //Actors that didn't play this turn
    private Queue<ITurnActor> currentTurnActors;
    //Actors that already played
    private List<ITurnActor> playedTurnActors;

    private List<TurnActionChain> _pastTurns;

    public TurnActionChain currentTurn;

    private TurnAction[] _possibleActions;

    #region Initializion Methods

    public TurnServer()
    {
        InitializeAttributes();
    }

    void DefineActions()
    {
        _possibleActions = new TurnAction[TurnUtils.ActionType.GetNames(typeof(TurnUtils.ActionType)).Length];
        //Change to Scriptable objects later

        //_possibleActions[(int)TurnUtils.ActionType.Control_WaitingAction] = new TurnAction(TurnUtils.ActionType.Control_WaitingAction, false);
        //_possibleActions[(int)TurnUtils.ActionType.Control_WaitingAcknowledge] = new TurnAction(TurnUtils.ActionType.Control_WaitingAcknowledge, false);
        //_possibleActions[(int)TurnUtils.ActionType.Control_WaitingReaction] = new TurnAction(TurnUtils.ActionType.Control_WaitingReaction, false);
        //_possibleActions[(int)TurnUtils.ActionType.Control_Finish] = new TurnAction(TurnUtils.ActionType.Control_Finish, false);

        _possibleActions[(int)TurnUtils.ActionType.Pass] = new TurnAction(TurnUtils.ActionType.Pass, false, true, "Pass");
        _possibleActions[(int)TurnUtils.ActionType.InvokeCreature] = new TurnAction(TurnUtils.ActionType.InvokeCreature, false, false, "Invoke Creature");
        _possibleActions[(int)TurnUtils.ActionType.Move] = new TurnAction(TurnUtils.ActionType.Move, true, true, "Move");
        _possibleActions[(int)TurnUtils.ActionType.Attack] = new TurnAction(TurnUtils.ActionType.Attack, true, true, "Attack");
        _possibleActions[(int)TurnUtils.ActionType.SpecialAbility] = new TurnAction(TurnUtils.ActionType.Attack, true, true, "Special Ability");
    }

    void InitializeAttributes()
    {
        _turnActors = new List<ITurnActor>();
        _pastTurns = new List<TurnActionChain>();
        _turnPlayers = new List<ITurnPlayer>();
        playedTurnActors = new List<ITurnActor>();

        DefineActions();
    }

    #endregion

    public void Actor_Add(ITurnActor actor)
    {
        _turnActors.Add(actor);
    }

    public void Actor_Remove(ITurnActor actor)
    {
        _turnActors.Remove(actor);
    }

    private ITurnPlayer Turn_GetPlayerById(string playerIdentifier)
    {
        return _turnPlayers.Find(x => x.playerToken == playerIdentifier);
    }

    private TurnAction Turn_GetActionDefinition(TurnUtils.ActionType actionType)
    {
        return _possibleActions[(int)actionType];
    }

    private TurnActionInstance Turn_GetControl_Finish(ITurnActor player)
    {
        return new TurnActionInstance(new TurnControlAction(TurnUtils.ControlActionType.Control_Finish), player, null);
    }

    private TurnActionInstance Turn_GetControl_WaitingReaction(ITurnActor player)
    {
        return new TurnActionInstance(new TurnControlAction(TurnUtils.ControlActionType.Control_WaitingReaction), player, null);
    }

    private TurnActionInstance Turn_GetControl_WaitingAction(ITurnActor player)
    {
        return new TurnActionInstance(new TurnControlAction(TurnUtils.ControlActionType.Control_WaitingAction), player, null);
    }

    private TurnActionInstance Turn_GetControl_WaitingAcknowledge(ITurnActor player, int actionIndex)
    {
        TurnActionExtrasAcknowledge extras = new TurnActionExtrasAcknowledge(actionIndex);
        return new TurnActionInstance(new TurnControlAction(TurnUtils.ControlActionType.Control_WaitingAcknowledge), player, extras);
    }

    private TurnActionInstance Turn_GetControl_WaitingResolve(ITurnActor player)
    {
        return new TurnActionInstance(new TurnControlAction(TurnUtils.ControlActionType.Control_WaitingResolve), player, null);
    }

    private void Turn_Init_NewRound()
    {
        currentTurnActors = new Queue<ITurnActor>(_turnActors);
        //currentTurnActors.Concat(_turnActors);
        playedTurnActors.Clear();
        if (currentTurnActors.Count == 0)
            return;
        ITurnActor nextActor = currentTurnActors.Peek();
        if (nextActor != null)
        {
            Turn_Start(nextActor);
        }
    }

    private void Turn_Start(ITurnActor actor)
    {
        if (currentTurn != null)
        {
            _pastTurns.Add(currentTurn);
        }
        currentTurn = new TurnActionChain();
        TurnEventRaiser.Instance.TurnManager_ActionChanged?.Raise();
        Turn_AddControl(Turn_GetControl_WaitingAction(actor));
        actor.OnTurnStart();
        TurnEventRaiser.Instance.TurnManager_OnTurnStarted?.Raise(actor);
    }

    private void Turn_End(ITurnActor actor)
    {
        playedTurnActors.Add(actor);
        actor.OnTurnEnd();
        TurnEventRaiser.Instance.TurnManager_OnTurnEnded?.Raise(actor);

    }

    public void Turn_Next()
    {
        if (currentTurn != null)
        {
            //Check if turn is finished
            if (currentTurn.Control_Check(null, TurnUtils.ControlActionType.Control_Finish, true) != null)
            {

                ITurnActor lastActor = currentTurnActors.Peek();

                if (lastActor != null)
                {
                    Turn_End(currentTurnActors.Dequeue());
                }

                ITurnActor nextActor;
                currentTurnActors.TryPeek(out nextActor);
                //Check if there is a next
                if (nextActor != null)
                {
                    Turn_Start(nextActor);
                }
                //Start a new round
                else
                {
                    Turn_Init_NewRound();
                }
            }

        }
        else
        {
            Turn_Init_NewRound();
        }
    }

    private void Turn_AddAction(TurnActionInstance action)
    {
        if (currentTurn == null)
        {
            return;
        }

        currentTurn.Action_Add(action);
        TurnEventRaiser.Instance.TurnManager_ActionChanged?.Raise();
    }

    private void Turn_RemoveAction(TurnActionInstance action)
    {
        if (currentTurn == null)
        {
            return;
        }

        currentTurn.Action_Remove(action);
        TurnEventRaiser.Instance.TurnManager_ActionChanged?.Raise();
    }

    private void Turn_AddControl(TurnActionInstance action)
    {
        if (currentTurn == null)
        {
            return;
        }

        currentTurn.Control_Add(action);
        TurnEventRaiser.Instance.TurnManager_ControlChanged?.Raise();
    }

    private void Turn_RemoveControl(TurnActionInstance action)
    {
        if (currentTurn == null)
        {
            return;
        }

        currentTurn.Control_Remove(action);
        TurnEventRaiser.Instance.TurnManager_ControlChanged?.Raise();
    }

    private TurnActionInstance Player_CanPerform(TurnUtils.ControlActionType type, ITurnPlayer player)
    {
        if (player == null || currentTurn == null)
        {
            return null;
        }
        return currentTurn.Control_Check(player.playerToken, type, true);
    }

    private TurnActionInstance Player_NeedAcknowledge(ITurnPlayer player, int? actionToAckIndex)
    {
        if (player == null || currentTurn == null)
        {
            return null;
        }
        if (actionToAckIndex == null)
        {
            return currentTurn.Control_Check(player.playerToken, TurnUtils.ControlActionType.Control_WaitingAcknowledge, false);
        }
        else
        {
            return currentTurn.Control_Ack_Check(player.playerToken, (int)actionToAckIndex);
        }
    }

    private TurnActionInstance Player_NeedResolve(ITurnPlayer player)
    {
        if (player == null || currentTurn == null)
        {
            return null;
        }

        return currentTurn.Control_Check(player.playerToken, TurnUtils.ControlActionType.Control_WaitingResolve, false);
    }

    private void Player_Perform(TurnUtils.ControlActionType type, TurnActionInstance actionToPerform)
    {
        //The actor of the action can be the unit as well, so we make sure to always get the player object
        ITurnPlayer player = Turn_GetPlayerById(actionToPerform.actor.playerToken);

        //Check if player exist
        if (player == null)
        {
            return;
        }

        TurnActionInstance controlAction = Player_CanPerform(type, player);

        //Check if player can perform the action
        if (controlAction == null)
        {
            return;
        }

        Turn_RemoveControl(controlAction);
        Debug.Log(player.playerToken + " action performed " + actionToPerform.action.type);

        //Add the new action to the turn
        Turn_AddAction(actionToPerform);

        int actionIndex = currentTurn.Action_GetLastIndex();

        foreach (ITurnPlayer turnPlayer in _turnPlayers)
        {
            Turn_AddControl(Turn_GetControl_WaitingAcknowledge(turnPlayer, actionIndex));
        }

        //TurnManager_ActionChanged?.Raise();
        Player_UpdatePlayerActionControls(player);
    }

    public void Player_PerformAction(TurnActionInstance actionToPerform)
    {
        Player_Perform(TurnUtils.ControlActionType.Control_WaitingAction, actionToPerform);
    }

    public void Player_PerformReaction(TurnActionInstance actionToPerform)
    {
        Player_Perform(TurnUtils.ControlActionType.Control_WaitingReaction, actionToPerform);
    }

    public void Player_PerformAcknowledge(TurnActionInstance actInstace)
    {
        ITurnPlayer player = Turn_GetPlayerById(actInstace.actor.playerToken);

        //Check if player exist
        if (player == null)
        {
            return;
        }

        TurnActionInstance controlAction = Player_NeedAcknowledge(player, ((TurnActionExtrasAcknowledge)actInstace.extras).actionIndex);

        //Check if player can perform the action
        if (controlAction == null)
        {
            return;
        }

        Turn_RemoveControl(controlAction);
        Debug.Log(actInstace.actor.playerToken + " ack performed");

        //Can go to next action?
        if (currentTurn.Control_Check(null, TurnUtils.ControlActionType.Control_WaitingAcknowledge, false) == null)
        {
            //All acknowledges are done
            TurnActionExtrasAcknowledge ackExtras = controlAction.extras as TurnActionExtrasAcknowledge;
            TurnActionInstance actionToAck = currentTurn.Action_GetIndex(ackExtras.actionIndex);

            if (Turn_GetActionDefinition((TurnUtils.ActionType)actionToAck.action.type).isReactable)
            {
                foreach (ITurnPlayer enemyPlayer in _turnPlayers)
                {
                    if (enemyPlayer.playerToken != actionToAck.actor.playerToken)
                    {
                        //If enemy doesn't have a reaction in the chain, add one to allow him to react.
                        if (currentTurn.Control_Check(enemyPlayer.playerToken, TurnUtils.ControlActionType.Control_WaitingReaction, false) == null)
                        {
                            Turn_AddControl(Turn_GetControl_WaitingReaction(enemyPlayer));
                        }
                    }
                }
            }
            else
            {
                foreach (ITurnPlayer turnPlayer in _turnPlayers)
                {
                    //If all the players acknowledge, add the resolve;
                    Turn_AddControl(Turn_GetControl_WaitingResolve(turnPlayer));
                }
            }
        }

        Player_UpdatePlayerActionControls(player);

        //TurnManager_ActionChanged?.Raise();

    }

    public void Player_PerformResolve(TurnActionInstance actInstace)
    {
        ITurnPlayer player = Turn_GetPlayerById(actInstace.actor.playerToken);

        //Check if player exist
        if (player == null)
        {
            return;
        }

        TurnActionInstance controlAction = Player_NeedResolve(player);

        //Check if player can perform the action
        if (controlAction == null)
        {
            return;
        }

        Turn_RemoveControl(controlAction);
        Debug.Log(actInstace.actor.playerToken + " resolve performed");

        //Can go to next action?
        if (currentTurn.Control_Check(null, TurnUtils.ControlActionType.Control_WaitingResolve, false) == null)
        {
            Turn_AddControl(Turn_GetControl_Finish(null));
        }

        Player_UpdatePlayerActionControls(player);

        //TurnManager_ActionChanged?.Raise();

    }

    public void Player_Add(ITurnPlayer player)
    {
        _turnPlayers.Add(player);
    }

    public void Player_UpdatePlayerActionControls(ITurnPlayer player)
    {
        TurnPlayerActionControl controls = new TurnPlayerActionControl();
        controls.player = player;
        controls.canPerformAcknowledge = Player_NeedAcknowledge(player, null) != null;
        controls.canPerformAction = Player_CanPerform(TurnUtils.ControlActionType.Control_WaitingAction, player) != null;
        controls.canPerformReaction = Player_CanPerform(TurnUtils.ControlActionType.Control_WaitingReaction, player) != null;
        controls.canPerformResolveTurn = Player_NeedResolve(player) != null;

        controls.actionsToAcknowledge = new List<TurnActionAckInstance>();
        controls.actionsToResolve = new Stack<TurnActionInstance>();

        //For now always allow everything
        controls.possibleActions = _possibleActions.ToList().FindAll(x => x.showInActionMenu);

        if (controls.canPerformAcknowledge)
        {
            List<TurnActionInstance> ackControlActions = currentTurn.Control_GetAll(player.playerToken, TurnUtils.ControlActionType.Control_WaitingAcknowledge);
            foreach (TurnActionInstance ackInstance in ackControlActions)
            {
                TurnActionExtrasAcknowledge ackExtras = ackInstance.extras as TurnActionExtrasAcknowledge;
                TurnActionAckInstance actionToAck = new TurnActionAckInstance(currentTurn.Action_GetIndex(ackExtras.actionIndex), ackExtras.actionIndex);
                controls.actionsToAcknowledge.Add(actionToAck);
            }
        }

        if (controls.canPerformResolveTurn)
        {
            controls.actionsToResolve = new Stack<TurnActionInstance>(currentTurn.Action_GetAll("", null));
        }

        TurnEventRaiser.Instance.TurnManager_PlayerControlsUpdated?.Raise(controls);
    }

    private ITurnPlayer Player_GetByToken(string playerToken)
    {
        return _turnPlayers.Find(x => x.playerToken == playerToken);
    }

    public void Player_UpdatePlayerInfo(string playerToken)
    {
        ITurnPlayer player = Player_GetByToken(playerToken);
        if (player != null)
        {
            TurnEventRaiser.Instance.TurnManager_PlayerInfoUpdated?.Raise(player);
        }
    }

    public void Player_DrawCard(string playerToken)
    {
        Player player = Player_GetByToken(playerToken) as Player;
        if (player != null)
        {
            player.DrawCard();
            TurnEventRaiser.Instance.TurnManager_PlayerInfoUpdated?.Raise(player);
        }
    }

    public void Player_RollResources(string playerToken)
    { 
        Player player = Player_GetByToken(playerToken) as Player;
        if (player != null)
        {
            //For now we are not checking nothing and not rolling resources based on dices
            player.AddResourcePoints(ResourceUtils.ResourceTypes.MovePoints, 5);
            player.AddResourcePoints(ResourceUtils.ResourceTypes.AbilityPoints, 1);
            player.AddResourcePoints(ResourceUtils.ResourceTypes.AttackPoints, 2);
            player.AddResourcePoints(ResourceUtils.ResourceTypes.MagicPoints, 3);
            player.AddResourcePoints(ResourceUtils.ResourceTypes.InvocationPonints, 4);
            TurnEventRaiser.Instance.TurnManager_PlayerInfoUpdated?.Raise(player);
        }
    }
}

public interface ITurnActor
{
    string playerToken { get; }
    public abstract void OnTurnStart();
    public abstract void OnTurnEnd();
}

public interface ITurnPlayer : ITurnActor
{
    public string playerName { get; }
}

public class TurnPlayerActionControl
{
    public ITurnPlayer player;
    public bool canPerformAction;
    public bool canPerformReaction;
    public bool canPerformAcknowledge;
    public bool canPerformResolveTurn;
    public List<TurnAction> possibleActions;
    public List<TurnActionAckInstance> actionsToAcknowledge;
    public Stack<TurnActionInstance> actionsToResolve;
}

public class TurnActionAckInstance
{
    public TurnActionInstance actionToAck { get; private set; }
    public int actionToAckIndex { get; private set; }

    public TurnActionAckInstance(TurnActionInstance actionToAck, int actionToAckIndex)
    {
        this.actionToAck = actionToAck;
        this.actionToAckIndex = actionToAckIndex;
    }
}

//A list of actions, that represent a full turn
public class TurnActionChain
{
    private List<TurnActionInstance> _actions;
    private List<TurnActionInstance> _controls;

    public TurnActionChain()
    {
        _actions = new List<TurnActionInstance>();
        _controls = new List<TurnActionInstance>();
    }

    public void Action_Add(TurnActionInstance action)
    {
        _actions.Add(action);
    }

    public void Action_Remove(TurnActionInstance action)
    {
        _actions.Remove(action);
    }

    public TurnActionInstance Action_GetLast()
    {
        return _actions.Last();
    }

    public int Action_GetLastIndex()
    {
        return _actions.Count - 1;
    }

    public bool Action_isEmpty()
    {
        return _actions.Count > 0;
    }

    public TurnActionInstance Action_GetIndex(int index)
    {
        return _actions.ElementAt(index);
    }

    public List<TurnActionInstance> Action_GetAll(string playerIdentifier, TurnUtils.ControlActionType? controlType)
    {
        return _actions.FindAll(x => (controlType == null || (TurnUtils.ControlActionType)x.action.type == controlType) && (playerIdentifier == null || playerIdentifier == "" || x.actor.playerToken == playerIdentifier));
    }

    public TurnActionInstance Control_Check(string playerIdentifier, TurnUtils.ControlActionType controlType, bool onlyLast)
    {
        if (onlyLast)
        {
            TurnActionInstance last = _controls.Last();

            if ((TurnUtils.ControlActionType)last.action.type == controlType && (playerIdentifier == null || playerIdentifier == "" || last.actor.playerToken == playerIdentifier))
            {
                return last;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return _controls.Find(x => (TurnUtils.ControlActionType)x.action.type == controlType && (playerIdentifier == null || playerIdentifier == "" || x.actor.playerToken == playerIdentifier));
        }
    }

    public TurnActionInstance Control_GetLast()
    {
        return _controls.Last();
    }

    public List<TurnActionInstance> Control_GetAll(string playerIdentifier, TurnUtils.ControlActionType? controlType)
    {
        return _controls.FindAll(x => (controlType == null || (TurnUtils.ControlActionType)x.action.type == controlType) && (playerIdentifier == null || playerIdentifier == "" || x.actor.playerToken == playerIdentifier));
    }

    public TurnActionInstance Control_Ack_Check(string playerIdentifier, int actionToAckIndex)
    {
        return _controls.Find(x => (TurnUtils.ControlActionType)x.action.type == TurnUtils.ControlActionType.Control_WaitingAcknowledge && x.actor.playerToken == playerIdentifier && ((TurnActionExtrasAcknowledge)x.extras).actionIndex == actionToAckIndex);
    }

    public void Control_Add(TurnActionInstance action)
    {
        _controls.Add(action);
    }

    public void Control_Remove(TurnActionInstance action)
    {
        _controls.Remove(action);
    }
}

//Instance of an action
public class TurnActionInstance
{
    public ITurnAction action { get; private set; }
    public ITurnActor actor { get; private set; }
    public ITurnActionExtras extras { get; private set; }

    public TurnActionInstance(ITurnAction action, ITurnActor actor, ITurnActionExtras extras)
    {
        this.action = action;
        this.actor = actor;
        this.extras = extras;
    }
}

//This class is the definition of the actions. We are using it just to create a list of possible actions
public class TurnAction : ITurnAction
{
    private TurnUtils.ActionType _type;
    public object type
    {
        get { return _type; }
    }

    public bool isReactable { get; private set; }
    public bool showInActionMenu { get; private set; }
    public string readableName { get; private set; }

    public TurnAction(TurnUtils.ActionType type, bool isReactable, bool showInActionMenu, string readableName)
    {
        _type = type;
        this.isReactable = isReactable;
        this.showInActionMenu = showInActionMenu;
        this.readableName = readableName;

    }
}

//This class is the definition of the actions. We are using it just to create a list of possible actions
public class TurnControlAction : ITurnAction
{
    private TurnUtils.ControlActionType _type;
    public object type
    {
        get { return _type; }
    }

    public TurnControlAction(TurnUtils.ControlActionType type)
    {
        _type = type;
    }
}

//Interface to be extended for extra arguments
public interface ITurnAction
{
    object type { get; }
}

//Interface to be extended for extra arguments
public interface ITurnActionExtras
{
}

public class TurnActionExtrasAcknowledge : ITurnActionExtras
{
    public int actionIndex { get; private set; }

    public TurnActionExtrasAcknowledge(int actionIndex)
    {
        this.actionIndex = actionIndex;
    }
}

public class TurnActionExtrasGridAction : ITurnActionExtras
{
    public GridPosition gridPosition { get; private set; }

    public TurnActionExtrasGridAction(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }
}

public class TurnUtils
{

    //Possible Types of control actions. This shouldn't change
    public enum ControlActionType
    {
        Control_WaitingAction,
        Control_WaitingAcknowledge,
        Control_WaitingReaction,
        Control_WaitingResolve,
        Control_Finish,
    }

    //Possible Types of actions. This can be extended. Possibly in the future it will be an ScriptableObject
    public enum ActionType
    {
        InvokeCreature,
        Move,
        Attack,
        SpecialAbility,
        Pass
    }
}