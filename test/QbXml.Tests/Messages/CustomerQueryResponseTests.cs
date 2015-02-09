﻿using QbSync.QuickbooksDesktopSync.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using QbSync.QbXml.Struct;
using QbSync.QbXml;
using QbSync.QbXml.Type;
using QbSync.QbXml.Filters;
using QbSync.QbXml.Messages;
using QbSync.QuickbooksDesktopSync.Tests.Helpers;
using QbSync.QbXml.Objects;

namespace QbSync.QuickbooksDesktopSync.Tests.QbXml
{
    [TestFixture]
    class CustomerQueryResponseTests
    {
        [Test]
        public void BasicCustomerResponseTest()
        {
            var customerRet = "<CustomerRet><ListID>80000001-1422671082</ListID><IsActive>true</IsActive><TimeCreated>2015-01-30T18:24:42-08:00</TimeCreated><TimeModified>2015-01-30T18:24:42-08:00</TimeModified><EditSequence>1422671082</EditSequence><Name>Jean-S&#233;bastien Goupil</Name><FullName>Jean-S&#233;bastien Goupil</FullName></CustomerRet>";

            var customerResponse = new CustomerQueryResponse();
            var response = customerResponse.ParseResponse(QuickbooksTestHelper.CreateQbXmlWithEnvelope(customerRet));
            var customers = response.Object;
            var customer = customers[0];

            Assert.AreEqual(1, customers.Length);
            QBAssert.AreEqual("80000001-1422671082", customer.ListID);
            QBAssert.AreEqual("Jean-Sébastien Goupil", customer.Name);
        }

        [Test]
        public void BasicCustomerResponseTest_WithIEnumerable()
        {
            var customerRet = "<CustomerRet><AdditionalContactRef><ContactName>Name1</ContactName><ContactValue>Value1</ContactValue></AdditionalContactRef><AdditionalContactRef><ContactName>Name2</ContactName><ContactValue>Value2</ContactValue></AdditionalContactRef></CustomerRet>";

            var customerResponse = new CustomerQueryResponse();
            var response = customerResponse.ParseResponse(QuickbooksTestHelper.CreateQbXmlWithEnvelope(customerRet));
            var customers = response.Object;
            var customer = customers[0];

            Assert.AreEqual(1, customers.Length);
            Assert.AreEqual(2, customer.AdditionalContactRef.Count());
            QBAssert.AreEqual("Name1", customer.AdditionalContactRef.First().ContactName);
            QBAssert.AreEqual("Value1", customer.AdditionalContactRef.First().ContactValue);
            QBAssert.AreEqual("Name2", customer.AdditionalContactRef.Last().ContactName);
            QBAssert.AreEqual("Value2", customer.AdditionalContactRef.Last().ContactValue);
        }

        [Test]
        public void BasicCustomerResponseTest_WithEnum()
        {
            var customerRet = "<CustomerRet><JobStatus>InProgress</JobStatus></CustomerRet>";

            var customerResponse = new CustomerQueryResponse();
            var response = customerResponse.ParseResponse(QuickbooksTestHelper.CreateQbXmlWithEnvelope(customerRet));
            var customers = response.Object;
            var customer = customers[0];

            Assert.AreEqual(1, customers.Length);
            Assert.AreEqual(JobStatus.InProgress, customer.JobStatus);
        }
    }
}