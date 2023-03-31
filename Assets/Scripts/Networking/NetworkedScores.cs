using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedScores : NetworkScript
{
    int score = 0;
    int scoreToAdd = 0;
    //string username;
    public override void setData(object d)
    {
        return;
        if (!(d is string))
        {
            return;
        }

        //username = (string)d;
    }

    public override void onStart()
    {
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

    public int getScore()
    {
        return score;
    }

    public void resetScoreToAdd()
    {
        return;
        scoreToAdd = 0;
    }

    public void addToScoreToAdd(int i)
    {
        return;
        scoreToAdd += i;
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
