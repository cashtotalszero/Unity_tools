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

	const int PIXE_OCEAN_MOLECULE_NAME = 0;
	const int PIXE_OCEAN_MOLECULE_TYPE = 1;
	const int PIXE_OCEAN_MOLECULE_VALUE = 2;
	const int PIXE_OCEAN_MOLECULE_DATA = 3;

	const int PIXE_OCEAN_SESSION_POINTER = 0;

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
		Write (thisSession, "A", null, PIXE_PSML_ELEMENT);
		Debug.Log ("ONE:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,PIXE_OCEAN_MOLECULE_NAME] +
			          " TYPE: " +ocean[i,PIXE_OCEAN_MOLECULE_TYPE] +
			          " VALUE: " +ocean[i,PIXE_OCEAN_MOLECULE_VALUE] +
			          " DATA: " +ocean[i,PIXE_OCEAN_MOLECULE_DATA]);
			
		}

		Write (thisSession, "B", null, PIXE_PSML_ELEMENT);
		Debug.Log ("TWO:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,PIXE_OCEAN_MOLECULE_NAME] +
			          " TYPE: " +ocean[i,PIXE_OCEAN_MOLECULE_TYPE] +
			          " VALUE: " +ocean[i,PIXE_OCEAN_MOLECULE_VALUE] +
			          " DATA: " +ocean[i,PIXE_OCEAN_MOLECULE_DATA]);
			
		}

		Move (thisSession, "B");

		Write (thisSession, "C", null, PIXE_PSML_ELEMENT);
		Debug.Log ("THREE:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,PIXE_OCEAN_MOLECULE_NAME] +
			          " TYPE: " +ocean[i,PIXE_OCEAN_MOLECULE_TYPE] +
			          " VALUE: " +ocean[i,PIXE_OCEAN_MOLECULE_VALUE] +
			          " DATA: " +ocean[i,PIXE_OCEAN_MOLECULE_DATA]);
			
		}

		Write (thisSession, "D", null, PIXE_PSML_ELEMENT);
		Debug.Log ("FOUR:");
		for (i=0; i<8; i++) {
			Debug.Log("NAME: " +ocean[i,PIXE_OCEAN_MOLECULE_NAME] +
			          " TYPE: " +ocean[i,PIXE_OCEAN_MOLECULE_TYPE] +
			          " VALUE: " +ocean[i,PIXE_OCEAN_MOLECULE_VALUE] +
			          " DATA: " +ocean[i,PIXE_OCEAN_MOLECULE_DATA]);

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
		long sessionPointer = sessions [lSession, PIXE_OCEAN_SESSION_POINTER];
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
			if (ocean [sessionPointer, PIXE_OCEAN_MOLECULE_TYPE] == "") {
				if (lFlags == PIXE_PSML_ELEMENT) {
					createMolecule(
						sessionPointer, sessionPointer,
						sPath, "H", "0", currentHeader.ToString()
						);
				}
				else if  (lFlags == PIXE_PSML_ATTRIBUTE){
					Debug.Log("ERROR = Unable to write Attribute without a parent element");
				}
			
				// Note: Session pointer does not move in this case
				return lFlags;
			}

			// Find the current Drop Header location
			currentHeader = sessionPointer;
			while (ocean[currentHeader,PIXE_OCEAN_MOLECULE_TYPE] != "H") {
				currentHeader--;
			}

			// If SP is on an Attribute, move it to the Header:
			if(ocean[sessionPointer,PIXE_OCEAN_MOLECULE_TYPE] == "A") {
				sessionPointer = currentHeader;
			}
			// Get the current Drop size...
			elementSize = Convert.ToInt64 (ocean [currentHeader, PIXE_OCEAN_MOLECULE_VALUE]);

			// ...then add the nested element details to the end of the current Drop
			createMolecule(
				(currentHeader + elementSize), currentHeader, 
				sPath, "E", PIXE_OCEAN_UNSET.ToString (), ""
				);
		
			// Move the session point to the newly written record
			sessionPointer = currentHeader + elementSize;

			// Save the position of the SP
			sessions [lSession, PIXE_OCEAN_SESSION_POINTER] = sessionPointer;
		}
 
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

	private void createMolecule(long lLocation, long lHeader, string sName, string sType, string sValue, string sData)
	{
		// Write all the molecule information into Ocean
		ocean [lLocation,PIXE_OCEAN_MOLECULE_NAME] = sName;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_TYPE] = sType;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_VALUE] = sValue;
		ocean [lLocation,PIXE_OCEAN_MOLECULE_DATA] = sData;

		// Update the size of the Drop header to reflect new addtion
		long update = Convert.ToInt64 (ocean [lHeader, PIXE_OCEAN_MOLECULE_VALUE]);
		update = update + 1;
		ocean [lHeader, PIXE_OCEAN_MOLECULE_VALUE] = update.ToString();
		return;
	}

	private long createNested(long lCursor, long lParentHeader)
	{
		// Save the nested Element reference orgin & find somewhere to put the new Header
		long lOrigin = lCursor;
		lCursor = getDrop(lCursor);
		
		// Write the Header data
		createMolecule(
			lCursor, lCursor, 
			ocean[lOrigin,PIXE_OCEAN_MOLECULE_NAME], "H", "0", (lParentHeader - lCursor).ToString ()
			);	
		// Update the offset value in the parent element molecule to point to this Header
		ocean [lOrigin,PIXE_OCEAN_MOLECULE_VALUE] = (lCursor - lOrigin).ToString();

		// Return the new Cursor location
		return lCursor;
	}

	public void Move(long lSession, string sDestination) 
	{
		// Retrieve the session pointer from the session array
		long lCursor = sessions [lSession, PIXE_OCEAN_SESSION_POINTER];
	
		// MOVE UP WITH ".." NEEDS TO GO HERE

		// Move the cursor to the current Drop Header location
		while (ocean[lCursor,PIXE_OCEAN_MOLECULE_TYPE] != "H") {
			lCursor--;
		}
		long lHeader = lCursor;				// Save this header location (for use in child Header)

		// Get the size of the current Drop & set found flag to false
		long lLimit = Convert.ToInt64 (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);

		// Search for Element/Attribute matching the requested destination
		bool bFound = false;
		while (lCursor < lLimit) {
			if(ocean[lCursor,PIXE_OCEAN_MOLECULE_NAME] == sDestination ) {
				bFound = true;
				break;
			}
			lCursor++;
		}
		// Display error if destination not found in current Drop
		if (!bFound) {
			Debug.Log("ERROR = Unable to move cursor. No matching record found in current location.");
			return;
		}
		// When moving into nested elements:
		if(ocean [lCursor,PIXE_OCEAN_MOLECULE_TYPE] == "E") {
			// Create a Drop Header if one doesn't already exist
			if(ocean [lCursor,PIXE_OCEAN_MOLECULE_VALUE] == PIXE_OCEAN_UNSET.ToString ()) {
				lCursor = createNested(lCursor, lHeader);
			}
			// Else, move the currsor to the correct header.
			else {
				lCursor = lCursor + Convert.ToInt64(ocean [lCursor,PIXE_OCEAN_MOLECULE_VALUE]);
			}
		}
		// Update the postion of the session cursor
		sessions [lSession, PIXE_OCEAN_SESSION_POINTER] = lCursor;
		return;
	}

	// MOVE DROP FUNCTION NEEDED
}
