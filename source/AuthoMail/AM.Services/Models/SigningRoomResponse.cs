using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class SigningRoomResponse
    {
        public SigningRoom SigningRoom { get; set; }
        [Description("Returns a list of errors that may have occurred when creating the SigningRoom.")]
        public List<string> Warnings { get; set; }
        public string Error { get; set; }
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(this.Error) ? true : false;
            }
        }
        public bool HasWarnings
        {
            get
            {
                return Warnings == null ? false : Warnings.Count() > 0 ? true : false;
            }
        }
    }

    public class SigningRoom
    {
        [Description("Returns the integer SigningRoom identifier.")]
        public int SigningRoomId { get; set; }
        [Description("Returns the GUID of SigningRoom.")]
        public Guid SigningRoomGuid { get; set; }
        [Description("Returns a string representing the date and time that the SigningRoom starts.")]
        public DateTime StartDate { get; set; }
        [Description("Returns a value indicating whether the SigningRoom has expired.")]
        public bool Expired { get; set; }
        [Description("Returns a string representing the date and time that the SigningRoom expires.")]
        public DateTime ExpirationDate { get; set; }
        [Description("Returns the SigningRoom status.")]
        public string Status { get; set; }
        [JsonProperty("Documents")]
        public List<DocumentResponse> Documents { get; set; }
        [Description("Returns the name of the user that updated the model.")]
        public string UpdatedBy { get; set; }
        [Description("Returns the date and time that the model was updated.")]
        public DateTime UpdatedDate { get; set; }
    }

    public class DocumentResponse
    {
        public int DocumentId { get; set; }
        public Guid DocumentGuid { get; set; }
    }
}
