using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Tests : MonoBehaviour {

	// iFlag definitions (must be greater than 0)
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_WRITE_ELEMENT = 2;
	const int PIXE_PSML_READ_ATTRIBUTE = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;

	const int PIXE_OCEAN_HOME = 0;
	const int PIXE_RESET = -1;
	
	public Heap Jelly;
	
	void Start () {
	
		//test1 ();
		//test2 ();
		//test3 ();
		test4 ();
	}

	/*
	  Basic Write/Read:
	 	<A>
			<B att=”Alex” />
		</A>
	 */
	private void test1() {

		Debug.Log ("***************** TEST 1 *********************");

		// NOTE: If iOceanIndex = PIXE_RESET, a new empty ocean is created
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;

		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, 0);	
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];

		// Write the elements & attribute
		Jelly.oldWrite(thisSession,"A",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"B",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "B");
		Jelly.oldWrite(thisSession,"att","Alex",PIXE_PSML_WRITE_ATTRIBUTE);

		// Read using relative
		object read = Jelly.oldRead (thisSession,"att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Test attribute read (relative). Expecting 'Alex' = "+read);

		// Read using full path
		read = Jelly.oldRead (thisSession,"psml://A/B/att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Test attribute read (full path). Expecting 'Alex' = "+read);

		// Read element that does exist
		read = Jelly.oldRead (thisSession,"psml://A/B", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Test existing element read. Expecting 'true' = "+read);

		// Read element that doesn't exist
		Jelly.Move (thisSession, "..");
		read = Jelly.oldRead (thisSession,"C", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Test nonexistant element read. Expecting 'false' = "+read);

		Jelly.freeSession (ref thisSession);

	}
	/*
	 * Complex read & write which will involve movement of drops.
	 * */
	private void test2() {

		Debug.Log ("***************** TEST 2 *********************");

		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;

		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, 0);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];

		// Write the elements & attributes
		Jelly.oldWrite(thisSession,"Root",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"Jellyfish1",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish1");
		Jelly.oldWrite(thisSession,"Nested1a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"Att 1",29,PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Move (thisSession, "..");
		Jelly.oldWrite(thisSession,"Jellyfish2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish2");
		Jelly.oldWrite(thisSession,"Nested2a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Nested2a");
		Jelly.oldWrite (thisSession, "Nested2att", 15, PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish1/Alex",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish1/PROBLEM",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Fixed?",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedDo",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedMe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Fixed?Egon",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash!!!",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Crash!!!");
		Jelly.oldWrite(thisSession,"Crash!ATT",4,PIXE_PSML_WRITE_ATTRIBUTE);

		// Read some random elements/atributes
		object read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish1/Att 1", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 29 = "+read);

		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 15 = "+read);

		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/FixedDo", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Read element. Expecting true = "+read);

		// Overwrite an attribute 
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, PIXE_PSML_WRITE_ATTRIBUTE);
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);

		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);

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
		Jelly.oldWrite(thisSession,"Root",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"Jellyfish1",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish1");
		Jelly.oldWrite(thisSession,"Nested1a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"Att 1",29,PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Move (thisSession, "..");
		Jelly.oldWrite(thisSession,"Jellyfish2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish2");
		Jelly.oldWrite(thisSession,"Nested2a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Nested2a");
		Jelly.oldWrite (thisSession, "Nested2att", 15, PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish1/Alex",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish1/PROBLEM",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Fixed?",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedDo",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedMe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Fixed?Egon",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash!!!",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Crash!!!");
		Jelly.oldWrite(thisSession,"Crash!ATT",4,PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe1",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe3",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe4",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe5",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt","Yo",PIXE_PSML_WRITE_ATTRIBUTE);
		
		// Read some random elements/atributes
		object read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish1/Att 1", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 29 = "+read);
		
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 15 = "+read);
	
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 'Yo' = "+read);

		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/FixedDo", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Read element. Expecting true = "+read);
		
		// Overwrite an attribute 
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, PIXE_PSML_WRITE_ATTRIBUTE);
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);
		
		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);
		
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

	private void test4() {

		Debug.Log ("***************** TEST 4 *********************");
		
		int iOceanIndex = PIXE_RESET;			
		int thisSession = PIXE_RESET;
		
		// Initialise the session and declare the ocean
		Jelly.Initialise (ref thisSession, iOceanIndex, 0);
		Session session = Jelly.sessionList [thisSession]; 
		List<Molecule> ocean = Jelly.oceanList [session.Ocean];
		Molecule home = ocean [PIXE_OCEAN_HOME];
		
		// Write the elements & attributes
		Jelly.oldWrite(thisSession,"Root",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"Jellyfish1",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish1");
		Jelly.oldWrite(thisSession,"Nested1a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"Att 1",29,PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Move (thisSession, "..");
		Jelly.oldWrite(thisSession,"Jellyfish2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Jellyfish2");
		Jelly.oldWrite(thisSession,"Nested2a",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Nested2a");
		Jelly.oldWrite (thisSession, "Nested2att", 15, PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish1/Alex",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish1/PROBLEM",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Fixed?",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedDo",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedMe",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Fixed?Egon",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Crash!!!",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "Crash!!!");
		Jelly.oldWrite(thisSession,"Crash!ATT",4,PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe1",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe2",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe3",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe4",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/FixedMe5",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt","Yo",PIXE_PSML_WRITE_ATTRIBUTE);

		int i; string toAdd,moved;
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe1/loop"+i);
			moved = ("loop"+i);
			Jelly.oldWrite(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.oldWrite (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe2/loop"+i);
			moved = ("loop"+i);
			Jelly.oldWrite(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.oldWrite (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe3/loop"+i);
			moved = ("loop"+i);
			Jelly.oldWrite(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.oldWrite (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe4/loop"+i);
			moved = ("loop"+i);
			Jelly.oldWrite(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.oldWrite (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe5/loop"+i);
			moved = ("loop"+i);
			Jelly.oldWrite(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.oldWrite (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
	
		// Read some random elements/atributes
		object read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish1/Att 1", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 29 = "+read);
		
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 15 = "+read);
		
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 'Yo' = "+read);
		
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/FixedDo", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Read element. Expecting true = "+read);

		read = Jelly.oldRead (thisSession,"psml://Root/FixedMe3/loop3/Blam", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (LOOP). Expecting 3: "+read);

		// Overwrite an attribute 
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, PIXE_PSML_WRITE_ATTRIBUTE);
		read = Jelly.oldRead (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);
		
		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		Jelly.oldWrite(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);

		// Display the entire ocean
		Molecule current;
		for (i=0; i<200; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}

		Jelly.freeSession (ref thisSession);

	}
}
