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
using System.Collections.Generic;

public class JellyfishXMLgui : MonoBehaviour {


	// iFlag definitions (must be greater than 0)
	const int PIXE_PSML_READ_ELEMENT = 1;
	const int PIXE_PSML_WRITE_ELEMENT = 2;
	const int PIXE_PSML_READ_ATTRIBUTE = 3;
	const int PIXE_PSML_WRITE_ATTRIBUTE = 4;
	const int PIXE_PSML_UNSET_FLAG = 5;
	const int PIXE_PSML_ATTRIBUTE = 5;
	const int PIXE_PSML_ELEMENT = 6;
	
	const int PIXE_OCEAN_HOME = 0;
	const int PIXE_RESET = -1;

	public Heap API;
	public XmlValidator XmlValidator;

	// xml file to read in must be stated in XmlReader declaration
	XmlReader myReader;
	
	// Booleans for all expected elements in each jellyfish.
	private bool jellyfishWritten = false;
	private bool appearance = false;
	private bool appearanceWritten = false;
	private bool style = false;
	private bool styleWritten = false;
	private bool blackhole = false;
	private bool blackholeWritten = false;
	private bool resize = false;
	private bool resizeWritten = false;
	private bool title = false;
	private bool titleWritten = false;
	private bool titleFont = false;
	private bool titleFontWritten = false;
	private bool bungee = false;
	private bool bungeeWritten = false;
	private bool bungeeConnector = false;
	private bool bungeeConnectorWritten = false;
	private bool bungeeConnectorMarker = false;
	private bool bungeeConnectorMarkerWritten = false;
	private bool bungeeLine = false;
	private bool bungeeLineWritten = false;
	private bool blackholeGraphic = false;
	private bool blackholeGraphicWritten = false;
	private bool blackholeHotArea = false;
	private bool blackholeHotAreaWritten = false;
	private bool styleGraphic = false;
	private bool styleGraphicWritten = false;
	private bool resizeGraphic = false;
	private bool resizeGraphicWritten = false;
	private bool resizeHotArea = false;
	private bool resizeHotAreaWritten = false;

	// Booleans for log
	private bool rootError = false;
	private bool xmlError = false;
	private bool warning = false;
	private string myLog;
	string jellyfishName;

	int iOceanIndex = PIXE_RESET;			
	int thisSession = PIXE_RESET;
	int iFlags = PIXE_PSML_UNSET_FLAG;
	bool secondPass = false;

	private int graphicCount;

	public bool success = false;
	public bool load = false;

	void Start()
	{
		graphicCount = 0;

	}

	void Update() {
		if (load) {
			guiText.text = "SUCESS :)";
		} else {
			guiText.text = "FAIL : (";
		}
	}

