﻿using FluentAssertions;
using IdsLib.IdsSchema;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace idsTool.tests
{
    public class MinMaxTests
    {
        [Fact]
        public void RequiredConfigurationValid()
        {
            var t = GetMM(""" minOccurs="1" maxOccurs="unbounded" """);
            t.Audit(out var _).Should().Be(IdsLib.Audit.Status.Ok);
        }

        [Fact]
        public void OptionalConfigurationValid()
        {
            var r = GetReader(""" minOccurs="0" maxOccurs="unbounded" """);
            var t = new MinMaxOccur(r);
            t.Audit(out var _).Should().Be(IdsLib.Audit.Status.Ok);
        }

        [Fact]
        public void ProhibitedConfigurationValid()
        {
            var r = GetReader(""" minOccurs="0" maxOccurs="0" """);
            var t = new MinMaxOccur(r);
            t.Audit(out var _).Should().Be(IdsLib.Audit.Status.Ok);
        }

        [Theory]
        [InlineData(""" minOccurs="1" maxOccurs="1" """)]
        [InlineData(""" minOccurs="2" maxOccurs="4" """)]
        [InlineData(""" minOccurs="2" maxOccurs="unbounded" """)]
        public void InvalidConfigurations(string attributeString)
        {
            var t = GetMM(attributeString);
            var audit = t.Audit(out var _);
            audit.Should().Be(IdsLib.Audit.Status.IdsContentError);
        }

        private static MinMaxOccur GetMM(string attributeString)
        {
            var r = GetReader(attributeString);
            var t = new MinMaxOccur(r);
            return t;
        }

        private static XmlReader GetReader(string attributeString)
        {
            var stringNode = $"<node {attributeString} />";
            using TextReader sr = new StringReader(stringNode);
            var nd = XmlReader.Create(sr);
            nd.Read();
            return nd;
        }
    }
}
