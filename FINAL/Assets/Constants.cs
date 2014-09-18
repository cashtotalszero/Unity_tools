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

public class Constants : MonoBehaviour {


	// iFlag definitions (must be greater than 0)
	public int PSML_UNSET_FLAG = 5;
	public int PSML_READ_ELEMENT = 1;
	public int PSML_READ_ATTRIBUTE = 2;
	public int PSML_WRITE_ELEMENT = 3;
	public int PSML_WRITE_ATTRIBUTE = 4;
	public int PSML_ATTRIBUTE = 5;
	public int PSML_ELEMENT = 6;
	public int PSML_MOVE = 7;

	// Unsafe iFlags
	public int OCEAN_READ_MOLECULE_NAME = 0;
	public int OCEAN_READ_MOLECULE_TYPE = 1;
	public int OCEAN_READ_MOLECULE_VALUE = 2;
	public int OCEAN_READ_MOLECULE_DATA = 3;
	
	// Operation success/fail codes
	public int OP_SUCCESSFUL = 1;
	public int OP_FAIL = 0;
	public int OP_FAIL_INVALID_PATH = -1;
	public int OP_FAIL_INVALID_CURSOR_POSITION = -2;
	public int OP_FAIL_DUPLICATE_RECORD = -3;
	public int OP_FAIL_WRITE_ERROR = -4;
	public int OP_FAIL_NO_FREE_SESSION = -5;
	public int OP_FAIL_MEMORY_ERROR = -6;
	public int OP_FAIL_INVALID_PSML = -7;
	public int OP_FAIL_XML_LOAD_ERROR = -8;
	
	// Ocean & Session definitions
	public int OCEAN_LIST_DEFAULT_SIZE = 10;
	public int SESSION_LIST_DEFAULT_SIZE = 50;
	public int OCEAN_DEFAULT_MOLECULE_COUNT = 30000;
	public int OCEAN_EMPTY = 0;
	public int OCEAN_HOME = 0;
	public int OCEAN_UNSET = 0;
	public int OCEAN_DEFAULT_HOME = 1;			// The root node of the PIXE ocean
	
	// Drop allocation array definitions
	public int OCEAN_DROP_5 = 0;
	public int OCEAN_DROP_10 = 1;
	public int OCEAN_DROP_15 = 2;
	public int OCEAN_DROP_20 = 3;
	public int OCEAN_DROP_25 = 4;
	public int OCEAN_DROP_30 = 5;
	public int OCEAN_DROP_MANY = 6;
	public int OCEAN_DROP_COULMN_COUNT = 7;
	public int OCEAN_DROP_MIN = 6;
	public int OCEAN_DROP_MIN2 = 5;
	
	// Reset flag 
	public int RESET = -1;
	public int OCEAN_NEW = -1;
	
}
