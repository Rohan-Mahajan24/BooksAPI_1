using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace books.Controllers
{
   // Make sure to use the correct namespace for your Category model

    namespace WebApplication2.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class CategoryController : ControllerBase
        {
            private static readonly HttpClient _httpClient = new HttpClient();

            [HttpGet]
            public List<Category> Get()
            {
                return category;
            }

            [HttpPost]
            public IActionResult Post([FromBody] Category cat)
            {
                category.Add(cat);
                return Ok(category);
            }

            [HttpGet("fetchExternalData")]
            public async Task<IActionResult> FetchExternalData()
            {
                try
                {
                    // Replace "https://example.com/api/external-data" with the actual URL of the external API you want to fetch data from.
                    string externalApiUrl = "https://www.googleapis.com/books/v1/volumes?q=kaplan%20test%20prep";
                    HttpResponseMessage response = await _httpClient.GetAsync(externalApiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var externalData = await response.Content.ReadAsAsync<List<Category>>(); // Assuming the response is of type List<Category>
                        return Ok(externalData);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, "External API request failed");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"An error occurred: {ex.Message}");
                }
            }

            private static List<Category> category = new List<Category>()
        {
            new Category() { ID = 1, Title = "Electronic" },
            new Category() { ID = 2, Title = "Fashion" }
        };
        }
    }

}
