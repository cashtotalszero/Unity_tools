using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Main : MonoBehaviour {

	// iFlag definitions (must be greater than 0)
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_WRITE_ELEMENT = 2;
	const int PIXE_PSML_READ_ATTRIBUTE = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;
	const int PIXE_PSML_UNSET_FLAG = 5;
	
	const int PIXE_OCEAN_HOME = 0;
	const int PIXE_RESET = -1;

	public JellyfishXMLgui PSML;
	public Heap API;

	// Use this for initialization
	void Start () {
	
		// NOTE: If iOceanIndex = PIXE_RESET, a new empty ocean is created
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;
		
		// Initialise the session and declare the ocean
		API.Initialise (ref thisSession, iOceanIndex, ref iFlags);	
		Session session = API.sessionList [thisSession]; 
		List<Molecule> ocean = API.oceanList [session.Ocean];

		PSML.Load ();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGui()
	{
		GUI.Box(new Rect(10,10,100,90), "Hello?");
		
		if (PSML.success) {
			GUI.Label (new Rect (10, 10, Screen.width, Screen.height),
			           "SUCCESS :)");
		} 
		else {
			GUI.Label (new Rect (10, 10, Screen.width, Screen.height),
			           "FAIL :(");
		}
	}
}
