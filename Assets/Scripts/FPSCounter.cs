﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.Universal;

public class FPSCounter : MonoBehaviour
{
    public Text tx;

    //public Input_Manager ip;

    Player player;

    int framerate;

    int averageframerate;

    int counter;


    string resolution;
    string aspect;

    PixelPerfectCamera ppc;


    private void OnEnable()
    {
        ppc = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PixelPerfectCamera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //ip = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Input_Manager>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();


        framerate = (int)(1f / Time.unscaledDeltaTime);

        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 input = ip.readInput();
        
        int fixd = (int)(1f / Time.fixedDeltaTime);
        
        framerate = (int)(1f / Time.unscaledDeltaTime);

    }

    private void FixedUpdate()
    {
        counter++;
        if (counter >= 5)
        {
            resolution = "" + Screen.width.ToString() + " x " + Screen.height.ToString();
            aspect = ppc.pixelRatio.ToString() + " : 1";

            tx.text = framerate.ToString() + " FPS"; // + " | INPUT: "; // + input.x + ", " + input.y;
            tx.text = tx.text + "\n" + resolution;
            tx.text = tx.text + "\n" + aspect;
            counter = 0;
        }

        
    }


}
