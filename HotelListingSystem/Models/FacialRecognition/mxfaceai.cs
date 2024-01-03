using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models.FacialRecognition
{
    public class mxfaceai
    {
    }


    public class MXFaceAiResponse
    {
        public Matchedface[] MatchedFaces { get; set; }
    }

    public class Matchedface
    {
        public int matchResult { get; set; }
        public Image1_Face image1_face { get; set; }
        public Image2_Face image2_face { get; set; }
    }

    public class Image1_Face
    {
        public float quality { get; set; }
        public Point[] Points { get; set; }
        public Facerectangle faceRectangle { get; set; }
    }

    public class Facerectangle
    {
        public int x { get; set; }
        public int y { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Image2_Face
    {
        public float quality { get; set; }
        public Point1[] Points { get; set; }
        public Facerectangle1 faceRectangle { get; set; }
    }

    public class Facerectangle1
    {
        public int x { get; set; }
        public int y { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

    public class Point1
    {
        public float X { get; set; }
        public float Y { get; set; }
    }



    public class MXFaceFacialRequest
    {
        public String encoded_image1 { get; set; }
        public String encoded_image2 { get; set; }
    }

}