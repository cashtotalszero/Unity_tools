/* 
Gamification of Space Exploration Project.

Written in 2014 by Alex Parrott

To the extent possible under law, the author(s) have dedicated all
copyright and related and neighboring rights to this software to the
public domain worldwide. This software is distributed without any
warranty.

You should have received a copy of the CC0 Public Domain Dedication
along with this software. If not, see
<http://creativecommons.org/publicdomain/zero/1.0/>.
*/

using UnityEngine;
using System.Collections;

public class Session {

	public int ID { get; set;}				// The unique ID of the session (matches index number)
	public int Cursor { get; set;}			// The Molecule index of the cursor position in the Ocean
	public int Ocean { get; set;}			// The oceanList index of the Ocean being referenced by the cursor
	public int Privileges { get; set;}		// Read/Write access for the session	
	public bool InUse { get; set;}			// Is Session currently being used?
}
