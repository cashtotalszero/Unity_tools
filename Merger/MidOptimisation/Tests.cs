using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Tests : MonoBehaviour {

	// iFlag definitions (must be greater than 0)
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_READ_ATTRIBUTE = 2;
	const int PIXE_PSML_WRITE_ELEMENT = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;
	const int PIXE_PSML_UNSET_FLAG = 5;

	const int PIXE_OCEAN_HOME = 0;
	const int PIXE_RESET = -1;
	
	public Heap Jelly;
	
	void Start () {
	
		//test1 ();
		//test2 ();
		//test3 ();	// Needs fix
		//test4 ();
		//test5 ();
		//test6 ();	// Minor glitch
		//test7 ();
		//randomisedTest ();
	
		Debug.Log ("Memory used = " + System.GC.GetTotalMemory(false));
	}

	/*
	  Basic Write/Read:
	 	<A>
			<B att=”Alex” />
		</A>
	 */

	private void randomisedTest() {
		Debug.Log ("***************** RANDOM TEST *********************");

		// NOTE: If iOceanIndex = PIXE_RESET, a new empty ocean is created
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;
		
		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, ref iFlags);	
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Root",null,ref iFlags);

		int randAttEl = UnityEngine.Random.Range (PIXE_PSML_WRITE_ELEMENT, PIXE_PSML_WRITE_ATTRIBUTE);
		int randMove = UnityEngine.Random.Range (0, 2);
		int i;
		string newMolecule = "MoveNumber";
		string value;

		for (i=0; i<15000; i++) {

			// Get some randomised values
			randAttEl = UnityEngine.Random.Range (PIXE_PSML_WRITE_ELEMENT, (PIXE_PSML_WRITE_ATTRIBUTE+1));
			randMove = UnityEngine.Random.Range (0, 3);
			newMolecule = "MoveNumber"+i;
			bool bDone = false;
			int size;

		
			int iCursor = session.Cursor;
			
			// Move the cursor to the current Drop header - get Drop size & add the Att/El to the end
			while(ocean[iCursor].Type != "H") {
				iCursor--;
			}
			size = (int)ocean[iCursor].Value;

				//size = Jelly.moveToHeader(ref session, ref ocean);

				// Do a random write:
				if (randAttEl == PIXE_PSML_WRITE_ELEMENT) {
					//if(size<15) {
						iFlags = PIXE_PSML_WRITE_ELEMENT;
						Jelly.Write (thisSession, newMolecule, null, ref iFlags);
						//Debug.Log("Wrote ELEMENT");
						bDone = true;
					//	}
					}
				else {
					//if(size<15) {
						iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
						Jelly.Write (thisSession, newMolecule, i, ref iFlags);
						//Debug.Log("Wrote ATT");
						bDone = true;
					//}
				}

		

			// Do a random move
			iFlags = PIXE_PSML_UNSET_FLAG;
			switch (randMove) {
			// Move to new molecule
			case 0:
				Jelly.Move (thisSession, newMolecule, ref iFlags);
				//Debug.Log("Moved into new molecule");
				break;
			// Move to parent
			case 1:
				Jelly.Move (thisSession, "..", ref iFlags);
				//Debug.Log("Moved up");
				break;
			// Do not move
			case 2:
				//Debug.Log("Did not move");
				break;
			}
		}
		//Session session = API.sessionList [thisSession]; 
		//List<Molecule> ocean = API.oceanList [session.Ocean];

		Debug.Log ("OCEAN SIZE = " + ocean.Count);

		/*
		Molecule current;
		int length = ocean.Count;
		for (i=0; i<100; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}
*/
	}

	private void test1() {

		Debug.Log ("***************** TEST 1 *********************");

		// NOTE: If iOceanIndex = PIXE_RESET, a new empty ocean is created
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;;

		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, ref iFlags);	
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];

		// Write the elements & attribute
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"A",null,ref iFlags);
		Jelly.Write(thisSession,"B",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "B",ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"att","Alex",ref iFlags);

		// Read using relative
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		object read = Jelly.Read (thisSession,"att", ref iFlags);
		Debug.Log ("Test attribute read (relative). Expecting 'Alex' = "+read);

		// Read using full path
		read = Jelly.Read (thisSession,"psml://A/B/att", ref iFlags);
		Debug.Log ("Test attribute read (full path). Expecting 'Alex' = "+read);

		// Read element that does exist
		iFlags = PIXE_PSML_READ_ELEMENT;
		read = Jelly.Read (thisSession,"psml://A/B", ref iFlags);
		Debug.Log ("Test existing element read. Expecting 'true' = "+read);

		// Read element that doesn't exist
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "..", ref iFlags);
		iFlags = PIXE_PSML_READ_ELEMENT;
		read = Jelly.Read (thisSession,"C", ref iFlags);
		Debug.Log ("Test nonexistant element read. Expecting 'false' = "+read);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.freeSession (ref thisSession, ref iFlags);
	}

	// Complex read & write which will involve movement of drops.
	private void test2() {

		Debug.Log ("***************** TEST 2 *********************");

		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;

		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, ref iFlags);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];

		// Write the elements & attributes

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Root",null,ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Jellyfish1",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Jellyfish1", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Nested1a",null,ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"Att 1",29,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "..", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Jellyfish2",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Jellyfish2", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Nested2a",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Nested2a", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write (thisSession, "Nested2att", 15, ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Alex",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/PROBLEM",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedDo",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedMe",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?Egon",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Crash",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Crash2",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Crash!!!",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Crash!!!", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"Crash!ATT",4,ref iFlags);

		// Read some random elements/atributes
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish1/Att 1", ref iFlags);
		Debug.Log ("Read attribute. Expecting 29 = "+read);

		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", ref iFlags);
		Debug.Log ("Read attribute. Expecting 15 = "+read);

		iFlags = PIXE_PSML_READ_ELEMENT;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedDo", ref iFlags);
		Debug.Log ("Read element. Expecting true = "+read);

		// Overwrite an attribute 
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, ref iFlags);

		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", ref iFlags);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);

		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,ref iFlags);

		// Display the entire ocean
		int i;
		Molecule current;
		for (i=0; i<100; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}

		Jelly.freeSession (ref thisSession, ref iFlags);
	}
	/*
	// Test 3 is similar to test 2 but forces the root node to be moved
	private void test3() {
		
		Debug.Log ("***************** TEST 3 *********************");
		
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		
		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, 0);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];
		
		// Write the elements & attributes
		Jelly.Write(thisSession,"Root",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"Jellyfish1",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish1");
		Jelly.Write(thisSession,"Nested1a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"Att 1",29,PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Move (thisSession, "..");
		Jelly.Write(thisSession,"Jellyfish2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish2");
		Jelly.Write(thisSession,"Nested2a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Nested2a");
		Jelly.Write (thisSession, "Nested2att", 15, PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Alex",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/PROBLEM",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedDo",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedMe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?Egon",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Crash",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Crash2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Crash!!!",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Crash!!!");
		Jelly.Write(thisSession,"Crash!ATT",4,PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Write(thisSession,"psml://Root/FixedMe1",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/FixedMe2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/FixedMe3",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/FixedMe4",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/FixedMe5",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt","Yo",PIXE_PSML_WRITE_ATTRIBUTE);
		
		// Read some random elements/atributes
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish1/Att 1", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 29 = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 15 = "+read);
	
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 'Yo' = "+read);

		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedDo", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Read element. Expecting true = "+read);
		
		// Overwrite an attribute 
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, PIXE_PSML_WRITE_ATTRIBUTE);
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);
		
		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);
		
		// Display the entire ocean
		int i;
		Molecule current;
		for (i=0; i<100; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}
		
		Jelly.freeSession (ref thisSession);
	}
*/
	// Similar to test 3 but also forces ocean to increase in size

	private void test4() {

		Debug.Log ("***************** TEST 4 *********************");
		
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;

		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, ref iFlags);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];
		
		// Write the elements & attributes
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Root",null,ref iFlags);
		Jelly.Write(thisSession,"Jellyfish1",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Jellyfish1", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Nested1a",null,ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"Att 1",29,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "..", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Jellyfish2",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Jellyfish2", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Nested2a",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Nested2a", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write (thisSession, "Nested2att", 15, ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Alex",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/PROBLEM",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedDo",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedMe",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?Egon",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Crash",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Crash2",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Crash!!!",null,ref iFlags);

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Crash!!!", ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"Crash!ATT",4,ref iFlags);

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/FixedMe1",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/FixedMe2",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/FixedMe3",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/FixedMe4",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/FixedMe5",null,ref iFlags);

		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt","Yo",ref iFlags);

		int i; string toAdd,moved;
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe1/loop"+i);
			moved = ("loop"+i);

			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);

			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved, ref iFlags);

			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe2/loop"+i);
			moved = ("loop"+i);

			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);

			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "psml://Root/FixedMe2/loop"+i+"/Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe3/loop"+i);
			moved = ("loop"+i);

			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);

			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved, ref iFlags);

			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe4/loop"+i);
			moved = ("loop"+i);

			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);

			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved, ref iFlags);

			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe5/loop"+i);
			moved = ("loop"+i);

			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);

			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved, ref iFlags);

			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
	
		// Read some random elements/atributes
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish1/Att 1", ref iFlags);
		Debug.Log ("Read attribute. Expecting 29 = "+read);
		
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", ref iFlags);
		Debug.Log ("Read attribute. Expecting 15 = "+read);
		
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt", ref iFlags);
		Debug.Log ("Read attribute. Expecting 'Yo' = "+read);
		
		iFlags = PIXE_PSML_READ_ELEMENT;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedDo", ref iFlags);
		Debug.Log ("Read element. Expecting true = "+read);

		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/FixedMe3/loop3/Blam", ref iFlags);
		Debug.Log ("Read attribute (LOOP). Expecting 3: "+read);

		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/FixedMe1/loop4/Blam", ref iFlags);
		Debug.Log ("Read attribute (LOOP). Expecting 4: "+read);

		// Overwrite an attribute 
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, ref iFlags);

		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", ref iFlags);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);
		
		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,ref iFlags);

		// Display the entire ocean
		Molecule current;
		for (i=0; i<200; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}

		Jelly.freeSession (ref thisSession, ref iFlags);

	}

	// Tests for elements overlapping

	private void test5() {
		
		Debug.Log ("***************** TEST 5 *********************");
		
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;
		
		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, ref iFlags);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];
		
		// Write the elements & attributes
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Root",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Jellyfish1",null,ref iFlags);
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Jellyfish1",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Nested1a",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"Att 1",29,ref iFlags);
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "..",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Jellyfish2",null,ref iFlags);
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Jellyfish2",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Nested2a",null,ref iFlags);
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Nested2a",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write (thisSession, "Nested2att", 15, ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Alex",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/PROBLEM",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedDo",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedMe",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Fixed?Egon",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Crash",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Crash2",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Crash!!!",null,ref iFlags);
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "Crash!!!",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"Crash!ATT",4,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/FixedMe1",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/FixedMe2",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/FixedMe3",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/FixedMe4",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/FixedMe5",null,ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt","Yo",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers","54",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers2","55",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers3","56",ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers4","57",ref iFlags);
		
		int i; string toAdd,moved;
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe1/loop"+i);
			moved = ("loop"+i);
			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);
			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved,ref iFlags);
			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe2/loop"+i);
			moved = ("loop"+i);
			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);
			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "psml://Root/FixedMe2/loop"+i+"/Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe3/loop"+i);
			moved = ("loop"+i);
			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);
			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved,ref iFlags);
			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe4/loop"+i);
			moved = ("loop"+i);
			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);
			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved,ref iFlags);
			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe5/loop"+i);
			moved = ("loop"+i);
			iFlags = PIXE_PSML_WRITE_ELEMENT;
			Jelly.Write(thisSession,toAdd,null,ref iFlags);
			iFlags = PIXE_PSML_UNSET_FLAG;
			Jelly.Move(thisSession, moved,ref iFlags);
			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			Jelly.Write (thisSession, "Blam", i, ref iFlags);
		}
		
		// Read some random elements/atributes
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish1/Att 1", ref iFlags);
		Debug.Log ("Read attribute. Expecting 29 = "+read);
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", ref iFlags);
		Debug.Log ("Read attribute. Expecting 15 = "+read);
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt", ref iFlags);
		Debug.Log ("Read attribute. Expecting 'Yo' = "+read);
		iFlags = PIXE_PSML_READ_ELEMENT;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedDo", ref iFlags);
		Debug.Log ("Read element. Expecting true = "+read);
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/FixedMe3/loop3/Blam", ref iFlags);
		Debug.Log ("Read attribute (LOOP). Expecting 3: "+read);
		
		// Overwrite an attribute 
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, ref iFlags);
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", ref iFlags);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);
		
		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,ref iFlags);

		// Display the entire ocean
		Molecule current;
		for (i=0; i<200; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.freeSession (ref thisSession, ref iFlags);
		
	}

	// Tests for elements overlapping

	private void test6() {
		
		Debug.Log ("***************** TEST 6 *********************");
		
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;

		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, ref iFlags);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];
		
		// Write the elements & attributes
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Root",null,ref iFlags);
		Jelly.Write(thisSession,"Jellyfish1",null,ref iFlags);
		Jelly.Write(thisSession,"Jellyfish2",null,ref iFlags);
		Jelly.Write(thisSession,"Jellyfish3",null,ref iFlags);
		Jelly.Write(thisSession,"Jellyfish4",null,ref iFlags);
		Jelly.Write(thisSession,"Jellyfish5",null,ref iFlags);
		Jelly.Write(thisSession,"Jellyfish6",null,ref iFlags);


		int i,j,k; string toAdd,moved;
		for (j=1; j<7; j++) {
			for (i=0; i<50; i++) {
				toAdd = ("psml://Root/Jellyfish" + j + "/loop" + i);
				//moved = ("loop" + i);
				//Jelly.Move(thisSession, moved);
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				Jelly.Write (thisSession, toAdd, i, ref iFlags);
				for(k=0;k<5;k++) {
					toAdd = ("psml://Root/Jellyfish" + j +"/loop"+i+ "/DEEP" + k);
					moved = "DEEP"+k;

					iFlags = PIXE_PSML_WRITE_ELEMENT;
					Jelly.Write (thisSession, toAdd, i, ref iFlags);

					iFlags = PIXE_PSML_UNSET_FLAG;
					Jelly.Move(thisSession, moved, ref iFlags);

					iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
					Jelly.Write (thisSession, "SLAM", i, ref iFlags);
				}
			}
		}

		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/loop3/DEEP1/SLAM", ref iFlags);
		Debug.Log ("Read attribute. Expecting 49: "+read);



		// Display the entire ocean
		Molecule current;
		int length = ocean.Count;
		for (i=0; i<200; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.freeSession (ref thisSession, ref iFlags);
		
	}
	// High volume test - over 1000 writes then 1000 reads 
	private void test7() {
		
		Debug.Log ("***************** TEST 7 *********************");
		
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		int iFlags = PIXE_PSML_UNSET_FLAG;
		
		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, ref iFlags);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];
		
		// Write the elements & attributes
		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"Root",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish2",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish3",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish4",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish5",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish6",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish7",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish8",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish9",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish10",null,ref iFlags);
		Jelly.Write(thisSession,"psml://Root/Jellyfish11",null,ref iFlags);

		int i, j;
		string toWrite, move;
		for (j=1; j<12; j++) {
			for (i=0; i<100; i++) {
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				toWrite = "psml://Root/Jellyfish"+j+"/Nested" + i;
				Jelly.Write (thisSession, toWrite, null, ref iFlags);
				iFlags = PIXE_PSML_UNSET_FLAG;
				move = "Nested" + i;
				Jelly.Move (thisSession, move, ref iFlags);
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				Jelly.Write (thisSession, "Attribute", i, ref iFlags);
			}
		}

		iFlags = PIXE_PSML_WRITE_ELEMENT;
		Jelly.Write(thisSession,"psml://Root/Jellyfish9/Nested65/GoldDust",null,ref iFlags);
		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.Move (thisSession, "GoldDust", ref iFlags);
		iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
		Jelly.Write (thisSession, "AlexIsOnFire", 101, ref iFlags);

		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish9/Nested65/GoldDust/AlexIsOnFire", ref iFlags);
		Debug.Log ("Read attribute AlexIsOnFire. Expecting 101: "+read);


		for (j=1; j<12; j++) {
			for (i=0; i<100; i++) {
				iFlags = PIXE_PSML_READ_ATTRIBUTE;
				toWrite = "psml://Root/Jellyfish"+j+"/Nested" + i+"/Attribute";

				read = Jelly.Read (thisSession,toWrite, ref iFlags);
				Debug.Log ("Read attribute " + toWrite+". Expecting "+i+": "+read);

			}
		}
		
		
		// Display the ocean (1st 200 molecules only)
		Molecule current;
		int length = ocean.Count;
		for (i=0; i<200; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}

		iFlags = PIXE_PSML_UNSET_FLAG;
		Jelly.freeSession (ref thisSession, ref iFlags);
		
	}
}
