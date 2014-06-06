using UnityEngine;
using System.Collections;
using System;

public class MethodCalls : MonoBehaviour {

	const int PIXE_PSML_ELEMENT = 1;
	const int PIXE_PSML_ATTRIBUTE = 2;
	const int PIXE_ERROR = -1;
	const int MEMORY_ERROR = -2;
	const int PIXE_OCEAN_FREE = -1;
	const int PIXE_OCEAN_HOME = 0;			// The root node of the PIXE ocean
	const int PIXE_OCEAN_UNSET = 0;
	const int PIXE_OCEAN_DEFAULT_DROP = 1;

	const int NAME = 0;
	const int TYPE = 1;
	const int VALUE = 2;
	const int DATA = 3;
	//gsOcean
	private string[,] ocean;
	private long[,] sessions;

	void Start() {

		long thisSession;
		long anotherSession;

		thisSession = Initialise (1);
		anotherSession = Initialise (1);
		//Debug.Log ("Session num: " + thisSession);

		int i;
		Write (thisSession, "A", "A", PIXE_PSML_ELEMENT);
		Debug.Log ("ONE:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,NAME] +
			          " TYPE: " +ocean[i,TYPE] +
			          " VALUE: " +ocean[i,VALUE] +
			          " DATA: " +ocean[i,DATA]);
			
		}

