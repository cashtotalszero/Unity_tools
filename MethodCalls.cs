using UnityEngine;
using System.Collections;
using System;

public class MethodCalls : MonoBehaviour {

	const int ELEMENT = 1;
	const int ATTRIBUTE = 2;
	const int ERROR = -1;
	const int MEMORY_ERROR = -2;
	const int FREE = -1;
	const int HOME = 0;

	const int NAME = 0;
	const int TYPE = 1;
	const int VALUE = 2;
	const int DATA = 3;

	private string[,] ocean;
	private long[,] sessions;

	void Start() {

		long thisSession;
		long anotherSession;

		thisSession = Initialise (1);
		anotherSession = Initialise (1);
		Debug.Log ("Session num: " + thisSession);


		Write (thisSession, "A", "A", ELEMENT);
		Write (thisSession, "A", "B", ELEMENT);
		//Write (thisSession, "Jellyfish", "Jellyfish", ELEMENT);
	
		int i;
		for (i=0; i<4; i++) {
			Debug.Log("NAME: " +ocean[i,NAME] +
			" TYPE: " +ocean[i,TYPE] +
			" VALUE: " +ocean[i,VALUE] +
			" DATA: " +ocean[i,DATA]);

		}

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
			if(sessions[i,0] == FREE) {
				sessions[i,0] = HOME;
				/*
				 * SET PRIVLEDGES HERE - according to lFlags
				 * */
				return i;
			}
		}
		// Return ERROR code to show no spaces found if end of array is reached.
		return ERROR;
	}

	public long Write(long lSession, string sPath, object oValue, long lFlags){

		// 1) Retrieve the session pointer from the session array
		long sessionPointer = sessions [lSession, 0];
		// CHECK PRIVILEDGES - do they have write access?
		// RETURN ERROR if not
	
		Debug.Log ("Session pointer = " + sessionPointer);

		// Remove any leading/trailing spaces from the path
		sPath = sPath.Trim ();

		// Save the initial session location in case of error
		long sessionReset = sessionPointer;

		// 2) Syntax check for provided sPath (ABSOLUTE)
		if (sPath.StartsWith ("psml//:")) {

			// Remove the "psml//:" marker
			char[] toTrimS = {'p','s','m','l','/',':'};
			sPath = sPath.TrimStart (toTrimS);
			// NOTE - trailing spaces and the final / may need to be trimmed also

			// Split the string at each "/" and place tokenised strings into an array
			string[] splitPath = sPath.Split ('/');

			foreach (string tag in splitPath) {
				/*
				 * sessionPointer = NAVIGATE TO CORRECT HEADER TAG IN OCEAN
				 * 
				 * if NOT FOUND && NOT AT END OF LOOP - return ERROR
				 * 
				 * if LAST IN LOOP {
				 * 		
				 * 		CHECK FLAGS - attribute or element?
				 * 
				 * 		SEARCH CURRENT HEADER FOR NAMED ATT OR ELEMENT:
				 * 
				 * 		if (FOUND) {
				 * 			OVERWRITE
				 * 		}
				 * 		else {
				 * 			CREATE NEW
				 * 			- CHECK FOR FREE SPACE IN OCEAN
				 * 			- IF SPACE WRITE IT IN
				 * 			- IF NOT, COPY INTO A NEW OCEAN WHERE MOVING ITEMS APPROPRIATELY
				 * 			- NOTE OFFSETS etc will need to be updated accordingly
				 * 		}
				 * }
				 * */
			}
		} 

		// 3) Syntax check for provided sPath (RELATIVE)
		else {
			if(lFlags == ELEMENT) {

				// Initialise parent offset to 0 (ie. itself)
				long offsetToParent = 0;
				long offsetToHeader = 0;
				long elementSize = 0;

				// Get the element size
				if (ocean[sessionPointer,TYPE] == "H") {
					elementSize = Convert.ToInt64(ocean[sessionPointer,VALUE]);
				}
			
				// If free, write element into it
				if(ocean[sessionPointer,TYPE] == "") {
					ocean[sessionPointer,NAME]	= oValue.ToString();
					ocean[sessionPointer,TYPE]	= "H";
					ocean[sessionPointer,VALUE]	= "1";
					ocean[sessionPointer,DATA]	= offsetToParent.ToString();
				}
				else {
					long i, writeTo = 0;

					// Ensure the nested element doesn't already exist
					for(i=1;i<elementSize;i++) {
						if(ocean[sessionPointer + i,TYPE] == "E") 
						{
							if(ocean[sessionPointer+i,NAME] == oValue.ToString())
							{
								Debug.Log("Nested element '"+oValue.ToString()+"' already exists.");
								return sessionReset;
							}
						}
					}

					// 1) Add the nested element details 
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
						// If no space is found the ocean will need to be resized
						if (writeTo == ocean.GetLength(0)) {
							Debug.Log("EXPANSION NEEDED");
							return sessionReset;
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
					sessionPointer = writeTo;
					ocean[sessionPointer,NAME]	= oValue.ToString();
					ocean[sessionPointer,TYPE]	= "H";
					ocean[sessionPointer,VALUE]	= "1";
					ocean[sessionPointer,DATA]	= offsetToParent.ToString();
				}


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

		}
	
		// Return success code 
		return lFlags;
	}


	// Sets every cell in a 2D string array to "" 
	/*
	private object[,] setArrayToFree(object[,] array){

		// Initialise ALL cells to FREE ("")
		int i,j;
		int x=array.GetLength(0),y=array.GetLength(1);
		object insert;


		if (array.GetType () == typeof(string)) {
			insert = (string) "";
		}
		if (array.GetType () == typeof(long)) {
			insert = (long) 0;
		}

		for(i=0;i<x;i++) {
			for(j=0;j<y;j++) {
				array[i,j] = insert;
			}
		}
		return array;
	}
*/
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
				sessions[i,j] = FREE;
			}
		}
		return sessions;
	}
}
