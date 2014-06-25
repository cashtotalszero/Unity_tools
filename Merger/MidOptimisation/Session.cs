using UnityEngine;
using System.Collections;

public class Session {

	public int ID { get; set;}				// The unique ID of the session (matches index number)
	public int Cursor { get; set;}			// The Molecule index of the cursor position in the Ocean
	public int CursorReset { get; set;}	
	public int Ocean { get; set;}			// The oceanList index of the Ocean being referenced by the cursor
	public int Privileges { get; set;}		// Read/Write access for the session	
	public bool InUse { get; set;}			// Is Session currently being used?
}
