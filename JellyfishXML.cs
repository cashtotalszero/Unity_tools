using UnityEngine;
using System.Collections;
using System;						// NOTE the additonal inclusion of System.... 
using System.Xml;					// ...and System.Xml

public class JellyfishXML : MonoBehaviour {

	XmlReader myReader = XmlReader.Create("Assets/JellyAlex.xml");		// Declaration of xml reader
	private string toPrint;												// String to print to console
	private bool jellyfish = false;
	private bool appearance = false;
	private bool style = false;
	private bool blackhole = false;
	private bool resize = false;
	private bool title = false;
	private bool bungee = false;

	// Function called at the start of the program
	void Start () {

		while (myReader.Read())
		{
			// Steps into the root node ("Jellyfish")
			if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "Jellyfish")
			{
				jellyfish = true;
				// Loop until reader reaches the end of the "Jellyfish" element
				while (myReader.NodeType != XmlNodeType.EndElement)
				{
					myReader.Read();							// Step into the next node
				
					if (myReader.Name == "Appearance")
					{
						appearance=true;
						getAppearance(myReader);
						myReader.Read();						// Step to next node
					}
				
				}
			}
		}

		Debug.Log ("Jellyfish: " + jellyfish);
		Debug.Log ("Appearance: " + appearance);
		Debug.Log ("Style: " + style);
		Debug.Log ("BlackHole: " + blackhole);
		Debug.Log ("Resize: " + resize);
		Debug.Log ("Title: " + title);
		Debug.Log ("Bungee: " + bungee); 
		Debug.Log ("End");
		/*
		while (myReader.Read())
		{
			// Steps into the root node ("Jellyfish")
			if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "Jellyfish")
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
*/
	}

	// Deals with Appearance node
	void getAppearance(XmlReader myReader) 
	{
		while (myReader.NodeType != XmlNodeType.EndElement)
		{
			myReader.Read();					
			if (myReader.Name == "BlackHole")
			{
				blackhole=true;
				myReader.Read();				
			}
			
			else if (myReader.Name == "Style")
			{
				style=true;
				myReader.Read();
			}

			else if (myReader.Name == "Resize")
			{
				resize=true;
				myReader.Read();
			}

			else if (myReader.Name == "Title")
			{
				title=true;
				myReader.Read();
			}

			else if (myReader.Name == "Bungee")
			{
				bungee=true;
				myReader.Read();
			}

			// SKIP ANY UNKNOWN TAGS
			else if (myReader.Name !="Appearance" && myReader.Name !="") {
				myReader.Read();
			}
			
		}

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
