using System;
using System.Linq;
using System.Net;

using Eventures.Data;
using Eventures.Tests.Common;
using Eventures.WebAPI.Models;
using Eventures.WebAPI.Controllers;
using Eventures.WebAPI.Models.Event;

using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Eventures.WebAPI.UnitTests
{
    public class ApiEventUnitTests : ApiUnitTestsBase
    {
        EventsController eventsController;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.eventsController = new EventsController(this.dbContext);
        }

        [Test]
        public void Test_Put_ValidId()
        {
            // Arrange: set UserMaria as a currently logged in user
            TestingUtils.AssignCurrentUserForController(
                this.eventsController, this.testDb.UserMaria);

            // Create a new event in the database for editing
            var newEvent = new Event()
            {
                Name = "Beach Party" + DateTime.Now.Ticks,
                Place = "Ibiza",
                Start = DateTime.Now.AddMonths(3),
                End = DateTime.Now.AddMonths(3),
                TotalTickets = 20,
                PricePerTicket = 120.00m,
                OwnerId = this.testDb.UserMaria.Id
            };
            this.dbContext.Add(newEvent);
            this.dbContext.SaveChanges();

            // Create an event binding model with changed event name
            var changedEvent = new EventBindingModel()
            {
                Name = "House Party" + DateTime.Now.Ticks,
                Place = newEvent.Place,
                Start = newEvent.Start,
                End = newEvent.End,
                TotalTickets = newEvent.TotalTickets,
                PricePerTicket = newEvent.PricePerTicket
            };

            // Act: invoke the controller method and cast the result
            var result = this.eventsController.PutEvent(newEvent.Id, changedEvent) 
                as NoContentResult;
            Assert.IsNotNull(result);

            // Assert a "NoContent" result is returned
            Assert.AreEqual((int)HttpStatusCode.NoContent, result.StatusCode);

            // Assert the event in the database has a changed name
            var newEventFromDb = this.dbContext.Events.Find(newEvent.Id);
            Assert.AreEqual(newEventFromDb.Name, changedEvent.Name);
        }

        [Test]
        public void Test_Put_InvalidId()
        {
            // Arrange: create an event binding model 
            var changedEvent = new EventBindingModel();

            var invalidId = -1;

            // Act: invoke the controller method with invalid id and cast the result
            var result = this.eventsController.PutEvent(invalidId, changedEvent) 
                as NotFoundObjectResult;
            Assert.IsNotNull(result);

            // Assert a "NotFound" result with an error message is returned
            Assert.AreEqual((int)HttpStatusCode.NotFound, result.StatusCode);

            var resultValue = result.Value as ResponseMsg;
            Assert.AreEqual($"Event #{invalidId} not found.", resultValue.Message);
        }

        [Test]
        public void Test_Delete_ValidId()
        {
            // Arrange: set UserMaria as currently logged in user
            TestingUtils.AssignCurrentUserForController(this.eventsController, this.testDb.UserMaria);

            // Create a new event in the database for deleting
            var newEvent = new Event()
            {
                Name = "Beach Party" + DateTime.Now.Ticks,
                Place = "Ibiza",
                Start = DateTime.Now.AddMonths(3),
                End = DateTime.Now.AddMonths(3),
                TotalTickets = 20,
                PricePerTicket = 120.00m,
                OwnerId = this.testDb.UserMaria.Id
            };
            this.dbContext.Add(newEvent);
            this.dbContext.SaveChanges();

            // Get events count before the deletion
            int eventsCountBefore = this.dbContext.Events.Count();

            // Act: invoke the controller method and cast the result
            var result = this.eventsController.DeleteEvent(newEvent.Id) as OkObjectResult;
            Assert.IsNotNull(result);

            // Assert an "OK" result with the deleted event is returned
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);

            var resultValue = result.Value as EventListingModel;
            Assert.IsNotNull(resultValue);
            Assert.AreEqual(resultValue.Id, newEvent.Id);
            Assert.AreEqual(resultValue.Name, newEvent.Name);
            Assert.AreEqual(resultValue.Place, newEvent.Place);

            // Assert the event is deleted from the database
            int eventsCountAfter = this.dbContext.Events.Count();
            Assert.AreEqual(eventsCountBefore - 1, eventsCountAfter);
            Assert.IsNull(this.dbContext.Events.Find(newEvent.Id));
        }

        [Test]
        public void Test_Delete_InvalidId()
        {
            // Set UserMaria as currently logged in user
            TestingUtils.AssignCurrentUserForController(this.eventsController, this.testDb.UserMaria);

            int invalidId = -1;

            // Act: invoke the controller method with an invalid id and cast the result
            var result = this.eventsController.DeleteEvent(invalidId) as NotFoundObjectResult;
            Assert.IsNotNull(result);

            // Assert a "NotFound" result with an error message is returned
            Assert.AreEqual((int)HttpStatusCode.NotFound, result.StatusCode);

            var resultValue = result.Value as ResponseMsg;
            Assert.AreEqual($"Event #{invalidId} not found.", resultValue.Message);
        }
    }
}