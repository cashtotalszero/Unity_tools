/*
 * This script demonstrates how Open Mission Control jellyfish variables can be read in 
 * from an xml file. 
 * 
 * To run the script:
 * 1) Open a new project in Unity and put the script into the Assets folder.
 * 2) Create an empty game object and attach the script using Add Component. 
 * 3) Ensure that the xml file (jellyfish.xml) is also placed in the Assets folder.
 */
using UnityEngine;
using System.Collections;
using System.Xml;						// CRUCIAL - holds all Xml handling funcitons 
using System;							// Needed for Convert function

public class XMLTutorial : MonoBehaviour {

	private int x_pos;					// x position of jellyfish rectangle on GUI screen
	private int y_pos;					// y position of jellyfish on GUI screen
	private int width;					// Width of the jellyfish
	private int height;					// Height of the jellyfish
	private string text;				// Text to be displayed on jellyfish
	private XmlReader reader;			// Holds the XML reader object

	// Iniailises all variables at start of program
	void Start() {

		// Creates an instance of the reader class
		reader = XmlReader.Create ("Assets/jellyfish.xml");

		while (reader.Read()) 
		{
			// Find all references to jellyfish elements in the XML file
			if(reader.NodeType == XmlNodeType.Element && reader.Name == "jellyfish")
			{
				// Get the name and ID details to display 
				text = reader.GetAttribute(0) + "\n" + reader.GetAttribute(1);

				// Get all the jellyfish details from file
				while(reader.NodeType != XmlNodeType.EndElement)
				{
					reader.Read();
					if(reader.Name == "x_pos")
					{
						x_pos = getVariable(reader);
					}
					if(reader.Name == "y_pos")
					{
						y_pos = getVariable (reader);
					}
					if(reader.Name == "width")
					{
						width = getVariable (reader);
					}
					if(reader.Name == "height")
					{
						height = getVariable (reader);
					}
				}
			}
		}
	}
	
	// This creates the rectangle on the GUI display
	void OnGUI () 
	{
		GUI.Box(new Rect(x_pos,y_pos,width,height), text);
	}

	// Function reads in data from XML file and returns it as an int
	int getVariable(XmlReader reader) 
	{
		int var = -1;						// Variable to return

		while(reader.NodeType != XmlNodeType.EndElement)
		{
			reader.Read();
			if(reader.NodeType == XmlNodeType.Text)
			{
				// Convert input from string to int
				var = Convert.ToInt32(reader.Value); 	
			}
		}
		reader.Read ();
		return var;
	}
}
