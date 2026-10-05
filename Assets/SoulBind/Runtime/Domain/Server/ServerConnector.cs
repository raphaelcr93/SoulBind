using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{

    //We are considering we will only have one instance per game.
    public static ServerConnector Instance { get; private set; }

    enum Mode
    {
        Local,
        API
    }

    Mode serverMode;
    public ServerMethods serverMethods;

    void InitializeAttributes()
    {
        serverMode = Mode.Local;
        initializeServerMethodsInstance();
    }

    void initializeServerMethodsInstance()
    {
        switch (serverMode)
        {
            case Mode.Local:
                serverMethods = new ServerMethods_Local();
                break;
            case Mode.API:
                throw new NotImplementedException();
                //break;
        }
    }

    /*****************************/
    /****MonoBehaviour Methods****/
    /*****************************/
    #region MonoBehaviour Methods

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeAttributes();
            Debug.Log($"Setting Instance to {this}");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    #endregion
}



