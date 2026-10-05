using System.Collections.Generic;

public interface ServerMethods
{


    //Turn Methods
    public void Turn_Actor_Add(ITurnActor player);
    public void Turn_Actor_Remove(ITurnActor player);
    public void Turn_Player_Add(ITurnPlayer player);
    public void Turn_Player_PerformAction(TurnActionInstance actInstace);
    public void Turn_Player_PerformAcknowledge(TurnActionInstance actInstace);
    public void Turn_Player_PerformReaction(TurnActionInstance actInstace);
    public void Turn_Player_PerformResolve(TurnActionInstance actInstace);
    public void Turn_Player_UpdatePlayerActionControls(ITurnPlayer player);
    public void Turn_Player_UpdatePlayerInfo(string playerToken);
    public void Turn_Player_DrawCard(string playerToken);
    public void Turn_Player_RollResources(string playerToken);
    public void Turn_Next();
    public List<TurnActionInstance> Turn_Action_GetAll();
    public List<TurnActionInstance> Turn_Control_GetAll();

    //Grid Methods
    public GridServerInfo Grid_GetCurrentMap();


}