using System;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
   
    public Player player;

    void Start()
    {
        YenidenBaslatLevel();
    }
    private void Update()
    {
        
    }

    public void YenidenBaslatLevel()
    {
        
        player.RestartPlayer();
    }

    

    


}
