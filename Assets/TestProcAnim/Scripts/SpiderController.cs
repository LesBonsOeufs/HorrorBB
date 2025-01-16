/* 
 * This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
 * Copyright (C) 2020 Philipp Schofield - All Rights Reserved
 */

using UnityEngine;
using UnityEngine.InputSystem;

/*
 * This class needs a reference to the Spider class and calls the walk and turn functions depending on player input.
 * So in essence, this class translates player input to spider movement. The input direction is relative to a camera and so a 
 * reference to one is needed.
 */

[DefaultExecutionOrder(-1)] // Make sure the players input movement is applied before the spider itself will do a ground check and possibly add gravity
public class SpiderController : MonoBehaviour {

    public Spider spider;

    void FixedUpdate() {
        //** Movement **//
        Vector3 input = getInput();

        if (Keyboard.current.leftShiftKey.isPressed) spider.run(input);
        else spider.walk(input);
        spider.turn(input);
    }

    void Update() {
        //Hold down Space to deactivate ground checking. The spider will fall while space is hold.
        spider.setGroundcheck(!Keyboard.current.spaceKey.isPressed);
    }

    private Vector3 getInput() {
        float lHorizontal = (Keyboard.current.dKey.isPressed ? 1 : 0) + (Keyboard.current.aKey.isPressed ? -1 : 0);
        float lVertical = (Keyboard.current.wKey.isPressed ? 1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0);

        Vector3 up = spider.transform.up;
        Vector3 right = spider.transform.right;
        Vector3 input = Vector3.ProjectOnPlane(Camera.main.transform.forward, up).normalized * lVertical
            + (Vector3.ProjectOnPlane(Camera.main.transform.right, up).normalized * lHorizontal);
        Quaternion fromTo = Quaternion.AngleAxis(Vector3.SignedAngle(up, spider.getGroundNormal(), right), right);
        input = fromTo * input;
        float magnitude = input.magnitude;
        return (magnitude <= 1) ? input : input /= magnitude;
    }
}