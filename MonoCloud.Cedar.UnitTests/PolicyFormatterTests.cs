using MonoCloud.Cedar.Formatter;
using MonoCloud.Cedar.Model.Formatter;

namespace MonoCloud.Cedar.UnitTests;

public sealed class PolicyFormatterTests
{
  [Fact]
  public void FormatsPolicy()
  {
    var unformatted = TestUtil.ReadResource("unformatted_policy.cedar");
    var expected = TestUtil.ReadResource("formatted_policy.cedar").Trim();

    Assert.Equal(expected, PolicyFormatter.PoliciesStrToPretty(unformatted).Trim());
  }

  [Fact]
  public void FormatsPolicyWithConfig()
  {
    var unformatted = TestUtil.ReadResource("unformatted_policy.cedar");
    var expected = TestUtil.ReadResource("formatted_policy_custom_config.cedar").Trim();

    Assert.Equal(expected, PolicyFormatter.PoliciesStrToPrettyWithConfig(unformatted, new Config(120, 4)).Trim());
  }
}
