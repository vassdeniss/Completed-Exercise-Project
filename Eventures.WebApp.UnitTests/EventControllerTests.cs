using System;
using System.Linq;
using System.Collections.Generic;

using Eventures.Data;
using Eventures.WebApp.Models;
using Eventures.WebApp.Controllers;
using Eventures.Tests.Common;

using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Eventures.WebApp.UnitTests
{
    public class EventControllerTests : UnitTestsBase
    {
        private EventsController controller;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Instantiate the controller class with the testing database
            this.controller = new EventsController(this.dbContext);

            // Set UserMaria as current logged user
            TestingUtils.AssignCurrentUserForController(this.controller, this.testDb.UserMaria);
        }

        [Test]
        public void Test_All()
        {
            // Arrange

            // Act: invoke the controller method
            var result = this.controller.All();

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert events count is correct
            var resultModel = viewResult.Model as List<EventViewModel>;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(this.dbContext.Events.Count(), resultModel.Count);
        }

        [Test]
        public void Test_Create()
        {
            // Arrange

            // Act: invoke the controller method
            var result = this.controller.Create();

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert an event model is returned
            var resultModel = viewResult.Model as EventBindingModel;
            Assert.IsNotNull(resultModel);
        }

        [Test]
        public void Test_DeletePage_ValidId()
        {
            // Arrange: create a new event in the database for deleting
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

            // Act: invoke the controller method
            var result = this.controller.Delete(newEvent.Id);

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert the correct event is returned
            var resultModel = viewResult.Model as EventViewModel;
            Assert.IsNotNull(resultModel);
            Assert.That(resultModel.Id == newEvent.Id);
            Assert.That(resultModel.Name == newEvent.Name);
            Assert.That(resultModel.Place == newEvent.Place);
        }

        [Test]
        public void Test_DeletePage_InvalidId()
        {
            // Arrange
            var invalidId = -1;

            // Act: invoke the controller method with invalid id
            var result = this.controller.Delete(invalidId);

            // Assert a "Bad Request" result is returned
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
        }

        [Test]
        public void Test_Edit_ValidId()
        {
            // Arrange: get the "Dev Conference" event from the database for editing
            var devConfEvent = this.testDb.EventDevConf;

            // Act: invoke the controller method
            var result = this.controller.Edit(devConfEvent.Id);

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert fields are filled with correct data
            var resultModel = viewResult.Model as EventBindingModel;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(resultModel.Name, devConfEvent.Name);
            Assert.AreEqual(resultModel.Place, devConfEvent.Place);
            Assert.AreEqual(resultModel.Start, devConfEvent.Start);
            Assert.AreEqual(resultModel.End, devConfEvent.End);
            Assert.AreEqual(resultModel.PricePerTicket, devConfEvent.PricePerTicket);
            Assert.AreEqual(resultModel.TotalTickets, devConfEvent.TotalTickets);
        }

        [Test]
        public void Test_Edit_InvalidId()
        {
            // Arrange

            var invalidId = -1;
            // Act: invoke the controller method with invalid id
            var result = this.controller.Edit(invalidId);

            // Assert a "Bad Request" result is returned
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
        }
    }
}
