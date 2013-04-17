// <copyright file="AtomPosterFixture.cs" company="RobMensching.com LLC">
//    Copyright (c) RobMensching.com LLC.  All rights reserved.
// </copyright>

namespace PosterApi.test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Xunit;

    public class AtomPosterFixture
    {
        [Fact]
        public void CanCreateEntry()
        {
            DateTime now = DateTime.Now;
            DateTime utc = DateTime.UtcNow;

            XElement entryXml = new AtomPoster(new Uri("http://example.com/")).CreateEntryXml(
                null, null, "Title of document", now, "<p>Example document.</p>", null);

            Assert.NotNull(entryXml);
        }

        [Fact]
        public void CanCreateEntryWithTags()
        {
            DateTime now = new DateTime(2013, 4, 16, 12, 10, 0);
            XElement entryXml = new AtomPoster(new Uri("http://example.com/")).CreateEntryXml(
                null, null, "Title of document", now, "<p>Example document.</p>", new[] { "foo", "bar" });

            Assert.NotNull(entryXml);
        }
    }
}
