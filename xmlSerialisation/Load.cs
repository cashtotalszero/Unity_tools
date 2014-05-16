/*
 * Execution in Unity of http://tech.pro/tutorial/798/csharp-tutorial-xml-serialization
 * 
 * Put this script and the Movie class into the same game object/
 * */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;

public class Load : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Movie movie1 = new Movie();
		movie1.Title = "Starship Troopers";
		movie1.ReleaseDate = DateTime.Parse("11/7/1997");
		movie1.Rating = 6.9f;

		Movie movie2 = new Movie();
		movie2.Title = "Ace Ventura: When Nature Calls";
		movie2.ReleaseDate = DateTime.Parse("11/10/1995");
		movie2.Rating = 5.4f;

		// Create an xml file of the above movies
		List<Movie> movies = new List<Movie>() { movie1, movie2 };
		SerializeToXML(movies);

		// Read these back into a list of movies
		List<Movie> movies2 = DeserializeFromXML();
		// Print to demonstrate it worked...
		foreach(Movie movie in movies2){
			Debug.Log("Title: "+ movie.Title);
			Debug.Log("Rating: "+ movie.Rating);
		}
	}

	// Takes movie classes and generates an xml file of them
	static public void SerializeToXML(List<Movie> movies)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(List<Movie>));
		TextWriter textWriter = new StreamWriter("Assets/movie.xml");
		serializer.Serialize(textWriter, movies);
		textWriter.Close();
	}

	// Takes the file movie.xml from assets folder and stores returns a list of movie classes
	static public List<Movie> DeserializeFromXML()
	{
		XmlSerializer deserializer = new XmlSerializer(typeof(List<Movie>));
		TextReader textReader = new StreamReader("Assets/movie.xml");
		List<Movie> movies; 
		movies = (List<Movie>)deserializer.Deserialize(textReader);
		textReader.Close();
		
		return movies;
	}
}
