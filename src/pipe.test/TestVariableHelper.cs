using System.Collections.Generic;
using pipe.Exceptions;
using pipe.test.Builders;
using pipe.test.TestDoubles;
using Xunit;

namespace pipe.test
{
    public class TestVariableHelper
    {
        [Fact]
        public void returns_expected_when_no_variables_has_been_provided_and_no_variables_are_referenced()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>();
            
            var result = sut.ExpandVariables(stubVariables, "foo");
            
            Assert.Equal("foo", result);
        }

        [Fact]
        public void returns_expected_when_single_variable_has_been_provided_and_no_variables_are_referenced()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {
                    "dummy", "dummy"
                }
            };
            var result = sut.ExpandVariables(stubVariables, "foo");
            
            Assert.Equal("foo", result);
        }

        [Fact]
        public void returns_expected_when_no_variables_has_been_provided_but_single_variable_is_referenced()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>();
            
            var result = sut.ExpandVariables(stubVariables, "foo $(BAR)");
            
            Assert.Equal("foo $(BAR)", result);
        }

        [Fact]
        public void returns_expected_when_single_variable_is_expanded()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"BAR", "bar"}
            };
            
            var result = sut.ExpandVariables(stubVariables, "foo $(BAR)");
            
            Assert.Equal("foo bar", result);
        }

        [Fact]
        public void returns_expected_when_variable_is_expanded_inside_quotes()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"BAR", "bar"}
            };
            
            var result = sut.ExpandVariables(stubVariables, "foo \"$(BAR)\"");
            
            Assert.Equal("foo \"bar\"", result);
        }

        [Fact]
        public void returns_expected_when_expanding_environment_variable()
        {
            var sut = new VariableHelperBuilder()
                .WithEnvironmentVariableProvider(new StubEnvironmentVariableProvider("bar"))
                .Build();
            
            var stubEmptyVariables = new Dictionary<string, string>();
            
            var result = sut.ExpandVariables(stubEmptyVariables, "foo ${BAR}");
            
            Assert.Equal("foo bar", result);
        }

        [Fact]
        public void returns_expected_when_expanding_environment_variable_that_does_not_exist()
        {
            var sut = new VariableHelperBuilder()
                .WithEnvironmentVariableProvider(new StubEnvironmentVariableProvider())
                .Build();
            
            var stubEmptyVariables = new Dictionary<string, string>();
            
            var result = sut.ExpandVariables(stubEmptyVariables, "foo ${BAR}");
            
            Assert.Equal("foo ${BAR}", result);
        }

        [Fact]
        public void returns_expected_when_expanding_multiple_environment_variables()
        {
            var stubEnvVarValues = new Dictionary<string, string>
            {
                {"FOO", "foo"},
                {"BAR", "bar"},
                {"BAZ", "baz"},
                {"QUX", "qux"},
            };
            
            var sut = new VariableHelperBuilder()
                .WithEnvironmentVariableProvider(new FakeEnvironmentVariableProvider(stubEnvVarValues))
                .Build();
            
            var stubEmptyVariables = new Dictionary<string, string>();
            
            var result = sut.ExpandVariables(stubEmptyVariables, "foo ${BAR} ${BAZ} ${QUX}");
            
            Assert.Equal("foo bar baz qux", result);
        }

        [Fact]
        public void returns_expected_when_expanding_variable_with_environment_variable_reference_inside()
        {
            var stubEnvVarValues = new Dictionary<string, string>
            {
                {"BAZ", "baz"},
                {"QUX", "qux"},
            };
            
            var sut = new VariableHelperBuilder()
                .WithEnvironmentVariableProvider(new FakeEnvironmentVariableProvider(stubEnvVarValues))
                .Build();

            var stubVariables = new Dictionary<string, string>
            {
                {"BAR", "${BAZ} ${QUX}"}
            };
            
            var result = sut.ExpandVariables(stubVariables, "foo $(BAR)");
            
            Assert.Equal("foo baz qux", result);
        }

        [Fact]
        public void variables_can_reference_other_variables()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"BAR", "bar $(BAZ)"},
                {"BAZ", "baz $(QUX)"},
                {"QUX", "qux"},
            };
            
            var result = sut.ExpandVariables(stubVariables, "foo $(BAR)");
            
            Assert.Equal("foo bar baz qux", result);
        }

        [Fact]
        public void variables_can_reference_other_variables_in_random_order()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"QUX", "qux"},
                {"BAZ", "baz $(QUX)"},
                {"BAR", "bar $(BAZ)"},
            };
            
            var result = sut.ExpandVariables(stubVariables, "foo $(BAR)");
            
            Assert.Equal("foo bar baz qux", result);
        }

        [Fact]
        public void throws_expected_exception_when_one_variable_references_another_none_declared_variable()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"BAR", "bar $(NOT-DECLARED)"},
            };
            
            Assert.Throws<VariableNotDeclaredException>(() => sut.ExpandVariables(stubVariables, "foo $(BAR)"));
        }

        [Fact]
        public void prevents_direct_circular_references_between_variables()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"FOO", "$(BAR)"},
                {"BAR", "$(FOO)"},
            };

            Assert.Throws<CircularVariableReferenceException>(() => sut.ExpandVariables(stubVariables, "foo $(BAR)"));
        }
        
        [Fact]
        public void prevents_indirect_circular_references_between_variables()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"FOO", "$(BAR)"},
                {"BAR", "$(BAZ)"},
                {"BAZ", "$(FOO)"},
            };

            Assert.Throws<CircularVariableReferenceException>(() => sut.ExpandVariables(stubVariables, "foo $(BAR)"));
        }
        
        [Fact]
        public void prevents_self_reference_in_variables()
        {
            var sut = new VariableHelperBuilder().Build();
            var stubVariables = new Dictionary<string, string>
            {
                {"FOO", "$(FOO)"},
            };

            Assert.Throws<CircularVariableReferenceException>(() => sut.ExpandVariables(stubVariables, "foo"));
        }
    }
}