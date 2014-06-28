#define PIXE_DEBUG_LOG
//#undef PIXE_DEBUG_LOG

//#define PIXE_DEBUG_PROFILER
#undef PIXE_DEBUG_PROFILER

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Heap : MonoBehaviour {

	// Reference to the PIXE definitions script
	public Constants PIXE;

	// The ocean and sessions lists
	public List<List<Molecule>> oceanList;
	public List<Session> sessionList;

	/*
	 * API PUBLIC METHODS:
	 * - Initialise()
	 * - Write()
	 * - Read()
	 * - Move()
	 * - WhereAmI()
	 * - freeSession()
	 * */

	public void Initialise(ref int iSessionIndex, int iOceanIndex, ref int iFlags) {

		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("Initialise");
		#endif

		// If the ocean list doesn't already exist create one
		int i;
		if (oceanList == null) {
			oceanList = new List<List<Molecule>> ();
			for(i=0;i<PIXE.OCEAN_LIST_DEFAULT_SIZE;i++) {
				oceanList.Add(new List<Molecule>());
			}
		}
		// Likewise if the session list doesn't already exist create it
		if (sessionList == null) {
			sessionList = new List<Session> ();
			for(i=0; i<PIXE.SESSION_LIST_DEFAULT_SIZE; i++) {
				Session newSession = new Session();
				newSession.InUse = false;
				newSession.ID = i;
				sessionList.Add(newSession);
			}
		}
		// If no iOceanIndex is provided (ie iOceanIndex = -1) - assign it to a new (empty) ocean
		if (iOceanIndex == PIXE.OCEAN_NEW) {
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
				Molecule home = ocean[PIXE.OCEAN_HOME];
				session.Cursor = (int)home.Value;
				// Set privleges - according to iFlags - INCOMPLETE
				iSessionIndex = i;
				break;
			}
		}
		// Flag an error if no free session is available
		if (i == sessionList.Count) {
			iSessionIndex = PIXE.RESET;
			iFlags = PIXE.OP_FAIL_NO_FREE_SESSION;
			#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("ERROR = No free sessions are currently available. Please try again later.");
			#endif
		}
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		return;
	}

	public void Write(int iSessionIndex, string sPath, object oValue, ref int iFlags)
	{
		#if (PIXE_DEBUG_PROFILER)
		Profiler.BeginSample ("Write");
		#endif
		
		// Retrieve the session cursor and the referenced ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];
		
		// Move the session cursor to the requested Drop location
		navigatePath (ref session, ref ocean, ref sPath, ref iFlags);
		
		// If successful - retrieve the session pointer from the session array
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			int iCursor = session.Cursor;
			int iDropSize = 0;
			// CHECK PRIVILEDGES HERE (stored in iFlags) - do they have write access?
			
			// If this is the first (root) element in an empty ocean:
			if(ocean[iCursor].Type == "" || ocean[iCursor].Type == null) {
				if (iFlags == PIXE.PSML_WRITE_ELEMENT && iCursor == PIXE.OCEAN_DEFAULT_HOME) {
					createMolecule (
						ref session, ref ocean,
						iCursor, iCursor,
						sPath, "H", 0, 0, ref iFlags);
					ocean[PIXE.OCEAN_HOME].Type = "LOADED";
				} else {
					#if (PIXE_DEBUG_LOG)
					UnityEngine.Debug.Log("ERROR = Invalid cursor position.");
					#endif
					iFlags = PIXE.OP_FAIL_INVALID_CURSOR_POSITION;
				}
				// NOTE: Session cursor does not move in this case
				#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
				#endif
				return;
			}
			// Move the cursor to the current Drop header - get Drop size & add the new Att/El to the end
			findHeader(ref ocean, ref iCursor);
			iDropSize = (int)ocean[iCursor].Value;

			// Search for molecules with a matching name
			if(findMolecule(ref session, ref ocean, ref sPath, iCursor)) {
				// If Attribute with that name already exists, overwrite the value
				if(iFlags == PIXE.PSML_WRITE_ATTRIBUTE) {
					//UnityEngine.Debug.Log("WRITTEN YO - "+oValue);
					iCursor = session.Cursor;
					ocean[iCursor].Value = oValue;
					ocean[iCursor].Data = (oValue.GetType ()).ToString ();
					#if (PIXE_DEBUG_PROFILER)
					Profiler.EndSample ();
					#endif
					return;
				}
				// Duplicate elements are not allowed
				else {
					#if (PIXE_DEBUG_LOG)
					UnityEngine.Debug.Log("ERROR = Cannot write. This element name already exists in this Drop: "+ sPath);
					#endif
					iFlags = PIXE.OP_FAIL_DUPLICATE_RECORD;
					#if (PIXE_DEBUG_PROFILER)
					Profiler.EndSample ();
					#endif
					return;
				}
			}
			// If it doesn't alread exist, create a Molecule for it. For elements:
			if (iFlags == PIXE.PSML_WRITE_ELEMENT) {
				createMolecule (
					ref session, ref ocean,
					(iCursor + iDropSize), iCursor, 
					sPath, "E", PIXE.OCEAN_UNSET, "", ref iFlags);
				
			} // For attributes: 
			else if (iFlags == PIXE.PSML_WRITE_ATTRIBUTE) {
				createMolecule (
					ref session, ref ocean,
					(iCursor + iDropSize), iCursor, 
					sPath, "A", oValue, (oValue.GetType ()).ToString (), ref iFlags);
			}
			// NOTE: Cursor is moved to newly written molecule by createMolecule()
		}
		#if (PIXE_DEBUG_PROFILER)
		Profiler.EndSample ();
		#endif
		return;
	}

	public object Read(int iSessionIndex, string sPath, ref int iFlags) 
	{
		#if (PIXE_DEBUG_PROFILER)
		Profiler.BeginSample ("Read");
		#endif
		
		// Retrieve the session cursor and the referenced ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];
		int iOriginalFlags = iFlags;
		
		// Move the ocean cursor to the requested Drop location
		navigatePath (ref session, ref ocean, ref sPath, ref iFlags);
		
		// If successful - Retrieve the session pointer from the session array
		if (iFlags >= PIXE.OP_SUCCESSFUL) {
			// CHECK PRIVILEDGES HERE - do they have read access? - return error if not
			
			// If it's on a FREE them throw an error - Cannot read from an empty molecule
			if(ocean[session.Cursor].Type == "") {
				#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("ERROR = Cannot read. Invalid cursor position");
				#endif
				iFlags = PIXE.OP_FAIL_INVALID_CURSOR_POSITION;
				#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
				#endif
				return null;
			}
			else {
				// Move cursor onto requested attribute/element within the Drop
				if(findMolecule(ref session, ref ocean, ref sPath, PIXE.PSML_UNSET_FLAG)) {
					// For successfully found elements - return true
					if (iFlags == PIXE.PSML_READ_ELEMENT) {
						#if (PIXE_DEBUG_PROFILER)
						Profiler.EndSample ();
						#endif
						return true;
					}
					// For attributes - return the value (in correct data type)
					else if (iFlags == PIXE.PSML_READ_ATTRIBUTE) {
						// Need to switch to correct data type - INCOMPLETE
						#if (PIXE_DEBUG_PROFILER)
						Profiler.EndSample ();
						#endif
						return ocean[session.Cursor].Value;
					}
				}
				else {
					// If element is not found, return false
					if(iFlags == PIXE.PSML_READ_ELEMENT) {
						#if (PIXE_DEBUG_PROFILER)
						Profiler.EndSample ();
						#endif
						return false;
					}
					// Return an error if att/element not found
					#if (PIXE_DEBUG_PROFILER)
					Profiler.EndSample ();
					#endif
					return "Requested attribute not found";
				}
			}
		}
		else if (iOriginalFlags == PIXE.PSML_READ_ELEMENT){
			#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
			#endif
			return false;
		}
		#if (PIXE_DEBUG_PROFILER)
		Profiler.EndSample ();
		#endif
		return "Requested attribute not found";
	}

	public bool Move(int iSessionIndex, string sDestination, ref int iFlags) 
	{
		#if (PIXE_DEBUG_PROFILER)
		Profiler.BeginSample ("Move");
		#endif
		
		// Retrieve the session cursor and correct ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];
		int iCursor = session.Cursor;
		
		// Move the cursor to the Drop Header molecule & save this header location (for use in child Header)
		findHeader (ref ocean, ref iCursor);
		int iHeader = iCursor;	
		
		// Move cursor to parent Header if desitination is ".."
		if (sDestination == "..") {
			iCursor = iCursor + (int)ocean[iCursor].Data;
			session.Cursor = iCursor;
		} 
		// Else, search for the destination attribute/element reference in the current Drop
		else {
			if(!findMolecule(ref session, ref ocean, ref sDestination, iHeader)){
				#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log ("ERROR = Unable to move cursor. No matching record found in current location:" + sDestination);
				#endif
				iFlags = PIXE.OP_FAIL_INVALID_PATH;
				#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
				#endif
				return false;
			}
			iCursor = session.Cursor;
			
			// When moving into nested elements - create a Drop Header if one doesn't already exist...
			if(ocean[iCursor].Type == "E") {
				if((Convert.ToInt32(ocean[iCursor].Value)) == PIXE.OCEAN_UNSET) {
					createNested (ref session, ref ocean, iHeader, ref iFlags); 
				}
				// ...or move the currsor to the correct header if it does already exist.
				else {
					iCursor = iCursor + (int)ocean[iCursor].Value;
					session.Cursor = iCursor;
				}
			}
		}
		#if (PIXE_DEBUG_PROFILER)
		Profiler.EndSample ();
		#endif
		return true;
	}

	public string WhereAmI(int iSessionIndex) {
		
		// Retrieve the session cursor and correct ocean
		Session session = sessionList[iSessionIndex];
		List<Molecule> ocean = oceanList[session.Ocean];
		
		return ocean [session.Cursor].Name;
	}

	public void freeSession(ref int iSessionIndex, ref int iFlags)
	{
		#if (PIXE_DEBUG_PROFILER)
		Profiler.BeginSample ("freeSession");
		#endif
		
		//Retrieve the session
		Session toFree = sessionList [iSessionIndex];
		
		// Reset all the session settings
		toFree.Cursor = PIXE.RESET;
		toFree.Ocean = PIXE.RESET;
		toFree.Privileges = PIXE.RESET;
		toFree.InUse = false;
		
		iSessionIndex = PIXE.RESET;
		#if (PIXE_DEBUG_PROFILER)
		Profiler.EndSample ();
		#endif
		return;
	}

	/*
	 * OTHER PUBLIC METHODS:
	 * hasElements()
	 * getElements()
	 * */

	// Searches Drop for any nested ELEMENTS or ATTRIBUTES - returns a bool.
	public bool hasElements(ref Session session, ref List<Molecule> ocean, int iType)
	{
		int iCursor, iDropSize;
		iCursor = session.Cursor;
		bool hasElements = false;
		string sSearch;

		// Set search to specified ELEMENT or ATTRIBUTE
		if (iType == PIXE.PSML_ELEMENT) {
			sSearch = "E";
		} else if (iType == PIXE.PSML_ATTRIBUTE) {
			sSearch = "A";
		} else {
			#if (PIXE_DEBUG_LOG)
			UnityEngine.Debug.Log("ERROR = Unknown Molecule type seached for.");
			#endif
			return hasElements;
		}
		// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
		findHeader (ref ocean, ref iCursor);
		iDropSize = (int)ocean[iCursor].Value;

		int iLimit = iCursor + iDropSize;
		while(iCursor < iLimit) {
			if(ocean[iCursor].Type == sSearch) {
				hasElements = true;
			}
			iCursor++;
		}
		return hasElements;
	}
	// Can be merged ^^^^^^^

	// Returns a Drops nested element/attribute names as a List of strings
	public List<string> getElements(ref Session session, ref List<Molecule> ocean, int iType)
	{
		int iCursor, iDropSize;
		List<string> elementList = new List<string>();
		iCursor = session.Cursor;
		string sSearch;

		// Set search to specified ELEMENT or ATTRIBUTE
		if (iType == PIXE.PSML_ELEMENT) {
			sSearch = "E";
		} else if (iType == PIXE.PSML_ATTRIBUTE) {
			sSearch = "A";
		} else {
			#if(PIXE_DEBUG_LOG)
			UnityEngine.Debug.Log("ERROR = Unknown Molecule type seached for.");
			#endif
			return null;
		}
		// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
		findHeader (ref ocean, ref iCursor);
		iDropSize = (int)ocean[iCursor].Value;
	
		// Add each element/attribute name into the list
		int iLimit = iCursor + iDropSize;
		while(iCursor < iLimit) {
			if(ocean[iCursor].Type == sSearch) {
				elementList.Add (ocean[iCursor].Name);
			}
			iCursor++;
		}
		return elementList;
	}

	/*
	 * UNSAFE PUBLIC METHODS
	 * - Read & Write
	 * */
	
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

	public object unsafeRead(int iSession, ref int iFlags)
	{
		// Retrieve the session cursor
		int iReadIndex = sessionList [iSession].Cursor; 
		
		// Retreive the ocean being referenced by the cursor
		int iOceanIndex = sessionList [iSession].Ocean;
		List<Molecule> ocean = oceanList [iOceanIndex];
		
		// Read the data
		if (iOceanIndex >= 0 && iOceanIndex < ocean.Count) {
			if(iFlags == PIXE.OCEAN_READ_MOLECULE_NAME) {
				return ocean [iReadIndex].Name;
			} else if (iFlags == PIXE.OCEAN_READ_MOLECULE_TYPE) {
				return ocean [iReadIndex].Type;
			} else if (iFlags == PIXE.OCEAN_READ_MOLECULE_VALUE) {
				return ocean [iReadIndex].Value;
			} else if (iFlags == PIXE.OCEAN_READ_MOLECULE_DATA) {
				return ocean [iReadIndex].Data;
			} else {
				#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log ("ERROR = Unable to read. Invalid data request.");
				#endif
				iFlags = PIXE.OP_FAIL;
			}
		} 
		else {
			#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("ERROR = Unable to read. Invalid cursor position. " + iReadIndex);
			#endif
			iFlags = PIXE.OP_FAIL_INVALID_PATH;
		}
		return null;
	}

	/*
	 * PRIVATE INTERNAL METHODS
	 * */

	private void createMolecule(ref Session session, ref List<Molecule> ocean, 
	                            int iLocation, int iHeader, string sName, 
	                            string sType, object oValue, object oData, ref int iFlags)
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("createMolecule");
		#endif

		// Get the offset of the last molecule in the Drop from its header
		int iOffset = 0; 
		if (ocean [iHeader].Name != null && ocean [iHeader].Name != "") {
			iOffset = (int)ocean[iHeader].Value;
		}
		// Move the Drop if it has run out of free space or overlaps another drop's space
		if (ocean [PIXE.OCEAN_HOME].Type != "EMPTY") {
			preventDropOverlap (ref session, ref ocean, iLocation, iHeader);
		}
		// If overlap occurs, move the smaller of the overlapping drops 
		if (iLocation >= (ocean.Count-1) ||
		    (ocean [iLocation].Name != "" && ocean [iLocation].Name != null)) {
			// If the Drop being written is the smaller:
			if((int)ocean[iHeader].Value < (int)ocean[iLocation].Value) {
				moveDrop(ref session, ref ocean, ref iHeader/*, ref iFlags*/, true);
				// Amend the write lLocation to match the new Drop
				iLocation = iHeader;
				iLocation += iOffset;
			}
			// Else, if the other overlapping Drop is smaller:
			else {
				int iCursorReset = iLocation;
				session.Cursor = iLocation;
				moveDrop(ref session, ref ocean, ref iLocation,false);
				session.Cursor = iCursorReset;
				iLocation = iCursorReset;
			}
		}
		// Check to ensure overlap is fixed
		if (ocean [iLocation].Name != "" && ocean [iLocation].Name != null) {
			#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("FATAL ERROR = Molecule overlap occurred at index "+iLocation);
			#endif
			iFlags = PIXE.OP_FAIL_WRITE_ERROR;
			#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
			#endif
			return;
		}
		// If the space is now free, write to it
		ocean [iLocation].Name = sName;
		ocean [iLocation].Type = sType;
		ocean [iLocation].Value = oValue;
		ocean [iLocation].Data = oData;

		// Update the size of the Drop header to reflect new addtion
		iOffset += 1;
		ocean[iHeader].Value = iOffset;

		// Move the session cursor to the newly created Molecule
		session.Cursor = iLocation;
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		return;
	}
	
	private void createNested(ref Session session, ref List<Molecule> ocean, int iParentHeader, ref int iFlags)
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("createNested");
		#endif

		// Save the nested Element reference orgin & find somewhere to put the new Header
		int iOrigin = session.Cursor;
		int iCursor = getDrop(ref session, ref ocean, 5, PIXE.RESET, false);

		// Write the Header data (NOTE - session cursor is moved by createMolecule()
		createMolecule(
			ref session, ref ocean,
			iCursor, iCursor, 
			ocean [iOrigin].Name, "H", 0, (iParentHeader - iCursor), ref iFlags);   /// CHANGE 1 to 0 if wrong

		// Update the offset value in the parent element molecule to point to this Header
		ocean [iOrigin].Value = (iCursor - iOrigin);
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		return;
	}

	private void findHeader(ref List<Molecule> ocean, ref int iCursor) 
	{
		#if (PIXE_DEBUG_PROFILER)
		Profiler.BeginSample ("findHeader");
		#endif
		
		// Move the Cursor to the Drop header
		while (ocean[iCursor].Type != "H") {
			iCursor--;
		}
		#if (PIXE_DEBUG_PROFILER)
		Profiler.EndSample ();
		#endif
		return;
	}

	// Finds a record within a specified Drop & moves the session cursor to it
	private bool findMolecule(ref Session session, ref List<Molecule> ocean, ref string sName, int iHeader)
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("findMolecule");
		#endif

		// Create a temp cursor to step through the Drop
		int iCursor = session.Cursor;

		// Move the temp cursor to the current Drop header...
		if (iHeader == PIXE.PSML_UNSET_FLAG) {
			findHeader(ref ocean, ref iCursor);
		} else {
			iCursor = iHeader;
		}
		// ... and get the size of the Drop (to use for search area limit)
		int iLimit = (int)ocean[iCursor].Value;
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
			#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
			#endif
			return true;
		}
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		return false;
	}

	/*
	 * NAVIGATE PATH:
	 * Moves the session cursor to the Node specified in the sPath paremeter.
	 * */
	private void navigatePath(ref Session session, ref List<Molecule> ocean, ref string sPath, ref int iFlags)
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("navigatePath");
		#endif

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
			if(iFlags == PIXE.PSML_WRITE_ELEMENT || iFlags == PIXE.PSML_WRITE_ATTRIBUTE) {
				sLast = sPathArray[sPathArray.Length-1];
				Array.Resize(ref sPathArray, sPathArray.Length-1);
			}
			// Save current cursor location then set the cursor to the root node of the ocean
			int iCursorReset = session.Cursor;
			session.Cursor = (int)ocean[PIXE.OCEAN_HOME].Value;

			// Attempt to move cursor to provided path
			int i=0, iDepth = sPathArray.GetLength(0);
			bool bFound = true;
			while(i<iDepth && bFound == true) {
				bFound = Move (session.ID, sPathArray[i], ref iFlags);
				i++;
			}
			if(i != iDepth || !bFound) {
				// Move cursor to any elements which don't have a header yet.
				if (iFlags == PIXE.PSML_WRITE_ELEMENT) {
					if (findMolecule(ref session, ref ocean, ref sLast, PIXE.PSML_UNSET_FLAG)) {
						return;
					}
				}
				// If path is invalid - Reset cursor to original location
				session.Cursor = iCursorReset;
				#if (PIXE_DEBUG_LOG)
					UnityEngine.Debug.Log("ERROR = Invalid path provided.");
				#endif
				iFlags = PIXE.OP_FAIL_INVALID_PATH;
			}
		}
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		return;
	}

	/*
	 * Creates an empty ocean.
	 * */
	private void createOcean(ref int iOceanIndex, ref int iFlags)
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("createOcean");
		#endif

		List<Molecule> newOcean = null;
		int i, iOceanCount, iMoleculeCount;

		if (oceanList != null) {
			// Find the first empty Ocean in the list & assign the index
			iOceanCount = oceanList.Count;
			for (i=0; i<iOceanCount; i++) {
				newOcean = oceanList [i];
				iMoleculeCount = newOcean.Count;
				if (iMoleculeCount == PIXE.OCEAN_EMPTY) {
					break;
				}
			}
			iOceanIndex = i;

			// Fill the Ocean with default min number of Molecules
			for (i=0; i<PIXE.OCEAN_DEFAULT_MOLECULE_COUNT; i++) {
				// Add and intialise all molecules 
				newOcean.Add (new Molecule ());
				newOcean[i].Name = "";
				newOcean[i].Type = "";
				newOcean[i].Value = "";
				newOcean[i].Data = "";
			}
			// Initialise the ocean home in the first free molecule (1)
			newOcean[PIXE.OCEAN_HOME].Name = "HOME";
			newOcean[PIXE.OCEAN_HOME].Type = "Empty";
			newOcean[PIXE.OCEAN_HOME].Value = PIXE.OCEAN_DEFAULT_HOME;
			newOcean[PIXE.OCEAN_HOME].Data = new List<int>[PIXE.OCEAN_DROP_COULMN_COUNT];

			// Initialise the drops array by creating a list in each cell
			List<int>[] lDrops = (List<int>[])newOcean[PIXE.OCEAN_HOME].Data;
			int iDropsSize = lDrops.GetLength (0);
			for(i=0;i<iDropsSize;i++) {
				lDrops[i] = new List<int>();
			}
			// As the ocean is empty, the first availble drop is at the end of the first (resevered for root) header
			// NOTE: The MANY DropList is one value which is the index of the start of the many block
			lDrops [PIXE.OCEAN_DROP_MANY].Add(6);
			newOcean[6].Data = PIXE.OCEAN_DROP_MANY;
		} 
		else {
			iFlags = PIXE.OP_FAIL_MEMORY_ERROR;
			#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("ERROR = Unable to create ocean. Ocean list has not been intialised.");
			#endif
			iOceanIndex = PIXE.RESET;
		}
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
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

	/*
	 * DROP MEMORY MANAGEMENT
	 * */

	private void preventDropOverlap(ref Session session, ref List<Molecule> ocean, int iIndex, int iHeader) 
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("preventDropOverlap");
		#endif

		// Retreive the List holding the appropriately sized Drops & get the index of the last ite
		List<int>[] dropArray = (List<int>[])ocean[PIXE.OCEAN_HOME].Data;
		List<int> dropLookup = dropArray[PIXE.OCEAN_DROP_MANY];

		// Check the many list - amend start point of the many block if overlap has occurred there
		int iLast = dropLookup.Count - 1;
		if (iLast >= 0) {
			int iCurrentStart = dropLookup[iLast];
			if(iCurrentStart == iIndex) {
				dropLookup.RemoveAt(iLast);
				dropLookup.Add(iCurrentStart + 5);
				ocean[(iCurrentStart + 5)].Data = PIXE.OCEAN_DROP_MANY;
				#if (PIXE_DEBUG_PROFILER)
					Profiler.EndSample ();
				#endif
				return;
			}
		}
		if (ocean [iIndex].Name != "") {
			#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
			#endif
			return;
		}
		if (ocean [iIndex].Data == "" || ocean [iIndex].Data == null) {
			#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
			#endif
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

				// Update the appropriate lists to reflect this new change
				if(iDropList == PIXE.OCEAN_DROP_5) {
					bAddToList = false;
				} else if (iDropList == PIXE.OCEAN_DROP_10) {
					iDropList = PIXE.OCEAN_DROP_5;
					ocean[iClash].Data=PIXE.OCEAN_DROP_5;
				} else if (iDropList == PIXE.OCEAN_DROP_15) {
					iDropList = PIXE.OCEAN_DROP_10;
					ocean[iClash].Data=PIXE.OCEAN_DROP_10;
				} else if (iDropList == PIXE.OCEAN_DROP_20) {
					iDropList = PIXE.OCEAN_DROP_15;
					ocean[iClash].Data=PIXE.OCEAN_DROP_15;
				} else if (iDropList == PIXE.OCEAN_DROP_25) {
					iDropList = PIXE.OCEAN_DROP_20;
					ocean[iClash].Data=PIXE.OCEAN_DROP_20;
				} else if (iDropList == PIXE.OCEAN_DROP_30) {
					iDropList = PIXE.OCEAN_DROP_25;
					ocean[iClash].Data=PIXE.OCEAN_DROP_25;
				} else {
					bAddToList = false;
				}
				if(bAddToList) {
					dropLookup = dropArray [iDropList];
					dropLookup.Add(iClash);
				}
				// Return if the drop is found & dealt with
				if(bFound) {
					#if (PIXE_DEBUG_PROFILER)
						Profiler.EndSample ();
					#endif
					return;
				}
			}
		}
		#if (PIXE_DEBUG_LOG)
			UnityEngine.Debug.Log ("ERROR = Matching Drop Not Found");
		#endif
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		return;
	}

	// Finds a block of free space to store a new drop
	private int getDrop(ref Session session, ref List<Molecule> ocean, int iSize, int iHeader, bool bMove) 
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("getDrop");
		#endif

		int iFoundDrop = PIXE.RESET;
		
		// Get the list holding the correct sized Drops and round up the Drop size
		int iDropList = PIXE.OCEAN_DROP_5;
		if (iSize < 0) {
			#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("ERROR = Invalid requested Drop size");
			#endif
			#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
			#endif
			return PIXE.RESET;
		} else if (iSize >= 0 && iSize <= 5) {
			iDropList = PIXE.OCEAN_DROP_5;
			iSize = 5;
		} else if (iSize > 5 && iSize <= 10) {
			iDropList = PIXE.OCEAN_DROP_10;
			iSize = 10;
		} else if (iSize > 10 && iSize <= 15) {
			iDropList = PIXE.OCEAN_DROP_15;
			iSize = 15;
		} else if (iSize > 15 && iSize <= 20) {
			iDropList = PIXE.OCEAN_DROP_20;
			iSize = 20;
		} else if (iSize > 20 && iSize <= 25) {
			iDropList = PIXE.OCEAN_DROP_25;
			iSize = 25;
		} else if (iSize > 25 && iSize <= 30) {
			iDropList = PIXE.OCEAN_DROP_30;
			iSize = 30;
		} else if (iSize > 30) {
			iDropList = PIXE.OCEAN_DROP_MANY;
			// Round up iSize value to be a mutliple of 5
			iSize = (iSize + (5-iSize % 5));
		}
		// Retreive the List holding the appropriately sized Drops & get the index of the last item
		List<int> dropLookup;
		int iLast;
		bool bDone = false;
		
		while (!bDone) {
			List<int>[] dropArray = (List<int>[])ocean[PIXE.OCEAN_HOME].Data;
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
				}
				// If in the many: amend the start point of the many block
				if(iDropList == PIXE.OCEAN_DROP_MANY) { 
					dropArray [PIXE.OCEAN_DROP_MANY].Add(iFoundDrop + iSize);
					ocean[(iFoundDrop + iSize)].Data = PIXE.OCEAN_DROP_MANY;
				}
				bDone = true;
			}
			//If nothing is found, repeat the search in the Many list.
			iDropList = PIXE.OCEAN_DROP_MANY;
		}
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		ocean [iFoundDrop].Data = "";
		return iFoundDrop;
	}
	
	private void expandOcean(ref List<Molecule> ocean, int iNewDropEnd) {

		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("expandOcean");
		#endif

		int iLastMolecule = ocean.Count-1;
		int iNewOceanEnd = PIXE.OCEAN_UNSET;

		// If ocean has less than 1000 molecules, double the size
		if (iLastMolecule <= (PIXE.OCEAN_DEFAULT_MOLECULE_COUNT*3)) {
			iNewOceanEnd = (int)(iLastMolecule * 2);
		}
		// If it has 1001-5000, increase size by 25%
		else if(iLastMolecule > (PIXE.OCEAN_DEFAULT_MOLECULE_COUNT*3) && iLastMolecule <= (PIXE.OCEAN_DEFAULT_MOLECULE_COUNT*1000)) {
			iNewOceanEnd = (int)(iLastMolecule * 1.25);
		}
		// If it has more than 5000, increase by 10%
		else if (iLastMolecule > PIXE.OCEAN_DEFAULT_MOLECULE_COUNT*1000) {
			 iNewOceanEnd = (int)(iLastMolecule * 1.1);
		}

		if (iNewOceanEnd < iNewDropEnd) {
			iNewOceanEnd = iNewDropEnd;
		}
		// Execute the expansion
		int i;
		for(i=iLastMolecule;i<iNewOceanEnd;i++) {
			ocean.Add (new Molecule ());
			ocean[i].Name = "";
			ocean[i].Type = "";
			ocean[i].Value = "";
			ocean[i].Data = "";
		}
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
	}
	
	private void moveDrop(ref Session session, ref List<Molecule> ocean, ref int iHeader, bool bAddToDrops) 
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("moveDrop");
		#endif

		// Retreive the session cursor and move it to the Drop header
		int iCursor = session.Cursor;
		findHeader (ref ocean, ref iCursor);
		iHeader = iCursor;

		// The required Drop size needs to be larger than the current Drop size:
		int iDropSize = (int)ocean[iHeader].Value;
		int iNewLocation = getDrop (ref session, ref ocean, (iDropSize+1), iHeader, true);

		if (ocean [iNewLocation].Name != "" && ocean [iNewLocation].Name != null) {
			#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("WARNING = moveDrop OVERLAP detected at index "+iNewLocation);
				UnityEngine.Debug.Log("** " +ocean [iNewLocation].Name+" **");
			#endif
			while (ocean [iNewLocation].Name != "" && ocean [iNewLocation].Name != null) {
				iNewLocation = getDrop (ref session, ref ocean, (iDropSize+1), iHeader, true);
			}
		}
		// Copy the drop to the new location
		int i;
		for(i=0; i<iDropSize; i++) {
			ocean [iNewLocation+i].Name = String.Copy (ocean [iCursor+i].Name);
			ocean [iNewLocation+i].Type = String.Copy (ocean [iCursor+i].Type);
			// Value Column
			string sDataType = (ocean [iCursor+i].Value.GetType()).ToString();
			switch(sDataType) {
			case "System.String":
				ocean [iNewLocation+i].Value = String.Copy ((string)ocean [iCursor+i].Value);
				break;
			case "System.Int32":
				ocean [iNewLocation+i].Value = ocean [iCursor+i].Value;
				break;
			default:
				#if (PIXE_DEBUG_LOG)
					UnityEngine.Debug.Log("ERROR = Invalid data type found in Molecule (Data).");
				#endif
				break;
			}
			// Data Column
			sDataType = (ocean [iCursor+i].Data.GetType()).ToString();
			switch(sDataType) {
			case "System.String":
				ocean[iNewLocation+i].Data = String.Copy ((string)ocean [iCursor+i].Data);
				break;
			case "System.Int32":
				ocean[iNewLocation+i].Data = ocean [iCursor+i].Data;
				break;
			default:
				#if (PIXE_DEBUG_LOG)
					UnityEngine.Debug.Log("ERROR = Invalid data type found in Molecule (Data).");
				#endif
				break;
			}
		}
		// If the root node has been moved - update the root positon
		if(iHeader == (int)ocean[PIXE.OCEAN_HOME].Value) {
			ocean[PIXE.OCEAN_HOME].Value = iNewLocation;
		}
		// Update all the offset information in the copied Header
		int iNewHeader = iNewLocation;
		int iCursorReset = iCursor;
			
		// Update the offset to its parent (NOTE - the root offset always = 0)
		if (iNewHeader != (int)ocean [PIXE.OCEAN_HOME].Value) {	
			iCursor = iCursor + (int)ocean [iCursor].Data;
			ocean [iNewHeader].Data = (iCursor - iNewHeader);
		}
		iCursor = iNewHeader + (int)ocean[iNewHeader].Data;
		session.Cursor = iCursor;

		// Update the offset in the Parent
		string sToFind = ocean[iNewHeader].Name;
		if (findMolecule (ref session, ref ocean, ref sToFind, PIXE.PSML_UNSET_FLAG)) {
			iCursor = session.Cursor; 
			ocean [iCursor].Value = (iNewHeader - iCursor);
		}
		else {
			#if (PIXE_DEBUG_LOG)
				UnityEngine.Debug.Log("FATAL ERROR = MOLECULE NOT FOUND!!!!!!!!!!!!!!!!!!!");
			#endif
		}
		iCursor = iCursorReset;

		// Step through the new moved Drop and update any Element offset information
		for (i=0; i<iDropSize; i++) {
			if (ocean [(iNewLocation + i)].Type == "E") {
				int iNewElement = iNewLocation + i;
				int iOldElement = iHeader + i;

				// Only update offsets to elements with existing Headers
				if ((int)ocean [iOldElement].Value != PIXE.OCEAN_UNSET) {
					// Update the offsets in the Elements Header
					iCursor = iOldElement + (int)ocean[iOldElement].Value;
					ocean[iNewElement].Value = (iCursor - iNewElement);
					ocean[iCursor].Data = (iNewLocation -iCursor);
				}
			}
		}
		// Delete the orginal Drop
		for (i=0; i<iDropSize; i++) {
			ocean [(iHeader + i)].Name = "";
			ocean [(iHeader + i)].Type = "";
			ocean [(iHeader + i)].Value = null;
			ocean [(iHeader + i)].Data = null;
		}
		// Put the newly freed space into the drops array (if space not already been assigned)
		if (bAddToDrops) {
			addToDrops (ref ocean, iDropSize, iHeader);
		}
		// Move the cursor and header references to the new Drop location
		session.Cursor = iNewLocation + iDropSize;
		iCursor = iNewLocation + iDropSize;
		iHeader = iNewLocation;
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample ();
		#endif
		return;
	}
	private void addToDrops(ref List<Molecule> ocean, int iDropSize, int iLocation)
	{
		#if (PIXE_DEBUG_PROFILER)
			Profiler.BeginSample ("addToDrops");
		#endif

		List<int>[] dropArray;

		// Allocate to the correctly sized drop list
		int iDropList = PIXE.OCEAN_DROP_5;
		if (iDropSize >= 0 && iDropSize <= 5) {
			iDropList = PIXE.OCEAN_DROP_5;
		} else if (iDropSize > 5 && iDropSize <= 10) {
			iDropList = PIXE.OCEAN_DROP_10;
		} else if (iDropSize > 10 && iDropSize <= 15) {
			iDropList = PIXE.OCEAN_DROP_15;
		} else if (iDropSize > 15 && iDropSize <= 20) {
			iDropList = PIXE.OCEAN_DROP_20;
		} else if (iDropSize > 20 && iDropSize <= 25) {
			iDropList = PIXE.OCEAN_DROP_25;
		} else if (iDropSize > 25 && iDropSize <= 30) {
			iDropList = PIXE.OCEAN_DROP_30;
		}
		// If it's bigger than 30, break it up into chunks of 5
		// THIS MAY NEED CHANGING LATER - break into varying chunk sizes?
		else if (iDropSize > 30) {
			int iNumberOfBlocks = iDropSize / 5;
			int i;
			for(i=0;i<iNumberOfBlocks;i++) {
				dropArray = (List<int>[])ocean [PIXE.OCEAN_HOME].Data;
				dropArray [PIXE.OCEAN_DROP_5].Add (iLocation);
				ocean[iLocation].Data = PIXE.OCEAN_DROP_5;
				iLocation+=5;
			}
			#if (PIXE_DEBUG_PROFILER)
				Profiler.EndSample ();
			#endif
			return;
		}	
		// Add the Drop to the selected list
		dropArray = (List<int>[])ocean [PIXE.OCEAN_HOME].Data;
		dropArray [iDropList].Add (iLocation);
		ocean [iLocation].Data = iDropList;
		#if (PIXE_DEBUG_PROFILER)
			Profiler.EndSample (); 
		#endif
		return;
	}
}
