using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace siscomex.Controllers
{
    [Route("[controller]")]
    [ApiController]    
    public class CeController : ControllerBase
    {
        public IConfiguration Configuration { get; }

        public CeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        [HttpGet]        
        public IActionResult Get(string  nr)
        {
            if (string.IsNullOrWhiteSpace(nr))
            {
                return Ok(new List<Models.CeMercante>());
            }

            string[] ceNbrList = nr.Split(",");

            ceNbrList = ceNbrList.Where(x => string.IsNullOrWhiteSpace(x) == false).Distinct().ToArray();

            if (ceNbrList.Length > 50)
            {
                return StatusCode(422, new { timestamp = DateTime.Now, message = $"O tamanho máximo da lista contendo os Conhecimentos de Embarque é de 50. Tamanho da Lista: {ceNbrList.Length}" });
            }

            List<Models.CeMercante> result = new List<Models.CeMercante>();

            foreach (var item in ceNbrList)
            {
                result.Add(new Models.CeMercante() { Nbr = item });             
            }

            return Ok(result);
        }


        [HttpGet]
        [Route("dataUltimaAtualizacao")]
        public async Task<IActionResult> dataUltimaAtualizacao(string nr)
        {
            if (string.IsNullOrWhiteSpace(nr))
            {
                return Ok(new List<Models.CeUltimaAtualizacao>());
            }

            string[] ceNbrList = nr.Split(",");

            ceNbrList = ceNbrList.Where(x => string.IsNullOrWhiteSpace(x) == false).Distinct().ToArray();

            if (ceNbrList.Length > 50)
            {
                return StatusCode(422, new { timestamp = DateTime.Now, message = $"O tamanho máximo da lista contendo os Conhecimentos de Embarque é de 50. Tamanho da Lista: {ceNbrList.Length}" });
            }

            nr = string.Join(',', ceNbrList);

            List<Models.CeUltimaAtualizacao> result = new List<Models.CeUltimaAtualizacao>();

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Configuration["Siscomex:UrlBase"]);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await httpClient.GetAsync($"siscarga-api/ce/data-ultima-atualizacao?nr={nr}");
                
                string apiResult = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.CeUltimaAtualizacao>>(apiResult);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, apiResult);
                }

            }

            return Ok(result);
        }
    }
}
