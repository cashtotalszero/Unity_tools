/*
 * For use with any xml files meeting the psml specifications.
 * 
 * At present all unknown child tags are skipped (warning is printed in debug log).
 * */

using UnityEngine;
using System.Collections;
using System;						// NOTE the additonal inclusion of System.... 
using System.Xml;					// ...and System.Xml

public class JellyfishXML : MonoBehaviour {

	// xml file to read in must be stated in XmlReader declaration
	XmlReader myReader = XmlReader.Create("Assets/JellyFishV0.11.xml");
	// Booleans for all elements expected in psml file.
	private bool jellyfish = false;
	private bool appearance = false;
	private bool style = false;
	private bool blackhole = false;
	private bool resize = false;
	private bool title = false;
	private bool titleFont = false;
	private bool bungee = false;
	private bool bungeeConnector = false;
	private bool bungeeConnectorMarker = false;
	private bool bungeeLine = false;
	private bool blackholeGraphic = false;
	private bool blackholeHotArea = false;
	private bool styleGraphic = false;
	private bool resizeGraphic = false;
	private bool resizeHotArea = false;
	
	void Start () {

		string elementName;

		// Reader first steps into the root node (expecting <psml>)
		while (myReader.Read()) {
			if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "psml") {
				while (myReader.NodeType != XmlNodeType.EndElement) {
					myReader.Read();							// Step into the next node.
					elementName = myReader.Name;				// Get the element name from reader.

					// Process exepected nested tags: <Jellyfish>
					switch (elementName) {
						case "Jellyfish":
							jellyfish = true;
							getJellyfish(myReader,elementName);
							myReader.Read ();
							break;
						case "psml":							// Do nothing with parent element.
						case "":								// Likewise with non tags.
							break;
						// Skip any unknown tags & display a warning.
						default:
							Debug.Log("WARNING = Unknown element name found inside <psml> tag: " + elementName);
							myReader.Skip ();			 
							break;
					}
				}
			}
			// Display error if root node is incorrect
			else if(myReader.Name!="") {
				Debug.Log("ERROR = Unexpected root node. Expecting <psml> found " + myReader.Name);
			}

		}

