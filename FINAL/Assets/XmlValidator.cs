using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;

public class XmlValidator : MonoBehaviour {

	private bool valid = true;

	/*
	 * Method checks XML file against it's corresponding XSD file.
	 * All attributes and nested tags are verified here.
	 */
	//public bool validateXML(string psmlFile) {
		public bool validateXML(string psmlFile) {


		if(XmlFormValidator(psmlFile))
		{
			valid = true;
			/*
			// Set the validation settings.
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
			settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
			settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
			settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
		
			// Create the XmlReader object.
			XmlReader reader = XmlReader.Create(psmlFile, settings);
			
			// Parse the file. 
			while (reader.Read());*/
		}
		return valid;
	}

	// Display any warnings or errors.
	private void ValidationCallBack(object sender, ValidationEventArgs args)
	{
		valid = false;					// Set valid flag to false.

		if (args.Severity == XmlSeverityType.Warning) {
			Debug.Log ("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
		} 
		else {
			Debug.Log ("\tValidation error: " + args.Message);
		}
	}	

	// Method ensures provided XML file is properly formed
//	private bool XmlFormValidator(string psmlFile)
		private bool XmlFormValidator(string psmlFile)

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

}
