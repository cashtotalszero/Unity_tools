/*
 * For use with any xml files meeting the psml (Pocket Spacecraft Markup Language) specifications.
 * 
 * Current version validates file and reads through all elements. A list of all expected elements
 * is printed on the GUI with a boolean expression: True if the element was found in the file,
 * else False.
 * 
 * At present a warning is printed if any unknown elements are encountered and that element
 * (along with all nested elements) is skipped.
 * */

using UnityEngine;
using System.Collections;
using System;						// NOTE the additonal inclusion of System.... 
using System.Xml;					// ...and System.Xml

public class JellyfishXMLgui : MonoBehaviour {
	
	// xml file to read in must be stated in XmlReader declaration
	XmlReader myReader = XmlReader.Create("Assets/Jellyfish.xml");
	
	// Booleans for all expected elements in psml file.
	private bool psml = false;
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

	// Booleans for log
	private bool rootError = false;
	private bool xmlError = false;
	private bool warning = false;
	private string myLog;
	
	void Start () {

		string elementName;
		myLog = "WARNINGS:";

		if (validateXML ()) {
	
			// Reader first steps into the root node (expecting <psml>)
			while (myReader.Read()) {
				if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "psml") {
					psml = true;
					while (myReader.NodeType != XmlNodeType.EndElement) {
						myReader.Read ();							// Step into the next node.
						elementName = myReader.Name;				// Get the element name from reader.
	
						// Process exepected nested tags: <Jellyfish>
						switch (elementName) {
						case "Jellyfish":
							jellyfish = true;
							getJellyfish (myReader, elementName);
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
				Debug.Log(myLog);
			}
		} 
	}

	// Method ensures provided XML file is well formed and valid
	bool validateXML() {

		XmlReader psml = XmlReader.Create("Assets/Jellyfish.xml");

		// Try loading the XML document...
		try
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(psml);
	
			// ...if no exceptions generated this is valid XML..
			return true;
		}
		// ...return false if invalid XML.
		catch(System.Xml.XmlException exception)
		{
			myLog=exception.ToString();			// Print exception details to log
			Debug.Log(myLog);
			xmlError=true;
			return false;
		}
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
		myLog+="\nUnknown element found inside <"+parentTag+"> tag: <"+elementName+">";
		Debug.Log (myLog);

		// Reader skips this unrecognised element (and all child elements of this tag)
		myReader.Skip();
	}

	// Method prints status of all tags to GUI
	void OnGUI() {

		if (xmlError) {
			GUI.Label (new Rect (10, 10, Screen.width, Screen.height),
			           "ERROR = Invalid or badly formed XML file\n"+myLog);
		} 
		else if (rootError) {
			GUI.Label (new Rect (10, 10, Screen.width, Screen.height),
			           "ERROR = Invalid root node. Expecting <psml>");
		}
		else {
			GUI.Label (new Rect (10, 10, Screen.width, Screen.height), 
			    "TAG STATUS:\n\n" +
				"TOP LEVEL TAGS\n" +
				"psml\t\t\t\t= " + psml + "\n" +
				"Jellyfish\t\t= " + jellyfish + "\n" +
				"Appearance\t= " + appearance + "\n" +
				"Style\t\t\t\t= " + style + "\n" +
				"BlackHole\t\t= " + blackhole + "\n" +
				"Resize\t\t\t= " + resize + "\n" +
				"Title\t\t\t\t= " + title + "\n" +
				"Bungee\t\t\t= " + bungee + "\n\n" +
				"NESTED TAGS (parent in brackets)\n" +
				"Graphic (Style)\t\t\t\t\t\t= " + styleGraphic + "\n" +
				"Graphic (BlackHole)\t\t\t\t= " + blackholeGraphic + "\n" +
				"HotArea (BlackHole)\t\t\t\t= " + blackholeHotArea + "\n" +
				"Graphic (Resize)\t\t\t\t\t= " + resizeGraphic + "\n" +
				"HotArea (Resize)\t\t\t\t\t= " + resizeHotArea + "\n" +
				"Font (Title)\t\t\t\t\t\t\t\t= " + titleFont + "\n" +
				"Connector (Bungee)\t\t\t\t= " + bungeeConnector + "\n" +
				"Marker (Bungee->Connector)\t= " + bungeeConnectorMarker + "\n" +
				"Line (Bungee)\t\t\t\t\t\t= " + bungeeLine + "\n\n" +
				myLog);
		} 
	}

}