	public void Load () {

		string elementName;
		myLog = "------------------------------------------------------------\nWARNINGS:";
		int i;

		TextAsset psml = Resources.Load("Assets/psml.xml") as TextAsset;

		if (XmlValidator.validateXML ("Assets/psml.xml")) {
	


			load = true;
			// Makes 2 passes
			for (i=0;i<2;i++) {
				myReader = XmlReader.Create ("Assets/psml.xml");
			
				while (myReader.Read()) {

					// Skip the Xml header
					if (myReader.NodeType == XmlNodeType.XmlDeclaration) {
						myReader.Skip ();
					}
					// Step into the root node (expecting <psml>)
					if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "psml") {

						// On the 2nd pass - write the root node into the Jellyfish ocean
						if(secondPass) {

							// Initialise the session and write the root node
							API.Initialise (ref thisSession, iOceanIndex, ref iFlags);
							iFlags = PIXE_PSML_WRITE_ELEMENT;
							API.Write(thisSession,myReader.Name,null,ref iFlags);
						}

						while (myReader.NodeType != XmlNodeType.EndElement) {
							myReader.Read ();			
							elementName = myReader.Name;	
	
							// Process exepected nested tags: <Jellyfish>
							switch (elementName) {
							case "Jellyfish":
							    // Get the name and reset all tags before processing each jellyfish
								jellyfishName = myReader.GetAttribute ("Name");
								resetTags ();
							    // Process the jellyfish and output tags statuses to log
								getJellyfish (myReader, elementName);
							
								if(secondPass) {
									API.Move(thisSession,"..", ref iFlags);
								}

								tagsToLog (jellyfishName);
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
				// Update log & display the result on frist pass
				if (!warning) {
					myLog += "\nNone.\n";
				}
				myLog += "------------------------------------------------------------";
				if(!secondPass) {
					Debug.Log (myLog);
				}
				secondPass = true;
			}
		}
		// Display the entire ocean at the end
		Session session = API.sessionList [thisSession]; 
		List<Molecule> ocean = API.oceanList [session.Ocean];
		Molecule current;
		int length = ocean.Count;
		for (i=0; i<length; i++) {
			current = ocean[i];
			Debug.Log(i + " NAME: " + current.Name + " TYPE: " + current.Type +
			          " VALUE: " + current.Value + " DATA: " + current.Data);
		}



		Save (thisSession, ref ocean); 
		iFlags = PIXE_PSML_READ_ATTRIBUTE;
		object toRead = API.Read (thisSession, "psml://psml/Jellyfish/Appearance/BlackHole/HotArea/X", ref iFlags);
		Debug.Log ("EXPECTING 85.4: " + toRead);
		if (toRead.ToString() == "85.4") {
			Debug.Log("SUCESS!");
			success = true;
		}
	}



	public void Save(int iSessionIndex, ref List<Molecule> ocean) {

		// Ensure session cursor is set to the root node
		Session session = API.sessionList [thisSession]; 
		session.Cursor = (int)ocean [PIXE_OCEAN_HOME].Value;
		List<string> nestedElements;

		// Set up xml writer 
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
		{
			Indent = true,
			IndentChars = "\t",
			NewLineOnAttributes = true
		};
		XmlWriter xmlWriter = XmlWriter.Create("Assets/SAVED.xml",xmlWriterSettings);

		// Write the root node
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement(ocean[session.Cursor].Name);

	

		saveNestedElements (xmlWriter, ref session, ref ocean,"Jellyfish");

	
		xmlWriter.WriteEndElement();

		xmlWriter.WriteEndDocument();
		xmlWriter.Close();

	}

	private void saveNestedElements(XmlWriter xmlWriter, ref Session session, ref List<Molecule> ocean, string sParent)
	{
		// Look for any elements in the current Drop
		if(API.hasElements (ref session, ref ocean, PIXE_PSML_ELEMENT)) {
			List<string> nestedElements = API.getElements (ref session, ref ocean, PIXE_PSML_ELEMENT);

			/*
			Debug.Log("Parent elements: ");
			foreach(string ssElement in nestedElements) {
				Debug.Log(ssElement);
			}
			Debug.Log("****************");
*/

	


			// Move into each in turn
			foreach(string sElement in nestedElements) {
				if(sElement == sParent) {
					iFlags = PIXE_PSML_UNSET_FLAG;
	
					// Move the cursor into the element & write the xml start tag to file
					API.Move(session.ID, sElement, ref iFlags);
					xmlWriter.WriteStartElement (ocean [session.Cursor].Name);
	
					// Check for attributes
					if(API.hasElements(ref session, ref ocean, PIXE_PSML_ATTRIBUTE)) {	
						List<string>  attributes = API.getElements (ref session, ref ocean, PIXE_PSML_ATTRIBUTE);
						// Write each one
						foreach(string sAttribute in attributes) {
							API.Move (session.ID, sAttribute, ref iFlags);
							xmlWriter.WriteAttributeString(ocean[session.Cursor].Name,ocean[session.Cursor].Value.ToString());
						}
					}
					// Check for any nested elements
					if(API.hasElements (ref session, ref ocean, PIXE_PSML_ELEMENT)) {
						List<string> nestedElements2 = API.getElements (ref session, ref ocean, PIXE_PSML_ELEMENT);
						/*
						Debug.Log("Nested elements: ");
						foreach(string sssElement in nestedElements2) {
							Debug.Log(sssElement);
						}
						Debug.Log("****************");
*/
						foreach(string sNestedElement in nestedElements2) {
							saveNestedElements (xmlWriter, ref session, ref ocean, sNestedElement);
						}
					}
					// Write the xml end tag to file & move cursor back up to the parent
					xmlWriter.WriteEndElement ();
					API.Move(session.ID,"..", ref iFlags);
				}
			}
		}
	}


	// Method checks attributes & nested tags inside <Jellyfish> tag
	void getJellyfish(XmlReader myReader, string elementName) {
			
		while (myReader.NodeType != XmlNodeType.EndElement) {

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !jellyfishWritten) {

				// Element
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				API.Write(thisSession,myReader.Name,null,ref iFlags);
				API.Move(thisSession,myReader.Name, ref iFlags);

				// Attributes
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				API.Write(thisSession,"Name",myReader.GetAttribute("Name"),ref iFlags);
				API.Write(thisSession,"Type",myReader.GetAttribute("Type"),ref iFlags);
				API.Write(thisSession,"Resizable",myReader.GetAttribute("Resizable"),ref iFlags);
				jellyfishWritten = true;
			}

			// Process nested elements - expecting <Appearance>
			myReader.Read ();	
			elementName = myReader.Name;
			switch (elementName) {
			case "Appearance":
				appearance = true;
				getAppearance(myReader,elementName);
				myReader.Read ();
				if(secondPass) {
					API.Move(thisSession,"..", ref iFlags);
				}
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
		
			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !appearanceWritten) {

				// Element
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				API.Write(thisSession,myReader.Name,null,ref iFlags);
				API.Move(thisSession,myReader.Name, ref iFlags);

				// Attributes
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				API.Write(thisSession,"Status",myReader.GetAttribute("Status"),ref iFlags);
				API.Write(thisSession,"AspectRatio",myReader.GetAttribute("AspectRatio"),ref iFlags);
				API.Write(thisSession,"Spacing",myReader.GetAttribute("Spacing"),ref iFlags);
				API.Write(thisSession,"Mask",myReader.GetAttribute("Mask"),ref iFlags);
				appearanceWritten = true;
			}
			// Process exepected nested tags: <BlackHole>,<Style>,<Resize>,<Title>,<Bungee>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "BlackHole":
				blackhole = true;
				getBlackHole(myReader,elementName);
				myReader.Read ();
				/*if(secondPass) {
					API.Move(thisSession,"..", ref iFlags);
				}*/
				break;
			case "Style":
				style = true;
				getStyle (myReader,elementName);
				myReader.Read ();
				/*if(secondPass) {
					API.Move(thisSession,"..", ref iFlags);
				}*/
				break;
			case "Resize":
				resize = true;
				getResize (myReader,elementName);
				myReader.Read ();
				/*if(secondPass) {
					API.Move(thisSession,"..", ref iFlags);
				}*/
				break;
			case "Title":
				title = true;
				getTitle (myReader,elementName);
				myReader.Read ();
				/*if(secondPass) {
					API.Move(thisSession,"..", ref iFlags);
				}*/
				break;
			case "Bungee":
				bungee = true;
				getBungee (myReader,elementName);
				myReader.Read ();
				/*if(secondPass) {
					API.Move(thisSession,"..", ref iFlags);
				}*/
				break;
			case "Appearance":						// Do nothing with parent element.
			case "":								// Likewise with non tags.
				break;
			default:			 
				displayWarning(myReader,elementName,"Appearance");	
				break;
			}
			// Move the session cursor back up if neccessary
			if(secondPass) {
				if(elementName == "BlackHole" || elementName == "Style" ||
				   elementName == "Resize" || elementName == "Title" || elementName == "Bungee")
				{
					API.Move(thisSession,"..", ref iFlags);
				}
			}
		}
	}

	// Method checks attributes & nested tags inside <BlackHole> tag
	void getBlackHole(XmlReader myReader,string elementName) {



		while (myReader.NodeType != XmlNodeType.EndElement) {

			graphicCount++;


			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !blackholeWritten) {
				// Element
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				API.Write(thisSession,myReader.Name,null,ref iFlags);
				API.Move(thisSession,myReader.Name, ref iFlags);
				// Attributes
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				API.Write(thisSession,"X",myReader.GetAttribute("X"),ref iFlags);
				API.Write(thisSession,"Y",myReader.GetAttribute("Y"),ref iFlags);
				API.Write(thisSession,"Width",myReader.GetAttribute("Width"),ref iFlags);
				API.Write(thisSession,"Height",myReader.GetAttribute("Height"),ref iFlags);
				blackholeWritten = true;


			}
			// Process exepected nested tags: <Graphic>,<HotArea>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Graphic":
				blackholeGraphic = true;
		
				// Write nested graphic element (which has no children)
				if(secondPass && !blackholeGraphicWritten) {
					// Element
					iFlags = PIXE_PSML_WRITE_ELEMENT;
					API.Write(thisSession,myReader.Name,null,ref iFlags);
					API.Move(thisSession,myReader.Name, ref iFlags);
					// Attributes
					iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
					API.Write(thisSession,"Default",myReader.GetAttribute("Default"),ref iFlags);
					API.Write(thisSession,"Hover",myReader.GetAttribute("Hover"),ref iFlags);
					API.Write(thisSession,"Selected",myReader.GetAttribute("Selected"),ref iFlags);
					API.Write(thisSession,"Disabled",myReader.GetAttribute("Disabled"),ref iFlags);
					API.Move(thisSession,"..",ref iFlags);
					blackholeGraphicWritten = true;
				}

				myReader.Read ();
				break;
			case "HotArea":
				blackholeHotArea = true;
				//getHotArea(myReader,elementName,"BlackHole");

				if(secondPass && !blackholeHotAreaWritten) {
					// Element
					iFlags = PIXE_PSML_WRITE_ELEMENT;
					API.Write(thisSession,myReader.Name,null,ref iFlags);
					API.Move(thisSession,myReader.Name, ref iFlags);
					// Attributes
					iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
					API.Write(thisSession,"X",myReader.GetAttribute("X"),ref iFlags);
					API.Write(thisSession,"Y",myReader.GetAttribute("Y"),ref iFlags);
					API.Write(thisSession,"Width",myReader.GetAttribute("Width"),ref iFlags);
					API.Write(thisSession,"Height",myReader.GetAttribute("Height"),ref iFlags);
					API.Move(thisSession,"..", ref iFlags);
					blackholeHotAreaWritten = true;
				}



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

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !resizeWritten) {
				// Element
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				API.Write(thisSession,myReader.Name,null,ref iFlags);
				API.Move(thisSession,myReader.Name, ref iFlags);
				// Attributes
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				API.Write(thisSession,"MaxX",myReader.GetAttribute("MaxX"),ref iFlags);
				API.Write(thisSession,"MaxY",myReader.GetAttribute("MaxY"),ref iFlags);
				API.Write(thisSession,"Width",myReader.GetAttribute("Width"),ref iFlags);
				API.Write(thisSession,"Height",myReader.GetAttribute("Height"),ref iFlags);
				resizeWritten = true;
			}
			// Process exepected nested tags: <Graphic>,<HotArea>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Graphic":
				resizeGraphic = true;

				// Write nested graphic element (which has no children)
				if(secondPass && !resizeGraphicWritten) {
					// Element
					iFlags = PIXE_PSML_WRITE_ELEMENT;
					API.Write(thisSession,myReader.Name,null,ref iFlags);
					API.Move(thisSession,myReader.Name, ref iFlags);
					// Attributes
					iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
					API.Write(thisSession,"Default",myReader.GetAttribute("Default"),ref iFlags);
					API.Write(thisSession,"Hover",myReader.GetAttribute("Hover"),ref iFlags);
					API.Write(thisSession,"Selected",myReader.GetAttribute("Selected"),ref iFlags);
					API.Write(thisSession,"Disabled",myReader.GetAttribute("Disabled"),ref iFlags);
					API.Move(thisSession,"..",ref iFlags);
					resizeGraphicWritten = true;
				}

				myReader.Read ();
				break;
			case "HotArea":
				resizeHotArea = true;
				getHotArea(myReader,elementName,"Resize");
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

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !styleWritten) {

				// Element
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				API.Write(thisSession,myReader.Name,null,ref iFlags);
				API.Move(thisSession,myReader.Name, ref iFlags);
				
				// Attributes
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				API.Write(thisSession,"Border",myReader.GetAttribute("Border"),ref iFlags);
				API.Write(thisSession,"FillColour",myReader.GetAttribute("FillColour"),ref iFlags);


				styleWritten = true;
			}
			// Process exepected nested tags: <Graphic>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Graphic":
				styleGraphic = true;
			

