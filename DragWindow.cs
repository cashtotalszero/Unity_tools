using UnityEngine;
using System.Collections;

public class DragWindow : MonoBehaviour {

	public Rect windowRect = new Rect(20, 20, 120, 50);
	void OnGUI() {
		windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
	}
	void DoMyWindow(int windowID) {
		GUI.DragWindow(new Rect(0, 0, 10000, 20));
	}
}
