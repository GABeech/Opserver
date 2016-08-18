using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Jil;

namespace StackExchange.Opserver.Data.PagerDuty
{
    public partial class PagerDutyAPI
    {
        public async Task<Incident> UpdateIncidentStatusAsync(string incidentId, PagerDutyPerson person, IncidentStatus newStatus)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));
            /*
            var data = new PagerDutyIncidentPut
            {
                Incidents = new List<Incident>
                {
                    new Incident {Id = incidentId, Status = newStatus}
                },
                RequesterId = person.Id
            };
            */
            var data = new
            {
                type = "incident_reference",
                status = newStatus
            };

            var headers = new Dictionary<string,string>()
            {
                {  "From", person.Email}
            };
            try
            {
               var result = await Instance.GetFromPagerDutyAsync($"incidents/{incidentId}",
               response => JSON.Deserialize<PagerDutyIncidentUpdateResp>(response.ToString(), JilOptions),
               httpMethod: "PUT",
               data: data,
               extraHeaders: headers).ConfigureAwait(false);
               await Incidents.PollAsync(true).ConfigureAwait(false);

               return result?.Response ?? new Incident();
            }
            catch (DeserializationException de)
            {
                Current.LogException(
                    de.AddLoggedData("Message", de.Message)
                    .AddLoggedData("Snippet After", de.SnippetAfterError)
                    );
                return null;
            }
           

        
        }

        public class PagerDutyIncidentPut
        {
            [DataMember(Name = "requester_id")]
            public string RequesterId { get; set; }
            [DataMember(Name = "incidents")]
            public List<Incident> Incidents { get; set; }
            public bool Refresh { get; set; }
        }

        public class PagerDutyIncidentUpdateResp
        {
            [DataMember(Name = "incident")]
            public Incident Response { get; set; }
        }
    }
}
