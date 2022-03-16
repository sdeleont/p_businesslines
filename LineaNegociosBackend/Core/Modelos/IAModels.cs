using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Modelos
{
    public class IAModels
    {

    }
    public class PeticionIA {
        public string peticion { get; set; }
    }
    public class ResponseIA { 
        public string response { get; set; }
        public string status { get; set; }
        public string mensaje { get; set; }
        public List<ParRetornoIA> pares { get; set; }
    }
    public class RequestGoogleIA {
        public DocumentG rawDocument { get; set; }
        public string name { get; set; }
    }
    public class DocumentG {
        public string content { get; set; }
        public string mimeType { get; set; }
    }
    public class RequestImageIA {
        public string idLineaNegocio { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile Archivo { get; set; }
    }
    public class ResponseDocumentApi {
        public DocumentR document { get; set; }
    }
    public class DocumentR {
        public string mimeType { get; set; }
        public string text { get; set; }
        public List<Page> pages { get; set; }
    }
    public class Page {
        public int pageNumber { get; set; }
        public Layout layout { get; set; }
        public List<Block> blocks { get; set; }
        public List<Paragraph> paragraphs { get; set; }
        public List<Line> lines { get; set; }
        public List<Token> tokens { get; set; }
        public List<FormField> formFields { get; set; }
    }
    public class Block {
        public Layout layout { get; set; }
    }
    public class Paragraph
    {
        public Layout layout { get; set; }
    }
    public class Line
    {
        public Layout layout { get; set; }
    }
    public class Token
    {
        public Layout layout { get; set; }
    }
    public class FormField
    {
        public FieldName fieldName { get; set; }
        public FieldValue fieldValue { get; set; }
    }
    public class FieldName
    {
        public TextAnchor textAnchor { get; set; }
        public double confidence { get; set; }
    }
    public class FieldValue
    {
        public TextAnchor textAnchor { get; set; }
        public double confidence { get; set; }
    }
    public class Layout {
        public TextAnchor textAnchor { get; set; }
        public double confidence { get; set; }
    }
    public class TextAnchor
    {
        public List<TextSegments> textSegments { get; set; }
    }
    public class TextSegments
    {
        public string startIndex { get; set; }
        public string endIndex { get; set; }
    }

    public class ParCampoIA { // esta clase es para obtener y relacionar los campos obtenidos del form hacia los que tiene base de datos 
        public string formName { get; set; }
        public string formValue { get; set; }
        public string campoBD { get; set; }
    }
    public class ParRetornoIA {
        public string idCampo { get; set; }
        public string campo { get; set; }
        public string valor { get; set; }
    }
}
