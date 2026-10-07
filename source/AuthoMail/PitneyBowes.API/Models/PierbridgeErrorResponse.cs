using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[XmlRoot("PierbridgeErrorResponse")]
public class PierbridgeErrorResponse
{
    public ResponseStatus Status { get; set; }
}

public class ResponseStatus
{
    public int Code { get; set; }
    public string Description { get; set; }

    public ErrorResponse Errors { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; }
}
