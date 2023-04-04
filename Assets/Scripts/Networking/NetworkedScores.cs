using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedScores : NetworkScript
{
    int score = 0;
    int scoreToAdd = 0;
    public int index = -1;
    string userName = "";
    //string username;
    public override void setData(object d)
    {
        if(!(d is int))
        {
            return;

        }

        index = (int)d;
        return;
        if (!(d is string))
        {
            return;
        }

        //username = (string)d;
    }

    public override void onStart()
    {


        if (!isHost)
        {
            return;
        }
        RoundManager.instance.addPlayer(gameObject, NetworkManager.instance.username);
        RoundManager.instance.addScore(gameObject, 0);
    }

    public void roundEnd(object sender, RoundArgs e)
    {
        return;
        int addingScore = 0;
        if (GameManager.instance.isNetworked)
        {
            sendData();
            return;
        }
        setData(addingScore);
    }

    public int getIndex() { return index; }

    public int getScore()
    {
        return score;
    }

    public void resetScoreToAdd()
    {
        
        scoreToAdd = 0;
    }

    public void setUserName(string s)
    {
        userName = s;
    }

    public string getUserName()
    {
        return userName;
    }

    public void addScore(int i)
    {
        
        score += i;
    }

    protected override void applyData()
    {
        return;
        score += scoreToAdd;
        resetScoreToAdd();
    }

    protected override void sendData()
    {
        return;
        string data =  scoreToAdd.ToString() + "|" + gameObject.GetInstanceID();
        NetworkManager.instance.queueTCPInstruction(this, NetworkManager.instance.getInstruction(InstructionType.ADD_SCORE, data), false);//.sendUDPMessage();
        resetScoreToAdd();
    }
}
