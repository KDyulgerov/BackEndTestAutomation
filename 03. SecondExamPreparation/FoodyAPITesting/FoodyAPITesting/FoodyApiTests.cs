using FoodyAPITesting.DTO_Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace FoodyAPITesting
{
    [TestFixture]
    public class FoodyApiTests
    {
        private RestClient client;
        private const string BASEURL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";
        private const string USERNAME = "username";
        private const string PASSWORD = "password";
        private static string foodId;

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
            var tempClient = new RestClient(BASEURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new
            {
                username,
                password
            });

            var response = tempClient.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);

                var token = content.GetProperty("accessToken").GetString();

                if(string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("The JWT Token is null or empty.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Authentication failded: {response.StatusCode} with data {response.Content}");
            }
        }

        // Positive Path
        [Order(1)]
        [Test]
        public void CreateFood_WithValidInput_ShouldSucceed()
        {
            var foodRequestBody = new FoodDto
            {
                Name = "Food Title Example",
                Description = "Food Description Example"
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodRequestBody);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(responseData.FoodId, Is.Not.Null);
            Assert.That(responseData.FoodId, Is.Not.Empty);

            foodId = responseData.FoodId;
        }

        [Order(2)]
        [Test]
        public void EditFoodTitle_WithValidInput_ShouldSucceed()
        {
            var foodRequestBody = new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Food Title EDITED by Request"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);

            request.AddJsonBody(foodRequestBody);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var editResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(editResponse.Message, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void GetAllFoods_WithValidInput_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<List<ApiResponseDto>>(response.Content);

            Assert.IsNotEmpty(responseData);
            Assert.IsNotNull(responseData);
            Assert.That(responseData.Count, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void DeleteFood_WithValidInput_ShouldSucceed()
        {
            var request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(responseData.Message, Is.EqualTo("Deleted successfully!"));
        }

        // Negative Path
        [Order(5)]
        [Test]
        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var invalidFoodRequestBody = new FoodDto
            {
                // missing required Name
                Description = string.Empty
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(invalidFoodRequestBody);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditFood_WithNonExistingId_ShouldFail()
        {
            var foodRequestBody = new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Food Title EDITED by Request"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/XXX123XXX", Method.Patch);

            request.AddJsonBody(foodRequestBody);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(responseData.Message, Is.EqualTo("No food revues..."));
        }

        [Order(7)]
        [Test]
        public void DeleteFood_WithNonExistingId_ShouldFail()
        {
            var request = new RestRequest($"/api/Food/Delete/XXX123XXX", Method.Delete);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(responseData.Message, Is.EqualTo("Unable to delete this food revue!"));
        }


        [OneTimeTearDown]
        public void Teardown()
        {
            client?.Dispose();
        }
    }
}