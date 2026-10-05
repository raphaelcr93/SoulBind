using System;
using System.Collections.Generic;

public class ServerMethods_Local : ServerMethods
{
    private TurnServer _turnServer;
    private GridServer _gridServer;

    public ServerMethods_Local()
    {
        InitializeAttributes();
    }

    void InitializeAttributes()
    {
        _turnServer = new TurnServer();
        _gridServer = new GridServer();
    }

    /*************************/
    /****Interface Methods****/
    /*************************/
    #region Interface Methods

    public List<Card> GetPlayerHand(string playerToken)
    {
        throw new NotImplementedException();
    }

    //Turn
    public void Turn_Player_Add(ITurnPlayer player)
    {
        _turnServer.Player_Add(player);
    }

    public void Turn_Actor_Add(ITurnActor player)
    {
        _turnServer.Actor_Add(player);
    }

    public void Turn_Actor_Remove(ITurnActor player)
    {
        _turnServer.Actor_Remove(player);
    }

    public void Turn_Player_PerformAction(TurnActionInstance actInstace)
    {
        _turnServer.Player_PerformAction(actInstace);
    }

    public void Turn_Player_PerformAcknowledge(TurnActionInstance actInstace)
    {
        _turnServer.Player_PerformAcknowledge(actInstace);
    }

    public void Turn_Player_PerformReaction(TurnActionInstance actInstace)
    {
        _turnServer.Player_PerformReaction(actInstace);
    }

    public void Turn_Player_PerformResolve(TurnActionInstance actInstace)
    {
        _turnServer.Player_PerformResolve(actInstace);
    }

    public void Turn_Player_UpdatePlayerActionControls(ITurnPlayer player)
    {
        _turnServer.Player_UpdatePlayerActionControls(player);
    }

    public void Turn_Player_UpdatePlayerInfo(string playerToken)
    {
        _turnServer.Player_UpdatePlayerInfo(playerToken);
    }

    public void Turn_Next()
    {
        _turnServer.Turn_Next();
    }

    public List<TurnActionInstance> Turn_Action_GetAll()
    {
        return _turnServer.currentTurn.Action_GetAll(null, null);
    }

    public List<TurnActionInstance> Turn_Control_GetAll()
    {
        return _turnServer.currentTurn.Control_GetAll(null, null);
    }

    public void Turn_Player_DrawCard(string playerToken)
    {
        _turnServer.Player_DrawCard(playerToken);
    }

    public void Turn_Player_RollResources(string playerToken)
    {
        _turnServer.Player_RollResources(playerToken);
    }

    //Grid
    public GridServerInfo Grid_GetCurrentMap()
    {
       return _gridServer.GetGridInfo();
    }



    #endregion
}