				if(secondPass && !styleGraphicWritten) {
					// Element
					iFlags = PIXE_PSML_WRITE_ELEMENT;
					API.Write(thisSession,myReader.Name,null,ref iFlags);
					API.Move(thisSession,myReader.Name, ref iFlags);
					// Attributes
					iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
					API.Write(thisSession,"Default",myReader.GetAttribute("Default"),ref iFlags);
					API.Write(thisSession,"Ratio_50",myReader.GetAttribute("Ratio_50"),ref iFlags);
					API.Write(thisSession,"Ratio_150",myReader.GetAttribute("Ratio_150"),ref iFlags);
					API.Move(thisSession,"..",ref iFlags);
					styleGraphicWritten = true;
				}

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

	void getHotArea(XmlReader myReader, string elementName, string sParent)
	{
		bool bWritten;
			switch(sParent) {
			case "BlackHole":
				bWritten = blackholeHotAreaWritten;
				break;
			case "Resize":
				bWritten = resizeHotAreaWritten;
				break;
			default:
				Debug.Log("ERROR = Unknown HotArea nested tag.");
				return;
				break;
			}

		if(secondPass && !bWritten) {
			// Element
			iFlags = PIXE_PSML_WRITE_ELEMENT;
			API.Write(thisSession,myReader.Name,null,ref iFlags);
			API.Move(thisSession,myReader.Name, ref iFlags);
			// Attributes
			iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
			API.Write(thisSession,"X",myReader.GetAttribute("X"),ref iFlags);
			API.Write(thisSession,"Y",myReader.GetAttribute("Y"),ref iFlags);
			API.Write(thisSession,"Width",myReader.GetAttribute("Width"),ref iFlags);
			API.Write(thisSession,"Height",myReader.GetAttribute("Height"),ref iFlags);
			API.Move(thisSession,"..", ref iFlags);

			switch(sParent) {
			case "BlackHole":
				blackholeHotAreaWritten = true;;
				break;
			case "Resize":
				resizeHotAreaWritten = true;
				break;
			default:
				break;
			}
		}
	}
	
