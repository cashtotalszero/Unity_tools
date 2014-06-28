//#define PIXE_DEBUG_LOG
#undef PIXE_DEBUG_LOG

using UnityEngine;
using System.Collections;
using System;						// NOTE the additonal inclusion of System.... 
using System.Xml;					// ...and System.Xml
using System.Xml.Schema;
using System.IO;
using System.Collections.Generic;

public class JellyfishXMLgui : MonoBehaviour {

	// References to other scripts
	public Constants PIXE;						// PIXE definitions 
	public Heap API;							// PSML API function calls

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

	// Debug log information
	private bool rootError = false;
	private bool xmlError = false;
	private bool warning = false;
	private string myLog;
	private string jellyfishName;

	private bool secondPass = false;
	private bool success = false;
	private int iFlags;
	private bool validXml = false;
		
	void Update() {

		// GUI display for testing purposes

		if (success) {
			guiText.text = "SUCESS :)\n Jellyfish Name = "+jellyfishName;

		} else {
			guiText.text = "FAIL : ( ";
		}
	
		// For touch screens - quit app when screen is touched
		if (Input.touchCount > 0) {
			Application.Quit();
		}
	}

	private void getXml(ref string sXml) {

		TextAsset taDefaultPsml = (TextAsset)Resources.Load("psml",typeof(TextAsset));
		bool bLoadAsTextAsset = false;
		string sPath;

		// Standalone can read/write files directly into application package root directory
		#if UNITY_STANDALONE
		sPath = Application.dataPath+"/psml.xml";
		#endif

		// iOS can only write to the application Documents directory (NOTE: Exclude .xml file extension)
		#if UNITY_IPHONE
		sPath = Application.dataPath + "/../../Documents/psml";
		#endif

		// For non-webplayer platforms: Read XML data from saved file if present, else load form default
		#if !UNITY_WEBPLAYER
		if(File.Exists(sPath)) {
			sXml = File.ReadAllText(sPath);
		}
		else {
			bLoadAsTextAsset = true;
		}
		#endif

		// Webplayer cannot read/write from file so always use the TextAsset
		#if UNITY_WEBPLAYER
		bLoadAsTextAsset = true;
				
		/*
		// DO NOT DELETE!!!
		// POSSIBLE ALTERNATIVE: HOW TO DOWNLOAD XML FROM WEB 
		// NOTE: this url needs to be corrected to make it work.
		string sUrl = "http://www.pocketspacecraft.com/psml.xml";
		WWW webResource = new WWW(sUrl);
		while(!webResource.isDone) {
			Debug.Log("Loading XML from the web...");
			sLoadedXml = webResource.text;
		} */
		#endif

		// If loading from default TextAsset:
		if(bLoadAsTextAsset) {		 
			if(taDefaultPsml == null)
			{
				iFlags = PIXE.OP_FAIL_XML_LOAD_ERROR;
				Debug.Log("ERROR = Failed to load TextAsset from Resources.");
			}
			else {
				sXml = taDefaultPsml.text;
			}
		}
	}

