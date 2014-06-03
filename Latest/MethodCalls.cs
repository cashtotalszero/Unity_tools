using UnityEngine;
using System.Collections;
using System;

public class MethodCalls : MonoBehaviour {

	private string[,] ocean;
	private string[,] sessions;

	void Start() {
		if (ocean == null) {
			Debug.Log("NULL");
		}
		Initialise (1);
		if (ocean == null) {
			Debug.Log("STILL NULL");
		}
	}

	public long Initialise(long lFlags) {

		// If ocean doesn't already exist create one with 100 rows to start with
		if (ocean == null) {
			ocean = new string[100, 4];
			ocean = setArrayToFree(ocean);
		}
		// Likewise if sessions doesn't already exist create one with 50 spaces
		if (sessions == null) {
			sessions = new string[50,2];
			sessions = setArrayToFree(sessions);
		}

		// Step through session array and look for a free slot
		int i, length = sessions.GetLength(0);
		for (i=0; i<length; i++) {
			// Return the index number if one is found
			if(sessions[i,0] == "") {
				/*
				 * SET PRIVLEDGES HERE - according to lFlags
				 * */
				return i;
			}
		}
		// Return ERROR code to show no spaces found if end of array is reached.
		return 0;
	}

	public object Read(long lSession, string sPath, long lFlags){

		// Retrieve the session pointer from the session array
		string sessionPointer = sessions [lSession, 0];
		if (sessionPointer != "") {
			long lSessionPointer = (long)Convert.ToDouble (sessions [lSession, 0]);
		}

		/*
		 * If sPath starts with "psml://" look up absolute path
		 * 
		 * else, lookup named attribute
		 * 
		 * */

		return lFlags;
	}


	// Sets every cell in a 2D string array to "" 
	private string[,] setArrayToFree(string[,] array){

		// Initialise ALL cells to FREE ("")
		int i,j;
		int x=array.GetLength(0),y=array.GetLength(1);

		for(i=0;i<x;i++) {
			for(j=0;j<y;j++) {
				array[i,j] = "";
			}
		}
		return array;
	}
}
