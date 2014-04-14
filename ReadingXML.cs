/*
 * This script is an example of how to retreive data from an xml file. 
 * To run:
 * 1) Create a new Unity project.
 * 2) Place ./MyExample.xml in the Assets folder.
 * 3) Add an empty game object to the scene.
 * 4) Add this script as a compnent of the empty game object.
 * 
 * The element and attribute information held in the xml file will be
 * printed to the Debug console.
*/

using UnityEngine;
using System.Collections;
using System;						// NOTE the additonal inclusion of System.... 
using System.Xml;					// ...and System.Xml

public class ReadingXML : MonoBehaviour {

	XmlReader myReader = XmlReader.Create("Assets/MyExample.xml");		// Declaration of xml reader
	private string toPrint;												// String to print to console

	// Function called at the start of the program
	void Start () {
		
		while (myReader.Read())
		{
			// Steps into the root node ("Document")
			if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "Document")
			{
				// Loop until reader reaches the end of the "Document" element
				while (myReader.NodeType != XmlNodeType.EndElement)
				{
					myReader.Read();							// Step into the next node ("Element1")

					// Element1 is a single value.
					if (myReader.Name == "Element1")
					{
						getValue(myReader);						// Find any contained values
						myReader.Read();						// Step to the next node ("Element2")
					}

					// Element2 holds two nested elements 
					if (myReader.Name == "Element2")
					{
						while (myReader.NodeType != XmlNodeType.EndElement)
						{
							myReader.Read();					// Step into the next node ("Nested1")	
							if (myReader.Name == "Nested1")
							{
								getValue(myReader);				// Find any contained values
								myReader.Read();				// Step into the next node ("Nested2")
							}
							
							if (myReader.Name == "Nested2")
							{
								getValue(myReader);				// Find any contained values			
							}
						}
						myReader.Read();						// Step to next node "Element3"
					}

					// Element3 has two attributes 
					if (myReader.Name == "Element3")
					{
						toPrint = myReader.GetAttribute("Attribute1");
						Debug.Log ("Attribute1: " + toPrint);
						toPrint = myReader.GetAttribute("Attribute2");
						Debug.Log ("Attribute2: " + toPrint);
					}
				}
			}
		}
		Debug.Log ("End");
	}

	// This method prints any element value to the console
	void getValue(XmlReader myReader) {

		// Loop until the reader reaches the end of the current element
		while (myReader.NodeType != XmlNodeType.EndElement) 
		{
			// Step through the node and print if it is text
			myReader.Read ();
			if (myReader.NodeType == XmlNodeType.Text) {
				toPrint = myReader.Value;
				Debug.Log ("Value = " + toPrint);
			}
		}
	}

}
