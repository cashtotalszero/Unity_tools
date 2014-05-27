using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.IO;

public class ValidXSD : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Set the validation settings.
		XmlReaderSettings settings = new XmlReaderSettings();
		settings.ValidationType = ValidationType.Schema;
		settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
		settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
		settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
		settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
		
		// Create the XmlReader object.
		XmlReader reader = XmlReader.Create("Assets/shiporder.xml", settings);
		
		// Parse the file. 
		while (reader.Read()) ;
	}
	// Display any warnings or errors.
	private static void ValidationCallBack(object sender, ValidationEventArgs args)
	{
		if (args.Severity == XmlSeverityType.Warning)
			Debug.Log("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
		else
			Debug.Log("\tValidation error: " + args.Message);
		
	}	
}