		// Prints the status of all tags to debug window
		Debug.Log ("<Jellyfish>: " + jellyfish);
		Debug.Log ("\t<Appearance>: " + appearance);
		Debug.Log ("\t\t<BlackHole>: " + blackhole);
		Debug.Log ("\t\t\t<Graphic>: " + blackholeGraphic);
		Debug.Log ("\t\t\t<HotArea>: " + blackholeHotArea);
		Debug.Log ("\t\t<Style>: " + style);
		Debug.Log ("\t\t\t<Graphic>: " + styleGraphic);
		Debug.Log ("\t\t<Resize>: " + resize);
		Debug.Log ("\t\t\t<Graphic>: " + resizeGraphic);
		Debug.Log ("\t\t\t<HotArea>: " + resizeHotArea);
		Debug.Log ("\t\t<Title>: " + title);
		Debug.Log ("\t\t\t<Font>: " + titleFont);
		Debug.Log ("\t\t<Bungee>: " + bungee); 
		Debug.Log ("\t\t\t<Connector>: " + bungeeConnector); 
		Debug.Log ("\t\t\t\t<Marker>: " + bungeeConnectorMarker); 
		Debug.Log ("\t\t\t<Line>: " + bungeeLine); 
		Debug.Log ("END");
	}

	// Method checks attributes & nested tags inside <Jellyfish> tag
	void getJellyfish(XmlReader myReader, string elementName) {
			
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName = myReader.Name;

			// Process exepected nested tags: <Appearance>
			switch (elementName) {
				case "Appearance":
					appearance = true;
					getAppearance(myReader,elementName);
					myReader.Read ();
					break;
				case "Jellyfish":
				case "":							
					break;
				default:
					Debug.Log ("WARNING = Unknown element name found inside <Jellyfish> tag: " + elementName);
					myReader.Skip ();			 
					break;
			}
		}
	}
	
	// Method checks attributes & nested tags inside <Appearance> tag
	void getAppearance(XmlReader myReader, string elementName) {

		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;

			// Process exepected nested tags: <BlackHole>,<Style>,<Resize>,<Title>,<Bungee>
			switch(elementName)	{
				case "BlackHole":
					blackhole = true;
					getBlackHole(myReader,elementName);
					myReader.Read ();
					break;
				case "Style":
					style = true;
					getStyle (myReader,elementName);
					myReader.Read ();
					break;
				case "Resize":
					resize = true;
					getResize (myReader,elementName);
					myReader.Read ();
					break;
				case "Title":
					title = true;
					getTitle (myReader,elementName);
					myReader.Read ();
					break;
				case "Bungee":
					bungee = true;
					getBungee (myReader,elementName);
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

	// Method checks attributes & nested tags inside <BlackHole> tag
	void getBlackHole(XmlReader myReader,string elementName) {

		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;
			
			// Process exepected nested tags: <Graphic>,<HotArea>
			switch(elementName)	{
			case "Graphic":
				blackholeGraphic = true;
				myReader.Read ();
				break;
			case "HotArea":
				blackholeHotArea = true;
				myReader.Read ();
				break;
			case "BlackHole":
			case "":								
				break;
			default:
				Debug.Log("WARNING = Unknown element name found inside <BlackHole> tag: " + elementName);
				myReader.Skip();			 
				break;
			}
		}
	}

	// Method checks attributes & nested tags inside <Resize> tag
	void getResize(XmlReader myReader,string elementName) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;
			
			// Process exepected nested tags: <Graphic>,<HotArea>
			switch(elementName)	{
			case "Graphic":
				resizeGraphic = true;
				myReader.Read ();
				break;
			case "HotArea":
				resizeHotArea = true;
				myReader.Read ();
				break;
			case "Resize":
			case "":								
				break;
			default:
				Debug.Log("WARNING = Unknown element name found inside <Resize> tag: " + elementName);
				myReader.Skip();			 
				break;
			}
		}
	}

	// Method checks attributes & nested tags inside <Style> tag
	void getStyle(XmlReader myReader,string elementName) {
			
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;
			
			// Process exepected nested tags: <Graphic>
			switch(elementName)	{
			case "Graphic":
				styleGraphic = true;
				myReader.Read ();
				break;
			case "Style":
			case "":								
				break;
			default:
				Debug.Log("WARNING = Unknown element name found inside <Style> tag: " + elementName);
				myReader.Skip();			 
				break;
			}
		}
	}

	// Method checks attributes & nested tags inside <Title> tag
	void getTitle(XmlReader myReader,string elementName) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;
			
			// Process exepected nested tags: <Font>
			switch(elementName)	{
			case "Font":
				titleFont = true;
				myReader.Read ();
				break;
			case "Title":
			case "":								
				break;
			default:
				Debug.Log("WARNING = Unknown element name found inside <Title> tag: " + elementName);
				myReader.Skip();			 
				break;
			}
		}
	}

	// Method checks attributes & nested tags inside <Bungee> tag
	void getBungee(XmlReader myReader,string elementName) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;
			
			// Process exepected nested tags: <Connector>,<Line>
			switch(elementName)	{
			case "Connector":
				bungeeConnector = true;
				getConnector(myReader,elementName);
				myReader.Read ();
				break;
			case "Line":
				bungeeLine = true;
				myReader.Read ();
				break;
			case "Bungee":
			case "":								
				break;
			default:
				Debug.Log("WARNING = Unknown element name found inside <Bungee> tag: " + elementName);
				myReader.Skip();			 
				break;
			}
		}
	}

	// Method checks attributes & nested tags inside <Connector> tag
	void getConnector(XmlReader myReader,string elementName) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {
			myReader.Read ();	
			elementName=myReader.Name;
			
			// Process exepected nested tags: <Marker>
			switch(elementName)	{
			case "Marker":
				bungeeConnectorMarker = true;
				myReader.Read ();
				break;
			case "Connector":
			case "":								
				break;
			default:
				Debug.Log("WARNING = Unknown element name found inside <Connector> tag (child of <Bungee>): " + elementName);
				myReader.Skip();			 
				break;
			}
		}
	}
}
