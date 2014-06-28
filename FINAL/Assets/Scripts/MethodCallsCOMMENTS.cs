using UnityEngine;
using System.Collections;
using System;

public class MethodCallsCOMMENTS : MonoBehaviour {

	// lFlag definitions (must be greater than 0)
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_WRITE_ELEMENT = 2;
	const int PIXE_PSML_READ_ATTRIBUTE = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;

	const int PIXE_ERROR = -1;
	const int MEMORY_ERROR = -2;
	const int PIXE_OCEAN_FREE = -1;
	const int PIXE_OCEAN_HOME = 0;			// The root node of the PIXE ocean
	const int PIXE_OCEAN_UNSET = 0;
	const int PIXE_OCEAN_DEFAULT_DROP = 1;

	// Ocean array dfinitions
	const int PIXE_OCEAN_MOLECULE_NAME = 0;			// Name column
	const int PIXE_OCEAN_MOLECULE_TYPE = 1;			// Type column
	const int PIXE_OCEAN_MOLECULE_VALUE = 2;		// Value column
	const int PIXE_OCEAN_MOLECULE_DATA = 3;			// Data column
	const int PIXE_OCEAN_COLUMN_COUNT = 4;			// Total count of columns
	const int PIXE_OCEAN_ROW_COUNT = 100;
	
	// Session array definitions
	const int PIXE_OCEAN_SESSION_CURSOR = 0;
	const int PIXE_OCEAN_SESSION_PRIVILEGES = 1;
	const int PIXE_OCEAN_SESSION_COLUMN_COUNT = 2;
	const int PIXE_OCEAN_SESSION_ROW_COUNT = 50;

	// Operation success/fail codes
	const int PIXE_OP_SUCCESSFUL = 1;
	const int PIXE_OP_FAIL_INVALID_PATH = 0;

	//gsOcean
	// Create a list of oceans & sessions
	private string[,] ocean;
	private long[,] sessions;

