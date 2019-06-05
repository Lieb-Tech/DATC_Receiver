using DATC_Receiver.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DATC_Receiver.Tests
{
    [TestFixture]
    class CosmosDBHelperTest
    {
        [Test]
        public void NoConnectionSettingsFile()
        {
            if (File.Exists("cosmosDbKey.json"))
                File.Delete("cosmosDbKey.json"); 

            Assert.Catch(() => new CosmosDB());
        }

        [Test]
        public void TestOpenConnection()
        {
            var cdb = new CosmosDB();
            Assert.DoesNotThrow(() => cdb.OpenConnection());
        }

        [Test]
        public void FailSavingNull()
        {
            var cdb = new CosmosDB();
            Assert.DoesNotThrow(() => cdb.OpenConnection());
            Assert.Catch(() => cdb.UpsertDocument(null, "flight").Wait());
        }

        [Test]
        public void FailBadCollection()
        {
            var cdb = new CosmosDB();
            Assert.DoesNotThrow(() => cdb.OpenConnection());
            Assert.Catch(() => cdb.UpsertDocument(new object(), "badCollection").Wait());
        }
    }
}