	// Method checks attributes & nested tags inside <Title> tag
	void getTitle(XmlReader myReader,string elementName) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !titleWritten) {

				// Element
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				API.Write(thisSession,myReader.Name,null,ref iFlags);
				API.Move(thisSession,myReader.Name, ref iFlags);

				// Attributes
				iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
				API.Write(thisSession,"X",myReader.GetAttribute("X"),ref iFlags);
				API.Write(thisSession,"Y",myReader.GetAttribute("Y"),ref iFlags);
				API.Write(thisSession,"Width",myReader.GetAttribute("Width"),ref iFlags);
				API.Write(thisSession,"Height",myReader.GetAttribute("Height"),ref iFlags);
				API.Write(thisSession,"Style",myReader.GetAttribute("Style"),ref iFlags);
				API.Write(thisSession,"HorizontalAlignment",myReader.GetAttribute("HorizontalAlignment"),ref iFlags);
				API.Write(thisSession,"VerticalAlignment",myReader.GetAttribute("VerticalAlignment"),ref iFlags);
				titleWritten = true;
			}
			// Process exepected nested tags: <Font>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Font":
				titleFont = true;


				// Write the Jellyfish element/attribute details into ocean
				if(secondPass && !titleFontWritten) {
					
					// Element
					iFlags = PIXE_PSML_WRITE_ELEMENT;
					API.Write(thisSession,myReader.Name,null,ref iFlags);
					API.Move(thisSession,myReader.Name, ref iFlags);
					
					// Attributes
					iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
					API.Write(thisSession,"Face",myReader.GetAttribute("Face"),ref iFlags);
					API.Write(thisSession,"Weight",myReader.GetAttribute("Weight"),ref iFlags);
					API.Write(thisSession,"Size",myReader.GetAttribute("Size"),ref iFlags);
					API.Write(thisSession,"Colour",myReader.GetAttribute("Colour"),ref iFlags);
					titleFontWritten = true;
				}
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

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !bungeeWritten) {

				// Element
				iFlags = PIXE_PSML_WRITE_ELEMENT;
				API.Write(thisSession,myReader.Name,null,ref iFlags);
				API.Move(thisSession,myReader.Name, ref iFlags);

				// Currently no attributes in this tag.
				bungeeWritten = true;
			}
			// Process exepected nested tags: <Connector>,<Line>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Connector":
				bungeeConnector = true;
				getConnector(myReader,elementName);
				myReader.Read ();
				break;
			case "Line":
				bungeeLine = true;

				// Write the Jellyfish element/attribute details into ocean
				if(secondPass && !bungeeLineWritten) {
					
					// Element
					iFlags = PIXE_PSML_WRITE_ELEMENT;
					API.Write(thisSession,myReader.Name,null,ref iFlags);
					API.Move(thisSession,myReader.Name, ref iFlags);
					
					// Attributes
					iFlags = PIXE_PSML_WRITE_ATTRIBUTE;
					API.Write(thisSession,"Colour",myReader.GetAttribute("Colour"),ref iFlags);
					API.Write(thisSession,"MaxWidth",myReader.GetAttribute("MaxWidth"),ref iFlags);
					API.Write(thisSession,"WidthPerLayer",myReader.GetAttribute("WidthPerLayer"),ref iFlags);
					API.Write(thisSession,"AvoidJellyfish",myReader.GetAttribute("AvoidJellyfish"),ref iFlags);
					API.Write(thisSession,"MergeLinesAtSameLevel",myReader.GetAttribute("MergeLinesAtSameLevel"),ref iFlags);
					bungeeLineWritten = true;
				}
			
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


	// INCOMPLETE
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

	
	private void resetTags()
	{
		jellyfishWritten = false;
		appearance = false;
		appearanceWritten = false;
		style = false;
		styleWritten = false;
		blackhole = false;
		blackholeWritten = false;
		resize = false;
		resizeWritten = false;
		title = false;
		titleWritten = false;
		titleFont = false;
		titleFontWritten = false;
		bungee = false;
		bungeeWritten = false;
		bungeeConnector = false;
		bungeeConnectorWritten = false;
		bungeeConnectorMarker = false;
		bungeeConnectorMarkerWritten = false;
		bungeeLine = false;
		bungeeLineWritten = false;
		blackholeGraphic = false;
		blackholeGraphicWritten = false;
		blackholeHotArea = false;
		blackholeHotAreaWritten = false;
		styleGraphic = false;
		styleGraphicWritten = false;
		resizeGraphic = false;
		resizeGraphicWritten = false;
		resizeHotArea = false;
		resizeHotAreaWritten = false;
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

}
