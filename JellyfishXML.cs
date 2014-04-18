/*
 * For use with JellyAlex.xml - INCOMPLETE
 * At present the Appearace tag works - all unknown children will be ingored
 * */

using UnityEngine;
using System.Collections;
using System;						// NOTE the additonal inclusion of System.... 
using System.Xml;					// ...and System.Xml

public class JellyfishXML : MonoBehaviour {

	XmlReader myReader = XmlReader.Create("Assets/JellyAlex.xml");		// Declaration of xml reader
	private bool jellyfish = false;
	private bool appearance = false;
	private bool style = false;
	private bool blackhole = false;
	private bool resize = false;
	private bool title = false;
	private bool bungee = false;

	// Function called at the start of the program
	/*
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
				
					if (myReader.Name == "Appearance") {
						appearance=true;
						getAppearance(myReader);
						myReader.Read();						// Step to next node
					}
					// Skip any unknown elements
					else if (myReader.Name!="Jellyfish" && myReader.Name !="") {
						myReader.Skip();
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
	}
*/
	void Start () {

		string elementName;

		while (myReader.Read()) {
			// Reader steps into the root node ("psml")
			if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "psml") {
				while (myReader.NodeType != XmlNodeType.EndElement) {
					myReader.Read();							// Step into the next node.
					elementName = myReader.Name;				// Get the element name from reader.

					// Process exepected nested tags: <Jellyfish>
					switch (elementName) {
						case "Jellyfish":
							jellyfish = true;
							getJellyfish(myReader);
							myReader.Read ();
							break;
						case "psml":							// Do nothing with parent element.
						case "":								// Likewise with non tags.
							break;
						default:
							Debug.Log("WARNING = Unknown element name found inside <psml> tag: " + elementName);
							myReader.Skip ();			 
							break;
					}

					/*
					if (myReader.Name == "Jellyfish") {
						jellyfish=true;
						getJellyfish(myReader);
						myReader.Read();						// Step to next node
					}
					// Skip any unknown elements
					else if (myReader.Name!="psml" && myReader.Name !="") {
						myReader.Skip();
					}
					*/
				}
			}
			// Display error if root node is incorrect
			else if(myReader.Name!="") {
				Debug.Log("ERROR = Unexpected root node. Expecting <psml> found " + myReader.Name);
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
	}

	// Method checks attributes & nested tags inside <Jellyfish> tag
	void getJellyfish(XmlReader myReader) {

		string elementName;
		
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName = myReader.Name;

			// Process exepected nested tags: <Appearance>
			switch (elementName) {
				case "Appearance":
					appearance = true;
					getAppearance(myReader);
					myReader.Read ();
					break;
				case "Jellyfish":						// Do nothing with parent element.
				case "":								// Likewise with non tags.
					break;
				default:
					Debug.Log ("WARNING = Unknown element name found inside <Jellyfish> tag: " + elementName);
					myReader.Skip ();			 
					break;
			}
		}
	}
	
	// Method checks attributes & nested tags inside <Appearance> tag
	void getAppearance(XmlReader myReader) {

		string elementName;

		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;

			// Process exepected nested tags: <BlackHole>,<Style>,<Resize>,<Title>,<Bungee>
			switch(elementName)	{
				case "BlackHole":
					blackhole = true;
					myReader.Read ();
					break;
				case "Style":
					style = true;
					myReader.Read ();
					break;
				case "Resize":
					resize = true;
					myReader.Read ();
					break;
				case "Title":
					title = true;
					myReader.Read ();
					break;
				case "Bungee":
					bungee = true;
					myReader.Read ();
					break;
				case "Appearance":						// Do nothing with parent element.
				case "":								// Likewise with non tags.
					break;
				default:
					Debug.Log("WARNING = Unknown element name found inside <Appearance> tag: " + elementName);
					myReader.Skip();			 
					break;
			}
		}
	}
	/*
	getBlackHole(XmlReader myReader) {

	}
*/
	/*
	// Deals with Appearance node
	void getAppearance(XmlReader myReader) 
	{
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();					
			if (myReader.Name == "BlackHole") {
				blackhole = true;
				myReader.Read ();				
			} else if (myReader.Name == "Style") {
				style = true;
				myReader.Read ();
			} else if (myReader.Name == "Resize") {
				resize = true;
				myReader.Read ();
			} else if (myReader.Name == "Title") {
				title = true;
				myReader.Read ();
			} else if (myReader.Name == "Bungee") {
				bungee = true;
				myReader.Read ();
			}
			// Skip any unknown tags 
			else if (myReader.Name != "Appearance" && myReader.Name != "") {
				myReader.Skip ();
			}
		}	
	}
	*/
}
