using System;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PitneyBowes.API; // Make sure to match your actual namespace

class Program
{
    static async Task Main(string[] args)
    {
        var response = await CallShippingAPI();

        if (response != null)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented));
        }
        else
        {
            Console.WriteLine("Failed to retrieve response from API.");
        }
    }

    public static async Task<dynamic> CallShippingAPI()
    {
        var url = "https://corelogicxb002.sendproenterprise.com/Logistics%20Server/xmlservice.aspx?apikey=UEB3YKN97IE1MSCP1EDXJ1I4L";

        string xmlPayload = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<PierbridgeShipRequest>
    <Carrier>106</Carrier>
    <ServiceType>981</ServiceType>
    <Receiver>
        <CompanyName>The White House</CompanyName>
        <Street>1600 Pennsylvania Avenue NW</Street>
        <City>Washington</City>
        <Region>DC</Region>
        <PostalCode>20500</PostalCode>
        <Country>US</Country>
    </Receiver>
    <Packages>
        <Package>
            <ShipperReference>MyRef</ShipperReference>
            <ReceiverName>Mr President</ReceiverName>
            <ReceiverPhone>202-456-1414</ReceiverPhone>
            <PackageType>656</PackageType>
            <Weight>10.5</Weight>
            <Length>7</Length>
            <Width>8</Width>
            <Height>9</Height>
            <ContentDescription>A gift</ContentDescription>
        </Package>
    </Packages>
    <UserName>blackbox</UserName>
</PierbridgeShipbRequest>";

        using (var client = new HttpClient())
        {
            var content = new StringContent(xmlPayload, Encoding.UTF8, "application/xml");

            try
            {
                var response = await client.PostAsync(url, content);
                var xmlResponse = await response.Content.ReadAsStringAsync();
                if (xmlResponse.Contains("PierbridgeErrorResponse"))
                {
                    return DeserializeXml<PierbridgeErrorResponse>(xmlResponse);
                }

                var res = DeserializeXml<PierbridgeShipResponse>(xmlResponse);
                if (res.Status.Code == 0)
                    res.Error = res.Status.Description;
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }
    }

    public static T DeserializeXml<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using (var reader = new StringReader(xml))
        {
            return (T)serializer.Deserialize(reader);
        }
    }
}
