using MonoCloud.Cedar.Model.Exception;
using MonoCloud.Cedar.Model.Policy;

namespace MonoCloud.Cedar.UnitTests;

public sealed class PolicySetTests
{
  [Fact]
  public void ParsePoliciesTests()
  {
    var policySet = PolicySet.ParsePoliciesFile(TestUtil.ResourcePath("policies.cedar"));

    Assert.All(policySet.Policies, policy => Assert.NotNull(policy.PolicySrc));
    Assert.Equal(2, policySet.Policies.Select(policy => policy.PolicyID).Distinct().Count());
    Assert.Equal(2, policySet.Policies.Count);
    Assert.Empty(policySet.Templates);
  }

  [Fact]
  public void ParsePoliciesStringTests()
  {
    var policySet = PolicySet.ParsePolicies("permit(principal, action, resource);");

    Assert.Single(policySet.Policies);
    Assert.Empty(policySet.Templates);
  }

  [Fact]
  public void ParseTemplatesTests()
  {
    var policySet = PolicySet.ParsePoliciesFile(TestUtil.ResourcePath("template.cedar"));

    Assert.Equal(2, policySet.Policies.Count);
    Assert.Single(policySet.Templates);
  }

  [Fact]
  public void ParsePoliciesExceptionTests()
  {
    Assert.Throws<FileNotFoundException>(() => PolicySet.ParsePoliciesFile("nonExistentFilePath.cedar"));
    Assert.Throws<InternalException>(() => PolicySet.ParsePoliciesFile(TestUtil.ResourcePath("malformed_policy_set.cedar")));
  }

  [Fact]
  public void PolicySetToJsonTests()
  {
    var validJson = TestUtil.BuildValidPolicySet().ToJson();

    Assert.Contains("\"templates\"", validJson);
    Assert.Contains("\"staticPolicies\"", validJson);
    Assert.Contains("\"templateLinks\"", validJson);
    Assert.Throws<InternalException>(() => TestUtil.BuildInvalidPolicySet().ToJson());
  }
}
