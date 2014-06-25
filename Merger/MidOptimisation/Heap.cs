using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Heap : MonoBehaviour {

	// iFlag definitions (must be greater than 0)
	const int PIXE_PSML_UNSET_FLAG = 5;
	/*const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_WRITE_ELEMENT = 2;
	const int PIXE_PSML_READ_ATTRIBUTE = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;
*/
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_READ_ATTRIBUTE = 2;
	const int PIXE_PSML_WRITE_ELEMENT = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;

	const int PIXE_PSML_ATTRIBUTE = 5;
	const int PIXE_PSML_ELEMENT = 6;

	//const int PIXE_OCEAN_UNSET = 0;

	// Operation success/fail codes
	const int PIXE_OP_SUCCESSFUL = 1;
	const int PIXE_OP_FAIL = 0;
	const int PIXE_OP_FAIL_INVALID_PATH = -1;
	const int PIXE_OP_FAIL_INVALID_CURSOR_POSITION = -2;
	const int PIXE_OP_FAIL_DUPLICATE_RECORD = -3;
	const int PIXE_OP_FAIL_WRITE_ERROR = -4;
	const int PIXE_OP_FAIL_NO_FREE_SESSION = -5;
	const int PIXE_OP_FAIL_MEMORY_ERROR = -6;

	const int PIXE_OCEAN_LIST_DEFAULT_SIZE = 10;
	const int PIXE_SESSION_LIST_DEFAULT_SIZE = 50;
	const int PIXE_OCEAN_DEFAULT_MOLECULE_COUNT = 30000;
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


	public int initialise;
	public int write;
	public int read;
	public int move;
	public int freesession;
	public int createmolecule;
	public int createnested;
	public int findmolecule;
	public int navigatepath;
	public int createocean;
	public int preventdropoverlap;
	public int getdrop;
	public int movedrop;
	public int expandocean;

	

	void Start () {
		initialise = 0;
		write = 0;
		read = 0;
		move = 0;
		freesession = 0;
		createmolecule = 0;
		createnested = 0;
		findmolecule = 0;
		navigatepath = 0;
		createocean = 0;
		preventdropoverlap = 0;
		getdrop = 0;
		movedrop = 0;
		expandocean = 0;
	}

	/*
	 * INITIALISE: Needs update to handle multiple oceans
	 * */
	public void Initialise(ref int iSessionIndex, int iOceanIndex, ref int iFlags) {

		Profiler.BeginSample ("Initialise");

		initialise++;

		// If the ocean list doesn't already exist create one
		int i;
		if (oceanList == null) {
			oceanList = new List<List<Molecule>> ();
			
			for(i=0;i<PIXE_OCEAN_LIST_DEFAULT_SIZE;i++) {
				oceanList.Add(new List<Molecule>());
			}
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
		// If no iOceanIndex is provided - assign it to a new (empty) ocean
		if (iOceanIndex == PIXE_RESET) {
			createOcean (ref iOceanIndex, ref iFlags);
		} 
		// Step through session list and look for a free slot
		for (i=0; i<sessionList.Count; i++) {
			Session session = sessionList[i];
			// Attach the found session to the specified ocean (setting the cursor to its Home)
			if(!session.InUse) {
				session.InUse = true;
				session.Ocean = iOceanIndex;
				List<Molecule> ocean = oceanList[iOceanIndex];
				Molecule home = ocean[PIXE_OCEAN_HOME];
				session.Cursor = (int)home.Value;
				// Set privleges - according to iFlags - INCOMPLETE
				iSessionIndex = i;
				break;
			}
		}
		// Display an error if no free session is available
		if (i == sessionList.Count) {
			iSessionIndex = PIXE_RESET;
			UnityEngine.Debug.Log("ERROR = No free sessions are currently available. Please try again later.");
			iFlags = PIXE_OP_FAIL_NO_FREE_SESSION;
		}
		Profiler.EndSample ();
		return;
	}

	// IMPLEMENT LATER
	public int moveToHeader(ref Session session, ref List<Molecule> ocean) {

		int iCursor = session.Cursor;

		// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
		while(ocean[iCursor].Type != "H") {
			iCursor--;
		}
		return (int)ocean[iCursor].Value;

	}


	public bool hasElements(ref Session session, ref List<Molecule> ocean, int iType){
		int iCursor, iDropSize;
		iCursor = session.Cursor;
		bool hasElements = false;
		string sSearch;

		switch (iType) {
		case PIXE_PSML_ELEMENT:
			sSearch = "E";
			break;
		case PIXE_PSML_ATTRIBUTE:
			sSearch = "A";
			break;
		default:
			UnityEngine.Debug.Log("ERROR = Unknown Molecule type seached for.");
			return hasElements;
		}
		
		// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
		while(ocean[iCursor].Type != "H") {
			iCursor--;
		}
		// MAYBE USE OBJECTS - rather than strings, then no need for int conversion
		iDropSize = (Convert.ToInt32 (ocean[iCursor].Value));
		
		int iLimit = iCursor + iDropSize;
		while(iCursor < iLimit) {
			if(ocean[iCursor].Type == sSearch) {
				hasElements = true;
			}
			iCursor++;
		}
		return hasElements;
	}

	// Returns a Drops nested element names as a List of strings
	public List<string> getElements(ref Session session, ref List<Molecule> ocean, int iType)
	{
		int iCursor, iDropSize;
		List<string> elementList = new List<string>();
		iCursor = session.Cursor;
		string sSearch;

		switch (iType) {
		case PIXE_PSML_ELEMENT:
			sSearch = "E";
			break;
		case PIXE_PSML_ATTRIBUTE:
			sSearch = "A";
			break;
		default:
			UnityEngine.Debug.Log("ERROR = Unknown Molecule type seached for.");
			return null;
		}

		// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
		while(ocean[iCursor].Type != "H") {
			iCursor--;
		}
		// MAYBE USE OBJECTS - rather than strings, then no need for int conversion
		iDropSize = (Convert.ToInt32 (ocean[iCursor].Value));
	
		int iLimit = iCursor + iDropSize;
		while(iCursor < iLimit) {
			if(ocean[iCursor].Type == sSearch) {
				elementList.Add (ocean[iCursor].Name);
			}
			iCursor++;
		}
		return elementList;
	}

	public void Write(int iSessionIndex, string sPath, object oValue, ref int iFlags)
	{
		Profiler.BeginSample ("Write");
		write++;

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
						sPath, "H", 0, 0, ref iFlags);
					ocean[PIXE_OCEAN_HOME].Type = "LOADED";
				} else {
					UnityEngine.Debug.Log("ERROR = Invalid cursor position.");
					iFlags = PIXE_OP_FAIL_INVALID_CURSOR_POSITION;
				}
				// NOTE: Session cursor does not move in this case
				Profiler.EndSample ();
				return;
			}
			// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
			while(ocean[iCursor].Type != "H") {
				iCursor--;
			}
			// MAYBE USE OBJECTS - rather than strings, then no need for int conversion
			iDropSize = (Convert.ToInt32 (ocean[iCursor].Value));

			// Search for molecules with a matching name
			if(findMolecule(ref session, ref ocean, ref sPath)) {
				// If Attribute with that name already exists, overwrite the value
				if(iFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
					iCursor = session.Cursor;
					ocean[iCursor].Value = oValue;
					ocean[iCursor].Data = (oValue.GetType ()).ToString ();
					Profiler.EndSample ();
					return;
				}
				// Duplicate elements are not allowed
				else {
					UnityEngine.Debug.Log("ERROR = Cannot write. This element name already exists in this Drop: "+ sPath);
					iFlags = PIXE_OP_FAIL_DUPLICATE_RECORD;
					Profiler.EndSample ();
					return;
				}
			}
			// If it doesn't alread exist, create a Molecule for it. For elements:
			if (iFlags == PIXE_PSML_WRITE_ELEMENT) {
				createMolecule (
					ref session, ref ocean,
					(iCursor + iDropSize), iCursor, 
					sPath, "E", PIXE_OCEAN_UNSET, "", ref iFlags);

			} // For attributes: 
			else if (iFlags == PIXE_PSML_WRITE_ATTRIBUTE) {
				createMolecule (
					ref session, ref ocean,
					(iCursor + iDropSize), iCursor, 
					sPath, "A", oValue, (oValue.GetType ()).ToString (), ref iFlags);
			}
			// NOTE: Cursor is moved to newly written molecule by createMolecule()
		}
		Profiler.EndSample ();
		return;
	}

	public object Read(int iSessionIndex, string sPath, ref int iFlags) 
	{
		Profiler.BeginSample ("Read");
		read++;

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
				UnityEngine.Debug.Log("ERROR = Cannot read. Invalid cursor position");
				iFlags = PIXE_OP_FAIL_INVALID_CURSOR_POSITION;
				Profiler.EndSample ();
				return null;
			}
			else {
				// Move cursor onto requested attribute/element within the Drop
				if(findMolecule(ref session, ref ocean, ref sPath)) {
					// For successfully found elements - return true
					if (iFlags == PIXE_PSML_READ_ELEMENT) {
						Profiler.EndSample ();
						return true;
					}
					// For attributes - return the value (in correct data type)
					else if (iFlags == PIXE_PSML_READ_ATTRIBUTE) {
						// Need to switch to correct data type!!!!!!
						Profiler.EndSample ();
						return ocean[session.Cursor].Value;
					}
				}
				else {
					// If element is not found, return false
					if(iFlags == PIXE_PSML_READ_ELEMENT) {
						Profiler.EndSample ();
						return false;
					}
					// Return an error if att/element not found
					Profiler.EndSample ();
					return "Requested attribute not found";
				}
			}
		}
		else if (iOriginalFlags == PIXE_PSML_READ_ELEMENT){
			Profiler.EndSample ();
			return false;
		}
		Profiler.EndSample ();
		return "Requested attribute not found";
	}

	public string WhereAmI(int iSessionIndex) {

		// Retrieve the session cursor and correct ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];

		return ocean [session.Cursor].Name;

	}

	public bool Move(int iSessionIndex, string sDestination, ref int iFlags) 
	{
		Profiler.BeginSample ("Move");
		move++;

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
			iCursor = iCursor + (int)ocean[iCursor].Data;
			session.Cursor = iCursor;
		} 
		// Else, search for the destination attribute/element reference in the current Drop
		else {
			if(!findMolecule(ref session, ref ocean, ref sDestination)){
				UnityEngine.Debug.Log ("ERROR = Unable to move cursor. No matching record found in current location:" + sDestination);
				UnityEngine.Debug.Log(ocean[session.Cursor].Name);
				iFlags = PIXE_OP_FAIL_INVALID_PATH;
				Profiler.EndSample ();
				return false;
			}
			iCursor = session.Cursor;
			
			// When moving into nested elements - create a Drop Header if one doesn't already exist...
			if(ocean[iCursor].Type == "E") {
				if((Convert.ToInt32(ocean[iCursor].Value)) == PIXE_OCEAN_UNSET) {
					createNested (ref session, ref ocean, iHeader, ref iFlags); 
				}
				// ...or move the currsor to the correct header if it already exists.
				else {
					iCursor = iCursor + (int)ocean[iCursor].Value;
					session.Cursor = iCursor;
				}
			}
		}
		//UnityEngine.Debug.Log ("Cursor moved to: " + ocean [session.Cursor].Name);
		Profiler.EndSample ();
		return true;
	}
	
	public void freeSession(ref int iSessionIndex, ref int iFlags)
	{
		Profiler.BeginSample ("freeSession");
		freesession++;

		//Retrieve the session
		Session toFree = sessionList [iSessionIndex];
		
		// Reset all the session settings
		toFree.Cursor = PIXE_RESET;
		toFree.Ocean = PIXE_RESET;
		toFree.Privileges = PIXE_RESET;
		toFree.InUse = false;
		
		iSessionIndex = PIXE_RESET;
		Profiler.EndSample ();
		return;
	}

	// BASIC WRITE MOLECULE
	public void unsafeWrite(ref Session session, ref List<Molecule> ocean, object oName, object oType, object oValue, object oData, ref int iFlags)
	{
		// Retrieve the session cursor
		int iCursor = session.Cursor; 
		
		// Write the data to the ocean
		ocean[iCursor].Name = oName.ToString();
		ocean[iCursor].Type = oType.ToString();
		ocean[iCursor].Value = oValue;
		ocean[iCursor].Data = oData;
		return;
	}
	
	// BASIC READ MOLECULE
	public object unsafeRead(int iSession, ref int iFlags)
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
				UnityEngine.Debug.Log ("ERROR = Unable to read. Invalid data request.");
				iFlags = PIXE_OP_FAIL;
				break;
			}
		} 
		else {
			UnityEngine.Debug.Log("ERROR = Unable to read. Invalid cursor position. " + iReadIndex);
			iFlags = PIXE_OP_FAIL_INVALID_PATH;
		}
		return null;
	}

	private void createMolecule(ref Session session, ref List<Molecule> ocean, 
	                            int iLocation, int iHeader, string sName, 
	                            string sType, object oValue, object oData, ref int iFlags)
	{
		Profiler.BeginSample ("createMolecule");
		createmolecule++;

		int iOffset = 0; 

		// When writing the root node:
		if (ocean [iHeader].Name != null && ocean [iHeader].Name != "") {
			iOffset = (int)ocean[iHeader].Value;
		}
		// Move the Drop if it has run out of free space or bleeds into another drop's space
		if (ocean [PIXE_OCEAN_HOME].Type != "EMPTY") {
			preventDropOverlap (ref session, ref ocean, iLocation, iHeader);
		}
		if (iLocation >= (ocean.Count-1) ||
		    (ocean [iLocation].Name != "" && ocean [iLocation].Name != null)) {
			moveDrop(ref session, ref ocean, ref iHeader/*, ref iFlags*/);
			
			// Amend the write lLocation to match the new Drop
			iLocation = iHeader;					// Needed to lookup new header
			iLocation += iOffset;
		}

		if (ocean [iLocation].Name != "" && ocean [iLocation].Name != null) {
			UnityEngine.Debug.Log("ERROR = Molecule OVERLAP !!!!!!!!!! "+iLocation);
			UnityEngine.Debug.Log(ocean [iLocation].Name);
			iFlags = PIXE_OP_FAIL_WRITE_ERROR;
			Profiler.EndSample ();
			return;
		}
		// If the space is free, write to it
		ocean [iLocation].Name = sName;
		ocean [iLocation].Type = sType;
		ocean [iLocation].Value = oValue;
		ocean [iLocation].Data = oData;

		// Update the size of the Drop header to reflect new addtion
		iOffset += 1;
		ocean[iHeader].Value = iOffset;

		// Move the session cursor to the newly created Molecule
		session.Cursor = iLocation;
		Profiler.EndSample ();
		return;
	}
	
	private void createNested(ref Session session, ref List<Molecule> ocean, int iParentHeader, ref int iFlags)
	{
		Profiler.BeginSample ("createNested");
		createnested++;

		// Save the nested Element reference orgin & find somewhere to put the new Header
		int iOrigin = session.Cursor;
		int iCursor = getDrop(ref session, ref ocean, 5, PIXE_RESET, false);

		// Write the Header data (NOTE - session cursor is moved by createMolecule()
		Molecule mHeader = ocean [iOrigin];
		createMolecule(
			ref session, ref ocean,
			iCursor, iCursor, 
			mHeader.Name, "H", 0, (iParentHeader - iCursor), ref iFlags);   /// CHANGE 1 to 0 if wrong

		// Update the offset value in the parent element molecule to point to this Header
		mHeader.Value = (iCursor - iOrigin);
		Profiler.EndSample ();
		return;
	}

	// Finds a record within a specified Drop & moves the session cursor to it
	private bool findMolecule(ref Session session, ref List<Molecule> ocean, ref string sName)
	{
		Profiler.BeginSample ("findMolecule");
		findmolecule++;

		// Create a temp cursor to step through the Drop
		int iCursor = session.Cursor;
		//session.CursorReset = session.Cursor;

		// Move the temp cursor to the current Drop header...

		while (ocean[iCursor].Type != "H") {
			iCursor--;
		}

		/*
		while (ocean[session.Cursor].Type != "H") {
			session.Cursor--;
		}*/

		// ... and get the size of the Drop (to use for search area limit)
		int iLimit = (int)ocean[iCursor].Value;
		//int iLimit = (int)ocean[session.Cursor].Value;
		iLimit += iCursor;
		//iLimit += session.Cursor;
		bool bFound = false;

		// Move the cursor through the Drop until the requested Molecule is found
		while (iCursor < iLimit) {
			if(ocean[iCursor].Name == sName) {
				bFound = true;
				break;
			}
			iCursor++;
		}
		/*while (session.Cursor < iLimit) {
			if(ocean[session.Cursor].Name == sName) {
				bFound = true;
				break;
			}
			session.Cursor++;
		}*/
		// If it's found, move the session cursor to this location
		if (bFound) {
			session.Cursor = iCursor;
			Profiler.EndSample ();
			return true;
		}
		/*if (!bFound) {
			session.Cursor = session.CursorReset;
			Profiler.EndSample ();
			return false;
		}*/
		Profiler.EndSample ();
		//return true;
		return false;
	}

	/*
	 * NAVIGATE PATH:
	 * Moves the session cursor to the Node specified in the sPath paremeter.
	 * */
	private void navigatePath(ref Session session, ref List<Molecule> ocean, ref string sPath, ref int iFlags)
	{
		Profiler.BeginSample ("navigatePath");
		navigatepath++;

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
			session.Cursor = (int)ocean[PIXE_OCEAN_HOME].Value;

			// Attempt to move cursor to provided path
			int i=0, iDepth = sPathArray.GetLength(0);
			bool bFound = true;
			while(i<iDepth && bFound == true) {
				bFound = Move (session.ID, sPathArray[i], ref iFlags);
				i++;
			}
			if(i != iDepth || !bFound) {
				// Move cursor to any elements which don't have a header yet.
				if (iFlags == PIXE_PSML_WRITE_ELEMENT) {
					if (findMolecule(ref session, ref ocean, ref sLast)) {
						return;
					}
				}
				// If path is invalid - Reset cursor to original location
				session.Cursor = iCursorReset;
				UnityEngine.Debug.Log("ERROR = Invalid path provided.");
				iFlags = PIXE_OP_FAIL_INVALID_PATH;
			}
		}
		Profiler.EndSample ();
		return;
	}

	/*
	 * Creates an empty ocean.
	 * */
	private void createOcean(ref int iOceanIndex, ref int iFlags)
	{
		Profiler.BeginSample ("createOcean");

		createocean++;

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
				// Add and intialise all molecules 
				newOcean.Add (new Molecule ());
				newOcean[i].Name = "";
				newOcean[i].Type = "";
				newOcean[i].Value = "";
				newOcean[i].Data = "";
			}
			// Initialise the ocean home in the first free molecule (1)
			newOcean[PIXE_OCEAN_HOME].Name = "HOME";
			newOcean[PIXE_OCEAN_HOME].Type = "Empty";
			newOcean[PIXE_OCEAN_HOME].Value = PIXE_OCEAN_DEFAULT_HOME;
			newOcean[PIXE_OCEAN_HOME].Data = new List<int>[PIXE_OCEAN_DROP_COULMN_COUNT];

			// Initialise the drops array by creating a list in each cell
			List<int>[] lDrops = (List<int>[])newOcean[PIXE_OCEAN_HOME].Data;
			int iDropsSize = lDrops.GetLength (0);
			for(i=0;i<iDropsSize;i++) {
				lDrops[i] = new List<int>();
			}
			// As the ocean is empty, the first availble drop is at the end of the first (resevered for root) header
			// NOTE: The MANY DropList is one value which is the index of the start of the many block
			lDrops [PIXE_OCEAN_DROP_MANY].Add(6);
			newOcean[6].Data = PIXE_OCEAN_DROP_MANY;
		} 
		else {
			iFlags = PIXE_OP_FAIL_MEMORY_ERROR;
			UnityEngine.Debug.Log("ERROR = Unable to create ocean. Ocean list has not been intialised.");
			iOceanIndex = PIXE_RESET;
		}
		Profiler.EndSample ();
		return;
	}

	/*
	 * Clears the entire contents of a specified Ocean in the Ocean List
	 * */
	public void drainOcean(int iOceanIndex, ref int iFlags)
	{
		List<Molecule> toDrain = oceanList [iOceanIndex];
		toDrain.Clear ();
	}
	// THERE IS AN ISSUE HERE WITH TEST 
	private void preventDropOverlap(ref Session session, ref List<Molecule> ocean, int iIndex, int iHeader) 
	{
		Profiler.BeginSample ("preventDropOverlap");
		preventdropoverlap++;

		// Retreive the List holding the appropriately sized Drops & get the index of the last ite
		List<int>[] dropArray = (List<int>[])ocean[PIXE_OCEAN_HOME].Data;
		List<int> dropLookup = dropArray[PIXE_OCEAN_DROP_MANY];

		// Check the many list - amend start point of the many block if overlap has occurred there
		int iLast = dropLookup.Count - 1;
		if (iLast >= 0) {
			int iCurrentStart = dropLookup[iLast];
			if(iCurrentStart == iIndex) {
				dropLookup.RemoveAt(iLast);
				dropLookup.Add(iCurrentStart + 5);
				ocean[(iCurrentStart + 5)].Data = PIXE_OCEAN_DROP_MANY;
				Profiler.EndSample ();
				return;
			}
		}
		if (ocean [iIndex].Name != "") {
			Profiler.EndSample ();
			return;
		}
		if (ocean [iIndex].Data == "" || ocean [iIndex].Data == null) {
			Profiler.EndSample ();
			return;
		}



		int iDropList = (int)ocean [iIndex].Data;
		dropLookup = dropArray [iDropList];
		bool bFound = false;
		int i, iLimit = dropLookup.Count;
		for (i = 0; i < iLimit; i++) {
			if (dropLookup[i] == iIndex) {
				bFound = true;
				int iClash = iIndex;
				// Remove the free drop from the list
				dropLookup.Remove(iClash);
				// Modify the original free drop start point
				iClash += 5;
				bool bAddToList = true;
				// Update the lists to reflect this new change
				switch(iDropList) {
				case PIXE_OCEAN_DROP_5:
					bAddToList = false;
					break;
				case PIXE_OCEAN_DROP_10:
					iDropList = PIXE_OCEAN_DROP_5;
					ocean[iClash].Data=PIXE_OCEAN_DROP_5;
					break;
				case PIXE_OCEAN_DROP_15:
					iDropList = PIXE_OCEAN_DROP_10;
					ocean[iClash].Data=PIXE_OCEAN_DROP_10;
					break;
				case PIXE_OCEAN_DROP_20:
					iDropList = PIXE_OCEAN_DROP_15;
					ocean[iClash].Data=PIXE_OCEAN_DROP_15;
					break;
				case PIXE_OCEAN_DROP_25:
					iDropList = PIXE_OCEAN_DROP_20;
					ocean[iClash].Data=PIXE_OCEAN_DROP_20;
					break;
				case PIXE_OCEAN_DROP_30:
					iDropList = PIXE_OCEAN_DROP_25;
					ocean[iClash].Data=PIXE_OCEAN_DROP_25;
					break;
				default:
					bAddToList = false;
					break;
				}
				if(bAddToList) {
					dropLookup = dropArray [iDropList];
					dropLookup.Add(iClash);
				}
				// Return if the drop is found & dealt with
				if(bFound) {
					//UnityEngine.Debug.Log("Overlap prevented. Problem cell = "+iClash);
					Profiler.EndSample ();
					return;
				}
			}
		}
		UnityEngine.Debug.Log ("ERROR = Matching Drop Not Found");
		/*
		// Otherwise, check each drop list to ensure new write does not encroach into another drop
		int iDropList = PIXE_OCEAN_DROP_5;
		dropLookup = dropArray [iDropList];
		bool bFound = false;
		while (iDropList < PIXE_OCEAN_DROP_30) {
			//foreach (int drop in dropLookup) {
			int i, iLimit=dropLookup.Count;
			for (i = 0; i < iLimit; i++) {
				// If a clash occurs
				//if (drop == iIndex) {
				if (dropLookup[i] == iIndex) {
					bFound = true;
					int iClash = iIndex;
					// Remove the free drop from the list
					dropLookup.Remove(iClash);
					// Modify the original free drop start point
					iClash += 5;
					bool bAddToList = true;
					// Update the lists to reflect this new change
					switch(iDropList) {
					case PIXE_OCEAN_DROP_5:
						bAddToList = false;
						break;
					case PIXE_OCEAN_DROP_10:
						iDropList = PIXE_OCEAN_DROP_5;
						ocean[iClash].Value=PIXE_OCEAN_DROP_5;
						break;
					case PIXE_OCEAN_DROP_15:
						iDropList = PIXE_OCEAN_DROP_10;
						ocean[iClash].Value=PIXE_OCEAN_DROP_10;
						break;
					case PIXE_OCEAN_DROP_20:
						iDropList = PIXE_OCEAN_DROP_15;
						ocean[iClash].Value=PIXE_OCEAN_DROP_15;
						break;
					case PIXE_OCEAN_DROP_25:
						iDropList = PIXE_OCEAN_DROP_20;
						ocean[iClash].Value=PIXE_OCEAN_DROP_20;
						break;
					case PIXE_OCEAN_DROP_30:
						iDropList = PIXE_OCEAN_DROP_25;
						ocean[iClash].Value=PIXE_OCEAN_DROP_25;
						break;
					default:
						bAddToList = false;
						break;
					}
					if(bAddToList) {
						dropLookup = dropArray [iDropList];
						dropLookup.Add(iClash);
					}
					// Return if the drop is found & dealt with
					if(bFound) {
						//UnityEngine.Debug.Log("Overlap prevented. Problem cell = "+iClash);
						Profiler.EndSample ();
						return;
					}
				}
			}
			// Else, check the next drop list
			iDropList++;
			dropLookup = dropArray[iDropList];
		}
		*/
		Profiler.EndSample ();
		return;
	}

	// Finds a block of free space to store a new drop
	private int getDrop(ref Session session, ref List<Molecule> ocean, int iSize, int iHeader, bool bMove) 
	{
		Profiler.BeginSample ("getDrop");
		getdrop++;

		int iFoundDrop = PIXE_RESET;
		
		// Get the list holding the correct sized Drops and round up the Drop size
		int iDropList = PIXE_OCEAN_DROP_5;
		if (iSize < 0) {
			UnityEngine.Debug.Log("ERROR = Invalid requested Drop size");
			Profiler.EndSample ();
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
			iSize = (iSize + (5-iSize % 5));
			//UnityEngine.Debug.Log("Rounded up: "+iSize);
		}
		// Retreive the List holding the appropriately sized Drops & get the index of the last item
		List<int> dropLookup;
		int iLast;
		bool bDone = false;
		
		while (!bDone) {
			List<int>[] dropArray = (List<int>[])ocean[PIXE_OCEAN_HOME].Data;
			dropLookup = dropArray[iDropList];
			iLast = dropLookup.Count - 1;
			
			// If the list is not empty, allocate the last Drop then remove it from the list
			if (iLast >= 0) {
				iFoundDrop = dropLookup[iLast];
				dropLookup.RemoveAt(iLast);

				// Expand ocean if needed

				int iLastMolecule = ocean.Count -1;
				int iNewDropEnd = iFoundDrop + iSize;
				if(iNewDropEnd >= iLastMolecule) {
					expandOcean(ref ocean, iNewDropEnd);
					/*
					int i;
					for(i=iLastMolecule;i<iNewDropEnd;i++) {
						ocean.Add (new Molecule ());
						ocean[i].Name = "";
						ocean[i].Type = "";
						ocean[i].Value = "";
						ocean[i].Data = "";
					}*/
				}
				// If in the many: amend the start point of the many block
				if(iDropList == PIXE_OCEAN_DROP_MANY) { 
					dropArray [PIXE_OCEAN_DROP_MANY].Add(iFoundDrop + iSize);
					ocean[(iFoundDrop + iSize)].Data = PIXE_OCEAN_DROP_MANY;
				}
				bDone = true;
			}
			//If nothing is found, repeat the search in the Many list.
			iDropList = PIXE_OCEAN_DROP_MANY;
		}
		Profiler.EndSample ();
		ocean [iFoundDrop].Data = "";
		return iFoundDrop;
	}


	private void expandOcean(ref List<Molecule> ocean, int iNewDropEnd) {

		Profiler.BeginSample ("expandOcean");



		expandocean++;

		//UnityEngine.Debug.Log ("Expand ocean called (" + expandocean + ")");
		//var stopwatch = Stopwatch.StartNew ();

		//GC.Collect ();

		int iLastMolecule = ocean.Count-1;
		int iNewOceanEnd = PIXE_OCEAN_UNSET;

		// If ocean has less than 1000 molecules, double the size
		if (iLastMolecule <= (PIXE_OCEAN_DEFAULT_MOLECULE_COUNT*3)) {
			iNewOceanEnd = (int)(iLastMolecule * 2);
		}
		// If it has 1001-5000, increase size by 25%
		else if(iLastMolecule > (PIXE_OCEAN_DEFAULT_MOLECULE_COUNT*3) && iLastMolecule <= (PIXE_OCEAN_DEFAULT_MOLECULE_COUNT*1000)) {
			iNewOceanEnd = (int)(iLastMolecule * 1.25);
		}
		// If it has more than 5000, increase by 10%
		else if (iLastMolecule > PIXE_OCEAN_DEFAULT_MOLECULE_COUNT*1000) {
			 iNewOceanEnd = (int)(iLastMolecule * 1.1);
		}

		//int iNewOceanEnd = iLastMolecule + PIXE_OCEAN_DEFAULT_MOLECULE_COUNT;

		if (iNewOceanEnd < iNewDropEnd) {
			iNewOceanEnd = iNewDropEnd;
		}
		int i;
		for(i=iLastMolecule;i<iNewOceanEnd;i++) {
			ocean.Add (new Molecule ());
			ocean[i].Name = "";
			ocean[i].Type = "";
			ocean[i].Value = "";
			ocean[i].Data = "";
		}


		//UnityEngine.Debug.Log (expandocean +" Time taken: " + stopwatch.ElapsedMilliseconds);

		/*
		int i = (ocean.Count-1);
		int iNewEnd = i + PIXE_OCEAN_DEFAULT_MOLECULE_COUNT;
		for(i=i;i<iNewEnd;i++) {
			ocean.Add (new Molecule ());
			ocean[i].Name = "";
			ocean[i].Type = "";
			ocean[i].Value = "";
			ocean[i].Data = "";
		}
*/
		Profiler.EndSample ();
	}

	private void findHeader(ref List<Molecule> ocean, ref int iCursor) {
		Profiler.BeginSample ("findHeader");
		while (ocean[iCursor].Type != "H") {
			iCursor--;
		}
		Profiler.EndSample ();
		return;
	}

	private void moveDrop(ref Session session, ref List<Molecule> ocean, ref int iHeader) 
	{
		Profiler.BeginSample ("moveDrop");
		movedrop++;

		// Retreive the session cursor and move it to the Drop header
		int iCursor = session.Cursor;
		/*
		while (ocean[iCursor].Type != "H") {
			iCursor--;
		}*/
		findHeader (ref ocean, ref iCursor);
		iHeader = iCursor;

		// The required Drop size needs to be larger than the current Drop size:
		//Molecule mHeader = ocean [iHeader];
		//int iDropSize = (int)mHeader.Value;
		int iDropSize = (int)ocean[iHeader].Value;
		int iNewLocation = getDrop (ref session, ref ocean, (iDropSize+1), iHeader, true);

		if (ocean [iNewLocation].Name != "" && ocean [iNewLocation].Name != null) {
			UnityEngine.Debug.Log("WARNING = moveDrop OVERLAP detected!!!!!!!!!! "+iNewLocation);
			UnityEngine.Debug.Log("** " +ocean [iNewLocation].Name+" **");
			while (ocean [iNewLocation].Name != "" && ocean [iNewLocation].Name != null) {
				iNewLocation = getDrop (ref session, ref ocean, (iDropSize+1), iHeader, true);
			}
		}
		// 1) Copy the drop to the new location
		int i;
		for(i=0; i<iDropSize; i++) {
			//Molecule mCopyTo = ocean [iNewLocation+i];
			//Molecule mCopyFrom = ocean [iCursor+i];
			//mCopyTo.Name = String.Copy (mCopyFrom.Name);
			ocean [iNewLocation+i].Name = String.Copy (ocean [iCursor+i].Name);
			//mCopyTo.Type = String.Copy (mCopyFrom.Type);
			ocean [iNewLocation+i].Type = String.Copy (ocean [iCursor+i].Type);
			// Values
			//string sDataType = (mCopyFrom.Value.GetType()).ToString();
			string sDataType = (ocean [iCursor+i].Value.GetType()).ToString();
			switch(sDataType) {
			case "System.String":
				//mCopyTo.Value = String.Copy ((string)mCopyFrom.Value);
				ocean [iNewLocation+i].Value = String.Copy ((string)ocean [iCursor+i].Value);
				break;
			case "System.Int32":
				//mCopyTo.Value = mCopyFrom.Value;
				ocean [iNewLocation+i].Value = ocean [iCursor+i].Value;
				break;
			default:
				UnityEngine.Debug.Log("ERROR = Invalid data type found in Molecule (Data).");
				break;
			}
			// Data
			//sDataType = (mCopyFrom.Data.GetType()).ToString();
			sDataType = (ocean [iCursor+i].Data.GetType()).ToString();
			switch(sDataType) {
			case "System.String":
				//mCopyTo.Data = String.Copy ((string)mCopyFrom.Data);
				ocean[iNewLocation+i].Data = String.Copy ((string)ocean [iCursor+i].Data);
				break;
			case "System.Int32":
				//mCopyTo.Data = mCopyFrom.Data;
				ocean[iNewLocation+i].Data = ocean [iCursor+i].Data;
				break;
			default:
				UnityEngine.Debug.Log("ERROR = Invalid data type found in Molecule (Data).");
				break;
			}
		}
		// If the root node has been moved - update the root positon
		if(iHeader == (int)ocean[PIXE_OCEAN_HOME].Value) {
			ocean[PIXE_OCEAN_HOME].Value = iNewLocation;
		}
		// 2) Update all the offset information in the copied Header
		int iNewHeader = iNewLocation;
		int iCursorReset = iCursor;
			
		// Update the ofset to its parent (NOTE - the root offset always = 0
		if (iNewHeader != (int)ocean [PIXE_OCEAN_HOME].Value) {	
			iCursor = iCursor + (int)ocean [iCursor].Data;
			ocean [iNewHeader].Data = (iCursor - iNewHeader);
		}
		iCursor = iNewHeader + (int)ocean[iNewHeader].Data;
		session.Cursor = iCursor;

		// Update the offset in the Parent
		string sToFind = ocean[iNewHeader].Name;
		if (findMolecule (ref session, ref ocean, ref sToFind/*ocean [iNewHeader].Name*/)) {
			iCursor = session.Cursor; 
			ocean [iCursor].Value = (iNewHeader - iCursor);
		}
		else {
			UnityEngine.Debug.Log("MOLECULE NOT FOUND!!!!!!!!!!!!!!!!!!!");
		}
		iCursor = iCursorReset;

		// 3) Step through the new moved Drop and update any Element offset information
		for (i=0; i<iDropSize; i++) {
			if (ocean [(iNewLocation + i)].Type == "E") {
				int iNewElement = iNewLocation + i;
				int iOldElement = iHeader + i;

				// Only update offsets to elements with existing Headers
				if ((int)ocean [iOldElement].Value != PIXE_OCEAN_UNSET) {
					// Update the offsets in the Elements Header
					iCursor = iOldElement + (int)ocean[iOldElement].Value;
					ocean[iNewElement].Value = (iCursor - iNewElement);
					ocean[iCursor].Data = (iNewLocation -iCursor);
				}
			}
		}
		// 4) Delete the orginal Drop
		for (i=0; i<iDropSize; i++) {
			ocean [(iHeader + i)].Name = "";
			ocean [(iHeader + i)].Type = "";
			ocean [(iHeader + i)].Value = null;
			ocean [(iHeader + i)].Data = null;
		}
		// 5) Put the newly freed space into the drops array
		addToDrops (ref ocean, iDropSize, iHeader);

		// 6) Move the cursor and header references to the new Drop location
		session.Cursor = iNewLocation + iDropSize;
		iCursor = iNewLocation + iDropSize;
		iHeader = iNewLocation;
		Profiler.EndSample ();
		return;
	}
	private void addToDrops(ref List<Molecule> ocean, int iDropSize, int iLocation)
	{
		Profiler.BeginSample ("addToDrops");
		List<int>[] dropArray;

		// Allocate to the correctly sized drop list
		int iDropList = PIXE_OCEAN_DROP_5;
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
		// If it's bigger than 30, break it up into chunks of 5
		if (iDropSize > 30) {
			int iNumberOfBlocks = iDropSize / 5;
			int i;
			for(i=0;i<iNumberOfBlocks;i++) {
				dropArray = (List<int>[])ocean [PIXE_OCEAN_HOME].Data;
				dropArray [PIXE_OCEAN_DROP_5].Add (iLocation);
				ocean[iLocation].Data = PIXE_OCEAN_DROP_5;
				iLocation+=5;
			}
			Profiler.EndSample ();
			return;
		}	
		// Add the Drop to the selected list
		dropArray = (List<int>[])ocean [PIXE_OCEAN_HOME].Data;
		dropArray [iDropList].Add (iLocation);
		ocean [iLocation].Data = iDropList;
		Profiler.EndSample (); 
		return;
	}
}