	void Start() {

		long thisSession;
		long anotherSession;

		thisSession = Initialise (1);
		anotherSession = Initialise (1);

		int i;
		Write (thisSession, "A", null, PIXE_PSML_WRITE_ELEMENT);

		Write (thisSession, "B", null, PIXE_PSML_WRITE_ELEMENT);

		Write (thisSession, "C", null, PIXE_PSML_WRITE_ELEMENT);
		//Write (thisSession, "B", null, PIXE_PSML_ELEMENT);
		//Move (thisSession, "B");
		//Write (thisSession, "NestedB", null, PIXE_PSML_WRITE_ELEMENT);
		Write (thisSession, "psml://A/B/Emma", 14, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "psml://A/Alex", 99, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "psml://A/D", null, PIXE_PSML_WRITE_ELEMENT);


		//Move (thisSession, "..");
		//Write (thisSession, "psml://A/B", null, PIXE_PSML_ELEMENT);
		//Write (thisSession, "psml://A/B/Emma", 1, PIXE_PSML_ATTRIBUTE);
		//Write (thisSession, "psml://A/Alex", 1, PIXE_PSML_ATTRIBUTE);
		//Write (thisSession, "psml://A/Alex", null, PIXE_PSML_ELEMENT);
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
			if(sessions[i,PIXE_OCEAN_SESSION_CURSOR] == PIXE_OCEAN_FREE) {
				sessions[i,PIXE_OCEAN_SESSION_CURSOR] = PIXE_OCEAN_HOME;
				/*
				 * SET PRIVLEDGES HERE - according to lFlags
				 * */
				return i;
			}
		}
		// Return ERROR code to show no spaces found if end of array is reached.
		return PIXE_ERROR;
	}

	/*
	// PreparePath() = for write, checks if attributes/elements already exist 
	private string[] processPath(string sPath)
	{
		// Remove the "psml//:" marker...
		sPath = sPath.Remove(0,7);
		
		// ...and the last "/" (if present)

		if (sPath.EndsWith("/")) {
			sPath = sPath.Substring (0, sPath.Length - 1);
		}

		// Split the path string at each "/" and place tokenised strings into an array
		string[] splitPath = sPath.Split ('/');
		return splitPath;
	}
*/
	private void verifyPath(long lSession, ref string sPath, ref long lFlags)
	{
		// Remove any trailing/leading spaces
		sPath = sPath.Trim();

		// For absolute paths:
		if (sPath.StartsWith ("psml://")) {
			// Remove the "psml//:" marker & tokenise the path at each "/"
			sPath = sPath.Remove(0,7);
			string[] sPathArray = sPath.Split ('/');

			// Amend the path to the attribute or element to write (i.e. the last one)
			sPath = sPathArray[(sPathArray.GetLength(0)-1)];
			
			// When writing elements, set last path string to the elements parent
			if(lFlags == PIXE_PSML_WRITE_ELEMENT) {
				Array.Resize(ref sPathArray, sPathArray.Length-1);
			}
			// Save current cursor location then set the cursor to the root node of the ocean
			long lCursorReset = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
			sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = PIXE_OCEAN_HOME;

			// Attempt to move cursor to provided path
			int i=0, iDepth = sPathArray.GetLength(0);
			bool bFound = true;
			while(i<iDepth && bFound == true) {
				bFound = Move (lSession, sPathArray[i]);
				i++;
			}
			// Return sucess flag if path is successfully navigated
			if(i != iDepth) {
				// Reset cursor to original location if path is invalid & flag error
				sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursorReset;
				lFlags = PIXE_OP_FAIL_INVALID_PATH;
			}
		}
		return;
	}


	// CHANGE RETURN TYPE FROM BOOL TO lFlags 
	/*
	private bool navigatePath(long lSession, string [] sPathArray) 
	{
		// Save current cursor location in case of error
		long lCursorReset = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];

		// Set the session cursor to Home (the root node)
		sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = PIXE_OCEAN_HOME;

		// Attempt to move cursor to provided path
		int i=0, iDepth = sPathArray.GetLength(0);
		bool bFound = true;
		while(i<iDepth && bFound == true) {
			bFound = Move (lSession, ref sPathArray[i],true);
			i++;
		}
		// Return sucess flag if path is successfully navigated
		if(i == iDepth) {
			return true;
		}
		// Reset cursor to original location if path is invalid & return false
		sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursorReset;
		return false;
	}
	*/
		// Move the cursor and put back in sessions
		// Return the updated path
	/*
	public object Read(long lSession, string sPath, long lFlags) 
	{
		long lCursorReset = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
		
		// Remove any leading/trailing spaces from the path
		//sPath = sPath.Trim ();

		// Syntax check for provided sPath (ABSOLUTE)
		if (sPath.StartsWith ("psml://")) {
			// Minor tweak needed from Write
		} 
		// 1) Retrieve the session pointer from the session array
		long lCursor = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
		
		// CHECK PRIVILEDGES HERE - do they have write access? - return error if not
		
		// 3) Syntax check for provided sPath (RELATIVE)
		long lDropSize = 0;				// Size of the current drop
		
		// If it's on a FREE them throw an error
		if (ocean [lCursor, PIXE_OCEAN_MOLECULE_TYPE] == "") {
			// Cannot read from an empty molecule
			return PIXE_ERROR;
		}

		// Move the cursor to the current Drop header
		while (ocean[lCursor,PIXE_OCEAN_MOLECULE_TYPE] != "H") {
			lCursor--;
		}
		// Get the current Drop size and add the Attribute/Element details to the end
		lDropSize = Convert.ToInt64 (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);
		
		// Search for the specific element/att to read
		bool bFound = false;
		while (lCursor < (lCursor + lDropSize)) {
			if(ocean[lCursor, PIXE_OCEAN_MOLECULE_NAME] == sPath) {
				bFound = true;
				break;
			}
			lCursor++;
		}
		if(bFound) {
			sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;

			if (lFlags == PIXE_PSML_ELEMENT) {
				// What to return for elements?
			}
			else if (lFlags == PIXE_PSML_ATTRIBUTE) {
				// Switch to correct data type
				return null // return the att value here in correct datatype
			}
		}
		else {
			sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursorReset;
			// Return an error if att/element not found
		}
	}
*/
	public long Write(long lSession, string sPath, object oValue, long lFlags)
	{
		// Save initial cursor location (in case of error) and set cursor to path 
		long lCursorReset = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
		verifyPath (lSession, ref sPath, ref lFlags);

		if (lFlags > 0) {
			// Remove any leading/trailing spaces from the path
			//sPath = sPath.Trim ();

			// Syntax check for provided sPath (ABSOLUTE)
			/*
			if (sPath.StartsWith ("psml://")) {
			string[]sPathArray = processPath(sPath);
			// Amend path to the attribute/element to write (i.e. the last one)
			sPath = sPathArray[(sPathArray.GetLength(0)-1)];

			// For elements to write, set path array to it's parent
			if(lFlags == PIXE_PSML_WRITE_ELEMENT) {
				Array.Resize(ref sPathArray, sPathArray.Length-1);
			}
			// Navigate to the path (return if fails)
			if(!navigatePath(lSession, sPathArray)) {
				return lFlags;
			}
		} 
		*/
			// 1) Retrieve the session pointer from the session array
			long lCursor = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];

			// CHECK PRIVILEDGES HERE - do they have write access? - return error if not

			// 3) Syntax check for provided sPath (RELATIVE)
			long lDropSize = 0;				// Size of the current drop
			
			// If cursor is on a free slot, write element into it (only used once for empty oceans)
			if (ocean [lCursor, PIXE_OCEAN_MOLECULE_TYPE] == "") {
				if (lFlags == PIXE_PSML_WRITE_ELEMENT) {
					createMolecule (
					lCursor, lCursor,
					sPath, "H", "0", lCursor.ToString ());		// Note: Offset to parent = itself
				} else if (lFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
					Debug.Log ("ERROR = Unable to write Attribute without a parent element");

				}
				// Note: Session pointer does not move in this case
				return lFlags;
			}
			// Move the cursor to the current Drop header
			while (ocean[lCursor,PIXE_OCEAN_MOLECULE_TYPE] != "H") {
				lCursor--;
			}
			// Get the current Drop size and add the Attribute/Element details to the end
			lDropSize = Convert.ToInt64 (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);

			// NEED CHECK TO SEE IF ATT/EL ALREADY EXISTS

			if (lFlags == PIXE_PSML_WRITE_ELEMENT) {
				createMolecule (
				(lCursor + lDropSize), lCursor, 
				sPath, "E", PIXE_OCEAN_UNSET.ToString (), "");
			} else if (lFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
				createMolecule (
				(lCursor + lDropSize), lCursor, 
				sPath, "A", oValue.ToString (), (oValue.GetType ()).ToString ());
			}
			// Move the cursor to the newly written record and save in sessions array
			lCursor = lCursor + lDropSize;
			sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;
			return lFlags;
		}
		return lFlags;
	}
	
	private string[,] createOcean(string[,] ocean)
	{
		ocean = new string[PIXE_OCEAN_ROW_COUNT, PIXE_OCEAN_COLUMN_COUNT];

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
		sessions = new long[PIXE_OCEAN_SESSION_ROW_COUNT, PIXE_OCEAN_SESSION_COLUMN_COUNT];
		
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
		// NEED ARRAY/LIST TO HOLD FREE DROPS

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
		ocean [lLocation, PIXE_OCEAN_MOLECULE_NAME] = sName;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_TYPE] = sType;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_VALUE] = sValue;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_DATA] = sData;

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
			ocean[lOrigin,PIXE_OCEAN_MOLECULE_NAME], "H", "0", (lParentHeader - lCursor).ToString ());

		// Update the offset value in the parent element molecule to point to this Header
		ocean [lOrigin,PIXE_OCEAN_MOLECULE_VALUE] = (lCursor - lOrigin).ToString();

		// Return the new Cursor location
		return lCursor;
	}

	// CHANGE RETURN TO LFLAGS
	public bool Move(long lSession, string sDestination) 
	{
		// Retrieve the session pointer from the session array
		long lCursor = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
	
		// Move the cursor to the current Drop Header location
		while (ocean[lCursor,PIXE_OCEAN_MOLECULE_TYPE] != "H") {
			lCursor--;
		}
		long lHeader = lCursor;				// Save this header location (for use in child Header)

		// Move cursor to parent Header if desitination is ".."
		if (sDestination == "..") {
			lCursor = lCursor + Convert.ToInt64 (ocean[lCursor,PIXE_OCEAN_MOLECULE_DATA]);
		} 
		// Else, search for the destination attribute/element reference in the current Drop
		else {
			// Get the size of the current Drop & set found flag to false
			long lLimit = Convert.ToInt64 (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);

			// Search for Element/Attribute matching the requested destination
			bool bFound = false;
			while (lCursor < lLimit) {
				if (ocean [lCursor, PIXE_OCEAN_MOLECULE_NAME] == sDestination) {
					bFound = true;
					break;
				}
				lCursor++;
			}
			// Display error if destination not found in current Drop
			if (!bFound) {
				Debug.Log ("ERROR = Unable to move cursor. No matching record found in current location.");
				return false;
			}
			// When moving into nested elements:
			if (ocean [lCursor, PIXE_OCEAN_MOLECULE_TYPE] == "E") {
				// Create a Drop Header if one doesn't already exist
				if (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE] == PIXE_OCEAN_UNSET.ToString ()) {
					lCursor = createNested (lCursor, lHeader);
				}
				// Else, move the currsor to the correct header.
				else {
					lCursor = lCursor + Convert.ToInt64 (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);
				}
			}
		}
		// Update the postion of the session cursor
		sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;
		return true;	
	}

	// MOVE DROP FUNCTION NEEDED
}
