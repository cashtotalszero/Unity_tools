/*
 * This script creates a basic draggable jellyfish for both desktop and touchscreen devices.
 * To run:
 * 1) Create a new Unity project
 * 2) Add an empty game object to the scene.
 * 3) Add this script as a component to the empty game object.
 * 
 * Notes:
 * - Tested OK on Mac stand alone, IOS, Android, Safari (Mac) and Mozilla (Mac) web browers.
 * - Does not run on Mac Chrome as scripts are not supported.
 * - Windows 8 Phone version requires debugging.
 * */

using UnityEngine;
using System.Collections;

public class MoverTouch : MonoBehaviour {
	
	Rect jellyfish = new Rect(10, 10, 120, 100);	// Holds jellyfish dimensions/start position
	bool dragActive = false;						// Bool to allow draggin of jellyfish
	bool touchScreen = false;						// Bool for use on touchscreen devices	

	// This is called every frame - creates the draggable jellyfish on the GUI
	void OnGUI()
	{
		// For touch screens
		if (Input.touchCount > 0) {
			Touch t = Input.GetTouch (0);
			touchScreen = true;

			// Activate drag is jellyfish is being touched
			if (jellyfish.Contains (Event.current.mousePosition)) {
				if (Event.current.type == EventType.MouseDown) {
					dragActive = true;
				}
			}
			// De-activate when touch ends
			if (t.phase == TouchPhase.Ended) {
				dragActive = false;
			}
		}

		// For desktops - activate drag when mouseover jellyfish with mouse button pressed 
		else if (jellyfish.Contains (Event.current.mousePosition)) {
				if (Event.current.type == EventType.MouseDown) {
					dragActive = true;
				}	
				if (Event.current.type == EventType.MouseUp) {
					dragActive = false;
				}
			}

		// Change the jellyfish co-ordinates to the current mouse/touch position
		if (dragActive && Event.current.type == EventType.MouseDrag) {
			jellyfish.x += Event.current.delta.x;
			// For touchscreens Y position needs to be inverted
			if (touchScreen) {
				jellyfish.y -= Event.current.delta.y;
			} 
			else {
				jellyfish.y += Event.current.delta.y;
			}
		}

		// Display jellyfish on GUI
		GUI.Box (jellyfish, "Draggable jellyfish");
	}
}