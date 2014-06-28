using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System;

//[XmlElement("Movie")]
public class Movie {

	[XmlAttribute("Title")]
	public string Title
	{ get; set; }
	
	[XmlAttribute("Rating")]
	public float Rating
	{ get; set; }

	[XmlArray("Appearance")]
	public Appearance AppearanceList
	{ get; set; }
}

//[XmlElement("Appearance")]
public class Appearance {
	
	[XmlAttribute("Yo")]
	public string Yo
	{ get; set; }

	[XmlElement("Appearance")]
	public Appearance[] appearance
	{ get; set; }
}