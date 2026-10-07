using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class SigningRoomRequest
    {
        [Description("This is a mandatory field which is used to identify signing room and must be unique.")]
        public string SigningRoomName { get; set; }
        [Description("This is an optional field which used to provide description of SigningRoom.")]
        public string Description { get; set; }
        [Description("This is used to set the start date and time of signing room.")]
        public DateTime StartDate { get; set; }
        [Description("This is used to set the expiration date and time of signing room.")]
        public DateTime ExpirationDate { get; set; }
        public List<Participant> Participants { get; set; }
        [JsonProperty("Documents")]
        public List<DocumentInfo> Documents { get; set; }
    }

    public class Participant
    {
        [Description("This is mandatory field to set the first name of participant.")]
        public string FirstName { get; set; }

        [Description("This is mandatory field to set the last name of participant.")]
        public string LastName { get; set; }

        [Description("This is mandatory field to set the email address of participant.")]
        public string InvitationEmail { get; set; }
    }

    public class DocumentInfo
    {
        [Description("This is optional field to set the document name to upload.")]
        public string DocumentName { get; set; }

        [Description("This is optional field to set the file name to upload.")]
        public string FileName { get; set; }

        [Description("This is mandatory field to set the document data in base64 string.")]
        public string DocumentData { get; set; }

        [Description("This is optional field.")]
        public string UDF1 { get; set; }

        [Description("This is optional field.")]
        public string UDF2 { get; set; }

        [Description("This is optional field.")]
        public string UDF3 { get; set; }

        [Description("This is optional field.")]
        public string DocumentType { get; set; }

        [Description("Used to Gets or sets the list of signers if clients wants e-Sign functionality.")]
        public List<DocumentSigners> DocumentSigners { get; set; }
    }

    public class DocumentSigners
    {
        [Description("This is mandatory field to set the first name of participant.")]
        public string FirstName { get; set; }
        [Description("This is mandatory field to set the last name of participant.")]
        public string LastName { get; set; }
        [Description("This is mandatory field to set the email address of participant.")]
        public string InvitationEmail { get; set; }
        [Description("Gets or sets the list of document annotations associated with the new signer.")]
        public List<DocumentAnnotations> DocumentAnnotations { get; set; }
    }

    public class DocumentAnnotations
    {
        [Description("Gets or sets the annotation's annotation type. In our case 'Signature'.")]
        public string AnnotationType { get; set; }
        [Description("Gets or sets a value indicating whether the document annotation is optional.")]
        public bool Optional { get; set; }

        [Description("This property is used to set width for signing window for e-Sign purpose.")]
        public int Width { get; set; }

        [Description("This property is used to set height for signing window for e-Sign purpose.")]
        public int Height { get; set; }

        [Description("This property is used to set X OffSet position for signing window for e-Sign purpose.")]
        public int XOffset { get; set; }

        [Description("This property is used to set Y OffSet position for signing window for e-Sign purpose.")]
        public int YOffset { get; set; }

        [Description("This property is used to set e-Sign window on particular page.")]
        public int PageNumber { get; set; }
    }
}
