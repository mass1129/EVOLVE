using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// ���� ������ ���� Ŭ����
public class GM_StateMachine<T> where T : class
{
    private T ownerEntity; // StateMachine�� ������ (������Ʈ Ŭ����)
    private GM_State<T> currentState; // ���� �⺻ ����
    private GM_State<T> previousState; // ���� ����
    public GM_State<T> globalState; // ���� ����

    // StateMachine�� ���� Ŭ������ ����ϴ� ������Ʈ���� ȣ��. ������Ʈ�� ���� �ʱ�ȭ.
    public void SetUp(T owner, GM_State<T> entryState)
    {
        ownerEntity = owner;
        currentState = null;
        previousState = null;
        globalState = null;
        
        ChangeState(entryState);
    }

    public void Execute()
    {
        if(currentState != null)
        {
            currentState.Execute(ownerEntity);
        }

        if(globalState != null)
        {
            globalState.Execute(ownerEntity);
        }
    }

    public void ChangeState(GM_State<T> newState)
    {
        // ���� �ٲٷ��� ���°� ��������� ���� ���� X
        if (newState == null) return;

        if (currentState != null)
        {
            // ���� ����Ǹ� ���� ���°� ���� ���°� ��.
            previousState = currentState;
            currentState.Exit(ownerEntity);
        }

        // ���ο� ���·� �����ϰ�, ���� �ٲ� ������ Enter() �޼ҵ� ȣ��
        currentState = newState;
        currentState.Enter(ownerEntity);
    }

    public void SetGlobalState(GM_State<T> newState)
    {
        globalState = newState;
    }

    public void RevertToPreviousState()
    {
        ChangeState(previousState);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
