/*
 * Copyright Cedar Contributors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#![cfg(test)]

use crate::answer::Answer;
use crate::{
    call_cedar, get_policy_annotations, get_template_annotations, parse_entity_uid, parse_policy,
    parse_policy_template, policy_effect, policy_from_json, policy_set_to_json, policy_to_json,
    template_effect,
};
#[cfg(feature = "partial-eval")]
use cedar_policy::ffi::PartialAuthorizationAnswer;
use cedar_policy::ffi::{AuthorizationAnswer, ValidationAnswer};
use cool_asserts::assert_matches;
use std::str::FromStr;

#[track_caller]
fn assert_failure(result: &str) {
    let result: Answer = serde_json::from_str(result).unwrap();
    assert_matches!(result, Answer::Failure { .. });
}

#[track_caller]
fn assert_success(result: &str) {
    let result: Answer = serde_json::from_str(result).unwrap();
    assert_matches!(result, Answer::Success { .. });
}

#[track_caller]
fn assert_authorization_success(result: &str) {
    let result: AuthorizationAnswer = serde_json::from_str(result).unwrap();
    assert_matches!(result, AuthorizationAnswer::Success { .. });
}

#[track_caller]
fn assert_authorization_failure(result: &str) {
    let result: AuthorizationAnswer = serde_json::from_str(result).unwrap();
    assert_matches!(result, AuthorizationAnswer::Failure { .. });
}

#[cfg(feature = "partial-eval")]
#[track_caller]
fn assert_partial_authorization_success(result: &str) {
    let result: PartialAuthorizationAnswer = serde_json::from_str(result).unwrap();
    assert_matches!(result, PartialAuthorizationAnswer::Residuals { .. });
}

#[track_caller]
fn assert_validation_success(result: &str) {
    let result: ValidationAnswer = serde_json::from_str(result).unwrap();
    assert_matches!(result, ValidationAnswer::Success { .. });
}

#[test]
fn unrecognized_call_fails() {
    let result = call_cedar("BadOperation", "");
    assert_failure(&result);
}

mod authorization_tests {
    use super::*;

    #[test]
    fn empty_authorization_call_succeeds() {
        let result = call_cedar(
            "AuthorizationOperation",
            r#"
    {
    "principal" : { "type" : "User", "id" : "alice" },
    "action" : { "type" : "Photo", "id" : "view" },
    "resource" : { "type" : "Photo", "id" : "photo" },
    "policies": {},
    "entities": [],
    "context": {}
    }
            "#,
        );
        assert_authorization_success(&result);
    }

    #[test]
    fn test_unspecified_principal_call_succeeds() {
        let result = call_cedar(
            "AuthorizationOperation",
            r#"
    {
        "context": {},
        "policies": {
            "staticPolicies": {
            "001": "permit(principal, action, resource);"
            },
            "templates": {},
            "templateLinks": []
        },
        "entities": [],
        "principal": null,
        "action" : { "type" : "Action", "id" : "view" },
        "resource" : { "type" : "Resource", "id" : "thing" }
    }
    "#,
        );
        assert_authorization_failure(&result);
    }

    #[test]
    fn test_unspecified_resource_call_succeeds() {
        let result = call_cedar(
            "AuthorizationOperation",
            r#"
    {
        "context": {},
        "policies": {
            "staticPolicies": {
            "001": "permit(principal, action, resource);"
            },
            "templates": {},
            "templateLinks": []
        },
        "entities": [],
        "principal" : { "type" : "User", "id" : "alice" },
        "action" : { "type" : "Action", "id" : "view" },
        "resource": null
    }
    "#,
        );
        assert_authorization_failure(&result);
    }

    #[test]
    fn template_authorization_call_succeeds() {
        let result = call_cedar(
            "AuthorizationOperation",
            r#"
        {
            "principal" : {
                "type" : "User",
                "id" : "alice"
            },
            "action" : {
                "type" : "Photo",
                "id" : "view"
            },
            "resource" : {
                "type" : "Photo",
                "id" : "door"
            },
            "context" : {},
            "policies" : {
                "staticPolicies" : {},
                "templates" : {
                    "ID0": "permit(principal == ?principal, action, resource);"
                },
                "templateLinks" : [
                    {
                        "templateId" : "ID0",
                        "newId" : "ID0_User_alice",
                        "values" : {
                            "?principal": {
                                "type" : "User",
                                "id" : "alice"
                            }
                        }
                    }
                ]
            },
            "entities" : []
        }
            "#,
        );
        assert_authorization_success(&result);
    }
}

mod validation_tests {
    use super::*;

    #[test]
    fn empty_validation_call_json_schema_succeeds() {
        let result = call_cedar("ValidateOperation", r#"{ "schema": {}, "policies": {} }"#);
        assert_validation_success(&result);
    }

    #[test]
    fn empty_validation_call_succeeds() {
        let result = call_cedar("ValidateOperation", r#"{ "schema": "", "policies": {} }"#);
        assert_validation_success(&result);
    }

    #[test]
    fn validate_with_level_succeeds() {
        let input = r#" {
            "schema": {
                "": {
                    "entityTypes": {
                        "User": {
                            "memberOfTypes": [
                                "UserGroup"
                            ],
                            "shape": {
                                "type": "Record",
                                "attributes": {
                                    "friend": {
                                        "type": "Entity",
                                        "name": "User"
                                    }
                                }
                            }
                        },
                        "Photo": {
                            "memberOfTypes": [
                                "Album",
                                "Account"
                            ],
                            "shape": {
                                "type": "Record",
                                "attributes": {
                                    "owner": {
                                        "type": "Entity",
                                        "name": "User"
                                    }
                                }
                            }
                        },
                        "Album": {
                            "memberOfTypes": [
                                "Album",
                                "Account"
                            ]
                        },
                        "Account": {},
                        "UserGroup": {}
                    },
                    "actions": {
                        "readOnly": {},
                        "readWrite": {},
                        "createAlbum": {
                            "appliesTo": {
                                "resourceTypes": [
                                    "Account",
                                    "Album"
                                ],
                                "principalTypes": [
                                    "User"
                                ]
                            }
                        },
                        "addPhotoToAlbum": {
                            "appliesTo": {
                                "resourceTypes": [
                                    "Album"
                                ],
                                "principalTypes": [
                                    "User"
                                ]
                            }
                        },
                        "viewPhoto": {
                            "appliesTo": {
                                "resourceTypes": [
                                    "Photo"
                                ],
                                "principalTypes": [
                                    "User"
                                ]
                            }
                        },
                        "viewComments": {
                            "appliesTo": {
                                "resourceTypes": [
                                    "Photo"
                                ],
                                "principalTypes": [
                                    "User"
                                ]
                            }
                        }
                    }
                }
            },
            "policies": {
                "staticPolicies": {
                    "policy0": "permit(principal in UserGroup::\"alice_friends\", action == Action::\"viewPhoto\", resource) when {principal in resource.owner.friend};"
                }
            },
            "maxDerefLevel": 2
        }
        "#;

        let result = call_cedar("ValidateWithLevelOperation", input);

        assert_validation_success(&result);
    }
}

mod entity_validation_tests {
    use super::*;
    use serde_json::json;

    #[test]
    fn validate_entities_succeeds() {
        let json_data = json!(
            {
              "entities":[
                {
                    "uid": {
                        "type": "PhotoApp::User",
                        "id": "alice"
                    },
                    "attrs": {
                        "userId": "897345789237492878",
                        "personInformation": {
                            "age": 25,
                            "name": "alice"
                        },
                    },
                    "parents": [
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "alice_friends"
                        },
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "AVTeam"
                        }
                    ]
                },
                {
                    "uid": {
                        "type": "PhotoApp::Photo",
                        "id": "vacationPhoto.jpg"
                    },
                    "attrs": {
                        "private": false,
                        "account": {
                            "__entity": {
                                "type": "PhotoApp::Account",
                                "id": "ahmad"
                            }
                        }
                    },
                    "parents": []
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "alice_friends"
                    },
                    "attrs": {},
                    "parents": []
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "AVTeam"
                    },
                    "attrs": {},
                    "parents": []
                }
              ],
              "schema":{
                "PhotoApp": {
                    "commonTypes": {
                        "PersonType": {
                            "type": "Record",
                            "attributes": {
                                "age": {
                                    "type": "Long"
                                },
                                "name": {
                                    "type": "String"
                                }
                            }
                        },
                        "ContextType": {
                            "type": "Record",
                            "attributes": {
                                "ip": {
                                    "type": "Extension",
                                    "name": "ipaddr",
                                    "required": false
                                },
                                "authenticated": {
                                    "type": "Boolean",
                                    "required": true
                                }
                            }
                        }
                    },
                    "entityTypes": {
                        "User": {
                            "shape": {
                                "type": "Record",
                                "attributes": {
                                    "userId": {
                                        "type": "String"
                                    },
                                    "personInformation": {
                                        "type": "PersonType"
                                    }
                                }
                            },
                            "memberOfTypes": [
                                "UserGroup"
                            ]
                        },
                        "UserGroup": {
                            "shape": {
                                "type": "Record",
                                "attributes": {}
                            }
                        },
                        "Photo": {
                            "shape": {
                                "type": "Record",
                                "attributes": {
                                    "account": {
                                        "type": "Entity",
                                        "name": "Account",
                                        "required": true
                                    },
                                    "private": {
                                        "type": "Boolean",
                                        "required": true
                                    }
                                }
                            },
                            "memberOfTypes": [
                                "Album",
                                "Account"
                            ]
                        },
                        "Album": {
                            "shape": {
                                "type": "Record",
                                "attributes": {}
                            }
                        },
                        "Account": {
                            "shape": {
                                "type": "Record",
                                "attributes": {}
                            }
                        }
                    },
                    "actions": {}
                }
            }
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_success(&result);
    }

    #[test]
    fn validate_entities_with_cedarschema_succeeds() {
        let json_data = json!(
        {
            "entities":[
            {
                "uid": {
                    "type": "PhotoApp::User",
                    "id": "alice"
                },
                "attrs": {
                    "userId": "897345789237492878",
                    "personInformation": {
                        "age": 25,
                        "name": "alice"
                    },
                },
                "parents": [
                    {
                        "type": "PhotoApp::UserGroup",
                        "id": "alice_friends"
                    },
                    {
                        "type": "PhotoApp::UserGroup",
                        "id": "AVTeam"
                    }
                ]
            },
            {
                "uid": {
                    "type": "PhotoApp::Photo",
                    "id": "vacationPhoto.jpg"
                },
                "attrs": {
                    "private": false,
                    "account": {
                        "__entity": {
                            "type": "PhotoApp::Account",
                            "id": "ahmad"
                        }
                    }
                },
                "parents": []
            },
            {
                "uid": {
                    "type": "PhotoApp::UserGroup",
                    "id": "alice_friends"
                },
                "attrs": {},
                "parents": []
            },
            {
                "uid": {
                    "type": "PhotoApp::UserGroup",
                    "id": "AVTeam"
                },
                "attrs": {},
                "parents": []
            }
            ],
            "schema":r#"
                namespace PhotoApp {
                    type ContextType = {
                    "authenticated": __cedar::Bool,
                    "ip"?: __cedar::ipaddr
                    };

                    type PersonType = {
                    "age": __cedar::Long,
                    "name": __cedar::String
                    };

                    entity Account;

                    entity Album;

                    entity Photo in [Album, Account] = {
                    "account": Account,
                    "private": __cedar::Bool
                    };

                    entity User in [UserGroup] = {
                    "personInformation": PersonType,
                    "userId": __cedar::String
                    };

                    entity UserGroup;
                    }"#
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_success(&result);
    }

    #[test]
    fn validate_entities_field_missing() {
        let json_data = json!(
            {
              "entities":[
                {
                    "uid": {
                        "type": "PhotoApp::User",
                        "id": "alice"
                    },
                    "attrs": {
                        "userId": "897345789237492878"
                    },
                    "parents": [
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "alice_friends"
                        },
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "AVTeam"
                        }
                    ]
                },
                {
                    "uid": {
                        "type": "PhotoApp::Photo",
                        "id": "vacationPhoto.jpg"
                    },
                    "attrs": {
                        "private": false,
                        "account": {
                            "__entity": {
                                "type": "PhotoApp::Account",
                                "id": "ahmad"
                            }
                        }
                    },
                    "parents": []
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "alice_friends"
                    },
                    "attrs": {},
                    "parents": []
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "AVTeam"
                    },
                    "attrs": {},
                    "parents": []
                }
              ],
              "schema":{
                "PhotoApp": {
                    "commonTypes": {
                        "PersonType": {
                            "type": "Record",
                            "attributes": {
                                "age": {
                                    "type": "Long"
                                },
                                "name": {
                                    "type": "String"
                                }
                            }
                        },
                        "ContextType": {
                            "type": "Record",
                            "attributes": {
                                "ip": {
                                    "type": "Extension",
                                    "name": "ipaddr",
                                    "required": false
                                },
                                "authenticated": {
                                    "type": "Boolean",
                                    "required": true
                                }
                            }
                        }
                    },
                    "entityTypes": {
                        "User": {
                            "shape": {
                                "type": "Record",
                                "attributes": {
                                    "userId": {
                                        "type": "String"
                                    },
                                    "personInformation": {
                                        "type": "PersonType"
                                    }
                                }
                            },
                            "memberOfTypes": [
                                "UserGroup"
                            ]
                        },
                        "UserGroup": {
                            "shape": {
                                "type": "Record",
                                "attributes": {}
                            }
                        },
                        "Photo": {
                            "shape": {
                                "type": "Record",
                                "attributes": {
                                    "account": {
                                        "type": "Entity",
                                        "name": "Account",
                                        "required": true
                                    },
                                    "private": {
                                        "type": "Boolean",
                                        "required": true
                                    }
                                }
                            },
                            "memberOfTypes": [
                                "Album",
                                "Account"
                            ]
                        },
                        "Album": {
                            "shape": {
                                "type": "Record",
                                "attributes": {}
                            }
                        },
                        "Account": {
                            "shape": {
                                "type": "Record",
                                "attributes": {}
                            }
                        }
                    },
                    "actions": {}
                }
            }
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_failure(&result);
    }

    #[test]
    fn validate_entities_with_cedarschema_field_missing() {
        let json_data = json!(
            {
              "entities":[
                {
                    "uid": {
                        "type": "PhotoApp::User",
                        "id": "alice"
                    },
                    "attrs": {
                        "userId": "897345789237492878"
                    },
                    "parents": [
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "alice_friends"
                        },
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "AVTeam"
                        }
                    ]
                },
                {
                    "uid": {
                        "type": "PhotoApp::Photo",
                        "id": "vacationPhoto.jpg"
                    },
                    "attrs": {
                        "private": false,
                        "account": {
                            "__entity": {
                                "type": "PhotoApp::Account",
                                "id": "ahmad"
                            }
                        }
                    },
                    "parents": []
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "alice_friends"
                    },
                    "attrs": {},
                    "parents": []
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "AVTeam"
                    },
                    "attrs": {},
                    "parents": []
                }
              ],
              "schema":r#"namespace PhotoApp {
                    type ContextType = {
                    "authenticated": __cedar::Bool,
                    "ip"?: __cedar::ipaddr
                    };

                    type PersonType = {
                    "age": __cedar::Long,
                    "name": __cedar::String
                    };

                    entity Account;

                    entity Album;

                    entity Photo in [Album, Account] = {
                    "account": Account,
                    "private": __cedar::Bool
                    };

                    entity User in [UserGroup] = {
                    "personInformation": PersonType,
                    "userId": __cedar::String
                    };

                    entity UserGroup;
                    }"#
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_failure(&result);
    }

    #[test]
    #[should_panic]
    fn validate_entities_invalid_json_fails() {
        call_cedar("ValidateEntities", "{]");
    }

    #[test]
    fn validate_entities_invalid_schema_fails() {
        let json_data = json!(
        {
            "entities": [

            ],
            "schema": {
                "PhotoApp": {
                    "commonTypes": {},
                    "entityTypes": {
                        "UserGroup": {
                            "shape44": {
                                "type": "Record",
                                "attributes": {}
                            },
                            "memberOfTypes": [
                                "UserGroup"
                            ]
                        }
                    },
                    "actions": {}
                }
            }
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_failure(&result);

        assert!(
            result.contains(
                "unknown field `shape44`, expected one of `memberOfTypes`, `shape`, `tags`"
            ),
            "result was `{result}`",
        );
    }

    #[test]
    fn validate_entities_invalid_cedarschema_fails() {
        let json_data = json!(
        {
            "entities": [

            ],
            "schema": r#"namespace PhotoApp {
                type ContextType = {
                "authenticated": __cedar::Bool,
                "ip"?: __cedar::ipaddr
                };

                type PersonType = {
                "age": __cedar::Long,
                "name": __cedar::String
                };

                entity Account;

                entity Album;

                entity Photo in [Album, Account] = {
                "account": Account,
                "private": __cedar::Tool
                };

                entity User in [UserGroup] = {
                "personInformation": PersonType,
                "userId": __cedar::String
                };

                entity UserGroup;
                }"#
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_failure(&result);

        assert!(
            result.contains("failed to resolve type: __cedar::Tool"),
            "result was `{result}`",
        );
    }

    #[test]
    fn validate_entities_detect_cycle_fails() {
        let json_data = json!(
        {
            "entities": [
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "ABCTeam"
                    },
                    "attrs": {},
                    "parents": [
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "AVTeam"
                        }
                    ]
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "AVTeam"
                    },
                    "attrs": {},
                    "parents": [
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "ABCTeam"
                        }
                    ]
                }
            ],
            "schema": {
                "PhotoApp": {
                    "commonTypes": {},
                    "entityTypes": {
                        "UserGroup": {
                            "shape": {
                                "type": "Record",
                                "attributes": {}
                            },
                            "memberOfTypes": [
                                "UserGroup"
                            ]
                        }
                    },
                    "actions": {}
                }
            }
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_failure(&result);

        assert!(
            result.contains("input graph has a cycle containing vertex `PhotoApp::UserGroup"),
            "result was `{result}`",
        );
    }

    #[test]
    fn validate_entities_with_cedarschema_detect_cycle_fails() {
        let json_data = json!(
        {
            "entities": [
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "ABCTeam"
                    },
                    "attrs": {},
                    "parents": [
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "AVTeam"
                        }
                    ]
                },
                {
                    "uid": {
                        "type": "PhotoApp::UserGroup",
                        "id": "AVTeam"
                    },
                    "attrs": {},
                    "parents": [
                        {
                            "type": "PhotoApp::UserGroup",
                            "id": "ABCTeam"
                        }
                    ]
                }
            ],
            "schema": r#"namespace PhotoApp {
                entity UserGroup in [UserGroup];
                }
                "#
        });
        let result = call_cedar("ValidateEntities", json_data.to_string().as_str());
        assert_failure(&result);

        assert!(
            result.contains("input graph has a cycle containing vertex `PhotoApp::UserGroup"),
            "result was `{result}`",
        );
    }
}

#[cfg(feature = "partial-eval")]
mod partial_authorization_tests {
    use super::*;

    #[test]
    fn test_missing_resource_call_succeeds() {
        let result = call_cedar(
            "AuthorizationPartialOperation",
            r#"
    {
        "context": {},
        "policies": {
            "staticPolicies": {
            "001": "permit(principal == User::\"alice\", action, resource == Photo::\"door\");"
            },
            "templates": {},
            "templateLinks": []
        },
        "entities": [],
        "principal" : { "type" : "User", "id" : "alice" },
        "action" : { "type" : "Action", "id" : "view" }
    }
    "#,
        );
        assert_partial_authorization_success(&result);
    }

    #[test]
    fn test_missing_principal_call_succeeds() {
        let result = call_cedar(
            "AuthorizationPartialOperation",
            r#"
    {
        "context": {},
        "policies": {
            "staticPolicies": {
            "001": "permit(principal == User::\"alice\", action, resource == Photo::\"door\");"
            },
            "templates": {},
            "templateLinks": []
        },
        "entities": [],
        "action" : { "type" : "Action", "id" : "view" },
        "resource" : { "type" : "Photo", "id" : "door" }
    }
    "#,
        );
        assert_partial_authorization_success(&result);
    }
}

mod policy_tests {
    use super::*;

    #[test]
    fn policy_effect_tests() {
        assert_eq!(
            policy_effect("permit(principal,action,resource);").unwrap(),
            "permit"
        );
        assert_eq!(
            policy_effect("forbid(principal,action,resource);").unwrap(),
            "forbid"
        );
    }

    #[test]
    fn parse_policy_returns_canonical_text() {
        let input = "permit(principal,action,resource);";
        let parsed = parse_policy(input).unwrap();
        assert_eq!(
            parsed,
            cedar_policy::Policy::from_str(input).unwrap().to_string()
        );
    }

    #[test]
    fn static_policy_annotations_tests() {
        let annotations = get_policy_annotations(
            r#"@id("policyID1") @myAnnotationKey("myAnnotatedValue") permit(principal,action,resource);"#,
        )
        .unwrap();
        assert_eq!(
            annotations,
            r#"{"id":"policyID1","myAnnotationKey":"myAnnotatedValue"}"#
        );
    }

    #[test]
    fn template_policy_annotations_tests() {
        let annotations = get_template_annotations(
            r#"@id("policyID1") @myAnnotationKey("myAnnotatedValue") permit(principal==?principal,action,resource);"#,
        )
        .unwrap();
        assert_eq!(
            annotations,
            r#"{"id":"policyID1","myAnnotationKey":"myAnnotatedValue"}"#
        );
    }

    #[test]
    fn get_template_annotations_invalid_template() {
        assert!(get_template_annotations("invalid template syntax").is_err());
    }

    #[test]
    fn parse_policy_template_valid_test() {
        let policy_template =
            r#"permit(principal==?principal,action == Action::"readfile",resource==?resource );"#;
        let actual = parse_policy_template(policy_template).unwrap();
        let expected = cedar_policy::Template::from_str(policy_template)
            .unwrap()
            .to_string();
        assert_eq!(actual, expected);
    }

    #[test]
    fn parse_policy_template_invalid_missing_template_slots() {
        let input = r#"permit(principal == User::"alice", action == Action::"read", resource == Resource::"file");"#;
        assert!(parse_policy_template(input).is_err());
    }

    #[test]
    fn parse_policy_template_invalid_test() {
        assert!(parse_policy_template(r#"permit(Principa,Action,Resource );"#).is_err());
    }

    #[test]
    fn from_json_test_valid() {
        let policy_json = r#"
        {
            "effect": "permit",
            "principal": {
                "op": "==",
                "entity": { "type": "User", "id": "12UA45" }
            },
            "action": {
                "op": "==",
                "entity": { "type": "Action", "id": "view" }
            },
            "resource": {
                "op": "in",
                "entity": { "type": "Folder", "id": "abc" }
            },
            "conditions": [
                {
                    "kind": "when",
                    "body": {
                        "==": {
                            "left": {
                                ".": {
                                    "left": { "Var": "context" },
                                    "attr": "tls_version"
                                }
                            },
                            "right": { "Value": "1.3" }
                        }
                    }
                }
            ]
        }
        "#;

        let actual = policy_from_json(policy_json).unwrap();
        let policy_json_value: serde_json::Value = serde_json::from_str(policy_json).unwrap();
        let expected = cedar_policy::Policy::from_json(None, policy_json_value)
            .unwrap()
            .to_string();
        assert_eq!(actual, expected);
    }

    #[test]
    fn from_json_invalid() {
        let invalid_input = r#"
        {
            "Effect": "permit",
            "Principal": {
                "op": "==",
                "Entity": { "type": "User", "id": "12UA45" }
            },
            "Action": {
                "op": "==",
                "entity": { "type": "Action", "id": "view" }
            },
            "Resource": {
                "op": "in",
                "entity": { "type": "Folder", "id": "abc" }
            },
            "Conditions": []
        }
        "#;
        assert!(policy_from_json(invalid_input).is_err());
    }

    #[test]
    fn to_json_internal_test() {
        let input = r#"permit(principal, action, resource);"#;
        let actual_json_str = policy_to_json(input).unwrap();
        let expected_policy = cedar_policy::Policy::from_str(input).unwrap();
        let expected_json_str = serde_json::to_string(&expected_policy.to_json().unwrap()).unwrap();

        assert_eq!(actual_json_str, expected_json_str);
    }

    #[test]
    fn to_json_internal_invalid() {
        assert!(policy_to_json(r#"Permit(Principal, Resource, Action);"#).is_err());
    }

    #[test]
    fn template_effect_permit_test() {
        let template_policy =
            r#"permit(principal==?principal,action == Action::"readfile",resource==?resource );"#;
        assert_eq!(template_effect(template_policy).unwrap(), "permit");
    }

    #[test]
    fn template_effect_forbid_test() {
        let template_policy =
            r#"forbid(principal==?principal,action == Action::"readfile",resource==?resource );"#;
        assert_eq!(template_effect(template_policy).unwrap(), "forbid");
    }

    #[test]
    fn policyset_to_json_valid_test() {
        let policyset_str = r#"{
            "staticPolicies": {
                "p1": "permit(principal == User::\"Bob\", action == Action::\"View_Photo\", resource in Album::\"Vacation\");"
            },
            "templates": {
                "t0": "permit(principal == ?principal, action == Action::\"View_Photo\", resource in Album::\"Vacation\");"
            },
            "templateLinks": [{
                "templateId": "t0",
                "newId": "tl0",
                "values": {
                    "?principal": {"type": "User", "id": "Alice"}
                }
            }]
        }"#;

        let actual_json: serde_json::Value =
            serde_json::from_str(&policy_set_to_json(policyset_str).unwrap()).unwrap();
        let expected_json = serde_json::json!({
            "templates": {
                "t0": {
                    "effect": "permit",
                    "principal": {
                        "op": "==",
                        "slot": "?principal"
                    },
                    "action": {
                        "op": "==",
                        "entity": {
                            "type": "Action",
                            "id": "View_Photo"
                        }
                    },
                    "resource": {
                        "op": "in",
                        "entity": {
                            "type": "Album",
                            "id": "Vacation"
                        }
                    },
                    "conditions": []
                }
            },
            "staticPolicies": {
                "p1": {
                    "effect": "permit",
                    "principal": {
                        "op": "==",
                        "entity": {
                            "type": "User",
                            "id": "Bob"
                        }
                    },
                    "action": {
                        "op": "==",
                        "entity": {
                            "type": "Action",
                            "id": "View_Photo"
                        }
                    },
                    "resource": {
                        "op": "in",
                        "entity": {
                            "type": "Album",
                            "id": "Vacation"
                        }
                    },
                    "conditions": []
                }
            },
            "templateLinks": [{
                "templateId": "t0",
                "newId": "tl0",
                "values": {
                    "?principal": {
                        "__entity": {
                            "type": "User",
                            "id": "Alice"
                        }
                    }
                }
            }]
        });

        assert_eq!(actual_json, expected_json);
    }

    #[test]
    fn policyset_to_json_invalid_test() {
        let policyset_str = r#"{
            "staticPolicies": {
                "p1": "permit(principal == User::\"Bob\", act == Action::\"View_Photo\", resrce in Album::\"Vacation\");"
            },
            "templates": {
                "t0": "permit(principal == ?
                }
            }
        }"#;

        assert!(policy_set_to_json(policyset_str).is_err());
    }
}

mod entity_tests {
    use super::*;

    #[test]
    fn invalid_entity_uid_parse_returns_json_null_payload() {
        assert_eq!(parse_entity_uid("not an entity uid").unwrap(), "null");
    }
}
