/*
 * This script demonstrates how to create basic draggable jellyfish on Open Mission Control.
 * 
 * To run this script:
 * 1) Create a new unity project and add the script to the Assets folder.
 * 2) Create an empty game object and attach the script.
 * */

using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour
{
	Rect jellyfish = new Rect(10, 10, 120, 100);		// Holds jellyfish dimensions/start position
	bool buttonPressed = false;							// Bool for mouse button pressed
	
	// This is called every frame - creates the draggable jellyfish on the GUI
	void OnGUI()
	{
		// Check whether the mouse button has been pressed
		if(jellyfish.Contains(Event.current.mousePosition))
		{
			if(Event.current.type == EventType.MouseDown)
			{
				buttonPressed = true;
			}
			
			if(Event.current.type == EventType.MouseUp)
			{
				buttonPressed = false;
			}
		}

		// Move the jellyfish to the current mouse position
		if(buttonPressed && Event.current.type == EventType.MouseDrag)
		{
			jellyfish.x += Event.current.delta.x;
			jellyfish.y += Event.current.delta.y;
		}

		GUI.Box (jellyfish, "Draggable jellyfish");
	}
}