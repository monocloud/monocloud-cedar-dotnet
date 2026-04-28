namespace MonoCloud.Cedar;

public interface AuthorizationEngine
{
  AuthorizationResponse IsAuthorized(AuthorizationRequest request, PolicySet policySet, ISet<Entity> entities);

  AuthorizationResponse IsAuthorized(AuthorizationRequest request, PolicySet policySet, Entities entities);

  [Experimental(ExperimentalFeature.PartialEvaluation)]
  PartialAuthorizationResponse IsAuthorizedPartial(PartialAuthorizationRequest request, PolicySet policySet, ISet<Entity> entities);

  [Experimental(ExperimentalFeature.PartialEvaluation)]
  PartialAuthorizationResponse IsAuthorizedPartial(PartialAuthorizationRequest request, PolicySet policySet, Entities entities);

  ValidationResponse Validate(ValidationRequest request);

  ValidationResponse ValidateWithLevel(LevelValidationRequest request);

  void ValidateEntities(EntityValidationRequest request);

  static string GetCedarLangVersion() => "4.0";
}
