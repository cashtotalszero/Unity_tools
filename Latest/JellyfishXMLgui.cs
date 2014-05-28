/*
 * For use with any xml files meeting the psml (Pocket Spacecraft Markup Language) specifications.
 * 
 * Current version validates XML file and reads through all elements. A list of all expected elements
 * is printed to the log file with a boolean expression: True if the element was found in the file,
 * else False.
 * 
 * At present a warning is printed if any unknown elements are encountered and that element
 * (along with all nested elements) is skipped.
 * */

using UnityEngine;
using System.Collections;
using System;						// NOTE the additonal inclusion of System.... 
using System.Xml;					// ...and System.Xml
using System.Xml.Schema;
using System.IO;

public class JellyfishXMLgui : MonoBehaviour {

	public XmlValidator XmlValidator;

	// xml file to read in must be stated in XmlReader declaration
	XmlReader myReader = XmlReader.Create("Assets/psml.xml");
	
	// Booleans for all expected elements in each jellyfish.
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

	// Booleans for log
	private bool rootError = false;
	private bool xmlError = false;
	private bool warning = false;
	private string myLog;
	string jellyfishName;
	
	void Start () {

		string elementName;
		myLog = "------------------------------------------------------------\nWARNINGS:";

		if (XmlValidator.validateXML ("Assets/psml.xml")) {
			while (myReader.Read()) {

				// Skip the Xml header
				if(myReader.NodeType == XmlNodeType.XmlDeclaration) {
					myReader.Skip();
				}
				// Step into the root node (expecting <psml>)
				if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "psml") {
					while (myReader.NodeType != XmlNodeType.EndElement) {
						myReader.Read ();			
						elementName = myReader.Name;	
	
						// Process exepected nested tags: <Jellyfish>
						switch (elementName) {
						case "Jellyfish":
							// Get the name and reset all tags before processing each jellyfish
							jellyfishName = myReader.GetAttribute("Name");
							resetTags();
							// Process the jellyfish and output tags statuses to log
							getJellyfish (myReader, elementName);
							tagsToLog(jellyfishName);
							// Move to the next jellyfish (if any)
							myReader.Read ();
							break;
						case "psml":							// Do nothing with parent element.
						case "":								// Likewise with non tags.
							break;
						// Skip any unknown tags & display a warning.
						default:		 
							displayWarning (myReader, elementName, "psml");	
							break;
						}
					}
				}
				// Display error if root node is incorrect
				else if (myReader.Name != "") {
					rootError = true;
				}
			}
			// Update log if necessary
			if (!warning) {
				myLog += "\nNone.";
			}
			myLog += "------------------------------------------------------------";
			Debug.Log(myLog);
		} 
	}

	private void resetTags()
	{
		appearance = false;
		style = false;
	 	blackhole = false;
		resize = false;
		title = false;
		titleFont = false;
		bungee = false;
		bungeeConnector = false;
		bungeeConnectorMarker = false;
		bungeeLine = false;
		blackholeGraphic = false;
		blackholeHotArea = false;
		styleGraphic = false;
		resizeGraphic = false;
		resizeHotArea = false;
	}

	private void tagsToLog(string name)
	{
		Debug.Log (
			"Tag status for Jellyfish: " + name + "\n" +
			"TOP LEVEL TAGS\n" +
			"Appearance = " + appearance + "\n" +
			"Style = " + style + "\n" +
			"BlackHole = " + blackhole + "\n" +
			"Resize = " + resize + "\n" +
			"Title = " + title + "\n" +
			"Bungee = " + bungee + "\n\n" +
			"NESTED TAGS (parent in brackets)\n" +
			"Graphic (Style) = " + styleGraphic + "\n" +
			"Graphic (BlackHole) = " + blackholeGraphic + "\n" +
			"HotArea (BlackHole) = " + blackholeHotArea + "\n" +
			"Graphic (Resize) = " + resizeGraphic + "\n" +
			"HotArea (Resize) = " + resizeHotArea + "\n" +
			"Font (Title) = " + titleFont + "\n" +
			"Connector (Bungee) = " + bungeeConnector + "\n" +
			"Marker (Bungee->Connector) = " + bungeeConnectorMarker + "\n" +
			"Line (Bungee) = " + bungeeLine + "\n\n"
			);
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
				displayWarning(myReader,elementName,"Jellyfish");	
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
				displayWarning(myReader,elementName,"Appearance");	
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
				displayWarning(myReader,elementName,"BlackHole");
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
				displayWarning(myReader,elementName,"Resize");
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
				displayWarning(myReader,elementName,"Style");
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
				displayWarning(myReader,elementName,"Title");
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
				displayWarning(myReader,elementName,"Bungee");
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
				displayWarning(myReader,elementName,"Connector");
				break;
			}
		}
	}

	// Method updates warning log and moves XML reader passed unknown elements
	void displayWarning(XmlReader myReader,string elementName,string parentTag)
	{
		// Set warning flag and add details to error log
		warning = true;
		myLog += "\nFound in Jellyfish '" + jellyfishName + "':" +
		"\nUnknown element found inside <"+parentTag+"> tag: <"+elementName+">\n";

		// Reader skips this unrecognised element (and all child elements of this tag)
		myReader.Skip();
	}
	
}
