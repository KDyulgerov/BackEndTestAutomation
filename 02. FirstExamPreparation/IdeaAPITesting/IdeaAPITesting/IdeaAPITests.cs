using IdeaAPITesting.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdeaAPITesting
{
    [TestFixture]
    public class IdeaAPITests
    {
        private RestClient client;
        private const string BASEURL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private const string EMAIL = "userapitestingpostman@example.com";
        private const string PASSWORD = "123456";
        private static string lastCreatedIdeaId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(EMAIL, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("api/User/Authentication");
            request.AddJsonBody(new
            {
                email,
                password
            });

            var response = authClient.Execute(request, Method.Post);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token is null or empty.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
            }
        }

        // Positive Path
        [Test, Order(1)]
        public void CreateNewIdea_WithCorrectData_ShouldSucceed()
        {
            var requestData = new IdeaDTO
            {
                Title = "Some Title for Request",
                Description = "Some Description for Request" 
            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllIdeas_ShouldReturnAllIdeasInAnArray()
        {
            var request = new RestRequest("/api/Idea/All");

            var response = client.Execute(request, Method.Get);

            var responseData = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData, Is.Not.Null);
            Assert.That(responseData, Is.Not.Empty);
            Assert.That(responseData.Count, Is.GreaterThan(0));

            //Extracting the "id" of the last created Idea
            lastCreatedIdeaId = responseData.LastOrDefault()?.IdeaId;
        }

        [Test, Order(3)]
        public void EditIdea_WithCorrectData_ShouldEditTheIdeaSucessffully()
        {
            var editRequest = new IdeaDTO()
            {
                Title = "Some Title for Request EDITED",
                Description = "Some Description for Request EDITED"
            };

            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(editRequest);

            var response = client.Execute(request, Method.Put);

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Edited successfully"));
            // Assert.That(responseData.Idea.Title, Is.EqualTo(editRequest.Title));
        }

        [Test, Order(4)]
        public void DeleteIdea_WithCorrectData_ShouldDeleteTheIdeaSucessffully()
        {
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);

            var response = client.Execute(request, Method.Delete);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.TypeOf<string>());
            Assert.That(response.Content, Does.Contain("The idea is deleted!"));
        }

        // Negative Path
        [Test, Order(5)]
        public void CreateNewIdea_WithoutRequredData_ShouldFail()
        {
            var invalidData = new IdeaDTO
            {
                //Missing required title
                Description = "Some Description for Request"
            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(invalidData);

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditIdea_WithNonExistingId_ShouldFail()
        {
            var editRequest = new IdeaDTO()
            {
                Title = "Some Title for Request EDITED",
                Description = "Some Description for Request EDITED"
            };

            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", "#123123123");
            request.AddJsonBody(editRequest);

            var response = client.Execute(request, Method.Put);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }

        [Test, Order(7)]
        public void DeleteIdea_WithNonExistingId_ShouldFail()
        {
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", "#456456456");

            var response = client.Execute(request, Method.Delete);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }
    }
}