using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System;

public class Movie {

	[XmlElement("Title")]
	public string Title
	{ get; set; }
	
	[XmlElement("Rating")]
	public float Rating
	{ get; set; }
	
	[XmlElement("ReleaseDate")]
	public DateTime ReleaseDate
	{ get; set; }
}
