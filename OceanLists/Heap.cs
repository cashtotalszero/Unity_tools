using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Heap : MonoBehaviour {

	const int PIXE_OCEAN_LIST_DEFAULT_SIZE = 10;
	const int PIXE_SESSION_LIST_DEFAULT_SIZE = 50;
	const int PIXE_OCEAN_DEFAULT_MOLECULE_COUNT = 100;

	const int PIXE_RESET = -1;

	List<List<Molecule>> oceanList;
	List<Session> sessionList;

	// Use this for initialization
	void Start () {

		int iOceanIndex = PIXE_RESET;			// Holds the index of an Ocean in the Ocean List
		int thisSession = PIXE_RESET;

		createOcean (ref iOceanIndex);
		Debug.Log (iOceanIndex);

		createSession (ref thisSession, iOceanIndex);
		Debug.Log (thisSession);
	}

	/*
	 * Creates a session handle to an Ocean specified as a parameter.
	 * */
	private void createSession(ref int iSessionIndex, int iOceanIndex)
	{
		Session newSession = null;
		int i, iSessionCount;

		// Declare the sessionList if one doesn't exist (MOVE TO INITIALISE?)
		if (sessionList == null) {
			sessionList = new List<Session> ();
			for(i=0;i<PIXE_SESSION_LIST_DEFAULT_SIZE;i++) {
				newSession = new Session();
				newSession.InUse = false;
				sessionList.Add(newSession);
			}
		}
		// Find the first session not in use in the list & assign the index
		iSessionCount = sessionList.Count;
		for(i=0; i<iSessionCount; i++) {
			newSession = sessionList[i];
			// Once found, initialise the session settings
			if(!newSession.InUse) {
				newSession.Cursor = PIXE_RESET;
				newSession.Ocean = iOceanIndex;
				newSession.Privileges = PIXE_RESET;
				newSession.InUse = true;
				break;
			}
		}
		iSessionIndex = i;
		return;
	}

	/*
	 * Creates an empty ocean.
	 * */
	private void createOcean(ref int iOceanIndex)
	{
		List<Molecule> newOcean = null;
		int i, iOceanCount, iMoleculeCount;

		// Declare the oceanList if one doesn't exist (MOVE TO INITIALISE?)
		if (oceanList == null) {
			oceanList = new List<List<Molecule>> ();
			for(i=0;i<PIXE_OCEAN_LIST_DEFAULT_SIZE;i++) {
				oceanList.Add(new List<Molecule>());
			}
		}
		// Find the first empty Ocean in the list & assign the index
		iOceanCount = oceanList.Count;
		for(i=0; i<iOceanCount; i++) {
			newOcean = oceanList[i];
			iMoleculeCount = newOcean.Count;
			if(iMoleculeCount == 0) {
				break;
			}
		}
		iOceanIndex = i;

		// Fill the Ocean with default min number of Molecules
		for(i=0; i<PIXE_OCEAN_DEFAULT_MOLECULE_COUNT; i++) {
			// NOTE: All Molecule variables are automatically initialised as null
			newOcean.Add (new Molecule());
		}
		return;
	}

	/*
	 * Clears the entire contents of a specified Ocean in the Ocean List
	 * */
	private void drainOcean(int iOceanIndex)
	{
		List<Molecule> toDrain = oceanList [iOceanIndex];
		toDrain.Clear ();
	}

	// BASIC WRITE MOLECULE
	private void writeMolecule(int iSession, int iIndex, object oName, object oType, object oValue, object oData)
	{
		// Retrieve the session cursor

		// Retreive the ocean being referenced by the cursor

		// Write the data

		// Return
	}

/*
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

*/

	// BASIC READ MOLECULE
	
}
