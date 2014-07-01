using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Main : MonoBehaviour {

	// References to other scripts
	public Constants PIXE;						// PIXE definitions
	public JellyfishXMLgui PSML;				// PSML loader
	public Heap API;							// API function calls


	void Start () {
	
		/*
		 * TO RUN - Just uncomment the example you wish to run...
		 * */

		psmlExample ();
		//example ();
	}
	/*
	 * Example parses and loads up the psml file saved in the application package into memory. A default psml file
	 * must be included in the software build:
	 * 
	 * - PC & Mac standalone: The psml.xml file can be replaced at any time swapping the file in 
	 *   the application directory.
	 * - iOS: After the build the psml file can only be amended witihn the application.
	 * - Webplayer cannot save changes as it has no file access. However, future builds can potentially
	 *   retrieve xml files from the web.
	 * 
	 */
	private void psmlExample() {

		int iOceanIndex = PIXE.OCEAN_NEW;			
		int thisSession = PIXE.RESET;
		int iFlags = PIXE.PSML_UNSET_FLAG;
		
		// Initialise the session
		API.Initialise (ref thisSession, iOceanIndex, ref iFlags);	
	
		//Call LoadPsml()
		PSML.LoadPsml (ref thisSession);
		return;
	}

	/*
	 * Example demonstrates a basic write and read of the following XML tree:
	 * 
	 * <A>
	 * 		<B 
	 * 		att1 = ”Alex”
	 * 		att2 = "Michael"/>
	 *	</A>
	 *
	 * All information is printed to the Unity Editor debug console.
	 */
	private void example() {

		int iOceanIndex = PIXE.OCEAN_NEW;				// Create a new ocean		
		int thisSession = PIXE.RESET;
		int iFlags = PIXE.PSML_UNSET_FLAG;				// iFlags to hold privileges info in future release
		object oToRead;

		// (1) Initialise the session and declare the ocean
		API.Initialise (ref thisSession, iOceanIndex, ref iFlags);	
	
	
		// (2) Write the root element <A>
		iFlags = PIXE.PSML_WRITE_ELEMENT;
		API.Write(thisSession,"A",null,ref iFlags);

		// NOTE: No Move() required as cursor default start point is the root node.

		// (3) Write an element (<B>) into the current cusor position (<A>)
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			iFlags = PIXE.PSML_WRITE_ELEMENT;
			API.Write (thisSession, "B", null, ref iFlags);
		}

		// (4) Move cursor into <B>
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			API.Move (thisSession, "B", ref iFlags);
		}

		// (5) Write an attrbute into the current cursor location (<B>)
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
			API.Write (thisSession, "att1", "Alex", ref iFlags);
		}

		// (6) Write an attribute into <B> using an absolute path 
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
			API.Write (thisSession, "att2", "Michael", ref iFlags);
		}

		// (7) Attempt to read attributes 
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			// Read using relative
			iFlags = PIXE.PSML_READ_ATTRIBUTE;
			oToRead = API.Read (thisSession, "att2", ref iFlags);
			Debug.Log ("Test attribute read (relative path). Expecting 'Michael' = " + oToRead);
		}
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			// Read using full path
			oToRead = API.Read (thisSession, "psml://A/B/att1", ref iFlags);
			Debug.Log ("Test attribute read (full path). Expecting 'Alex' = " + oToRead);
		}

		// (8) Test if elements exists
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			// Read element that does exist
			iFlags = PIXE.PSML_READ_ELEMENT;
			oToRead = API.Read (thisSession, "psml://A/B", ref iFlags);
			Debug.Log ("Test existing element read (<B>). Expecting 'true' = " + oToRead);
		}
		// Read element that doesn't exist
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			iFlags = PIXE.PSML_READ_ELEMENT;
			oToRead = API.Read (thisSession, "C", ref iFlags);
			Debug.Log ("Test nonexistant element read (<C>). Expecting 'false' = " + oToRead);
		}

		// (9) Print out the first 50 rows of the Jellyfish Ocean in memory.
		Session session = API.sessionList [thisSession]; 
		List<Molecule> ocean = API.oceanList [session.Ocean];
		Molecule current;
		int i,length = ocean.Count;
		for (i=0; i<50; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}

		// (10) Release the session ready for the next user
		API.freeSession (ref thisSession);
		Debug.Log ("END");
		return;
	}
}
