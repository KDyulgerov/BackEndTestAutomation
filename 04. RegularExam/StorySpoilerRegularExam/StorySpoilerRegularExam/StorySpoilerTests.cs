using RestSharp;
using RestSharp.Authenticators;
using StorySpoilerRegularExam.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoilerRegularExam
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private const string BASEURL = "https://d5wfqm7y6yb3q.cloudfront.net";
        private const string USERNAME = "username";
        private const string PASSWORD = "password";
        private static string storyId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(USERNAME, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("api/User/Authentication");
            request.AddJsonBody(new
            {
                username,
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
        [Order(1)]
        [Test]
        public void CreateNewStory_WithCorrectData_ShouldSucceed()
        {
            var requestData = new StoryDTO()
            {
                Title = "Some Title for the Request",
                Description = "Some Description for the Request"
            };

            var request = new RestRequest("/api/Story/Create");
            request.AddJsonBody(requestData);

            var response = this.client.Execute(request, Method.Post);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
            Assert.That(responseData.StoryId, Is.Not.Null);
            Assert.That(responseData.StoryId, Is.Not.Empty);

            storyId = responseData.StoryId;
        }

        [Order(2)]
        [Test]
        public void EditStoryTitle_WithValidInput_ShouldSucceed()
        {
            var requestData = new StoryDTO()
            {
                Title = "Some Title for the Request EDITED",
                Description = "Some Description for the Request EDITED"
            };

            var request = new RestRequest($"/api/Story/Edit/{storyId}", Method.Put);
            request.AddJsonBody(requestData);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void DeleteStory_WithValidInput_ShouldSucceed()
        {
            var request = new RestRequest($"/api/Story/Delete/{storyId}", Method.Delete);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Deleted successfully!"));
        }

        // Negative Path
        [Order(4)]
        [Test]
        public void CreateNewStory_WithoutRequredData_ShouldFail()
        {
            var invalidData = new StoryDTO
            {
                //Missing required title
                Description = string.Empty
            };

            var request = new RestRequest("/api/Story/Create");
            request.AddJsonBody(invalidData);

            var response = client.Execute(request, Method.Post);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(5)]
        [Test]
        public void EditStoryTitle_WithNonExistingId_ShouldFail()
        {
            var requestData = new StoryDTO()
            {
                Title = "Some Title for the Invalid Request EDITED",
                Description = "Some Description for the Invalid Request EDITED"
            };

            var request = new RestRequest($"/api/Story/Edit/XXX123XXX", Method.Put);
            request.AddJsonBody(requestData);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("No spoilers..."));
        }

        [Order(6)]
        [Test]
        public void DeleteStory_WithNonExistingId_ShouldFail()
        {
            var request = new RestRequest($"/api/Story/Delete/XXX123XXX", Method.Delete);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            client?.Dispose();
        }
    }
}