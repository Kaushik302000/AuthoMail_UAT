using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class SigningRoomActivityLogResponse
    {
        [Description("This is a unique integer value which is used to send Invitation to participants.")]
        public int SigningRoomId { get; set; }
        [Description("This is used to track the activity of signing room.")]
        [JsonProperty("SigningRoomActivityType")]
        public string EDeliveryStatus { get; set; }
        [Description("This is a created when participant login into the signing room.")]
        public int? ParticipantId { get; set; }
        [Description("This is a first name of participant associated to the SigningRoom activity log.")]
        public string ParticipantFirstName { get; set; }
        [Description("This is a last name of participant associated to the SigningRoom activity log.")]
        public string ParticipantLastName { get; set; }
        [Description("This is a email address of participant to send the invitation.")]
        public string ParticipantEmail { get; set; }
        [Description("This is a IP address of machine from which participant login.")]
        public string IPAddress { get; set; }
        [Description("Returns the name of the user that updated the model.")]
        public string UpdatedBy { get; set; }
        [Description("Returns the date and time that the model was updated.")]
        public DateTime UpdatedDate { get; set; }
        public string Error { get; set; }
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(this.Error) ? true : false;
            }
        }
    }
}
