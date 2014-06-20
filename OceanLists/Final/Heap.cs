using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Heap : MonoBehaviour {

	// iFlag definitions (must be greater than 0)
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_WRITE_ELEMENT = 2;
	const int PIXE_PSML_READ_ATTRIBUTE = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;

	//const int PIXE_OCEAN_UNSET = 0;

	// Operation success/fail codes
	const int PIXE_OP_SUCCESSFUL = 1;
	const int PIXE_OP_FAIL_INVALID_PATH = 0;
	const int PIXE_OP_FAIL_INVALID_CURSOR_POSITION = -1;
	const int PIXE_OP_FAIL_DUPLICATE_RECORD = -1;

	const int PIXE_OCEAN_LIST_DEFAULT_SIZE = 10;
	const int PIXE_SESSION_LIST_DEFAULT_SIZE = 50;
	const int PIXE_OCEAN_DEFAULT_MOLECULE_COUNT = 100;
	const int PIXE_OCEAN_EMPTY = 0;
	const int PIXE_OCEAN_HOME = 0;
	const int PIXE_OCEAN_UNSET = 0;
	const int PIXE_OCEAN_DEFAULT_HOME = 1;			// The root node of the PIXE ocean

	const int PIXE_OCEAN_READ_MOLECULE_NAME = 0;
	const int PIXE_OCEAN_READ_MOLECULE_TYPE = 1;
	const int PIXE_OCEAN_READ_MOLECULE_VALUE = 2;
	const int PIXE_OCEAN_READ_MOLECULE_DATA = 3;

	// Drop allocation array definitions
	const int PIXE_OCEAN_DROP_5 = 0;
	const int PIXE_OCEAN_DROP_10 = 1;
	const int PIXE_OCEAN_DROP_15 = 2;
	const int PIXE_OCEAN_DROP_20 = 3;
	const int PIXE_OCEAN_DROP_25 = 4;
	const int PIXE_OCEAN_DROP_30 = 5;
	const int PIXE_OCEAN_DROP_MANY = 6;
	const int PIXE_OCEAN_DROP_COULMN_COUNT = 7;
	const int PIXE_OCEAN_DROP_MIN = 6;
	const int PIXE_OCEAN_DROP_MIN2 = 5;


	const int PIXE_RESET = -1;

	public List<List<Molecule>> oceanList;
	public List<Session> sessionList;
	public List<int>[] drops;

	List<List<int>[]> dropLists;
	
	void Start () {

	}

	/*
	 * INITIALISE: Needs update to handle multiple oceans
	 * */
	public void Initialise(ref int iSessionIndex, int iOceanIndex, int iFlags) {
		
		// If the ocean list doesn't already exist create one (with a matching drop list)
		int i;
		if (oceanList == null) {
			oceanList = new List<List<Molecule>> ();
			drops = new List<int>[PIXE_OCEAN_DROP_COULMN_COUNT];
			
			for(i=0;i<PIXE_OCEAN_LIST_DEFAULT_SIZE;i++) {
				oceanList.Add(new List<Molecule>());
			}
			// NEEDS FIX - DROP LIST FOR EACH OCEAN
			// Initialise the drops array by creating a list in each cell
			int x = drops.GetLength (0);
			for(i=0;i<x;i++) {
				drops[i] = new List<int>();
			}
			// As the ocean is empty, the first availble drop is at the end of the first (resevered for root) header
			// NOTE: The MANY DropList is one value which is the index of the start of the many block
			drops [PIXE_OCEAN_DROP_MANY].Add(6);
		}
		
		// If no iOceanIndex is provided - assign it to a new (empty) ocean
		if(iOceanIndex == PIXE_RESET) {
			createOcean(ref iOceanIndex);
		}
		// Likewise if the session list doesn't already exist create it
		if (sessionList == null) {
			sessionList = new List<Session> ();
			for(i=0; i<PIXE_SESSION_LIST_DEFAULT_SIZE; i++) {
				Session newSession = new Session();
				newSession.InUse = false;
				newSession.ID = i;
				sessionList.Add(newSession);
			}
		}
		// Step through session list and look for a free slot
		for (i=0; i<sessionList.Count; i++) {
			Session session = sessionList[i];
			if(!session.InUse) {
				session.InUse = true;
				session.Ocean = iOceanIndex;
				// Set cursor to ocean home
				List<Molecule> ocean = oceanList[iOceanIndex];
				Molecule home = ocean[PIXE_OCEAN_HOME];
				session.Cursor = (Convert.ToInt32(home.Value));
				// Set privleges - according to iFlags - INCOMPLETE
				iSessionIndex = i;
				break;
			}
		}
		// Display an error if no free session is available
		if (i == sessionList.Count) {
			iSessionIndex = PIXE_RESET;
			Debug.Log("ERROR = No free sessions are currently available. Please try again later.");
		}
		return;
	}

	public long Write(int iSessionIndex, string sPath, object oValue, int iFlags)
	{
		// Retrieve the session cursor and the referenced ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];

		// Move the session cursor to the requested Drop location
		navigatePath (ref session, ref ocean, ref sPath, ref iFlags);

		// If successful - retrieve the session pointer from the session array
		if (iFlags >= PIXE_OP_SUCCESSFUL) {
			int iCursor = session.Cursor;
			int iDropSize = 0;
			// CHECK PRIVILEDGES HERE - do they have write access? - return error 

			// If this is the first (root) element in an empty ocean:
			if(ocean[iCursor].Type == "" || ocean[iCursor].Type == null) {
				if (iFlags == PIXE_PSML_WRITE_ELEMENT && iCursor == PIXE_OCEAN_DEFAULT_HOME) {
					createMolecule (
						ref session, ref ocean,
						iCursor, iCursor,
						sPath, "H", "1", "0");
				} else {
					iFlags = PIXE_OP_FAIL_INVALID_CURSOR_POSITION;
					Debug.Log("ERROR = Invalid cursor position.");
				}
				// NOTE: Session cursor does not move in this case
				return iFlags;
			}
			// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
			while(ocean[iCursor].Type != "H") {
				iCursor--;
			}
			iDropSize = (Convert.ToInt32 (ocean[iCursor].Value));

			// Search for molecules with a matching name
			if(findMolecule(ref session, ref ocean, sPath)) {
				// If Attribute with that name already exists, overwrite the value
				if(iFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
					iCursor = session.Cursor;
					ocean[iCursor].Value = oValue.ToString();
					ocean[iCursor].Data = (oValue.GetType ()).ToString ();
					return iFlags;
				}
				// Duplicate elements are not allowed
				else {
					Debug.Log("ERROR = Cannot write. This element name already exists in this Drop: "+ sPath);
					iFlags = PIXE_OP_FAIL_DUPLICATE_RECORD;
					return iFlags;
				}
			}
			// If it doesn't alread exist, create a Molecule for it. For elements:
			if (iFlags == PIXE_PSML_WRITE_ELEMENT) {
				createMolecule (
					ref session, ref ocean,
					(iCursor + iDropSize), iCursor, 
					sPath, "E", PIXE_OCEAN_UNSET.ToString (), "");

			} // For attributes: 
			else if (iFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
				createMolecule (
					ref session, ref ocean,
					(iCursor + iDropSize), iCursor, 
					sPath, "A", oValue.ToString (), (oValue.GetType ()).ToString ());
			}
			// NOTE: Cursor is moved to newly written molecule by createMolecule()
		}
		return iFlags;
	}

	public object Read(int iSessionIndex, string sPath, int iFlags) 
	{
		// Retrieve the session cursor and the referenced ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];
		int iOriginalFlags = iFlags;

		// Move the ocean cursor to the requested Drop location
		navigatePath (ref session, ref ocean, ref sPath, ref iFlags);
		
		// If successful - Retrieve the session pointer from the session array
		if (iFlags >= PIXE_OP_SUCCESSFUL) {
			// CHECK PRIVILEDGES HERE - do they have write access? - return error if not

			// If it's on a FREE them throw an error - Cannot read from an empty molecule
			if(ocean[session.Cursor].Type == "") {
			   iFlags = PIXE_OP_FAIL_INVALID_CURSOR_POSITION;
			   Debug.Log("ERROR = Cannot read. Invalid cursor position");
			   return null;
			}
			else {
				// Move cursor onto requested attribute/element within the Drop
				if(findMolecule(ref session, ref ocean, sPath)) {
					// For successfully found elements - return true
					if (iFlags == PIXE_PSML_READ_ELEMENT) {
						return true;
					}
					// For attributes - return the value (in correct data type)
					else if (iFlags == PIXE_PSML_READ_ATTRIBUTE) {
						// Need to switch to correct data type!!!!!!
						return ocean[session.Cursor].Value;
					}
				}
				else {
					// If element is not found, return false
					if(iFlags == PIXE_PSML_READ_ELEMENT) {
						return false;
					}
					// Return an error if att/element not found
					return "Requested attribute not found";
				}
			}
		}
		else if (iOriginalFlags == PIXE_PSML_READ_ELEMENT){
			return false;
		}
		return "Requested attribute not found";
	}

	public bool Move(int iSessionIndex, string sDestination) 
	{
		// Retrieve the session cursor and correct ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];
		int iCursor = session.Cursor;
		
		// Move the cursor to the Drop Header molecule
		while (ocean[iCursor].Type != "H") {
			iCursor--;
		}
		int iHeader = iCursor;				// Save this header location (for use in child Header)
		
		// Move cursor to parent Header if desitination is ".."
		if (sDestination == "..") {
			iCursor = iCursor + (Convert.ToInt32 (ocean[iCursor].Data));
			session.Cursor = iCursor;
		} 
		// Else, search for the destination attribute/element reference in the current Drop
		else {
			if(!findMolecule(ref session, ref ocean, sDestination)){
				Debug.Log ("ERROR = Unable to move cursor. No matching record found in current location:" + sDestination);
				return false;
			}
			iCursor = session.Cursor;
			
			// When moving into nested elements - create a Drop Header if one doesn't already exist...
			if(ocean[iCursor].Type == "E") {
				if((Convert.ToInt32(ocean[iCursor].Value)) == PIXE_OCEAN_UNSET) {
					createNested (ref session, ref ocean, iHeader); 
				}
				// ...or move the currsor to the correct header if it already exists.
				else {
					iCursor = iCursor + (Convert.ToInt32(ocean[iCursor].Value));
					session.Cursor = iCursor;
				}
			}
		}
		return true;
	}
	
	public void freeSession(ref int iSessionIndex)
	{
		//Retrieve the session
		Session toFree = sessionList [iSessionIndex];
		
		// Reset all the session settings
		toFree.Cursor = PIXE_RESET;
		toFree.Ocean = PIXE_RESET;
		toFree.Privileges = PIXE_RESET;
		toFree.InUse = false;
		
		iSessionIndex = PIXE_RESET;
		return;
	}

	// BASIC WRITE MOLECULE
	public void unsafeWrite(ref Session session, ref List<Molecule> ocean, object oName, object oType, object oValue, object oData)
	{
		// Retrieve the session cursor
		int iCursor = session.Cursor; 
		
		// Write the data to the ocean
		ocean[iCursor].Name = oName.ToString();
		ocean[iCursor].Type = oType.ToString();
		ocean[iCursor].Value = oValue.ToString();
		ocean[iCursor].Data = oData.ToString();
		return;
	}
	
	// BASIC READ MOLECULE
	public object unsafeRead(int iSession, int iFlags)
	{
		// Retrieve the session cursor
		int iReadIndex = sessionList [iSession].Cursor; 
		
		// Retreive the ocean being referenced by the cursor
		int iOceanIndex = sessionList [iSession].Ocean;
		List<Molecule> ocean = oceanList [iOceanIndex];
		
		// Read the data
		if (iOceanIndex >= 0 && iOceanIndex < ocean.Count) {
			switch (iFlags) {
			case PIXE_OCEAN_READ_MOLECULE_NAME:
				return ocean [iReadIndex].Name;
				break;
			case PIXE_OCEAN_READ_MOLECULE_TYPE:
				return ocean [iReadIndex].Type;
				break;
			case PIXE_OCEAN_READ_MOLECULE_VALUE:
				return ocean [iReadIndex].Value;
				break;
			case PIXE_OCEAN_READ_MOLECULE_DATA:
				return ocean [iReadIndex].Data;
				break;
			default:
				Debug.Log ("ERROR = Unable to read. Invalid data request.");
				break;
			}
		} 
		else {
			Debug.Log("ERROR = Unable to read. Invalid cursor position. " + iReadIndex);
		}
		return null;
	}

	private void createMolecule(ref Session session, ref List<Molecule> ocean, 
	                            int iLocation, int iHeader, string sName, 
	                            string sType, string sValue, string sData)
	{
		int iOffset = 0; 

		//Molecule mHeader = ocean [iHeader];
		if (ocean [iHeader].Name != null && ocean [iHeader].Name != "") {
			iOffset = (Convert.ToInt32 (ocean[iHeader].Value));
		}

		// Move the Drop if it has run out of free space or bleeds into another 
		preventDropOverlap (ref session, ref ocean, iLocation, iHeader);
		if (ocean [iLocation].Name != "" && ocean [iLocation].Name != null) {
			moveDrop(ref session, ref ocean, ref iHeader);
			
			// Amend the write lLocation to match the new Drop
			iLocation = iHeader;				// NEED TO LOOKUP NEW HEADER
			iLocation += iOffset;
			//mHeader = ocean[iHeader];
		}
		// If the space is free, write to it
		//Molecule mCurrent = ocean [iLocation];
		ocean [iLocation].Name = sName;
		ocean [iLocation].Type = sType;
		ocean [iLocation].Value = sValue;
		ocean [iLocation].Data = sData;

		// Update the size of the Drop header to reflect new addtion
		iOffset += 1;
		ocean[iHeader].Value = iOffset.ToString();

		// Move the session cursor to the newly created Molecule
		session.Cursor = iLocation;
		return;
	}
	
	private void createNested(ref Session session, ref List<Molecule> ocean, int iParentHeader)
	{
		// Save the nested Element reference orgin & find somewhere to put the new Header
		int iOrigin = session.Cursor;
		int iCursor = getDrop(ref session, ref ocean, 5, PIXE_RESET, false);

		// Write the Header data (NOTE - session cursor is moved by createMolecule()
		Molecule mHeader = ocean [iOrigin];
		createMolecule(
			ref session, ref ocean,
			iCursor, iCursor, 
			mHeader.Name, "H", "1", (iParentHeader - iCursor).ToString ());   /// CHANGE 1 to 0 if wrong

		// Update the offset value in the parent element molecule to point to this Header
		mHeader.Value = (iCursor - iOrigin).ToString();
		return;
	}

	// Finds a record within a specified Drop & moves the session cursor to it
	private bool findMolecule(ref Session session, ref List<Molecule> ocean, string sName)
	{
		// Create a temp cursor to step through the Drop
		int iCursor = session.Cursor;

		// Move the temp cursor to the current Drop header...
		while (ocean[iCursor].Type != "H") {
			iCursor--;
		}
		// ... and get the size of the Drop (to use for search area limit)
		int iLimit = (Convert.ToInt32 (ocean[iCursor].Value));
		iLimit += iCursor;
		bool bFound = false;

		// Move the cursor through the Drop until the requested Molecule is found
		while (iCursor < iLimit) {
			if(ocean[iCursor].Name == sName) {
				bFound = true;
				break;
			}
			iCursor++;
		}
		// If it's found, move the session cursor to this location
		if (bFound) {
			session.Cursor = iCursor;
			return true;
		}
		return false;
	}

	/*
	 * NAVIGATE PATH:
	 * Moves the session cursor to the Node specified in the sPath paremeter.
	 * */
	private void navigatePath(ref Session session, ref List<Molecule> ocean, ref string sPath, ref int iFlags)
	{
		// Remove any trailing/leading spaces
		sPath = sPath.Trim();
		
		// For absolute paths - Remove the "psml//:" marker & tokenise the path at each "/"
		if (sPath.StartsWith ("psml://")) {
			sPath = sPath.Remove(0,7);
			string[] sPathArray = sPath.Split ('/');
			string sLast = "";
			
			// Amend the path to the actual attribute/element to write/read (i.e. the last one)
			sPath = sPathArray[(sPathArray.GetLength(0)-1)];
			
			// When writing, set last path string to the parent
			if(iFlags == PIXE_PSML_WRITE_ELEMENT || iFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
				sLast = sPathArray[sPathArray.Length-1];
				Array.Resize(ref sPathArray, sPathArray.Length-1);
			}
			// Save current cursor location then set the cursor to the root node of the ocean
			int iCursorReset = session.Cursor;
			session.Cursor = (Convert.ToInt32(ocean[PIXE_OCEAN_HOME].Value));

			// Attempt to move cursor to provided path
			int i=0, iDepth = sPathArray.GetLength(0);
			bool bFound = true;
			while(i<iDepth && bFound == true) {
				bFound = Move (session.ID, sPathArray[i]);
				i++;
			}
			if(i != iDepth || !bFound) {
				// Move cursor to any elements which don't have a header yet.
				if (iFlags == PIXE_PSML_WRITE_ELEMENT) {
					if (findMolecule(ref session, ref ocean, sLast)) {
						return;
					}
				}
				// If path is invalid - Reset cursor to original location
				session.Cursor = iCursorReset;
				iFlags = PIXE_OP_FAIL_INVALID_PATH;
				Debug.Log("ERROR = Invalid path provided.");
			}
		}
		return;
	}

	/*
	 * Creates an empty ocean.
	 * */
	private void createOcean(ref int iOceanIndex)
	{
		List<Molecule> newOcean = null;
		int i, iOceanCount, iMoleculeCount;

		if (oceanList != null) {
			// Find the first empty Ocean in the list & assign the index
			iOceanCount = oceanList.Count;
			for (i=0; i<iOceanCount; i++) {
				newOcean = oceanList [i];
				iMoleculeCount = newOcean.Count;
				if (iMoleculeCount == PIXE_OCEAN_EMPTY) {
					break;
				}
			}
			iOceanIndex = i;

			// Fill the Ocean with default min number of Molecules
			for (i=0; i<PIXE_OCEAN_DEFAULT_MOLECULE_COUNT; i++) {
				// NOTE: All Molecule variables are automatically initialised as null
				newOcean.Add (new Molecule ());
			}
			// Initialise the ocean home in the first free molecule (1)
			newOcean[PIXE_OCEAN_HOME].Name = "HOME";
			newOcean[PIXE_OCEAN_HOME].Value = PIXE_OCEAN_DEFAULT_HOME.ToString();
		} 
		else {
			Debug.Log("ERROR = Unable to create ocean. Ocean list has not been intialised.");
			iOceanIndex = PIXE_RESET;
		}
		return;
	}


	/*
	 * Clears the entire contents of a specified Ocean in the Ocean List
	 * */
	public void drainOcean(int iOceanIndex)
	{
		List<Molecule> toDrain = oceanList [iOceanIndex];
		toDrain.Clear ();
	}

	// WILL NEED MODIFYING
	private void preventDropOverlap(ref Session session, ref List<Molecule> ocean, int iIndex, int iHeader) {

		// Retreive the List holding the appropriately sized Drops & get the index of the last item
		List<int> dropLookup = drops[PIXE_OCEAN_DROP_MANY];
		// Check the many list
		int iLast = dropLookup.Count - 1;
		
		// Amend the start point of the many block if overlap has occurred
		if (iLast >= 0) {
			int iCurrentStart = dropLookup[iLast];
			if(iCurrentStart == iIndex) {
				dropLookup.RemoveAt(iLast);
				dropLookup.Add(iCurrentStart + 5);
				return;
			}
		}

		// Get the the Drop size & appropriate list
		if (ocean [iHeader].Value == "" || ocean [iHeader].Value == null) {
			return;
		}
		int iDropSize = Convert.ToInt32 (ocean [iHeader].Value);
		int iDropList = PIXE_OCEAN_DROP_5;
		if (iDropSize >= 0 && iDropSize <= 5) {
			iDropList = PIXE_OCEAN_DROP_5;
		} else if (iDropSize > 5 && iDropSize <= 10) {
			iDropList = PIXE_OCEAN_DROP_10;
		} else if (iDropSize > 10 && iDropSize <= 15) {
			iDropList = PIXE_OCEAN_DROP_15;
		} else if (iDropSize > 15 && iDropSize <= 20) {
			iDropList = PIXE_OCEAN_DROP_20;
		} else if (iDropSize > 20 && iDropSize <= 25) {
			iDropList = PIXE_OCEAN_DROP_25;
		} else if (iDropSize > 25 && iDropSize <= 30) {
			iDropList = PIXE_OCEAN_DROP_30;
		} else if (iDropSize > 30) {
			iDropList = PIXE_OCEAN_DROP_MANY;
		} 
		dropLookup = drops [iDropList];


		if (iDropList != PIXE_OCEAN_DROP_MANY) {
			//Debug.Log("GOT HERE!!! "+iDropSize );

			foreach (int drop in dropLookup) {
				// If a clash occurs
				if (drop == iIndex) {
					int iClash = iIndex;
					// Remove the free drop from the list
					dropLookup.Remove(iClash);
					// Resize the free drop and put in correct list
					iClash += 5;
					switch(iDropList) {
					case PIXE_OCEAN_DROP_10:
						iDropList = PIXE_OCEAN_DROP_5;
						dropLookup = drops [iDropList];
						dropLookup.Add(iClash);
						Debug.Log("Drop found in "+iDropList);
						break;
					case PIXE_OCEAN_DROP_15:
						iDropList = PIXE_OCEAN_DROP_10;
						dropLookup = drops [iDropList];
						dropLookup.Add(iClash);
						Debug.Log("Drop found in "+iDropList);
						break;
					case PIXE_OCEAN_DROP_20:
						iDropList = PIXE_OCEAN_DROP_15;
						dropLookup = drops [iDropList];
						dropLookup.Add(iClash);
						Debug.Log("Drop found in "+iDropList);
						break;
					case PIXE_OCEAN_DROP_25:
						iDropList = PIXE_OCEAN_DROP_20;
						dropLookup = drops [iDropList];
						dropLookup.Add(iClash);
						Debug.Log("Drop found in "+iDropList);
						break;
					case PIXE_OCEAN_DROP_30:
						iDropList = PIXE_OCEAN_DROP_25;
						dropLookup = drops [iDropList];
						dropLookup.Add(iClash);
						Debug.Log("Drop found in "+iDropList);
						break;
					default:
						Debug.Log("No drop found in "+iDropList);
						break;
					}
					return;
				}
			}
			return;
		}
		return;
	}

	// Finds a block of free space to store a new drop
	private int getDrop(ref Session session, ref List<Molecule> ocean, int iSize, int iHeader, bool bMove) {
		
		int iFoundDrop = PIXE_RESET;
		
		// Get the list holding the correct sized Drops and round up the Drop size
		int iDropList = PIXE_OCEAN_DROP_5;
		if (iSize < 0) {
			Debug.Log("ERROR = Invalid requested Drop size");
			return PIXE_RESET;
		}
		if (iSize >= 0 && iSize <= 5) {
			iDropList = PIXE_OCEAN_DROP_5;
			iSize = 5;
		}
		if (iSize > 5 && iSize <= 10) {
			iDropList = PIXE_OCEAN_DROP_10;
			iSize = 10;
		}
		if (iSize > 10 && iSize <= 15) {
			iDropList = PIXE_OCEAN_DROP_15;
			iSize = 15;
		}
		if (iSize > 15 && iSize <= 20) {
			iDropList = PIXE_OCEAN_DROP_20;
			iSize = 20;
		}
		if (iSize > 20 && iSize <= 25) {
			iDropList = PIXE_OCEAN_DROP_25;
			iSize = 25;
		}
		if (iSize > 25 && iSize <= 30) {
			iDropList = PIXE_OCEAN_DROP_30;
			iSize = 30;
		}
		if (iSize > 30) {
			iDropList = PIXE_OCEAN_DROP_MANY;
			iSize = (iSize + iSize % 5);
			Debug.Log("Rounded up: "+iSize);
		}
		// Retreive the List holding the appropriately sized Drops & get the index of the last item
		List<int> dropLookup;
		int iLast;
		bool bDone = false;
		
		while (!bDone) {
			dropLookup = drops [iDropList];
			iLast = dropLookup.Count - 1;
			
			// If the list is not empty, allocate the last Drop then remove it from the list
			if (iLast >= 0) {
				iFoundDrop = dropLookup[iLast];
				dropLookup.RemoveAt(iLast);

				// If in the many: 
				if(iDropList == PIXE_OCEAN_DROP_MANY) {
					// Expand ocean if needed
					int iMoleculeCount = ocean.Count;
					iMoleculeCount = iMoleculeCount - iFoundDrop;
					if((iMoleculeCount - iSize) < 5) {
						int i;
						for(i=0;i<(iSize+5);i++) {
							ocean.Add (new Molecule ());
						}
					} // Then amend the start point of the many block
					drops [PIXE_OCEAN_DROP_MANY].Add(iFoundDrop + iSize);
				}
				bDone = true;
			}
			/* 
			 * If nothing is found, repeat the search in the Many list.
			 * If the Many list has already been search - make the ocean bigger then try again.
			 */
			if(iDropList == PIXE_OCEAN_DROP_MANY) {
				// TO DO:
				// INCREASE THE SIZE OF THE OCEAN
				// THIS FUNCTION CALL SHOULD ADD FREE TO MANY
			}
			iDropList = PIXE_OCEAN_DROP_MANY;
		}
		return iFoundDrop;
	}

	private void moveDrop(ref Session session, ref List<Molecule> ocean, /*ref long iCursor,*/ ref int iHeader) 
	{
		// Retreive the session cursor and move it to the Drop header
		int iCursor = session.Cursor;
		while (ocean[iCursor].Type != "H") {
			iCursor--;
		}
		iHeader = iCursor;

		// The required Drop size needs to be larger than the current Drop size:
		Molecule mHeader = ocean [iHeader];
		int iDropSize = (Convert.ToInt32 (mHeader.Value));
		int iNewLocation = getDrop (ref session, ref ocean, (iDropSize+1), iHeader, true);

		// 1) Copy the drop to the new location
		int i;
		for(i=0; i<iDropSize; i++) {
			Molecule mCopyTo = ocean [iNewLocation+i];
			Molecule mCopyFrom = ocean [iCursor+i];
			mCopyTo.Name = String.Copy (mCopyFrom.Name);
			mCopyTo.Type = String.Copy (mCopyFrom.Type);
			mCopyTo.Value = String.Copy (mCopyFrom.Value);
			mCopyTo.Data = String.Copy (mCopyFrom.Data);
		}
		// If the root node has been moved - update the root positon
		if(iHeader == (Convert.ToInt32(ocean[PIXE_OCEAN_HOME].Value))) {
			ocean[PIXE_OCEAN_HOME].Value = iNewLocation.ToString();
		}
		// 2) Update all the offset information in the copied Header
		int iNewHeader = iNewLocation;
		//int iOldHeader = iHeader;
		int iCursorReset = iCursor;
			
		// Update the ofset to its parent (NOTE - the root offset always = 0
		if (iNewHeader != (Convert.ToInt32 (ocean [PIXE_OCEAN_HOME].Value))) {	
			iCursor = iCursor + (Convert.ToInt32 (ocean [iCursor].Data));
			ocean [iNewHeader].Data = (iCursor - iNewHeader).ToString ();
		}
		iCursor = iNewHeader + (Convert.ToInt32(ocean[iNewHeader].Data));
		session.Cursor = iCursor;

		// Update the offset in the Parent
		if (findMolecule (ref session, ref ocean, ocean [iNewHeader].Name)) {
			iCursor = session.Cursor; // THIS MAY FUCK THINGS UP
			ocean [iCursor].Value = (iNewHeader - iCursor).ToString();
		}
		else {
			Debug.Log("NOT FOUND!!!!!!!!!!!!!!!!!!!");
		}
		iCursor = iCursorReset;

		// 3) Step through the new moved Drop and update any Element offset information
		for (i=0; i<iDropSize; i++) {
			if (ocean [(iNewLocation + i)].Type == "E") {
				int iNewElement = iNewLocation + i;
				int iOldElement = iHeader + i;

				// Only update offsets to elements with existing Headers
				if (ocean [iOldElement].Value != PIXE_OCEAN_UNSET.ToString ()) {

					// Update the offsets in the Elements Header
					iCursor = iOldElement + (Convert.ToInt32(ocean[iOldElement].Value));
					ocean[iNewElement].Value = (iCursor - iNewElement).ToString();
					ocean[iCursor].Data = (iNewLocation -iCursor).ToString();
				}	
			}
		}
		// 4) Delete the orginal Drop
		for (i=0; i<iDropSize; i++) {
			ocean [(iHeader + i)].Name = "";
			ocean [(iHeader + i)].Type = "";
			ocean [(iHeader + i)].Value = "";
			ocean [(iHeader + i)].Data = "";
		}
		// 5) Put the newly freed space into the drops array
		int iDropList = PIXE_OCEAN_DROP_5;
		int iNewFree = iHeader;
		if (iDropSize >= 0 && iDropSize <= 5) {
			iDropList = PIXE_OCEAN_DROP_5;
		}
		if (iDropSize > 5 && iDropSize <= 10) {
			iDropList = PIXE_OCEAN_DROP_10;
		}
		if (iDropSize > 10 && iDropSize <= 15) {
			iDropList = PIXE_OCEAN_DROP_15;
		}
		if (iDropSize > 15 && iDropSize <= 20) {
			iDropList = PIXE_OCEAN_DROP_20;
		}
		if (iDropSize > 20 && iDropSize <= 25) {
			iDropList = PIXE_OCEAN_DROP_25;
		}
		if (iDropSize > 25 && iDropSize <= 30) {
			iDropList = PIXE_OCEAN_DROP_30;
		}
		if (iDropSize > 30) {
			iDropList = PIXE_OCEAN_DROP_MANY;
		}
		drops [iDropList].Add (iNewFree);
			
		// 6) Move the cursor and header references to the new Drop location
		session.Cursor = iNewLocation + iDropSize;
		iCursor = iNewLocation + iDropSize;
		iHeader = iNewLocation;
		return;
	}
}
