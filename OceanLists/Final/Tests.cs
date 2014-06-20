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
		//test5 ();

		Debug.Log ("FUNCTION USE COUNT:");
		Debug.Log ("Initialise() : "+ Jelly.initialise);
		Debug.Log ("Write() : "+ Jelly.write);
		Debug.Log ("Read() : "+ Jelly.read);
		Debug.Log ("Move() : "+ Jelly.move);
		Debug.Log ("freeSession() : "+ Jelly.freesession);
		Debug.Log ("createMolecule() : "+ Jelly.createmolecule);
		Debug.Log ("createNested() : "+ Jelly.createnested);
		Debug.Log ("findMolecule() : "+ Jelly.findmolecule);
		Debug.Log ("navigatePath() : "+ Jelly.navigatepath);
		Debug.Log ("createOcean() : "+ Jelly.createocean);
		Debug.Log ("preventDropOverlap() : "+ Jelly.preventdropoverlap);
		Debug.Log ("getDrop() : "+ Jelly.getdrop);
		Debug.Log ("moveDrop() : "+ Jelly.movedrop);
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
		Jelly.Write(thisSession,"A",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Write(thisSession,"B",null,PIXE_PSML_WRITE_ELEMENT);
		Jelly.Move (thisSession, "B");
		Jelly.Write(thisSession,"att","Alex",PIXE_PSML_WRITE_ATTRIBUTE);

		// Read using relative
		object read = Jelly.Read (thisSession,"att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Test attribute read (relative). Expecting 'Alex' = "+read);

		// Read using full path
		read = Jelly.Read (thisSession,"psml://A/B/att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Test attribute read (full path). Expecting 'Alex' = "+read);

		// Read element that does exist
		read = Jelly.Read (thisSession,"psml://A/B", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Test existing element read. Expecting 'true' = "+read);

		// Read element that doesn't exist
		Jelly.Move (thisSession, "..");
		read = Jelly.Read (thisSession,"C", PIXE_PSML_READ_ELEMENT);
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

		// Read some random elements/atributes
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish1/Att 1", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 29 = "+read);

		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 15 = "+read);

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
	/*
	 * Similar to test 3 but also forces ocean to increase in size
	 * */
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

		int i; string toAdd,moved;
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe1/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe2/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Write (thisSession, "psml://Root/FixedMe2/loop"+i+"/Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe3/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe4/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe5/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
	
		// Read some random elements/atributes
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish1/Att 1", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 29 = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 15 = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 'Yo' = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedDo", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Read element. Expecting true = "+read);

		read = Jelly.Read (thisSession,"psml://Root/FixedMe3/loop3/Blam", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (LOOP). Expecting 3: "+read);

		read = Jelly.Read (thisSession,"psml://Root/FixedMe1/loop4/Blam", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (LOOP). Expecting 4: "+read);

		// Overwrite an attribute 
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, PIXE_PSML_WRITE_ATTRIBUTE);
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);
		
		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);

		// Display the entire ocean
		Molecule current;
		for (i=0; i<200; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}

		Jelly.freeSession (ref thisSession);

	}
	/**
	 * Tests for elements overlapping
	 * */
	private void test5() {
		
		Debug.Log ("***************** TEST 5 *********************");
		
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
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers","54",PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers2","55",PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers3","56",PIXE_PSML_WRITE_ATTRIBUTE);
		Jelly.Write(thisSession,"psml://Root/Jellyfish1/Yeppers4","57",PIXE_PSML_WRITE_ATTRIBUTE);
		
		int i; string toAdd,moved;
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe1/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe2/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Write (thisSession, "psml://Root/FixedMe2/loop"+i+"/Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe3/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe4/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		for(i=0; i<6; i++) {
			toAdd = ("psml://Root/FixedMe5/loop"+i);
			moved = ("loop"+i);
			Jelly.Write(thisSession,toAdd,null,PIXE_PSML_WRITE_ELEMENT);
			Jelly.Move(thisSession, moved);
			Jelly.Write (thisSession, "Blam", i, PIXE_PSML_WRITE_ATTRIBUTE);
		}
		
		// Read some random elements/atributes
		object read = Jelly.Read (thisSession,"psml://Root/Jellyfish1/Att 1", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 29 = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 15 = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedMe/attattatt", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute. Expecting 'Yo' = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/FixedDo", PIXE_PSML_READ_ELEMENT);
		Debug.Log ("Read element. Expecting true = "+read);
		
		read = Jelly.Read (thisSession,"psml://Root/FixedMe3/loop3/Blam", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (LOOP). Expecting 3: "+read);
		
		// Overwrite an attribute 
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", 99, PIXE_PSML_WRITE_ATTRIBUTE);
		read = Jelly.Read (thisSession,"psml://Root/Jellyfish2/Nested2a/Nested2att", PIXE_PSML_READ_ATTRIBUTE);
		Debug.Log ("Read attribute (overwrite test). Expecting 99 = "+read);
		
		// Overwrite an element
		Debug.Log ("Overwrite element. Expecting error: ");
		Jelly.Write(thisSession,"psml://Root/Jellyfish2/FixedRe",null,PIXE_PSML_WRITE_ELEMENT);
		
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