	public void LoadPsml (ref int iSessionIndex) {
		
		// Declare tools to read in psml (xml) data:
		XmlReader myReader;	
		string elementName, sLoadedXml = "\0";
		int i;
		iFlags = PIXE.PSML_UNSET_FLAG;
		
		// Initialise debugging log
		#if (PIXE_DEBUG_LOG)
		myLog = "------------------------------------------------------------\nWARNINGS:";
		#endif
	
		// Retrieve XML data and validate form
		getXml (ref sLoadedXml);
		validXml = xmlFormValid (sLoadedXml);

		// If XML file is valid, make 2 passes: 1st to check all els/atts are valid; 2nd to read them into memory
		if (validXml) {
			for (i=0; i<2; i++) {
				// Create an xml reader to read through the psml file
				myReader = XmlReader.Create (new StringReader(sLoadedXml));
				while (myReader.Read()) {
					
					// Skip the xml declaration header...
					if (myReader.NodeType == XmlNodeType.XmlDeclaration) {
						myReader.Skip ();
					}
					// ...and step into the root node (expecting <psml>)
					if (myReader.NodeType == XmlNodeType.Element && myReader.Name == "psml") {
						
						// On the 2nd pass: Write the root node into specified ocean
						if(secondPass) { 
							iFlags = PIXE.PSML_WRITE_ELEMENT;
							API.Write(iSessionIndex,myReader.Name,null,ref iFlags);
						}
						// Then read through all the nested elements
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
								getJellyfish (myReader, elementName, iSessionIndex);
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
				#if (PIXE_DEBUG_LOG)
				// Update log & display the result on frist pass
				if (!warning) {
					myLog += "\nNone.\n";
				}
				myLog += "------------------------------------------------------------";
				if(!secondPass) {
					Debug.Log (myLog);
				}
				#endif
				secondPass = true;
				myReader.Close();
			}
		}
		// Save the loaded ocean back into XML
		Session session = API.sessionList [iSessionIndex]; 
		List<Molecule> ocean = API.oceanList [session.Ocean];
		// Change the Jellyfish name (proof of write save)
		API.Write(iSessionIndex,"psml://psml/Jellyfish/Name","ALEXALEXALEX",ref iFlags);
		
		// Webplayer cannot write to file
		#if !UNITY_WEBPLAYER
		Save (iSessionIndex, ref ocean); 
		#endif
		
		// READ TEST
		iFlags = PIXE.PSML_READ_ATTRIBUTE;
		object toRead = API.Read (iSessionIndex, "psml://psml/Jellyfish/Appearance/BlackHole/HotArea/X", ref iFlags);
		Debug.Log ("EXPECTING 85.4: " + toRead);
		if (toRead.ToString() == "85.4") {
			Debug.Log("SUCESS!");
			success = true;
		}
	}
	
	// XML VALIDATION

	// Method ensures provided XML file is properly formed
	private bool xmlFormValid(string psmlFile)
	{
		//XmlReader psml = XmlReader.Create(psmlFile);
		XmlReader psml = XmlReader.Create(new StringReader(psmlFile));
		
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
			Debug.Log("ERROR = Invalid XML file provided.\n" + exception.ToString());
			return false;
		}
	}

	// SAVING OCEAN INTO XML
	
	// Saves ocean into an xml file
	public void Save(int iSessionIndex, ref List<Molecule> ocean) {

		XmlWriter xmlWriter;
		string sPath = "\0";

		// Ensure session cursor is set to the root node
		Session session = API.sessionList [iSessionIndex]; 
		session.Cursor = (int)ocean [PIXE.OCEAN_HOME].Value;
		List<string> nestedElements;

		// Set up xml writer 
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
		{
			Indent = true,
			IndentChars = "\t",
			NewLineOnAttributes = true
		};
		// Set write location according to platform
		#if UNITY_STANDALONE
		sPath = Application.dataPath + "/psml.xml";
		#endif

		#if UNITY_IPHONE
		sPath = Application.dataPath + "/../../Documents/psml";
		#endif
		xmlWriter = XmlWriter.Create(sPath, xmlWriterSettings);

		// Write the root node <psml>
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement(ocean[session.Cursor].Name);
			
		// Save all nested elements from <Jellyfish> onwards
		saveNestedElements (xmlWriter, ref session, ref ocean,"Jellyfish");

		// Finalise document & close the reader
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndDocument();
		xmlWriter.Close();
		return;
	}

	// Uses recursion to save all nested tags in the XML tree
	private void saveNestedElements(XmlWriter xmlWriter, ref Session session, ref List<Molecule> ocean, string sParent)
	{
		// Look for any elements in the current Drop
		if(API.hasElements (ref session, ref ocean, PIXE.PSML_ELEMENT)) {
			List<string> nestedElements = API.getElements (ref session, ref ocean, PIXE.PSML_ELEMENT);

			// Move into each in turn
			foreach(string sElement in nestedElements) {
				if(sElement == sParent) {
					iFlags = PIXE.PSML_UNSET_FLAG;
	
					// Move the cursor into the element & write the xml start tag to file
					API.Move(session.ID, sElement, ref iFlags);
					xmlWriter.WriteStartElement (ocean [session.Cursor].Name);
	
					// Check for attributes
					if(API.hasElements(ref session, ref ocean, PIXE.PSML_ATTRIBUTE)) {	
						List<string>  attributes = API.getElements (ref session, ref ocean, PIXE.PSML_ATTRIBUTE);
						// Write each one
						foreach(string sAttribute in attributes) {
							API.Move (session.ID, sAttribute, ref iFlags);
							xmlWriter.WriteAttributeString(ocean[session.Cursor].Name,ocean[session.Cursor].Value.ToString());
						}
					}
					// Check for any nested elements
					if(API.hasElements (ref session, ref ocean, PIXE.PSML_ELEMENT)) {
						List<string> nestedElements2 = API.getElements (ref session, ref ocean, PIXE.PSML_ELEMENT);

						// SWAP OUT FOR EACH (it's too slow!)
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

	// XML TAG PROCESSING - READING FROM XML 

	void writeElement(int iSessionIndex, string sElementName)
	{
		iFlags = PIXE.PSML_WRITE_ELEMENT;
		API.Write(iSessionIndex,sElementName,null,ref iFlags);
		API.Move(iSessionIndex,sElementName, ref iFlags);
		return;
	}

	// Method checks attributes & nested tags inside <Jellyfish> tag
	void getJellyfish(XmlReader myReader, string elementName, int iSessionIndex) {
			
		while (myReader.NodeType != XmlNodeType.EndElement) {

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !jellyfishWritten) {

				// Element
				writeElement(iSessionIndex,myReader.Name);

				// Attributes
				iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
				API.Write(iSessionIndex,"Name",myReader.GetAttribute("Name"),ref iFlags);
				API.Write(iSessionIndex,"Type",myReader.GetAttribute("Type"),ref iFlags);
				API.Write(iSessionIndex,"Resizable",myReader.GetAttribute("Resizable"),ref iFlags);
				jellyfishWritten = true;
			}

			// Process nested elements - expecting <Appearance>
			myReader.Read ();	
			elementName = myReader.Name;
			switch (elementName) {
			case "Appearance":
				appearance = true;
				getAppearance(myReader,elementName, iSessionIndex);
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
		// Move cursor back to parent
		if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}
		return;
	}
	
	// Method checks attributes & nested tags inside <Appearance> tag
	void getAppearance(XmlReader myReader, string elementName, int iSessionIndex) {

		while (myReader.NodeType != XmlNodeType.EndElement) {
	
			// ON FIRST PASS - CHECK ALL ATTRIBUTES ARE PRESENT - INCOMPLETE

			// On 2nd pass: Write the Jellyfish <Appearance> details into ocean:
			if(secondPass && !appearanceWritten) {

				// The Element Header
				writeElement(iSessionIndex,myReader.Name);

				// Attributes
				iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
				API.Write(iSessionIndex,"Status",myReader.GetAttribute("Status"),ref iFlags);
				API.Write(iSessionIndex,"AspectRatio",myReader.GetAttribute("AspectRatio"),ref iFlags);
				API.Write(iSessionIndex,"Spacing",myReader.GetAttribute("Spacing"),ref iFlags);
				API.Write(iSessionIndex,"Mask",myReader.GetAttribute("Mask"),ref iFlags);
				appearanceWritten = true;
			}

			// Process expected nested tags: <BlackHole>,<Style>,<Resize>,<Title>,<Bungee>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "BlackHole":
				blackhole = true;
				getBlackHole(myReader,elementName, iSessionIndex);
				myReader.Read ();
				break;
			case "Style":
				style = true;
				getStyle (myReader,elementName, iSessionIndex);
				myReader.Read ();
				break;
			case "Resize":
				resize = true;
				getResize (myReader,elementName, iSessionIndex);
				myReader.Read ();
				break;
			case "Title":
				title = true;
				getTitle (myReader,elementName, iSessionIndex);
				myReader.Read ();
				break;
			case "Bungee":
				bungee = true;
				getBungee (myReader,elementName, iSessionIndex);
				myReader.Read ();
				break;
			case "Appearance":							// Do nothing with parent element.
			case "":									// Likewise with non tags.
				break;
			default:			 
				displayWarning(myReader,elementName,"Appearance");	
				break;
			}
		}
		// Move cursor back to parent
		if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}
		return;
	}
	
	// Method checks attributes & nested tags inside <BlackHole> tag
	void getBlackHole(XmlReader myReader,string elementName, int iSessionIndex) {

		while (myReader.NodeType != XmlNodeType.EndElement) {

			// ON FIRST PASS - CHECK ALL ATTRIBUTES ARE PRESENT - INCOMPLETE

			// On 2nd pass: Write the <BlackHole> details into the specified ocean
			if(secondPass && !blackholeWritten) {

				// Element Header
				writeElement(iSessionIndex,myReader.Name);

				// Attributes
				iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
				API.Write(iSessionIndex,"X",myReader.GetAttribute("X"),ref iFlags);
				API.Write(iSessionIndex,"Y",myReader.GetAttribute("Y"),ref iFlags);
				API.Write(iSessionIndex,"Width",myReader.GetAttribute("Width"),ref iFlags);
				API.Write(iSessionIndex,"Height",myReader.GetAttribute("Height"),ref iFlags);
				blackholeWritten = true;
			}
			// Process exepected nested tags: <Graphic>,<HotArea>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Graphic":
				blackholeGraphic = true;
				getGraphic(myReader, elementName, "BlackHole", iSessionIndex);
				myReader.Read ();
				break;
			case "HotArea":
				blackholeHotArea = true;
				getHotArea(myReader, elementName, "BlackHole", iSessionIndex);
				myReader.Read ();
				break;
			case "BlackHole":
			case "":								
				break;
			default:			 
				displayWarning(myReader, elementName, "BlackHole");
				break;
			}
		}
		// Move session cursor back up to parent node
		if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}
		return;
	}
	
	// Method checks attributes & nested tags inside <Resize> tag
	void getResize(XmlReader myReader,string elementName, int iSessionIndex) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !resizeWritten) {

				// Element
				writeElement(iSessionIndex,myReader.Name);

				// Attributes
				iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
				API.Write(iSessionIndex,"MaxX",myReader.GetAttribute("MaxX"),ref iFlags);
				API.Write(iSessionIndex,"MaxY",myReader.GetAttribute("MaxY"),ref iFlags);
				API.Write(iSessionIndex,"Width",myReader.GetAttribute("Width"),ref iFlags);
				API.Write(iSessionIndex,"Height",myReader.GetAttribute("Height"),ref iFlags);
				resizeWritten = true;
			}
			// Process exepected nested tags: <Graphic>,<HotArea>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Graphic":
				resizeGraphic = true;
				getGraphic(myReader, elementName, "Resize", iSessionIndex);
				myReader.Read ();
				break;
			case "HotArea":
				resizeHotArea = true;
				getHotArea(myReader, elementName, "Resize", iSessionIndex);
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
		// Move session cursor back up to parent node
		if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}
		return;
	}

	// Method checks attributes & nested tags inside <Style> tag
	void getStyle(XmlReader myReader,string elementName, int iSessionIndex) {
			
		while (myReader.NodeType != XmlNodeType.EndElement) {

			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !styleWritten) {

				// Element
				writeElement(iSessionIndex,myReader.Name);
		
				// Attributes
				iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
				API.Write(iSessionIndex,"Border",myReader.GetAttribute("Border"),ref iFlags);
				API.Write(iSessionIndex,"FillColour",myReader.GetAttribute("FillColour"),ref iFlags);
				styleWritten = true;
			}
			// Process exepected nested tags: <Graphic>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Graphic":
				styleGraphic = true;
				getGraphic(myReader, elementName, "Style", iSessionIndex);
				myReader.Read ();
				break;
			case "Style":
			case "":								
				break;
			default:		 
				displayWarning(myReader, elementName, "Style");
				break;
			}
		}
		// Move session cursor back up to parent node
		if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}
		return;
	}

	// Method checks attributes & nested tags inside <Title> tag
	void getTitle(XmlReader myReader,string elementName, int iSessionIndex) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {
			
			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !titleWritten) {
				
				// Element
				writeElement(iSessionIndex,myReader.Name);
				
				// Attributes
				iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
				API.Write(iSessionIndex,"X",myReader.GetAttribute("X"),ref iFlags);
				API.Write(iSessionIndex,"Y",myReader.GetAttribute("Y"),ref iFlags);
				API.Write(iSessionIndex,"Width",myReader.GetAttribute("Width"),ref iFlags);
				API.Write(iSessionIndex,"Height",myReader.GetAttribute("Height"),ref iFlags);
				API.Write(iSessionIndex,"Style",myReader.GetAttribute("Style"),ref iFlags);
				API.Write(iSessionIndex,"HorizontalAlignment",myReader.GetAttribute("HorizontalAlignment"),ref iFlags);
				API.Write(iSessionIndex,"VerticalAlignment",myReader.GetAttribute("VerticalAlignment"),ref iFlags);
				titleWritten = true;
			}
			// Process exepected nested tags: <Font>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Font":
				titleFont = true;
				getFont(myReader, elementName, iSessionIndex);
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
		// Move session cursor back up to parent node
		if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}
		return;
	}

	// Method checks attributes & nested tags inside <Bungee> tag
	void getBungee(XmlReader myReader,string elementName, int iSessionIndex) {
		
		while (myReader.NodeType != XmlNodeType.EndElement) {
			
			// Write the Jellyfish element/attribute details into ocean
			if(secondPass && !bungeeWritten) {
				
				// Element
				writeElement(iSessionIndex,myReader.Name);
				
				// Currently no attributes in this tag.
				bungeeWritten = true;
			}
			// Process exepected nested tags: <Connector>,<Line>
			myReader.Read ();	
			elementName=myReader.Name;
			switch(elementName)	{
			case "Connector":
				bungeeConnector = true;
				getConnector(myReader, elementName, iSessionIndex);
				myReader.Read ();
				break;
			case "Line":
				bungeeLine = true;
				getBungeeLine(myReader, elementName, iSessionIndex);
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
		// Move session cursor back up to parent node
		if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}
		return;
	}

	// Method processes attributes & nested tags inside <Graphic> tag
	void getGraphic(XmlReader myReader, string elementName, string sParent, int iSessionIndex)
	{
		// Set the parent details: <Graphic> is a child of <BlackHole>, <Resize> or <Style>
		bool bWritten;
		switch(sParent) {
		case "BlackHole":
			bWritten = blackholeGraphicWritten;
			break;
		case "Resize":
			bWritten = resizeGraphicWritten;
			break;
		case "Style":
			bWritten = styleGraphicWritten;
			break;
		default:
			iFlags = PIXE.OP_FAIL_INVALID_PSML;
			Debug.Log("ERROR = Unknown Graphic nested tag.");
			return;
			break;
		}
		// Chreck/write element/attribute details into memory
		if(secondPass && !bWritten) {
			
			// Element
			writeElement(iSessionIndex,myReader.Name);
			
			// Attributes
			iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
			API.Write(iSessionIndex,"Default",myReader.GetAttribute("Default"),ref iFlags);
			if(sParent == "BlackHole" || sParent == "Resize") {
				API.Write(iSessionIndex,"Hover",myReader.GetAttribute("Hover"),ref iFlags);
				API.Write(iSessionIndex,"Selected",myReader.GetAttribute("Selected"),ref iFlags);
				API.Write(iSessionIndex,"Disabled",myReader.GetAttribute("Disabled"),ref iFlags);
			}
			else if(sParent == "Style") {
				API.Write(iSessionIndex,"Ratio_50",myReader.GetAttribute("Ratio_50"),ref iFlags);
				API.Write(iSessionIndex,"Ratio_150",myReader.GetAttribute("Ratio_150"),ref iFlags);
			}
			API.Move(iSessionIndex,"..", ref iFlags);
			
			// Set correct parent flag to written (this avoids overwrites)
			switch(sParent) {
			case "BlackHole":
				blackholeGraphicWritten = true;;
				break;
			case "Resize":
				resizeGraphicWritten = true;
				break;
			case "Style":
				styleGraphicWritten = true;
				break;
			default:
				break;
			}
		}
		return;
	}

	// Method processes attributes & nested tags inside <HotArea> tag
	void getHotArea(XmlReader myReader, string elementName, string sParent, int iSessionIndex)
	{
		// Set the parent details: <HotArea> is a child of <BlackHole> & <Resize>
		bool bWritten;
			switch(sParent) {
			case "BlackHole":
				bWritten = blackholeHotAreaWritten;
				break;
			case "Resize":
				bWritten = resizeHotAreaWritten;
				break;
			default:
				iFlags = PIXE.OP_FAIL_INVALID_PSML;
				Debug.Log("ERROR = Unknown HotArea nested tag.");
				return;
				break;
			}
		// Write element/attribute details into memory
		if(secondPass && !bWritten) {

			// Element
			writeElement(iSessionIndex,myReader.Name);

			// Attributes
			iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
			API.Write(iSessionIndex,"X",myReader.GetAttribute("X"),ref iFlags);
			API.Write(iSessionIndex,"Y",myReader.GetAttribute("Y"),ref iFlags);
			API.Write(iSessionIndex,"Width",myReader.GetAttribute("Width"),ref iFlags);
			API.Write(iSessionIndex,"Height",myReader.GetAttribute("Height"),ref iFlags);
			API.Move(iSessionIndex,"..", ref iFlags);

			// Set correct parent flag to written (avoids overwrites)
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
		return;
	}
	
	// Method processes attributes & nested tags inside <Font> tag
	void getFont(XmlReader myReader, string elementName, int iSessionIndex)
	{
		// FIRST PASS ATTRIBUTE CHECK TO BE ADDED
		// Write the Jellyfish element/attribute details into ocean
		if(secondPass && !titleFontWritten) {
				
			// Element
			writeElement(iSessionIndex,myReader.Name);
	
			// Attributes
			iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
			API.Write(iSessionIndex,"Face",myReader.GetAttribute("Face"),ref iFlags);
			API.Write(iSessionIndex,"Weight",myReader.GetAttribute("Weight"),ref iFlags);
			API.Write(iSessionIndex,"Size",myReader.GetAttribute("Size"),ref iFlags);
			API.Write(iSessionIndex,"Colour",myReader.GetAttribute("Colour"),ref iFlags);
			API.Move(iSessionIndex,"..",ref iFlags);
			titleFontWritten = true;
		}
		// NOTE, there are currently no nested elements in this node.
		return;
	}

	// Method processes attributes & nested tags inside <Line> tag
	void getBungeeLine(XmlReader myReader, string elementName, int iSessionIndex)
	{
		// FIRST PASS ATTRIBUTE CHECK TO BE ADDED

		// Write the Jellyfish element/attribute details into ocean
		if(secondPass && !bungeeLineWritten) {
			
			// Element
			writeElement(iSessionIndex,myReader.Name);

			// Attributes
			iFlags = PIXE.PSML_WRITE_ATTRIBUTE;
			API.Write(iSessionIndex,"Colour",myReader.GetAttribute("Colour"),ref iFlags);
			API.Write(iSessionIndex,"MaxWidth",myReader.GetAttribute("MaxWidth"),ref iFlags);
			API.Write(iSessionIndex,"WidthPerLayer",myReader.GetAttribute("WidthPerLayer"),ref iFlags);
			API.Write(iSessionIndex,"AvoidJellyfish",myReader.GetAttribute("AvoidJellyfish"),ref iFlags);
			API.Write(iSessionIndex,"MergeLinesAtSameLevel",myReader.GetAttribute("MergeLinesAtSameLevel"),ref iFlags);
			API.Move(iSessionIndex,"..",ref iFlags);
			bungeeLineWritten = true;
		}
		// NOTE, there are currently no nested elements in this node.
		return;
	}

	// INCOMPLETE - separate stop and start connector?
	// Method processes attributes & nested tags inside <Connector> tag
	void getConnector(XmlReader myReader,string elementName, int iSessionIndex) {
		
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
		// Move session cursor back up to parent node
		// COMMENTED OUT AS THIS HAS YET TO BE IMPLEMENTED
		/*if(secondPass) {
			API.Move(iSessionIndex,"..", ref iFlags);
		}*/
		return;
	}

	// DEBUG INFORMATION

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
		return;
	}
	
	private void tagsToLog(string name)
	{
		#if (PIXE_DEBUG_LOG)
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
		#endif
		return;
	}
}