		Write (thisSession, "A", "B", PIXE_PSML_ELEMENT);
		Debug.Log ("TWO:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,NAME] +
			          " TYPE: " +ocean[i,TYPE] +
			          " VALUE: " +ocean[i,VALUE] +
			          " DATA: " +ocean[i,DATA]);
			
		}

		Write (thisSession, "B", "C", PIXE_PSML_ELEMENT);
		Debug.Log ("THREE:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,NAME] +
			          " TYPE: " +ocean[i,TYPE] +
			          " VALUE: " +ocean[i,VALUE] +
			          " DATA: " +ocean[i,DATA]);
			
		}

		Write (thisSession, "B", "D", PIXE_PSML_ELEMENT);
		Debug.Log ("FOUR:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,NAME] +
			" TYPE: " +ocean[i,TYPE] +
			" VALUE: " +ocean[i,VALUE] +
			" DATA: " +ocean[i,DATA]);

		}

		/*
		ocean = insertOceanRow (1);
		Debug.Log ("UPDATED:");
		for (i=0; i<5; i++) {
			Debug.Log("NAME: " +ocean[i,NAME] +
			          " TYPE: " +ocean[i,TYPE] +
			          " VALUE: " +ocean[i,VALUE] +
			          " DATA: " +ocean[i,DATA]);
			
		}*/
	}

	public long Initialise(long lFlags) {

		// If ocean doesn't already exist create one with 100 rows to start with
		if (ocean == null) {
			ocean = createOcean(ocean);
		}
		// Likewise if sessions doesn't already exist create one with 50 spaces
		if (sessions == null) {
			sessions = createSessions(sessions);
		}

		// Step through session array and look for a free slot
		int i, length = sessions.GetLength(0);
		for (i=0; i<length; i++) {

			// If free slot is found, set it to home them return the index
			if(sessions[i,0] == PIXE_OCEAN_FREE) {
				sessions[i,0] = PIXE_OCEAN_HOME;
				/*
				 * SET PRIVLEDGES HERE - according to lFlags
				 * */
				return i;
			}
		}
		// Return ERROR code to show no spaces found if end of array is reached.
		return PIXE_ERROR;
	}

	public long Write(long lSession, string sPath, object oValue, long lFlags){

		// 1) Retrieve the session pointer from the session array
		long sessionPointer = sessions [lSession, 0];
		// CHECK PRIVILEDGES HERE - do they have write access? - return error if not

		// Remove any leading/trailing spaces from the path
		sPath = sPath.Trim ();

		// Save the initial session location in case of error
		long sessionReset = sessionPointer;

		// 2) Syntax check for provided sPath (ABSOLUTE)
		if (sPath.StartsWith ("psml://")) {

			// Remove the "psml//:" marker
			sPath = sPath.Remove(0,7);
			Debug.Log(sPath);
			// NOTE - trailing spaces and the final / may need to be trimmed also

			// Split the string at each "/" and place tokenised strings into an array
			string[] splitPath = sPath.Split ('/');

			foreach (string tag in splitPath) {
				/*
				 * sessionPointer = NAVIGATE TO CORRECT HEADER TAG IN OCEAN
				 * 
				 * if NOT FOUND && NOT AT END OF LOOP - return ERROR
				 *
				 * */
			}
		} 

		// 3) Syntax check for provided sPath (RELATIVE)

		// WRITING ELEMENTS
		if (lFlags == PIXE_PSML_ELEMENT) {

			long elementSize = 0;				// Size of the current drop
			long currentHeader = 0;				// Index of the current Drop header
			long elementOrigin;					// Index of nested element declaration
			
			// If session pointer  is on a free slot, write element into it (only used once for empty oceans)
			// PIXE_OCEAN_POOL_TYPE, PIXE_OCEAN_SESSION_...
			if (ocean [sessionPointer, TYPE] == "") {
				ocean [sessionPointer, NAME] = oValue.ToString ();
				ocean [sessionPointer, TYPE] = "H";
				ocean [sessionPointer, VALUE] = "1";
				ocean [sessionPointer, DATA] = currentHeader.ToString();
				// Note: Session pointer does not move in this case
				return lFlags;
			}

			// Find the current Drop Header location
			currentHeader = sessionPointer;
			while (ocean[currentHeader,TYPE] != "H") {
				currentHeader--;
			}

			// If SP is on an Attribute, move it to the Header:
			if(ocean[sessionPointer,TYPE] == "A") {
				sessionPointer = currentHeader;
			}

			// If SP is on an Element, create a Header for the Element (if it doesn't already exist)
			if(ocean[sessionPointer,TYPE] == "E") {

				// Check path is valid - must reference same Element as SP or its header
				if(sPath != ocean[sessionPointer,NAME] && sPath != ocean[currentHeader,NAME]) {
					Debug.Log("ERROR = Invalid Path provided. Element does not exist in current location.");
					return lFlags;
				}

				// To add elements to NESTED elements:
				if(sPath == ocean[sessionPointer,NAME]) {

					// Create a Header for the nested element if needed
					if(ocean [sessionPointer,VALUE] == PIXE_OCEAN_UNSET.ToString ()) {

						// 1) Save the nested Element reference orgin & find somewhere to put the new Header
						elementOrigin = sessionPointer;
						sessionPointer = getDrop(sessionPointer);

						// 2) Write the Header data (including the offset to its parent)
						ocean[sessionPointer,NAME] = ocean[elementOrigin,NAME];
						ocean [sessionPointer, TYPE] = "H";
						ocean [sessionPointer, VALUE] = "1";
						ocean[sessionPointer,DATA] = (currentHeader - sessionPointer).ToString ();

						// 3) Update the offset value in the parent element molecule
						ocean [elementOrigin,VALUE] = (sessionPointer - elementOrigin).ToString();

						// 4) Move current header to newlt created header
						currentHeader = sessionPointer;
					}
					// Else, move the SP to the correct header
					else {
						sessionPointer = sessionPointer + Convert.ToInt64(ocean [sessionPointer,VALUE]);
					}
			
				}
			}
			// Get the current Drop size...
			elementSize = Convert.ToInt64 (ocean [currentHeader, VALUE]);

			// ...then add the nested element details to the end of the current Drop
			ocean [(currentHeader + elementSize), NAME] = oValue.ToString ();
			ocean [(currentHeader + elementSize), TYPE] = "E";
			ocean [(currentHeader + elementSize), VALUE] = PIXE_OCEAN_UNSET.ToString ();

			// Move the session point to the newly written record
			sessionPointer = currentHeader + elementSize;

			// Increase the size of the current Drop to reflect the new addtion
			long update = Convert.ToInt64 (ocean [currentHeader, VALUE]);
			update = update +1;
			ocean [currentHeader, VALUE] = update.ToString();

			// Save the position of the SP
			sessions [lSession, 0] = sessionPointer;
		}

			/*
	
			long i;						// counter
			long writeTo = 0;			// Index of cell to write new element to
		
			// Ensure the nested element doesn't already exist
			for(i=1;i<elementSize;i++) {
				if(ocean[sessionPointer + i,TYPE] == "E") 
				{
					if(ocean[sessionPointer+i,NAME] == oValue.ToString())
					{
						Debug.Log("Nested element '" + oValue.ToString() + "' already exists.");
						return sessionReset;
					}
				}
			}

			// 1) Add the nested element details to the end of element

			// First Ensure there is free space at end of element - if not, insert a free row 
			if(ocean[(sessionPointer + elementSize),NAME] != "") {
				ocean = insertOceanRow((sessionPointer + elementSize));
			}
			// Write the nested element Name and Type to this row
			ocean[(sessionPointer + elementSize),NAME]	= oValue.ToString();
			ocean[(sessionPointer + elementSize),TYPE]	= "E";

				// 2) Search for some free space to store the new header
				if(ocean[writeTo,VALUE] != "") {
					while(writeTo<ocean.GetLength(0)) {
						// Break out of loop when a free slot is found
						if (ocean[writeTo,NAME] == "") {
							break;
						}
						else {
							writeTo++;
						}
					}
				}

				// 3) Update the nested element details to include offset to its header
				offsetToHeader = writeTo - (sessionPointer + elementSize);
				ocean[(sessionPointer + elementSize),VALUE]	= offsetToHeader.ToString();
					
				// 4) Update the element size in the header to include the new record
				ocean[sessionPointer,VALUE]	= (elementSize + 1).ToString();

				// 5) Get the offset to parent value for the new location
				offsetToParent = (sessionPointer - writeTo);

				// 6) Move the session pointer to the new location & write new header
				sessions[sessionPointer,0] = writeTo;
				ocean[sessionPointer,NAME]	= oValue.ToString();
				ocean[sessionPointer,TYPE]	= "H";
				ocean[sessionPointer,VALUE]	= "1";
				ocean[sessionPointer,DATA]	= offsetToParent.ToString();

		}
		else if(lFlags == ATTRIBUTE) {

				// WRITE ATTRIBUTE

		}

			/*
			 * CHECK FLAGS - attribute or element?
			 * 
			 * SEARCH CURRENT HEADER FOR NAMED ATT OR ELEMENT:
			 * 
			 * if (FOUND) {
			 * 			OVERWRITE
			 * 		}
			 * 		else {
			 * 			CREATE NEW
			 * 			- CHECK FOR FREE SPACE IN OCEAN
			 * 			- IF SPACE WRITE IT IN
			 * 			- IF NOT, COPY INTO A NEW OCEAN WHERE MOVING ITEMS APPROPRIATELY
			 * 			- NOTE OFFSETS etc will need to be updated accordingly
			 * 		}
			 * 
			 * 
			 * */

		
	
		// Return success code 
		//Debug.Log ("Amended session = " + sessionPointer);
		return lFlags;
	}
	
	private string[,] createOcean(string[,] ocean)
	{
		ocean = new string[100, 4];

		// Initialise all cells to FREE ("")
		int i,j;
		int x=ocean.GetLength(0),y=ocean.GetLength(1);
	    
		for(i=0;i<x;i++) {
			for(j=0;j<y;j++) {
				ocean[i,j] = "";
			}
		}
		return ocean;
	}

	private long[,] createSessions(long[,] sessions)
	{
		sessions = new long[50, 2];
		
		// Initialise all cells to FREE
		int i,j;
		int x=sessions.GetLength(0),y=sessions.GetLength(1);
		
		for(i=0;i<x;i++) {
			for(j=0;j<y;j++) {
				sessions[i,j] = PIXE_OCEAN_FREE;
			}
		}
		return sessions;
	}


	// Finds a block of free space to store a new drop
	private long getDrop(long searchIndex) {

		long sizeX = ocean.GetLength (0);
		long i;

		for (i=searchIndex; i<sizeX-1; i++) {
			if(ocean[i,0] == "") {
			   break;
			}
		}
		return /*i*/5;
	}



	private string[,] insertOceanRow(long rowNum)
	{
		long sizeX = ocean.GetLength (0);
		long sizeY = ocean.GetLength (1);
		string[,] newOcean = new string[sizeX,sizeY];
		long i,j;

		// 1) Ensure that there is a free space at the end of the ocean to move everything into
		if (ocean [sizeX-1, 0] != "") {
			// EXPAND THE OCEAN IF NO FREE AT END
		}

		// 2 Copy all rows before the row to insert
		for (i=0; i<rowNum; i++) {
			for (j=0; j<sizeY; j ++) {
				newOcean [i, j] = ocean [i, j];
			}
		}

		// 3) Set the new free row
		for (j=0; j<sizeY; j++) {
			newOcean[rowNum,j] = "";
		}

		// 4) Copy all rows after the inserted row
		for (i=rowNum; i<sizeX-1; i++) {
			for (j=0; j<sizeY; j++) {
				newOcean [i+1, j] = ocean [i, j];
				// REMEMBER TO UPDATE OFFSETS
			}
		}
	    // 5) Return a copy of the new ocean
		return newOcean;
	}
}
