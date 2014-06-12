using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MethodCalls : MonoBehaviour {

	// lFlag definitions (must be greater than 0)
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_WRITE_ELEMENT = 2;
	const int PIXE_PSML_READ_ATTRIBUTE = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;

	const int PIXE_ERROR = -1;
	const int MEMORY_ERROR = -2;
	const int PIXE_OCEAN_FREE = -1;
	const int PIXE_OCEAN_DEFAULT_HOME = 0;			// The root node of the PIXE ocean
	const int PIXE_OCEAN_UNSET = 0;
	const int PIXE_OCEAN_DEFAULT_DROP = 1;
	const int PIXE_OCEAN_UNSET_DROP = -1;

	// Ocean array dfinitions
	const int PIXE_OCEAN_MOLECULE_NAME = 0;			// Name column
	const int PIXE_OCEAN_MOLECULE_TYPE = 1;			// Type column
	const int PIXE_OCEAN_MOLECULE_VALUE = 2;		// Value column
	const int PIXE_OCEAN_MOLECULE_DATA = 3;			// Data column
	const int PIXE_OCEAN_COLUMN_COUNT = 4;			// Total count of columns
	const int PIXE_OCEAN_DEFAULT_ROW_COUNT = 100;
	
	// Session array definitions
	const int PIXE_OCEAN_SESSION_CURSOR = 0;
	const int PIXE_OCEAN_SESSION_PRIVILEGES = 1;
	const int PIXE_OCEAN_SESSION_OCEAN = 2;
	const int PIXE_OCEAN_SESSION_COLUMN_COUNT = 3;
	const int PIXE_OCEAN_SESSION_ROW_COUNT = 50;

	// Drop allocation array definitions
	const int PIXE_OCEAN_DROP_5 = 0;
	const int PIXE_OCEAN_DROP_10 = 1;
	const int PIXE_OCEAN_DROP_15 = 2;
	const int PIXE_OCEAN_DROP_20 = 3;
	const int PIXE_OCEAN_DROP_25 = 4;
	const int PIXE_OCEAN_DROP_30 = 5;
	const int PIXE_OCEAN_DROP_MANY = 6;
	const int PIXE_OCEAN_DROP_COULMN_COUNT = 7;
	const int PIXE_OCEAN_DROP_MIN = 5;

	// Operation success/fail codes
	const int PIXE_OP_SUCCESSFUL = 1;
	const int PIXE_OP_FAIL_INVALID_PATH = 0;
	const int PIXE_OP_FAIL_INVALID_CURSOR_POSITION = -1;
	const int PIXE_OP_FAIL_DUPLICATE_RECORD = -1;

	//gsOcean
	// Create a list of oceans & sessions
	private string[,] ocean;
	private long[,] sessions;
	private List<long>[] drops;
	List<string[,]> oceanList;				// List to hold all oceans (INCOMPLETE)
	private long oceanHome;

	void Start() {

		// Create a List to hold oceans (For multiple ocean functionality - INCOMPLETE)
		//oceanList = new List<string[,]> ();

		// Create a session
		long thisSession = Initialise ();

		// Write the root node:
		Write (thisSession, "A", null, PIXE_PSML_WRITE_ELEMENT);
		Write (thisSession, "Att1", 1, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "Att2", 1, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "Att3", 1, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "B", null, PIXE_PSML_WRITE_ELEMENT);
		Move (thisSession, "B");
	
		Write (thisSession, "AttB", 65, PIXE_PSML_WRITE_ATTRIBUTE);

		//Write (thisSession, "psml://A/Att4", 1, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "psml://A/B/Att99", 1, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "psml://A/B/C", null, PIXE_PSML_WRITE_ELEMENT);
		Write (thisSession, "psml://A/B/C/Catt", 69, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "psml://A/attttt", 75, PIXE_PSML_WRITE_ATTRIBUTE);


		Write (thisSession, "psml://A/B/X1", 12, PIXE_PSML_WRITE_ATTRIBUTE);
		Write (thisSession, "psml://A/B/D", null, PIXE_PSML_WRITE_ELEMENT);
		Write (thisSession, "psml://A/B/D/Fox", 29, PIXE_PSML_WRITE_ATTRIBUTE);


		//Write (thisSession, "psml://A/B/X2", 15, PIXE_PSML_WRITE_ATTRIBUTE);

		//Write (thisSession, "psml://A/X99", 12, PIXE_PSML_WRITE_ATTRIBUTE);
		/*
		object ReadAt1 = Read (thisSession, "psml://A/B/AttB", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Att that does exist outputs (AttB): " + ReadAt1);

		ReadAt1 = Read (thisSession,"psml://A/B/C/Catt",PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Att that does exist outputs (Catt,69): " + ReadAt1);

		ReadAt1 = Read (thisSession,"psml://A/B/C",PIXE_PSML_READ_ELEMENT);
		Debug.Log ("El that does exist outputs (C): " + ReadAt1);

		ReadAt1 = Read (thisSession,"psml://A/attttt",PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Att that does exist outputs (atttttt,75): " + ReadAt1);

		//ReadAt1 = Read (thisSession,"psml://A/B/C/D",PIXE_PSML_READ_ELEMENT);
		//Debug.Log ("El that does exist outputs (D): " + ReadAt1);
*/
		// Print out the Ocean
		Debug.Log ("OCEAN STATUS:");
		int i;
		for (i=0; i<40; i++) {
			Debug.Log(i+" NAME: " + ocean[i,PIXE_OCEAN_MOLECULE_NAME] +
			          " TYPE: " +ocean[i,PIXE_OCEAN_MOLECULE_TYPE] +
			          " VALUE: " +ocean[i,PIXE_OCEAN_MOLECULE_VALUE] +
			          " DATA: " +ocean[i,PIXE_OCEAN_MOLECULE_DATA]);

		}
	}

	public long Initialise(/*long lFlags*/) {

		// If ocean doesn't already exist create one with 100 rows to start with
		if (ocean == null) {
			//ocean = createOcean(ocean);
			createOcean(ref ocean);
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
				sessions[i,PIXE_OCEAN_SESSION_CURSOR] = oceanHome;
				/*
				 * SET PRIVLEDGES HERE - according to lFlags
				 * */
				return i;
			}
		}
		// Return ERROR code to show no spaces found if end of array is reached.
		return PIXE_ERROR;
	}

	public void Free(long lSession) 
	{
		sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = PIXE_OCEAN_FREE;
		// UNSET PRIVILEGES HERE
		return;
	}

	public object Read(long lSession, string sPath, long lFlags) 
	{
		// Move the ocean cursor to the requested Drop location
		navigatePath (lSession, ref sPath, ref lFlags);

		// If successful - Retrieve the session pointer from the session array
		if (lFlags >= PIXE_OP_SUCCESSFUL) {
			long lCursor = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
			// CHECK PRIVILEDGES HERE - do they have write access? - return error if not
			
			// If it's on a FREE them throw an error - Cannot read from an empty molecule
			if (ocean [lCursor, PIXE_OCEAN_MOLECULE_TYPE] == "") {
				lFlags = PIXE_OP_FAIL_INVALID_CURSOR_POSITION;
				Debug.Log("ERROR = Cannot read. Invalid cursor position");
				return null;
			}
			else {
				// Move cursor onto requested attribute/element within the Drop
				if(findMolecule(ref lCursor, sPath)) {
					sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;
					// For successfully found elements - return true
					if (lFlags == PIXE_PSML_READ_ELEMENT) {
						return true;
					}
					// For attributes - return the value (in correct data type)
					else if (lFlags == PIXE_PSML_READ_ATTRIBUTE) {
						// Need to switch to correct data type!!!!!!!
						return ocean[lCursor, PIXE_OCEAN_MOLECULE_VALUE];
					}
				}
				else {
					// If element is not found, return false
					if(lFlags == PIXE_PSML_READ_ELEMENT) {
						return false;
					}
					// Return an error if att/element not found
					sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;
					return "No attribute found";
				}
			}
		}
		return null;
	}

	public long Write(long lSession, string sPath, object oValue, long lFlags)
	{
		// Move the ocean cursor to the requested Drop location
		navigatePath (lSession, ref sPath, ref lFlags);

		// If successful - Retrieve the session pointer from the session array
		if (lFlags >= PIXE_OP_SUCCESSFUL) {
			long lCursor = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
			// CHECK PRIVILEDGES HERE - do they have write access? - return error if not
			long lDropSize = 0;	
			
			// If cursor is on a free slot & cursor is on the default ocean home - write the root node
			if (ocean [lCursor, PIXE_OCEAN_MOLECULE_TYPE] == "") {
				if (lFlags == PIXE_PSML_WRITE_ELEMENT && lCursor == PIXE_OCEAN_DEFAULT_HOME) {
					oceanHome = (long)PIXE_OCEAN_DEFAULT_HOME;
					createMolecule (
						lCursor, lCursor,
						sPath, "H", "0", lCursor.ToString (), 
						ref lCursor);
				} else {
					lFlags = PIXE_OP_FAIL_INVALID_CURSOR_POSITION;
					Debug.Log("ERROR = Invalid cursor position.");
				}
				// Note: Session pointer does not move in this case
				return lFlags;
			}
			// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
			while (ocean[lCursor,PIXE_OCEAN_MOLECULE_TYPE] != "H") {
				lCursor--;
			}
			lDropSize = Convert.ToInt64 (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);
			
			// Ensure molecule with a matching name does not already exist
			long lSearch = lCursor;
			if(findMolecule(ref lSearch, sPath)) {
				// If Attribute with that name already exists, overwrite the value
				if(lFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
					ocean[lSearch, PIXE_OCEAN_MOLECULE_VALUE] = oValue.ToString();
					ocean[lSearch, PIXE_OCEAN_MOLECULE_DATA] = (oValue.GetType ()).ToString ();
					sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;
					return lFlags;
				}
				else {
					Debug.Log("ERROR = Cannot write. This element/attribute name already exists in this Drop.");
					lFlags = PIXE_OP_FAIL_DUPLICATE_RECORD;
					return lFlags;
				}
			}
			// If it doesn't, create a Molecule for it
			if (lFlags == PIXE_PSML_WRITE_ELEMENT) {
				createMolecule (
					(lCursor + lDropSize), lCursor, 
					sPath, "E", PIXE_OCEAN_UNSET.ToString (), "", 
					ref lCursor);
			} else if (lFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
				//Debug.Log("Before cursor: "+lCursor);
				createMolecule (
					(lCursor + lDropSize), lCursor, 
					sPath, "A", oValue.ToString (), (oValue.GetType ()).ToString (), 
					ref lCursor);
				//Debug.Log("Written att = "+sPath);
				//Debug.Log("Cursor now at: "+lCursor);
				//Debug.Log("Drop size: "+lDropSize);
				//Debug.Log("****************************");
			}
			// Move the cursor to the newly written record and save in sessions array
			lCursor += lDropSize;
			sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;
			return lFlags;
		}
		return lFlags;
	}

	// CHANGE RETURN TO LFLAGS
	public bool Move(long lSession, string sDestination) 
	{
		// Retrieve the session pointer from the session array
		long lCursor = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
		//Debug.Log ("Retreived cursor = " + lCursor);

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
			if(!findMolecule(ref lCursor, sDestination)){
				Debug.Log ("ERROR = Unable to move cursor. No matching record found in current location:" + sDestination);
				return false;
			}
			// When moving into nested elements - Create a Drop Header if one doesn't already exist...
			if (ocean [lCursor, PIXE_OCEAN_MOLECULE_TYPE] == "E") {
				if (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE] == PIXE_OCEAN_UNSET.ToString ()) {
					//lCursor = createNested (lCursor, lHeader);
					createNested (ref lCursor, lHeader);
				}
				// ...or move the currsor to the correct header if it already exists.
				else {
					lCursor = lCursor + Convert.ToInt64 (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);
				}
			}
		}
		// Update the postion of the session cursor
		sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursor;
		return true;	
	}

	private void navigatePath(long lSession, ref string sPath, ref long lFlags)
	{
		// Remove any trailing/leading spaces
		sPath = sPath.Trim();

		// For absolute paths - Remove the "psml//:" marker & tokenise the path at each "/"
		if (sPath.StartsWith ("psml://")) {
			sPath = sPath.Remove(0,7);
			string[] sPathArray = sPath.Split ('/');

			// Amend the path to the actual attribute/element to write/read (i.e. the last one)
			sPath = sPathArray[(sPathArray.GetLength(0)-1)];
			
			// When writing, set last path string to the parent
			if(lFlags == PIXE_PSML_WRITE_ELEMENT || lFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
				Array.Resize(ref sPathArray, sPathArray.Length-1);
			}
			// Save current cursor location then set the cursor to the root node of the ocean
			long lCursorReset = sessions [lSession, PIXE_OCEAN_SESSION_CURSOR];
			sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = oceanHome;

			// Attempt to move cursor to provided path
			int i=0, iDepth = sPathArray.GetLength(0);
			bool bFound = true;
			while(i<iDepth && bFound == true) {
				bFound = Move (lSession, sPathArray[i]);
				i++;
			}
			// If path is invalid - Reset cursor to original location
			if(i != iDepth || !bFound) {
				sessions [lSession, PIXE_OCEAN_SESSION_CURSOR] = lCursorReset;
				lFlags = PIXE_OP_FAIL_INVALID_PATH;
				Debug.Log("ERROR = Invalid path provided.");
			}
		}
		return;
	}

	private void createOcean(ref string[,] ocean)
	{
		ocean = new string[PIXE_OCEAN_DEFAULT_ROW_COUNT, PIXE_OCEAN_COLUMN_COUNT];
		drops = new List<long>[PIXE_OCEAN_DROP_COULMN_COUNT]; 
		oceanHome = PIXE_OCEAN_DEFAULT_HOME;

		// Initialise all cells in ocean to FREE ("")
		int i,j;
		int x=ocean.GetLength(0),y=ocean.GetLength(1);
	    
		for(i=0;i<x;i++) {
			for(j=0;j<y;j++) {
				ocean[i,j] = "";
			}
		}
		// Initialise the drops array by creating a list in each cell
		x = drops.GetLength (0);
		for(i=0;i<x;i++) {
			drops[i] = new List<long>();
		}
		// As the ocean is empty, the first availble drop is at the end of the first (resevered for root) header
		// NOTE: The MANY DropList is one value which is the index of the start of the many block
		drops [PIXE_OCEAN_DROP_MANY].Add(PIXE_OCEAN_DROP_MIN);
		//return ocean;
	}

	private void expandOcean(ref string[,] ocean)
	{
		// Set dimensions for new expanded ocean
		long oldRowCount = ocean.GetLength (0);
		long newRowCount = oldRowCount + PIXE_OCEAN_DEFAULT_ROW_COUNT;
		string[,] newOcean = new string[newRowCount, PIXE_OCEAN_COLUMN_COUNT];

		// Copy the original ocean into the new one
		int i,j;
		int x=newOcean.GetLength(0), y=newOcean.GetLength(1);
		int oldLimit = ocean.GetLength (0);
		for(i=0;i<x;i++) {
			for(j=0;j<y;j++) {
				// Copy the original
				if(i<oldLimit) {
					newOcean[i,j] = ocean[i,j];
				}
				// Set all cells beyond the original to FREE
				else {
					newOcean[i,j] = "";
				}
			}
		}
		//Marshal.FreeHGlobal(ocean);
		ocean = newOcean;
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
	private long getDrop(long lSize) {

		long lFoundDrop = PIXE_OCEAN_UNSET_DROP;

		// Get the list holding the correct sized Drops and round up the Drop size
		long lDropList = PIXE_OCEAN_DROP_5;
		if (lSize < 0) {
			Debug.Log("ERROR = Invalid requested Drop size");
			return PIXE_ERROR;
		}
		if (lSize >= 0 && lSize <= 5) {
			lDropList = PIXE_OCEAN_DROP_5;
			lSize = 5;
		}
		if (lSize > 5 && lSize <= 10) {
			lDropList = PIXE_OCEAN_DROP_10;
			lSize = 10;
		}
		if (lSize > 10 && lSize <= 15) {
			lDropList = PIXE_OCEAN_DROP_15;
			lSize = 15;
		}
		if (lSize > 15 && lSize <= 20) {
			lDropList = PIXE_OCEAN_DROP_20;
			lSize = 20;
		}
		if (lSize > 20 && lSize <= 25) {
			lDropList = PIXE_OCEAN_DROP_25;
			lSize = 25;
		}
		if (lSize > 25 && lSize <= 30) {
			lDropList = PIXE_OCEAN_DROP_30;
			lSize = 30;
		}
		if (lSize > 30) {
			lDropList = PIXE_OCEAN_DROP_MANY;
			lSize = (lSize + lSize % 5);
			Debug.Log("Rounded up: "+lSize);
		}
		// Retreive the List holding the appropriately sized Drops & get the index of the last item
		List<long> dropLookup;
		int iLast;
		bool bDone = false;

		while (!bDone) {
			dropLookup = drops [lDropList];
			iLast = dropLookup.Count - 1;

			// If the list is not empty, allocate the last Drop then remove it from the list
			if (iLast >= 0) {
				lFoundDrop = dropLookup[iLast];
				dropLookup.RemoveAt(iLast);
				
				// If in the many, amend the start point of the many block
				if(lDropList == PIXE_OCEAN_DROP_MANY) {
					drops [PIXE_OCEAN_DROP_MANY].Add(lFoundDrop + lSize);
				}
				bDone = true;
				//return lFoundDrop;
			}
			/* 
			 * If nothing is found, repeat the search in the Many list.
			 * If the Many list has already been search - make the ocean bigger then try again.
			 */
			if(lDropList == PIXE_OCEAN_DROP_MANY) {
				// TO DO:
				// INCREASE THE SIZE OF THE OCEAN
				// THIS FUNCTION CALL SHOULD ADD FREE TO MANY
			}
			lDropList = PIXE_OCEAN_DROP_MANY;
		}
		//Debug.Log ("Found Drop = " + lFoundDrop);
		return lFoundDrop;
	}
	
	private void moveDrop(ref long lCursor, ref long lHeader) 
	{
	

		//Debug.Log (ocean[lHeader,PIXE_OCEAN_MOLECULE_NAME]+ " Drop Moved from: " + lHeader);

		// The required Drop size needs to be larger than the current Drop size:
		long lDropSize = Convert.ToInt64 (ocean [lHeader, PIXE_OCEAN_MOLECULE_VALUE]);
		long lNewLocation = getDrop ((lDropSize+1));

		// 1) Copy the drop to the new location 
		long i, j;
		for (i=0; i<lDropSize; i++) {
			for(j=0;j<PIXE_OCEAN_COLUMN_COUNT;j++) {
				ocean[(lNewLocation+i),j] = ocean[(lCursor+i),j];
			}
		}
		// If the root node has been moved - update the root positon
		if(lHeader == oceanHome) {
			oceanHome = lNewLocation;
		}
		// 2) Update all the offset information in the copied Header
		long lNewHeader = lNewLocation;
		// If it is the root node being moved - update the parent header index to itself
		if(lNewHeader == oceanHome) {
			//ocean[lNewHeader,PIXE_OCEAN_MOLECULE_DATA] = oceanHome.ToString();
		}
		// Else, correct all offset references to parent/child elements
		else {
			//Debug.Log("FUCK YOU!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

			long lCursorReset = lCursor;

			// Update the ofset to its parent
			lCursor = lCursor + Convert.ToInt64 (ocean[lCursor,PIXE_OCEAN_MOLECULE_DATA]);
			ocean[lNewHeader,PIXE_OCEAN_MOLECULE_DATA] = (lCursor - lNewHeader).ToString();

			// Update the offset in the Parent
			if(findMolecule(ref lCursor,ocean[lNewHeader,PIXE_OCEAN_MOLECULE_NAME])) {
				ocean[lCursor,PIXE_OCEAN_MOLECULE_VALUE] = (lNewHeader - lCursor).ToString();
			}
			lCursor = lCursorReset;
			// BELOW SECTION IS WRONG - I think....

			//Debug.Log("Cursor = "+lCursor);
			//Debug.Log("New Header = "+lNewHeader);

	//		long Alex = Convert.ToInt64 (ocean[lCursor,PIXE_OCEAN_MOLECULE_DATA]);
	//		Debug.Log("Extracted value = "+Alex);

			/*
			// Update the offset to its parent
			lCursor = lNewHeader + Convert.ToInt64 (ocean[lCursor,PIXE_OCEAN_MOLECULE_DATA]);
			ocean[lNewHeader,PIXE_OCEAN_MOLECULE_DATA] = (lCursor - lNewHeader).ToString();

		

			// Update the offset in the Parent
			if(findMolecule(ref lCursor,ocean[lNewHeader,PIXE_OCEAN_MOLECULE_NAME])) {
				ocean[lCursor,PIXE_OCEAN_MOLECULE_VALUE] = (lNewHeader - lCursor).ToString();
			}
			////////////
 */
		}
		// 3) Step through the new moved Drop and update any Element offset information

		// NEEDS CORRECTING!!!!!!!

		for (i=0; i<lDropSize; i++) {
			if(ocean[(lNewLocation+i),PIXE_OCEAN_MOLECULE_TYPE] == "E") {
				long lNewElement = lNewLocation + i;
				long lOldElement = lHeader + i;

				// Only update offsets to elements with existing Headers
				if (ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE] != PIXE_OCEAN_UNSET.ToString ()) {
					// Update the offsets in the Elements Header
					lCursor += i;
					lCursor = lCursor + Convert.ToInt64  (ocean[(lHeader+i), PIXE_OCEAN_MOLECULE_VALUE]);
					ocean[lNewElement,PIXE_OCEAN_MOLECULE_VALUE] = (lCursor - lNewElement).ToString();

					// Update the offset to its Header
					lCursor = lOldElement + Convert.ToInt64(ocean[lOldElement,PIXE_OCEAN_MOLECULE_VALUE]);
					ocean[lCursor, PIXE_OCEAN_MOLECULE_DATA] = (lNewHeader - lCursor).ToString();
				}
			}
		}
		// 4) Delete the orginal Drop
		for (i=0; i<lDropSize; i++) {
			for(j=0;j<PIXE_OCEAN_COLUMN_COUNT;j++) {
				ocean[(lHeader+i),j] = "";
			}
		}
		// 5) Put the newly freed space into the drops array
		long lDropList = PIXE_OCEAN_DROP_5;
		long lNewFree = lHeader;
		if (lDropSize >= 0 && lDropSize <= 5) {
			lDropList = PIXE_OCEAN_DROP_5;
		}
		if (lDropSize > 5 && lDropSize <= 10) {
			lDropList = PIXE_OCEAN_DROP_10;
		}
		if (lDropSize > 10 && lDropSize <= 15) {
			lDropList = PIXE_OCEAN_DROP_15;
		}
		if (lDropSize > 15 && lDropSize <= 20) {
			lDropList = PIXE_OCEAN_DROP_20;
		}
		if (lDropSize > 20 && lDropSize <= 25) {
			lDropList = PIXE_OCEAN_DROP_25;
		}
		if (lDropSize > 25 && lDropSize <= 30) {
			lDropList = PIXE_OCEAN_DROP_30;
		}
		if (lDropSize > 30) {
			lDropList = PIXE_OCEAN_DROP_MANY;
		}
		drops [lDropList].Add(lNewFree);

		// 6) Move the cursor and header references to the new Drop location
		lCursor = lNewLocation + lDropSize;
		lHeader = lNewLocation;
	
		//Debug.Log ("MOVED SIZE = " + lDropSize);
		//Debug.Log ("MOVED HEADER = " + lHeader);
		//Debug.Log ("To: " + lHeader);
		//Debug.Log ("Cursor after move = " + lCursor);
		return;
	}

	private void createMolecule(long lLocation, long lHeader, string sName, string sType, string sValue, string sData, ref long lCursor)
	{
		long lOffset = 0; 

		if (ocean [lHeader, PIXE_OCEAN_MOLECULE_NAME] != "") {
			lOffset = Convert.ToInt64 (ocean[lHeader,PIXE_OCEAN_MOLECULE_VALUE]);
		}
		// Move the Drop if it has run out of free space
		if (ocean [lLocation, PIXE_OCEAN_MOLECULE_NAME] != "") {
			//long lOffset = Convert.ToInt64 (ocean[lHeader,PIXE_OCEAN_MOLECULE_VALUE]);

			//Debug.Log("Header before the move: "+lHeader);

			moveDrop(ref lCursor, ref lHeader);

			// Amend the write lLocation to match the new Drop
			//lLocation = lCursor;
			lLocation = lHeader;
			//long lOffset = Convert.ToInt64 (ocean[lHeader,PIXE_OCEAN_MOLECULE_VALUE]);
			lLocation += lOffset;
			//lLocation += Convert.ToInt64 (ocean[lHeader,PIXE_OCEAN_MOLECULE_VALUE]);
			//Debug.Log("The offset!!!!!!!!!! "+lOffset);
			//Debug.Log("Mol created after move: "+sName);
			//Debug.Log("Its header "+lHeader);
			//Debug.Log("At position: "+lLocation);
			//Debug.Log("******************");
		}
		// If the space is free, write to it
		ocean [lLocation, PIXE_OCEAN_MOLECULE_NAME] = sName;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_TYPE] = sType;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_VALUE] = sValue;
		ocean [lLocation, PIXE_OCEAN_MOLECULE_DATA] = sData;

		// Update the size of the Drop header to reflect new addtion

		/*
		long lUpdate = Convert.ToInt64 (ocean [lHeader, PIXE_OCEAN_MOLECULE_VALUE]);
		lUpdate = lUpdate + 1;
		ocean [lHeader, PIXE_OCEAN_MOLECULE_VALUE] = lUpdate.ToString ();
*/
		lOffset += 1;
		ocean [lHeader, PIXE_OCEAN_MOLECULE_VALUE] = lOffset.ToString ();



		//lCursor = lHeader;
		return;
	}

	private void createNested(ref long lCursor, long lParentHeader)
	{
		// Save the nested Element reference orgin & find somewhere to put the new Header
		long lOrigin = lCursor;
		lCursor = getDrop(PIXE_OCEAN_DROP_MIN);
		
		// Write the Header data
		createMolecule(
			lCursor, lCursor, 
			ocean[lOrigin,PIXE_OCEAN_MOLECULE_NAME], "H", "0", (lParentHeader - lCursor).ToString (), 
			ref lCursor);

		// Update the offset value in the parent element molecule to point to this Header
		ocean [lOrigin,PIXE_OCEAN_MOLECULE_VALUE] = (lCursor - lOrigin).ToString();
		return;
	}

	// Finds a record within a specified Drop
	private bool findMolecule(ref long lCursor, string sName)
	{
		// Save the original position of the cursor
		long lCursorReset = lCursor;
		
		// Move the cursor to the current Drop header...
		while (ocean[lCursor,PIXE_OCEAN_MOLECULE_TYPE] != "H") {
			lCursor--;
		}
		// ... and get the size of the Drop (to use for search area limit)
		long lLimit = Convert.ToInt64(ocean [lCursor, PIXE_OCEAN_MOLECULE_VALUE]);
		lLimit += lCursor;
		bool bFound = false;

		// Move the cursor through the Drop until the requested Molecule is found
		while (lCursor<lLimit) {
			if(ocean [lCursor, PIXE_OCEAN_MOLECULE_NAME] == sName) {
				bFound = true;
				break;
			}
			lCursor++;
		}
		if (bFound) {
			return true;
		}
		// If it's not found - reset the cursor to its original position
		lCursor = lCursorReset;
		return false;
	}
}
