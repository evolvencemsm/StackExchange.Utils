﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace StackExchange.Utils.Tests
{
    public class PrefixedConfigurationProviderTests
    {
        [Fact]
        public void InvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ConfigurationBuilder().WithPrefix(null, c => { })
            );
            
            Assert.Throws<ArgumentNullException>(
                () => new ConfigurationBuilder().WithPrefix(string.Empty, c => { })
            );
            
            Assert.Throws<ArgumentNullException>(
                () => new ConfigurationBuilder().WithPrefix("prefix", null)
            );
        }
        
        [Fact]
        public void PrefixesDoNotTrashPreviousKeys()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    ["Kestrel:Endpoints:Http:Url"] = "http://*:6001/",
                    ["Testing:Blah"] = "BaseValue"
                }
                )
                .WithPrefix(
                    "secrets",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Testing:Blah"] = "ShouldNotOverride",
                                ["ShouldBeAccessibleUsingPrefix"] = "Test"
                            });
                    })
                .Build();

            Assert.Equal("http://*:6001/", configuration.GetValue<string>("Kestrel:Endpoints:Http:Url"));
            Assert.Equal("BaseValue", configuration.GetValue<string>("Testing:Blah"));
            Assert.Equal("ShouldNotOverride", configuration.GetValue<string>("secrets:Testing:Blah"));
            Assert.Equal("Test", configuration.GetValue<string>("secrets:ShouldBeAccessibleUsingPrefix"));
            Assert.Null(configuration.GetValue<string>("ShouldBeAccessibleUsingPrefix"));
        }

        [Fact]
        public void ValuesArePrefixed()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["ShouldBeAccessibleUsingPrefix"] = "Test"
                            });
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("ShouldBeAccessibleUsingPrefix"));
            Assert.Equal("Test", configuration.GetValue<string>("test:ShouldBeAccessibleUsingPrefix"));
        }
        
        [Fact]
        public void NestedValuesArePrefixed()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Nested:ShouldBeAccessibleUsingPrefix"] = "Test"
                            });
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("ShouldBeAccessibleUsingPrefix"));
            Assert.Equal("Test", configuration.GetValue<string>("test:Nested:ShouldBeAccessibleUsingPrefix"));
        }
        
        [Fact]
        public void JustPrefix()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "Value"
                            });
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("test:"));
        }

        [Fact]
        public void NonExistent()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                ["Key"] = "Value"
                            });
                    })
                .Build();

            Assert.NotNull(configuration.GetValue<string>("test:Key"));
            Assert.Null(configuration.GetValue<string>("test:NotHere"));
        }

        [Fact]
        public void NestedPrefixes()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "test",
                    c =>
                    {
                        c.WithPrefix("nested",
                            nested => nested.AddInMemoryCollection(
                                new Dictionary<string, string>
                                {
                                    ["Key"] = "Value"
                                })
                        );
                    })
                .Build();

            Assert.Null(configuration.GetValue<string>("test:Key"));
            Assert.Equal("Value", configuration.GetValue<string>("test:nested:Key"));
        }
        
        [Fact]
        public void CanSetExistingKey()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "prefix",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string> {["Key"] = "Value"}
                        );
                    }
                )
                .Build();

            configuration["prefix:Key"] = "NewValue";

            Assert.Null(configuration["Key"]);
            Assert.Equal("NewValue", configuration["prefix:Key"]);
        }
        
        [Fact]
        public void CanSetNewKey()
        {
            var configuration = new ConfigurationBuilder()
                .WithPrefix(
                    "prefix",
                    c =>
                    {
                        c.AddInMemoryCollection(
                            new Dictionary<string, string> {["Key"] = "Value"}
                        );
                    }
                )
                .Build();
            
            configuration["prefix:NewKey"] = "NewValue";

            Assert.Null(configuration["Key"]);
            Assert.Null(configuration["NewKey"]);
            Assert.Equal("Value", configuration["prefix:Key"]);
            Assert.Equal("NewValue", configuration["prefix:NewKey"]);
        }
    }
}
